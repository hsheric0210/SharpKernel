using System;
using System.Collections.Generic;
using System.Text;

namespace SharpKernelLib.Exception
{
    public class CodeExecutionException : SharpKernelException
    {
        public CodeExecutionException() : base()
        {
        }

        public CodeExecutionException(string message) : base(message)
        {
        }

        public CodeExecutionException(string message, System.Exception innerException) : base(message, innerException)
        {
        }
    }
}
