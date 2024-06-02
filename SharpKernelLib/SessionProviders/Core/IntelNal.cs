using System;
using System.Collections.Generic;
using System.Text;

namespace SharpKernelLib.SessionProviders.Core
{
    /// <summary>
    /// iqvw64e.sys CVE-2015-2291 INTEL-SA-00051
    /// </summary>
    public class IntelNal : IProvider
    {
        public Version MinSupportedOsVersion => throw new NotImplementedException();

        public Version MaxSupportedOsVersion => throw new NotImplementedException();

        public ProviderFlags Flags => throw new NotImplementedException();

        public ProviderShellcodes SupportedShellcodes => throw new NotImplementedException();

        public string ProviderName => throw new NotImplementedException();

        public string AssignedCVE => throw new NotImplementedException();

        public string DriverName => throw new NotImplementedException();

        public string DeviceName => throw new NotImplementedException();

        public bool IsMemoryAccessSupported => true;

        public bool IsProcessAccessSupported => false;

        public bool IsAvailable() => true;
        public bool PostOpenDriverCallback() => throw new NotImplementedException();
        public bool PreOpenDriverCallback() => throw new NotImplementedException();
        public bool RegisterDriverCallback() => throw new NotImplementedException();
        public bool StartVulnerableDriver() => throw new NotImplementedException();
        public bool StopVulnerableDriver() => throw new NotImplementedException();
        public bool UnregisterDriverCallback() => throw new NotImplementedException();
        public IMemoryAccessProvider GetMemoryAccessor() => throw new NotImplementedException();
        public IProcessAccessProvider GetProcessAccessor() => throw new NotImplementedException();
    }
}
