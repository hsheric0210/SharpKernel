using SharpKernelLib.Exception;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

using static SharpKernelLib.Utils.NtWrapper;

namespace SharpKernelLib.Utils
{
    public sealed unsafe class PageWalkVirtualToPhysical
    {
        public PageWalkVirtualToPhysical(IntPtr pml4Value)
        {

        }

        public static PageWalkVirtualToPhysical FindPML4AndCreate(IntPtr lowStub1M)
        {
            var offset = 0;
            var lmTargetOffset = Marshal.OffsetOf<PROCESSOR_START_BLOCK>("LmTarget");
            var cr3Offset = Marshal.OffsetOf<PROCESSOR_START_BLOCK>("ProcessorState").Add(Marshal.OffsetOf<KSPECIAL_REGISTERS>("Cr3"));

            var cr3Value = IntPtr.Zero;

            // Prevent access violation crash
            VEHTryCatch(
                () =>
                {
                    // TODO: parallel search using 'Parallel.For'
                    while (offset < 0x100000) // 1 MiB limit
                    {
                        offset += (int)PAGE_SIZE;

                        // PROCESSOR_START_BLOCK->Jmp
                        var jmp = *(ulong*)(lowStub1M + offset);
                        if ((jmp & 0xffffffffffff00ff) != 0x00000001000600E9)
                            continue;

                        // PROCESSOR_START_BLOCK->LmTarget
                        var lmTarget = *(ulong*)(lowStub1M + offset).Add(lmTargetOffset);
                        if ((lmTarget & 0xfffff80000000003) != 0xfffff80000000000)
                            continue;

                        // PROCESSOR_START_BLOCK->ProcessorState->Cr3
                        var cr3 = *(ulong*)(lowStub1M + offset).Add(cr3Offset);
                        if ((cr3 & 0xffffff0000000fff) != 0)
                            continue;

                        cr3Value = new IntPtr((long)cr3);
                    }
                },
                (ExceptionRecord _) =>
                {
                    // ignore
                }
            );

            if (cr3Value == IntPtr.Zero)
                throw new MemoryAccessException("PML4 value not found.");

            return new PageWalkVirtualToPhysical(cr3Value);
        }
    }
}
