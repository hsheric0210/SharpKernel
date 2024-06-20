# SharpKernel :: 윈도우 커널의 기능을 C#에서 자유롭게 탐험해 보세요!

<p align="center">
    <img src="logo.png" width="300px" height="auto">
</p>

취약한 드라이버들을 이용해 Windows 커널에 직접적으로 접근하고, 마음껏 탐험해 보세요!

## 기반이 된 프로젝트들

* [hfiref0x/KDU](https://github.com/hfiref0x/kdu)
* [TheCruz/KDMapper](https://github.com/TheCruZ/kdmapper)
* [UnknownCheats: Vulnerable Driver Megathread](https://www.unknowncheats.me/forum/anti-cheat-bypass/334557-vulnerable-driver-megathread.html)

## 알림

1. 비록 이 프로젝트는 교육용, 실험적인 목적이지만, 그 외 어떤 용도로 사용하든 상관 없습니다. (루트킷 개발, 핵 개발 등) 단 명심하세요, 이에 따른 결과는 전혀 책임지지 않습니다.

2. 몇몇 제공자 또는 기능들은 OS 버전, HVCI 활성화 여부, MSDBL 활성화 여부와 같은 여러 요인들에 의해 작동하지 않을 수도 있습니다.

3. 대부분의 취약한 드라이버들은 이미 MSDBL (취약한 드라이버 차단 목록)에 의해 이미 막힌 상태입니다. 모든 제공자들을 사용하고 싶으시다면 해당 기능을 비활성화하시는 것을 추천드립니다.

4. 이 프로젝트의 실행 파일 및 DLL 파일들은 NuGet이나 GitHub의 Release 탭과 같은 그 어느 곳에도 업로드되지 않을 예정입니다. 왜냐하면 업로드 즉시 위험한 파일로 진단되어 삭제되거나, GitHub 프로젝트 사이트 자체가 위험한 사이트로 분류되어 버릴 가능성이 있기 때문입니다.

5. 대부분의 코드 구조는 [hfiref0x](https://github.com/hfiref0x) 님의 [KDU](https://github.com/hfiref0x/KDU) 프로젝트에 기반을 두고 있습니다. 제가 한 일은 코드를 C#으로 포팅하고, 몇몇 제공자를 추가하고, 몇몇 기능을 추가한 것 외에는 없습니다. 이 견고하고 안전한 코드베이스를 만들어주신 [hfiref0x](https://github.com/hfiref0x)님께 진심으로 감사드립니다.

## 안티바이러스 오진

이 프로젝트는 사실상 최근 증가하는 BYOVD 공격 (Bring Your Own Vulnerable Driver 공격)과 크게 다르지 않은 구조를 띠고 있습니다. BYOVD 공격에 대한 설명은 [이 기사](https://company.ahnlab.com/kr/news/press_release_view.do?seqPressRelease=6166)를 참고하세요.

때문에, 많은 안티바이러스 프로그램들이 이 라이브러리를 악성으로 진단하고, 삭제하려 들 것입니다.

그러나, 이 라이브러리는 실질적으로 내부에 그 어떤 '악성 행위'를 하는 코드도 담고 있지 않습니다. 때문에 이 모든 진단과 탐지는 오진이라고 볼 수 있습니다.

오진이 계속되면 라이브러리 폴더를 안티바이러스의 제외 목록에 추가해 주세요.

## 기능 목록

내장된 기능:

* IMemoryAccessProvider: 취약한 드라이버 제공자를 통해 커널 모드의 가상 메모리와 물리 메모리를 직접적으로 읽고 쓸 수 있습니다.

* ArbitraryProcessVmAccessor: 선택한 목표 프로세스의 메모리 영역을 아무런 제한 없이 읽고 쓸 수 있도록 해 줍니다. 이는 해당 프로세스의 메모리 페이지 테이블을 읽어들여 해당 프로세스의 메모리 공간에 해당되는 물리 주소를 구하고, 이 물리 주소에 직접적으로 읽기, 쓰기 작업을 수행함으로써 이루어집니다.

* DriverMapper: 커널 모드 드라이버를 매뉴얼 매핑합니다.
    * MapThenCallEntry: 드라이버 매핑 후 `DriverEntry` 함수를 직접적으로 호출합니다. (이때, `DriverEntry`가 최대한 빨리 끝나는 것이 좋습니다) (KDMapper에서 사용하는 방법)

    * MapThenStartAsSystemThread: 드라이버 매핑 후 `DriverEntry` 함수에 대해 시스템 스레드를 생성합니다. (KDU Shellcode V1에서 사용하는 방법)

    * MapThenStartAsWorkerThread: 드라이버 매핑 후 `DriverEntry` 함수에 대해 작업 스레드를 생성합니다. (KDU Shellcode V2에서 사용하는 방법)

    * MapWithDriverObject: 드라이버 매핑 시 `DriverObject`를 수동으로 생성하여 넘기고, `DriverEntry`에 대해 시스템 스레드를 생성합니다. (KDU Shellcode V3에서 사용하는 방법)

* KillProcess: 목표 프로세스를 강제로 종료시킵니다.
    * KillByIOCTL: 임의 프로세스 강제 종료를 지원하는 드라이버를 이용해 목표 프로세스를 강제 종료시킵니다.
    * KillByHandleDuplication: 임의 핸들 복제를 지원하는 드라이버를 통해 해당 목표 프로세스의 핸들을 복제한 후 강제로 종료시킵니다.
    * KillByMemoryCorruption: 해당 목표 프로세스의 메모리 공간을 쓰레기값으로 가득 채워 메모리 오류로 프로세스가 튕기도록 합니다.

* CallbackDisabler: 일시적으로 모든 커널 모드 콜백 기능들을 비활성화합니다. (예시: ObRegisterCallbacks, PsSetCreateProcessNotifyRoutine 등) 안티바이러스와 같은 보안 프로그램들의 제약을 받지 않고 마음껏 코드를 실행할 수 있습니다.

* DriverTraceCleaner: 취약한 드라이버가 로드되었던 흔적을 커널 상에서 지웁니다. KDMapper의 기능에서 옮겨왔습니다. (e.g. PiDDBCacheTable 비우기)

* DseOverwriter: 일시적으로 DSE(드라이버 서명 검증) 기능을 수정 또는 비활성화시켜 서명되지 않은 드라이버도 로드할 수 있는 상태로 만들 수 있습니다.

* PPLLauncher: 원하는 프로그램을 '보호된 프로세스(ProtectedProcess-Light)' 권한으로 실행할 수 있습니다. 이러면 웬만한 프로그램은 해당 프로세스를 종료하기는 커녕, 액세스조차 할 수 없는 상태가 됩니다.

이 기능들을 활용하면, 다음과 같은 것들도 가능합니다:

* 각 취약한 드라이버들이 어떻게 악용되는지 알아보기.
* 자작 백도어나 루트킷을 제작, 테스트해 보기.
* KASLR 무력화하기.
* 임의 프로세스의 권한 상승시키기.
* 블루스크린 띄우기.
* 커널 모드에서 셸코드 실행시키기.
* 커널 모드에서 돌아가는 핵을 제작하거나 테스트하거나, 커널 모드 안티치트를 우회하기.

## 사용 방법

```csharp
static void Main(string[] args)
{
    // Initialize the kernel session
    using (var provider = new KernelSession(
        new IntelNal(), // 제공자
        new ProcExpDispatchHandlerHijack())) // 셸코드 실행 방법
    {
        // Read arbitrary physical memory
        var memoryAccess = provider.GetMemoryAccessor();
        memoryAccess.ReadPhysical((IntPtr)0xDEADBEEF, out byte[] bytes, 0x1000);
        Console.WriteLine(Convert.ToHexString(memoryAccess));
    }
}
```

## [지원하는 제공자 목록](provider-list.md)
