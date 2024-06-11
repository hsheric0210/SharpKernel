# SharpKernel :: Unleash Windows Kernel Capabilities in C#

<p align="center">
    <img src="logo.png" width="300px" height="auto">
</p>

>tl;dr : C# port of KDU and KDMapper

*Let's have some fun with the Windows Kernel by exploiting the vulnerable kernel drivers!*

Provides a kernel memory access using the vulnerable drivers.

Also supports additional features like unsigned kernel driver manual-mapping, spawn and kill protected process, etc.

GOAL: Support all vulnerable driver existing on this world.

* [KDMapper by TheCruz](https://github.com/TheCruZ/kdmapper)
* [KDU by hfiref0x](https://github.com/hfiref0x/kdu)
* [UC: Vulnerable Driver Megathread](https://www.unknowncheats.me/forum/anti-cheat-bypass/334557-vulnerable-driver-megathread.html)

## Anti-virus False Flags

Because of the nature of this project, anti-virus softwares MAY flag binaries and source code files as malware.

I don't have any plan to implement or integrate a anti-virus bypass for this program/library. (e.g. obfuscation, unhooking, etc.)

If your anti-virus software keeps flag SharpKernel, please add it to the exclusion list.

If you really want to bypass the AV flags, obfuscate/pack this library by yourself. (There are a plenty of useful open-source .NET obfuscators out there)

## Potential Purpose of this Library

* Learn how an each vulnerable driver can be exploited.
* Read/Write arbitrary memory addresses on your computor, without any restrictions.
* Manual map your own kernel mode driver.
* Temporarly disable DSE(Driver Signature Enforcement) to load your unsigned driver.
* Terminate/Crash arbitrary processes.
* Make your own BYOVD backdoor/rootkit.
* Nullify KASLR.
* Acheive privilege escalation by tampering the process token.

## Features

* TODO: A simple kernel-mode physical/virtual memory read-write operation interface.
* TODO: Unsigned driver loading/mapping via various methods:
    * by hooking kernel-mode functions and manual-map the driver ([KDMapper](https://github.com/TheCruZ/kdmapper))
    * by patching CI.dll!g_CiOptions to temporarily disable DSE then use NtLoadDriver ([DSEFix](https://github.com/hfiref0x/DSEFix))
* TODO: 

## How to use

```csharp
static void Main(string[] args)
{
    // Initialize the kernel session
    using (var provider = new KernelSession(new IntelNal(), new ProcExpDispatchHandlerHijack()))
    {
        var memoryAccess = provider.GetMemoryAccessor();
        memoryAccess.ReadPhysical((IntPtr)0xDEADBEEF, out byte[] bytes, 0x1000);
        Console.WriteLine(Convert.ToHexString(memoryAccess));
    }
}
```

## Supported Vulnerable Driver

### Ported from KDU:

* Intel Nal (iqvw64e.sys - CVE-2015-2291)

### Ported from KDMapper:

* Intel Nal (iqvw64e.sys - CVE-2015-2291)
