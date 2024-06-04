using System;
using System.Collections.Generic;
using System.Text;

namespace SharpKernelLib.Exception
{
    public class MemoryNotFoundException : SharpKernelException
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
