using SharpKernelLib.SessionProviders;

namespace SharpKernelLib.RemoteCodeExecution
{
    public interface IRemoteCodeExecutionProvider
    {
        bool IsSupported(IProvider provider);
    }
}
