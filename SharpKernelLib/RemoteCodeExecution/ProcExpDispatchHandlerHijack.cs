using System;
using System.Collections.Generic;
using System.Text;

namespace SharpKernelLib.RemoteCodeExecution
{
    public class ProcExpDispatchHandlerHijack : IRemoteCodeExecutionProvider
    {
        // Code execution by patching the dispatch handler of the Process Explorer's driver (KDU's default style)
        // Ported of KDU 'victim.cpp/victim.h'
    }
}
