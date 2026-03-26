#!/usr/bin/env bash
set -euo pipefail

HOST=${HOST:-http://localhost:5231}
JOB_ID=${JOB_ID:-emails-outbox}
TOKEN_ADMIN=${TOKEN_ADMIN:-}
ADMIN_EMAIL=${ADMIN_EMAIL:-admin@meajudaai.local}
ADMIN_PASSWORD=${ADMIN_PASSWORD:-Admin@123}
INTERVAL=${INTERVAL:-300}
LOG_DIR=${LOG_DIR:-"$PWD/logs"}
LOG_FILE=${LOG_FILE:-"$LOG_DIR/watch-jobs.log"}
WATCH_JOBS_METRICS_FILE=${WATCH_JOBS_METRICS_FILE:-"$LOG_DIR/watch-jobs-metrics.prom"}
ALERT_WEBHOOK_URL=${ALERT_WEBHOOK_URL:-}

mkdir -p "$LOG_DIR"

log() {
  local ts
  ts=$(date +"%Y-%m-%dT%H:%M:%S%z")
  printf "[%s] %s\n" "$ts" "$*" | tee -a "$LOG_FILE"
}

write_watch_metrics() {
  local duration_ns=$1
  local alert_count=$2
  local duration_sec

  duration_sec=$(awk -v ns="$duration_ns" 'BEGIN {printf "%.6f", ns/1e9}')

  cat <<EOF > "$WATCH_JOBS_METRICS_FILE"
# HELP meajudaai_watch_jobs_cycle_duration_seconds duração do ciclo do watch-jobs
# TYPE meajudaai_watch_jobs_cycle_duration_seconds gauge
meajudaai_watch_jobs_cycle_duration_seconds $duration_sec
# HELP meajudaai_watch_jobs_alerts_count número de alertas retornados em cada ciclo
# TYPE meajudaai_watch_jobs_alerts_count gauge
meajudaai_watch_jobs_alerts_count $alert_count
EOF
}

if ! command -v curl >/dev/null 2>&1; then
  echo "curl is required" >&2
  exit 1
fi

if ! command -v jq >/dev/null 2>&1; then
  echo "jq is required" >&2
  exit 1
fi

if ! command -v awk >/dev/null 2>&1; then
  echo "awk is required" >&2
  exit 1
fi

ensure_api() {
  if [[ -z "$TOKEN_ADMIN" ]]; then
    TOKEN_ADMIN=$(curl -fsS -X POST "$HOST/api/auth/login" \
      -H "Content-Type: application/json" \
      -d "{\"email\":\"$ADMIN_EMAIL\",\"senha\":\"$ADMIN_PASSWORD\"}" \
      | jq -r '.token // empty')
  fi

  if [[ -z "$TOKEN_ADMIN" ]]; then
    echo "failed to get admin token" >&2
    exit 1
  fi
}

process_queue() {
  curl -fsS -X POST "$HOST/api/admin/jobs/fila/processar" \
    -H "Authorization: Bearer $TOKEN_ADMIN" \
    -H "Content-Type: application/json" | jq .
}

fetch_alerts() {
  curl -fsS "$HOST/api/admin/jobs/fila/alertas" \
    -H "Authorization: Bearer $TOKEN_ADMIN"
}

maybe_notify_alerts() {
  local alerts=$1
  if [[ -z "$ALERT_WEBHOOK_URL" || "$alerts" == "[]" ]]; then
    return 0
  fi

  local payload
  local message
  message=$(jq -n \
    --arg host "$HOST" \
    --arg jobId "$JOB_ID" \
    --arg ts "$(date -Is)" \
    --argjson alerts "$alerts" \
    '
    {
      text: "MeAjudaAi alerta: \($jobId) em \($host) às \($ts)",
      attachments: $alerts | map({
        fallback: "\(.nivelAlerta) \(.mensagem)",
        color: .cor,
        title: .nivelAlerta,
        text: "\(.mensagem)\nPendentes: \(.totalPendentes), Falhas: \(.totalFalhas), Tempo médio fila: \(.tempoMedioFilaSegundos | gsub("\\\\.?"; ""))s"
      })
    }
    ')
  payload="$message"

  curl -fsS -X POST "$ALERT_WEBHOOK_URL" \
    -H "Content-Type: application/json" \
    -d "$payload" >/dev/null || log "failed to post alerts to webhook"
  log "alerts dispatched to $ALERT_WEBHOOK_URL"
}

cycle() {
  scripts/jobs-cycle.sh
}

main_loop() {
  while true; do
    local cycle_start_ns cycle_end_ns alerts_count

    cycle_start_ns=$(date +%s%N)
    ensure_api
    process_queue
    alerts_json=$(fetch_alerts)
    alerts_count=$(jq -r 'length' <<<"$alerts_json")
    maybe_notify_alerts "$alerts_json"
    cycle
    cycle_end_ns=$(date +%s%N)
    write_watch_metrics "$((cycle_end_ns - cycle_start_ns))" "$alerts_count"

    echo "sleeping $INTERVAL seconds..."
    sleep "$INTERVAL"
  done
}

main_loop
