# Deploy SlowTox Visualized: dotnet build Release + copy FULL mod folder into Vintage Story Mods.
# Copies bin/Release/Mods/slowtoxvisualized/* -> destination (flatten), never nests slowtoxvisualized/slowtoxvisualized.
#
# Destination:
#   $env:VS_MODS_TEST\slowtoxvisualized   if VS_MODS_TEST is set (see docs/DEV_ENV.md),
#   else %AppData%\Roaming\VintagestoryData\Mods\slowtoxvisualized
#
# Usage (from repo root):
#   .\scripts\deploy-slowtoxvisualized.ps1
# Requires: VINTAGE_STORY for dotnet build (same as .csproj).

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path $PSScriptRoot -Parent
$projectDir = Join-Path $repoRoot "SlowToxVisualized"
$csproj = Join-Path $projectDir "SlowToxVisualized.csproj"
$buildOut = Join-Path $projectDir "bin\Release\Mods\slowtoxvisualized"

if (-not (Test-Path $csproj)) {
    Write-Error "Project not found: $csproj"
}

$dstRoot = if ($env:VS_MODS_TEST -and $env:VS_MODS_TEST.Trim().Length -gt 0) {
    Join-Path $env:VS_MODS_TEST.TrimEnd('\', '/') "slowtoxvisualized"
} else {
    Join-Path $env:APPDATA "VintagestoryData\Mods\slowtoxvisualized"
}

Write-Host "Building Release..."
dotnet build $csproj -c Release
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

if (-not (Test-Path $buildOut)) {
    Write-Error "Build output missing: $buildOut"
}

Write-Host "Deploying to: $dstRoot"
New-Item -ItemType Directory -Path $dstRoot -Force | Out-Null
Remove-Item -Path (Join-Path $dstRoot "*") -Recurse -Force -ErrorAction SilentlyContinue
Copy-Item -Path (Join-Path $buildOut "*") -Destination $dstRoot -Recurse -Force

$modinfo = Join-Path $dstRoot "modinfo.json"
if (Test-Path $modinfo) {
    $v = (Get-Content $modinfo -Raw | ConvertFrom-Json).version
    Write-Host "Done. modinfo version: $v"
} else {
    Write-Host "Done."
}
