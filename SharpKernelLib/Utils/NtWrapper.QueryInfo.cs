using System;
using System.Runtime.InteropServices;
using Windows.Wdk.System.SystemInformation;
using Windows.Wdk.Foundation;
using Windows.Win32.Foundation;
using Microsoft.Win32;
using SharpKernelLib.Exception;
using Windows.Win32.Storage.FileSystem;
using Windows.Wdk.Storage.FileSystem;

using static SharpKernelLib.Utils.NtUndocumented;
using static Windows.Win32.PInvoke;
using static Windows.Wdk.PInvoke;

namespace SharpKernelLib.Utils
{
    // Port of KDU Hamakaze sup.c and sup.h
    internal unsafe static partial class NtWrapper
    {
        internal const uint NTQSI_MAX_BUFFER_LENGTH = 512 * 1024 * 1024; // 512 MiB

        private static bool IsSuccess(this NTSTATUS ntstatus) => ntstatus.SeverityCode == NTSTATUS.Severity.Success;

        internal static bool IsSuccess(this int ntstatus) => ((NTSTATUS)ntstatus).IsSuccess();

        internal static bool IsSuccess(this uint ntstatus) => ((NTSTATUS)ntstatus).IsSuccess();

        /// <summary>
        /// ntsupQueryHVCIState: Query HVCI/IUM state.
        /// </summary>
        internal static bool QueryHVCIState(out bool hvciEnabled, out bool hvciStrict, out bool hvciIUM)
        {
            hvciEnabled = false;
            hvciStrict = false;
            hvciIUM = false;

            var returnLength = 0u;
            var ciInfo = new SYSTEM_CODEINTEGRITY_INFORMATION();
            ciInfo.Length = (uint)Marshal.SizeOf(ciInfo);

            var status = NtQuerySystemInformation(SYSTEM_INFORMATION_CLASS.SystemCodeIntegrityInformation, &ciInfo, (uint)Marshal.SizeOf(ciInfo), ref returnLength);
            if (!status.IsSuccess())
                return false;

            hvciEnabled = ciInfo.CodeIntegrityOptions.HasFlag(CodeIntegrityOptions.Enabled);
            hvciStrict = ciInfo.CodeIntegrityOptions.HasFlag(CodeIntegrityOptions.StrictMode);
            hvciIUM = ciInfo.CodeIntegrityOptions.HasFlag(CodeIntegrityOptions.IUM);

            return true;
        }

        /// <summary>
        /// ntsupIsObjectExists: Return TRUE if the given object exists, FALSE otherwise.
        /// </summary>
        internal static bool IsObjectExists(string rootDirectory, string objectName)
        {
            var rootDirectoryU = new UNICODE_STRING();
            RtlInitUnicodeString(ref rootDirectoryU, rootDirectory);

            var objectNameU = new UNICODE_STRING();
            RtlInitUnicodeString(ref objectNameU, objectName);

            var objectAttributes = new OBJECT_ATTRIBUTES
            {
                Length = (uint)sizeof(OBJECT_ATTRIBUTES),
                RootDirectory = HANDLE.Null,
                Attributes = (uint)ObjectAttributes.OBJ_CASE_INSENSITIVE,
                ObjectName = &rootDirectoryU,
                SecurityDescriptor = null,
                SecurityQualityOfService = null
            };

            var ntstatus = NtOpenDirectoryObject(out var directoryHandle, AccessMask.DIRECTORY_QUERY, &objectAttributes);
            if (!ntstatus.IsSuccess())
                throw new SessionInitializationException("IsObjectExists#NtOpenDirectoryObject", new NtStatusException(ntstatus));

            var context = 0u;
            var found = false;
            do
            {
                ntstatus = NtQueryDirectoryObject(directoryHandle, null, 0, true, false, ref context, out var requiredBufferSize);
                if (ntstatus != (uint)NtStatus.BufferTooSmall)
                    throw new SessionInitializationException("IsObjectExists#NtQueryDirectoryObject(BufSize)", new NtStatusException(ntstatus));

                var buffer = Marshal.AllocHGlobal(requiredBufferSize);
                ntstatus = NtQueryDirectoryObject(directoryHandle, (OBJECT_DIRECTORY_INFORMATION*)buffer.ToPointer(), (uint)requiredBufferSize, true, false, ref context, out requiredBufferSize);
                if (!ntstatus.IsSuccess())
                {
                    Marshal.FreeHGlobal(buffer);
                    throw new SessionInitializationException("IsObjectExists#NtQueryDirectoryObject(Data)", new NtStatusException(ntstatus));
                }

                var dirName = ((OBJECT_DIRECTORY_INFORMATION*)buffer)->Name;
                Marshal.FreeHGlobal(buffer);
                if (RtlEqualUnicodeString(dirName, objectNameU, true))
                {
                    found = true;
                    break;
                }

            } while (true);

            if (!directoryHandle.IsNull)
                NtClose(directoryHandle);

            return found;
        }

        /// <summary>
        /// supGetSystemInfo
        /// </summary>
        /// <remarks>
        /// DON'T FORGET TO 'Marshal.FreeHGlobal()' the returned buffer!
        /// </remarks>
        private static void* GetNtSystemInfo(SystemInformationClass infoClass, out int returnSize)
        {
            var bufferSize = (int)PAGE_SIZE;
            var returnedLength = 0u;
            var buffer = Marshal.AllocHGlobal(bufferSize);

            NTSTATUS ntstatus;
            while ((ntstatus = NtQuerySystemInformation((SYSTEM_INFORMATION_CLASS)infoClass, buffer.ToPointer(), (uint)bufferSize, &returnedLength)) == (uint)NtStatus.InfoLengthMismatch)
            {
                // Retry if the buffer size is insufficient
                Marshal.FreeHGlobal(buffer);
                bufferSize <<= 1; // Double the buffer
                if (bufferSize > NTQSI_MAX_BUFFER_LENGTH)
                    throw new OutOfMemoryException();

                buffer = Marshal.AllocHGlobal(bufferSize);
            }

            if (!ntstatus.IsSuccess())
            {
                Marshal.FreeHGlobal(buffer);
                throw new NtStatusException(ntstatus); // TODO: Find the most fitting exception class or make a new one
            }

            returnSize = (int)returnedLength;
            return buffer.ToPointer();
        }

        private delegate NTSTATUS QueryInformationRoutine(HANDLE handle, uint informationClass, void* informationBuffer, uint informationBufferLength, out uint returnLength);

        private static NTSTATUS NtQueryObjectWrap(HANDLE handle, uint informationClass, void* informationBuffer, uint informationBufferLength, out uint returnLength)
        {
            var returnedLength = 0u;
            var ntstatus = NtQueryObject(handle, (OBJECT_INFORMATION_CLASS)informationClass, informationBuffer, informationBufferLength, &returnedLength);
            returnLength = returnedLength;
            return ntstatus;
        }

        /// <summary>
        /// ntsupQuerySystemObjectInformationVariableSize
        /// </summary>
        /// <remarks>
        /// DON'T FORGET TO 'Marshal.FreeHGlobal()' the returned buffer!
        /// </remarks>
        private static IntPtr QuerySystemObjectInformation(QueryInformationRoutine queryRoutine, HANDLE objectHandle, uint informationClass, out int returnLength)
        {
            var ntstatus = queryRoutine(objectHandle, informationClass, null, 0, out var bufferSize);
            if (ntstatus != (uint)NtStatus.BufferOverflow && ntstatus != (uint)NtStatus.BufferTooSmall && ntstatus != (uint)NtStatus.InfoLengthMismatch)
                throw new NtStatusException(ntstatus);

            var buffer = Marshal.AllocHGlobal((int)bufferSize);
            ntstatus = queryRoutine(objectHandle, informationClass, buffer.ToPointer(), bufferSize, out var returnedLength);
            if (!ntstatus.IsSuccess())
            {
                Marshal.FreeHGlobal(buffer);
                throw new NtStatusException(ntstatus);
            }

            returnLength = (int)returnedLength;

            return buffer;
        }

        internal static IntPtr QueryObjectInformation(ObjectInformationClass informationClass, IntPtr handle, out int returnLength) => QuerySystemObjectInformation(NtQueryObjectWrap, (HANDLE)handle, (uint)informationClass, out returnLength);

        /// <summary>
        /// ntsupGetLoadedModulesListEx
        /// </summary>
        /// <remarks>
        /// DON'T FORGET TO 'Marshal.FreeHGlobal()' the returned buffer!
        /// </remarks>
        internal static IntPtr GetLoadedModulesList(bool extendedOutput, out int returnLength)
        {
            var infoClass = (uint)(extendedOutput ? SystemInformationClass.SystemModuleInformationEx : SystemInformationClass.SystemModuleInformation);
            var bufferSize = 0u;
            var ntstatus = NtQuerySystemInformation((SYSTEM_INFORMATION_CLASS)infoClass, null, 0, &bufferSize);
            if (ntstatus != (uint)NtStatus.InfoLengthMismatch)
                throw new NtStatusException(ntstatus);

            var returnedLength = 0u;
            var buffer = Marshal.AllocHGlobal((int)bufferSize);
            ntstatus = NtQuerySystemInformation((SYSTEM_INFORMATION_CLASS)infoClass, buffer.ToPointer(), bufferSize, &returnedLength);
            returnLength = (int)returnedLength;

            // Handle unexpected return (check out  KDU ntsup.c#L810 for more information)
            if (ntstatus == (uint)NtStatus.BufferOverflow)
            {
                if (((RTL_PROCESS_MODULES*)buffer)->NumberOfModules != 0)
                    return buffer;
            }

            if (!ntstatus.IsSuccess())
                throw new NtStatusException(ntstatus);

            return buffer;
        }

        internal static IntPtr GetNtBase()
        {
            var buffer = GetLoadedModulesList(false, out _);
            try
            {
                // ntoskrnl module is always located at Modules[0]
                return ((RTL_PROCESS_MODULES*)buffer)->Modules[0].ImageBase;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        /// <summary>
        /// supQueryMaximumUserModeAddress
        /// </summary>
        internal static IntPtr GetMaxUserModeAddress()
        {
            var returnLength = 0u;
            var info = new SYSTEM_BASIC_INFORMATION();
            var ntstatus = NtQuerySystemInformation(SYSTEM_INFORMATION_CLASS.SystemBasicInformation, &info, (uint)sizeof(SYSTEM_BASIC_INFORMATION), ref returnLength);
            if (ntstatus.IsSuccess())
                return (IntPtr)info.MaximumUserModeAddress.ToPointer();

            GetSystemInfo(out var sysInfo);
            return (IntPtr)sysInfo.lpMaximumApplicationAddress;
        }

        internal static bool IsSecureBootEnabled()
        {
            SetPrivilegeState(Privilege.SE_SYSTEM_ENVIRONMENT_PRIVILEGE, true);

            BOOLEAN state = false;
            GetFirmwareEnvironmentVariable("SecureBoot", "{8be4df61-93ca-11d2-aa0d-00e098032b8c}", &state, (uint)sizeof(BOOLEAN));

            SetPrivilegeState(Privilege.SE_SYSTEM_ENVIRONMENT_PRIVILEGE, false);

            return state;
        }

        internal static FirmwareType GetFirmwareType()
        {
            var returnLength = 0u;
            var info = new SYSTEM_BOOT_ENVIRONMENT_INFORMATION();
            var ntstatus = NtQuerySystemInformation((SYSTEM_INFORMATION_CLASS)SystemInformationClass.SystemBootEnvironmentInformation, &info, (uint)sizeof(SYSTEM_BOOT_ENVIRONMENT_INFORMATION), &returnLength);
            if (!ntstatus.IsSuccess())
                throw new NtStatusException(ntstatus);

            return info.FirmwareType;
        }
    }
}
