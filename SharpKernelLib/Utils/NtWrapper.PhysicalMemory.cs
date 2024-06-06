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
using System.Linq;

namespace SharpKernelLib.Utils
{
    // Port of KDU Hamakaze sup.c and sup.h
    internal unsafe static partial class NtWrapper
    {
        internal const ulong PAGE_SIZE = 0x1000ul; // Windows Default Page Size: 1024 bytes
        internal const uint SYSTEM_PROCESSID = 4;

        /// <summary>
        /// supMapPhysicalMemory: Map physical memory.
        /// </summary>
        internal static IntPtr MapPhysicalMemory(IntPtr sectionHandle, IntPtr physicalAddress, int numberOfBytes, bool readWrite)
        {
            void* viewBase = null;
            var offset = (ulong)physicalAddress.ToInt64() & ~(PAGE_SIZE - 1);
            var viewSize = new UIntPtr(((ulong)physicalAddress.ToInt64() - offset) + (uint)numberOfBytes);
            var protect = readWrite ? PAGE_PROTECTION_FLAGS.PAGE_READWRITE : PAGE_PROTECTION_FLAGS.PAGE_READONLY;
            var ntstatus = NtMapViewOfSection((HANDLE)sectionHandle, NtCurrentProcess(), &viewBase, null, UIntPtr.Zero, &offset, &viewSize, SECTION_INHERIT.ViewUnmap, 0, protect);
            if (!ntstatus.IsSuccess())
                throw new MemoryAccessException("MapPhysicalMemory#NtMapViewOfSection", new NtStatusException(ntstatus));

            return new IntPtr(viewBase);
        }

        internal static void UnmapSection(IntPtr baseAddress) => NtUnmapViewOfSection(NtCurrentProcess(), baseAddress.ToPointer());

        internal static void ReadPhysicalMemory(IntPtr sectionHandle, IntPtr physicalAddress, byte[] buffer, int startIndex, int length)
        {
            var mappedSection = MapPhysicalMemory(sectionHandle, physicalAddress, length, false);

            if (mappedSection == IntPtr.Zero)
                return;

            try
            {
                var offset = (ulong)physicalAddress.ToInt64() & ~(PAGE_SIZE - 1);
                Marshal.Copy(mappedSection.Add(offset), buffer, startIndex, length);
            }
            finally
            {
                UnmapSection(mappedSection);
            }
        }

        internal static byte[] ReadPhysicalMemory(IntPtr sectionHandle, IntPtr physicalAddress, int length)
        {
            var buffer = new byte[length];
            ReadPhysicalMemory(sectionHandle, physicalAddress, buffer, 0, length);
            return buffer;
        }

        internal static void WritePhysicalMemory(IntPtr sectionHandle, IntPtr physicalAddress, byte[] buffer, int startIndex, int length)
        {
            var mappedSection = MapPhysicalMemory(sectionHandle, physicalAddress, length, true);

            if (mappedSection == IntPtr.Zero)
                return;

            try
            {
                var offset = (ulong)physicalAddress.ToInt64() & ~(PAGE_SIZE - 1);
                Marshal.Copy(buffer, startIndex, mappedSection.Add(offset), length);
            }
            finally
            {
                UnmapSection(mappedSection);
            }
        }

        internal static void WritePhysicalMemory(IntPtr sectionHandle, IntPtr physicalAddress, byte[] buffer)
            => WritePhysicalMemory(sectionHandle, physicalAddress, buffer, 0, buffer.Length);

        internal delegate IntPtr DuplicateHandleDelegate(IntPtr deviceHandle, uint sourceProcessId, IntPtr sourceProcessHandle, IntPtr sourceHandle, AccessMask desiredAccess, uint handleAttributes, uint options);

        internal static string ConvertToString(this UNICODE_STRING unicodeString) => new string(unicodeString.Buffer.Value, 0, unicodeString.Length);

        internal static IntPtr DuplicatePhysicalMemoryHandle(IntPtr deviceHandle, IntPtr sourceProcessHandle, DuplicateHandleDelegate duplicateHandle)
        {
            var sectionHandle = HANDLE.Null;
            var handleArrayNativeBuf = IntPtr.Zero;
            var dupHandleInfo = IntPtr.Zero;

            try
            {
                var sectionNameU = new UNICODE_STRING();
                RtlInitUnicodeString(ref sectionNameU, @"\KnownDlls\kernel32.dll");

                var sectionObjectAttributes = new OBJECT_ATTRIBUTES
                {
                    Length = (uint)sizeof(OBJECT_ATTRIBUTES),
                    RootDirectory = HANDLE.Null,
                    Attributes = (uint)ObjectAttributes.OBJ_CASE_INSENSITIVE,
                    ObjectName = &sectionNameU,
                    SecurityDescriptor = null,
                    SecurityQualityOfService = null,
                };

                var ntstatus = NtOpenSection(out sectionHandle, AccessMask.SECTION_QUERY, &sectionObjectAttributes);
                if (!ntstatus.IsSuccess())
                    throw new MemoryAccessException("DuplicatePhysicalMemoryHandle#NtOpenSection", new NtStatusException(ntstatus));

                // Marshal manually because .NET marshaller doesn't support dynamic array
                handleArrayNativeBuf = GetNtSystemInfo(SystemInformationClass.SystemExtendedHandleInformation, out _);
                var handleArrayNative = Marshal.PtrToStructure<SYSTEM_HANDLE_INFORMATION_EX>(handleArrayNativeBuf);
                var handleArray = new SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX[(int)handleArrayNative.NumberOfHandles];
                for (ulong i = 0, j = handleArrayNative.NumberOfHandles.ToUInt64(); i < j; i++)
                    handleArray[i] = Marshal.PtrToStructure<SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX>(handleArrayNative.Handles.Add(i * (ulong)sizeof(SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX)));

                var currentProcessId = GetCurrentProcessId();

                var sectionObjectTypeElements = handleArray
                    .Where(handle => handle.UniqueProcessId.ToInt64() == currentProcessId && handle.HandleValue == sectionHandle)
                    .Select(handle => handle.ObjectTypeIndex);
                if (!sectionObjectTypeElements.Any())
                    throw new MemoryAccessException("SectionObjectType find fail");

                var sectionObjectType = sectionObjectTypeElements.First();

                NtClose(sectionHandle);
                sectionHandle = HANDLE.Null;

                sectionNameU = new UNICODE_STRING();
                RtlInitUnicodeString(ref sectionNameU, @"\Device\PhysicalMemory");

                foreach (var handleEntry in handleArray)
                {
                    if (handleEntry.UniqueProcessId == (IntPtr)SYSTEM_PROCESSID && handleEntry.ObjectTypeIndex == sectionObjectType && handleEntry.GrantedAccess == (uint)AccessMask.SECTION_ALL_ACCESS)
                    {
                        var dupHandle = duplicateHandle(deviceHandle, SYSTEM_PROCESSID, sourceProcessHandle, handleEntry.HandleValue, AccessMask.MAXIMUM_ALLOWED, 0, 0);
                        dupHandleInfo = QueryObjectInformation(ObjectInformationClass.ObjectNameInformation, dupHandle, out _);
                        var handleName = Marshal.PtrToStructure<OBJECT_NAME_INFORMATION>(dupHandleInfo).Name;
                        if (RtlEqualUnicodeString(handleName, sectionNameU, true))
                        {
                            return dupHandle;
                        }
                    }
                }

                throw new MemoryNotFoundException();
            }
            catch (NtStatusException ex)
            {
                throw new MemoryAccessException("", ex); // TODO: Specify error message
            }
            finally
            {
                if (sectionHandle != HANDLE.Null)
                    NtClose(sectionHandle);

                if (handleArrayNativeBuf != IntPtr.Zero)
                    Marshal.FreeHGlobal(handleArrayNativeBuf);

                if (dupHandleInfo != IntPtr.Zero)
                    Marshal.FreeHGlobal(dupHandleInfo);
            }
        }

        /// <summary>
        /// supGetPML4FromLowStub1M
        /// </summary>
        internal static IntPtr FindPML4FromLowStub1M(IntPtr lowStub1M)
        {
            var offset = 0;
            var lmTargetOffset = Marshal.OffsetOf<PROCESSOR_START_BLOCK>("LmTarget");
            var cr3Offset = Marshal.OffsetOf<PROCESSOR_START_BLOCK>("ProcessorState").Add(Marshal.OffsetOf<KSPECIAL_REGISTERS>("Cr3"));

            // hope this don't fuq up the .NET vm
            while (offset < 0x100000) // 1 MiB limit
            {
                offset += (int)PAGE_SIZE;

                var jmp = (ulong)Marshal.ReadInt64(lowStub1M + offset);
                if ((jmp & 0xffffffffffff00ff) != 0x00000001000600E9)
                    continue;

                var lmTarget = (ulong)Marshal.ReadInt64((lowStub1M + offset).Add(lmTargetOffset));
                if ((lmTarget & 0xfffff80000000003) != 0xfffff80000000000)
                    continue;

                var cr3 = (ulong)Marshal.ReadInt64((lowStub1M + offset).Add(cr3Offset));
                if ((cr3 & 0xffffff0000000fff) != 0)
                    continue;

                return new IntPtr((long)cr3);
            }

            return IntPtr.Zero;
        }
    }
}
