namespace SharpKernelLib.Exception
{
    public class ProviderLoadException : SessionInitializationException
    {
        public ProviderLoadException() : base()
        {
        }

        public ProviderLoadException(string message) : base(message)
        {
        }

        public ProviderLoadException(string message, System.Exception innerException) : base(message, innerException)
        {
        }
    }
}
