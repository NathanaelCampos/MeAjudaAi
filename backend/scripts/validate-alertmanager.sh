#!/usr/bin/env bash
set -eo pipefail

PROMETHEUS_URL=${PROMETHEUS_URL:-http://localhost:9090}
ALERTMANAGER_URL=${ALERTMANAGER_URL:-http://localhost:9095}

required_rules=("MeAjudaAiJobsQueueLong" "MeAjudaAiWatchCycleSlow" "RunAdminJobsTestsFailed")

if ! command -v curl >/dev/null 2>&1; then
  echo "curl is required" >&2
  exit 1
fi

if ! command -v jq >/dev/null 2>&1; then
  echo "jq is required" >&2
  exit 1
fi

echo "consultando regras no Prometheus em $PROMETHEUS_URL..."
rules_json=$(curl -fsS "${PROMETHEUS_URL}/api/v1/rules")

for rule in "${required_rules[@]}"; do
  if ! jq -e --arg name "$rule" \
    '.data.groups[].rules[]? | select(.name == $name)' <<<"$rules_json" >/dev/null; then
    echo "regra Prometheus $rule não encontrada" >&2
    exit 1
  fi
done

echo "as regras obrigatórias estão ativas."

if curl -fsS "${ALERTMANAGER_URL}/api/v2/alerts" >/dev/null 2>&1; then
  echo "Alertmanager em $ALERTMANAGER_URL respondeu. Verifique routes/receivers para garantir entrega de notificações."
else
  echo "não foi possível contactar Alertmanager em $ALERTMANAGER_URL; revise a URL ou o serviço."
  exit 1
fi
