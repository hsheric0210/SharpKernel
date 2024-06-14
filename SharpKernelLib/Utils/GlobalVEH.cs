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
using System.Threading;

namespace SharpKernelLib.Utils
{
    // Port of KDU Hamakaze sup.c and sup.h
    internal unsafe static class GlobalVEH
    {
        private static bool handleVectoredExceptions;
        private static bool caughtVectoredException;
        private static ExceptionRecord lastVectoredException;

        private static int Handler(ref ExceptionPointers exceptionPointers)
        {
            if (!handleVectoredExceptions)
                return 0; // EXCEPTION_CONTINUE_SEARCH

            lastVectoredException = Marshal.PtrToStructure<ExceptionRecord>(exceptionPointers.ExceptionRecord);

            return 1; // EXCEPTION_CONTINUE_EXECUTION
        }

        internal static void TryCatch(Action tryBlock, Action<ExceptionRecord> catchBlock) => TryCatchFinally(tryBlock, catchBlock, () => { });

        internal static void TryCatchFinally(Action tryBlock, Action<ExceptionRecord> catchBlock, Action finallyBlock)
        {
            // Register
            var veh = AddVectoredExceptionHandler(1, Handler);

            // Pre init
            handleVectoredExceptions = true;
            caughtVectoredException = false;
            lastVectoredException = default;

            try // In case of C# exception
            {
                // Try
                tryBlock();

                handleVectoredExceptions = false;

                // Catch
                if (caughtVectoredException)
                    catchBlock(lastVectoredException);
            }
            finally
            {
                // Always cleanup

                finallyBlock();

                // Post cleanup
                handleVectoredExceptions = false;
                caughtVectoredException = false;
                lastVectoredException = default;

                // Unregister
                RemoveVectoredExceptionHandler(veh.ToPointer());
            }
        }
    }
}
