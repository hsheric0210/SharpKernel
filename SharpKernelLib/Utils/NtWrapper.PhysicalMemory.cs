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
        internal const uint ALL_1_UINT = unchecked((uint)-1);
        internal const ulong ALL_1_ULONG = unchecked((ulong)-1L);

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

            var offset = (ulong)physicalAddress.ToInt64() & ~(PAGE_SIZE - 1);

            // Prevent access violation crash
            VEHTryCatchFinally(
                () => Marshal.Copy(mappedSection.Add(offset), buffer, startIndex, length),
                (ExceptionRecord _) =>
                {
                    //ignore
                },
                () => UnmapSection(mappedSection)
            );
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

            var offset = (ulong)physicalAddress.ToInt64() & ~(PAGE_SIZE - 1);

            // Prevent access violation crash
            VEHTryCatchFinally(
                () => Marshal.Copy(buffer, startIndex, mappedSection.Add(offset), length),
                (ExceptionRecord _) =>
                {
                    //ignore
                },
                () => UnmapSection(mappedSection)
            );
        }

        internal static void WritePhysicalMemory(IntPtr sectionHandle, IntPtr physicalAddress, byte[] buffer)
            => WritePhysicalMemory(sectionHandle, physicalAddress, buffer, 0, buffer.Length);

        internal delegate IntPtr DuplicateHandleDelegate(IntPtr deviceHandle, uint sourceProcessId, IntPtr sourceProcessHandle, IntPtr sourceHandle, AccessMask desiredAccess, uint handleAttributes, uint options);

        internal static string ConvertToString(this UNICODE_STRING unicodeString) => new string(unicodeString.Buffer.Value, 0, unicodeString.Length);

        internal static IntPtr DuplicatePhysicalMemoryHandle(IntPtr deviceHandle, IntPtr sourceProcessHandle, DuplicateHandleDelegate duplicateHandle)
        {
            var sectionHandle = HANDLE.Null;
            SYSTEM_HANDLE_INFORMATION_EX* handleInfo = null;
            OBJECT_NAME_INFORMATION* dupHandleInfo = null;

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
                handleInfo = (SYSTEM_HANDLE_INFORMATION_EX*)GetNtSystemInfo(SystemInformationClass.SystemExtendedHandleInformation, out _);

                var currentProcessId = GetCurrentProcessId();

                var sectionObjectType = ALL_1_UINT;
                for (ulong i = 0u, j = handleInfo->NumberOfHandles.ToUInt64(); i < j; i++)
                {
                    var handle = handleInfo->Handles[i];
                    if (handle.UniqueProcessId.ToInt64() == currentProcessId && handle.HandleValue == sectionHandle)
                    {
                        sectionObjectType = handle.ObjectTypeIndex;
                        break;
                    }
                }
                if (sectionObjectType == ALL_1_UINT)
                    throw new MemoryNotFoundException("SectionObjectType find fail");

                NtClose(sectionHandle);
                sectionHandle = HANDLE.Null;

                sectionNameU = new UNICODE_STRING();
                RtlInitUnicodeString(ref sectionNameU, @"\Device\PhysicalMemory");

                for (ulong i = 0u, j = handleInfo->NumberOfHandles.ToUInt64(); i < j; i++)
                {
                    var handle = handleInfo->Handles[i];
                    if (handle.UniqueProcessId == (IntPtr)SYSTEM_PROCESSID && handle.ObjectTypeIndex == sectionObjectType && handle.GrantedAccess == (uint)AccessMask.SECTION_ALL_ACCESS)
                    {
                        var dupHandle = duplicateHandle(deviceHandle, SYSTEM_PROCESSID, sourceProcessHandle, handle.HandleValue, AccessMask.MAXIMUM_ALLOWED, 0, 0);
                        dupHandleInfo = (OBJECT_NAME_INFORMATION*)QueryObjectInformation(ObjectInformationClass.ObjectNameInformation, dupHandle, out _);
                        if (RtlEqualUnicodeString(dupHandleInfo->Name, sectionNameU, true))
                            return dupHandle;
                    }
                }

                throw new MemoryNotFoundException("PhysicalMemory handle not found");
            }
            catch (NtStatusException ex)
            {
                throw new MemoryAccessException("", ex); // TODO: Specify error message
            }
            finally
            {
                if (sectionHandle != HANDLE.Null)
                    NtClose(sectionHandle);

                if (handleInfo != null)
                    Marshal.FreeHGlobal((IntPtr)handleInfo);

                if (dupHandleInfo != null)
                    Marshal.FreeHGlobal((IntPtr)dupHandleInfo);
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

            var cr3Value = IntPtr.Zero;

            // Prevent access violation crash
            VEHTryCatch(
                () =>
                {
                    // TODO: parallel search using 'Parallel.For'
                    while (offset < 0x100000) // 1 MiB limit
                    {
                        offset += (int)PAGE_SIZE;

                        // PROCESSOR_START_BLOCK->Jmp
                        var jmp = *(ulong*)(lowStub1M + offset);
                        if ((jmp & 0xffffffffffff00ff) != 0x00000001000600E9)
                            continue;

                        // PROCESSOR_START_BLOCK->LmTarget
                        var lmTarget = *(ulong*)(lowStub1M + offset).Add(lmTargetOffset);
                        if ((lmTarget & 0xfffff80000000003) != 0xfffff80000000000)
                            continue;

                        // PROCESSOR_START_BLOCK->ProcessorState->Cr3
                        var cr3 = *(ulong*)(lowStub1M + offset).Add(cr3Offset);
                        if ((cr3 & 0xffffff0000000fff) != 0)
                            continue;

                        cr3Value = new IntPtr((long)cr3);
                    }
                },
                (ExceptionRecord _) =>
                {
                    // ignore
                }
            );

            return cr3Value;
        }
    }
}
