@setlocal enableextensions
@cd /d "%~dp0"
%windir%\Microsoft.NET\Framework\v2.0.50727\InstallUtil.exe sus.exe
sc start Sus
pause
