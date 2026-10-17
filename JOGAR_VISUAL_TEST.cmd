@echo off
setlocal
set "GAME=%~dp0src\client-unity\Builds\VisualTest\DragonMobaVisualTest.exe"
if not exist "%GAME%" (
  echo Build local ausente. Execute scripts\validate_unity.ps1 -Mode VisualBuild primeiro.
  exit /b 1
)
if not exist "%~dp0src\client-unity\Logs" mkdir "%~dp0src\client-unity\Logs"
start "" "%GAME%" -screen-fullscreen 0 -screen-width 1280 -screen-height 720 -logFile "%~dp0src\client-unity\Logs\Player-VisualTest.log"
