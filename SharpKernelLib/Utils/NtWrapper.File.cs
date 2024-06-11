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
    // Port of KDU Hamakaze sup.c and sup.h
    internal unsafe static partial class NtWrapper
    {
        internal static IntPtr LoadFileForMapping(string fileName)
        {
            var fileNameU = new UNICODE_STRING();
            RtlInitUnicodeString(ref fileNameU, fileName);

            var characteristics = (uint)ImageFileCharacteristics.IMAGE_FILE_EXECUTABLE_IMAGE;
            var ntstatus = LdrLoadDll(null, &characteristics, &fileNameU, out var imageBase);
            if (!ntstatus.IsSuccess() || imageBase == null)
                throw new NtStatusException(ntstatus);

            var ntHeaders = RtlImageNtHeader(imageBase);
            if (ntHeaders == null)
                throw new InvalidPEFileException();

            return (IntPtr)imageBase;
        }
    }
}
