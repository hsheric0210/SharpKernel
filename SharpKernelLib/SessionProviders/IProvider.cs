using System;

namespace SharpKernelLib.SessionProviders
{
    // _KDU_DB_ENTRY
    public interface IProvider
    {
        Version MinSupportedOsVersion { get; }
        Version MaxSupportedOsVersion { get; }
        ProviderFlags Flags { get; }

        string ProviderName { get; }
        string AssignedCVE { get; }
        string DriverName { get; }
        string DeviceName { get; }

        IMemoryAccessProvider MemoryAccess { get; }

        IProcessAccessProvider ProcessAccess { get; }

        bool IsSupported();

        bool RegisterDriverCallback();
        bool UnregisterDriverCallback();
        bool PreOpenDriverCallback();
        bool PostOpenDriverCallback();

        void StartVulnerableDriver();
        void StopVulnerableDriver();
    }

    public interface IMemoryAccessProvider
    {
        MemoryAccessProviderFlags Flags { get; }

        bool ReadKernelVirtual(IntPtr address, IntPtr buffer, int numberOfBytes);
        bool ReadKernelVirtual(IntPtr address, out byte[] bytes, int numberOfBytes);

        bool WriteKernelVirtual(IntPtr address, IntPtr buffer, int numberOfBytes);
        bool WriteKernelVirtual(IntPtr addres, in byte[] bytes, int numberOfBytes);

        bool VirtualToPhysical(IntPtr virtualAddress, out IntPtr physicalAddress);

        bool QueryPML4(out IntPtr pml4Value);

        bool ReadPhysical(IntPtr physicalAddress, IntPtr buffer, int numberOfBytes);
        bool ReadPhysical(IntPtr physicalAddress, out byte[] bytes, int numberOfBytes);

        bool WritePhysical(IntPtr physicalAddress, IntPtr buffer, int numberOfBytes);
        bool WritePhysical(IntPtr physicalAddress, in byte[] bytes, int numberOfBytes);
    }

    public interface IProcessAccessProvider
    {
        bool OpenProcess(int processId, int accessMask, out IntPtr processHandle);

        bool TerminateProcess(int processId);
    }

    [Flags]
    public enum ProviderFlags
    {
        None = 0,
        SupportHVCI = 1 << 0,
        SignatureWHQL = 1 << 1,
        IgnoreChecksum = 1 << 2,
        NoForcedSD = 1 << 3,
        NoUnloadSupport = 1 << 4,
        NoVictim = 1 << 5,
        UseSymbols = 1 << 6,
    }

    [Flags]
    public enum MemoryAccessProviderFlags
    {
        None = 0,
        PML4FromLowStub = 1 << 0,
        PhysMemoryBruteForce = 1 << 1,
        PreferPhysical = 1 << 2,
        PreferVirtual = 1 << 3,
    }
}
