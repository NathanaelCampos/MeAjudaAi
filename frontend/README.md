# Frontend mobile-first blueprint

Este projeto será construído com **React + Next.js** (pode ser iniciado com `npx create-next-app frontend --typescript`) + **Tailwind CSS** para garantir UI moderna, responsiva e leve. O foco é suportar qualquer tela mobile e, opcionalmente, virar um PWA.

## Stack proposta
- **Next.js 15 (app router)** para renderização híbrida (SSR/ISR para páginas críticas).\n- **Tailwind CSS** + `@headlessui/react`/`Radix` para componentes acessíveis.\n- **SWR** ou React Query para cache e revalidação das chamadas aos endpoints `/api/admin/jobs/...`.
- **Storybook** (opcional) para iterar com designers mobile.
- **Vercel** ou outro host com edge caching, mas o foco é mobile-first e offline-friendly.

## Estrutura inicial sugerida

```
frontend/
├── app/                # Next.js App Router
│   ├── page.tsx        # Entrada principal (Dashboard)
│   └── hooks/          # Hooks compartilhados (useJobs, useMetrics)
├── src/
│   ├── components/
│   │   ├── JobCard.tsx
│   │   ├── MetricsPanel.tsx
│   │   └── AlertsCarousel.tsx
│   └── hooks/
│       ├── useAdminJobs.ts
│       └── useAlertsHistory.ts
├── styles/
│   └── globals.css     # Tailwind + resets para mobile
├── public/             # ícones, logos e assets mobile
├── README.md (este)
└── package.json        # scripts e dependências (next, tailwind, swr)
```

## Consumindo o backend
- **API hooks** (ex.: `frontend/src/hooks/useAdminJobs.ts`) devem chamar `fetch('/api/admin/jobs/fila?limit=15')` e expor `status`, `data`, `error`.
- Use `SWR` com `fetcher` padrão:
  ```ts
  const fetcher = (url: string) => fetch(url).then((res) => res.json());
  const { data, error, isLoading } = useSWR('/api/admin/jobs/fila/metricas', fetcher);
  ```
- Os componentes devem tratar estados: loading skeletons, empty states (sem alertas) e erros (mensagem amigável). Sempre priorize o conteúdo principal no topo da tela.

## Telas iniciais prioritárias

1. **Dashboard de jobs** (cards com `BackgroundJobAdminItemResponse` + status).\n2. **Fila de execuções** (`/fila` com `BackgroundJobFilaItemResponse`, botões para cancelar/reprocessar no futuro).\n3. **Métricas em tempo real** (`/fila/metricas` com indicadores de pendentes/processando/sucesso).\n4. **Alertas ativos** (`/fila/alertas` com cores/mensagens).\n5. **Histórico + retries** para detalhes das execuções.

## Próximos passos

1. `cd frontend && npx create-next-app@latest --typescript --app --tailwind`\n+2. Copiar `backend/src/MeAjudaAi.Api/MeAjudaAi.Api.http` para `frontend/docs/api-contracts.md` como referência de payloads.\n+3. Implementar hooks `useAdminJobs`/`useAlertsHistory` que encapsulam os fetches com SWR.\n+4. Criar storyboards (mobile) com cards/indicadores usando Tailwind Utilities (`flex`, `gap`, `border`, `shadow`).\n5. Habilitar PWA: `next-pwa` + manifest com iconografia leve.

Precisa que eu avance gerando um `package.json` + componentes iniciais (JobCard, MetricsPanel, AlertsCarousel) com alguns dados estáticos e chamadas SWR mockadas?"
