<Query Kind="Statements" />


var align = 16 / 8;

var functionAddress = (IntPtr)0xdeadbeef;
var parameters = new Type[] { typeof(byte), typeof(short), typeof(int), typeof(long), typeof(void*), typeof(byte), typeof(short), typeof(int), typeof(long), typeof(void*) };

var shellcode = new List<byte>();

            // Stack initialization: allocate space for parameters (8 bytes per parameter)
            int stackSize = (Math.Max(0, parameters.Length - 4) + 1) * 8;
            if (stackSize > 0)
            {
                shellcode.AddRange(new byte[] { 0x48, 0x81, 0xEC });
                shellcode.AddRange(BitConverter.GetBytes(stackSize));
            }

            // Backup the parameter structure pointer from RCX to RDI
            shellcode.AddRange(new byte[] { 0x48, 0x89, 0xCF }); // mov rdi, rcx

            // Load the function pointer into RAX
            shellcode.AddRange(new byte[] { 0x48, 0xB8 });
            shellcode.AddRange(BitConverter.GetBytes(functionAddress.ToInt64()));

            // Backup the function pointer from RAX to R11
            shellcode.AddRange(new byte[] { 0x49, 0x89, 0xC3 });

            // Load parameters into registers or push onto stack if necessary
            var structOffset = 0;
			var stackOffset = -stackSize-(stackSize % 16 == 0 ? 0 : (16 - stackSize % 16));
            var paramPush = new Stack<byte[]>();
            for (var i = 0; i < parameters.Length; i++)
            {
                var paramType = MapParameterType(parameters[i]);
                // Compute offset based on stack position
                if (i < 4)
                {
                    switch (paramType)
                    {
                        case ParamType.Int8:
                            // movzx reg, byte ptr [rdi+offset]
                            paramPush.Push(new byte[] { 0x0F, 0xB6, GetParameterRegister(i), (byte)structOffset });
                            structOffset += Math.Max(align, 1);
                            break;
                        case ParamType.Int16:
                            // movzx reg, word ptr [rdi+offset]
                            paramPush.Push(new byte[] { 0x0F, 0xB7, GetParameterRegister(i), (byte)structOffset });
                            structOffset += Math.Max(align, 2);
                            break;
                        case ParamType.Int32:
                            // mov reg, dword ptr [rdi+offset]
                            paramPush.Push(new byte[] { 0x44, 0x8B, GetParameterRegister(i), (byte)structOffset });
                            structOffset += Math.Max(align, 4);
                            break;
                        case ParamType.Int64:
                        case ParamType.Pointer:
                            // mov reg, qword ptr [rdi+offset]
                            paramPush.Push(new byte[] { 0x4C, 0x8B, GetParameterRegister(i), (byte)structOffset });
                            structOffset += 8;
                            break;
                    }
                }
                else
                {
                    // Parameters beyond the fourth go onto the stack at [RSP+stackOffset]
		            stackOffset += 8; // Decrement by 8 for the next parameter
		            switch (paramType)
		            {
		                case ParamType.Int8:
		                    // movzx rax, byte ptr [rdi+offset]; mov [rsp+stackOffset], rax
		                    paramPush.Push(new byte[] {
								0x0F, 0xB6, 0x47, (byte)structOffset, // movzx rax, byte ptr [rdi+offset]
								0x48, 0x89, 0x44, 0x24, (byte)stackOffset // mov [rsp+stackOffset], rax
							});
		                    structOffset += Math.Max(align, 1);
		                    break;
		                case ParamType.Int16:
		                    // movzx rax, word ptr [rdi+offset]; mov [rsp+stackOffset], rax
		                    paramPush.Push(new byte[] {
								0x0F, 0xB7, 0x47, (byte)structOffset, // movzx rax, word ptr [rdi+offset]
								0x48, 0x89, 0x44, 0x24, (byte)stackOffset // mov [rsp+stackOffset], rax
							});
		                    structOffset += 2; // 2 bytes
		                    break;
		                case ParamType.Int32:
		                    // mov rax, dword ptr [rdi+offset]; mov [rsp+stackOffset], rax
		                    paramPush.Push(new byte[] {
								0x8B, 0x47, (byte)structOffset, // mov rax, dword ptr [rdi+offset]
								0x48, 0x89, 0x44, 0x24, (byte)stackOffset // mov [rsp+stackOffset], rax
							});
		                    structOffset += 4; // 4 bytes
		                    break;
		                case ParamType.Int64:
		                case ParamType.Pointer:
		                    // mov rax, qword ptr [rdi+offset]; mov [rsp+stackOffset], rax
		                    paramPush.Push(new byte[] {
								0x48, 0x8B, 0x47, (byte)structOffset, // mov rax, qword ptr [rdi+offset]
								0x48, 0x89, 0x44, 0x24, (byte)stackOffset // mov [rsp+stackOffset], rax
							});
		                    structOffset += 8; // 8 bytes
		                    break;
		            }
                }
            }
			
			while (paramPush.Count > 0)
				shellcode.AddRange(paramPush.Pop());

            // Restore the function pointer to RAX from R11
            shellcode.AddRange(new byte[] { 0x4C, 0x89, 0xD8 }); // mov rax, r11

            // Call the function
            shellcode.AddRange(new byte[] { 0xFF, 0xD0 }); // call rax

            // Restore the stack pointer if stack space was allocated
            if (stackSize > 0)
            {
                shellcode.AddRange(new byte[] { 0x48, 0x81, 0xC4 });
                shellcode.AddRange(BitConverter.GetBytes(stackSize));
            }

            // Return
            shellcode.Add(0xC3); // ret

var shellCode = shellcode.Select(t=>$"{t:X2}").ToArray();
Console.WriteLine(string.Join(" ", shellCode));

static byte GetParameterRegister(int i)
{
	switch(i)
	{
	case 0:
		return 0x4f; // RCX
	case 1:
		return 0x57; // RDX
	case 2:
		return 0x47; // R8
	case 3:
		return 0x4f; // R9
	}

	throw new NotSupportedException();
}

ParamType MapParameterType(Type type)
{
    if (type == typeof(bool) || type == typeof(byte))
        return ParamType.Int8;

    if (type == typeof(char) || type == typeof(short) || type == typeof(ushort))
        return ParamType.Int16;

    if (type == typeof(int) || type == typeof(uint))
        return ParamType.Int32;

    if (type == typeof(long) || type == typeof(ulong))
        return ParamType.Int64;

    if (type == typeof(IntPtr) || type == typeof(UIntPtr) || type.IsPointer)
        return ParamType.Pointer;

    throw new NotSupportedException($"Unsupported parameter type: {type}");
}

public delegate int Del2(byte a, short b, int c, long d, IntPtr e);

public enum ParamType
{
    Int8,
    Int16,
    Int32,
    Int64,
    Pointer
}