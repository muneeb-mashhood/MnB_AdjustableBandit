@echo off
set "ENV_FILE=%~dp0..\.env"
if exist "%ENV_FILE%" (
  for /f "usebackq eol=# delims=" %%A in ("%ENV_FILE%") do (
    if not "%%A"=="" set "%%A"
  )
)

if not defined GAME_DIR set "GAME_DIR=D:\SteamLibrary\steamapps\common\Mount & Blade II Bannerlord"
if not defined GAME_MODULE_DIR set "GAME_MODULE_DIR=%GAME_DIR%\Modules\AdjustableBandits"
