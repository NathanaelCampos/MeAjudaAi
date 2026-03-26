#!/usr/bin/env bash
set -euo pipefail

TARGET_ZIP=${TARGET_ZIP:-deploy/observability-docs.zip}
ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
DEPLOY_DIR="$ROOT_DIR/deploy"

files=(
  "deploy/alert-channel-setup.md"
  "deploy/observability-deploy-checklist.md"
  "deploy/alertmanager-production-checklist.md"
  "deploy/support-observability.md"
  "deploy/observability-summary.md"
)

rm -f "$ROOT_DIR/$TARGET_ZIP"
zip -j "$ROOT_DIR/$TARGET_ZIP" "${files[@]}"
echo "Pacote criado: $TARGET_ZIP"
