# Provider List

## Kernel memory access

|FileName|Name|Assigned CVE|Source Base|Physical Memory R/W|Virtual Memory R/W|MSR R/W|Original Source Available In|
|:---|:---|:---:|:---:|:---:|:---:|:---:|:---:|
|iqvw64.sys|Intel Network Adapter Diagnostic Driver|CVE-2015-2291||O|O||KDU, KDMapper|
|RTCore64.sys|Micro-Star MSI Afterburner|CVE-2019-16098||X|O||KDU|
|GDrv.sys||CVE-2018-19320|MapMem|O|O (Virtual-to-Physical translation)||KDU|
|ATSZIO64.sys|ASUSTeK WinFlash|CVE-2024-33222||O|O (Virtual-to-Physical translation)||KDU|
|MsIo64.sys||CVE-2019-18845|WinIo||||KDU|
|GLCKIO2.sys|ASRock Polychrome RGB|CVE-2018-18535, CVE-2018-18536, CVE-2018-18537|WinIo||||KDU|
|EneIo64.sys|G.SKILL Trident Z Lighting Control|CVE-2020-12446|WinIo||||KDU|
|WinRing0x64.sys|EVGA Precision X1|CVE-2020-14979|WinRing0||||KDU|
|EneTechIo64.sys|Thermaltake TOUGHRAM Software||WinIo||||KDU|
|phymemx64.sys|Huawei MateBook Manager||WinIo||||KDU|
|rtkio64.sys|Realtek Dash Client Utility|CVE-2024-33224|PhyMem||||KDU|
|ene.sys (EneTechIo64.sys)|MSI Dragon Center||WinIo||||KDU|
|lha.sys||CVE-2019-8372|||||KDU|
|AsIO2.sys|ASUS GPU Tweak|CVE-2021-28685|WinIo||||KDU|
|DirectIo64.sys|PassMark DirectIO|CVE-2020-15479, CVE-2020-15480|||||KDU|
|gmer64.sys|Gmer Antirootkit||||||KDU|
|dbutil_2_3.sys||CVE-2021-21551|||||KDU|
|mimidrv.sys|Mimikatz mimidrv||||||KDU|
|kprocesshacker.sys|Process Hacker 2||||||KDU|
|procexp152.sys|Process Explorer b1627||||||KDU|
|dbutildrv2.sys||CVE-2021-36276|||||KDU|
|dbk64.sys|Cheat Engine Dbk64||||||KDU|
|AsIO3.sys|ASUS GPU Tweak II||WinIo||||KDU|
|HW64.sys|Marvin Hardware Access Driver for Windows|CVE-2024-36054, CVE-2024-36055|||||KDU|
|SysDrv3S.sys|CODESYS SysDrv3S|CVE-2022-22516|MapMem||||KDU|
|amsdk.sys|Zemana AntiMalware|CVE-2021-31728, CVE-2022-42045|||||KDU|
|inpoutx64.sys|inpoutx64 Driver Version 1.2||||||KDU|
|DirectIo64.sys|PassMark OSForensics DirectIO|CVE-2020-15479, CVE-2020-15480|||||KDU|
|AsrDrv106.sys|ASRock IO Driver|CVE-2020-15368|RWEverything||||KDU|
|ALSysIO64.sys|Core Temp||||||KDU|
|AMDRyzenMasterDriver.sys|AMD Ryzen Master Service Driver|CVE-2020-12928|||||KDU|
|physmem.sys|Physical Memory Access Driver||||||KDU|
|LenovoDiagnosticsDriver.sys|Lenovo Diagnostics Driver for Windows 10 and later|CVE-2022-3699|||||KDU|
|pcdsrvc_x64.sys|PC-Doctor|CVE-2019-12280|||||KDU|
|WinIo64.sys|MSI Foundation Service||WinIo||||KDU|
|etdsupp.sys|ETDi Support Driver|CVE-2023-32673|||||KDU|
|KExplore.sys|MSI Foundation Service||Pavel Yosifovich (zodiacon)||||KDU|
|KObjExp.sys|Kernel Object Explorer Driver||Pavel Yosifovich (zodiacon)||||KDU|
|KExplore.sys|Kernel Explorer Driver||Pavel Yosifovich (zodiacon)||||KDU|
|KRegExp.sys|Kernel Registry Explorer Driver||Pavel Yosifovich (zodiacon)||||KDU|
|echo_driver.sys|Echo AntiCheat|CVE-2023-38817|||||KDU|
|nvoclock.sys|NVidia System Utility Driver||||||KDU|
|irec.sys|Binalyze (re-verify needed)|CVE-2023-41444|||||KDU|
|PhyDMACC.sys|SLIC ToolKit||WinRing0||||KDU|
|rzpnk.sys|Razer Overlay Support driver|CVE-2017-9769|||||KDU|
|PdFwKrnl.sys|AMD USB-C Power Delivery Firmware Update Utility|CVE-2023-20598|||||KDU|
|AODDriver215.sys|AMD OverDrive Driver|CVE-2020-12928|||||KDU|
|wnBios64.sys|WnBios Driver||||||KDU|
|eleetx1.sys|EVGA Low Level Driver||||||KDU|
|AxtuDrv.sys|RW-Everything Read & Write Driver|CVE-2020-15368|RWEverything||||KDU|
|AppShopDrv103.sys|AppShopDrv103 Driver|CVE-2020-15368|RWEverything||||KDU|
|AsrDrv107n.sys|ASRock IO Driver|CVE-2020-15368|RWEverything||||KDU|
|AsrDrv107.sys|ASRock IO Driver|CVE-2020-15368|RWEverything||||KDU|
|pmxdrv64.sys|Intel(R) Management Engine Tools Driver||||||KDU|
|ntguard_x64.sys||CVE-2017-14961||||||
|WinRing0x64.sys||CVE-2023-1047||||||
|mhyprot2.sys||CVE-2020-36603||||||
|ucorew64.sys||||||||
|zam64.sys||||||||
|mydrivers64.sys||CVE-2023-1679||||||
|IMFCameraProtect.sys||CVE-2023-1646||||||

## Kernel process access

|FileName|Name|Category|Assigned CVE|Kill Arbitrary Process|
|:---|:---:|:---:|:---:|:---:|
