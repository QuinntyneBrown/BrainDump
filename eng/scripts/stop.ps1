# Click-to-run wrapper around `docker compose down`. Stops and removes the
# containers brought up by start.bat / `docker compose up`. Persistent
# volumes (e.g. mssql-data) are retained — pass -v to also drop them.

param(
    [switch]$VolumesToo
)

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..' '..')

Push-Location $repoRoot
try {
    if ($VolumesToo) {
        Write-Host 'Stopping BrainDump stack and removing volumes ...'
        docker compose down -v
    } else {
        Write-Host 'Stopping BrainDump stack (volumes retained) ...'
        docker compose down
    }
}
finally {
    Pop-Location
}

Write-Host 'Done.'
