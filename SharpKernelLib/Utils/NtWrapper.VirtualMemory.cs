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
        /// <summary>
        /// supAllocateLockedMemory: Wrapper for VirtualAllocEx+VirtualLock.
        /// </summary>
        internal static IntPtr AllocateLockedVM(UIntPtr size, VIRTUAL_ALLOCATION_TYPE allocationType, PAGE_PROTECTION_FLAGS protect)
        {
            var buffer = VirtualAllocEx(NtCurrentProcess_SafeHandle(), null, size, allocationType, protect);
            if (buffer == null)
                return new IntPtr(buffer); // failed

            if (!VirtualLock(buffer, size))
            {
                VirtualFreeEx(NtCurrentProcess_SafeHandle(), buffer, UIntPtr.Zero, VIRTUAL_FREE_TYPE.MEM_RELEASE);
                buffer = null;
            }

            return new IntPtr(buffer);
        }

        /// <summary>
        /// supFreeLockedMemory: Wrapper for VirtualUnlock + VirtualFreeEx.
        /// </summary>
        internal static void FreeLockedVM(IntPtr memory, UIntPtr size)
        {
            if (!VirtualUnlock(memory.ToPointer(), size))
                return;

            VirtualFreeEx(NtCurrentProcess(), memory.ToPointer(), size, VIRTUAL_FREE_TYPE.MEM_RELEASE);
        }

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
