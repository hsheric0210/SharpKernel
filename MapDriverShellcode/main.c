#include<stdio.h>
#include<Windows.h>
#include<intrin.h>

unsigned long long WINAPI read_msr(unsigned long msr);
void WINAPI write_msr(unsigned long msr, unsigned long long value);
unsigned long long WINAPI read_cr0();
void WINAPI write_cr0(unsigned long long value);
unsigned long long WINAPI read_cr3();
void WINAPI write_cr3(unsigned long long value);

int main()
{
    write_msr(0, read_msr(0));
    write_cr0(read_cr0());
    write_cr3(read_cr3());
    return 0;
}


unsigned long long WINAPI read_msr(unsigned long msr)
{
    return __readmsr(msr);
}

void WINAPI write_msr(unsigned long msr, unsigned long long value)
{
    __writemsr(msr, value);
}

unsigned long long WINAPI read_cr0()
{
    return __readcr0();
}

void WINAPI write_cr0(unsigned long long value)
{
    __writecr0(value);
}

unsigned long long WINAPI read_cr3()
{
    return __readcr3();
}

void WINAPI write_cr3(unsigned long long value)
{
    __writecr3(value);
}

int state_0;
