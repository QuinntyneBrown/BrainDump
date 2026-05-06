# Stops the brain-dump backend (5153) and frontend (4200) by killing the
# PIDs listening on those ports. Get-NetTCPConnection is more reliable
# than parsing netstat output and avoids cmd quoting issues.

$ports = @{
    5153 = 'BrainDump.Api'
    4200 = 'BrainDump.Web'
}

foreach ($port in $ports.Keys) {
    $label = $ports[$port]
    $owners = Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue |
              Select-Object -ExpandProperty OwningProcess -Unique

    if (-not $owners) {
        Write-Host "No process listening on port $port ($label)."
        continue
    }

    foreach ($id in $owners) {
        Write-Host "Stopping $label (PID $id on port $port)..."
        Stop-Process -Id $id -Force -ErrorAction SilentlyContinue
    }
}

Write-Host 'Done.'
