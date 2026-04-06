# Frontend

Frontend mobile-first do Me Ajuda Ai, construído com **Next.js 16**, **React 19**, **Tailwind CSS** e **SWR**.

## O que já existe
- descoberta pública de profissionais
- perfil público do profissional
- cadastro, login e onboarding
- fluxo de solicitação e acompanhamento de serviços
- área do profissional
- avaliação de serviço
- painel admin operacional
- PWA com `manifest` e prompt de instalação
- suíte inicial de testes com `Vitest`

## Variáveis
Crie um `.env.local` com base em [`.env.example`](/home/nathanael-campos/dev/src/me-ajuda-ai/frontend/.env.example).

Variável principal:
- `NEXT_PUBLIC_API_BASE_URL`

Exemplo local:
```bash
NEXT_PUBLIC_API_BASE_URL=http://localhost:5231
PORT=3000
```

## Desenvolvimento
```bash
cd frontend
npm install
npm run dev
```

## Qualidade
```bash
cd frontend
npm run build
npm run test:run
```

## CI/CD
- Workflow dedicado: `.github/workflows/frontend-ci.yml`
- O pipeline executa:
  - `npm ci`
  - `npm run test:run`
  - `npm run build`
  - upload do artefato `frontend-standalone-bundle`
  - smoke test do `Dockerfile` com `docker build`
- Esse workflow dispara em `push`, `pull_request` e `workflow_dispatch` quando houver mudanças relevantes no frontend.

## Deploy com Docker
O projeto está preparado para deploy com imagem standalone do Next.

Build manual:
```bash
cd frontend
docker build -t meajudaai-frontend .
```

Execução manual:
```bash
docker run --rm -p 3000:3000 \
  -e NEXT_PUBLIC_API_BASE_URL=http://host.docker.internal:5231 \
  meajudaai-frontend
```

Execução via compose:
```bash
cd devops
NEXT_PUBLIC_API_BASE_URL=http://host.docker.internal:5231 \
docker compose -f docker-compose.frontend.yml up -d --build
```

Checklist formal de deploy:
- [frontend-deploy-checklist.md](/home/nathanael-campos/dev/src/me-ajuda-ai/deploy/frontend-deploy-checklist.md)

## Estrutura relevante
```text
frontend/
├── app/
├── public/
├── src/
│   ├── components/
│   ├── hooks/
│   ├── lib/
│   ├── providers/
│   └── test/
├── Dockerfile
├── .dockerignore
├── .env.example
└── vitest.config.ts
```
