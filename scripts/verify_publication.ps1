param([string]$DomainBaseline)
$ErrorActionPreference = 'Stop'
$repo = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$project = Join-Path $repo 'src/client-unity'
$paths = @(git -c "safe.directory=$($repo.Replace('\','/'))" -C $repo ls-files --cached --others --exclude-standard | Sort-Object -Unique)
if ($LASTEXITCODE -ne 0) { throw 'Cannot inventory Git publication candidates.' }
$required = @('AGENTS.md','docs/coordination/TO_CODEX.md','docs/coordination/PROJECT_STATUS.md',
    'docs/specifications/GAME_DESIGN.md','docs/specifications/TECH_ARCHITECTURE.md',
    'docs/specifications/ART_PIPELINE.md','docs/specifications/CODEX_WORKFLOW.md',
    'src/client-unity/Assets/_DragonMOBA/Scenes/VisualTest/ArenaVisualTest.unity',
    'src/client-unity/Packages/manifest.json','src/client-unity/Packages/packages-lock.json',
    'src/client-unity/ProjectSettings/ProjectVersion.txt')
foreach ($path in $required) { if ($path -notin $paths) { throw "Missing publication source: $path" } }
$forbidden = '(^|/)(Library|Temp|Logs|Builds?|Backups|UserSettings|ExternalAssets|Research|secrets|\.git)(/|$)|\.(zip|7z|rar|unitypackage|exe|dll|pdb|pem|pfx|p12|key)$|(^|/)\.env($|\.)'
$privateContent = '(?i)C:[\\/]+Users[\\/]+|/Users/|/home/|-----BEGIN (?:RSA |EC |OPENSSH )?PRIVATE KEY-----|gh[pousr]_[A-Za-z0-9]{30,}|github_pat_[A-Za-z0-9_]{40,}|AKIA[0-9A-Z]{16}|xox[baprs]-[A-Za-z0-9-]{20,}|sk-proj-[A-Za-z0-9_-]{20,}'
$textExtensions = @('.md','.json','.csv','.txt','.html','.jsonl','.yaml','.yml','.ps1','.py','.cs','.asset','.unity','.prefab','.meta','.shader','.mat','.controller','.cmd')
$totalBytes = 0L
$metaCount = 0
foreach ($relative in $paths) {
    # Public, vetted research documentation is allowed; local Research directories
    # outside docs/research remain forbidden publication candidates.
    if ($relative -match $forbidden -and $relative -notmatch '^docs/research/' -and
        $relative -notmatch '(^|/)\.env\.example$') { throw "Forbidden publication path: $relative" }
    $full = Join-Path $repo $relative
    $file = Get-Item -LiteralPath $full
    $totalBytes += $file.Length
    if ($file.Length -gt 50MB) { throw "Large file needs separate review: $relative" }
    if ($file.Extension -in $textExtensions -and $relative -ne 'scripts/verify_publication.ps1') {
        if ([IO.File]::ReadAllText($full) -match $privateContent) { throw "Potential private content; inspect locally: $relative" }
    }
    if ($relative -like 'src/client-unity/Assets/*' -and $file.Extension -ne '.meta' -and -not $file.Name.StartsWith('.')) {
        if (($relative + '.meta') -notin $paths) { throw "Missing versioned meta: $relative" }
    }
    if ($relative -like 'src/client-unity/Assets/*.meta') { $metaCount++ }
}
Get-Content -LiteralPath (Join-Path $project 'Packages/manifest.json') -Raw | ConvertFrom-Json | Out-Null
Get-Content -LiteralPath (Join-Path $project 'Packages/packages-lock.json') -Raw | ConvertFrom-Json | Out-Null
$imported = @(Import-Csv -LiteralPath (Join-Path $project 'docs/art/IMPORTED_ASSETS.csv'))
foreach ($asset in $imported) {
    if ($asset.license -ne 'CC0') { throw "Asset license requires review: $($asset.path)" }
    if ((Get-FileHash -LiteralPath (Join-Path $project $asset.path) -Algorithm SHA256).Hash -ne $asset.sha256) { throw "Imported asset changed: $($asset.path)" }
}
if (-not $DomainBaseline) {
    $candidates = @(
        (Join-Path $repo 'docs/coordination/evidence/ART-002/DOMAIN_BASELINE.csv'),
        (Join-Path $repo 'docs/coordination/evidence/DBM-BATCH-001/DOMAIN_BASELINE.csv'),
        (Join-Path $repo 'docs/coordination/evidence/DBM-002/DOMAIN_BASELINE.csv'),
        (Join-Path $repo 'docs/coordination/evidence/DBM-001/DOMAIN_BASELINE.csv'),
        (Join-Path $project 'docs/art/DOMAIN_BASELINE.csv')
    )
    $DomainBaseline = $candidates | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
}
foreach ($baseline in Import-Csv -LiteralPath $DomainBaseline) {
    $fileName = Split-Path $baseline.Path -Leaf
    if ((Get-FileHash -LiteralPath (Join-Path $project "Assets/Scripts/Domain/$fileName") -Algorithm SHA256).Hash -ne $baseline.Hash) { throw "Domain changed: $fileName" }
}
$licenseFiles = @(Get-ChildItem -LiteralPath (Join-Path $project 'Assets/_DragonMOBA/Art') -Recurse -Filter 'LICENSE*.txt' -File)
if ($licenseFiles.Count -ne 7) { throw 'Expected seven included asset license notices.' }
foreach ($license in $licenseFiles) {
    if ([IO.File]::ReadAllText($license.FullName) -notmatch 'CC0') { throw "Review license notice: $($license.Name)" }
}
Write-Output "PUBLICATION CHECK PASSED | files=$($paths.Count) | bytes=$totalBytes | metas=$metaCount | verifiedCC0Files=$($imported.Count) | licenses=$($licenseFiles.Count) | domain=$(@(Import-Csv -LiteralPath $DomainBaseline).Count) baseline verified"
Write-Output 'Scope: source inventory, common private-content patterns, size limits, file metas, package JSON, included CC0 notices and imported hashes. This is not a comprehensive security audit or a gameplay test.'
