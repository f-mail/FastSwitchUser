@setlocal enableextensions
@cd /d "%~dp0"
sc stop Sus
%windir%\Microsoft.NET\Framework\v2.0.50727\InstallUtil.exe /u sus.exe
pause