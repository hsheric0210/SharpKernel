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
using System.IO;

namespace SharpKernelLib.Utils
{
    // Port of KDU Hamakaze sup.c and sup.h
    internal sealed unsafe class PEImage
    {
        private void* imageBase;
        private PEImage(void* imageBase) => this.imageBase = imageBase;

        public static PEImage ManualMap(byte[] imageData)
        {
        }

        public static PEImage FromMapped(IntPtr imageBase)
        {
        }

        public static PEImage LoadDll(string fileName)
        {
        }

        internal static int GetImageSize(IntPtr imageBase)
        {
            var ntstatus = LdrFindEntryForAddress(imageBase.ToPointer(), out var table);
            if (!ntstatus.IsSuccess())
                throw new NtStatusException(ntstatus);

            return (int)table.SizeOfImage;
        }

        internal static void ReplaceDllEntrypoint(byte[] dllImage, int dllImageSize, string entryPointName, bool convertToExe)
        {
            var ntHeader = RtlImageNtHeader(&dllImage);
            if (ntHeader == null)
                throw new InvalidPEFileException();

            var dllBase = PELoader.LoadImage(dllImage, out var imageSize);

            var entry = PELoader.GetProcAddress(dllBase, entryPointName);
            if (entry == IntPtr.Zero)
                throw new EntryPointNotFoundException(entryPointName);

            ntHeader->OptionalHeader.AddressOfEntryPoint = (uint)entry.Subtract(dllBase).ToInt64();

            if (convertToExe)
                ntHeader->FileHeader.Characteristics &= ~ImageFileCharacteristics.IMAGE_FILE_DLL;

            ntHeader->OptionalHeader.CheckSum = RecalculatePECheckSum(dllImage);

        }

        internal static uint RecalculatePECheckSum(byte[] image)
        {
        }
    }
}
