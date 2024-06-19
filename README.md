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

## Potential Purpose of this Project

Integrated features:

* IMemoryAccessProvider: Read/Write arbitrary memory addresses on your computor, without any restrictions.
* DriverMapper: Manual map your own kernel mode driver.
* DseOverwriter: Temporarly disable DSE(Driver Signature Enforcement) to load your unsigned driver.
* DriverTraceCleaner: Clean up all traces that can be used to track if the vulnerable driver is loaded before. (e.g. PiDDBCacheTable)
* KillProcess: Terminate arbitrary processes, even a protected one.
* PPLLauncher: Launch a program as PPL(ProtectedProcess-Light) right.
* CallbackDisabler: Temporarily disable/unregister ALL kernel-mode callbacks (ObRegisterCallbacks, PsSetCreateProcessNotifyRoutine, and more) to execute your code without being restricted/blocked by anti-things (e.g. anti-malwares, anti-cheats).

Can be utilized to:

* Learn how each vulnerable driver could be exploited.
* Make/test your own BYOVD backdoor/rootkit.
* Nullify KASLR to do more jobs or exploits.
* Achieve privilege escalation by tampering the process token. (typical Privilege Escalation)
* Crash your system.
* Run some shellcode in kernel-mode.
* Inject your own code/driver to kernel-mode and bypass some kernel-mode rootkits/shitwares (a.k.a. "anti-cheats").

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
        // Read arbitrary physical memory
        var memoryAccess = provider.GetMemoryAccessor();
        memoryAccess.ReadPhysical((IntPtr)0xDEADBEEF, out byte[] bytes, 0x1000);
        Console.WriteLine(Convert.ToHexString(memoryAccess));
    }
}
```

## [Supported Vulnerable Driver List](provider-list.md)
