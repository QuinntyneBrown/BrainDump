@echo off
REM Thin wrapper — see stop.ps1 for the actual logic.
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0stop.ps1"
