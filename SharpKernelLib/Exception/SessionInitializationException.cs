namespace SharpKernelLib.Exception
{
    public class SessionInitializationException : SharpKernelException
    {
        public SessionInitializationException() : base()
        {
        }

        public SessionInitializationException(string message) : base(message)
        {
        }

        public SessionInitializationException(string message, System.Exception innerException) : base(message, innerException)
        {
        }
    }
}
