@echo off
setlocal enabledelayedexpansion enableextensions

for /R %%i in (*.pwn) do (
echo Compiling %%i...
pawncc %%i -i\ -D\ -o%%~pi%%~ni.amx -r\docs\%%~ni.xml -;+ -(+ -d0 -O3
)


echo Compilation completed

endlocal