@echo off
"%programfiles%\qemu\qemu-system-x86_64.exe" -cdrom HatchOS.iso -device VGA,vgamem_mb=256 -m 256
if %ERRORLEVEL% NEQ 0 pause