using System;
using System.Collections.Generic;
using System.Text;

namespace SharpKernelLib.Exception
{
    public class ProviderNotSupportedException : SharpKernelException
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
