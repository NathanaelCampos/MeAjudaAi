#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"

HOST=${HOST:-http://localhost:5231}
ADMIN_EMAIL=${ADMIN_EMAIL:-admin@meajudaai.local}
ADMIN_PASSWORD=${ADMIN_PASSWORD:-Admin@123}
TOKEN_ADMIN=${TOKEN_ADMIN:-}
JOB_ID_FILTER=${JOB_ID_FILTER:-}
FORMAT=${FORMAT:-prom}
OUTPUT_FILE=${OUTPUT_FILE:-"$ROOT_DIR/logs/background-job-metrics.$FORMAT"}
ALERT_WEBHOOK_URL=${ALERT_WEBHOOK_URL:-}

mkdir -p "$(dirname "$OUTPUT_FILE")"

require() {
  if ! command -v "$1" >/dev/null 2>&1; then
    echo "$1 is required" >&2
    exit 1
  fi
}

require curl
require jq

ensure_token() {
  if [[ -n "$TOKEN_ADMIN" ]]; then
    return 0
  fi

  TOKEN_ADMIN=$(curl -fsS -X POST "$HOST/api/auth/login" \
    -H "Content-Type: application/json" \
    -d "{\"email\":\"$ADMIN_EMAIL\",\"senha\":\"$ADMIN_PASSWORD\"}" \
    | jq -r '.token // empty')

  if [[ -z "$TOKEN_ADMIN" ]]; then
    echo "failed to obtain admin token" >&2
    exit 1
  fi
}

fetch_metrics() {
  local metrics
  metrics=$(curl -fsS "$HOST/api/admin/jobs/fila/metricas" \
    -H "Authorization: Bearer $TOKEN_ADMIN")
  echo "$metrics"
}

render_prometheus() {
  local data=$1
  local job_label=""
  if [[ -n "$JOB_ID_FILTER" ]]; then
    job_label="jobId=\"$JOB_ID_FILTER\","
  fi

  {
    echo "# HELP meajudaai_background_jobs_queue_total contagem total por status"
    echo "# TYPE meajudaai_background_jobs_queue_total gauge"

    for label in totalPendentes totalProcessando totalSucesso totalFalhas totalCancelados; do
      local value
      value=$(jq -r ".${label}" <<<"$data")
      [[ $value == "null" ]] && value=0
      echo "meajudaai_background_jobs_queue_total{${job_label}status=\"${label#total}\"} ${value}"
    done

    echo "# HELP meajudaai_background_jobs_queue_metric_tempo tempo medio em segundos"
    echo "# TYPE meajudaai_background_jobs_queue_metric_tempo gauge"
    for label in tempoMedioFilaSegundos tempoMedioProcessamentoSegundos tempoMedioFalhaSegundos; do
      if ! jq -e ".${label}" <<<"$data" >/dev/null 2>&1; then
        continue
      fi
      local value
      value=$(jq -r ".${label}" <<<"$data")
      [[ $value == "null" ]] && value=0
      echo "meajudaai_background_jobs_queue_metric{${job_label}metric=\"${label}\"} ${value}"
    done

    if [[ -n "$JOB_ID_FILTER" ]]; then
      echo "# HELP meajudaai_background_jobs_queue_job_total total por job"
      echo "# TYPE meajudaai_background_jobs_queue_job_total gauge"
    fi

    jq -r '.porJob | to_entries[] | "\(.key) \(.value)"' <<<"$data" | while read -r job count; do
      if [[ -z "$JOB_ID_FILTER" || "$JOB_ID_FILTER" == "$job" ]]; then
        echo "meajudaai_background_jobs_queue_job_total{jobId=\"${job}\"} ${count}"
      fi
    done
  } > "$OUTPUT_FILE"
}

notify_webhook() {
  local alerts
  alerts=$(curl -fsS "$HOST/api/admin/jobs/fila/alertas" \
    -H "Authorization: Bearer $TOKEN_ADMIN")

  if [[ -z "$ALERT_WEBHOOK_URL" || "$alerts" == "[]" ]]; then
    return 0
  fi

  local payload
  payload=$(jq -n \
    --arg host "$HOST" \
    --arg jobId "${JOB_ID_FILTER:-todos}" \
    --arg ts "$(date -Is)" \
    --argjson alerts "$alerts" \
    '
    {
      text: "MeAjudaAi alerta: \($jobId) em \($host) às \($ts)",
      attachments: $alerts | map({
        fallback: "\(.nivelAlerta) \(.mensagem)",
        color: .cor,
        title: .nivelAlerta,
        text: "\(.mensagem)\nPendentes: \(.totalPendentes), Falhas: \(.totalFalhas), Tempo medio fila: \(.tempoMedioFilaSegundos | gsub(\"\\.?\"; \"\"))s"
      })
    }
    ')

  curl -fsS -X POST "$ALERT_WEBHOOK_URL" \
    -H "Content-Type: application/json" \
    -d "$payload" >/dev/null && echo "alerts sent to $ALERT_WEBHOOK_URL" || echo "failed to send alerts"
}

main() {
  ensure_token
  local metrics
  metrics=$(fetch_metrics)

  if [[ "$FORMAT" == "prom" ]]; then
    render_prometheus "$metrics"
  else
    echo "$metrics" > "$OUTPUT_FILE"
  fi

  notify_webhook
  echo "metrics written to $OUTPUT_FILE"
}

main
