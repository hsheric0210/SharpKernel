using System;
using System.Runtime.InteropServices;
using Windows.Wdk.System.SystemInformation;
using Windows.Wdk.Foundation;
using Windows.Win32.Foundation;
using Microsoft.Win32;
using SharpKernelLib.Exception;
using Windows.Win32.Storage.FileSystem;
using Windows.Wdk.Storage.FileSystem;

using static SharpKernelLib.Utils.NtConstants;
using static SharpKernelLib.Utils.NtUndocumented;
using static Windows.Win32.PInvoke;
using static Windows.Wdk.PInvoke;

namespace SharpKernelLib.Utils
{
    // Port of KDU Hamakaze sup.c and sup.h
    internal unsafe static class NtQueryUtils
    {
        internal const uint NTQSI_MAX_BUFFER_LENGTH = 512 * 1024 * 1024; // 512 MiB

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
                var buffer = OBJECT_DIRECTORY_INFORMATION.QueryData(directoryHandle, ref context);
                var dirName = buffer->Name;
                Marshal.FreeHGlobal((IntPtr)buffer);
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
        internal static void* GetNtSystemInfo(SystemInformationClass infoClass, out int returnSize)
        {
            var bufferSize = (int)PAGE_SIZE;
            var returnedLength = 0u;
            var buffer = Marshal.AllocHGlobal(bufferSize);

            NTSTATUS ntstatus;
            while ((ntstatus = NtQuerySystemInformation((SYSTEM_INFORMATION_CLASS)infoClass, buffer.ToPointer(), (uint)bufferSize, &returnedLength)) == (uint)NtStatus.InfoLengthMismatch)
            {
                // Retry if the buffer size is insufficient
                Marshal.FreeHGlobal(buffer);
                bufferSize <<= 1; // Double the buffer size
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

        internal static IntPtr GetNtBase()
        {
            var buffer = RTL_PROCESS_MODULES.QueryLoadedSystemModules(false);
            try
            {
                // ntoskrnl module is always located at Modules[0]
                return buffer->Modules[0].ImageBase;
            }
            finally
            {
                Marshal.FreeHGlobal((IntPtr)buffer);
            }
        }

        /// <summary>
        /// supQueryMaximumUserModeAddress
        /// </summary>
        internal static IntPtr GetMaxUserModeAddress()
        {
            try
            {
                return (IntPtr)SYSTEM_BASIC_INFORMATION.QueryData().MaximumUserModeAddress.ToUInt64();
            }
            catch
            {
                // Ignored
            }

            // Fallback method
            GetSystemInfo(out var sysInfo);
            return (IntPtr)sysInfo.lpMaximumApplicationAddress;
        }

        internal static bool IsSecureBootEnabled()
        {
            NtSecurity.SetPrivilegeState(Privilege.SE_SYSTEM_ENVIRONMENT_PRIVILEGE, true);

            BOOLEAN state = false;
            GetFirmwareEnvironmentVariable("SecureBoot", "{8be4df61-93ca-11d2-aa0d-00e098032b8c}", &state, (uint)sizeof(BOOLEAN));

            NtSecurity.SetPrivilegeState(Privilege.SE_SYSTEM_ENVIRONMENT_PRIVILEGE, false);

            return state;
        }

        internal static FirmwareType GetFirmwareType() => SYSTEM_BOOT_ENVIRONMENT_INFORMATION.QueryData().FirmwareType;

        internal static uint ChooseNonPagedPoolTag()
        {
            var info = SYSTEM_POOLTAG_INFORMATION.QueryData();
            var tag = 0x20206f49u; // '  oI'
            var maxUse = 0ul;

            try
            {
                for (uint i = 0, j = info->Count; i < j; i++)
                {
                    var pool = info->TagInfo[i];
                    if (pool.NonPagedUsed.ToUInt64() > maxUse)
                    {
                        maxUse = pool.NonPagedUsed.ToUInt64();
                        tag = pool.Tag;
                    }
                }
            }
            finally
            {
                Marshal.FreeHGlobal((IntPtr)info);
            }

            return tag;
        }
    }
}
