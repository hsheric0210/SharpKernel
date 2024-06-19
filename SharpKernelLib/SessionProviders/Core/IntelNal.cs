using System;

namespace SharpKernelLib.SessionProviders.Core
{
    /// <summary>
    /// iqvw64e.sys CVE-2015-2291 INTEL-SA-00051
    /// </summary>
    public class IntelNal : ProviderBase
    {
        public override Version MinSupportedOsVersion => throw new NotImplementedException();

        public override Version MaxSupportedOsVersion => throw new NotImplementedException();

        public override ProviderFlags Flags => throw new NotImplementedException();

        public override string ProviderName => throw new NotImplementedException();

        public override string AssignedCVE => throw new NotImplementedException();

        public override string DriverName => throw new NotImplementedException();

        public override string DeviceName => throw new NotImplementedException();

        public bool IsMemoryAccessSupported => true;

        public IMemoryAccessProvider MemoryAccess => throw new NotImplementedException();

        public bool IsProcessAccessSupported => false;
        public IProcessAccessProvider ProcessAccess => throw new NotImplementedException();

        public override bool IsSupported() => true;
        public override bool PostOpenDriverCallback() => throw new NotImplementedException();
        public override bool PreOpenDriverCallback() => throw new NotImplementedException();
        public override bool RegisterDriverCallback() => throw new NotImplementedException();
        public override bool UnregisterDriverCallback() => throw new NotImplementedException();
        protected override byte[] GetDriverData() => throw new NotImplementedException();
    }
}
