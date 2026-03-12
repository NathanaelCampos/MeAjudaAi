# Me Ajuda Ai

Repositório do projeto "Me Ajuda Ai".

## Estrutura

- `backend`: API .NET 8 com Clean Architecture simplificada, EF Core e testes.
- `devops`: arquivos de infraestrutura local, incluindo `docker-compose.yml` do PostgreSQL.

## Como rodar

Suba o banco:

```bash
docker compose -f devops/docker-compose.yml up -d
```

Depois rode a API:

```bash
cd backend
dotnet restore MeAjudaAi.sln
dotnet run --project src/MeAjudaAi.Api
```

## Testes

No diretório `backend`:

```bash
dotnet test MeAjudaAi.sln
```

Os testes de integração usam `WebApplicationFactory` com SQLite em memória, então não dependem de PostgreSQL no CI.
