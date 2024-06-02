using System;
using System.Collections.Generic;
using System.Text;

namespace SharpKernelLib
{
    public abstract class ProviderBase : IProvider
    {
        public abstract string Name { get; }
        public abstract Version MinSupportedOsVersion { get; }
        public abstract Version MaxSupportedOsVersion { get; }
        public abstract ProviderVictimType Victim { get; }
        public abstract ProviderSourceBase SourceBase { get; }
        public abstract ProviderFlags Flags { get; }
        public abstract ProviderShellcodes SupportedShellcodes { get; }
        public abstract string ProviderName { get; }
        public abstract string AssignedCVE { get; }
        public abstract string DriverName { get; }
        public abstract string DeviceName { get; }
    }
}
