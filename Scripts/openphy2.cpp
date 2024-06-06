/*
* supOpenPhysicalMemory2
*
* Purpose:
*
* Locate and open physical memory section for read/write.
*
*/
BOOL WINAPI supOpenPhysicalMemory2(
    _In_ HANDLE DeviceHandle,
    _In_ pfnDuplicateHandleCallback DuplicateHandleCallback,
    _Out_ PHANDLE PhysicalMemoryHandle)
{
    BOOL bResult = FALSE;
    DWORD dwError = ERROR_NOT_FOUND;
    ULONG sectionObjectType = (ULONG)-1;
    HANDLE sectionHandle = NULL;
    PSYSTEM_HANDLE_INFORMATION_EX handleArray = NULL;
    UNICODE_STRING ustr;
    OBJECT_ATTRIBUTES obja;
    UNICODE_STRING usSection;

    do {

        *PhysicalMemoryHandle = NULL;

        RtlInitUnicodeString(&ustr, L"\\KnownDlls\\kernel32.dll");
        InitializeObjectAttributes(&obja, &ustr, OBJ_CASE_INSENSITIVE, NULL, NULL);

        NTSTATUS ntStatus = NtOpenSection(&sectionHandle, SECTION_QUERY, &obja);

        if (!NT_SUCCESS(ntStatus)) {
            dwError = RtlNtStatusToDosError(ntStatus);
            break;
        }

        handleArray = (PSYSTEM_HANDLE_INFORMATION_EX)supGetSystemInfo(SystemExtendedHandleInformation);
        if (handleArray == NULL) {
            dwError = ERROR_NOT_ENOUGH_MEMORY;
            break;
        }

        ULONG i;
        DWORD currentProcessId = GetCurrentProcessId();

        for (i = 0; i < handleArray->NumberOfHandles; i++) {
            if (handleArray->Handles[i].UniqueProcessId == currentProcessId &&
                handleArray->Handles[i].HandleValue == (ULONG_PTR)sectionHandle)
            {
                sectionObjectType = handleArray->Handles[i].ObjectTypeIndex;
                break;
            }
        }

        NtClose(sectionHandle);
        sectionHandle = NULL;

        if (sectionObjectType == (ULONG)-1) {
            dwError = ERROR_INVALID_DATATYPE;
            break;
        }

        RtlInitUnicodeString(&usSection, L"\\Device\\PhysicalMemory");

        for (i = 0; i < handleArray->NumberOfHandles; i++) {
            if (handleArray->Handles[i].UniqueProcessId == SYSTEM_PID_MAGIC &&
                handleArray->Handles[i].ObjectTypeIndex == (ULONG_PTR)sectionObjectType &&
                handleArray->Handles[i].GrantedAccess == SECTION_ALL_ACCESS)
            {
                HANDLE testHandle = NULL;

                if (DuplicateHandleCallback(DeviceHandle,
                    UlongToHandle(SYSTEM_PID_MAGIC),
                    NULL,
                    (HANDLE)handleArray->Handles[i].HandleValue,
                    &testHandle,
                    MAXIMUM_ALLOWED,
                    0,
                    0))
                {
                    union {
                        BYTE* Buffer;
                        POBJECT_NAME_INFORMATION Information;
                    } NameInfo;

                    NameInfo.Buffer = NULL;

                    ntStatus = supQueryObjectInformation(testHandle,
                        ObjectNameInformation,
                        (PVOID*)&NameInfo.Buffer,
                        NULL,
                        (PNTSUPMEMALLOC)supHeapAlloc,
                        (PNTSUPMEMFREE)supHeapFree);

                    if (NT_SUCCESS(ntStatus) && NameInfo.Buffer) {

                        if (RtlEqualUnicodeString(&usSection, &NameInfo.Information->Name, TRUE)) {
                            *PhysicalMemoryHandle = testHandle;
                            bResult = TRUE;
                        }

                        supHeapFree(NameInfo.Buffer);
                    }

                    if (bResult == FALSE)
                        NtClose(testHandle);
                }

                if (bResult)
                    break;

            }
        }

    } while (FALSE);

    if (sectionHandle) NtClose(sectionHandle);
    if (handleArray) supHeapFree(handleArray);

    if (bResult) dwError = ERROR_SUCCESS;

    SetLastError(dwError);
    return bResult;
}