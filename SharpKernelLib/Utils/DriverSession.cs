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

namespace SharpKernelLib.Utils
{
    public sealed unsafe class DriverSession : IDisposable
    {
        private HANDLE deviceHandle = HANDLE.Null;
        private bool disposedValue;

        public IntPtr DeviceHandle => deviceHandle;

        /// <summary>
        /// supOpenDriverEx: Open handle for driver.
        /// </summary>
        internal static NTSTATUS OpenCore(string deviceLink, AccessMask desiredAccess, out HANDLE deviceHandle)
        {
            deviceHandle = HANDLE.Null;

            var deviceLinkU = new UNICODE_STRING();
            RtlInitUnicodeString(ref deviceLinkU, deviceLink);

            var objectAttributes = new OBJECT_ATTRIBUTES
            {
                Length = (uint)sizeof(OBJECT_ATTRIBUTES),
                RootDirectory = HANDLE.Null,
                Attributes = (uint)ObjectAttributes.OBJ_CASE_INSENSITIVE,
                ObjectName = &deviceLinkU,
                SecurityDescriptor = null,
                SecurityQualityOfService = null
            };

            // Open the object
            var ntstatus = NtCreateFile(out var tmpDeviceHandle, (FILE_ACCESS_RIGHTS)desiredAccess, objectAttributes, out _, null, 0, 0, NTCREATEFILE_CREATE_DISPOSITION.FILE_OPEN, NTCREATEFILE_CREATE_OPTIONS.FILE_NON_DIRECTORY_FILE | NTCREATEFILE_CREATE_OPTIONS.FILE_SYNCHRONOUS_IO_NONALERT, null, 0);

            if (ntstatus.IsSuccess())
                deviceHandle = tmpDeviceHandle;

            return ntstatus;
        }

        public static DriverSession Open(string deviceName, AccessMask desiredAccess)
        {
            var session = new DriverSession();

            var deviceLink = $@"\DosDevices\{deviceName}";
            var ntstatus = OpenCore(deviceLink, desiredAccess, out var deviceHandle);
            if (ntstatus.IsSuccess())
                session.deviceHandle = deviceHandle;

            if (ntstatus == (uint)NtStatus.ObjectNameNotFound || ntstatus == (uint)NtStatus.NoSuchDevice)
            {
                // Try '\Device\%s'
                deviceLink = $@"\Device\{deviceName}";
                ntstatus = OpenCore(deviceLink, desiredAccess, out deviceHandle);
                if (ntstatus.IsSuccess())
                    session.deviceHandle = deviceHandle;
            }

            if (session.deviceHandle == HANDLE.Null)
                throw new ProviderLoadException("OpenDriver#OpenDriverCore", new NtStatusException(ntstatus));

            return session;
        }

        /// <summary>
        /// supCallDriverEx: Call driver.
        /// </summary>
        private NTSTATUS IoControlCore(uint ioctlCode, IntPtr inputBuffer, int inputBufferLength, IntPtr outputBuffer, int outputBufferLength, out IO_STATUS_BLOCK ioStatus)
        {
            var ntstatus = NtDeviceIoControlFile(deviceHandle, HANDLE.Null, null, null, out ioStatus, ioctlCode, inputBuffer.ToPointer(), (uint)inputBufferLength, outputBuffer.ToPointer(), (uint)outputBufferLength);
            if (ntstatus == (uint)NtStatus.Pending)
                ntstatus = NtWaitForSingleObject(deviceHandle, false, null);

            return ntstatus;
        }

        /// <summary>
        /// supCallDriver: Call driver.
        /// </summary>
        public bool IoControl(uint ioctlCode, IntPtr inputBuffer, int inputBufferLength, IntPtr outputBuffer, int outputBufferLength)
        {
            var ntstatus = IoControlCore(ioctlCode, inputBuffer, inputBufferLength, outputBuffer, outputBufferLength, out _);
            return ntstatus.IsSuccess();
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                NtClose(deviceHandle);
                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~DriverSession()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
