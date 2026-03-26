#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
DEVOPS_DIR="$ROOT/../devops"

PROM_CONF="$DEVOPS_DIR/prometheus.yml"
ALERT_CONF="$DEVOPS_DIR/alertmanager.yml"

echo "Reescrevendo configurações locais do Prometheus/Alertmanager..."
cat <<'EOF' >"$PROM_CONF"
global:
  scrape_interval: 15s
  evaluation_interval: 15s

rule_files:
  - /etc/prometheus/rules/prometheus-job-alerts.rules.yml
  - /etc/prometheus/rules/prometheus-ci-alerts.rules.yml

scrape_configs:
  - job_name: node-exporter
    static_configs:
      - targets: ['node-exporter:9100']
    metrics_path: /metrics
    scheme: http
EOF

cat <<'EOF' >"$ALERT_CONF"
global:
  resolve_timeout: 5m

route:
  receiver: default-receiver

receivers:
  - name: default-receiver
EOF

echo "Recarregando Prometheus/Alertmanager via docker compose..."
cd "$DEVOPS_DIR"
docker compose -f docker-compose-prometheus.yml down --remove-orphans
docker compose -f docker-compose-prometheus.yml up -d

wait_for_url() {
  local url="$1"
  local deadline=${2:-30}
  local start
  start=$(date +%s)
  while true; do
    if curl -fsS "$url" >/dev/null 2>&1; then
      return 0
    fi
    if (( $(date +%s) - start >= deadline )); then
      echo "timeout waiting for $url" >&2
      return 1
    fi
    sleep 1
  done
}

echo "Verificando endpoints Prometheus/Alertmanager..."
wait_for_url "http://localhost:9090/api/v1/rules"
wait_for_url "http://localhost:9095/-/ready"

echo "Executando validate-alertmanager.sh..."
cd "$ROOT"
PROMETHEUS_URL=http://localhost:9090 ALERTMANAGER_URL=http://localhost:9095 scripts/validate-alertmanager.sh

echo "Stack Prometheus/Alertmanager validada."
