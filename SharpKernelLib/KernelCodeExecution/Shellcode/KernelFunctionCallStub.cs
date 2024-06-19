using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SharpKernelLib.KernelCodeExecution.Shellcode
{
    /// <summary>
    /// Generates a simple function caller stub shellcode.
    /// The shellcode accepts a paramete, which contains pointer to function parameter struct.
    /// You should specify the parameter types to generate shellcode.
    /// </summary>
    /// <remarks>
    /// Only integer and pointer parameters are supported: [ bool, byte, char, short, ushort, int, uint, long, ulong, IntPtr(nint), UIntPtr(nuint), pointer(e.g. void*) ]
    /// <example>
    /// It generates the shellcode this when you specified the parameters [ byte, short, int, long ]:
    /// <code>
    /// typedef struct _PARAMS
    /// {
    ///     char a; // byte
    ///     short b; // short
    ///     int c; // int
    ///     long d; // long
    /// }
    /// PARAMS;
    /// int __stdcall t3_f(PARAMS *p)
    /// {
    ///     return <function>(p->a, p->b, p->c, p->d);
    /// }
    /// </code>
    /// </example>
    /// </remarks>
    public class KernelFunctionCallStub
    {
        private readonly byte[] shellCode;
        private static readonly byte[] parameterRegisterOperand;

        static KernelFunctionCallStub()
        {
            parameterRegisterOperand = new byte[]
            {
                0x4F, // RCX (RDI relative)
                0x57, // RDX (RDI relative)
                0x47, // (REX.R) R8 (RDI relative)
                0x4F, // (REX.R) R9 (RDI relative)
            };
        }

        private IEnumerable<byte> GenParameterRegisterPush(ParamType paramType, int parameterIndex, int structOffset)
        {
            var ops = new List<byte>();

            var prefix = (byte)(0x40 /* REX */ + (parameterIndex >= 2 ? 0x4 : 0x0)); // REX.R Prefix to access R8, R9
            if (paramType == ParamType.I8) // Wide target -> enable REX.W
                prefix += 0x8; // REX.WR or REX.W

            if (prefix != 0x40)
                ops.Add(prefix);

            switch (paramType)
            {
                case ParamType.I1:
                    // MOVZX byte ptr
                    ops.AddRange(new byte[] { 0x0F, 0xB6 });
                    break;
                case ParamType.I2:
                    // MOVZX word ptr
                    ops.AddRange(new byte[] { 0x0F, 0xB7 });
                    break;
                case ParamType.I4:
                case ParamType.I8:
                    // MOV dword(qword if REX.W) ptr
                    ops.Add(0x8B);
                    break;
            }

            ops.Add(parameterRegisterOperand[parameterIndex]); // RCX, RDX, R8, R9
            ops.Add((byte)structOffset);

            return ops;
        }

        private IEnumerable<byte> GenParameterStackPush(ParamType paramType, int stackOffset, int structOffset)
        {
            var ops = new List<byte>();

            switch (paramType) // Opcode select
            {
                case ParamType.I1:
                    ops.AddRange(new byte[] { 0x0F, 0xB6 }); // MOVZX (8-bit)
                    break;
                case ParamType.I2:
                    ops.AddRange(new byte[] { 0x0F, 0xB7 }); // MOVZX (16-bit)
                    break;
                case ParamType.I4:
                    ops.Add(0x8B); // MOV (32-bit)
                    break;
                case ParamType.I8:
                    ops.AddRange(new byte[] { 0x48, 0x8B }); // (REX.W) MOV (64-bit)
                    break;
            }

            ops.Add(0x47); // RDI
            ops.Add((byte)structOffset);

            switch (paramType) // Opcode select
            {
                case ParamType.I1:
                    ops.Add(0x88); // MOV (8-bit)
                    break;
                case ParamType.I2:
                    ops.AddRange(new byte[] { 0x66, 0x89 }); // MOV (16-bit)
                    break;
                case ParamType.I4:
                    ops.Add(0x89); // MOV (32-bit)
                    break;
                case ParamType.I8:
                    ops.AddRange(new byte[] { 0x48, 0x89 }); // (REX.W) MOV (64-bit)
                    break;
            }
            ops.Add(0x44); // RAX

            ops.AddRange(new byte[] { 0x24, (byte)stackOffset });

            return ops;
        }

        private int GetStructOffsetIncrement(ParamType paramType, int align)
        {
            var offset = 0;
            switch (paramType)
            {
                case ParamType.I1:
                    offset = 1;
                    break;
                case ParamType.I2:
                    offset = 2;
                    break;
                case ParamType.I4:
                    offset = 4;
                    break;
                case ParamType.I8:
                    offset = 8;
                    break;
            }

            return Math.Max(offset, align);
        }

        public KernelFunctionCallStub(IntPtr functionAddress, Type[] parameters, Type returnType, int structAlignmentBytes = 16)
        {
            var align = structAlignmentBytes / 8;

            var shellcode = new List<byte>();

            // Stack initialization: allocate space for parameters (8 bytes per parameter)
            const int returnAddrSize = 8;
            var stackSize = parameters.Length * 8 + returnAddrSize; // +1 for the return address
            const int stackAlign = 16; // Align stack in 16-byte
            stackSize += (stackAlign - (stackSize - returnAddrSize) % stackAlign) % stackAlign;
            shellcode.AddRange(new byte[] { 0x48, 0x81, 0xEC });
            shellcode.AddRange(BitConverter.GetBytes(stackSize));

            // Backup the parameter structure pointer from RCX to RDI
            shellcode.AddRange(new byte[] { 0x48, 0x89, 0xCF }); // mov rdi, rcx

            // Load the function pointer into RAX
            shellcode.AddRange(new byte[] { 0x48, 0xB8 });
            shellcode.AddRange(BitConverter.GetBytes(functionAddress.ToInt64()));

            // Backup the function pointer from RAX to R11
            shellcode.AddRange(new byte[] { 0x49, 0x89, 0xC3 });

            // Load parameters into registers or push onto stack if necessary
            var structOffset = 0;
            var stackOffset = 0x20;
            var paramPush = new Stack<IEnumerable<byte>>();
            for (var i = 0; i < parameters.Length; i++)
            {
                var paramType = MapParameterType(parameters[i]);
                // Compute offset based on stack position
                if (i < 4)
                {
                    paramPush.Push(GenParameterRegisterPush(paramType, i, structOffset));
                }
                else
                {
                    // Parameters beyond the fourth go onto the stack at [RSP+stackOffset]
                    paramPush.Push(GenParameterStackPush(paramType, stackOffset, structOffset));
                    stackOffset += 8; // next parameter stack
                }
                structOffset += GetStructOffsetIncrement(paramType, align);
            }

            while (paramPush.Count > 0)
                shellcode.AddRange(paramPush.Pop());

            // Restore the function pointer to RAX from R11
            shellcode.AddRange(new byte[] { 0x4C, 0x89, 0xD8 }); // mov rax, r11

            // Call the function
            shellcode.AddRange(new byte[] { 0xFF, 0xD0 }); // call rax

            switch (MapParameterType(returnType))
            {
                case ParamType.I1:
                    shellcode.AddRange(new byte[] { 0x0F, 0xB6, 0xC0 }); // movzx eax, al
                    break;
                case ParamType.I2:
                    shellcode.AddRange(new byte[] { 0x0F, 0xB7, 0xC0 }); // movzx eax, ax
                    break;
                case ParamType.I4:
                    // No conversion needed, as lower 32 bits of RAX are already in EAX
                    break;
                case ParamType.I8:
                    // No conversion needed, as full 64 bits of RAX are used
                    break;
            }

            // Restore the stack pointer if stack space was allocated
            shellcode.AddRange(new byte[] { 0x48, 0x81, 0xC4 });
            shellcode.AddRange(BitConverter.GetBytes(stackSize));

            // Return
            shellcode.Add(0xC3); // ret

            shellCode = shellcode.ToArray();
        }

        private static ParamType MapParameterType(Type type)
        {
            if (type == typeof(bool) || type == typeof(byte))
                return ParamType.I1;

            if (type == typeof(char) || type == typeof(short) || type == typeof(ushort))
                return ParamType.I2;

            if (type == typeof(int) || type == typeof(uint))
                return ParamType.I4;

            if (type == typeof(long) || type == typeof(ulong) || type == typeof(IntPtr) || type == typeof(UIntPtr) || type.IsPointer) // Assume it's x64
                return ParamType.I8;

            throw new NotSupportedException($"Unsupported parameter type: {type}");
        }

        public byte[] GetShellcode() => shellCode;

        public enum ParamType
        {
            I1,
            I2,
            I4,
            I8,
        }
    }
}
