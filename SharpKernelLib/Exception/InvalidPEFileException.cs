using System;
using System.Collections.Generic;
using System.Text;

namespace SharpKernelLib.Exception
{
    public class InvalidPEFileException : SharpKernelException
    {
        public InvalidPEFileException() : base()
        {
        }

        public InvalidPEFileException(string message) : base(message)
        {
        }

        public InvalidPEFileException(string message, System.Exception innerException) : base(message, innerException)
        {
        }
    }
}
