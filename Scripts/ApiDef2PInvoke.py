import re

# Define patterns for various parts of the C function signature
return_type_pattern = re.compile(r'(\w+)\s*$')
function_name_pattern = re.compile(r'NTAPI\s+(\w+)\s*\(')
parameter_pattern = re.compile(r'(\w+)\s*(_[a-zA-Z]+\s+)?(\w+)\s*(\w+)\s*[\[\]\*]*[;,]?')

# Define type mappings from C to C#
type_mapping = {
    "PHANDLE": "IntPtr",
    "ACCESS_MASK": "uint",
    "POBJECT_ATTRIBUTES": "OBJECT_ATTRIBUTES",
    "NTSTATUS": "NTStatus",
    "HANDLE": "IntPtr",
    "ULONG": "uint",
    "DWORD": "uint",
    "BOOLEAN": "bool",
    "PVOID": "IntPtr",
    "PUNICODE_STRING": "UNICODE_STRING",
    # Add more mappings as needed
}

# Function to map C type to C# type
def map_type(c_type):
    return type_mapping.get(c_type, c_type)

# Function to generate P/Invoke signature from C signature
def generate_pinvoke_signature(c_signature):
    # Split the signature into lines and clean up
    lines = [line.strip() for line in c_signature.strip().split('\n')]

    # Extract the return type
    return_type_match = return_type_pattern.search(lines[1])
    return_type = map_type(return_type_match.group(1)) if return_type_match else "void"

    # Extract the function name
    function_name_match = function_name_pattern.search(lines[2])
    function_name = function_name_match.group(1) if function_name_match else "FunctionName"

    # Extract parameters
    parameters = []
    for line in lines[3:-1]:
        param_match = parameter_pattern.search(line)
        if param_match:
            c_type, _, direction, param_name = param_match.groups()
            csharp_type = map_type(c_type)
            if direction == '_Out_':
                csharp_type = f'out {csharp_type}'
            elif direction == '_In_':
                csharp_type = f'in {csharp_type}'
            elif direction == '_Inout_':
                csharp_type = f'ref {csharp_type}'
            parameters.append(f'{csharp_type} {param_name}')

    # Join parameters
    parameters_str = ', '.join(parameters)

    # Generate the P/Invoke signature
    pinvoke_signature = (
        f'[DllImport("ntdll.dll")]\n'
        f'internal static extern {return_type} {function_name}(\n'
        f'    {parameters_str});'
    )

    return pinvoke_signature

print(generate_pinvoke_signature("""
NTSYSAPI
NTSTATUS
NTAPI
NtOpenDirectoryObject(
    _Out_ PHANDLE DirectoryHandle,
    _In_ ACCESS_MASK DesiredAccess,
    _In_ POBJECT_ATTRIBUTES ObjectAttributes);
"""))

print(generate_pinvoke_signature("""
NTSYSAPI
VOID
NTAPI
RtlInitUnicodeString(
    _Out_ PUNICODE_STRING DestinationString,
    _In_opt_ PCWSTR SourceString);
"""))

print(generate_pinvoke_signature("""
NTSYSAPI
BOOLEAN
NTAPI
RtlFreeHeap(
    _In_ PVOID HeapHandle,
    _In_ ULONG Flags,
    _Frees_ptr_opt_ PVOID BaseAddress);

"""))

print(generate_pinvoke_signature("""
NTSYSAPI
PVOID
NTAPI
RtlAllocateHeap(
    _In_ PVOID HeapHandle,
    _In_ ULONG Flags,
    _In_ SIZE_T Size);
"""))
