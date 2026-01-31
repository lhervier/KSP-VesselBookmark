# Teste ModLogger.ComputeLoggerName() avec plusieurs parametres.
# Executer depuis la racine du depot (ou definir $DllPath).
#
# Si la DLL depend d'Unity/KSP, definir $env:KSPDIR vers ton installation KSP
# pour que les dependances soient chargees depuis KSP_x64_Data\Managed.
#
# Les tests sont lances dans un processus enfant pour liberer la DLL a la fin
# et permettre de rebuilder sans fermer le terminal.

param([switch] $InChildProcess)

if (-not $InChildProcess) {
    $scriptPath = Join-Path $PSScriptRoot "Test-ComputeLoggerName.ps1"
    & powershell -NoProfile -ExecutionPolicy Bypass -File $scriptPath -InChildProcess
    exit $LASTEXITCODE
}

$ErrorActionPreference = "Stop"
$scriptDir = $PSScriptRoot
$projectRoot = Split-Path $scriptDir -Parent
$dllPath = Join-Path $projectRoot "Output\bin\VesselBookmarkMod.dll"

if (-not (Test-Path $dllPath)) {
    Write-Host "DLL introuvable: $dllPath" -ForegroundColor Red
    Write-Host "Compilez le projet d'abord (build.bat ou msbuild)." -ForegroundColor Yellow
    exit 1
}

# Charger les dependances Unity depuis KSP si KSPDIR est defini
$kspManaged = $null
if ($env:KSPDIR) {
    $kspManaged = Join-Path $env:KSPDIR "KSP_x64_Data\Managed"
    if (Test-Path $kspManaged) {
        [System.AppDomain]::CurrentDomain.add_AssemblyResolve({
            param($sender, $args)
            $name = $args.Name
            if ($name -like "*, *") { $name = $name.Split(",")[0].Trim() }
            $path = Join-Path $kspManaged "$name.dll"
            if (Test-Path $path) { return [System.Reflection.Assembly]::LoadFrom($path) }
            return $null
        })
    }
}

Add-Type -Path $dllPath
$type = [com.github.lhervier.ksp.bookmarksmod.ModLogger]

$testCases = @(
    "",
    "A",
    "Test",
    "BookmarkManager",
    "Exactly20Characters!",
    "UnNomDeLoggerTresLongPourTest",
    "AB",
    "     "
)

Write-Host "`n=== Test ComputeLoggerName (longueur attendue: 20) ===`n" -ForegroundColor Cyan

foreach ($case in $testCases) {
    $displayInput = if ($case -eq "") { '""' } else { $case }
    $result = $type::ComputeLoggerName($case)
    $len = $result.Length
    $preview = $result -replace " ", "_"
    $ok = $len -eq 20
    $color = if ($ok) { "Green" } else { "Red" }
    Write-Host "Entree : '$displayInput'" -ForegroundColor Gray
    Write-Host "Sortie : [$preview] (longueur $len)" -ForegroundColor $color
    Write-Host ""
}

Write-Host "=== Fin des tests ===" -ForegroundColor Cyan
