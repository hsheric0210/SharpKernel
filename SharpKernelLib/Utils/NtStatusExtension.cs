using System;
using System.Collections.Generic;
using System.Text;
using Windows.Win32.Foundation;

namespace SharpKernelLib.Utils
{
    public static class NtStatusExtension
    {
        internal static bool IsSuccess(this NTSTATUS ntstatus) => ntstatus.SeverityCode == NTSTATUS.Severity.Success;

        internal static bool IsSuccess(this int ntstatus) => ((NTSTATUS)ntstatus).IsSuccess();

        internal static bool IsSuccess(this uint ntstatus) => ((NTSTATUS)ntstatus).IsSuccess();

    }
}
