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
# Checagem de Dados de Suporte

Antes de liberar o frontend, confirme que o banco Postgres oficial (via `devops/docker-compose.yml`) foi migrado e que os seeders já popularam as tabelas essenciais de suporte.

1. Levante o Postgres com o compose padrão:

   ```bash
   docker compose -f devops/docker-compose.yml up -d
   ```

2. Verifique as tabelas aplicadas para garantir que as migrations rodaram:

   ```bash
   PGPASSWORD=postgres psql -h localhost -p 5433 -U postgres -d meajudaai_db -c '\dt'
   ```

3. Confirme os seeds que o frontend precisa consumir:

   ```bash
   PGPASSWORD=postgres psql -h localhost -p 5433 -U postgres -d meajudaai_db -c 'select count(*) from profissoes;'
   PGPASSWORD=postgres psql -h localhost -p 5433 -U postgres -d meajudaai_db -c 'select count(*) from planos_impulsionamento;'
   PGPASSWORD=postgres psql -h localhost -p 5433 -U postgres -d meajudaai_db -c 'select count(*) from profissionais;'
   PGPASSWORD=postgres psql -h localhost -p 5433 -U postgres -d meajudaai_db -c "select \"Id\",\"Email\",\"TipoPerfil\" from \"usuarios\" where \"Email\" = 'admin@meajudaai.local';"
   ```

   - Profissões: 5 entradas (eletricista, encanador, faxineira, jardineiro, pet sitter).
   - Planos de impulsionamento: 3 planos já documentados em `DbInitializer.cs`.
   - Profissionais: ao menos 8 registros disponíveis para testes manuais.
   - Admin ativo: `TipoPerfil = 3` garante login com `admin@meajudaai.local / Admin@123`.

4. Caso precise resetar os dados, remova o volume `devops_postgres_data` (ou o nome do volume definido no compose) e rode a aplicação ou `dotnet test` novamente; o `DbInitializer` aplica as migrations e dispara os seeds mencionados.

Esses passos garantem que os endpoints do backend (`/api/admin/jobs`, notificações e usuários) ficam disponíveis com dados reais para o time de frontend executar fluxo completo de métricas, alertas e autenticação.
