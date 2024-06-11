using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Windows.Win32.Foundation;

namespace SharpKernelLib.Utils
{
    internal enum ImageDataDirectory
    {
        IMAGE_DIRECTORY_ENTRY_EXPORT = 0,
        IMAGE_DIRECTORY_ENTRY_IMPORT = 1,
        IMAGE_DIRECTORY_ENTRY_RESOURCE = 2,
        IMAGE_DIRECTORY_ENTRY_EXCEPTION = 3,
        IMAGE_DIRECTORY_ENTRY_SECURITY = 4,
        IMAGE_DIRECTORY_ENTRY_BASERELOC = 5,
        IMAGE_DIRECTORY_ENTRY_DEBUG = 6,
        IMAGE_DIRECTORY_ENTRY_ARCHITECTURE = 7,
        IMAGE_DIRECTORY_ENTRY_GLOBALPTR = 8,
        IMAGE_DIRECTORY_ENTRY_TLS = 9,
        IMAGE_DIRECTORY_ENTRY_LOAD_CONFIG = 10,
        IMAGE_DIRECTORY_ENTRY_BOUND_IMPORT = 11,
        IMAGE_DIRECTORY_ENTRY_IAT = 12,
        IMAGE_DIRECTORY_ENTRY_DELAY_IMPORT = 13,
        IMAGE_DIRECTORY_ENTRY_COM_DESCRIPTOR = 14,
    }

    [Flags]
    internal enum ImageFileCharacteristics : ushort
    {
        IMAGE_FILE_RELOCS_STRIPPED = 0x0001,
        IMAGE_FILE_EXECUTABLE_IMAGE = 0x0002,
        IMAGE_FILE_LINE_NUMS_STRIPPED = 0x0004,
        IMAGE_FILE_LOCAL_SYMS_STRIPPED = 0x0008,
        IMAGE_FILE_AGGRESIVE_WS_TRIM = 0x0010,
        IMAGE_FILE_LARGE_ADDRESS_AWARE = 0x0020,
        IMAGE_FILE_BYTES_REVERSED_LO = 0x0080,
        IMAGE_FILE_32BIT_MACHINE = 0x0100,
        IMAGE_FILE_DEBUG_STRIPPED = 0x0200,
        IMAGE_FILE_REMOVABLE_RUN_FROM_SWAP = 0x0400,
        IMAGE_FILE_NET_RUN_FROM_SWAP = 0x0800,
        IMAGE_FILE_SYSTEM = 0x1000,
        IMAGE_FILE_DLL = 0x2000,
        IMAGE_FILE_UP_SYSTEM_ONLY = 0x4000,
        IMAGE_FILE_BYTES_REVERSED_HI = 0x8000,
    }

    internal enum ImageFileMachine : ushort
    {
        IMAGE_FILE_MACHINE_UNKNOWN = 0,
        IMAGE_FILE_MACHINE_I386 = 0x014c,
        IMAGE_FILE_MACHINE_R3000 = 0x0162,
        IMAGE_FILE_MACHINE_R4000 = 0x0166,
        IMAGE_FILE_MACHINE_R10000 = 0x0168,
        IMAGE_FILE_MACHINE_WCEMIPSV2 = 0x0169,
        IMAGE_FILE_MACHINE_ALPHA = 0x0184,
        IMAGE_FILE_MACHINE_SH3 = 0x01a2,
        IMAGE_FILE_MACHINE_SH3DSP = 0x01a3,
        IMAGE_FILE_MACHINE_SH3E = 0x01a4,
        IMAGE_FILE_MACHINE_SH4 = 0x01a6,
        IMAGE_FILE_MACHINE_SH5 = 0x01a8,
        IMAGE_FILE_MACHINE_ARM = 0x01c0,
        IMAGE_FILE_MACHINE_ARMV7 = 0x01c4,
        IMAGE_FILE_MACHINE_ARMNT = 0x01c4,
        IMAGE_FILE_MACHINE_ARM64 = 0xaa64,
        IMAGE_FILE_MACHINE_THUMB = 0x01c2,
        IMAGE_FILE_MACHINE_AM33 = 0x01d3,
        IMAGE_FILE_MACHINE_POWERPC = 0x01F0,
        IMAGE_FILE_MACHINE_POWERPCFP = 0x01f1,
        IMAGE_FILE_MACHINE_IA64 = 0x0200,
        IMAGE_FILE_MACHINE_MIPS16 = 0x0266,
        IMAGE_FILE_MACHINE_ALPHA64 = 0x0284,
        IMAGE_FILE_MACHINE_MIPSFPU = 0x0366,
        IMAGE_FILE_MACHINE_MIPSFPU16 = 0x0466,
        IMAGE_FILE_MACHINE_AXP64 = IMAGE_FILE_MACHINE_ALPHA64,
        IMAGE_FILE_MACHINE_TRICORE = 0x0520,
        IMAGE_FILE_MACHINE_CEF = 0x0CEF,
        IMAGE_FILE_MACHINE_EBC = 0x0EBC,
        IMAGE_FILE_MACHINE_AMD64 = 0x8664,
        IMAGE_FILE_MACHINE_M32R = 0x9041,
        IMAGE_FILE_MACHINE_CEE = 0xc0ee,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct IMAGE_DOS_HEADER
    {
        public ushort e_magic; // Magic number
        public ushort e_cblp; // Bytes on last page of file
        public ushort e_cp; // Pages in file
        public ushort e_crlc; // Relocations
        public ushort e_cparhdr; // Size of header in paragraphs
        public ushort e_minalloc; // Minimum extra paragraphs needed
        public ushort e_maxalloc; // Maximum extra paragraphs needed
        public ushort e_ss; // Initial (relative) SS value
        public ushort e_sp; // Initial SP value
        public ushort e_csum; // Checksum
        public ushort e_ip; // Initial IP value
        public ushort e_cs; // Initial (relative) CS value
        public ushort e_lfarlc; // File address of relocation table
        public ushort e_ovno; // Overlay number
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public ushort[] e_res1; // Reserved words
        public ushort e_oemid; // OEM identifier (for e_oeminfo)
        public ushort e_oeminfo; // OEM information; e_oemid specific
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public ushort[] e_res2; // Reserved words
        public int e_lfanew; // File address of new exe header
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct IMAGE_NT_HEADERS
    {
        public uint Signature;
        public IMAGE_FILE_HEADER FileHeader;
        public IMAGE_OPTIONAL_HEADER64 OptionalHeader; // Only support x64 files
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct IMAGE_FILE_HEADER
    {
        public ImageFileMachine Machine;
        public ushort NumberOfSections;
        public uint TimeDateStamp;
        public uint PointerToSymbolTable;
        public uint NumberOfSymbols;
        public ushort SizeOfOptionalHeader;
        public ImageFileCharacteristics Characteristics;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct IMAGE_OPTIONAL_HEADER64
    {
        public ushort Magic;
        public byte MajorLinkerVersion;
        public byte MinorLinkerVersion;
        public uint SizeOfCode;
        public uint SizeOfInitializedData;
        public uint SizeOfUninitializedData;
        public uint AddressOfEntryPoint;
        public uint BaseOfCode;
        public ulong ImageBase;
        public uint SectionAlignment;
        public uint FileAlignment;
        public ushort MajorOperatingSystemVersion;
        public ushort MinorOperatingSystemVersion;
        public ushort MajorImageVersion;
        public ushort MinorImageVersion;
        public ushort MajorSubsystemVersion;
        public ushort MinorSubsystemVersion;
        public uint Win32VersionValue;
        public uint SizeOfImage;
        public uint SizeOfHeaders;
        public uint CheckSum;
        public ushort Subsystem;
        public ushort DllCharacteristics;
        public ulong SizeOfStackReserve;
        public ulong SizeOfStackCommit;
        public ulong SizeOfHeapReserve;
        public ulong SizeOfHeapCommit;
        public uint LoaderFlags;
        public uint NumberOfRvaAndSizes;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public IMAGE_DATA_DIRECTORY[] DataDirectory;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct IMAGE_DATA_DIRECTORY
    {
        public uint VirtualAddress;
        public uint Size;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct IMAGE_EXPORT_DIRECTORY
    {
        public uint Characteristics;
        public uint TimeDateStamp;
        public ushort MajorVersion;
        public ushort MinorVersion;
        public uint Name;
        public uint Base;
        public uint NumberOfFunctions;
        public uint NumberOfNames;
        public uint AddressOfFunctions;
        public uint AddressOfNames;
        public uint AddressOfNameOrdinals;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct IMAGE_BASE_RELOCATION
    {
        public uint VirtualAddress;
        public uint SizeOfBlock;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct IMAGE_SECTION_HEADER
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] Name;
        public uint PhysicalAddress;
        public uint VirtualAddress;
        public uint SizeOfRawData;
        public uint PointerToRawData;
        public uint PointerToRelocations;
        public uint PointerToLinenumbers;
        public ushort NumberOfRelocations;
        public ushort NumberOfLinenumbers;
        public uint Characteristics;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct LDR_DATA_TABLE_ENTRY
    {
        public LIST_ENTRY InLoadOrderLinks;
        public LIST_ENTRY InMemoryOrderLinks;
        public LIST_ENTRY InInitializationOrderLinks; // InProgressLinks
        public IntPtr DllBase;
        public IntPtr EntryPoint;
        public uint SizeOfImage;
        public UNICODE_STRING FullDllName;
        public UNICODE_STRING BaseDllName;
        public uint Flags;
        public ushort ObsoleteLoadCount;
        public ushort TlsIndex;
        public IntPtr SectionPointer;
        public uint CheckSum;
        public uint TimeDateStamp;
        //fields below removed for compatibility, if you need them use LDR_DATA_TABLE_ENTRY_FULL
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct LIST_ENTRY
    {
        public IntPtr Flink;
        public IntPtr Blink;
    }
}
