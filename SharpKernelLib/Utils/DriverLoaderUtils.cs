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
    internal unsafe static class DriverLoaderUtils
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
    }
}
