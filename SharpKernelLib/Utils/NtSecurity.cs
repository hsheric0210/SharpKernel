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
    internal unsafe static class NtSecurity
    {
        internal const uint ACL_REVISION = 2u;
        internal const uint SECURITY_DESCRIPTOR_REVISION = 1u;
        internal const uint SECURITY_DESCRIPTOR_REVISION1 = 1u;
        internal static readonly SID_IDENTIFIER_AUTHORITY SECURITY_NT_AUTHORITY;

        static NtSecurity()
        {
            SECURITY_NT_AUTHORITY = new SID_IDENTIFIER_AUTHORITY();
            SECURITY_NT_AUTHORITY.Value[0] = 0;
            SECURITY_NT_AUTHORITY.Value[1] = 0;
            SECURITY_NT_AUTHORITY.Value[2] = 0;
            SECURITY_NT_AUTHORITY.Value[3] = 0;
            SECURITY_NT_AUTHORITY.Value[4] = 0;
            SECURITY_NT_AUTHORITY.Value[5] = 5;
        }

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

        /// <summary>
        /// supCreateSystemAdminAccessSD
        /// </summary>
        /// <remarks>
        /// DON'T FORGET TO 'Marshal.FreeHGlobal()' the returned buffer and aclBuffer!!
        /// </remarks>
        internal static IntPtr CreateSystemAdminAccessSecurityDescriptor(out IntPtr aclBuffer)
        {
            aclBuffer = IntPtr.Zero;
            var securityDescriptorBuffer = IntPtr.Zero;

            try
            {
                securityDescriptorBuffer = Marshal.AllocHGlobal(sizeof(SECURITY_DESCRIPTOR));
                var securityDescriptor = new PSECURITY_DESCRIPTOR(&securityDescriptorBuffer);
                var aclSize = RtlLengthRequiredSid(1) + RtlLengthRequiredSid(2) + sizeof(ACL) + (2 * sizeof(ACCESS_ALLOWED_ACE) - sizeof(uint));
                aclBuffer = Marshal.AllocHGlobal((int)aclSize);
                var acl = (ACL*)aclBuffer;

                var ntstatus = RtlCreateAcl(acl, (uint)aclSize, ACL_REVISION);
                if (!ntstatus.IsSuccess())
                    throw new NtStatusException(ntstatus);

                var sidBuffer = new ushort[2 * sizeof(SID)];
                var sid = new PSID(&sidBuffer);

                RtlInitializeSid(sid, SECURITY_NT_AUTHORITY, 1);
                *RtlSubAuthoritySid(sid, 0) = (uint)SecurityRID.SECURITY_LOCAL_SYSTEM_RID;
                RtlAddAccessAllowedAce(acl, ACL_REVISION, (uint)AccessMask.GENERIC_ALL, sid);

                RtlInitializeSid(sid, SECURITY_NT_AUTHORITY, 2);
                *RtlSubAuthoritySid(sid, 0) = (uint)SecurityRID.SECURITY_BUILTIN_DOMAIN_RID;
                *RtlSubAuthoritySid(sid, 0) = (uint)SecurityRID.DOMAIN_ALIAS_RID_ADMINS;
                RtlAddAccessAllowedAce(acl, ACL_REVISION, (uint)AccessMask.GENERIC_ALL, sid);

                ntstatus = RtlCreateSecurityDescriptor(securityDescriptor, SECURITY_DESCRIPTOR_REVISION1);
                if (!ntstatus.IsSuccess())
                    throw new NtStatusException(ntstatus);

                ntstatus = RtlSetDaclSecurityDescriptor(securityDescriptor, true, acl, false);
                if (!ntstatus.IsSuccess())
                    throw new NtStatusException(ntstatus);

                return securityDescriptorBuffer;
            }
            finally
            {
                if (aclBuffer != IntPtr.Zero)
                    Marshal.FreeHGlobal(aclBuffer);

                if (securityDescriptorBuffer != IntPtr.Zero)
                    Marshal.FreeHGlobal(securityDescriptorBuffer);

                aclBuffer = IntPtr.Zero;
                securityDescriptorBuffer = IntPtr.Zero;
            }
        }
    }
}
