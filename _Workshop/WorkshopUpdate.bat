@echo off
setlocal

call "%~dp0load-env.bat"
set "TOOL=%GAME_DIR%\bin\Win64_Shipping_Client\TaleWorlds.MountAndBlade.SteamWorkshop.exe"
set "XML=%~dp0WorkshopUpdate.xml"

if not exist "%TOOL%" (
	echo Steam Workshop tool not found:
	echo %TOOL%
	pause
	exit /b 1
)

if not exist "%XML%" (
	echo Workshop XML not found:
	echo %XML%
	pause
	exit /b 1
)

"%TOOL%" "%XML%"
pause