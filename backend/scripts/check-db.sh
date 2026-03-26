#!/usr/bin/env bash
set -euo pipefail

COMPOSE_FILE="devops/docker-compose.yml"
SERVICE=postgres
DATABASE=meajudaai_db
USER=postgres

function run_psql() {
  local cmd="$1"
  docker compose -f "$COMPOSE_FILE" exec -T "$SERVICE" psql -U "$USER" -d "$DATABASE" -c "$cmd"
}

echo "Verificando migrações e seeds em $SERVICE..."
run_psql "\dt"
run_psql "select count(*) from profissoes;"
run_psql "select count(*) from planos_impulsionamento;"
run_psql "select count(*) from profissionais;"
run_psql "select \"Id\", \"Email\", \"TipoPerfil\" from \"usuarios\" where \"Email\"='admin@meajudaai.local';"

echo "Validação concluída com sucesso."
