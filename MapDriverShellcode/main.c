#include<stdio.h>
#include<Windows.h>

unsigned char shellcode1[] =
"\x48\x81\xEC\x58\x00\x00\x00\x48\x89\xCF\x48\xB8\xEF\xBE\xAD\xDE\x00\x00\x00\x00\x49\x89\xC3\x48\x8B\x47\x28\x48\x89\x44\x24\x48\x48\x8B\x47\x20\x48\x89\x44\x24\x40\x8B\x47\x1C\x89\x44\x24\x38\x0F\xB7\x47\x1A\x66\x89\x44\x24\x30\x0F\xB6\x47\x18\x88\x44\x24\x28\x48\x8B\x47\x10\x48\x89\x44\x24\x20\x4C\x8B\x4F\x08\x44\x8B\x47\x04\x0F\xB7\x57\x02\x0F\xB6\x4F\x00\x4C\x89\xD8\xFF\xD0\x48\x81\xC4\x58\x00\x00\x00\xC3";

unsigned char shellcode2[] =
"\x48\x81\xEC\x38\x00\x00\x00\x48\x89\xCF\x48\xB8\xEF\xBE\xAD\xDE\x00\x00\x00\x00\x49\x89\xC3\x0F\xB6\x47\x16\x88\x44\x24\x20\x44\x0F\xB7\x4F\x14\x44\x8B\x47\x10\x48\x8B\x57\x08\x48\x8B\x4F\x00\x4C\x89\xD8\xFF\xD0\x48\x81\xC4\x38\x00\x00\x00\xC3";

unsigned char shellcode3[] =
"\x48\x81\xEC\x28\x00\x00\x00\x48\x89\xCF\x48\xB8\xEF\xBE\xAD\xDE\x00\x00\x00\x00\x49\x89\xC3\x44\x0F\xB7\x4F\x12\x44\x0F\xB6\x47\x10\x48\x8B\x57\x08\x48\x8B\x4F\x00\x4C\x89\xD8\xFF\xD0\x48\x81\xC4\x28\x00\x00\x00\xC3 ";

typedef struct _TEST
{
    __int8 i1;
    __int16 i2;
    __int32 i4;
    __int64 i8;
    int *ptra;
    __int8 j1;
    __int16 j2;
    __int32 j4;
    __int64 j8;
    int *ptrb;
} TEST, *PTEST;

typedef struct _TEST2
{
    int *ptra;
    __int64 i8;
    __int32 i4;
    __int16 i2;
    __int8 i1;
} TEST2, *PTEST2;

typedef struct _TEST3
{
    __int64 i8;
    int *ptra;
    __int8 i1;
    __int16 i2;
} TEST3, *PTEST3;

int __stdcall t1_target(__int8 i1, __int16 i2, __int32 i4, __int64 i8, int *ptr, __int8 i1_2, __int16 i2_2, __int32 i4_2, __int64 i8_2, int *ptr_2)
{
    printf("%d %d %d %lld %p %d %d %d %lld %p\n", i1, i2, i4, i8, ptr, i1_2, i2_2, i4_2, i8_2, ptr_2);
    return 123;
}

int __stdcall t1_f(PTEST param)
{
    return t1_target(param->i1, param->i2, param->i4, param->i8, param->ptra, param->j1, param->j2, param->j4, param->j8, param->ptrb);
}

int __stdcall t2_target(int *ptr, __int64 i8, __int32 i4, __int16 i2, __int8 i1)
{
    printf("%d %d %d %lld %p\n", i1, i2, i4, i8, ptr);
    return 123;
}

int __stdcall t2_f(PTEST2 param)
{
    return t2_target(param->ptra, param->i8, param->i4, param->i2, param->i1);
}

int __stdcall t3_target(__int64 i8, int *ptr, __int8 i1, __int16 i2)
{
    printf("%lld %p %d %d\n", i8, ptr, i1, i2);
    return 123;
}

int __stdcall t3_f(PTEST3 param)
{
    return t3_target(param->i8, param->ptra, param->i1, param->i2);
}

int main()
{
    PTEST mem = VirtualAlloc(0, 0x1000, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
    printf("offsets: %lld %lld %lld %lld %lld\n", offsetof(TEST, i1), offsetof(TEST, i2), offsetof(TEST, i4), offsetof(TEST, i8), offsetof(TEST, ptra));
    TEST tv = { 11, 222, 4444, 88888, 0x123456, 1, 22, 444, 8888, 0xdeadbeef };
    memcpy(mem, &tv, sizeof(TEST));
    t1_f(mem);

    TEST2 tv2 = { 0xDEADDEAD, 64646464, 3232, 16, 8 };
    memcpy(mem, &tv2, sizeof(TEST2));
    t2_f(mem);

    TEST3 tv3 = { 6464646464, 0xbeef, 88, 1612 };
    memcpy(mem, &tv3, sizeof(TEST3));
    t3_f(mem);

    void *exec = VirtualAlloc(0, 0x1000, MEM_COMMIT, PAGE_EXECUTE_READWRITE);

    RtlSecureZeroMemory(exec, 0x1000);
    *(int **)(shellcode1 + 12) = &t1_target;
    printf("T1 shellcode (@ %p, alloc %p) (target: %p)\n", &shellcode1, exec, &t1_target);
    memcpy(mem, &tv, sizeof(TEST));
    memcpy(exec, shellcode1, sizeof(shellcode1));
    ((int(*)())exec)(mem);

    RtlSecureZeroMemory(exec, 0x1000);
    *(int **)(shellcode2 + 12) = &t2_target;
    printf("T2 shellcode (@ %p, alloc %p) (target: %p)\n", &shellcode2, exec, &t2_target);
    memcpy(mem, &tv2, sizeof(TEST2));
    memcpy(exec, shellcode2, sizeof(shellcode2));
    int ret = ((int(*)())exec)(mem);
    printf("T2 return value %d\n", ret);

    RtlSecureZeroMemory(exec, 0x1000);
    *(int **)(shellcode3 + 12) = &t3_target;
    printf("T3 shellcode (@ %p, alloc %p) (target: %p)\n", &shellcode3, exec, &t3_target);
    memcpy(mem, &tv3, sizeof(TEST3));
    memcpy(exec, shellcode3, sizeof(shellcode3));
    ((int(*)())exec)(mem);

    VirtualFree(exec, 0, MEM_RELEASE);

    return 0;
}

