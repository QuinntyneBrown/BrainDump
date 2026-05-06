#!/usr/bin/env bash
# Launches the Angular dev server, redirecting stdout/stderr into logs/frontend.log.
set -euo pipefail

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
root="$(cd "$script_dir/.." && pwd)"
logs_dir="$root/logs"
mkdir -p "$logs_dir"

cd "$root/frontend"
npx ng serve brain-dump 2>&1 | tee "$logs_dir/frontend.log"
