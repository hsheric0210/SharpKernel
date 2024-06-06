using System;
using System.Collections.Generic;
using System.Text;

namespace SharpKernelLib.Utils
{
    public static class PointerArithmeticExtension
    {
        public static IntPtr Add(this IntPtr ptr, IntPtr offset) => new IntPtr(ptr.ToInt64() + offset.ToInt64());
        public static IntPtr Add(this IntPtr ptr, UIntPtr offset) => new IntPtr((long)((ulong)ptr.ToInt64() + offset.ToUInt64()));
        public static IntPtr Add(this IntPtr ptr, uint offset) => new IntPtr(ptr.ToInt64() + offset);
        public static IntPtr Add(this IntPtr ptr, long offset) => new IntPtr(ptr.ToInt64() + offset);
        public static IntPtr Add(this IntPtr ptr, ulong offset) => new IntPtr((long)((ulong)ptr.ToInt64() + offset));

        public static UIntPtr Add(this UIntPtr ptr, UIntPtr offset) => new UIntPtr(ptr.ToUInt64() + offset.ToUInt64());
        public static UIntPtr Add(this UIntPtr ptr, IntPtr offset) => new UIntPtr(ptr.ToUInt64() + (ulong)offset.ToInt64());
        public static UIntPtr Add(this UIntPtr ptr, uint offset) => new UIntPtr(ptr.ToUInt64() + offset);
        public static UIntPtr Add(this UIntPtr ptr, long offset) => new UIntPtr(ptr.ToUInt64() + (ulong)offset);
        public static UIntPtr Add(this UIntPtr ptr, ulong offset) => new UIntPtr(ptr.ToUInt64() + offset);
    }
}
