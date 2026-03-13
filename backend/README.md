# Me Ajuda Ai Backend

## Comandos principais

No diretório `backend`:

```bash
dotnet restore MeAjudaAi.sln
dotnet build MeAjudaAi.sln
dotnet test MeAjudaAi.sln
```

Para rodar apenas a regressao de contrato HTTP da API:

```bash
dotnet test tests/MeAjudaAi.IntegrationTests/MeAjudaAi.IntegrationTests.csproj --filter "Category=ApiContract"
```

## API local

Banco PostgreSQL via Docker:

```bash
docker compose -f ../devops/docker-compose.yml up -d
```

API:

```bash
dotnet run --project src/MeAjudaAi.Api
```

Swagger UI em desenvolvimento:

```text
http://localhost:5231/swagger
```

O Swagger agora inclui exemplos de request e response para:

- auth
- profissionais
- profissoes e especialidades
- cidades e bairros
- servicos
- avaliacoes
- webhooks
- endpoints operacionais de historico e metricas

O contrato OpenAPI tambem foi alinhado com os payloads reais da API:

- respostas de erro simples usam `MensagemErroResponse`
- erros de validacao usam `ErroValidacaoResponse`
- respostas administrativas e operacionais estao tipadas no Swagger
- endpoints de importacao, impulsionamento, servicos, avaliacoes e profissionais exibem contratos consistentes de `200`, `400`, `401` e `404` quando aplicavel

## Contrato HTTP

### Payload comum de erro simples

Usado principalmente para erro de regra de negocio, autenticacao e alguns `404`:

```json
{
  "mensagem": "Texto do erro."
}
```

Esse payload corresponde ao DTO:

- `MensagemErroResponse`

### Payload comum de erro de validacao

Usado quando o `ModelState` falha por request invalido:

```json
{
  "mensagem": "Erro de validação.",
  "erros": [
    {
      "campo": "Email",
      "mensagens": [
        "Email inválido."
      ]
    }
  ]
}
```

Esse payload corresponde aos DTOs:

- `ErroValidacaoResponse`
- `CampoErroValidacaoResponse`

### Semantica de status mais usada

- `200 OK`: operacao concluida com retorno de recurso ou resultado
- `204 No Content`: operacao concluida sem corpo de resposta
- `400 Bad Request`: validacao invalida ou regra de negocio violada
- `401 Unauthorized`: token ausente, invalido ou webhook sem autenticacao valida
- `403 Forbidden`: usuario autenticado sem role/permissao suficiente
- `404 Not Found`: recurso inexistente ou nao acessivel no contexto esperado

### Onde isso aparece

Os contratos acima estao refletidos no Swagger para os principais grupos:

- auth
- profissionais
- impulsionamentos
- servicos
- avaliacoes
- importacao
- webhooks e operacao

## Colecoes de apoio

O backend possui duas colecoes prontas para consumo manual:

- HTTP client do VS Code / Rider em [src/MeAjudaAi.Api/MeAjudaAi.Api.http](/home/nathanael-campos/dev/src/me-ajuda-ai/backend/src/MeAjudaAi.Api/MeAjudaAi.Api.http)
- Postman em [MeAjudaAi.postman_collection.json](/home/nathanael-campos/dev/src/me-ajuda-ai/backend/MeAjudaAi.postman_collection.json)
- Insomnia em [MeAjudaAi.insomnia.json](/home/nathanael-campos/dev/src/me-ajuda-ai/backend/MeAjudaAi.insomnia.json)

Ambas cobrem:

- auth
- catalogos
- perfil profissional
- impulsionamento
- servicos e avaliacoes
- webhooks
- endpoints operacionais

Quando usar cada artefato:

- Swagger: exploracao rapida da API, exemplos de payload e testes interativos no navegador
- `.http`: execucao manual versionada no editor
- Postman: colecao compartilhavel para times que ja usam Postman
- Insomnia: colecao compartilhavel para times que preferem Insomnia

### Uso rapido da colecao Postman

1. Importe [MeAjudaAi.postman_collection.json](/home/nathanael-campos/dev/src/me-ajuda-ai/backend/MeAjudaAi.postman_collection.json) no Postman
2. Ajuste a variavel `host` se a API nao estiver em `http://localhost:5231`
3. Preencha ou capture os tokens `token_admin`, `token_profissional` e `token_cliente`
4. Preencha os IDs dependentes (`profissao_id`, `cidade_id`, `bairro_id`, `profissional_id`, etc.) conforme os requests anteriores

Para webhooks HMAC, a colecao deixa variaveis dedicadas para assinatura:

- `webhook_signature`
- `webhook_asaas_signature`

### Uso rapido da colecao Insomnia

1. Importe [MeAjudaAi.insomnia.json](/home/nathanael-campos/dev/src/me-ajuda-ai/backend/MeAjudaAi.insomnia.json) no Insomnia
2. Ajuste as variaveis do `Base Environment`
3. Preencha tokens e IDs conforme os requests anteriores
4. Para webhooks HMAC, informe `webhook_signature` e `webhook_asaas_signature` manualmente

## Webhooks de pagamento

### Rotas

- Provedor padrao: `POST /api/webhooks/pagamentos/impulsionamentos`
- Provedor especifico: `POST /api/webhooks/pagamentos/{provedor}/impulsionamentos`

### Autenticacao

- O provedor padrao usa o header `X-Webhook-Signature`
- O provedor `asaas` usa o header `X-Asaas-Signature`
- A assinatura e um HMAC-SHA256 do corpo bruto da requisicao

### Exemplo: webhook padrao

```bash
WEBHOOK_SECRET='meajudaai-webhook-secret-dev'
PAYLOAD='{"codigoReferenciaPagamento":"ref-001","statusPagamento":"pago","eventoExternoId":"evt-001"}'
SIGNATURE=$(printf '%s' "$PAYLOAD" | openssl dgst -sha256 -hmac "$WEBHOOK_SECRET" -binary | xxd -p -c 256)

curl -i -X POST http://localhost:5231/api/webhooks/pagamentos/impulsionamentos \
  -H "X-Webhook-Signature: $SIGNATURE" \
  -H "Content-Type: application/json" \
  -d "$PAYLOAD"
```

### Exemplo: webhook Asaas

```bash
WEBHOOK_SECRET='meajudaai-webhook-secret-asaas-dev'
PAYLOAD='{"id":"evt-asaas-001","event":"PAYMENT_RECEIVED","payment":{"externalReference":"ref-asaas-001","status":"RECEIVED"}}'
SIGNATURE=$(printf '%s' "$PAYLOAD" | openssl dgst -sha256 -hmac "$WEBHOOK_SECRET" -binary | xxd -p -c 256)

curl -i -X POST http://localhost:5231/api/webhooks/pagamentos/asaas/impulsionamentos \
  -H "X-Asaas-Signature: $SIGNATURE" \
  -H "Content-Type: application/json" \
  -d "$PAYLOAD"
```

### Comportamento

- Eventos sao idempotentes por `EventoExternoId`
- `pago` confirma o impulsionamento
- `cancelado`, `recusado`, `estornado` e `expirado` cancelam o impulsionamento
- O backend persiste auditoria com:
  - payload bruto
  - headers recebidos
  - IP de origem
  - `RequestId`
  - `UserAgent`
  - provedor
  - resultado do processamento

## Endpoints operacionais

Todos os endpoints abaixo exigem usuario com role `Administrador`.

### Consultar historico de webhooks

`GET /api/impulsionamentos/webhooks`

Filtros suportados por query string:

- `eventoExternoId`
- `codigoReferenciaPagamento`
- `provedor`
- `pagina`
- `tamanhoPagina`

Exemplo:

```bash
curl -s "http://localhost:5231/api/impulsionamentos/webhooks?provedor=asaas&eventoExternoId=evt-asaas-001&pagina=1&tamanhoPagina=20" \
  -H "Authorization: Bearer SEU_TOKEN_ADMIN"
```

### Consultar metricas de webhook

`GET /api/impulsionamentos/webhooks/metricas`

Exemplo:

```bash
curl -s http://localhost:5231/api/impulsionamentos/webhooks/metricas \
  -H "Authorization: Bearer SEU_TOKEN_ADMIN"
```

O snapshot de metricas retorna contagens em memoria por:

- `provedor`
- `resultado`
- `statusRecebido`

Resultados atualmente expostos:

- `recebido`
- `processado`
- `duplicado`
- `rejeitado`
- `erro`

## Notificacoes internas

Resumo operacional das notificacoes internas com filtros por usuario, tipo e periodo:

```bash
curl -s "http://localhost:5231/api/notificacoes/resumo-operacional?usuarioId=USUARIO_ID&tipoNotificacao=ServicoSolicitado&dataCriacaoInicial=2026-03-12T00:00:00Z&dataCriacaoFinal=2026-03-12T23:59:59Z" \
  -H "Authorization: Bearer SEU_TOKEN_ADMIN"
```

O resumo consolida:
- total
- lidas
- nao lidas
- top tipos
- top usuarios com notificacoes nao lidas

Resumo operacional das notificacoes internas arquivadas:

```bash
curl -s "http://localhost:5231/api/notificacoes/arquivadas/resumo-operacional?usuarioId=USUARIO_ID&tipoNotificacao=ServicoSolicitado&dataCriacaoInicial=2026-03-12T00:00:00Z&dataCriacaoFinal=2026-03-12T23:59:59Z" \
  -H "Authorization: Bearer SEU_TOKEN_ADMIN"
```

Listagem admin de notificacoes internas com filtros e paginacao:

```bash
curl -s "http://localhost:5231/api/notificacoes?usuarioId=USUARIO_ID&tipoNotificacao=ServicoSolicitado&lida=false&pagina=1&tamanhoPagina=20" \
  -H "Authorization: Bearer SEU_TOKEN_ADMIN"
```

Listagem admin de notificacoes internas arquivadas com filtros e paginacao:

```bash
curl -s "http://localhost:5231/api/notificacoes/arquivadas?usuarioId=USUARIO_ID&tipoNotificacao=ServicoSolicitado&lida=false&pagina=1&tamanhoPagina=20" \
  -H "Authorization: Bearer SEU_TOKEN_ADMIN"
```

Detalhe admin de uma notificacao interna arquivada:

```bash
curl -s "http://localhost:5231/api/notificacoes/arquivadas/NOTIFICACAO_ID" \
  -H "Authorization: Bearer SEU_TOKEN_ADMIN"
```

Exportacao CSV das notificacoes internas arquivadas:

```bash
curl -s "http://localhost:5231/api/notificacoes/arquivadas/exportar?usuarioId=USUARIO_ID&tipoNotificacao=ServicoSolicitado&lida=false&limite=100" \
  -H "Authorization: Bearer SEU_TOKEN_ADMIN"
```

Dashboard de notificacoes internas por usuario:

```bash
curl -s "http://localhost:5231/api/notificacoes/usuarios/USUARIO_ID/dashboard?tipoNotificacao=ServicoSolicitado&dataCriacaoInicial=2026-03-12T00:00:00Z&dataCriacaoFinal=2026-03-12T23:59:59Z" \
  -H "Authorization: Bearer SEU_TOKEN_ADMIN"
```

Detalhe admin de uma notificacao interna:

```bash
curl -s "http://localhost:5231/api/notificacoes/NOTIFICACAO_ID" \
  -H "Authorization: Bearer SEU_TOKEN_ADMIN"
```

Marcar notificacoes internas como lidas em lote:

```bash
curl -X PUT "http://localhost:5231/api/notificacoes/marcar-lidas-lote" \
  -H "Authorization: Bearer SEU_TOKEN_ADMIN" \
  -H "Content-Type: application/json" \
  -d '{"usuarioId":"USUARIO_ID","tipoNotificacao":"ServicoSolicitado","limite":100}'
```

Arquivar notificacoes internas em lote:

```bash
curl -X PUT "http://localhost:5231/api/notificacoes/arquivar-lote" \
  -H "Authorization: Bearer SEU_TOKEN_ADMIN" \
  -H "Content-Type: application/json" \
  -d '{"usuarioId":"USUARIO_ID","tipoNotificacao":"ServicoSolicitado","lida":false,"limite":100}'
```

Restaurar notificacoes internas arquivadas em lote:

```bash
curl -X PUT "http://localhost:5231/api/notificacoes/restaurar-lote" \
  -H "Authorization: Bearer SEU_TOKEN_ADMIN" \
  -H "Content-Type: application/json" \
  -d '{"usuarioId":"USUARIO_ID","tipoNotificacao":"ServicoSolicitado","lida":false,"limite":100}'
```

Preview da restauracao em lote de notificacoes internas arquivadas:

```bash
curl -X POST "http://localhost:5231/api/notificacoes/restaurar-lote/preview" \
  -H "Authorization: Bearer SEU_TOKEN_ADMIN" \
  -H "Content-Type: application/json" \
  -d '{"usuarioId":"USUARIO_ID","tipoNotificacao":"ServicoSolicitado","lida":false,"limite":100}'
```

Preview do arquivamento em lote de notificacoes internas:

```bash
curl -s -X POST "http://localhost:5231/api/notificacoes/arquivar-lote/preview" \
  -H "Authorization: Bearer SEU_TOKEN_ADMIN" \
  -H "Content-Type: application/json" \
  -d '{"usuarioId":"USUARIO_ID","tipoNotificacao":"ServicoSolicitado","lida":false,"limite":100}'
```

Retencao automatica de notificacoes internas antigas:
- configuracao em `Notificacoes:Internas:Retencao`
- arquiva em background notificacoes antigas
- por padrao pode manter somente notificacoes lidas no ciclo automatico

Execucao manual da retencao de notificacoes internas:

```bash
curl -X POST "http://localhost:5231/api/notificacoes/retencao/executar" \
  -H "Authorization: Bearer SEU_TOKEN_ADMIN"
```

Resumo operacional da retencao de notificacoes internas:

```bash
curl -s "http://localhost:5231/api/notificacoes/retencao/resumo" \
  -H "Authorization: Bearer SEU_TOKEN_ADMIN"
```

Exportacao CSV das notificacoes internas:

```bash
curl -L "http://localhost:5231/api/notificacoes/exportar?usuarioId=USUARIO_ID&tipoNotificacao=ServicoSolicitado&lida=false&limite=100" \
  -H "Authorization: Bearer SEU_TOKEN_ADMIN" \
  -o notificacoes.csv
```

## Emails de notificacao

O backend suporta dois modos para o envio do outbox:

- `SimularEnvio = true`: nao envia e-mail real, apenas registra log e permite validar o fluxo inteiro
- `SimularEnvio = false`: usa SMTP real para processar a fila de `emails_notificacoes_outbox`

Quando o SMTP real esta ativo, o backend renderiza um template HTML por `TipoNotificacao`, padronizando assunto, corpo e identificacao da referencia no e-mail enviado.

### Configuracao minima para SMTP real

Em `Emails:Notificacoes`:

```json
{
  "ProcessadorHabilitado": true,
  "SimularEnvio": false,
  "RemetenteEmail": "noreply@seu-dominio.com",
  "RemetenteNome": "Me Ajuda AI",
  "SmtpHost": "smtp.seu-provedor.com",
  "SmtpPort": 587,
  "SmtpUsuario": "smtp-user",
  "SmtpSenha": "smtp-password",
  "SmtpSsl": true,
  "LoteProcessamento": 20,
  "IntervaloSegundos": 60,
  "AtrasoBaseSegundos": 60,
  "MaxTentativas": 3
}
```

### Comportamento do retry

- e-mail novo entra com `ProximaTentativaEm`
- falha de envio agenda nova tentativa com backoff linear
- ao exceder `MaxTentativas`, o item vai para `Cancelado`
- o endpoint admin de metricas continua refletindo o status atual do outbox

### Preview de template de email

Admins podem renderizar o HTML final de um email sem enviar nada:

`POST /api/notificacoes/emails/preview`

Exemplo:

```bash
curl -s -X POST http://localhost:5231/api/notificacoes/emails/preview \
  -H "Authorization: Bearer SEU_TOKEN_ADMIN" \
  -H "Content-Type: application/json" \
  -d '{
    "tipoNotificacao": 1,
    "assunto": "Servico solicitado",
    "corpo": "Um novo servico foi criado para voce.",
    "referenciaId": "11111111-1111-1111-1111-111111111111"
  }'
```

### Filtros do outbox de email

`GET /api/notificacoes/emails` suporta filtros combinaveis por query string:

- `status`
- `usuarioId`
- `tipoNotificacao`
- `emailDestino`
- `dataCriacaoInicial`
- `dataCriacaoFinal`
- `ordenarPor`
- `ordemDesc`
- `pagina`
- `tamanhoPagina`

Exemplo:

```bash
curl -s "http://localhost:5231/api/notificacoes/emails?status=Pendente&tipoNotificacao=ServicoSolicitado&emailDestino=teste.local&ordenarPor=emailDestino&ordemDesc=false&pagina=1&tamanhoPagina=20" \
  -H "Authorization: Bearer SEU_TOKEN_ADMIN"
```

Detalhe de um item especifico do outbox:

```bash
curl -s "http://localhost:5231/api/notificacoes/emails/EMAIL_ID" \
  -H "Authorization: Bearer SEU_TOKEN_ADMIN"
```

Acoes operacionais no item do outbox:

```bash
curl -X PUT "http://localhost:5231/api/notificacoes/emails/EMAIL_ID/cancelar" \
  -H "Authorization: Bearer SEU_TOKEN_ADMIN"

curl -X PUT "http://localhost:5231/api/notificacoes/emails/EMAIL_ID/reabrir" \
  -H "Authorization: Bearer SEU_TOKEN_ADMIN"
```

Acoes operacionais em lote no outbox:

```bash
curl -X PUT "http://localhost:5231/api/notificacoes/emails/cancelar-lote" \
  -H "Authorization: Bearer SEU_TOKEN_ADMIN" \
  -H "Content-Type: application/json" \
  -d '{"status":"Pendente","tipoNotificacao":"ServicoSolicitado","limite":100}'

curl -X PUT "http://localhost:5231/api/notificacoes/emails/reabrir-lote" \
  -H "Authorization: Bearer SEU_TOKEN_ADMIN" \
  -H "Content-Type: application/json" \
  -d '{"status":"Cancelado","tipoNotificacao":"ServicoSolicitado","limite":100}'

curl -X POST "http://localhost:5231/api/notificacoes/emails/reprocessar-lote" \
  -H "Authorization: Bearer SEU_TOKEN_ADMIN" \
  -H "Content-Type: application/json" \
  -d '{"status":"Falha","tipoNotificacao":"ServicoSolicitado","limite":100}'
```

Exportacao CSV filtrada do outbox:

```bash
curl -L "http://localhost:5231/api/notificacoes/emails/exportar?status=Pendente&tipoNotificacao=ServicoSolicitado&limite=100" \
  -H "Authorization: Bearer SEU_TOKEN_ADMIN" \
  -o emails-outbox.csv
```

Metricas agregadas do outbox com filtros:

```bash
curl -s "http://localhost:5231/api/notificacoes/emails/metricas?tipoNotificacao=ServicoSolicitado&emailDestino=teste.local&dataCriacaoInicial=2026-03-12T00:00:00Z&dataCriacaoFinal=2026-03-12T23:59:59Z" \
  -H "Authorization: Bearer SEU_TOKEN_ADMIN"
```

Resumo operacional do outbox com fila pronta e aguardando retry:

```bash
curl -s "http://localhost:5231/api/notificacoes/emails/resumo-operacional?usuarioId=USUARIO_ID&tipoNotificacao=ServicoSolicitado&dataCriacaoInicial=2026-03-12T00:00:00Z&dataCriacaoFinal=2026-03-12T23:59:59Z" \
  -H "Authorization: Bearer SEU_TOKEN_ADMIN"
```

Serie temporal agregada por dia, tipo e status:

```bash
curl -s "http://localhost:5231/api/notificacoes/emails/metricas/serie?tipoNotificacao=ServicoSolicitado&emailDestino=teste.local&dataCriacaoInicial=2026-03-12T00:00:00Z&dataCriacaoFinal=2026-03-12T23:59:59Z" \
  -H "Authorization: Bearer SEU_TOKEN_ADMIN"
```

Agregacao por destinatario com quebra por status:

```bash
curl -s "http://localhost:5231/api/notificacoes/emails/metricas/destinatarios?tipoNotificacao=ServicoSolicitado&emailDestino=teste.local&dataCriacaoInicial=2026-03-12T00:00:00Z&dataCriacaoFinal=2026-03-12T23:59:59Z" \
  -H "Authorization: Bearer SEU_TOKEN_ADMIN"
```

Agregacao por tipo de notificacao com quebra por status:

```bash
curl -s "http://localhost:5231/api/notificacoes/emails/metricas/tipos?emailDestino=teste.local&dataCriacaoInicial=2026-03-12T00:00:00Z&dataCriacaoFinal=2026-03-12T23:59:59Z" \
  -H "Authorization: Bearer SEU_TOKEN_ADMIN"
```

Dashboard consolidado do outbox em uma chamada:

```bash
curl -s "http://localhost:5231/api/notificacoes/emails/dashboard?tipoNotificacao=ServicoSolicitado&emailDestino=teste.local&dataCriacaoInicial=2026-03-12T00:00:00Z&dataCriacaoFinal=2026-03-12T23:59:59Z" \
  -H "Authorization: Bearer SEU_TOKEN_ADMIN"
```

Drill-down do outbox por usuario:

```bash
curl -s "http://localhost:5231/api/notificacoes/emails/usuarios/USUARIO_ID/dashboard?tipoNotificacao=ServicoSolicitado&dataCriacaoInicial=2026-03-12T00:00:00Z&dataCriacaoFinal=2026-03-12T23:59:59Z" \
  -H "Authorization: Bearer SEU_TOKEN_ADMIN"
```

## Troubleshooting

### `401 Webhook não autorizado.`

Verifique:

- se o header de assinatura esta correto para o provedor
- se o segredo configurado no ambiente bate com o usado para gerar o HMAC
- se a assinatura foi calculada sobre o corpo bruto exato da requisicao

Headers esperados:

- padrao: `X-Webhook-Signature`
- asaas: `X-Asaas-Signature`

### `400 Payload inválido.`

Verifique:

- se o JSON enviado corresponde ao formato esperado pelo provedor
- se `CodigoReferenciaPagamento` ou `payment.externalReference` foram enviados
- se o status recebido e mapeavel para um status interno suportado

Para `asaas`, o backend espera pelo menos:

```json
{
  "id": "evt-001",
  "event": "PAYMENT_RECEIVED",
  "payment": {
    "externalReference": "ref-001",
    "status": "RECEIVED"
  }
}
```

### `Webhook já processado.`

Isso significa que o mesmo `EventoExternoId` ja foi recebido antes.

O comportamento e esperado:

- a API responde `200`
- o evento nao e reprocessado
- a auditoria existente e reutilizada

### Webhook processou mas o impulsionamento nao mudou como esperado

Consulte primeiro:

- `GET /api/impulsionamentos/webhooks`
- `GET /api/impulsionamentos/webhooks/metricas`

Cheque especialmente:

- `CodigoReferenciaPagamento`
- `EventoExternoId`
- `StatusPagamento`
- `MensagemResultado`
- `StatusImpulsionamentoResultado`

### Campos de auditoria vazios no banco

Se `HeadersJson`, `IpOrigem`, `RequestId` ou `UserAgent` estiverem vazios:

- confirme que a migration mais recente foi aplicada
- reinicie a API para garantir que a instancia em execucao esta com o codigo novo

### Warning de `apphost` no `dotnet build`

Se aparecer aviso de arquivo em uso no `MeAjudaAi.Api` durante build:

- a API provavelmente esta rodando em paralelo
- isso nao indica erro funcional do codigo
- pare a API antes do build se quiser eliminar o warning

## Checklist de deploy

### Antes do deploy

- confirme que `dotnet build MeAjudaAi.sln` passa
- confirme que `dotnet test MeAjudaAi.sln` passa
- confirme que a CI do backend esta verde
- confirme que a API local sobe sem aplicar migrations pendentes inesperadas

### Configuracao obrigatoria

Garanta no ambiente:

- `ConnectionStrings:DefaultConnection`
- `Jwt:Issuer`
- `Jwt:Audience`
- `Jwt:Key`
- `Jwt:ExpiracaoEmMinutos`
- `Webhooks:Pagamentos:Segredo`

Se usar provedores especificos, configure tambem:

- `Webhooks:Pagamentos:Provedores:asaas:Segredo`
- `Webhooks:Pagamentos:Provedores:asaas:HeaderAssinatura`

### Banco de dados

Antes de subir a nova versao da API:

```bash
cd backend
dotnet ef database update --project src/MeAjudaAi.Infrastructure --startup-project src/MeAjudaAi.Api
```

Depois valide:

- migrations aplicadas em `__EFMigrationsHistory`
- tabela `webhooks_pagamentos_impulsionamentos_eventos` existente
- colunas de auditoria mais recentes presentes

### Verificacoes depois do deploy

Teste pelo menos:

- login admin
- listagem de planos de impulsionamento
- contratacao de impulsionamento
- webhook padrao com HMAC
- webhook `asaas` com payload bruto
- `GET /api/impulsionamentos/webhooks`
- `GET /api/impulsionamentos/webhooks/metricas`

### Itens de seguranca

- nao use os segredos default de desenvolvimento em producao
- use uma `Jwt:Key` forte e exclusiva do ambiente
- restrinja quem pode chamar os endpoints admin
- se houver proxy ou gateway, preserve IP e headers necessarios para auditoria

### Rollback

Se precisar voltar a versao da API:

- confirme compatibilidade da versao anterior com o schema atual
- se houver migration nova, avalie rollback do banco antes de voltar o binario
- revalide login, webhook e endpoints admin apos o rollback

## Testes

- Os testes unitários usam banco em memória quando necessário.
- Os testes de integração sobem a API com `WebApplicationFactory` e SQLite em memória.
- A suíte de integração não depende de PostgreSQL para rodar no CI.

## CI

Há uma workflow em `.github/workflows/backend-ci.yml` que executa:

```bash
dotnet restore MeAjudaAi.sln
dotnet build MeAjudaAi.sln --configuration Release --no-restore
dotnet test MeAjudaAi.sln --configuration Release --no-build
```
