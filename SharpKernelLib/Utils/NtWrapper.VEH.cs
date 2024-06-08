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
    internal unsafe static partial class NtWrapper
    {
        private static ThreadLocal<bool> handleVectoredExceptions = new ThreadLocal<bool>(() => false);
        private static ThreadLocal<bool> caughtVectoredException = new ThreadLocal<bool>(() => false);
        private static ThreadLocal<ExceptionRecord> lastVectoredException = new ThreadLocal<ExceptionRecord>(() => default);

        private static int VEHTryCatchHandler(ref ExceptionPointers exceptionPointers)
        {
            if (!handleVectoredExceptions.Value)
                return 0; // EXCEPTION_CONTINUE_SEARCH

            lastVectoredException.Value = Marshal.PtrToStructure<ExceptionRecord>(exceptionPointers.ExceptionRecord);

            return 1; // EXCEPTION_CONTINUE_EXECUTION
        }

        internal static void VEHTryCatch(Action tryBlock, Action<ExceptionRecord> catchBlock) => VEHTryCatchFinally(tryBlock, catchBlock, () => { });

        internal static void VEHTryCatchFinally(Action tryBlock, Action<ExceptionRecord> catchBlock, Action finallyBlock)
        {
            // Register
            var veh = AddVectoredExceptionHandler(1, VEHTryCatchHandler);

            // Pre init
            handleVectoredExceptions.Value = true;
            caughtVectoredException.Value = false;
            lastVectoredException.Value = default;

            // Try
            tryBlock();

            handleVectoredExceptions.Value = false;

            // Catch
            if (caughtVectoredException.Value)
                catchBlock(lastVectoredException.Value);

            // Finally

            finallyBlock();

            // Post cleanup
            caughtVectoredException.Value = false;
            lastVectoredException.Value = default;

            // Unregister
            RemoveVectoredExceptionHandler(veh.ToPointer());
        }
    }
}
