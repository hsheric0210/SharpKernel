using System;
using System.Runtime.InteropServices;
using Windows.Wdk.System.SystemInformation;
using Windows.Wdk.Foundation;
using Windows.Win32.Foundation;
using Microsoft.Win32;
using SharpKernelLib.Exception;
using Windows.Win32.Storage.FileSystem;
using Windows.Wdk.Storage.FileSystem;

using static SharpKernelLib.Utils.NtUndocumented;
using static Windows.Win32.PInvoke;
using static Windows.Wdk.PInvoke;
using Windows.Win32.System.IO;
using Windows.Win32.System.Memory;
using System.Linq;

namespace SharpKernelLib.Utils
{
    // Port of KDU Hamakaze sup.c and sup.h
    internal unsafe static partial class NtWrapper
    {
        internal const ulong PAGE_SIZE = 0x1000ul; // Windows Default Page Size: 1024 bytes
        internal const uint SYSTEM_PROCESSID = 4;
        internal const uint ALL_1_UINT = unchecked((uint)-1);
        internal const ulong ALL_1_ULONG = unchecked((ulong)-1L);

        internal static string ConvertToString(this UNICODE_STRING unicodeString) => new string(unicodeString.Buffer.Value, 0, unicodeString.Length);
    }
}
