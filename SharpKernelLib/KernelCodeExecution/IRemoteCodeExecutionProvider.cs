using SharpKernelLib.SessionProviders;

namespace SharpKernelLib.KernelCodeExecution
{
    public interface IRemoteCodeExecutionProvider
    {
        bool IsSupported(IProvider provider);
    }
}
