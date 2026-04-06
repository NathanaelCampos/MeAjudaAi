# Sprint 1 - Checklist Operacional

## 1. Subir o ambiente

- Backend:

```bash
cd backend
dotnet run --project src/MeAjudaAi.Api --urls http://0.0.0.0:5231
```

- Frontend + gateway:

```bash
cd devops
GATEWAY_PORT=8080 \
NEXT_PUBLIC_API_BASE_URL= \
docker compose -f docker-compose.gateway.yml up -d --build
```

## 2. Validar infraestrutura minima

- Frontend responde:

```bash
curl -I http://localhost:8080/
```

- API responde pelo gateway:

```bash
curl -I http://localhost:8080/api/profissoes
```

- Swagger responde:

```bash
curl -I http://localhost:8080/swagger/index.html
```

## 3. Executar validacao funcional minima

- Abrir `/explorar` e validar listagem.
- Fazer cadastro de cliente.
- Fazer cadastro de profissional.
- Validar onboarding de cliente.
- Validar onboarding de profissional.
- Criar solicitacao de servico.
- Validar fluxo do profissional.
- Validar painel admin.

## 4. Executar validacao operacional

- Verificar watcher:

```bash
sudo systemctl status watch-jobs.service
```

- Verificar timers:

```bash
sudo systemctl status export-job-metrics.timer validate-alertmanager.timer run-admin-jobs-tests.timer
```

- Verificar logs:

```bash
sudo journalctl -u watch-jobs.service -n 40 --no-pager
sudo journalctl -u validate-alertmanager.service -n 20 --no-pager
```

## 5. Revisar PR

- Confirmar PR `#2` limpo e com checks verdes.
- Confirmar `Backend CI` verde.
- Confirmar `Frontend CI` verde.

## 6. Fazer merge

```bash
gh pr merge 2 --squash --delete-branch
```

## 7. Atualizar ambiente apos merge

```bash
git checkout main
git pull origin main
```

## 8. Registrar entrega

- Registrar URL final de staging.
- Registrar commit mergeado.
- Registrar resultado da validacao manual.
- Registrar qualquer pendencia encontrada.
