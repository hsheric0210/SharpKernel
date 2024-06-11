using System;
using System.Runtime.InteropServices;
using Windows.Win32.Foundation;
using Windows.Wdk.Foundation;
using Microsoft.Win32.SafeHandles;
using Windows.Win32.System.Memory;
using static SharpKernelLib.Utils.NtWrapper;
using Windows.Win32.Security;
using Windows.Win32.Devices.DeviceAndDriverInstallation;
using System.Runtime.CompilerServices;

namespace SharpKernelLib.Utils
{
    internal unsafe class NtUndocumented
    {
        internal delegate int VectoredExceptionHandler(ref ExceptionPointers exceptionPointers);

        internal static HANDLE NtCurrentProcess() => new HANDLE((IntPtr)(-1));

        internal static SafeHandle NtCurrentProcess_SafeHandle() => new SafeProcessHandle((IntPtr)(-1), false);

        [DllImport("kernel32.dll")]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern IntPtr AddVectoredExceptionHandler(uint first, VectoredExceptionHandler handler);

        [DllImport("ntdll.dll", ExactSpelling = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern NTSTATUS NtOpenDirectoryObject(out HANDLE DirectoryHandle, [In] AccessMask DesiredAccess, [In] OBJECT_ATTRIBUTES* ObjectAttributes);

        [DllImport("ntdll.dll", ExactSpelling = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern NTSTATUS NtQueryDirectoryObject([In] HANDLE DirectoryHandle, [Out] OBJECT_DIRECTORY_INFORMATION* Buffer, [In] uint Length, [In] bool ReturnSingleEntry, [In] bool RestartScan, [In, Out] ref uint Context, [Optional] out int ReturnLength);

        [DllImport("ntdll.dll", ExactSpelling = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool RtlDosPathNameToNtPathName_U([In, MarshalAs(UnmanagedType.LPWStr)] string DosFileName, [Out] UNICODE_STRING* NtFileName, [Out, Optional] PWSTR* FilePath, void* Reserved);

        [DllImport("ntdll.dll", ExactSpelling = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern NTSTATUS NtLoadDriver([In] UNICODE_STRING* DriverServiceName);

        [DllImport("ntdll.dll", ExactSpelling = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern NTSTATUS NtUnloadDriver([In] UNICODE_STRING* DriverServiceName);

        [DllImport("ntdll.dll", ExactSpelling = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern NTSTATUS NtMapViewOfSection([In] HANDLE SectionHandle, [In] HANDLE ProcessHandle, [In, Out] void** BaseAddress, [In] uint* ZeroBits, [In] UIntPtr commitSize, [In, Out, Optional] ulong* SectionOffset, [In, Out, Optional] UIntPtr* ViewSize, [In] SECTION_INHERIT InheritDisposition, AllocationTypes allocationType, PageProtections win32Protect);

        [DllImport("ntdll.dll", ExactSpelling = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern NTSTATUS NtUnmapViewOfSection([In] HANDLE ProcessHandle, [In, Optional] void* BaseAddress);

        [DllImport("ntdll.dll", ExactSpelling = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern NTSTATUS NtOpenSection(out HANDLE SectionHandle, [In] AccessMask DesiredAccess, [In] OBJECT_ATTRIBUTES* ObjectAttributes);

        [DllImport("ntdll.dll", ExactSpelling = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern NTSTATUS LdrLoadDll([In, Optional] PCWSTR DllPath, [In, Optional] uint* DllCharacteristics, [In] UNICODE_STRING* DllName, out void* DllHandle);

        [DllImport("ntdll.dll", ExactSpelling = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern NTSTATUS LdrUnloadDll([In] void* DllHandle);

        [DllImport("ntdll.dll", ExactSpelling = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern NTSTATUS RtlSetDaclSecurityDescriptor([In, Out] PSECURITY_DESCRIPTOR SecurityDescriptor, [In] BOOLEAN DaclPresent, [In, Optional] ACL* Dacl, [In] BOOLEAN DaclDefaulted);

        [DllImport("ntdll.dll", ExactSpelling = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern IMAGE_NT_HEADERS* RtlImageNtHeader([In] void* imageBase);

        [DllImport("ntdll.dll", ExactSpelling = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern NTSTATUS LdrFindEntryForAddress([In] void* Address, [Out] out LDR_DATA_TABLE_ENTRY TableEntry);

        [DllImport("setupapi.dll", EntryPoint = "SetupDiCreateDeviceInfoW")]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern BOOL SetupDiCreateDeviceInfo(HDEVINFO DeviceInfoSet, PCWSTR DeviceName, Guid* ClassGuid, PCWSTR DeviceDescription, HWND HwndParent, uint Creationflags, SP_DEVINFO_DATA* DeviceInfoData);

        [DllImport("setupapi.dll", EntryPoint = "SetupDiSetDeviceRegistryPropertyW")]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern BOOL SetupDiSetDeviceRegistryProperty(HDEVINFO DeviceInfoSet, SP_DEVINFO_DATA* DeviceInfoData, uint Property, byte* PropertyBuffer, uint PropertyBufferSize);

        [DllImport("setupapi.dll", ExactSpelling = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern BOOL SetupDiCallClassInstaller(uint InstallFunction, HDEVINFO DeviceInfoSet, SP_DEVINFO_DATA* DeviceInfoData);

        [DllImport("setupapi.dll", ExactSpelling = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern BOOL SetupDiRemoveDevice(HDEVINFO DeviceInfoSet, SP_DEVINFO_DATA* DeviceInfoData);
    }
}
