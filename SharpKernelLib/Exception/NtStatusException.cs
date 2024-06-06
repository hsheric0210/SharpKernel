using SharpKernelLib.Utils;

namespace SharpKernelLib.Exception
{
    public class NtStatusException : SharpKernelException
    {
        public NtStatusException() : base()
        {
        }

        public NtStatusException(int ntstatus) : base($"0x{ntstatus:X8} ({(NtStatus)(uint)ntstatus})")
        {
        }

        public NtStatusException(uint ntstatus) : base($"0x{ntstatus:X8} ({(NtStatus)ntstatus})")
        {
        }
    }
}
