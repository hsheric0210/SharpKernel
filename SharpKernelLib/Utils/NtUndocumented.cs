using System;
using System.Runtime.InteropServices;
using Windows.Win32.Foundation;
using Windows.Wdk.Foundation;
using Microsoft.Win32.SafeHandles;
using Windows.Win32.System.Memory;

namespace SharpKernelLib.Utils
{
    internal unsafe class NtUndocumented
    {
        internal static HANDLE NtCurrentProcess() => new HANDLE((IntPtr)(-1));
        internal static SafeHandle NtCurrentProcess_SafeHandle() => new SafeProcessHandle((IntPtr)(-1), false);

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
        internal static extern NTSTATUS NtMapViewOfSection([In] HANDLE SectionHandle, [In] HANDLE ProcessHandle, [In, Out] void** BaseAddress, [In] uint* ZeroBits, [In] UIntPtr commitSize, [In, Out, Optional] ulong* SectionOffset, [In, Out, Optional] UIntPtr* ViewSize, [In] SECTION_INHERIT InheritDisposition, VIRTUAL_ALLOCATION_TYPE allocationType, PAGE_PROTECTION_FLAGS win32Protect);

        [DllImport("ntdll.dll", ExactSpelling = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern NTSTATUS NtUnmapViewOfSection([In] HANDLE ProcessHandle, [In, Optional] void* BaseAddress);

        [DllImport("ntdll.dll", ExactSpelling = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern NTSTATUS NtOpenSection(out HANDLE SectionHandle, [In] AccessMask DesiredAccess, [In] OBJECT_ATTRIBUTES* ObjectAttributes);
    }
}
