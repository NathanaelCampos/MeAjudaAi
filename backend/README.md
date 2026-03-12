# Me Ajuda Ai Backend

## Comandos principais

No diretório `backend`:

```bash
dotnet restore MeAjudaAi.sln
dotnet build MeAjudaAi.sln
dotnet test MeAjudaAi.sln
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
