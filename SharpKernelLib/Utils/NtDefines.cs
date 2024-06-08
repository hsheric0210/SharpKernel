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

    internal enum SecurityRID : uint
    {
        SECURITY_DIALUP_RID = 0x00000001,
        SECURITY_NETWORK_RID = 0x00000002,
        SECURITY_BATCH_RID = 0x00000003,
        SECURITY_INTERACTIVE_RID = 0x00000004,
        SECURITY_LOGON_IDS_RID = 0x00000005,
        SECURITY_LOGON_IDS_RID_COUNT = 3,
        SECURITY_SERVICE_RID = 0x00000006,
        SECURITY_ANONYMOUS_LOGON_RID = 0x00000007,
        SECURITY_PROXY_RID = 0x00000008,
        SECURITY_ENTERPRISE_CONTROLLERS_RID = 0x00000009,
        SECURITY_SERVER_LOGON_RID = SECURITY_ENTERPRISE_CONTROLLERS_RID,
        SECURITY_PRINCIPAL_SELF_RID = 0x0000000A,
        SECURITY_AUTHENTICATED_USER_RID = 0x0000000B,
        SECURITY_RESTRICTED_CODE_RID = 0x0000000C,
        SECURITY_TERMINAL_SERVER_RID = 0x0000000D,
        SECURITY_REMOTE_LOGON_RID = 0x0000000E,
        SECURITY_THIS_ORGANIZATION_RID = 0x0000000F,
        SECURITY_IUSER_RID = 0x00000011,
        SECURITY_LOCAL_SYSTEM_RID = 0x00000012,
        SECURITY_LOCAL_SERVICE_RID = 0x00000013,
        SECURITY_NETWORK_SERVICE_RID = 0x00000014,

        SECURITY_ENTERPRISE_READONLY_CONTROLLERS_RID = 0x00000016,

        SECURITY_BUILTIN_DOMAIN_RID = 0x00000020,
        SECURITY_WRITE_RESTRICTED_CODE_RID = 0x00000021,

        SECURITY_PACKAGE_BASE_RID = 0x00000040,
        SECURITY_PACKAGE_RID_COUNT = 2,
        SECURITY_PACKAGE_NTLM_RID = 0x0000000A,
        SECURITY_PACKAGE_SCHANNEL_RID = 0x0000000E,
        SECURITY_PACKAGE_DIGEST_RID = 0x00000015,

        SECURITY_CRED_TYPE_BASE_RID = 0x00000041,
        SECURITY_CRED_TYPE_RID_COUNT = 2,
        SECURITY_CRED_TYPE_THIS_ORG_CERT_RID = 0x00000001,

        SECURITY_MIN_BASE_RID = 0x00000050,

        SECURITY_SERVICE_ID_BASE_RID = 0x00000050,
        SECURITY_SERVICE_ID_RID_COUNT = 6,

        SECURITY_RESERVED_ID_BASE_RID = 0x00000051,

        SECURITY_APPPOOL_ID_BASE_RID = 0x00000052,
        SECURITY_APPPOOL_ID_RID_COUNT = 6,

        SECURITY_VIRTUALSERVER_ID_BASE_RID = 0x00000053,
        SECURITY_VIRTUALSERVER_ID_RID_COUNT = 6,

        SECURITY_USERMODEDRIVERHOST_ID_BASE_RID = 0x00000054,
        SECURITY_USERMODEDRIVERHOST_ID_RID_COUNT = 6,

        SECURITY_CLOUD_INFRASTRUCTURE_SERVICES_ID_BASE_RID = 0x00000055,
        SECURITY_CLOUD_INFRASTRUCTURE_SERVICES_ID_RID_COUNT = 6,

        SECURITY_WMIHOST_ID_BASE_RID = 0x00000056,
        SECURITY_WMIHOST_ID_RID_COUNT = 6,

        SECURITY_TASK_ID_BASE_RID = 0x00000057,

        SECURITY_NFS_ID_BASE_RID = 0x00000058,

        SECURITY_COM_ID_BASE_RID = 0x00000059,

        SECURITY_WINDOW_MANAGER_BASE_RID = 0x0000005a,

        SECURITY_RDV_GFX_BASE_RID = 0x0000005b,

        SECURITY_DASHOST_ID_BASE_RID = 0x0000005c,
        SECURITY_DASHOST_ID_RID_COUNT = 6,

        SECURITY_VIRTUALACCOUNT_ID_RID_COUNT = 6,

        SECURITY_MAX_BASE_RID = 0x0000006f,

        SECURITY_MAX_ALWAYS_FILTERED = 0x000003E7,
        SECURITY_MIN_NEVER_FILTERED = 0x000003E8,

        SECURITY_OTHER_ORGANIZATION_RID = 0x000003E8,

        SECURITY_WINDOWSMOBILE_ID_BASE_RID = 0x00000070,

        DOMAIN_GROUP_RID_AUTHORIZATION_DATA_IS_COMPOUNDED = 0x000001f0,
        DOMAIN_GROUP_RID_AUTHORIZATION_DATA_CONTAINS_CLAIMS = 0x000001f1,
        DOMAIN_GROUP_RID_ENTERPRISE_READONLY_DOMAIN_CONTROLLERS = 0x000001f2,

        FOREST_USER_RID_MAX = 0x000001F3,

        DOMAIN_USER_RID_ADMIN = 0x000001F4,
        DOMAIN_USER_RID_GUEST = 0x000001F5,
        DOMAIN_USER_RID_KRBTGT = 0x000001F6,

        DOMAIN_USER_RID_MAX = 0x000003E7,

        DOMAIN_GROUP_RID_ADMINS = 0x00000200,
        DOMAIN_GROUP_RID_USERS = 0x00000201,
        DOMAIN_GROUP_RID_GUESTS = 0x00000202,
        DOMAIN_GROUP_RID_COMPUTERS = 0x00000203,
        DOMAIN_GROUP_RID_CONTROLLERS = 0x00000204,
        DOMAIN_GROUP_RID_CERT_ADMINS = 0x00000205,
        DOMAIN_GROUP_RID_SCHEMA_ADMINS = 0x00000206,
        DOMAIN_GROUP_RID_ENTERPRISE_ADMINS = 0x00000207,
        DOMAIN_GROUP_RID_POLICY_ADMINS = 0x00000208,
        DOMAIN_GROUP_RID_READONLY_CONTROLLERS = 0x00000209,
        DOMAIN_GROUP_RID_CLONEABLE_CONTROLLERS = 0x0000020a,

        DOMAIN_ALIAS_RID_ADMINS = 0x00000220,
        DOMAIN_ALIAS_RID_USERS = 0x00000221,
        DOMAIN_ALIAS_RID_GUESTS = 0x00000222,
        DOMAIN_ALIAS_RID_POWER_USERS = 0x00000223,

        DOMAIN_ALIAS_RID_ACCOUNT_OPS = 0x00000224,
        DOMAIN_ALIAS_RID_SYSTEM_OPS = 0x00000225,
        DOMAIN_ALIAS_RID_PRINT_OPS = 0x00000226,
        DOMAIN_ALIAS_RID_BACKUP_OPS = 0x00000227,

        DOMAIN_ALIAS_RID_REPLICATOR = 0x00000228,
        DOMAIN_ALIAS_RID_RAS_SERVERS = 0x00000229,
        DOMAIN_ALIAS_RID_PREW2KCOMPACCESS = 0x0000022A,
        DOMAIN_ALIAS_RID_REMOTE_DESKTOP_USERS = 0x0000022B,
        DOMAIN_ALIAS_RID_NETWORK_CONFIGURATION_OPS = 0x0000022C,
        DOMAIN_ALIAS_RID_INCOMING_FOREST_TRUST_BUILDERS = 0x0000022D,

        DOMAIN_ALIAS_RID_MONITORING_USERS = 0x0000022E,
        DOMAIN_ALIAS_RID_LOGGING_USERS = 0x0000022F,
        DOMAIN_ALIAS_RID_AUTHORIZATIONACCESS = 0x00000230,
        DOMAIN_ALIAS_RID_TS_LICENSE_SERVERS = 0x00000231,
        DOMAIN_ALIAS_RID_DCOM_USERS = 0x00000232,

        DOMAIN_ALIAS_RID_IUSERS = 0x00000238,
        DOMAIN_ALIAS_RID_CRYPTO_OPERATORS = 0x00000239,
        DOMAIN_ALIAS_RID_CACHEABLE_PRINCIPALS_GROUP = 0x0000023B,
        DOMAIN_ALIAS_RID_NON_CACHEABLE_PRINCIPALS_GROUP = 0x0000023C,
        DOMAIN_ALIAS_RID_EVENT_LOG_READERS_GROUP = 0x0000023D,
        DOMAIN_ALIAS_RID_CERTSVC_DCOM_ACCESS_GROUP = 0x0000023e,
        DOMAIN_ALIAS_RID_RDS_REMOTE_ACCESS_SERVERS = 0x0000023f,
        DOMAIN_ALIAS_RID_RDS_ENDPOINT_SERVERS = 0x00000240,
        DOMAIN_ALIAS_RID_RDS_MANAGEMENT_SERVERS = 0x00000241,
        DOMAIN_ALIAS_RID_HYPER_V_ADMINS = 0x00000242,
        DOMAIN_ALIAS_RID_ACCESS_CONTROL_ASSISTANCE_OPS = 0x00000243,
        DOMAIN_ALIAS_RID_REMOTE_MANAGEMENT_USERS = 0x00000244,
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
        [MarshalAs(UnmanagedType.LPArray, SizeConst = 1)]
        public SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX[] Handles;
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
        [MarshalAs(UnmanagedType.LPArray, SizeConst = 1)]
        public RTL_PROCESS_MODULE_INFORMATION[] Modules;
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

    [StructLayout(LayoutKind.Sequential)]
    internal struct SYSTEM_POOLTAG_INFORMATION
    {
        public uint Count;
        [MarshalAs(UnmanagedType.LPArray, SizeConst = 1)]
        public SYSTEM_POOLTAG[] TagInfo;
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

    [StructLayout(LayoutKind.Sequential)]
    internal struct ExceptionPointers
    {
        public IntPtr ExceptionRecord;
        public IntPtr ContextRecord;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct ExceptionRecord
    {
        public uint ExceptionCode;
        public uint ExceptionFlags;
        public IntPtr ExceptionRecordPointer;
        public IntPtr ExceptionAddress;
        public uint NumberParameters;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
        public IntPtr[] ExceptionInformation;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct SECURITY_DESCRIPTOR
    {
        public byte Revision;
        public byte Sbz1;
        public ushort Control;
        public IntPtr Owner;
        public IntPtr Group;
        public IntPtr Sacl;
        public IntPtr Dacl;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct ACE_HEADER
    {
        public byte AceType;
        public byte AceFlags;
        public ushort AceSize;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct ACCESS_ALLOWED_ACE
    {
        public ACE_HEADER Header;
        public AccessMask Mask;
        public uint SidStart;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct SID
    {
        public byte Revision;
        public byte SubAuthorityCount;
        public Windows.Win32.Security.SID_IDENTIFIER_AUTHORITY IdentifierAuthority;
        public IntPtr SubAuthority; // uint[ANYSIZE_ARRAY]
    }
}
