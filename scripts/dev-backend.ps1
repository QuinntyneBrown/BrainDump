# Launches the BrainDump.Api dev server, redirecting stdout/stderr into logs/backend.log.
$ErrorActionPreference = 'Stop'

$root = Resolve-Path (Join-Path $PSScriptRoot '..')
$logsDir = Join-Path $root 'logs'
if (-not (Test-Path $logsDir)) { New-Item -ItemType Directory -Path $logsDir | Out-Null }

$logFile = Join-Path $logsDir 'backend.log'
$project = Join-Path $root 'backend\src\BrainDump.Api'

dotnet run --project $project --launch-profile http *>&1 | Tee-Object -FilePath $logFile
