param(
    [string]$GameDir,
    [string]$PackageDir = $PSScriptRoot,
    [string]$UserDataDir,
    [switch]$NoSaveSync
)

$ErrorActionPreference = "Stop"

$AssemblyName = "sts2_lan_connect"

function Write-Info {
    param([string]$Message)
    Write-Host "[sts2-lan-connect] $Message"
}

function Resolve-PackageDir {
    param([string]$Candidate)

    if (-not $Candidate) {
        throw "Package directory is empty."
    }

    $dllPath = Join-Path $Candidate "$AssemblyName.dll"
    $pckPath = Join-Path $Candidate "$AssemblyName.pck"
    $manifestPath = Join-Path $Candidate "$AssemblyName.json"
    if ((Test-Path $dllPath) -and (Test-Path $pckPath) -and (Test-Path $manifestPath)) {
        return (Resolve-Path $Candidate).Path
    }

    throw "Package directory '$Candidate' does not contain $AssemblyName.dll, $AssemblyName.pck, and $AssemblyName.json"
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
        if (Test-Path (Join-Path $Candidate "mods")) {
            return (Resolve-Path $Candidate).Path
        }
        if (Test-Path (Join-Path $Candidate "Slay the Spire 2.exe")) {
            return (Resolve-Path $Candidate).Path
        }
    }

    foreach ($root in Get-SteamLibraryRoots) {
        $candidateDir = Join-Path $root "steamapps\common\Slay the Spire 2"
        if (Test-Path $candidateDir) {
            return (Resolve-Path $candidateDir).Path
        }
    }

    throw "Could not locate the Slay the Spire 2 game directory. Re-run with -GameDir <path>."
}

function Resolve-UserDataDir {
    param([string]$Candidate)

    foreach ($path in @($Candidate, $env:STS2_USERDATA_DIR, (Join-Path $env:APPDATA "SlayTheSpire2"), (Join-Path $env:LOCALAPPDATA "SlayTheSpire2"))) {
        if ($path -and (Test-Path $path)) {
            return (Resolve-Path $path).Path
        }
    }

    return (Join-Path $env:APPDATA "SlayTheSpire2")
}

function Sync-ProfileSaves {
    param(
        [string]$PlatformName,
        [string]$UserDir,
        [System.IO.DirectoryInfo]$ProfileDir,
        [ref]$ProfilesSynced,
        [ref]$FilesCopied
    )

    $sourceSaves = Join-Path $ProfileDir.FullName "saves"
    if (-not (Test-Path $sourceSaves)) {
        return
    }

    $destProfile = Join-Path (Join-Path $UserDir "modded") $ProfileDir.Name
    $destSaves = Join-Path $destProfile "saves"

    New-Item -ItemType Directory -Force -Path $destSaves | Out-Null

    foreach ($sourceFile in Get-ChildItem -Path $sourceSaves -File -Recurse) {
        $relativePath = $sourceFile.FullName.Substring($sourceSaves.Length).TrimStart('\', '/')
        $destFile = Join-Path $destSaves $relativePath
        New-Item -ItemType Directory -Force -Path (Split-Path -Parent $destFile) | Out-Null

        if (-not (Test-Path $destFile)) {
            Copy-Item -Path $sourceFile.FullName -Destination $destFile -Force
            $FilesCopied.Value++
        }
    }

    $ProfilesSynced.Value++
}

$resolvedPackageDir = Resolve-PackageDir -Candidate $PackageDir
$resolvedGameDir = Resolve-GameDir -Candidate $GameDir
$resolvedUserDataDir = Resolve-UserDataDir -Candidate $UserDataDir

$targetModDir = Join-Path (Join-Path $resolvedGameDir "mods") $AssemblyName
New-Item -ItemType Directory -Force -Path $targetModDir | Out-Null

Write-Info "Installing mod files to: $targetModDir"
Remove-Item -Path (Join-Path $targetModDir "*.dll") -Force -ErrorAction SilentlyContinue
Remove-Item -Path (Join-Path $targetModDir "*.pck") -Force -ErrorAction SilentlyContinue
Remove-Item -Path (Join-Path $targetModDir "*.json") -Force -ErrorAction SilentlyContinue
Copy-Item -Path (Join-Path $resolvedPackageDir "$AssemblyName.dll") -Destination $targetModDir -Force
Copy-Item -Path (Join-Path $resolvedPackageDir "$AssemblyName.pck") -Destination $targetModDir -Force
Copy-Item -Path (Join-Path $resolvedPackageDir "$AssemblyName.json") -Destination $targetModDir -Force

$guideFile = Join-Path $resolvedPackageDir "STS2_LAN_CONNECT_USER_GUIDE_ZH.md"
if (Test-Path $guideFile) {
    Copy-Item -Path $guideFile -Destination $targetModDir -Force
}

if ($NoSaveSync) {
    Write-Info "Save sync skipped (-NoSaveSync)."
    exit 0
}

if (-not (Test-Path $resolvedUserDataDir)) {
    Write-Info "User data directory '$resolvedUserDataDir' does not exist yet. Installation finished without save sync."
    exit 0
}

$profilesSynced = 0
$filesCopied = 0

foreach ($platformName in @("steam", "default")) {
    $platformDir = Join-Path $resolvedUserDataDir $platformName
    if (-not (Test-Path $platformDir)) {
        continue
    }

    foreach ($userDir in Get-ChildItem -Path $platformDir -Directory) {
        foreach ($profileDir in Get-ChildItem -Path $userDir.FullName -Directory -Filter "profile*") {
            Sync-ProfileSaves -PlatformName $platformName -UserDir $userDir.FullName -ProfileDir $profileDir -ProfilesSynced ([ref]$profilesSynced) -FilesCopied ([ref]$filesCopied)
        }
    }
}

Write-Info "Save sync finished. Profiles scanned: $profilesSynced, missing files copied: $filesCopied"
Write-Info "This is a one-way sync from vanilla saves into modded saves. Existing modded files are never overwritten."
