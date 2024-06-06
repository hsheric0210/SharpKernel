using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Windows.Win32.Foundation;

namespace SharpKernelLib.Utils
{
    /// <summary>
    /// OBJECT_ATTRIBUTES.Attributes field
    /// </summary>
    [Flags]
    internal enum ObjectAttributes : uint
    {
        None = 0,
        OBJ_PROTECT_CLOSE = 0x00000001,
        OBJ_INHERIT = 0x00000002,
        OBJ_AUDIT_OBJECT_CLOSE = 0x00000004,
        OBJ_PERMANENT = 0x00000010,
        OBJ_EXCLUSIVE = 0x00000020,
        OBJ_CASE_INSENSITIVE = 0x00000040,
        OBJ_OPENIF = 0x00000080,
        OBJ_OPENLINK = 0x00000100,
        OBJ_KERNEL_HANDLE = 0x00000200,
        OBJ_FORCE_ACCESS_CHECK = 0x00000400,
        OBJ_IGNORE_IMPERSONATED_DEVICEMAP = 0x00000800,
        OBJ_DONT_REPARSE = 0x00001000,
        OBJ_VALID_ATTRIBUTES = 0x00001FF2,
    }

    [Flags]
    internal enum CodeIntegrityOptions : uint
    {
        None = 0,
        Enabled = 0x01,
        StrictMode = 0x1000,
        IUM = 0x2000,
    }

    internal enum SECTION_INHERIT : uint
    {
        None = 0,
        ViewShare = 1,
        ViewUnmap = 2,
    }

    [Flags]
    internal enum AccessMask : uint
    {
        DELETE = 0x00010000,
        READ_CONTROL = 0x00020000,
        WRITE_DAC = 0x00040000,
        WRITE_OWNER = 0x00080000,
        SYNCHRONIZE = 0x00100000,

        STANDARD_RIGHTS_REQUIRED = 0x000F0000,

        STANDARD_RIGHTS_READ = 0x00020000,
        STANDARD_RIGHTS_WRITE = 0x00020000,
        STANDARD_RIGHTS_EXECUTE = 0x00020000,

        STANDARD_RIGHTS_ALL = 0x001F0000,

        SPECIFIC_RIGHTS_ALL = 0x0000FFFF,

        ACCESS_SYSTEM_SECURITY = 0x01000000,

        MAXIMUM_ALLOWED = 0x02000000,

        GENERIC_READ = 0x80000000,
        GENERIC_WRITE = 0x40000000,
        GENERIC_EXECUTE = 0x20000000,
        GENERIC_ALL = 0x10000000,

        DESKTOP_READOBJECTS = 0x00000001,
        DESKTOP_CREATEWINDOW = 0x00000002,
        DESKTOP_CREATEMENU = 0x00000004,
        DESKTOP_HOOKCONTROL = 0x00000008,
        DESKTOP_JOURNALRECORD = 0x00000010,
        DESKTOP_JOURNALPLAYBACK = 0x00000020,
        DESKTOP_ENUMERATE = 0x00000040,
        DESKTOP_WRITEOBJECTS = 0x00000080,
        DESKTOP_SWITCHDESKTOP = 0x00000100,

        WINSTA_ENUMDESKTOPS = 0x00000001,
        WINSTA_READATTRIBUTES = 0x00000002,
        WINSTA_ACCESSCLIPBOARD = 0x00000004,
        WINSTA_CREATEDESKTOP = 0x00000008,
        WINSTA_WRITEATTRIBUTES = 0x00000010,
        WINSTA_ACCESSGLOBALATOMS = 0x00000020,
        WINSTA_EXITWINDOWS = 0x00000040,
        WINSTA_ENUMERATE = 0x00000100,
        WINSTA_READSCREEN = 0x00000200,

        WINSTA_ALL_ACCESS = 0x0000037F,

        DIRECTORY_QUERY = 0x0001,

        SECTION_QUERY = 0x0001,
        SECTION_MAP_WRITE = 0x0002,
        SECTION_MAP_READ = 0x0004,
        SECTION_MAP_EXECUTE = 0x0008,
        SECTION_EXTEND_SIZE = 0x0010,
        SECTION_MAP_EXECUTE_EXPLICIT = 0x0020,
        SECTION_ALL_ACCESS = STANDARD_RIGHTS_REQUIRED | SECTION_QUERY | SECTION_MAP_WRITE | SECTION_MAP_READ | SECTION_MAP_EXECUTE | SECTION_EXTEND_SIZE,

        TOKEN_ASSIGN_PRIMARY = 0x0001,
        TOKEN_DUPLICATE = 0x0002,
        TOKEN_IMPERSONATE = 0x0004,
        TOKEN_QUERY = 0x0008,
        TOKEN_QUERY_SOURCE = 0x0010,
        TOKEN_ADJUST_PRIVILEGES = 0x0020,
        TOKEN_ADJUST_GROUPS = 0x0040,
        TOKEN_ADJUST_DEFAULT = 0x0080,
        TOKEN_ADJUST_SESSIONID = 0x0100,
    }

    internal enum Privilege : uint
    {
        SE_CREATE_TOKEN_PRIVILEGE = 2,
        SE_ASSIGNPRIMARYTOKEN_PRIVILEGE = 3,
        SE_LOCK_MEMORY_PRIVILEGE = 4,
        SE_INCREASE_QUOTA_PRIVILEGE = 5,
        SE_MACHINE_ACCOUNT_PRIVILEGE = 6,
        SE_TCB_PRIVILEGE = 7,
        SE_SECURITY_PRIVILEGE = 8,
        SE_TAKE_OWNERSHIP_PRIVILEGE = 9,
        SE_LOAD_DRIVER_PRIVILEGE = 10,
        SE_SYSTEM_PROFILE_PRIVILEGE = 11,
        SE_SYSTEMTIME_PRIVILEGE = 12,
        SE_PROF_SINGLE_PROCESS_PRIVILEGE = 13,
        SE_INC_BASE_PRIORITY_PRIVILEGE = 14,
        SE_CREATE_PAGEFILE_PRIVILEGE = 15,
        SE_CREATE_PERMANENT_PRIVILEGE = 16,
        SE_BACKUP_PRIVILEGE = 17,
        SE_RESTORE_PRIVILEGE = 18,
        SE_SHUTDOWN_PRIVILEGE = 19,
        SE_DEBUG_PRIVILEGE = 20,
        SE_AUDIT_PRIVILEGE = 21,
        SE_SYSTEM_ENVIRONMENT_PRIVILEGE = 22,
        SE_CHANGE_NOTIFY_PRIVILEGE = 23,
        SE_REMOTE_SHUTDOWN_PRIVILEGE = 24,
        SE_UNDOCK_PRIVILEGE = 25,
        SE_SYNC_AGENT_PRIVILEGE = 26,
        SE_ENABLE_DELEGATION_PRIVILEGE = 27,
        SE_MANAGE_VOLUME_PRIVILEGE = 28,
        SE_IMPERSONATE_PRIVILEGE = 29,
        SE_CREATE_GLOBAL_PRIVILEGE = 30,
        SE_TRUSTED_CREDMAN_ACCESS_PRIVILEGE = 31,
        SE_RELABEL_PRIVILEGE = 32,
        SE_INC_WORKING_SET_PRIVILEGE = 33,
        SE_TIME_ZONE_PRIVILEGE = 34,
        SE_CREATE_SYMBOLIC_LINK_PRIVILEGE = 35,
        SE_DELEGATE_SESSION_USER_IMPERSONATE_PRIVILEGE = 36,
    }

    internal enum FirmwareType : uint
    {
        Unknown = 0,
        Bios = 1,
        Uefi = 2
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct SYSTEM_CODEINTEGRITY_INFORMATION
    {
        public uint Length;
        public CodeIntegrityOptions CodeIntegrityOptions;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct SYSTEM_HANDLE_INFORMATION_EX
    {
        public UIntPtr NumberOfHandles;
        public UIntPtr Reserved;
        public IntPtr Handles; // SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX[NumberOfHandles]
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

    [StructLayout(LayoutKind.Sequential)]
    internal struct OBJECT_DIRECTORY_INFORMATION
    {
        public UNICODE_STRING Name;
        public UNICODE_STRING TypeName;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct OBJECT_NAME_INFORMATION
    {
        public UNICODE_STRING Name;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct RTL_PROCESS_MODULES
    {
        public uint NumberOfModules;
        public IntPtr Modules; // RTL_PROCESS_MODULE_INFORMATION[NumberOfModules]
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
        public char[] FullPathName;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct SYSTEM_BASIC_INFORMATION
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
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct SYSTEM_BOOT_ENVIRONMENT_INFORMATION
    {
        public Guid BootIdentifier;
        public FirmwareType FirmwareType;
        public ulong BootFlags;
    }
}
