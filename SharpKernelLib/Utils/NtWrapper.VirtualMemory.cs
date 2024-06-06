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
    }
}
