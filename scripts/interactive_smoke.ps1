param(
 [ValidateSet('Launch','Capture','Click','Key','Wheel')][string]$Action='Capture',
 [ValidateSet('VisualTest','ArtWindows')][string]$Game='VisualTest',
 [int]$X=0,[int]$Y=0,[ValidateSet('Left','Right')][string]$Button='Left',
 [ValidateSet('Q','W','E','R','C','P','H','Y','SPACE','UP','DOWN','LEFT','RIGHT','ENTER')][string]$Key='H',
 [ValidateRange(1,1920)][int]$Width=1280,[ValidateRange(1,1080)][int]$Height=720,
 [ValidateRange(-8,8)][int]$WheelSteps=1,
 [ValidateRange(80,3000)][int]$HoldMs=120,
 [ValidatePattern('^[a-zA-Z0-9_-]+$')][string]$Name='ui',
 [ValidateRange(0,5000)][int]$WaitMs=300
)
$ErrorActionPreference='Stop'
$project=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../src/client-unity'))
$exe=Join-Path $project $(if($Game -eq 'VisualTest'){'Builds/VisualTest/DragonMobaVisualTest.exe'}else{'Builds/ArtWindows/DragonMobaArt.exe'})
Add-Type -AssemblyName System.Drawing
Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;
using System.Threading;
public static class GameWindowInput {
 [StructLayout(LayoutKind.Sequential)] public struct Point { public int x,y; }
 [StructLayout(LayoutKind.Sequential)] public struct Rect { public int left,top,right,bottom; }
 [StructLayout(LayoutKind.Sequential)] public struct Mouse { public int dx,dy; public uint data,flags,time; public UIntPtr extra; }
 [StructLayout(LayoutKind.Sequential)] public struct Keyboard { public ushort vk,scan; public uint flags,time; public UIntPtr extra; }
 [StructLayout(LayoutKind.Explicit)] public struct Union { [FieldOffset(0)] public Mouse mouse; [FieldOffset(0)] public Keyboard keyboard; }
 [StructLayout(LayoutKind.Sequential)] public struct Input { public uint type; public Union value; }
 [DllImport("user32.dll")] public static extern bool SetProcessDPIAware();
 [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hwnd);
 [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
 [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr hwnd,int mode);
 [DllImport("user32.dll")] public static extern bool GetClientRect(IntPtr hwnd,out Rect rect);
 [DllImport("user32.dll")] static extern bool GetWindowRect(IntPtr hwnd,out Rect rect);
 [DllImport("user32.dll")] static extern int GetSystemMetrics(int index);
 [DllImport("user32.dll")] static extern bool SetWindowPos(IntPtr hwnd,IntPtr after,int x,int y,int width,int height,uint flags);
 [DllImport("user32.dll")] public static extern bool ClientToScreen(IntPtr hwnd,ref Point point);
 [DllImport("user32.dll")] public static extern bool SetCursorPos(int x,int y);
 delegate bool EnumWindowProc(IntPtr hwnd,IntPtr parameter);
 [DllImport("user32.dll")] static extern bool EnumWindows(EnumWindowProc callback,IntPtr parameter);
 [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr hwnd,out uint processId);
 [DllImport("user32.dll")] static extern bool IsWindowVisible(IntPtr hwnd);
 public static IntPtr FindWindow(int processId) {
  IntPtr found=IntPtr.Zero; long largest=0;
  EnumWindows((hwnd,parameter)=>{
   uint owner; GetWindowThreadProcessId(hwnd,out owner); Rect rect;
   if(owner==(uint)processId && IsWindowVisible(hwnd) && GetClientRect(hwnd,out rect)) {
    long area=(long)(rect.right-rect.left)*(rect.bottom-rect.top);
    if(area>largest) {largest=area;found=hwnd;}
   }
   return true;
  },IntPtr.Zero);
  return found;
 }
 public static void FitClient(IntPtr hwnd) {
  Rect client,window; Point origin=new Point();
  if(!GetClientRect(hwnd,out client)||!GetWindowRect(hwnd,out window)||!ClientToScreen(hwnd,ref origin)) return;
  int width=client.right-client.left,height=client.bottom-client.top;
  int screenWidth=GetSystemMetrics(0),screenHeight=GetSystemMetrics(1);
  if(width<=screenWidth && height<=screenHeight &&
     (origin.x<0||origin.y<0||origin.x+width>screenWidth||origin.y+height>screenHeight))
   SetWindowPos(hwnd,IntPtr.Zero,window.left-origin.x,window.top-origin.y,0,0,0x0015);
 }
 [DllImport("user32.dll",SetLastError=true)] static extern uint SendInput(uint count,Input[] inputs,int size);
 static void Send(Input input) {
  if(SendInput(1,new[]{input},Marshal.SizeOf(typeof(Input)))!=1) throw new Exception("SendInput failed: "+Marshal.GetLastWin32Error());
 }
 public static void RequireFocus(IntPtr hwnd) { if(GetForegroundWindow()!=hwnd) throw new Exception("Game is not foreground; input/capture cancelled."); }
 public static void MouseButton(IntPtr hwnd,bool right,bool up) {
  if(!up) RequireFocus(hwnd);
  var i=new Input(); i.type=0; i.value.mouse.flags=right?(up?16u:8u):(up?4u:2u); Send(i);
 }
 public static void Key(IntPtr hwnd,ushort code,bool up) {
  if(!up) RequireFocus(hwnd);
  var i=new Input(); i.type=1; i.value.keyboard.vk=code; i.value.keyboard.flags=up?2u:0u; Send(i);
 }
 public static void Wheel(IntPtr hwnd,int steps) {
  RequireFocus(hwnd); var i=new Input(); i.type=0;
  i.value.mouse.flags=0x0800; i.value.mouse.data=unchecked((uint)(steps*120)); Send(i);
 }
}
'@
[GameWindowInput]::SetProcessDPIAware() | Out-Null
$player=Get-Process ([IO.Path]::GetFileNameWithoutExtension($exe)) -ErrorAction SilentlyContinue | Where-Object { $_.Path -eq $exe } | Select-Object -First 1
if($Action -eq 'Launch' -and -not $player) {
 # Visible window is intentional: this task tests the actual user interface.
 $player=Start-Process -FilePath $exe -ArgumentList @('-screen-fullscreen','0','-screen-width',"$Width",'-screen-height',"$Height",'-logFile',('"'+(Join-Path $project ('Logs/Validation-Interactive-'+$Name+'.log'))+'"')) -WorkingDirectory $project -PassThru
 Start-Sleep -Seconds 6
}
if(-not $player){ throw 'Project player is not running.' }
$player.Refresh()
$hwnd=[GameWindowInput]::FindWindow($player.Id)
if($hwnd -eq [IntPtr]::Zero){ throw 'No game window found.' }
[GameWindowInput]::ShowWindow($hwnd,9) | Out-Null
for($attempt=0;$attempt -lt 12;$attempt++) {
 $player.Refresh(); $hwnd=[GameWindowInput]::FindWindow($player.Id)
 [GameWindowInput]::SetForegroundWindow($hwnd) | Out-Null
 if($attempt -eq 3 -and [GameWindowInput]::GetForegroundWindow() -ne $hwnd) {
  # Standard Windows activation, restricted to the verified game process.
  $activator=New-Object -ComObject WScript.Shell
  $activator.AppActivate([int]$player.Id) | Out-Null
 }
 Start-Sleep -Milliseconds 250
 if([GameWindowInput]::GetForegroundWindow() -eq $hwnd){ break }
}
[GameWindowInput]::RequireFocus($hwnd)
[GameWindowInput]::FitClient($hwnd)
Start-Sleep -Milliseconds 150
$rect=New-Object GameWindowInput+Rect
$point=New-Object GameWindowInput+Point
[GameWindowInput]::GetClientRect($hwnd,[ref]$rect) | Out-Null
[GameWindowInput]::ClientToScreen($hwnd,[ref]$point) | Out-Null
$width=$rect.right-$rect.left
$height=$rect.bottom-$rect.top
$keyDown=$false
try {
 if($Action -eq 'Click'){
  if($X -lt 0 -or $X -ge $width -or $Y -lt 0 -or $Y -ge $height){ throw 'Click outside game client.' }
  [GameWindowInput]::SetCursorPos($point.x+$X,$point.y+$Y) | Out-Null
  [GameWindowInput]::MouseButton($hwnd,($Button -eq 'Right'),$false)
  try { Start-Sleep -Milliseconds 100 } finally { [GameWindowInput]::MouseButton($hwnd,($Button -eq 'Right'),$true) }
 }
 if($Action -eq 'Key'){
  $codes=@{ENTER=13;SPACE=32;LEFT=37;UP=38;RIGHT=39;DOWN=40}
  $code=if($codes.ContainsKey($Key)){$codes[$Key]}else{[int][char]$Key}
  [GameWindowInput]::Key($hwnd,$code,$false); $keyDown=$true
  Start-Sleep -Milliseconds $HoldMs
 }
 if($Action -eq 'Wheel'){ [GameWindowInput]::Wheel($hwnd,$WheelSteps) }
 Start-Sleep -Milliseconds $WaitMs
 [GameWindowInput]::RequireFocus($hwnd)
 $bitmap=New-Object Drawing.Bitmap($width,$height)
 $graphics=[Drawing.Graphics]::FromImage($bitmap)
 try {
  $graphics.CopyFromScreen($point.x,$point.y,0,0,$bitmap.Size)
  $destination=Join-Path $project ("Logs/Interactive-"+$Name+".png")
  $bitmap.Save($destination,[Drawing.Imaging.ImageFormat]::Png)
 } finally { $graphics.Dispose(); $bitmap.Dispose() }
 [pscustomobject]@{Action=$Action;ProcessId=$player.Id;Executable=$exe;Width=$width;Height=$height;Screenshot=$destination} | ConvertTo-Json -Compress
} finally { if($keyDown){ [GameWindowInput]::Key($hwnd,$code,$true) } }

