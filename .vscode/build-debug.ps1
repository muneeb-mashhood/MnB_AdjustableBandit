$ErrorActionPreference = "Stop"

. "$PSScriptRoot\load-env.ps1"
Load-EnvFile (Join-Path $PSScriptRoot "..\.env")

$msbuild = if ($env:MSBUILD_PATH) { $env:MSBUILD_PATH } else { "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe" }
& $msbuild "AdjustableBandits.sln" "/t:Restore" "/t:Build" "/p:Configuration=Debug" "/p:Platform=x64" "/p:RuntimeIdentifier=win-x64" "/p:RuntimeIdentifiers=win-x64"
if ($LASTEXITCODE -ne 0) {
  exit $LASTEXITCODE
}
