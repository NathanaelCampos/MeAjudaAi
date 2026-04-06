# Checklist de Validacao de Staging

## 1. Subida do ambiente

- Subir o backend:

```bash
cd backend
dotnet run --project src/MeAjudaAi.Api --urls http://0.0.0.0:5231
```

- Subir gateway e frontend:

```bash
cd devops
GATEWAY_PORT=8080 \
NEXT_PUBLIC_API_BASE_URL= \
docker compose -f docker-compose.gateway.yml up -d --build
```

## 2. Smoke tecnico

- Validar frontend:

```bash
curl -I http://localhost:8080/
```

- Validar API via gateway:

```bash
curl -I http://localhost:8080/api/profissoes
```

- Validar Swagger via gateway:

```bash
curl -I http://localhost:8080/swagger/index.html
```

## 3. Validacao publica

- `/`
  - App abre sem erro visual.
  - Header e navegacao mobile aparecem corretamente.
  - Prompt de instalacao PWA nao quebra layout.

- `/explorar`
  - Lista profissionais.
  - Filtros por nome, profissao e cidade funcionam.
  - Ordenacao e paginacao funcionam.
  - Botao de limpar filtros funciona.

- `/profissionais/{id}`
  - Perfil carrega descricao, profissoes, especialidades, contatos e avaliacoes.
  - CTA de solicitar servico redireciona corretamente.

## 4. Autenticacao e onboarding

- `/cadastro`
  - Cadastro de cliente redireciona para `/onboarding/cliente`.
  - Cadastro de profissional redireciona para `/onboarding/profissional`.

- `/login`
  - Login com usuario valido funciona.
  - Sessao persiste ao navegar.
  - Logout remove acesso as rotas protegidas.

- `/onboarding/cliente`
  - Preferencias de notificacao salvam corretamente.

- `/onboarding/profissional`
  - Perfil base salva corretamente.
  - Profissoes, especialidades e areas de atendimento salvam corretamente.
  - Upload de portfolio funciona.
  - Formas de recebimento salvam corretamente.

## 5. Fluxo de servicos

- Cliente
  - Em `/servicos/novo`, criar solicitacao com sucesso.
  - Em `/servicos`, filtrar por status.
  - Em `/servicos/{id}`, acompanhar timeline.
  - Em servico concluido, enviar avaliacao.

- Profissional
  - Em `/servicos/profissional`, aceitar, iniciar, concluir e cancelar servicos.
  - Em `/servicos/{id}`, validar acoes contextuais.

- Avaliacoes
  - Cliente envia avaliacao apos conclusao.
  - Avaliacao aparece no perfil publico do profissional.

## 6. Painel admin

- Login admin
  - Entrar com usuario admin documentado.
  - Acessar `/jobs`, `/fila`, `/historico` e `/retries`.

- `/jobs`
  - Executar job.
  - Enfileirar job.
  - Agendar job futuro.
  - Cancelar pendentes.

- `/fila`
  - Processar fila.
  - Reabrir execucoes.
  - Cancelar execucoes.
  - Confirmar atualizacao visual.

- `/historico`
  - Filtros funcionam.
  - Dados coerentes com backend.

- `/retries`
  - Filtros funcionam.
  - Dados coerentes com backend.

## 7. Observabilidade

- Validar watcher:

```bash
sudo systemctl status watch-jobs.service
```

- Validar timers:

```bash
sudo systemctl status export-job-metrics.timer validate-alertmanager.timer run-admin-jobs-tests.timer
```

- Validar logs:

```bash
sudo journalctl -u watch-jobs.service -n 40 --no-pager
sudo journalctl -u validate-alertmanager.service -n 20 --no-pager
```

## 8. Criterio de aprovacao

- Navegacao publica sem erro.
- Login, cadastro e onboarding funcionando.
- Fluxo cliente e profissional completo.
- Painel admin operacional.
- Gateway servindo frontend e API no mesmo host.
- Backend CI e Frontend CI verdes no PR.
