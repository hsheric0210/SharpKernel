using SharpKernelLib.KernelCodeExecution;
using SharpKernelLib.SessionProviders;
using SharpKernelLib.SessionProviders.Core;

namespace SharpKernelLib
{
    /// <summary>
    /// Main facade of SharpKernel. You must create this class to utilitze other features of this library.
    /// </summary>
    public class KernelSession
    {
        public IProvider Provider { get; }
        public IRemoteCodeExecutionProvider RceProvider { get; }

        public KernelSession() : this(new IntelNal(), new ProcExpDispatchHandlerHijack())
        {

        }

        public KernelSession(IProvider provider, IRemoteCodeExecutionProvider rceProvider)
        {
            Provider = provider;
            RceProvider = rceProvider;
        }

        public IMemoryAccessProvider MemoryAccess => Provider.MemoryAccess;
        public IProcessAccessProvider ProcessAccess => Provider.ProcessAccess;
    }
}
