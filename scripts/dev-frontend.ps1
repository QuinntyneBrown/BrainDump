# Launches the Angular dev server, redirecting stdout/stderr into logs/frontend.log.
$ErrorActionPreference = 'Stop'

$root = Resolve-Path (Join-Path $PSScriptRoot '..')
$logsDir = Join-Path $root 'logs'
if (-not (Test-Path $logsDir)) { New-Item -ItemType Directory -Path $logsDir | Out-Null }

$logFile = Join-Path $logsDir 'frontend.log'
$frontend = Join-Path $root 'frontend'

Push-Location $frontend
try {
    npx ng serve brain-dump *>&1 | Tee-Object -FilePath $logFile
}
finally {
    Pop-Location
}
