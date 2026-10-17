param([switch]$KeepOpen)
$ErrorActionPreference='Stop'
$repo=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$project=Join-Path $repo 'src/client-unity'
$logDir=Join-Path $project 'Logs'
$exe=Join-Path $project 'Builds/VisualTest/DragonMobaVisualTest.exe'
$probeDirectory=Join-Path (Split-Path $exe -Parent) 'Logs'
$statePath=Join-Path $probeDirectory 'DBM001-state.json'
$commandPath=Join-Path $probeDirectory 'DBM001-command.txt'
$results=New-Object Collections.Generic.List[object]
function State {
 for($i=0;$i -lt 20;$i++) {
  try { return (Get-Content -LiteralPath $statePath -Raw -ErrorAction Stop | ConvertFrom-Json) }
  catch { Start-Sleep -Milliseconds 100 }
 }
 throw 'Native telemetry unavailable.'
}
function Check([bool]$ok,[string]$label) {
 $s=State
 $results.Add([pscustomobject]@{test=$label;passed=$ok;state=$s})
 Write-Output "NATIVE $(if($ok){'PASSED'}else{'FAILED'}) | $label"
 if(-not $ok){throw $label}
}
function Focus {
 for($i=0;$i -lt 8;$i++) {
  if([GameWindowInput]::GetForegroundWindow() -eq $hwnd){return}
  [GameWindowInput]::ActivateVerifiedWindow($hwnd)
  Start-Sleep -Milliseconds 150
 }
 [GameWindowInput]::RequireFocus($hwnd)
}
function Key([int]$code,[bool]$control=$false,[int]$hold=100) {
 Focus
 try {
  if($control){[GameWindowInput]::Key($hwnd,17,$false);Start-Sleep -Milliseconds 80}
  [GameWindowInput]::Key($hwnd,$code,$false)
  Start-Sleep -Milliseconds $hold
 } finally {
  [GameWindowInput]::Key($hwnd,$code,$true)
  if($control){[GameWindowInput]::Key($hwnd,17,$true)}
 }
 Start-Sleep -Milliseconds 350
}
function Click([int]$x,[int]$y,[bool]$right=$false) {
 Focus
 if($x -lt 0 -or $x -ge $width -or $y -lt 0 -or $y -ge $height){throw 'Target outside client'}
 $origin=New-Object GameWindowInput+Point
 [GameWindowInput]::ClientToScreen($hwnd,[ref]$origin) | Out-Null
 [GameWindowInput]::SetCursorPos($origin.x+$x,$origin.y+$y) | Out-Null
 Start-Sleep -Milliseconds 150
 [GameWindowInput]::MouseButton($hwnd,$right,$false)
 try{Start-Sleep -Milliseconds 100}finally{[GameWindowInput]::MouseButton($hwnd,$right,$true)}
 Start-Sleep -Milliseconds 400
}
function Fixture([string]$command) {
 [IO.File]::WriteAllText($commandPath,$command)
 for($i=0;$i -lt 40 -and (Test-Path -LiteralPath $commandPath);$i++){Start-Sleep -Milliseconds 100}
 if(Test-Path -LiteralPath $commandPath){throw 'Fixture not acknowledged'}
 Start-Sleep -Milliseconds 500
}
function Capture([string]$name) {
 Focus
 $bitmap=New-Object Drawing.Bitmap($width,$height)
 $graphics=[Drawing.Graphics]::FromImage($bitmap)
 try{$graphics.CopyFromScreen($point.x,$point.y,0,0,$bitmap.Size);$bitmap.Save((Join-Path $logDir "DBM001-$name.png"),[Drawing.Imaging.ImageFormat]::Png)}
 finally{$graphics.Dispose();$bitmap.Dispose()}
}
if(Get-Process DragonMobaVisualTest -ErrorAction SilentlyContinue | Where-Object {$_.Path -eq $exe}){throw 'Close the previous VisualTest window before native validation.'}
foreach($file in @($statePath,$commandPath)){if(Test-Path -LiteralPath $file){Remove-Item -LiteralPath $file}}
$player=Start-Process -FilePath $exe -ArgumentList @('-dbm001-native-test','-screen-fullscreen','0','-screen-width','1280','-screen-height','720','-logFile',('"'+(Join-Path $logDir 'DBM001-player.log')+'"')) -WorkingDirectory $project -PassThru
$ownedProcessId=$player.Id
try {
 Start-Sleep -Seconds 6
 # Reuse the existing PID-scoped focus guard and real Windows SendInput implementation.
 . (Join-Path $PSScriptRoot 'interactive_smoke.ps1') -Action Capture -Game VisualTest -Name DBM001-start | Out-Null
 $s=State
 Check ($s.level -eq 1 -and $s.points -eq 1 -and ($s.ranks -join ',') -eq '0,0,0,0') 'Ordinary start: one point, all skills locked'
 Capture 'locked'
 Key 81
 $s=State
 $logStream=[IO.File]::Open((Join-Path $logDir 'DBM001-player.log'),[IO.FileMode]::Open,[IO.FileAccess]::Read,[IO.FileShare]::ReadWrite)
 $reader=New-Object IO.StreamReader($logStream)
 try{$castRejected=($reader.ReadToEnd() -match 'CAST Q \| accepted=False')}finally{$reader.Dispose()}
 Check ($castRejected -and $s.ki -ge 134.9 -and $s.cooldowns[0] -eq 0) 'Physical Q rejected without Ki or cooldown cost'
 Key 81 $true
 Check ((State).ranks[0] -eq 1 -and (State).points -eq 0) 'Physical Ctrl+Q spends one point'
 # Ground target near the hero, inside cast range.
 Click 650 360
 Key 81
 $s=State
 Check ($s.ki -lt 130 -and $s.cooldowns[0] -gt 0) 'Physical Q costs Ki and starts cooldown'
 Capture 'learned-Q'
 Fixture 'level:2'
 Key 81 $true
 Check ((State).level -eq 2 -and (State).ranks[0] -eq 1 -and (State).points -eq 1) 'Level 2 rejects Q rank 2 and preserves point'
 Fixture 'level:3'
 # Actual left-click on the Q + button, not a domain command.
 Click 522 625
 Check ((State).ranks[0] -eq 2 -and (State).points -eq 1) 'Level 3 Q rank 2 via physical plus button'
 Key 82 $true
 Check ((State).ranks[3] -eq 0) 'R rejected below level 6'
 Fixture 'combat'
 Key 32 # Recenter after fixture relocation; edge scrolling can leave free camera enabled.
 $s=State
 Click ([int]$s.enemyX) ([int]$s.enemyY) $true
 Check ((State).target -eq (State).enemyId) 'Single right click on 3D enemy body acquires target'
 Start-Sleep -Seconds 5
 $s=State
 Check ($s.attacks -ge 2 -and $s.minimumAttackTicks/30.0 -ge $s.attackInterval-.04) 'Chase and repeated attacks respect attack interval'
 Key 81
 $before=(State).attacks
 Start-Sleep -Seconds 2
 Check ((State).target -eq (State).enemyId -and (State).attacks -gt $before) 'Attacks resume after physical Q without another click'
 Capture 'combat'
 Click 640 380 $true
 $before=(State).attacks
 Start-Sleep -Seconds 2
 Check ((State).target -eq 0 -and (State).attacks -eq $before) 'Ground click cancels repeated attacks'
 $s=State;Click ([int]$s.enemyX) ([int]$s.enemyY) $true
 Key 67 $false 600
 Check ((State).target -eq 0) 'Holding physical C cancels attack'
 foreach($level in @(6,11,16)) {
  Fixture "level:$level"
  Key 82 $true
  $expected=if($level -eq 6){1}elseif($level -eq 11){2}else{3}
  Check ((State).ranks[3] -eq $expected) "R upgrade at level $level"
  if($level -lt 16){Key 82 $true;Check ((State).ranks[3] -eq $expected) "Next R rank blocked at level $level"}
 }
 Capture 'level16'
 Fixture 'enemy-transform'
 # Evolved enemy model also must receive the real right-click raycast.
 $s=State;Click ([int]$s.enemyX) ([int]$s.enemyY) $true
 Check ((State).target -eq (State).enemyId -and (State).enemyForm -eq 'cell_perfect') 'Evolved enemy 3D body remains clickable after transformation'
 $savedRanks=(State).ranks -join ',';$savedPoints=(State).points
 Fixture 'death'
 Check ((State).hp -le 0) 'Controlled death enters real respawn state'
 $deathTick=(State).tick
 $deadline=[DateTime]::UtcNow.AddSeconds(120)
 while((State).hp -le 0 -and (State).tick-$deathTick -lt 42*30 -and [DateTime]::UtcNow -lt $deadline){Focus; Start-Sleep -Milliseconds 500}
 Check ((State).hp -gt 0 -and ((State).ranks -join ',') -eq $savedRanks -and (State).points -eq $savedPoints) 'Normal respawn preserves ranks and points'
 Key 27
 Click 640 655 # Confirm the selected champion in the ART-002 selection screen (1280x720).
 Check ((State).level -eq 1 -and (State).points -eq 1 -and ((State).ranks -join ',') -eq '0,0,0,0') 'Esc selection and new match reset ranks and points'
 Capture 'restart'
 Write-Output 'DBM001 NATIVE SUITE PASSED'
} finally {
 try { [IO.File]::WriteAllText((Join-Path $logDir 'DBM001-native-results.json'),(ConvertTo-Json -InputObject $results.ToArray() -Depth 6)) }
 finally { if(-not $KeepOpen){Stop-Process -Id $ownedProcessId -ErrorAction SilentlyContinue} }
}
