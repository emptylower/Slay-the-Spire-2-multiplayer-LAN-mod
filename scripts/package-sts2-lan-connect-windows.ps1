param(
    [string]$Sts2Root,
    [string]$GodotBin,
    [string]$DotnetBin = "dotnet"
)

$ErrorActionPreference = "Stop"

$RootDir = Split-Path -Parent $PSScriptRoot
$AssemblyName = "sts2_lan_connect"
$ProjectDir = Join-Path $RootDir "sts2-lan-connect"
$PackageRootParent = Join-Path $RootDir "local-release"
$PackageRoot = Join-Path $PackageRootParent $AssemblyName
$ManifestPath = Join-Path $ProjectDir "$AssemblyName.json"
$DllFile = Join-Path $ProjectDir ".godot\mono\temp\bin\Debug\$AssemblyName.dll"
$PckFile = Join-Path $ProjectDir "build\$AssemblyName.pck"
$Manifest = Get-Content $ManifestPath -Raw -Encoding UTF8 | ConvertFrom-Json
$ReleaseVersion = "v$($Manifest.version)"
$ZipPath = Join-Path $PackageRootParent "$AssemblyName-$ReleaseVersion-windows.zip"
$GuidePath = Join-Path $RootDir "docs\STS2_LAN_CONNECT_USER_GUIDE_ZH.md"

& (Join-Path $PSScriptRoot "build-sts2-lan-connect-windows.ps1") `
    -Sts2Root $Sts2Root `
    -GodotBin $GodotBin `
    -DotnetBin $DotnetBin `
    -SkipInstallCopy

New-Item -ItemType Directory -Force -Path $PackageRootParent | Out-Null

if (Test-Path $PackageRoot) {
    Remove-Item $PackageRoot -Recurse -Force
}

New-Item -ItemType Directory -Force -Path $PackageRoot | Out-Null
Copy-Item $DllFile -Destination $PackageRoot -Force
Copy-Item $PckFile -Destination $PackageRoot -Force
Copy-Item $ManifestPath -Destination $PackageRoot -Force
Copy-Item (Join-Path $RootDir "RELEASE_README.md") -Destination (Join-Path $PackageRoot "README.md") -Force
Copy-Item $GuidePath -Destination (Join-Path $PackageRoot "STS2_LAN_CONNECT_USER_GUIDE_ZH.md") -Force
Copy-Item (Join-Path $RootDir "scripts\install-sts2-lan-connect-linux.sh") -Destination $PackageRoot -Force
Copy-Item (Join-Path $RootDir "scripts\install-sts2-lan-connect-macos.sh") -Destination $PackageRoot -Force
Copy-Item (Join-Path $RootDir "scripts\install-sts2-lan-connect-windows.ps1") -Destination $PackageRoot -Force
Copy-Item (Join-Path $RootDir "scripts\install-sts2-lan-connect-windows.bat") -Destination $PackageRoot -Force

Get-ChildItem -Path $PackageRootParent -Filter "$AssemblyName*-windows.zip" -ErrorAction SilentlyContinue | ForEach-Object {
    Remove-Item $_.FullName -Force
}

Compress-Archive -Path (Join-Path $PackageRoot "*") -DestinationPath $ZipPath -CompressionLevel Optimal
Write-Host "Package created at: $ZipPath"
