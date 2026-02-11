$ErrorActionPreference = "Stop"

. "$PSScriptRoot\load-env.ps1"
Load-EnvFile (Join-Path $PSScriptRoot "..\.env")

$msbuild = if ($env:MSBUILD_PATH) { $env:MSBUILD_PATH } else { "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe" }
& $msbuild "AdjustableBandits.sln" "/t:Restore" "/t:Build" "/p:Configuration=Release" "/p:Platform=x64" "/p:RuntimeIdentifier=win-x64" "/p:RuntimeIdentifiers=win-x64"
if ($LASTEXITCODE -ne 0) {
  exit $LASTEXITCODE
}

$modRoot = if ($env:MOD_ROOT) { $env:MOD_ROOT } elseif ($env:GAME_MODULE_DIR) { $env:GAME_MODULE_DIR } else { "D:\SteamLibrary\steamapps\common\Mount & Blade II Bannerlord\Modules\AdjustableBandits" }
$binDest = Join-Path $modRoot "bin\Win64_Shipping_Client"
New-Item -ItemType Directory -Force -Path $binDest, (Join-Path $modRoot "ModuleData") | Out-Null
Copy-Item ".\_Module\SubModule.xml" -Destination (Join-Path $modRoot "SubModule.xml") -Force
Copy-Item ".\_Module\ModuleData\*" -Destination (Join-Path $modRoot "ModuleData") -Recurse -Force

function Test-FileLocked {
  param([string]$Path)
  if (-not (Test-Path $Path)) {
    return $false
  }

  try {
    $stream = [System.IO.File]::Open($Path, [System.IO.FileMode]::Open, [System.IO.FileAccess]::ReadWrite, [System.IO.FileShare]::None)
    $stream.Close()
    return $false
  }
  catch {
    return $true
  }
}

$skipCopy = $false
$sourceFiles = Get-ChildItem ".\bin\x64\Release\*" -File
foreach ($file in $sourceFiles) {
  $dest = Join-Path $binDest $file.Name
  if (Test-FileLocked $dest) {
    Write-Warning "Deploy skipped for locked file: $dest"
    $skipCopy = $true
    continue
  }

  Copy-Item $file.FullName -Destination $dest -Force
}

if ($skipCopy) {
  Write-Warning "Close Bannerlord and re-run the task to copy DLLs."
}
