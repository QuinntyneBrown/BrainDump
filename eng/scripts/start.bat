@echo off
setlocal

REM Launches the brain-dump backend and frontend in two separate console windows
REM so the API and Angular dev server can run in parallel. Each window's title
REM matches what stop.bat looks for, so the two scripts are paired.

set "REPO_ROOT=%~dp0..\.."
for %%I in ("%REPO_ROOT%") do set "REPO_ROOT=%%~fI"

set "BACKEND_TITLE=BrainDump.Api"
set "FRONTEND_TITLE=BrainDump.Web"

echo Starting BrainDump backend (.NET API on http://localhost:5153)...
start "%BACKEND_TITLE%" cmd /k "cd /d ""%REPO_ROOT%\backend\src\BrainDump.Api"" && dotnet run --launch-profile http"

echo Starting BrainDump frontend (Angular dev server on http://localhost:4200)...
start "%FRONTEND_TITLE%" cmd /k "cd /d ""%REPO_ROOT%\frontend"" && npm start"

echo.
echo Both processes are starting in separate windows.
echo   Backend  : http://localhost:5153
echo   Frontend : http://localhost:4200
echo.
echo Run stop.bat in this folder to terminate both.

endlocal
