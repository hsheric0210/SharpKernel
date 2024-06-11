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
using Windows.Win32.System.Memory;
using Windows.Win32.Devices.DeviceAndDriverInstallation;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SharpKernelLib.Utils
{
    // Port of KDU Hamakaze sup.c and sup.h
    internal unsafe static partial class NtWrapper
    {
        internal const uint MAX_CLASS_NAME_LEN = 32;

        internal static IntPtr InstallDriverFromInf(string infName, byte[] hardwareId, IntPtr DeviceInfoData, uint installFlags)
        {
            var devInfoSet = HDEVINFO.Null;
            var classNameBuffer = Marshal.AllocHGlobal((int)MAX_CLASS_NAME_LEN * sizeof(char));
            var className = new PWSTR((char*)classNameBuffer.ToPointer());
            var DeviceInfoDataPtr = (SP_DEVINFO_DATA*)DeviceInfoData;

            try
            {
                if (!SetupDiGetINFClass(infName, out var classGuid, className, MAX_CLASS_NAME_LEN, null))
                    throw new ProviderLoadException("SetupDiGetINFClass", new Win32Exception());

                devInfoSet = SetupDiCreateDeviceInfoList(&classGuid, HWND.Null);
                if (devInfoSet == HDEVINFO.Null)
                    throw new ProviderLoadException("SetupDiCreateDeviceInfoList", new Win32Exception());

                const uint DICD_GENERATE_ID = 0x00000001;
                if (!SetupDiCreateDeviceInfo(devInfoSet, className, &classGuid, null, HWND.Null, DICD_GENERATE_ID, DeviceInfoDataPtr))
                    throw new ProviderLoadException("SetupDiCreateDeviceInfo", new Win32Exception());

                var hardwareIdPtr = (byte*)Unsafe.AsPointer(ref hardwareId);

                const uint SPDRP_HARDWAREID = 0x00000001;
                if (!SetupDiSetDeviceRegistryProperty(devInfoSet, DeviceInfoDataPtr, SPDRP_HARDWAREID, hardwareIdPtr, (uint)hardwareId.Length))
                    throw new ProviderLoadException("SetupDiSetDeviceRegistryProperty", new Win32Exception());

                const uint DIF_REGISTERDEVICE = 0x00000019;
                if (!SetupDiCallClassInstaller(DIF_REGISTERDEVICE, devInfoSet, DeviceInfoDataPtr))
                    throw new ProviderLoadException("SetupDiCallClassInstaller", new Win32Exception());

                if (!UpdateDriverForPlugAndPlayDevices(HWND.Null, new string((char*)hardwareIdPtr, 0, hardwareId.Length), infName, (UPDATEDRIVERFORPLUGANDPLAYDEVICES_FLAGS)installFlags, null))
                    throw new ProviderLoadException("UpdateDriverForPlugAndPlayDevices", new Win32Exception());
            }
            catch
            {
                if (devInfoSet != HDEVINFO.Null)
                {
                    SetupDiDestroyDeviceInfoList(devInfoSet);
                    devInfoSet = HDEVINFO.Null;
                }

                return HDEVINFO.Null;
            }
            finally
            {
                Marshal.FreeHGlobal(classNameBuffer);
            }

            return devInfoSet;
        }

        internal static void RemoveDriverPackage(IntPtr deviceInfo, IntPtr deviceInfoData)
        {
            if (!SetupDiRemoveDevice((HDEVINFO)deviceInfo, (SP_DEVINFO_DATA*)deviceInfoData))
                throw new ProviderUnloadException("SetupDiRemoveDevice", new Win32Exception());

            if (!SetupDiDestroyDeviceInfoList((HDEVINFO)deviceInfo))
                throw new ProviderUnloadException("SetupDiDestroyDeviceInfoList", new Win32Exception());
        }
    }
}
