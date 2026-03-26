# Checklist de liberação do backend antes do frontend

Este documento reúne os passos mínimos que devem estar sinalizados como "concluídos" para garantir que o backend entregue os dados/métricas/alertas que o frontend precisa consumir.

## 1. Banco e seeds prontos
- Rode `backend/scripts/check-db.sh` (que usa `docker compose -f devops/docker-compose.yml exec postgres psql ...`) para validar:
  - As tabelas `background_jobs_execucoes`, `background_job_fila_alerta_*`, `background_job_retry_logs`, `usuarios` (com admin) e o resto do schema que o frontend usa.
  - Os seeds: `profissoes` (5), `planos_impulsionamento` (3), `profissionais` (≥8) e `admin@meajudaai.local` com `TipoPerfil = 3`.
  - O script imprime `Validação concluída com sucesso.` e pode ser incluído em scripts de deploy para monotorização.

## 2. API `/api/admin/jobs` estabilizada
- Confirme que todas as rotas estão expostas com os DTOs descritos em `README.md`, `backend/OBSERVABILITY.md` e `backend/src/MeAjudaAi.Api/MeAjudaAi.Api.http`:
  - `/api/admin/jobs` (lista de jobs com snapshot do `IBackgroundJobExecutionMetricsService`).
  - `/api/admin/jobs/fila`, `/fila/metricas`, `/fila/alertas`, `/fila/alertas/historico`, `/fila/logs/retries` (todos com exemplos JSON). 
  - Alertas e métricas usam `BackgroundJobFilaAlertasHistoricoResponse`, `BackgroundJobRetryLogResponse` e `BackgroundJobFilaMetricasResponse` conforme definido no serviço `AdminJobService`.
- Verifique se os endpoints respondem para o admin seed (autenticar `admin@meajudaai.local` via token) completando os fluxos de fila/alertas/histórico.

## 3. Observability completa
- Os scripts versionados `backend/scripts/watch-jobs.sh`, `watch-jobs-logs.sh`, `export-job-metrics.sh`, `validate-alertmanager.sh`, `refresh-prometheus-stack.sh`, `setup-observability-stack.sh`, `jobs-cycle.sh`, `run-admin-jobs-tests.sh`, `package-observability-docs.sh` devem estar sincronizados e com permissão executável; eles cobrem watcher, exportador, validação e packaging dos docs.
- `watch-jobs.service`, `export-job-metrics.timer`, `validate-alertmanager.timer` e `run-admin-jobs-tests.timer` devem estar habilitados (confirme via `sudo systemctl status …`) e gerando os `.prom` descritos em `backend/docs/artefatos-observabilidade.md`.
- O Alertmanager responde em `http://localhost:9095` e o Prometheus em `http://localhost:9090` com as regras `MeAjudaAiJobsQueueLong`, `MeAjudaAiWatchCycleSlow` e `RunAdminJobsTestsFailed` validadas (via `scripts/validate-alertmanager.sh`).

## 4. Testes e artefatos
- O workflow `backend-ci.yml` deve terminar com `success` (run 23616933293 ou equivalente), com os jobs `build-test` e `admin-jobs-tests` completando. Os artefatos `backend-test-results` e `admin-jobs-tests-log` (baixados com `GH_RUN_DOWNLOAD_EXTRACT=false`) precisam estar disponíveis para auditoria, conforme documentado em `backend/docs/artefatos-observabilidade.md`.

## 5. Documentação e contratos
- Atualize o Swagger (`http://localhost:5231/swagger`), o arquivo `backend/src/MeAjudaAi.Api/MeAjudaAi.Api.http` e os README/OBSERVABILITY para refletir os formatos JSON e os campos que o frontend consome em cada endpoint.
- Confirme que `backend/docs/artefatos-observabilidade.md` e este checklist estão publicados e, caso necessário, rode `backend/scripts/package-observability-docs.sh` para gerar um ZIP com os guias.
- Opcional: adicione links diretos aos dashboards mencionados (Grafana, Prometheus) e inclua instruções sobre como importar `deploy/grafana-background-jobs-dashboard.json`.

Após esses passos, o frontend pode consumir métricas/alertas com confiança — a prática recomendada é repetir este checklist após cada refatoração pesada antes de liberar novas versões da UI.
