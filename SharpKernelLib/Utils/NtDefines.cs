using System;
using System.Runtime.InteropServices;

namespace SharpKernelLib.Utils
{
    internal static class NtDefines
    {
        internal const int SystemCodeIntegrityInformation = 103; // SYSTEM_INFORMATION_CLASS enumeration for Code Integrity Information

        [Flags]
        internal enum CodeIntegrityOptions : uint
        {
            None = 0,
            Enabled = 0x01,
            StrictMode = 0x1000,
            IUM = 0x2000,
        }

        internal static bool NT_SUCCESS(int ntStatus) => ntStatus >= 0;

        [StructLayout(LayoutKind.Sequential)]
        internal struct SYSTEM_CODEINTEGRITY_INFORMATION
        {
            public uint Length;
            public CodeIntegrityOptions CodeIntegrityOptions;
        }
    }
}
