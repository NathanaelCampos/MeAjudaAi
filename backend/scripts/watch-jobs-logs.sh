#!/usr/bin/env bash
set -euo pipefail

SERVICE=watch-jobs.service
LOG=./logs/watch-jobs.log

if ! command -v journalctl >/dev/null 2>&1; then
  echo "journalctl not available" >&2
  exit 1
fi

if [[ ! -f "$LOG" ]]; then
  mkdir -p "$(dirname "$LOG")"
  touch "$LOG"
fi

journalctl -fu "$SERVICE" &
JID=$!

tail -f "$LOG" &
TID=$!

trap 'kill "$JID" "$TID" 2>/dev/null || true' EXIT
wait
