param(
    [ValidateSet('Prepare', 'PlayMode', 'Build', 'ArtImport', 'ArtBase', 'ArtForms', 'ArtScene', 'ArtPlay', 'ArtBuild', 'VisualPrepare', 'VisualPlay', 'VisualBuild')][string]$Mode = 'Prepare',
    [string]$UnityEditor
)
$ErrorActionPreference = 'Stop'
$projectPath = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../src/client-unity'))
$versionLine = Get-Content -LiteralPath (Join-Path $projectPath 'ProjectSettings/ProjectVersion.txt') | Select-Object -First 1
$version = ($versionLine -split ':', 2)[1].Trim()
if (-not $UnityEditor) { $UnityEditor = Join-Path $env:ProgramFiles "Unity/Hub/Editor/$version/Editor/Unity.exe" }
if (-not (Test-Path -LiteralPath $UnityEditor)) { throw "Unity $version not found. Supply -UnityEditor with the full editor path." }
$lockPath = Join-Path $projectPath 'Temp/UnityLockfile'
if (Test-Path -LiteralPath $lockPath) {
    try {
        $lockProbe = [IO.File]::Open($lockPath, [IO.FileMode]::Open, [IO.FileAccess]::ReadWrite, [IO.FileShare]::None)
        $lockProbe.Dispose()
    } catch { throw 'Close this Unity project before batch validation (project lock is in use).' }
}
$logDir = Join-Path $projectPath 'Logs'
New-Item -ItemType Directory -Path $logDir -Force | Out-Null
$logPath = Join-Path $logDir ("Validation-{0}.log" -f $Mode)
$methods = @{ Prepare = 'DragonMoba.Editor.ProjectValidation.Prepare'; PlayMode = 'DragonMoba.Editor.PlayModeSmoke.Run'; Build = 'DragonMoba.Editor.ProjectValidation.BuildWindows' }
$methods.ArtImport = 'DragonMoba.Editor.ArtSetup.Import'
$methods.ArtBase = 'DragonMoba.Editor.ArtSetup.CreateBase'
$methods.ArtForms = 'DragonMoba.Editor.ArtSetup.CreateForms'
$methods.ArtScene = 'DragonMoba.Editor.ArtSetup.CreateScene'
$methods.ArtPlay = 'DragonMoba.Editor.ArtSmoke.Run'
$methods.ArtBuild = 'DragonMoba.Editor.ArtSetup.Build'
$methods.VisualPrepare = 'DragonMoba.Editor.VisualReplacement.Prepare'
$methods.VisualPlay = 'DragonMoba.Editor.ArtSmoke.RunVisual'
$methods.VisualBuild = 'DragonMoba.Editor.VisualReplacement.Build'
$arguments = @('-batchmode', '-projectPath', ('"{0}"' -f $projectPath), '-logFile', ('"{0}"' -f $logPath), '-executeMethod', $methods[$Mode])
if ($Mode -notin @('PlayMode','ArtPlay','VisualPlay')) { $arguments += @('-nographics', '-quit') }
Write-Host "Unity $version | $Mode | $logPath"
$process = Start-Process -FilePath $UnityEditor -ArgumentList $arguments -WindowStyle Hidden -PassThru
if (-not $process.WaitForExit(600000)) {
    Stop-Process -Id $process.Id -ErrorAction SilentlyContinue
    throw "Unity validation exceeded 10 minutes. Inspect $logPath"
}
$process.Refresh()
$log = Get-Content -LiteralPath $logPath -Raw
$marker = @{ Prepare = 'PROJECT VALIDATION PASSED'; PlayMode = 'PLAY MODE SMOKE PASSED'; Build = 'WINDOWS BUILD PASSED' }[$Mode]
if ($Mode.StartsWith('Art') -or $Mode.StartsWith('Visual')) { $marker = $Mode.ToUpperInvariant() + ' PASSED' }
if ($process.ExitCode -ne 0 -or $log -notmatch $marker) {
    Get-Content -LiteralPath $logPath -Tail 70
    throw "Unity validation failed (exit $($process.ExitCode)). See $logPath"
}
Select-String -LiteralPath $logPath -Pattern 'PASSED|PASSOU|warning CS' | ForEach-Object { Write-Host $_.Line }
Write-Host "SUCCESS: $Mode"
