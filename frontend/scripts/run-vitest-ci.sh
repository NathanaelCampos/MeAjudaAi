#!/usr/bin/env bash
set -euo pipefail

mapfile -t test_files < <(
  find app src -type f \( -name '*.test.ts' -o -name '*.test.tsx' -o -name '*.spec.ts' -o -name '*.spec.tsx' \) \
    ! -path 'app/onboarding/profissional/page-client.test.tsx' \
    | sort
)

if [ "${#test_files[@]}" -eq 0 ]; then
  echo "Nenhum arquivo de teste encontrado."
  exit 1
fi

for test_file in "${test_files[@]}"; do
  echo
  echo "==> Executando ${test_file}"
  npx vitest run "${test_file}" --reporter=verbose --pool=forks
done
