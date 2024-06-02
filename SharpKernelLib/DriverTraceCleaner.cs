using System;
using System.Collections.Generic;
using System.Text;

namespace SharpKernelLib
{
    /// <summary>
    /// Clean up the driver traces and artifacts left on kernel memory to circumvent third-party malware/rootkits. (e.g. Kernel-mode Anticheats)
    /// </summary>
    public class DriverTraceCleaner
    {
        public DriverTraceCleaner(IMemoryAccessProvider memoryAccess) { }
    }
}
