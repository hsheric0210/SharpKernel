namespace SharpKernelLib.Exception
{
    public class MemoryAccessException : SharpKernelException
    {
        public MemoryAccessException() : base()
        {
        }

        public MemoryAccessException(string message) : base(message)
        {
        }

        public MemoryAccessException(string message, System.Exception innerException) : base(message, innerException)
        {
        }
    }
}
