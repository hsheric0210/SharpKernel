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
using Windows.Win32.System.IO;

namespace SharpKernelLib.Utils
{
    // Port of KDU Hamakaze sup.c and sup.h
    internal unsafe static partial class NtWrapper
    {
        private static void CreateDriverEntryRegistry(string serviceName, string driverFilePath)
        {
            var driverImagePath = new UNICODE_STRING();
            if (!string.IsNullOrEmpty(driverFilePath) && !RtlDosPathNameToNtPathName_U(driverFilePath, &driverImagePath, null, null))
                throw new ProviderLoadException("LoadDriver#RtlDosPathNameToNtPathName_U");

            using (var serviceKey = Registry.LocalMachine.CreateSubKey($@"System\CurrentControlSet\Services\{serviceName}"))
            {
                serviceKey.SetValue("ErrorControl", 0, RegistryValueKind.DWord);
                serviceKey.SetValue("Type", 0, RegistryValueKind.DWord);
                serviceKey.SetValue("Start", 0, RegistryValueKind.DWord);
                if (!string.IsNullOrEmpty(driverFilePath))
                    serviceKey.SetValue("ImagePath", driverImagePath.ConvertToString(), RegistryValueKind.ExpandString);
            }

            RtlFreeUnicodeString(&driverImagePath);
        }

        /// <summary>
        /// supLoadDriver: Install driver and load it.
        /// </summary>
        internal static void LoadDriver(string driverName, string driverFilePath, bool unloadPreviousInstance)
        {
            CreateDriverEntryRegistry(driverName, driverFilePath);

            var driverServiceRegistry = new UNICODE_STRING();
            RtlInitUnicodeString(ref driverServiceRegistry, $@"\Registry\Machine\System\CurrentControlSet\Services\{driverName}");

            var ntstatus = NtLoadDriver(&driverServiceRegistry);

            // Retry
            if (unloadPreviousInstance &&
                (ntstatus == 0xC000010E || // STATUS_IMAGE_ALREADY_LOADED
                    ntstatus == (uint)NtStatus.ObjectNameCollision ||
                    ntstatus == (uint)NtStatus.ObjectNameExists))
            {
                ntstatus = NtUnloadDriver(&driverServiceRegistry);
                if (ntstatus.IsSuccess())
                    throw new ProviderLoadException("LoadDriver#NtUnloadDriver", new NtStatusException(ntstatus));

                ntstatus = NtLoadDriver(&driverServiceRegistry);
            }

            if (ntstatus != (uint)NtStatus.ObjectNameExists && !ntstatus.IsSuccess())
                throw new ProviderLoadException("LoadDriver#NtLoadDriver", new NtStatusException(ntstatus));
        }

        internal static void UnloadDriver(string driverName, bool removeRegistry)
        {
            CreateDriverEntryRegistry(driverName, null);

            var driverServiceRegistry = new UNICODE_STRING();
            var subkey = $@"System\CurrentControlSet\Services\{driverName}";
            RtlInitUnicodeString(ref driverServiceRegistry, $@"\Registry\Machine\{subkey}");

            var ntstatus = NtUnloadDriver(&driverServiceRegistry);
            if (!ntstatus.IsSuccess())
                throw new ProviderUnloadException("NtUnloadDriver", new NtStatusException(ntstatus));

            if (removeRegistry)
                Registry.LocalMachine.DeleteSubKeyTree(subkey);
        }

        /// <summary>
        /// supOpenDriverEx: Open handle for driver.
        /// </summary>
        internal static NTSTATUS OpenDriverCore(string deviceLink, AccessMask desiredAccess, out HANDLE deviceHandle)
        {
            deviceHandle = HANDLE.Null;

            var deviceLinkU = new UNICODE_STRING();
            RtlInitUnicodeString(ref deviceLinkU, deviceLink);

            var objectAttributes = new OBJECT_ATTRIBUTES
            {
                Length = (uint)sizeof(OBJECT_ATTRIBUTES),
                RootDirectory = HANDLE.Null,
                Attributes = (uint)ObjectAttributes.OBJ_CASE_INSENSITIVE,
                ObjectName = &deviceLinkU,
                SecurityDescriptor = null,
                SecurityQualityOfService = null
            };

            // Open the object
            var ntstatus = NtCreateFile(out var tmpDeviceHandle, (FILE_ACCESS_RIGHTS)desiredAccess, objectAttributes, out _, null, 0, 0, NTCREATEFILE_CREATE_DISPOSITION.FILE_OPEN, NTCREATEFILE_CREATE_OPTIONS.FILE_NON_DIRECTORY_FILE | NTCREATEFILE_CREATE_OPTIONS.FILE_SYNCHRONOUS_IO_NONALERT, null, 0);

            if (ntstatus.IsSuccess())
                deviceHandle = tmpDeviceHandle;

            return ntstatus;
        }

        /// <summary>
        /// supOpenDriver: Open handle for driver through \\DosDevices.
        /// </summary>
        internal static HANDLE OpenDriver(string deviceName, AccessMask desiredAccess)
        {
            // Try '\DosDevices\%s'
            var deviceLink = $@"\DosDevices\{deviceName}";
            var ntstatus = OpenDriverCore(deviceLink, desiredAccess, out var deviceHandle);
            if (ntstatus.IsSuccess())
                return deviceHandle;

            if (ntstatus == (uint)NtStatus.ObjectNameNotFound || ntstatus == (uint)NtStatus.NoSuchDevice)
            {
                // Try '\Device\%s'
                deviceLink = $@"\Device\{deviceName}";
                ntstatus = OpenDriverCore(deviceLink, desiredAccess, out deviceHandle);
                if (ntstatus.IsSuccess())
                    return deviceHandle;
            }

            throw new ProviderLoadException("OpenDriver#OpenDriverCore", new NtStatusException(ntstatus));
        }

        /// <summary>
        /// supCallDriverEx: Call driver.
        /// </summary>
        internal static NTSTATUS CallDriverCore(HANDLE deviceHandle, uint ioctlCode, IntPtr inputBuffer, int inputBufferLength, IntPtr outputBuffer, int outputBufferLength, out IO_STATUS_BLOCK ioStatus)
        {
            var ntstatus = NtDeviceIoControlFile(deviceHandle, HANDLE.Null, null, null, out ioStatus, ioctlCode, inputBuffer.ToPointer(), (uint)inputBufferLength, outputBuffer.ToPointer(), (uint)outputBufferLength);
            if (ntstatus == (uint)NtStatus.Pending)
                ntstatus = NtWaitForSingleObject(deviceHandle, false, null);

            return ntstatus;
        }

        /// <summary>
        /// supCallDriver: Call driver.
        /// </summary>
        internal static bool CallDriver(IntPtr deviceHandle, uint ioctlCode, IntPtr inputBuffer, int inputBufferLength, IntPtr outputBuffer, int outputBufferLength)
        {
            var ntstatus = CallDriverCore((HANDLE)deviceHandle, ioctlCode, inputBuffer, inputBufferLength, outputBuffer, outputBufferLength, out _);
            return ntstatus.IsSuccess();
        }
    }
}
