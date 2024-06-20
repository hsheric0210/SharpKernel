# SharpKernel :: Unleash Windows Kernel Capabilities in C#

[한국어 버전 설명서](README.ko.md)

<p align="center">
    <img src="logo.png" width="300px" height="auto">
</p>

>tl;dr : C# port of KDU and KDMapper

*Let's have some fun with the Windows Kernel by exploiting the vulnerable kernel drivers!*

Provides a kernel memory access using the vulnerable drivers.

Also supports additional features like unsigned kernel driver manual-mapping, spawn and kill protected process, etc.

GOAL: Support all vulnerable driver existing on this world.

## Based Projects

* [hfiref0x/KDU](https://github.com/hfiref0x/kdu)
* [TheCruz/KDMapper](https://github.com/TheCruZ/kdmapper)
* [UnknownCheats: Vulnerable Driver Megathread](https://www.unknowncheats.me/forum/anti-cheat-bypass/334557-vulnerable-driver-megathread.html)

## Notice

1. While this project is for educational purpose, I don't care whether you use this repository as virus, backdoor, rootkit, cheat development, or any other purpose. Always use this project at your own risk. Also, don't ask me about cheat development.

2. Some providers/features may or may not work depending on your OS version, HVCI enabled state, and MSDBL.

3. Most vulnerable drivers are blocked by MSDBL (Microsoft Vulnerable Driver Blocklist). You may want to disable that feature.

4. This project will not be uploaded to NuGet or any other package management service because they will surely flag this project as 'MALICIOUS'.

5. Nearly all code of this project is based on [hfiref0x](https://github.com/hfiref0x)'s [KDU](https://github.com/hfiref0x/KDU) project. What I have done is to port the C/C++ project to C#, and add more providers, features, and junks; that's all. All credit for the wonderful codebase should go to [him](https://github.com/hfiref0x).

## Anti-virus False Flags

Because of the nature of this project, anti-virus softwares MAY flag binaries and source code files as malware.

I don't have any plan to implement or integrate a anti-virus bypass for this program/library. (e.g. obfuscation, unhooking, etc.)

If your anti-virus software keeps flag SharpKernel, please add it to the exclusion list.

If you really want to bypass the AV flags, obfuscate/pack this library by yourself. (There are a plenty of useful open-source .NET obfuscators out there)

## Features

Integrated features:

* IMemoryAccessProvider: Read/Write kernel virtual memory or physical memory using vulnerable driver's IOCTLs.

* ArbitraryProcessVmAccessor: Read/Write arbitrary process' virtual memory without any limitations. It translates virtual address to to physical address then directly reads/writes the corresponding physical address, thus bypassing any restrictions.

* DriverMapper: Manual map your own kernel mode driver.
    * MapThenCallEntry: Map the driver then call the DriverEntry synchronously. (Make sure DriverEntry return as fast as possible) (KDMapper's method)

    * MapThenStartAsSystemThread: Map the driver then start a system thread with the DriverEntry code. (KDU Shellcode V1 method)

    * MapThenStartAsWorkerThread: Map the driver then start a worker thread with the DriverEntry code. (KDU Shellcode V2 method)

    * MapWithDriverObject: Map the driver by manually building DriverObject then start a system thread with the DriverEntry code. (KDU Shellcode V3 method)

* KillProcess: Terminate arbitrary processes, even a protected one.
    * KillByIOCTL: Kill an arbitrary process by passing its PID to vulnerable driver. (Only some of the providers which support the process kill IOCTL support this method)
    * KillByHandleDuplication: Kill an arbitrary process by obtaining a full-privileged handle to the target process leveraging the vulnerable driver. (Only some of the providers that support arbitrary handle duplication IOCTL support this method)
    * KillByMemoryCorruption: Kill an arbitrary process by completely messing up its memory regions with dummy data, causing a memory corruption crash.

* CallbackDisabler: Temporarily disable/unregister ALL kernel-mode callbacks (ObRegisterCallbacks, PsSetCreateProcessNotifyRoutine, and more). You can execute your code without being restricted/blocked by 'anti'-things (e.g. anti-malwares, anti-cheats).

* DriverTraceCleaner: Clean up all traces that can be used to track if the vulnerable driver has been loaded. (e.g. PiDDBCacheTable)

* DseOverwriter: Temporarly disable/tune DSE(Driver Signature Enforcement) to allow your unsigned driver to be loaded.
todo: [DSEFix](https://github.com/hfiref0x/DSEFix)

* PPLLauncher: Launch a program as PPL(ProtectedProcess-Light) right.

Can be utilized to:

* Learn how each vulnerable driver could be exploited.
* Make/test your own BYOVD backdoor/rootkit.
* Nullify KASLR to do more jobs or exploits.
* Achieve privilege escalation by tampering the process token. (typical Privilege Escalation)
* Crash your system.
* Run some shellcode in kernel-mode.
* Inject your own code/driver to kernel-mode and bypass some kernel-mode rootkits/shitwares (a.k.a. "anti-cheats").

## How to use

```csharp
static void Main(string[] args)
{
    // Initialize the kernel session
    using (var provider = new KernelSession(
        new IntelNal(), // Provider
        new ProcExpDispatchHandlerHijack())) // Shellcode Execution Method
    {
        // Read arbitrary physical memory
        var memoryAccess = provider.GetMemoryAccessor();
        memoryAccess.ReadPhysical((IntPtr)0xDEADBEEF, out byte[] bytes, 0x1000);
        Console.WriteLine(Convert.ToHexString(memoryAccess));
    }
}
```

## [Supported Vulnerable Driver List](provider-list.md)
