using SharpKernelLib.Exception;
using System;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Windows.Wdk;
using Windows.Wdk.System.SystemInformation;
using Windows.Win32.Foundation;

namespace SharpKernelLib.Utils
{
    #region SYSTEM_CODEINTEGRITY_INFORMATION

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct SYSTEM_CODEINTEGRITY_INFORMATION
    {
        public uint Length;
        public CodeIntegrityOptions CodeIntegrityOptions;

        public static SYSTEM_CODEINTEGRITY_INFORMATION QueryData()
        {
            var returnLength = 0u;
            var info = new SYSTEM_CODEINTEGRITY_INFORMATION();
            info.Length = (uint)Marshal.SizeOf(info);

            var ntstatus = PInvoke.NtQuerySystemInformation(SYSTEM_INFORMATION_CLASS.SystemCodeIntegrityInformation, &info, (uint)sizeof(SYSTEM_CODEINTEGRITY_INFORMATION), ref returnLength);
            if (!ntstatus.IsSuccess())
                throw new NtStatusException(ntstatus);

            return info;
        }
    }

    #endregion

    #region SYSTEM_HANDLE_INFORMATION_EX

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct SYSTEM_HANDLE_INFORMATION_EX
    {
        public UIntPtr NumberOfHandles;
        public UIntPtr Reserved;
        [MarshalAs(UnmanagedType.LPArray, SizeConst = 1)]
        public SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX[] Handles;

        /// <remarks>Remember to call 'Marshal.FreeHGlobal'</remarks>
        public static SYSTEM_HANDLE_INFORMATION_EX* QueryData() => (SYSTEM_HANDLE_INFORMATION_EX*)NtQueryUtils.GetNtSystemInfo(SystemInformationClass.SystemExtendedHandleInformation, out _);
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX
    {
        public IntPtr Object;
        public IntPtr UniqueProcessId;
        public IntPtr HandleValue;
        public uint GrantedAccess;
        public ushort CreatorBackTraceIndex;
        public ushort ObjectTypeIndex;
        public uint HandleAttributes;
        public uint Reserved;
    }

    #endregion

    #region OBJECT_DIRECTORY_INFORMATION

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct OBJECT_DIRECTORY_INFORMATION
    {
        public UNICODE_STRING Name;
        public UNICODE_STRING TypeName;

        /// <remarks>Remember to call 'Marshal.FreeHGlobal'</remarks>
        public static OBJECT_DIRECTORY_INFORMATION* QueryData(HANDLE directoryHandle, ref uint context)
        {
            var ntstatus = NtUndocumented.NtQueryDirectoryObject(directoryHandle, null, 0, true, false, ref context, out var requiredBufferSize);
            if (ntstatus != (uint)NtStatus.BufferTooSmall)
                throw new NtStatusException(ntstatus);

            var buffer = Marshal.AllocHGlobal(requiredBufferSize);
            ntstatus = NtUndocumented.NtQueryDirectoryObject(directoryHandle, (OBJECT_DIRECTORY_INFORMATION*)buffer.ToPointer(), (uint)requiredBufferSize, true, false, ref context, out requiredBufferSize);
            if (!ntstatus.IsSuccess())
            {
                Marshal.FreeHGlobal(buffer);
                throw new NtStatusException(ntstatus);
            }

            return (OBJECT_DIRECTORY_INFORMATION*)buffer;
        }
    }

    #endregion

    #region OBJECT_NAME_INFORMATION

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct OBJECT_NAME_INFORMATION
    {
        public UNICODE_STRING Name;

        /// <remarks>Remember to call 'Marshal.FreeHGlobal'</remarks>
        public static OBJECT_NAME_INFORMATION* QueryData(HANDLE handle) => (OBJECT_NAME_INFORMATION*)NtQueryUtils.QueryObjectInformation(ObjectInformationClass.ObjectNameInformation, handle, out _);
    }

    #endregion

    #region RTL_PROCESS_MODULES

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct RTL_PROCESS_MODULES
    {
        public uint NumberOfModules;
        [MarshalAs(UnmanagedType.LPArray, SizeConst = 1)]
        public RTL_PROCESS_MODULE_INFORMATION[] Modules;

        /// <remarks>Remember to call 'Marshal.FreeHGlobal'</remarks>
        public static RTL_PROCESS_MODULES* QueryLoadedSystemModules(bool extended)
        {
            var infoClass = (uint)(extended ? SystemInformationClass.SystemModuleInformationEx : SystemInformationClass.SystemModuleInformation);
            var bufferSize = 0u;
            var ntstatus = PInvoke.NtQuerySystemInformation((SYSTEM_INFORMATION_CLASS)infoClass, null, 0, &bufferSize);
            if (ntstatus != (uint)NtStatus.InfoLengthMismatch)
                throw new NtStatusException(ntstatus);

            var returnLength = 0u;
            var buffer = (RTL_PROCESS_MODULES*)Marshal.AllocHGlobal((int)bufferSize);
            ntstatus = PInvoke.NtQuerySystemInformation((SYSTEM_INFORMATION_CLASS)infoClass, buffer, bufferSize, &returnLength);

            // Handle unexpected return (check out  KDU ntsup.c#L810 for more information)
            if (ntstatus == (uint)NtStatus.BufferOverflow)
            {
                if (buffer->NumberOfModules == 0)
                    throw new NtStatusException(ntstatus);
            }

            if (!ntstatus.IsSuccess())
                throw new NtStatusException(ntstatus);

            return buffer;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct RTL_PROCESS_MODULE_INFORMATION
    {
        public IntPtr Section;
        public IntPtr MappedBase;
        public IntPtr ImageBase;
        public uint ImageSize;
        public uint Flags;
        public ushort LoadOrderIndex;
        public ushort InitOrderIndex;
        public ushort LoadCount;
        public ushort OffsetToFileName;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public byte[] FullPathName;
    }

    #endregion

    #region SYSTEM_BASIC_INFORMATION

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct SYSTEM_BASIC_INFORMATION
    {
        public uint Reserved;
        public uint TimerResolution;
        public uint PageSize;
        public uint NumberOfPhysicalPages;
        public uint LowestPhysicalPageNumber;
        public uint HighestPhysicalPageNumber;
        public uint AllocationGranularity;
        public UIntPtr MinimumUserModeAddress;
        public UIntPtr MaximumUserModeAddress;
        public UIntPtr ActiveProcessorsAffinityMask;
        public char NumberOfProcessors;

        public static SYSTEM_BASIC_INFORMATION QueryData()
        {
            var returnLength = 0u;
            var info = new SYSTEM_BASIC_INFORMATION();
            var ntstatus = PInvoke.NtQuerySystemInformation((SYSTEM_INFORMATION_CLASS)SystemInformationClass.SystemBasicInformation, &info, (uint)sizeof(SYSTEM_BASIC_INFORMATION), ref returnLength);
            if (!ntstatus.IsSuccess())
                throw new NtStatusException(ntstatus);

            return info;
        }
    }

    #endregion

    #region SYSTEM_BOOT_ENVIRONMENT_INFORMATION

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct SYSTEM_BOOT_ENVIRONMENT_INFORMATION
    {
        public Guid BootIdentifier;
        public FirmwareType FirmwareType;
        public ulong BootFlags;

        public static SYSTEM_BOOT_ENVIRONMENT_INFORMATION QueryData()
        {
            var returnLength = 0u;
            var info = new SYSTEM_BOOT_ENVIRONMENT_INFORMATION();
            var ntstatus = PInvoke.NtQuerySystemInformation((SYSTEM_INFORMATION_CLASS)SystemInformationClass.SystemBootEnvironmentInformation, &info, (uint)sizeof(SYSTEM_BOOT_ENVIRONMENT_INFORMATION), ref returnLength);
            if (!ntstatus.IsSuccess())
                throw new NtStatusException(ntstatus);

            return info;
        }
    }

    #endregion

    #region SYSTEM_POOLTAG_INFORMATION

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct SYSTEM_POOLTAG_INFORMATION
    {
        public uint Count;
        [MarshalAs(UnmanagedType.LPArray, SizeConst = 1)]
        public SYSTEM_POOLTAG[] TagInfo;

        /// <remarks>Remember to call 'Marshal.FreeHGlobal'</remarks>
        public static SYSTEM_POOLTAG_INFORMATION* QueryData() => (SYSTEM_POOLTAG_INFORMATION*)NtQueryUtils.GetNtSystemInfo(SystemInformationClass.SystemPoolTagInformation, out _);
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct SYSTEM_POOLTAG
    {
        public uint Tag;
        public uint PagedAllocs;
        public uint PagedFrees;
        public UIntPtr PagedUsed;
        public uint NonPagedAllocs;
        public uint NonPagedFrees;
        public UIntPtr NonPagedUsed;
    }

    #endregion
}
