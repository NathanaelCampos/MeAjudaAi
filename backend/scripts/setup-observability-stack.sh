#!/usr/bin/env bash
set -euo pipefail

TARGET_ETC=${TARGET_ETC:-/etc}
PROMETHEUS_ETC=${PROMETHEUS_ETC:-/etc/prometheus}
RULES_DIR=${RULES_DIR:-$PROMETHEUS_ETC/rules}
DEPLOY_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../deploy" && pwd)"

copy_env() {
  local src=$1
  local dst=$2

  if [[ -e "$dst" ]]; then
    echo "Arquivo $dst já existe; mantendo a versão atual."
  else
    sudo install -m 640 "$src" "$dst"
  fi
}

copy_unit() {
  local src=$1
  local dst=$2
  sudo install -m 644 "$src" "$dst"
}

copy_rule() {
  local src=$1
  local dst_dir=$2
  sudo mkdir -p "$dst_dir"
  sudo install -m 644 "$src" "$dst_dir/$(basename "$src")"
}

ensure_prometheus_config() {
  sudo mkdir -p "$PROMETHEUS_ETC"
  local target="$PROMETHEUS_ETC/prometheus.yml"
  if [[ -e "$target" ]]; then
    echo "$target já existe; substitua manualmente se precisar."
  else
    sudo install -m 644 "$DEPLOY_DIR/prometheus-configuration-example.yml" "$target"
  fi
}

main() {
  echo "Copiando envs..."
  copy_env "$DEPLOY_DIR/watch-jobs.env.example" "$TARGET_ETC/watch-jobs.env"
  copy_env "$DEPLOY_DIR/export-job-metrics.env.example" "$TARGET_ETC/export-job-metrics.env"
  copy_env "$DEPLOY_DIR/validate-alertmanager.env.example" "$TARGET_ETC/validate-alertmanager.env"
  copy_env "$DEPLOY_DIR/run-admin-jobs-tests.env.example" "$TARGET_ETC/run-admin-jobs-tests.env"

  echo "Instalando services e timers..."
  copy_unit "$DEPLOY_DIR/watch-jobs.service" "$TARGET_ETC/systemd/system/watch-jobs.service"
  copy_unit "$DEPLOY_DIR/export-job-metrics.service" "$TARGET_ETC/systemd/system/export-job-metrics.service"
  copy_unit "$DEPLOY_DIR/export-job-metrics.timer" "$TARGET_ETC/systemd/system/export-job-metrics.timer"
  copy_unit "$DEPLOY_DIR/validate-alertmanager.service" "$TARGET_ETC/systemd/system/validate-alertmanager.service"
  copy_unit "$DEPLOY_DIR/validate-alertmanager.timer" "$TARGET_ETC/systemd/system/validate-alertmanager.timer"
  copy_unit "$DEPLOY_DIR/run-admin-jobs-tests.service" "$TARGET_ETC/systemd/system/run-admin-jobs-tests.service"
  copy_unit "$DEPLOY_DIR/run-admin-jobs-tests.timer" "$TARGET_ETC/systemd/system/run-admin-jobs-tests.timer"

  echo "Instalando regras Prometheus..."
  copy_rule "$DEPLOY_DIR/prometheus-job-alerts.rules.yml" "$RULES_DIR"
  copy_rule "$DEPLOY_DIR/prometheus-ci-alerts.rules.yml" "$RULES_DIR"

  echo "Gerando arquivo prometheus.yml..."
  ensure_prometheus_config

  echo "Recarregando systemd, habilitando serviços e timers..."
  sudo systemctl daemon-reload
  sudo systemctl enable --now watch-jobs.service export-job-metrics.timer validate-alertmanager.timer run-admin-jobs-tests.timer

  echo "Reiniciando Prometheus e Alertmanager..."
  sudo systemctl restart prometheus || true
  sudo systemctl restart alertmanager || true

  echo "Observability stack configurada. Edite os arquivos em /etc (*) se necessário e valide com scripts/validate-alertmanager.sh e scripts/run-admin-jobs-tests.sh."
}

main
