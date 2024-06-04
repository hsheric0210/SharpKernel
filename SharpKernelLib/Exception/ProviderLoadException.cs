using System;
using System.Collections.Generic;
using System.Text;

namespace SharpKernelLib.Exception
{
    public class ProviderLoadException : SharpKernelException
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
