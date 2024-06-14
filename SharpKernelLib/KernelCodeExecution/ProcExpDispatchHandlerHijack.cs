using System;
using SharpKernelLib.SessionProviders;

namespace SharpKernelLib.KernelCodeExecution
{
    public class ProcExpDispatchHandlerHijack : IRemoteCodeExecutionProvider
    {
        // Code execution by patching the dispatch handler of the Process Explorer's driver (KDU's default style)
        // Ported of KDU 'victim.cpp/victim.h'
        public bool IsSupported(IProvider provider) => throw new NotImplementedException();
    }
}
