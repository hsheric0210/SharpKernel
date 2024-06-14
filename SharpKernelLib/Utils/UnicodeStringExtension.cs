using System;
using System.Collections.Generic;
using System.Text;
using Windows.Win32.Foundation;

namespace SharpKernelLib.Utils
{
    internal static class UnicodeStringExtension
    {
        public static unsafe string ConvertToString(this UNICODE_STRING unicodeString) => new string(unicodeString.Buffer.Value, 0, unicodeString.Length);
    }
}
