﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SharpKernelLib.RemoteCodeExecution
{
    public class KernelFunctionHook : IRemoteCodeExecutionProvider
    {
        // Code Execution by hooking random kernel functions (KDMapper style)
        // A bit unsafe because uses timing-attack(install hook; use; then remove it fast as possible) to bypass PatchGuard but most simple
    }
}