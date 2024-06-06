using SharpKernelLib.Exception;
using SharpKernelLib.Utils;
using System;
using System.IO;
using Windows.Win32.Foundation;
using static SharpKernelLib.Utils.NtWrapper;

namespace SharpKernelLib.SessionProviders
{
    public abstract class ProviderBase : IProvider
    {
        private HANDLE deviceHandle;
        private IMemoryAccessProvider memoryProvider;
        private IProcessAccessProvider processProvider;

        public abstract Version MinSupportedOsVersion { get; }
        public abstract Version MaxSupportedOsVersion { get; }
        public abstract ProviderFlags Flags { get; }
        public abstract string ProviderName { get; }
        public abstract string AssignedCVE { get; }
        public abstract string DriverName { get; }
        public abstract string DeviceName { get; }

        public IMemoryAccessProvider MemoryAccess => memoryProvider ??= CreateMemoryAccessProvider();

        public IProcessAccessProvider ProcessAccess => processProvider ??= CreateProcessAccessProvider();

        protected ProviderBase()
        {
        }

        public virtual bool IsSupported() => true;
        public virtual bool PreOpenDriverCallback() => true;
        public virtual bool PostOpenDriverCallback() => true;
        public virtual bool RegisterDriverCallback() => true;
        public virtual bool UnregisterDriverCallback() => true;
        public virtual void StartVulnerableDriver()
        {
            if (IsProviderAlreadyLoaded())
                return;

            var driverPath = $"{Environment.CurrentDirectory}\\{DriverName}.sys"; // TODO: Name randomization?
            File.WriteAllBytes(driverPath, GetDriverData());

            LoadDriver(DriverName, driverPath, false);

            if (!PreOpenDriverCallback())
                return;

            deviceHandle = OpenDriver(DeviceName, AccessMask.SYNCHRONIZE | AccessMask.WRITE_DAC | AccessMask.GENERIC_WRITE | AccessMask.GENERIC_READ);

            if (!PostOpenDriverCallback())
                return;

            RegisterDriverCallback();
        }

        public virtual void StopVulnerableDriver()
        {

        }

        public virtual IMemoryAccessProvider CreateMemoryAccessProvider() => new EmptyMemoryAccessProvider();

        public virtual IProcessAccessProvider CreateProcessAccessProvider() => new EmptyProcessAccessProvider();

        protected virtual bool IsProviderAlreadyLoaded() => IsObjectExists("\\Device", DeviceName);

        protected abstract byte[] GetDriverData();
    }

    public class EmptyMemoryAccessProvider : IMemoryAccessProvider
    {
        public MemoryAccessProviderFlags Flags => MemoryAccessProviderFlags.None;

        public bool QueryPML4(out IntPtr pml4Value)
        {
            pml4Value = default;
            return false;
        }

        public bool ReadKernelVirtual(IntPtr address, IntPtr buffer, int numberOfBytes) => false;

        public bool ReadKernelVirtual(IntPtr address, out byte[] bytes, int numberOfBytes)
        {
            bytes = null;
            return false;
        }

        public bool ReadPhysical(IntPtr physicalAddress, IntPtr buffer, int numberOfBytes) => false;

        public bool ReadPhysical(IntPtr physicalAddress, out byte[] bytes, int numberOfBytes)
        {
            bytes = null;
            return false;
        }

        public bool VirtualToPhysical(IntPtr virtualAddress, out IntPtr physicalAddress)
        {
            physicalAddress = default;
            return false;
        }

        public bool WriteKernelVirtual(IntPtr address, IntPtr buffer, int numberOfBytes) => false;

        public bool WriteKernelVirtual(IntPtr addres, in byte[] bytes, int numberOfBytes) => false;

        public bool WritePhysical(IntPtr physicalAddress, IntPtr buffer, int numberOfBytes) => false;

        public bool WritePhysical(IntPtr physicalAddress, in byte[] bytes, int numberOfBytes) => false;
    }

    public class EmptyProcessAccessProvider : IProcessAccessProvider
    {
        public bool OpenProcess(int processId, int accessMask, out IntPtr processHandle)
        {
            processHandle = default;
            return false;
        }

        public bool TerminateProcess(int processId) => false;
    }
}
