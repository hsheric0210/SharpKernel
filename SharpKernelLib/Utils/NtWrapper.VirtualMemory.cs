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

namespace SharpKernelLib.Utils
{
    // Port of KDU Hamakaze sup.c and sup.h
    internal unsafe static partial class NtWrapper
    {
        internal static uint ChooseNonPagedPoolTag()
        {
            var info = (SYSTEM_POOLTAG_INFORMATION*)GetNtSystemInfo(SystemInformationClass.SystemPoolTagInformation, out _);
            var tag = 0x20206f49u; // '  oI'
            var maxUse = 0ul;

            try
            {
                for (uint i = 0, j = info->Count; i < j; i++)
                {
                    var pool = info->TagInfo[i];
                    if (pool.NonPagedUsed.ToUInt64() > maxUse)
                    {
                        maxUse = pool.NonPagedUsed.ToUInt64();
                        tag = pool.Tag;
                    }
                }
            }
            finally
            {
                Marshal.FreeHGlobal((IntPtr)info);
            }

            return tag;
        }
    }
}
