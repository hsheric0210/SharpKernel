using System;

using static SharpKernelLib.Utils.NtUndocumented;
using SharpKernelLib.Exception;
using System.Runtime.InteropServices;

namespace SharpKernelLib.Utils
{
    internal static unsafe class PELoader
    {
        private static uint AlignGreater(uint p, uint align) => p % align == 0 ? p : p + align - p % align;

        private static uint AlignLessOrEqual(uint p, uint align) => p % align == 0 ? p : p - p % align;

        /// <summary>
        /// PELoaderLoadImage
        /// </summary>
        /// <remarks>
        /// DON'T FORGET TO 'Marshal.FreeHGlobal()' the returned buffer!
        /// </remarks>
        internal static IntPtr LoadImage(byte[] imageDataArray, out int imageSizeInt)
        {
            IntPtr image;

            fixed (byte* imageData = imageDataArray)
            {
                var ntHeader = RtlImageNtHeader(imageData);
                if (ntHeader == null)
                    throw new InvalidPEFileException();

                var optHeader = ntHeader->OptionalHeader;
                var align = optHeader.FileAlignment;

                var imageSize = ntHeader->OptionalHeader.SizeOfImage;
                if (imageDataArray.Length < imageSize) // Prevent OOB access (since we are doing direct pointer access, we MUST check this)
                    throw new InvalidPEFileException($"Expected imageData to be at least {imageSize} but got {imageDataArray.Length}. A truncated PE file?");
                imageSizeInt = (int)imageSize;

                image = Marshal.AllocHGlobal(imageSizeInt);

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

            return image;
        }

        /// <summary>
        /// PELoaderGetProcAddress
        /// </summary>
        internal static IntPtr GetProcAddress(IntPtr imageBase, string procName)
        {
            var ntHeaders = RtlImageNtHeader(imageBase.ToPointer());

            var exportDir = (IMAGE_EXPORT_DIRECTORY*)imageBase.Add(ntHeaders->OptionalHeader.DataDirectory[(int)ImageDataDirectory.IMAGE_DIRECTORY_ENTRY_EXPORT].VirtualAddress);

            var namePtr = (uint*)imageBase.Add(exportDir->AddressOfNames);
            var funcPtr = (uint*)imageBase.Add(exportDir->AddressOfFunctions);
            var ordPtr = (uint*)imageBase.Add(exportDir->AddressOfNameOrdinals);

            // Binary search for the function name
            uint low = 0u, mid = 0u, high = exportDir->NumberOfNames - 1;

            while (low <= high)
            {
                mid = (low + high) / 2;
                var functionName = Marshal.PtrToStringAnsi(imageBase.Add(*(namePtr + mid)));
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

            return imageBase.Add(*(funcPtr + ordinal));
        }
    }
}
