using System;
using System.Collections.Generic;
using System.Text;

using static SharpKernelLib.Utils.NtUndocumented;
using static Windows.Win32.PInvoke;
using static Windows.Wdk.PInvoke;
using Windows.Win32.System.IO;
using Windows.Win32.System.Memory;
using SharpKernelLib.Exception;

namespace SharpKernelLib.Utils
{
    internal sealed unsafe class LockedMemory : IDisposable
    {
        public static readonly LockedMemory Null = new LockedMemory();

        private void* baseAddress;
        private UIntPtr regionSize;
        private bool disposedValue;

        public IntPtr Address => (IntPtr)RawAddress;

        public void* RawAddress => disposedValue ? throw new ObjectDisposedException("baseAddress") : baseAddress;

        public long RegionSize => (long)RawRegionSize.ToUInt64();
        public UIntPtr RawRegionSize => disposedValue ? throw new ObjectDisposedException("regionSize") : regionSize;

        public static LockedMemory Allocate(long size, AllocationTypes allocationType, PageProtections protect)
            => Allocate((UIntPtr)size, allocationType, protect);

        public static LockedMemory Allocate(UIntPtr size, AllocationTypes allocationType, PageProtections protect)
        {
            var buffer = VirtualAllocEx(NtCurrentProcess_SafeHandle(), null, size, (VIRTUAL_ALLOCATION_TYPE)allocationType, (PAGE_PROTECTION_FLAGS)protect);
            if (buffer == null)
                return Null;

            if (!VirtualLock(buffer, size))
            {
                VirtualFreeEx(NtCurrentProcess_SafeHandle(), buffer, UIntPtr.Zero, VIRTUAL_FREE_TYPE.MEM_RELEASE);
                return Null;
            }

            var obj = new LockedMemory
            {
                baseAddress = buffer,
                regionSize = size
            };
            return obj;
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                VirtualUnlock(baseAddress, regionSize);
                VirtualFreeEx(NtCurrentProcess(), baseAddress, regionSize, VIRTUAL_FREE_TYPE.MEM_RELEASE);

                baseAddress = null;
                regionSize = UIntPtr.Zero;

                disposedValue = true;
            }
        }

        ~LockedMemory()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
