param(
    [string]$Sts2Root,
    [string]$GodotBin,
    [string]$DotnetBin = "dotnet",
    [string]$ModsDir,
    [switch]$SkipInstallCopy
)

$ErrorActionPreference = "Stop"

$RootDir = Split-Path -Parent $PSScriptRoot
$ProjectDir = Join-Path $RootDir "sts2-lan-connect"
$AssemblyName = "sts2_lan_connect"
$ProjectFile = Join-Path $ProjectDir "$AssemblyName.csproj"
$ManifestSource = Join-Path $ProjectDir "$AssemblyName.json"
$PckSource = Join-Path $ProjectDir "build\$AssemblyName.pck"
$DllSource = Join-Path $ProjectDir ".godot\mono\temp\bin\Debug\$AssemblyName.dll"
$TempModsDir = Join-Path $RootDir ".build_output\mods\$AssemblyName"
$GodotAppDataDir = Join-Path $RootDir ".build_output\appdata"
$GodotLocalAppDataDir = Join-Path $RootDir ".build_output\localappdata"
$GodotTempDir = Join-Path $RootDir ".build_output\tmp"

function Write-Info {
    param([string]$Message)
    Write-Host "[sts2-lan-connect] $Message"
}

function Get-SteamLibraryRoots {
    $roots = [System.Collections.Generic.List[string]]::new()

    foreach ($candidate in @($env:STS2_ROOT, $env:ProgramFiles.TrimEnd('\') + "\Steam", ${env:ProgramFiles(x86)}.TrimEnd('\') + "\Steam", $env:LOCALAPPDATA.TrimEnd('\') + "\Programs\Steam")) {
        if ($candidate -and (Test-Path $candidate)) {
            [void]$roots.Add($candidate)
        }
    }

    foreach ($regPath in @("HKCU:\Software\Valve\Steam", "HKLM:\Software\Valve\Steam", "HKLM:\Software\WOW6432Node\Valve\Steam")) {
        try {
            $steamPath = (Get-ItemProperty -Path $regPath -ErrorAction Stop).SteamPath
            if ($steamPath -and (Test-Path $steamPath)) {
                [void]$roots.Add($steamPath)
            }
        } catch {
        }

        try {
            $installPath = (Get-ItemProperty -Path $regPath -ErrorAction Stop).InstallPath
            if ($installPath -and (Test-Path $installPath)) {
                [void]$roots.Add($installPath)
            }
        } catch {
        }
    }

    $expanded = [System.Collections.Generic.List[string]]::new()
    foreach ($root in $roots | Select-Object -Unique) {
        [void]$expanded.Add($root)
        $libraryFile = Join-Path $root "steamapps\libraryfolders.vdf"
        if (-not (Test-Path $libraryFile)) {
            continue
        }

        $content = Get-Content -Path $libraryFile -Raw -Encoding UTF8
        foreach ($match in [regex]::Matches($content, '"path"\s*"([^"]+)"')) {
            $libraryPath = $match.Groups[1].Value -replace '\\\\', '\'
            if ($libraryPath -and (Test-Path $libraryPath)) {
                [void]$expanded.Add($libraryPath)
            }
        }
    }

    return $expanded | Select-Object -Unique
}

function Resolve-GameDir {
    param([string]$Candidate)

    if ($Candidate) {
        if (Test-Path (Join-Path $Candidate "data_sts2_windows_x86_64\sts2.dll")) {
            return (Resolve-Path $Candidate).Path
        }
    }

    foreach ($root in Get-SteamLibraryRoots) {
        $candidateDir = Join-Path $root "steamapps\common\Slay the Spire 2"
        if (Test-Path (Join-Path $candidateDir "data_sts2_windows_x86_64\sts2.dll")) {
            return (Resolve-Path $candidateDir).Path
        }
    }

    throw "Could not locate the Slay the Spire 2 game directory. Re-run with -Sts2Root <path>."
}

function Resolve-GodotBin {
    param([string]$Candidate)

    $commandCandidates = @(
        (Get-Command godot -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Source -ErrorAction SilentlyContinue),
        (Get-Command godot4 -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Source -ErrorAction SilentlyContinue)
    ) | Where-Object { $_ }

    foreach ($value in @(
        $Candidate,
        $env:GODOT_BIN,
        $env:GODOT4,
        $env:GODOT,
        $env:GODOT4_CONSOLE
    ) + $commandCandidates) {
        if (-not $value) {
            continue
        }

        $commandPath = (Get-Command $value -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Source -ErrorAction SilentlyContinue)
        if ($commandPath) {
            $value = $commandPath
        }

        if (-not (Test-Path $value)) {
            continue
        }

        $resolved = (Resolve-Path $value).Path
        $pathText = $resolved.ToLowerInvariant()
        $isMono = $pathText -match 'mono'
        $godotSharpDir = Join-Path ([IO.Path]::GetDirectoryName($resolved)) "GodotSharp\Api\Debug"
        if ($isMono -and -not (Test-Path $godotSharpDir)) {
            Write-Info "Skipping incomplete mono Godot candidate '$resolved' because '$godotSharpDir' is missing."
            continue
        }

        return $resolved
    }

    throw "Could not locate a Godot executable. Set GODOT_BIN/GODOT4, add Godot to PATH, or re-run with -GodotBin <path>."
}

function Assert-SupportedGodotVersion {
    param([string]$GodotExecutable)

    $versionOutput = & $GodotExecutable --version 2>$null
    if ($LASTEXITCODE -ne 0 -or -not $versionOutput) {
        throw "Could not determine Godot version from '$GodotExecutable'."
    }

    $versionText = ($versionOutput | Select-Object -First 1).Trim()
    if (-not $versionText.StartsWith("4.5.1")) {
        throw "Unsupported Godot version '$versionText'. Use Godot 4.5.1 to build a compatible PCK for Slay the Spire 2."
    }
}

$resolvedGameDir = Resolve-GameDir -Candidate $Sts2Root
$resolvedGodotBin = Resolve-GodotBin -Candidate $GodotBin
$resolvedModsDir = if ($ModsDir) { $ModsDir } else { Join-Path $resolvedGameDir "mods\$AssemblyName" }

Assert-SupportedGodotVersion -GodotExecutable $resolvedGodotBin

Write-Info "Using game dir: $resolvedGameDir"
Write-Info "Using Godot: $resolvedGodotBin"

New-Item -ItemType Directory -Force -Path $TempModsDir | Out-Null
New-Item -ItemType Directory -Force -Path $GodotAppDataDir, $GodotLocalAppDataDir, $GodotTempDir | Out-Null

$buildArgs = @(
    "build",
    $ProjectFile,
    "/p:Sts2Root=$resolvedGameDir",
    "/p:Sts2ModsDir=$TempModsDir"
)

& $DotnetBin @buildArgs
if ($LASTEXITCODE -ne 0) {
    throw "dotnet build failed."
}

try {
    $previousAppData = $env:APPDATA
    $previousLocalAppData = $env:LOCALAPPDATA
    $previousTemp = $env:TEMP
    $previousTmp = $env:TMP

    $env:APPDATA = $GodotAppDataDir
    $env:LOCALAPPDATA = $GodotLocalAppDataDir
    $env:TEMP = $GodotTempDir
    $env:TMP = $GodotTempDir

    & $resolvedGodotBin --headless --path $ProjectDir --script (Join-Path $ProjectDir "tools\build_pck.gd")
    if ($LASTEXITCODE -ne 0) {
        throw "Godot PCK build failed."
    }
}
finally {
    $env:APPDATA = $previousAppData
    $env:LOCALAPPDATA = $previousLocalAppData
    $env:TEMP = $previousTemp
    $env:TMP = $previousTmp
}

if (-not (Test-Path $DllSource)) {
    throw "Expected DLL not found: $DllSource"
}

if (-not (Test-Path $PckSource)) {
    throw "Expected PCK not found: $PckSource"
}

if (-not (Test-Path $ManifestSource)) {
    throw "Expected manifest not found: $ManifestSource"
}

if (-not $SkipInstallCopy) {
    New-Item -ItemType Directory -Force -Path $resolvedModsDir | Out-Null
    Remove-Item (Join-Path $resolvedModsDir "*.dll") -Force -ErrorAction SilentlyContinue
    Remove-Item (Join-Path $resolvedModsDir "*.pck") -Force -ErrorAction SilentlyContinue
    Remove-Item (Join-Path $resolvedModsDir "*.json") -Force -ErrorAction SilentlyContinue
    Copy-Item $DllSource -Destination $resolvedModsDir -Force
    Copy-Item $PckSource -Destination $resolvedModsDir -Force
    Copy-Item $ManifestSource -Destination $resolvedModsDir -Force
    Write-Info "MOD files copied to: $resolvedModsDir"
} else {
    Write-Info "Build complete. Install copy skipped."
}
