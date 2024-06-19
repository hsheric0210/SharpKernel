using System;
using System.Collections.Generic;
using System.Text;

namespace SharpKernelLib.KernelCodeExecution.Shellcode
{
    public class AccessMSR : IShellcode
    {
        private static AccessMSR ReadMSRInstance;
        private static AccessMSR WriteMSRInstance;

        private byte[] shellCode;
        private AccessMSR(byte[] shellCode) => this.shellCode = shellCode;

        public byte[] GetShellcode() => shellCode;

        /// <summary>
        /// <code>
        /// u64 shellcode(u32 msr)
        /// {
        ///     return __readmsr(msr);
        /// }
        /// </code>
        /// </summary>
        public static AccessMSR ReadMSR()
        {
            if (ReadMSRInstance != null)
                return ReadMSRInstance;

            var code = new List<byte>();
            code.AddRange(new byte[] {
                0x0F, 0x32, // RDMSR
                0x48, 0x4C, 0x24, 0x08, // SHL RDX, 0x20
                0x48, 0x0B, 0xC2, // OR RAX, RDX
                0xC3 // RET
            });

            return ReadMSRInstance = new AccessMSR(code.ToArray());
        }

        /// <summary>
        /// <code>
        /// void shellcode(u32 msr, u64 val)
        /// {
        ///     return __writemsr(msr, val);
        /// }
        /// </code>
        /// </summary>
        public static AccessMSR WriteMSR()
        {
            if (WriteMSRInstance != null)
                return WriteMSRInstance;

            var code = new List<byte>();
            code.AddRange(new byte[] {
                0x48, 0x8B, 0xC2, // MOV RAX, RDX
                0x48, 0xC1, 0xEA, 0x20, // SHR RDX, 0x20
                0x0F, 0x30, // WRMSR
                0xC3 // RET
            });

            return WriteMSRInstance = new AccessMSR(code.ToArray());
        }
    }
}
