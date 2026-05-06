@echo off
setlocal

REM Click-to-run wrapper around the canonical local workflow:
REM   docker compose up
REM
REM Brings up sqlserver -> api -> web. See README.md for details.

set "REPO_ROOT=%~dp0..\.."
for %%I in ("%REPO_ROOT%") do set "REPO_ROOT=%%~fI"

REM Seed .env from the committed template on first run so Compose has a
REM password to inject. The template ships a development-only default.
if not exist "%REPO_ROOT%\.env" (
    if exist "%REPO_ROOT%\.env.example" (
        echo Creating .env from .env.example ...
        copy /Y "%REPO_ROOT%\.env.example" "%REPO_ROOT%\.env" >nul
    )
)

echo Starting BrainDump stack via docker compose ...
echo   Frontend : http://localhost:4200
echo   API      : http://localhost:5153
echo   SQL      : localhost:1433
echo.
echo Run stop.bat in this folder (or `docker compose down`) to stop everything.
echo.

pushd "%REPO_ROOT%"
docker compose up
popd

endlocal
