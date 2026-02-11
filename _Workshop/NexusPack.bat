@echo off
setlocal

call "%~dp0load-env.bat"
set "REPO_ROOT=%~dp0.."
set "SUBMODULE_XML=%REPO_ROOT%\_Module\SubModule.xml"
set "OUT_DIR=%REPO_ROOT%\_Workshop"

if not exist "%GAME_MODULE_DIR%" (
  echo Module folder not found:
  echo %GAME_MODULE_DIR%
  pause
  exit /b 1
)

if not exist "%SUBMODULE_XML%" (
  echo SubModule.xml not found:
  echo %SUBMODULE_XML%
  pause
  exit /b 1
)

for /f "usebackq delims=" %%V in (`powershell -NoProfile -Command "$v=(Select-Xml -Path '%SUBMODULE_XML%' -XPath '/Module/Version').Node.value; if ($v.StartsWith('v')) { $v=$v.Substring(1) }; Write-Output $v"`) do set "VERSION=%%V"

if "%VERSION%"=="" (
  echo Failed to read version from SubModule.xml
  pause
  exit /b 1
)

set "ZIP_NAME=AdjustableBandits_v%VERSION%.zip"
set "ZIP_PATH=%OUT_DIR%\%ZIP_NAME%"

powershell -NoProfile -Command "Compress-Archive -Path '%GAME_MODULE_DIR%\*' -DestinationPath '%ZIP_PATH%' -Force"

echo Created: %ZIP_PATH%
pause
