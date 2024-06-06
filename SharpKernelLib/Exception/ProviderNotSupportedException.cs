namespace SharpKernelLib.Exception
{
    public class ProviderNotSupportedException : SessionInitializationException
    {
        public ProviderNotSupportedException() : base()
        {
        }

        public ProviderNotSupportedException(string message) : base(message)
        {
        }

        public ProviderNotSupportedException(string message, System.Exception innerException) : base(message, innerException)
        {
        }
    }
}
