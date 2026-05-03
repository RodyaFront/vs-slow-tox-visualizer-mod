# Build Release and zip slowtoxvisualized_<version>.zip into releases/
# Usage (from repo root): .\scripts\package-release.ps1
# Requires: VINTAGE_STORY, dotnet

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path $PSScriptRoot -Parent
$projectDir = Join-Path $repoRoot "SlowToxVisualized"
$csproj = Join-Path $projectDir "SlowToxVisualized.csproj"
$modinfoPath = Join-Path $projectDir "modinfo.json"
$buildOut = Join-Path $projectDir "bin\Release\Mods\slowtoxvisualized"
$releasesDir = Join-Path $repoRoot "releases"

if (-not (Test-Path $csproj)) { Write-Error "Project not found: $csproj" }

Write-Host "Building Release..."
dotnet build $csproj -c Release
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

if (-not (Test-Path $buildOut)) { Write-Error "Build output missing: $buildOut" }

$version = (Get-Content $modinfoPath -Raw | ConvertFrom-Json).version
if (-not $version) { Write-Error "Could not read version from modinfo.json" }

New-Item -ItemType Directory -Path $releasesDir -Force | Out-Null
$zipName = "slowtoxvisualized_$version.zip"
$zipPath = Join-Path $releasesDir $zipName
if (Test-Path $zipPath) { Remove-Item $zipPath -Force }

Write-Host "Packaging $zipPath ..."
# Mod DB requires modinfo.json at zip root (not inside slowtoxvisualized/), so archive contents only.
Compress-Archive -Path (Join-Path $buildOut "*") -DestinationPath $zipPath -Force

Write-Host "Done. $zipPath ($((Get-Item $zipPath).Length / 1KB | ForEach-Object { [math]::Round($_, 1) }) KB)"
