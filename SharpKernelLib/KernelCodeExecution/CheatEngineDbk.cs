﻿using System;
using SharpKernelLib.SessionProviders;

namespace SharpKernelLib.KernelCodeExecution
{
    public class CheatEngineDbk
    {
        // Use Cheat Engine DBK driver's IOCTL_EXECUTE_CODE (from KDU)
        public bool IsSupported(IProvider provider) => throw new NotImplementedException();
    }
}
