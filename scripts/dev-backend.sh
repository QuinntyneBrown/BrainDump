#!/usr/bin/env bash
# Launches the BrainDump.Api dev server, redirecting stdout/stderr into logs/backend.log.
set -euo pipefail

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
root="$(cd "$script_dir/.." && pwd)"
logs_dir="$root/logs"
mkdir -p "$logs_dir"

dotnet run --project "$root/backend/src/BrainDump.Api" --launch-profile http 2>&1 | tee "$logs_dir/backend.log"
