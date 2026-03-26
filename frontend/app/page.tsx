import { JobCard } from '@/components/JobCard';
import { MetricsPanel } from '@/components/MetricsPanel';
import { AlertsCarousel } from '@/components/AlertsCarousel';
import { useAdminJobs } from '@/hooks/useAdminJobs';
import { useMetrics } from '@/hooks/useMetrics';

export default function HomePage() {
  const { data: filas, isLoading: loadingFila } = useAdminJobs(6);
  const { data: metricas } = useMetrics();

  return (
    <main className="min-h-screen p-4 space-y-6">
      <section>
        <h1 className="text-2xl font-bold">Dashboard de background jobs</h1>
        <p className="text-sm text-neutral-500">Monitoramento mobile e alertas em tempo real.</p>
      </section>

      <section className="space-y-3">
        <MetricsPanel
          pendentes={metricas?.totalPendentes ?? 0}
          processando={metricas?.totalProcessando ?? 0}
          sucesso={metricas?.totalSucesso ?? 0}
          falhas={metricas?.totalFalhas ?? 0}
          cancelados={metricas?.totalCancelados ?? 0}
          tempoFila={metricas?.tempoMedioFilaSegundos ?? 0}
          tempoProcessamento={metricas?.tempoMedioProcessamentoSegundos ?? 0}
        />
      </section>

      <section className="space-y-3">
        <h2 className="text-lg font-semibold">Alertas ativos</h2>
        {loadingFila ? <p className="text-sm text-neutral-500">Carregando alertas...</p> : <AlertsCarousel alerts={filas ?? []} />}
      </section>

      <section className="space-y-3">
        <h2 className="text-lg font-semibold">Fila recente</h2>
        <div className="grid gap-4 md:grid-cols-2">
          {loadingFila
            ? Array.from({ length: 2 }).map((_, idx) => (
                <div key={idx} className="rounded-2xl border border-dashed border-neutral-200 p-4 text-center text-sm text-neutral-400">
                  Carregando...
                </div>
              ))
            : filas?.map((item) => (
                <JobCard
                  key={item.execucaoId}
                  jobId={item.jobId}
                  nome={item.nomeJob}
                  status={item.status === 'Pendente' ? 'Ativo' : 'Inativo'}
                  pendentes={item.tentativasProcessamento}
                  falhas={item.status === 'Falha' ? 1 : 0}
                />
              ))}
        </div>
      </section>
    </main>
  );
}
