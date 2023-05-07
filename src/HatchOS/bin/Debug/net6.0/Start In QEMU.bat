@echo off
"%programfiles%\qemu\qemu-system-x86_64.exe" -cdrom HatchOS.iso -vga std -m 256 -audiodev id=dsound,driver=dsound -device AC97,audiodev=dsound -d cpu_reset -monitor stdio -M smm=off -s -S
if %ERRORLEVEL% NEQ 0 pause