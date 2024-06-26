﻿using System;

namespace SharpKernelLib.Exception
{
    public class SharpKernelException : ApplicationException
    {
        public SharpKernelException() : base()
        {
        }

        public SharpKernelException(string message) : base(message)
        {
        }

        public SharpKernelException(string message, System.Exception innerException) : base(message, innerException)
        {
        }
    }
}
