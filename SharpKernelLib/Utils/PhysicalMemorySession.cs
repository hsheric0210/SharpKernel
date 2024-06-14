using SharpKernelLib.Exception;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Windows.Wdk.Foundation;
using Windows.Win32.Foundation;

using static SharpKernelLib.Utils.NtConstants;
using static SharpKernelLib.Utils.NtUndocumented;
using static Windows.Win32.PInvoke;
using static Windows.Wdk.PInvoke;
using Windows.Win32.System.IO;
using Windows.Win32.System.Memory;
using System.Linq;

namespace SharpKernelLib.Utils
{
    public sealed unsafe class PhysicalMemorySession : IDisposable
    {
        private bool disposedValue;
        private readonly HANDLE sectionHandle;

        internal HANDLE SectionHandle => disposedValue ? throw new ObjectDisposedException("sectionHandle") : sectionHandle;

        private PhysicalMemorySession(HANDLE sectionHandle) => this.sectionHandle = sectionHandle;

        public static PhysicalMemorySession Open(DriverSession driverSession, IntPtr systemProcessHandle, DuplicateHandleDelegate duplicateHandleProvider)
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
                handleInfo = SYSTEM_HANDLE_INFORMATION_EX.QueryData();

                var currentProcessId = GetCurrentProcessId();

                var sectionObjectType = MINUS_1_UINT;
                for (ulong i = 0u, j = handleInfo->NumberOfHandles.ToUInt64(); i < j; i++)
                {
                    var handle = handleInfo->Handles[i];
                    if (handle.UniqueProcessId.ToInt64() == currentProcessId && handle.HandleValue == sectionHandle)
                    {
                        sectionObjectType = handle.ObjectTypeIndex;
                        break;
                    }
                }

                if (sectionObjectType == MINUS_1_UINT)
                    throw new MemoryNotFoundException("Can't find the object type index of 'Section' type.");

                NtClose(sectionHandle);
                sectionHandle = HANDLE.Null;

                sectionNameU = new UNICODE_STRING();
                RtlInitUnicodeString(ref sectionNameU, @"\Device\PhysicalMemory");

                var physicalMemoryHandle = HANDLE.Null;
                for (ulong i = 0u, j = handleInfo->NumberOfHandles.ToUInt64(); i < j; i++)
                {
                    var handle = handleInfo->Handles[i];
                    if (handle.UniqueProcessId == (IntPtr)SYSTEM_PROCESSID && handle.ObjectTypeIndex == sectionObjectType && handle.GrantedAccess == (uint)AccessMask.SECTION_ALL_ACCESS)
                    {
                        var dupHandle = (HANDLE)duplicateHandleProvider(driverSession.DeviceHandle, SYSTEM_PROCESSID, systemProcessHandle, handle.HandleValue, AccessMask.MAXIMUM_ALLOWED, 0, 0);
                        dupHandleInfo = OBJECT_NAME_INFORMATION.QueryData(dupHandle);
                        if (RtlEqualUnicodeString(dupHandleInfo->Name, sectionNameU, true))
                        {
                            // Found!!
                            physicalMemoryHandle = dupHandle;
                            break;
                        }
                        else
                        {
                            NtClose((HANDLE)dupHandle); // Must not leak duplicated handles
                        }
                    }
                }

                if (physicalMemoryHandle == HANDLE.Null)
                    throw new MemoryNotFoundException("PhysicalMemory section handle not found from System process.");

                return new PhysicalMemorySession(physicalMemoryHandle);
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

        public MappedPhysicalMemory MapRegion(IntPtr physicalAddress, UIntPtr regionSize, PageProtections protect) => new MappedPhysicalMemory(SectionHandle, physicalAddress, regionSize, protect);

        public void Read(IntPtr physicalAddress, byte[] buffer, int startIndex, int length)
        {
            using (var mappedSection = MapRegion(physicalAddress, (UIntPtr)length, PageProtections.PAGE_READONLY))
                mappedSection.Read(buffer, startIndex, length);
        }

        public byte[] Read(IntPtr physicalAddress, int length)
        {
            var buffer = new byte[length];
            Read(physicalAddress, buffer, 0, length);
            return buffer;
        }

        public void Write(IntPtr physicalAddress, byte[] buffer, int startIndex, int length)
        {
            using (var mappedSection = MapRegion(physicalAddress, (UIntPtr)length, PageProtections.PAGE_READWRITE))
                mappedSection.Write(buffer, startIndex, length);
        }

        public void Write(IntPtr physicalAddress, byte[] buffer)
            => Write(physicalAddress, buffer, 0, buffer.Length);

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                NtClose(SectionHandle);
                disposedValue = true;
            }
        }

        ~PhysicalMemorySession()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public delegate IntPtr DuplicateHandleDelegate(IntPtr deviceHandle, uint sourceProcessId, IntPtr sourceProcessHandle, IntPtr sourceHandle, AccessMask desiredAccess, uint handleAttributes, uint options);

        public sealed class MappedPhysicalMemory : IDisposable
        {
            private readonly void* baseAddress;
            private readonly UIntPtr viewSize;
            private bool disposedValue;

            internal void* BaseAddressRaw => disposedValue ? throw new ObjectDisposedException("baseAddress") : baseAddress;
            public IntPtr BaseAddress => (IntPtr)BaseAddressRaw;

            internal UIntPtr ViewSizeRaw => disposedValue ? throw new ObjectDisposedException("viewSize") : viewSize;
            public long ViewSize => (long)ViewSizeRaw.ToUInt64();

            public void Read(byte[] buffer, int startIndex, int length)
            {
                var offset = (ulong)BaseAddress.ToInt64() & ~(PAGE_SIZE - 1);

                // Prevent access violation crash
                ThreadLocalVEH.TryCatch(
                    () => Marshal.Copy(BaseAddress.Add(offset), buffer, startIndex, length),
                    (ExceptionRecord record) => throw new MemoryAccessException($"Exception {record.ExceptionCode} occurred while reading {BaseAddress}")
                );
            }

            public byte[] Read(int length)
            {
                var buffer = new byte[length];
                Read(buffer, 0, length);
                return buffer;
            }

            public void Write(byte[] buffer, int startIndex, int length)
            {
                var offset = (ulong)BaseAddress.ToInt64() & ~(PAGE_SIZE - 1);

                // Prevent access violation crash
                ThreadLocalVEH.TryCatch(
                    () => Marshal.Copy(buffer, startIndex, BaseAddress.Add(offset), length),
                    (ExceptionRecord record) => throw new MemoryAccessException($"Exception {record.ExceptionCode} occurred while writing {BaseAddress}")
                );
            }

            public void Write(byte[] buffer)
                => Write(buffer, 0, buffer.Length);

            internal MappedPhysicalMemory(HANDLE sectionHandle, IntPtr physicalAddress, UIntPtr regionSize, PageProtections protect)
            {
                this.viewSize = regionSize;

                void* viewBase = null;
                var offset = (ulong)physicalAddress.ToInt64() & ~(PAGE_SIZE - 1);
                var viewSize = new UIntPtr(((ulong)physicalAddress.ToInt64() - offset) + (uint)regionSize);
                var ntstatus = NtMapViewOfSection((HANDLE)sectionHandle, NtCurrentProcess(), &viewBase, null, UIntPtr.Zero, &offset, &viewSize, SECTION_INHERIT.ViewUnmap, 0, protect);
                if (!ntstatus.IsSuccess())
                    throw new MemoryAccessException("MapPhysicalMemory#NtMapViewOfSection", new NtStatusException(ntstatus));

                baseAddress = viewBase;
            }

            private void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    NtUnmapViewOfSection(NtCurrentProcess(), baseAddress);
                    disposedValue = true;
                }
            }

            ~MappedPhysicalMemory()
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
}
