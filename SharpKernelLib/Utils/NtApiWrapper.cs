using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using static SharpKernelLib.Utils.NtDefines;
using static SharpKernelLib.Utils.NtApiCalls;

namespace SharpKernelLib.Utils
{
    // sup.c / sup.h
    internal class NtApiWrapper
    {
        internal static bool QueryHVCIState(out bool hvciEnabled, out bool hvciStrict, out bool hvciIUM)
        {
            hvciEnabled = false;
            hvciStrict = false;
            hvciIUM = false;

            var ciInfo = new SYSTEM_CODEINTEGRITY_INFORMATION();
            ciInfo.Length = (uint)Marshal.SizeOf(ciInfo);

            if (!NT_SUCCESS(NtQuerySystemInformation(SystemCodeIntegrityInformation, ref ciInfo, Marshal.SizeOf(ciInfo), out _)))
                return false;

            hvciEnabled = ciInfo.CodeIntegrityOptions.HasFlag(CodeIntegrityOptions.Enabled);
            hvciStrict = ciInfo.CodeIntegrityOptions.HasFlag(CodeIntegrityOptions.StrictMode);
            hvciIUM = ciInfo.CodeIntegrityOptions.HasFlag(CodeIntegrityOptions.IUM);

            return true;
        }
    }
}
