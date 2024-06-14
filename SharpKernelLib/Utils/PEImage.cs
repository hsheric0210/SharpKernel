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
using System.Runtime.CompilerServices;

namespace SharpKernelLib.Utils
{
    // Port of KDU Hamakaze sup.c and sup.h
    internal sealed unsafe class PEImage : IDisposable
    {
        public enum PEImageType
        {
            Unknown,
            ManualMapped,
            LdrLoadDll
        }

        private readonly void* imageBase;
        private readonly UIntPtr fileSize;
        private readonly PEImageType imageType;
        private bool disposedValue;

        internal void* ImageBaseRaw => disposedValue ? throw new ObjectDisposedException("imageBase") : imageBase;
        public IntPtr ImageBase => (IntPtr)ImageBaseRaw;

        private PEImage(void* imageBase, UIntPtr fileSize, PEImageType imageType)
        {
            this.imageBase = imageBase;
            this.fileSize = fileSize;
            this.imageType = imageType;
        }

        private static uint AlignGreater(uint p, uint align) => p % align == 0 ? p : p + align - p % align;

        private static uint AlignLessOrEqual(uint p, uint align) => p % align == 0 ? p : p - p % align;

        public static PEImage ManualMap(byte[] imageDataArray)
        {
            IntPtr image;
            uint imageSize;

            fixed (byte* imageData = imageDataArray)
            {
                var ntHeader = RtlImageNtHeader(imageData);
                if (ntHeader == null)
                    throw new InvalidPEFileException();

                var optHeader = ntHeader->OptionalHeader;
                var align = optHeader.FileAlignment;

                imageSize = ntHeader->OptionalHeader.SizeOfImage;
                if (imageDataArray.Length < imageSize) // Prevent OOB access (since we are doing direct pointer access, we MUST check this)
                    throw new InvalidPEFileException($"Expected imageData to be at least {imageSize} but got {imageDataArray.Length}. A truncated PE file?");

                image = Marshal.AllocHGlobal((int)imageSize);

                // Copy header
                Marshal.Copy(imageDataArray, 0, image, (int)AlignGreater(imageSize, align));

                // Copy sections
                var sections = (IMAGE_SECTION_HEADER*)(ntHeader + sizeof(IMAGE_FILE_HEADER) + ntHeader->FileHeader.SizeOfOptionalHeader);
                for (var i = 0; i < ntHeader->FileHeader.NumberOfSections; i++)
                {
                    if (sections[i].SizeOfRawData > 0 && sections[i].PointerToRawData > 0)
                        Marshal.Copy(imageDataArray, (int)AlignLessOrEqual(sections[i].PointerToRawData, align), image.Add(sections[i].VirtualAddress), (int)AlignGreater(sections[i].SizeOfRawData, align));
                }

                // Process Relocations
                const uint IMAGE_DIRECTORY_ENTRY_BASERELOC = (uint)ImageDataDirectory.IMAGE_DIRECTORY_ENTRY_BASERELOC;
                if (optHeader.NumberOfRvaAndSizes > IMAGE_DIRECTORY_ENTRY_BASERELOC)
                {
                    var relocDirHeader = optHeader.DataDirectory[IMAGE_DIRECTORY_ENTRY_BASERELOC];
                    if (relocDirHeader.VirtualAddress != 0)
                    {
                        var reloc = (IMAGE_BASE_RELOCATION*)image.Add(relocDirHeader.VirtualAddress);
                        var delta = image.ToInt64() - (long)optHeader.ImageBase;

                        var i = 0u;
                        while (i < relocDirHeader.Size)
                        {
                            var relocBlock = sizeof(IMAGE_BASE_RELOCATION);
                            var chains = (uint*)(reloc + relocBlock);

                            while (relocBlock < reloc->SizeOfBlock)
                            {
                                const uint IMAGE_REL_BASED_HIGHLOW = 3;
                                const uint IMAGE_REL_BASED_DIR64 = 10;
                                switch (*chains >> 12)
                                {
                                    case IMAGE_REL_BASED_HIGHLOW:
                                        *((uint*)image + reloc->VirtualAddress + (*chains & 0x0fff)) += (uint)delta;
                                        break;
                                    case IMAGE_REL_BASED_DIR64:
                                        *((long*)image + reloc->VirtualAddress + (*chains & 0x0fff)) += delta;
                                        break;
                                }

                                chains++;
                                relocBlock += sizeof(short);
                            }

                            i += reloc->SizeOfBlock;
                            reloc += reloc->SizeOfBlock;
                        }
                    }
                }
            }

            return FromExisting(image, (UIntPtr)imageSize, PEImageType.ManualMapped);
        }

        public static PEImage FromExisting(IntPtr imageBase, UIntPtr fileSize, PEImageType imageType) => new PEImage(imageBase.ToPointer(), fileSize, imageType);

        internal static PEImage LdrLoadDll(string fileName)
        {
            if (!new FileInfo(fileName).Exists)
                throw new FileNotFoundException(fileName);

            var fileNameU = new UNICODE_STRING();
            RtlInitUnicodeString(ref fileNameU, fileName);

            var characteristics = (uint)ImageFileCharacteristics.IMAGE_FILE_EXECUTABLE_IMAGE;
            var ntstatus = NtUndocumented.LdrLoadDll(null, &characteristics, &fileNameU, out var loadBase);
            if (!ntstatus.IsSuccess() || loadBase == null)
                throw new NtStatusException(ntstatus);

            var ntHeaders = RtlImageNtHeader(loadBase);
            if (ntHeaders == null)
                throw new InvalidPEFileException();

            return new PEImage(loadBase, (UIntPtr)ntHeaders->OptionalHeader.SizeOfImage, PEImageType.LdrLoadDll);
        }

        public int GetImageSize()
        {
            var ntstatus = LdrFindEntryForAddress(ImageBaseRaw, out var table);
            if (!ntstatus.IsSuccess())
                throw new NtStatusException(ntstatus);

            return (int)table.SizeOfImage;
        }

        public void ReplaceDllEntrypoint(string entryPointName, bool convertToExe)
        {
            var ntHeader = RtlImageNtHeader(ImageBaseRaw);
            if (ntHeader == null)
                throw new InvalidPEFileException();

            var entry = GetProcAddress(entryPointName);
            if (entry == IntPtr.Zero)
                throw new EntryPointNotFoundException(entryPointName);

            ntHeader->OptionalHeader.AddressOfEntryPoint = (uint)entry.Subtract(ImageBase).ToInt64();

            if (convertToExe)
                ntHeader->FileHeader.Characteristics &= ~ImageFileCharacteristics.IMAGE_FILE_DLL;

            // ntHeader->OptionalHeader.CheckSum = RecalculatePECheckSum(dllImage);

            throw new NotImplementedException();
        }

        /// <summary>
        /// RFC-1071
        /// </summary>
        private static ushort ChecksumCore(uint partialSum, ushort* source, uint length)
        {
            while (length-- > 0)
            {
                partialSum += *source++;
                partialSum = (partialSum >> 16) + (partialSum & 0xffff);
            }

            return (ushort)(((partialSum >> 16) + partialSum) & 0xffff);
        }

        public uint CalculateChecksum()
        {
            var fileSizeInt = fileSize.ToUInt32();
            var partialSum = ChecksumCore(0, (ushort*)ImageBaseRaw, (fileSizeInt + 1) >> 1);

            var ntHeaders = RtlImageNtHeader(ImageBaseRaw);
            if (ntHeaders != null)
            {
                var adjustSum = (ushort*)&ntHeaders->OptionalHeader.CheckSum;
                partialSum -= (ushort)(partialSum < adjustSum[0] ? 1 : 0);
                partialSum -= adjustSum[0];
                partialSum -= (ushort)(partialSum < adjustSum[1] ? 1 : 0);
                partialSum -= adjustSum[1];
            }
            else
            {
                partialSum = 0;
            }

            return partialSum + fileSizeInt;
        }

        public bool VerifyChecksum(out uint storedSum, out uint calculatedSum)
        {
            var ntHeaders = RtlImageNtHeader(ImageBaseRaw);
            storedSum = ntHeaders != null ? ntHeaders->OptionalHeader.CheckSum : fileSize.ToUInt32();
            calculatedSum = CalculateChecksum();

            return storedSum == calculatedSum;
        }

        public bool VerifyChecksum() => VerifyChecksum(out _, out _);

        public void ReplaceDllEntryPoint(string entryPointName, bool convertToExe)
        {
            if (imageType != PEImageType.ManualMapped)
                throw new NotSupportedException("This operation is only supported for Manual Mapped images.");

            var ntHeaders = RtlImageNtHeader(ImageBaseRaw);
            if (ntHeaders == null)
                throw new InvalidPEFileException();

            var entryPoint = GetProcAddress(entryPointName);
            if (entryPoint == IntPtr.Zero)
                throw new EntryPointNotFoundException(entryPointName);

            ntHeaders->OptionalHeader.AddressOfEntryPoint = (uint)entryPoint.Subtract(ImageBase).ToInt64();

            if (convertToExe)
                ntHeaders->FileHeader.Characteristics &= ~ImageFileCharacteristics.IMAGE_FILE_DLL;

            ntHeaders->OptionalHeader.CheckSum = CalculateChecksum();
        }

        /// <summary>
        /// PELoaderGetProcAddress
        /// </summary>
        public IntPtr GetProcAddress(string procName)
        {
            var ntHeaders = RtlImageNtHeader(ImageBaseRaw);

            var exportDir = (IMAGE_EXPORT_DIRECTORY*)ImageBase.Add(ntHeaders->OptionalHeader.DataDirectory[(int)ImageDataDirectory.IMAGE_DIRECTORY_ENTRY_EXPORT].VirtualAddress);

            var namePtr = (uint*)ImageBase.Add(exportDir->AddressOfNames);
            var funcPtr = (uint*)ImageBase.Add(exportDir->AddressOfFunctions);
            var ordPtr = (uint*)ImageBase.Add(exportDir->AddressOfNameOrdinals);

            // Binary search for the function name
            uint low = 0u, mid = 0u, high = exportDir->NumberOfNames - 1;

            while (low <= high)
            {
                mid = (low + high) / 2;
                var functionName = Marshal.PtrToStringAnsi(ImageBase.Add(*(namePtr + mid)));
                var comparison = string.Compare(functionName, procName, StringComparison.Ordinal); // DO NOT COMPARE IGNORECASE
                if (comparison < 0)
                    low = mid + 1;
                else if (comparison > 0)
                    high = mid - 1;
                else
                    break;
            }

            if (high < low)
                return IntPtr.Zero; // Function not found

            var ordinal = *(ordPtr + mid);
            if (ordinal >= exportDir->NumberOfFunctions)
                return IntPtr.Zero; // Function index OOB

            return ImageBase.Add(*(funcPtr + ordinal));
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                switch (imageType)
                {
                    case PEImageType.ManualMapped:
                        Marshal.FreeHGlobal(ImageBase);
                        break;
                    case PEImageType.LdrLoadDll:
                        LdrUnloadDll(ImageBaseRaw);
                        break;
                }

                disposedValue = true;
            }
        }

        ~PEImage()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
