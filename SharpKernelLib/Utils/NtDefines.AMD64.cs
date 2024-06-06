using System.Runtime.InteropServices;
using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace SharpKernelLib.Utils
{
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    internal struct FAR_JMP_16
    {
        public ushort OpCode; // Always 0xE9
        public ushort Selector;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    internal struct FAR_TARGET_32
    {
        public uint Offset;
        public ushort Selector;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    internal struct PSEUDO_DESCRIPTOR_32
    {
        public ushort Limit;
        public uint Base;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct KGDTENTRY64
    {
        public ushort LimitLow;
        public ushort BaseLow;
        public ushort BaseMiddle;
        public ushort Flags1;
        public ushort Flags2;
        public ushort BaseHigh;
        public uint BaseUpper;
        public uint MustBeZero;
        public ulong Alignment;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct KDESCRIPTOR
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public ushort[] Pad;
        public ushort Limit;
        public uint Base;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct KSPECIAL_REGISTERS
    {
        public ulong Cr0;
        public ulong Cr2;
        public ulong Cr3;
        public ulong Cr4;
        public ulong KernelDr0;
        public ulong KernelDr1;
        public ulong KernelDr2;
        public ulong KernelDr3;
        public ulong KernelDr6;
        public ulong KernelDr7;
        public KDESCRIPTOR Gdtr;
        public KDESCRIPTOR Idtr;
        public ushort Tr;
        public ushort Ldtr;
        public uint MxCsr;
        public ulong DebugControl;
        public ulong LastBranchToRip;
        public ulong LastBranchFromRip;
        public ulong LastExceptionToRip;
        public ulong LastExceptionFromRip;
        public ulong Cr8;
        public ulong MsrGsBase;
        public ulong MsrGsSwap;
        public ulong MsrStar;
        public ulong MsrLStar;
        public ulong MsrCStar;
        public ulong MsrSyscallMask;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct CONTEXT
    {
        public uint ContextFlags;
        public uint R0;
        public uint R1;
        public uint R2;
        public uint R3;
        public uint R4;
        public uint R5;
        public uint R6;
        public uint R7;
        public uint R8;
        public uint R9;
        public uint R10;
        public uint R11;
        public uint R12;
        public uint Sp;
        public uint Lr;
        public uint Pc;
        public uint Cpsr;
        public uint Fpscr;
        public uint Padding;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public ulong[] D;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public uint[] Bvr;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public uint[] Bcr;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
        public uint[] Wvr;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
        public uint[] Wcr;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public uint[] Padding2;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct KPROCESSOR_STATE
    {
        public KSPECIAL_REGISTERS SpecialRegisters;
        public CONTEXT ContextFrame;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct PROCESSOR_START_BLOCK
    {
        public FAR_JMP_16 Jmp;
        public uint CompletionFlag;
        public PSEUDO_DESCRIPTOR_32 Gdt32;
        public PSEUDO_DESCRIPTOR_32 Idt32;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public KGDTENTRY64[] Gdt;
        public ulong TiledCr3;
        public FAR_TARGET_32 PmTarget;
        public FAR_TARGET_32 LmIdentityTarget;
        public IntPtr LmTarget;
        public IntPtr SelfMap;
        public ulong MsrPat;
        public ulong MsrEFER;
        public KPROCESSOR_STATE ProcessorState;
    }
}
