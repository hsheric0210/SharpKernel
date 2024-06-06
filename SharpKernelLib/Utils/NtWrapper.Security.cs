using System;
using System.Runtime.InteropServices;
using Windows.Wdk.System.SystemInformation;
using Windows.Wdk.Foundation;
using Windows.Win32.Foundation;
using Microsoft.Win32;
using SharpKernelLib.Exception;
using Windows.Win32.Storage.FileSystem;
using Windows.Wdk.Storage.FileSystem;

using static SharpKernelLib.Utils.NtUndocumented;
using static Windows.Win32.PInvoke;
using static Windows.Wdk.PInvoke;
using Windows.Win32.System.IO;
using Windows.Win32.System.Memory;
using Windows.Win32.Security;

namespace SharpKernelLib.Utils
{
    // Port of KDU Hamakaze sup.c and sup.h
    internal unsafe static partial class NtWrapper
    {
        internal static void SetPrivilegeState(Privilege privilege, bool state)
        {
            var ntstatus = NtOpenProcessToken(NtCurrentProcess(), (uint)(AccessMask.TOKEN_ADJUST_PRIVILEGES | AccessMask.TOKEN_QUERY), out var tokenHandle);
            if (!ntstatus.IsSuccess())
                throw new NtStatusException(ntstatus);

            var tokenPrivileges = new TOKEN_PRIVILEGES
            {
                PrivilegeCount = 1
            };
            tokenPrivileges.Privileges[0] = new LUID_AND_ATTRIBUTES
            {
                Luid = new LUID
                {
                    LowPart = (uint)privilege,
                    HighPart = 0
                },
                Attributes = state ? TOKEN_PRIVILEGES_ATTRIBUTES.SE_PRIVILEGE_ENABLED : 0
            };

            try
            {
                var returnLength = 0u;
                ntstatus = NtAdjustPrivilegesToken(tokenHandle, false, &tokenPrivileges, (uint)TOKEN_PRIVILEGES.SizeOf((int)tokenPrivileges.PrivilegeCount), null, &returnLength);
                if (!ntstatus.IsSuccess())
                    throw new NtStatusException(ntstatus);
            }
            finally
            {
                NtClose(tokenHandle);
            }
        }
    }
}
