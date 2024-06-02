# SharpKernel :: Unleash Windows Kernel Capabilities in C#

>tl;dr : C# port of KDU and KDMapper

*Let's have some fun with the Windows Kernel by exploiting the vulnerable kernel drivers!*

> [!IMPORTANT]
> Because of the nature of this project, anti-virus softwares may flag binaries and source codes.
> I WILL NOT IMPLEMENT AN ANTI-VIRUS BYPASS ON THIS PROGRAM. If your anti-virus program does flag SharpKernel, please add it to the exclusion list, or obfuscator/encrypt this library by yourself. (There are a lot of open-source .NET obfuscator)

Provides a kernel memory access using the vulnerable drivers.

Also supports additional features like unsigned kernel driver manual-mapping, spawn and kill protected process, etc.

GOAL: Support all vulnerable driver existing on this world.

* [KDMapper by TheCruz](https://github.com/TheCruZ/kdmapper)
* [KDU by hfiref0x](https://github.com/hfiref0x/kdu)
* [UC: Vulnerable Driver Megathread](https://www.unknowncheats.me/forum/anti-cheat-bypass/334557-vulnerable-driver-megathread.html)

## Features

* TODO: A simple kernel-mode physical/virtual memory read-write operation interface.
* TODO: Unsigned driver loading/mapping via various methods:
    * by hooking kernel-mode functions and manual-map the driver ([KDMapper](https://github.com/TheCruZ/kdmapper))
    * by patching CI.dll!g_CiOptions to temporarily disable DSE then use NtLoadDriver ([DSEFix](https://github.com/hfiref0x/DSEFix))
* TODO: 

## How to use

```csharp

```

## Supported Vulnerable Driver

* Intel Nal (iqvw64e.sys - CVE-2015-2291)
