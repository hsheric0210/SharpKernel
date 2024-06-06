namespace SharpKernelLib.Exception
{
    public class ProviderUnloadException : SessionInitializationException
    {
        public ProviderUnloadException() : base()
        {
        }

        public ProviderUnloadException(string message) : base(message)
        {
        }

        public ProviderUnloadException(string message, System.Exception innerException) : base(message, innerException)
        {
        }
    }
}
