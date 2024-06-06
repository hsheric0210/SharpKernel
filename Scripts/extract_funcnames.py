import re

# Function to extract function names
def extract_function_names(c_code):
    # Define pattern to match function names
    function_name_pattern = re.compile(r'NTAPI\s+(\w+)\s*\(')
    
    # Find all matches
    matches = function_name_pattern.findall(c_code)
    
    return matches

# Example input
c_code = """

NTSYSAPI
VOID
NTAPI
RtlSetUnhandledExceptionFilter(
    _In_ PRTLP_UNHANDLED_EXCEPTION_FILTER UnhandledExceptionFilter);

NTSYSAPI
LONG
NTAPI
RtlUnhandledExceptionFilter(
    _In_ PEXCEPTION_POINTERS ExceptionPointers);

"""

# Extract function names
function_names = extract_function_names(c_code)

# Print extracted function names
for name in function_names:
    print(name)