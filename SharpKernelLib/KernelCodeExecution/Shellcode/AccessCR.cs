using System;
using System.Collections.Generic;
using System.Text;

namespace SharpKernelLib.KernelCodeExecution.Shellcode
{
    public class AccessCR : IShellcode
    {
        private byte[] shellCode;
        private AccessCR(byte[] shellCode) => this.shellCode = shellCode;

        public byte[] GetShellcode() => shellCode;

        /// <summary>
        /// <code>
        /// u64 shellcode()
        /// {
        ///     return __readcrN();
        /// }
        /// </code>
        /// </summary>
        public static AccessCR ReadCR(int cr)
        {
            var code = new List<byte>();
            if (cr == 8)
                code.Add(0x44); // REX.R prefix for CR8 access (instead of CR0)
            code.AddRange(new byte[] { 0x0F, 0X20 }); // MOV RAX, CR#

            switch (cr)
            {
                case 0:
                case 8:
                    code.Add(0xC0); // CR0 or CR8(REX.R)
                    break;
                case 2:
                    code.Add(0xD0); // CR2
                    break;
                case 3:
                    code.Add(0xD8); // CR3
                    break;
                case 4:
                    code.Add(0xE0); // CR4
                    break;
                default:
                    throw new ArgumentOutOfRangeException("cr", $"Control Register out of range: {cr}");
            }

            code.Add(0xC3); // RET

            return new AccessCR(code.ToArray());
        }

        /// <summary>
        /// <code>
        /// void shellcode(u64 val)
        /// {
        ///     return __writecrN(val);
        /// }
        /// </code>
        /// </summary>
        public static AccessCR WriteCR(int cr)
        {
            var code = new List<byte>();
            if (cr == 8)
                code.Add(0x44); // REX.R prefix for CR8 access (instead of CR0)
            code.AddRange(new byte[] { 0x0F, 0X22 }); // MOV CR#, RCX

            switch (cr)
            {
                case 0:
                case 8:
                    code.Add(0xC1); // CR0 or CR8(REX.R)
                    break;
                case 2:
                    code.Add(0xD1); // CR2
                    break;
                case 3:
                    code.Add(0xD9); // CR3
                    break;
                case 4:
                    code.Add(0xE1); // CR4
                    break;
                default:
                    throw new ArgumentOutOfRangeException("cr", $"Control Register out of range: {cr}");
            }

            code.Add(0xC3); // RET

            return new AccessCR(code.ToArray());
        }
    }
}
