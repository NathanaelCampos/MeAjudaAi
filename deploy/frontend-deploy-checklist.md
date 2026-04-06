# Frontend Deploy Checklist

## Variáveis
- Defina `NEXT_PUBLIC_API_BASE_URL` apontando para a API publicada.
- Em ambiente local com backend fora do container, use `http://host.docker.internal:5231` no compose do frontend.

## Validação antes do deploy
- Rode `cd frontend && npm run build`
- Rode `cd frontend && npm run test:run`
- Confirme que o backend está online e aceitando autenticação em `/api/auth/login`
- Confirme no GitHub Actions que o workflow `Frontend CI` passou:
  - job `frontend-quality`
  - job `frontend-docker-smoke`
- Baixe o artefato `frontend-standalone-bundle` quando precisar inspecionar o build gerado no CI

## Docker
- Build manual:
  - `cd frontend && docker build -t meajudaai-frontend .`
- Subida via compose:
  - `cd devops && NEXT_PUBLIC_API_BASE_URL=http://host.docker.internal:5231 docker compose -f docker-compose.frontend.yml up -d --build`
- Proxy unificado opcional:
  - `cd devops && GATEWAY_PORT=8080 NEXT_PUBLIC_API_BASE_URL= docker compose -f docker-compose.gateway.yml up -d --build`
  - suba o backend com binding externo:
    `cd backend && dotnet run --project src/MeAjudaAi.Api --urls http://0.0.0.0:5231`

## Verificações pós-subida
- Acesse `http://localhost:3000`
- Faça login com uma conta válida
- Verifique:
  - `/explorar`
  - `/profissionais/[id]`
  - `/servicos`
  - `/servicos/[id]`
  - `/jobs` e demais rotas admin, se o usuário for administrador
- Confirme no DevTools que as chamadas usam o host configurado em `NEXT_PUBLIC_API_BASE_URL`
- Se estiver usando o gateway unificado, valide também:
  - `http://localhost:8080`
  - `http://localhost:8080/api/...`
  - `http://localhost:8080/swagger`
  - `http://localhost:8080/uploads/...`

## PWA
- Abra o app no celular ou DevTools mobile
- Confirme que `manifest.webmanifest` carrega
- Confirme que o prompt de instalação aparece quando elegível
- Confirme que os ícones em `frontend/public/` estão sendo servidos corretamente
