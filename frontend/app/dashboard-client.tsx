'use client';

import { AlertsCarousel } from '@/components/AlertsCarousel';
import { JobCard } from '@/components/JobCard';
import { MetricsPanel } from '@/components/MetricsPanel';
import { useAdminJobs } from '@/hooks/useAdminJobs';
import { useAlerts } from '@/hooks/useAlerts';
import { useMetrics } from '@/hooks/useMetrics';

function LoadingBlock({ label }: { label: string }) {
  return (
    <div className="rounded-[1.5rem] border border-dashed border-slate-300 bg-white/60 p-5 text-sm text-slate-400">
      {label}
    </div>
  );
}

export function DashboardClient() {
  const { data: fila, isLoading: loadingFila, error: erroFila } = useAdminJobs({ limit: 6 });
  const { data: metricas, isLoading: loadingMetricas, error: erroMetricas } = useMetrics();
  const { data: alertas, isLoading: loadingAlertas, error: erroAlertas } = useAlerts();

  return (
    <main className="min-h-screen bg-[radial-gradient(circle_at_top,#f8ead7_0%,#f5f2ea_35%,#eef1f6_100%)] px-4 py-6 text-slate-900">
      <div className="mx-auto flex max-w-5xl flex-col gap-6">
        <section className="overflow-hidden rounded-[2rem] border border-white/60 bg-white/70 p-5 shadow-[0_20px_70px_rgba(15,23,42,0.08)] backdrop-blur">
          <p className="text-xs font-semibold uppercase tracking-[0.24em] text-amber-700">Me Ajuda Ai</p>
          <div className="mt-3 flex flex-col gap-3">
            <h1 className="max-w-xl text-3xl font-semibold leading-tight sm:text-4xl">
              Observabilidade de jobs com leitura clara para celular.
            </h1>
            <p className="max-w-2xl text-sm leading-6 text-slate-600">
              A home prioriza alertas, saúde da fila e últimas execuções. O foco aqui é leitura rápida em tela pequena, com blocos densos e hierarquia forte.
            </p>
          </div>
        </section>

        <section className="grid gap-4 lg:grid-cols-[1.2fr_0.8fr]">
          <div className="rounded-[2rem] border border-white/60 bg-white/80 p-4 shadow-[0_20px_60px_rgba(15,23,42,0.07)]">
            {loadingMetricas ? (
              <LoadingBlock label="Carregando métricas..." />
            ) : erroMetricas ? (
              <LoadingBlock label="Falha ao carregar métricas." />
            ) : (
              <MetricsPanel
                pendentes={metricas?.totalPendentes ?? 0}
                processando={metricas?.totalProcessando ?? 0}
                sucesso={metricas?.totalSucesso ?? 0}
                falhas={metricas?.totalFalhas ?? 0}
                cancelados={metricas?.totalCancelados ?? 0}
                tempoFila={metricas?.tempoMedioFilaSegundos ?? 0}
                tempoProcessamento={metricas?.tempoMedioProcessamentoSegundos ?? 0}
              />
            )}
          </div>

          <div className="rounded-[2rem] border border-white/60 bg-[#1f2937] p-4 text-white shadow-[0_20px_60px_rgba(15,23,42,0.16)]">
            <p className="text-xs font-semibold uppercase tracking-[0.2em] text-amber-300">Alertas ativos</p>
            <div className="mt-4">
              {loadingAlertas ? (
                <LoadingBlock label="Carregando alertas..." />
              ) : erroAlertas ? (
                <LoadingBlock label="Falha ao carregar alertas." />
              ) : (
                <AlertsCarousel alerts={alertas ?? []} />
              )}
            </div>
          </div>
        </section>

        <section className="rounded-[2rem] border border-white/60 bg-white/80 p-4 shadow-[0_20px_60px_rgba(15,23,42,0.07)]">
          <div className="mb-4 flex items-center justify-between gap-3">
            <div>
              <p className="text-xs font-semibold uppercase tracking-[0.2em] text-slate-500">Fila recente</p>
              <h2 className="mt-1 text-xl font-semibold">Ultimas execucoes monitoradas</h2>
            </div>
          </div>

          {loadingFila ? (
            <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-3">
              {Array.from({ length: 3 }).map((_, idx) => (
                <LoadingBlock key={idx} label="Carregando execucao..." />
              ))}
            </div>
          ) : erroFila ? (
            <LoadingBlock label="Falha ao carregar a fila." />
          ) : !fila?.length ? (
            <LoadingBlock label="Nenhuma execucao encontrada." />
          ) : (
            <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-3">
              {fila.map((item) => (
                <JobCard
                  key={item.execucaoId}
                  jobId={item.jobId}
                  nome={item.nomeJob}
                  status={item.status}
                  origem={item.origem}
                  processados={item.registrosProcessados}
                  tentativas={item.tentativasProcessamento}
                  mensagemResultado={item.mensagemResultado}
                  criadoEm={item.dataCriacao}
                />
              ))}
            </div>
          )}
        </section>
      </div>
    </main>
  );
}
