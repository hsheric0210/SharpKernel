using System;

namespace SharpKernelLib
{
    // _KDU_DB_ENTRY
    public interface IProvider
    {
        Version MinSupportedOsVersion { get; }
        Version MaxSupportedOsVersion { get; }
        ProviderFlags Flags { get; }
        ProviderShellcodes SupportedShellcodes { get; }

        string ProviderName { get; }
        string AssignedCVE { get; }
        string DriverName { get; }
        string DeviceName { get; }

        bool IsMemoryAccessSupported { get; }
        bool IsProcessAccessSupported { get; }

        bool IsAvailable();
        bool StartVulnerableDriver();
        bool StopVulnerableDriver();

        bool RegisterDriverCallback();
        bool UnregisterDriverCallback();
        bool PreOpenDriverCallback();
        bool PostOpenDriverCallback();

        IMemoryAccessProvider GetMemoryAccessor();

        IProcessAccessProvider GetProcessAccessor();
    }

    public interface IMemoryAccessProvider
    {
        bool ReadKernelVM(IntPtr address, IntPtr buffer, int numberOfBytes);
        bool ReadKernelVM(IntPtr address, out byte[] bytes, int numberOfBytes);

        bool WriteKernelVM(IntPtr address, IntPtr buffer, int numberOfBytes);
        bool WriteKernelVM(IntPtr addres, in byte[] bytes, int numberOfBytes);

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
        PML4FromLowStub = 1 << 5,
        NoVictim = 1 << 6,
        PhysMemoryBruteForce = 1 << 7,
        PreferPhysical = 1 << 8,
        PreferVirtual = 1 << 9,
        CompanionRequired = 1 << 10,
        UseSymbols = 1 << 11,
        OpenProcessSupported = 1 << 12
    }

    [Flags]
    public enum ProviderShellcodes
    {
        None = 0,
        V1 = 1 << 0,
        V2 = 1 << 1,
        V3 = 1 << 2,
        V4 = 1 << 3,
    }
}
