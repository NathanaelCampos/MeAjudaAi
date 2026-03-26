#!/usr/bin/env bash
set -euo pipefail

export MSBUILDDISABLENODEREUSE=1
export MSBUILDNOINPROCESS=1

dotnet test tests/MeAjudaAi.IntegrationTests/MeAjudaAi.IntegrationTests.csproj \
  --filter AdminJobsEndpointsTests
