using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

using static SharpKernelLib.Utils.NtDefines;

namespace SharpKernelLib.Utils
{
    internal class NtApiCalls
    {
        [DllImport("ntdll.dll")]
        internal static extern int NtQuerySystemInformation(
            int SystemInformationClass,
            ref SYSTEM_CODEINTEGRITY_INFORMATION SystemInformation,
            int SystemInformationLength,
            out int ReturnLength);
    }
}
