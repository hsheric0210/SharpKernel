using System;
using System.Collections.Generic;
using System.Text;

namespace SharpKernelLib.KernelCodeExecution.Shellcode
{
    public interface IShellcode
    {
        byte[] GetShellcode();
    }
}
