namespace SharpKernelLib.Exception
{
    public class MemoryNotFoundException : MemoryAccessException
    {
        public MemoryNotFoundException() : base()
        {
        }

        public MemoryNotFoundException(string message) : base(message)
        {
        }

        public MemoryNotFoundException(string message, System.Exception innerException) : base(message, innerException)
        {
        }
    }
}
