#!/usr/bin/env bash
set -euo pipefail

HOST=${HOST:-http://localhost:5231}
JOB_ID=${JOB_ID:-emails-outbox}
ADMIN_EMAIL=${ADMIN_EMAIL:-admin@meajudaai.local}
ADMIN_PASSWORD=${ADMIN_PASSWORD:-Admin@123}

if ! command -v curl >/dev/null 2>&1; then
  echo "curl is required" >&2
  exit 1
fi

if ! command -v jq >/dev/null 2>&1; then
  echo "jq is required" >&2
  exit 1
fi

TOKEN=${TOKEN_ADMIN:-}

if [[ -z "$TOKEN" ]]; then
  echo "TOKEN_ADMIN nao definido; tentando obter token via login admin em '$HOST'..."

  login_payload=$(jq -n \
    --arg email "$ADMIN_EMAIL" \
    --arg senha "$ADMIN_PASSWORD" \
    '{email: $email, senha: $senha}')

  login_response=$(curl -fsS -X POST "$HOST/api/auth/login" \
    -H "Content-Type: application/json" \
    -d "$login_payload")

  TOKEN=$(jq -r '.token // empty' <<<"$login_response")

  if [[ -z "$TOKEN" ]]; then
    echo "Nao foi possivel extrair o token do login admin." >&2
    exit 1
  fi
fi

echo "Enfileirando job '$JOB_ID'..."
curl -fsS -X POST "$HOST/api/admin/jobs/$JOB_ID/enfileirar" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{}'

echo "Consulta métrica de fila..."
curl -fsS "$HOST/api/admin/jobs/fila/metricas" -H "Authorization: Bearer $TOKEN" | jq

echo "Consulta alertas..."
curl -fsS "$HOST/api/admin/jobs/fila/alertas" -H "Authorization: Bearer $TOKEN" | jq

echo "Consulta logs de retries..."
curl -fsS "$HOST/api/admin/jobs/fila/logs/retries" -H "Authorization: Bearer $TOKEN" | jq

echo "Consulta fila (últimas execuções)..."
curl -fsS "$HOST/api/admin/jobs/fila?jobId=$JOB_ID&limit=5" -H "Authorization: Bearer $TOKEN" | jq
