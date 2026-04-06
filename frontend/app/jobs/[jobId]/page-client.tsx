'use client';

import Link from 'next/link';
import { useMemo } from 'react';
import { HistoryCard } from '@/components/HistoryCard';
import { QueueExecutionCard } from '@/components/QueueExecutionCard';
import { RetryLogCard } from '@/components/RetryLogCard';
import { useAdminJobs } from '@/hooks/useAdminJobs';
import { useAlerts } from '@/hooks/useAlerts';
import { useAlertsHistory } from '@/hooks/useAlertsHistory';
import { useJobCatalog } from '@/hooks/useJobCatalog';
import { useRetryLogs } from '@/hooks/useRetryLogs';

function Placeholder({ label }: { label: string }) {
  return (
    <div className="rounded-[1.5rem] border border-dashed border-slate-300 bg-white/60 p-5 text-sm text-slate-400">
      {label}
    </div>
  );
}

function formatDate(value?: string | null) {
  if (!value) {
    return '-';
  }

  return new Date(value).toLocaleString('pt-BR');
}

export function JobDetailPageClient({ jobId }: { jobId: string }) {
  const normalizedJobId = jobId.trim().toLowerCase();
  const { data: jobs, isLoading: loadingJobs, error: errorJobs } = useJobCatalog();
  const { data: queue, isLoading: loadingQueue, error: errorQueue } = useAdminJobs({
    jobId,
    limit: 12,
  });
  const { data: alerts, isLoading: loadingAlerts, error: errorAlerts } = useAlerts();
  const { data: history, isLoading: loadingHistory, error: errorHistory } = useAlertsHistory(21);
  const { data: retries, isLoading: loadingRetries, error: errorRetries } = useRetryLogs(80);

  const job = useMemo(
    () => jobs?.find((item) => item.jobId.toLowerCase() === normalizedJobId) ?? null,
    [jobs, normalizedJobId],
  );
  const activeAlerts = useMemo(
    () => (alerts ?? []).filter((item) => item.jobId.toLowerCase() === normalizedJobId),
    [alerts, normalizedJobId],
  );
  const historyItems = useMemo(
    () => (history ?? []).filter((item) => item.jobId.toLowerCase() === normalizedJobId),
    [history, normalizedJobId],
  );
  const retryItems = useMemo(
    () => (retries ?? []).filter((item) => item.jobId.toLowerCase() === normalizedJobId),
    [retries, normalizedJobId],
  );

  const latestExecution = queue?.[0] ?? null;
  const pendingCount = (queue ?? []).filter((item) => item.status === 'Pendente').length;
  const failedCount = (queue ?? []).filter((item) => item.status === 'Falha').length;

  const isLoading =
    loadingJobs || loadingQueue || loadingAlerts || loadingHistory || loadingRetries;
  const hasError =
    errorJobs || errorQueue || errorAlerts || errorHistory || errorRetries;

  return (
    <main className="min-h-screen bg-[linear-gradient(180deg,#f7f3ea_0%,#eef1f6_100%)] px-4 py-6">
      <div className="mx-auto flex max-w-5xl flex-col gap-6">
        <section className="rounded-[2rem] border border-white/60 bg-white/75 p-5 shadow-[0_20px_60px_rgba(15,23,42,0.08)]">
          <div className="flex flex-wrap items-center justify-between gap-3">
            <div>
              <p className="text-xs font-semibold uppercase tracking-[0.2em] text-slate-500">Detalhe do job</p>
              <h2 className="mt-2 text-2xl font-semibold text-slate-900">{job?.nome ?? jobId}</h2>
              <p className="mt-2 text-sm leading-6 text-slate-600">
                Visao consolidada de fila, alertas, historico e retries para <strong>{jobId}</strong>.
              </p>
            </div>
            <Link
              href="/jobs"
              className="rounded-full border border-slate-200 bg-white px-4 py-2 text-sm font-medium text-slate-700 transition hover:bg-slate-50"
            >
              Voltar ao catalogo
            </Link>
          </div>

          <div className="mt-5 grid gap-3 sm:grid-cols-2 xl:grid-cols-4">
            <div className="rounded-[1.25rem] bg-slate-50 px-4 py-3">
              <p className="text-[11px] font-semibold uppercase tracking-[0.18em] text-slate-500">Status</p>
              <p className="mt-2 text-xl font-semibold text-slate-900">{job?.ultimoStatus ?? '-'}</p>
            </div>
            <div className="rounded-[1.25rem] bg-slate-50 px-4 py-3">
              <p className="text-[11px] font-semibold uppercase tracking-[0.18em] text-slate-500">Pendentes</p>
              <p className="mt-2 text-xl font-semibold text-slate-900">{pendingCount}</p>
            </div>
            <div className="rounded-[1.25rem] bg-slate-50 px-4 py-3">
              <p className="text-[11px] font-semibold uppercase tracking-[0.18em] text-slate-500">Falhas recentes</p>
              <p className="mt-2 text-xl font-semibold text-slate-900">{failedCount}</p>
            </div>
            <div className="rounded-[1.25rem] bg-slate-50 px-4 py-3">
              <p className="text-[11px] font-semibold uppercase tracking-[0.18em] text-slate-500">Retries</p>
              <p className="mt-2 text-xl font-semibold text-slate-900">{retryItems.length}</p>
            </div>
          </div>
        </section>

        {isLoading ? (
          <Placeholder label="Carregando consolidado do job..." />
        ) : hasError ? (
          <Placeholder label="Falha ao carregar os detalhes do job." />
        ) : (
          <>
            <section className="grid gap-4 lg:grid-cols-[1.2fr_0.8fr]">
              <div className="rounded-[2rem] border border-white/60 bg-white/80 p-4 shadow-[0_20px_60px_rgba(15,23,42,0.07)]">
                <p className="text-xs font-semibold uppercase tracking-[0.2em] text-slate-500">Ultima execucao</p>
                <div className="mt-4">
                  {latestExecution ? (
                    <QueueExecutionCard item={latestExecution} />
                  ) : (
                    <Placeholder label="Nenhuma execucao recente para este job." />
                  )}
                </div>
              </div>

              <div className="rounded-[2rem] border border-white/60 bg-[#1f2937] p-4 text-white shadow-[0_20px_60px_rgba(15,23,42,0.16)]">
                <p className="text-xs font-semibold uppercase tracking-[0.2em] text-amber-300">Alertas ativos</p>
                {activeAlerts.length ? (
                  <div className="mt-4 space-y-3">
                    {activeAlerts.map((alert) => (
                      <div key={`${alert.jobId}-${alert.nivelAlerta}-${alert.mensagem}`} className="rounded-[1.25rem] bg-white/10 p-4">
                        <div className="flex items-center justify-between gap-3">
                          <p className="text-sm font-semibold">{alert.nivelAlerta}</p>
                          <span className="rounded-full bg-white/10 px-3 py-1 text-[11px] font-semibold uppercase tracking-wide">
                            {alert.totalPendentes} pend.
                          </span>
                        </div>
                        <p className="mt-3 text-sm leading-6 text-white/80">{alert.mensagem}</p>
                        <p className="mt-3 text-xs text-white/60">
                          Fila media: {alert.tempoMedioFilaSegundos.toFixed(0)}s | Processamento medio:{' '}
                          {alert.tempoMedioProcessamentoSegundos.toFixed(0)}s
                        </p>
                      </div>
                    ))}
                  </div>
                ) : (
                  <div className="mt-4 rounded-[1.25rem] border border-white/10 bg-white/5 p-4 text-sm text-white/70">
                    Nenhum alerta ativo para este job.
                  </div>
                )}
              </div>
            </section>

            <section className="rounded-[2rem] border border-white/60 bg-white/80 p-4 shadow-[0_20px_60px_rgba(15,23,42,0.07)]">
              <div className="flex items-center justify-between gap-3">
                <div>
                  <p className="text-xs font-semibold uppercase tracking-[0.2em] text-slate-500">Timeline</p>
                  <h3 className="mt-1 text-xl font-semibold text-slate-900">Execucoes recentes</h3>
                </div>
                <p className="text-sm text-slate-500">{queue?.length ?? 0} item(ns)</p>
              </div>

              <div className="mt-4 grid gap-4 md:grid-cols-2 xl:grid-cols-3">
                {queue?.length ? (
                  queue.map((item) => <QueueExecutionCard key={item.execucaoId} item={item} />)
                ) : (
                  <Placeholder label="Nenhuma execucao listada para este job." />
                )}
              </div>
            </section>

            <section className="grid gap-4 lg:grid-cols-2">
              <div className="rounded-[2rem] border border-white/60 bg-white/80 p-4 shadow-[0_20px_60px_rgba(15,23,42,0.07)]">
                <div className="flex items-center justify-between gap-3">
                  <div>
                    <p className="text-xs font-semibold uppercase tracking-[0.2em] text-slate-500">Historico</p>
                    <h3 className="mt-1 text-xl font-semibold text-slate-900">Ultimos dias</h3>
                  </div>
                  <Link
                    href={`/historico?jobId=${encodeURIComponent(jobId)}&dias=21`}
                    className="rounded-full border border-slate-200 bg-white px-3 py-2 text-sm font-medium text-slate-700 transition hover:bg-slate-50"
                  >
                    Ver tela completa
                  </Link>
                </div>

                <div className="mt-4 grid gap-4">
                  {historyItems.length ? (
                    historyItems.slice(0, 4).map((item) => (
                      <HistoryCard key={`${item.jobId}-${item.data}`} item={item} />
                    ))
                  ) : (
                    <Placeholder label="Sem historico de alertas para este job." />
                  )}
                </div>
              </div>

              <div className="rounded-[2rem] border border-white/60 bg-white/80 p-4 shadow-[0_20px_60px_rgba(15,23,42,0.07)]">
                <div className="flex items-center justify-between gap-3">
                  <div>
                    <p className="text-xs font-semibold uppercase tracking-[0.2em] text-slate-500">Retries</p>
                    <h3 className="mt-1 text-xl font-semibold text-slate-900">Eventos recentes</h3>
                  </div>
                  <Link
                    href={`/retries?jobId=${encodeURIComponent(jobId)}&top=80`}
                    className="rounded-full border border-slate-200 bg-white px-3 py-2 text-sm font-medium text-slate-700 transition hover:bg-slate-50"
                  >
                    Ver tela completa
                  </Link>
                </div>

                <div className="mt-4 grid gap-4">
                  {retryItems.length ? (
                    retryItems.slice(0, 4).map((item) => (
                      <RetryLogCard key={`${item.execucaoId}-${item.dataCriacao}-${item.tipo}`} item={item} />
                    ))
                  ) : (
                    <Placeholder label="Sem retries recentes para este job." />
                  )}
                </div>
              </div>
            </section>

            <section className="rounded-[2rem] border border-white/60 bg-white/80 p-4 shadow-[0_20px_60px_rgba(15,23,42,0.07)]">
              <p className="text-xs font-semibold uppercase tracking-[0.2em] text-slate-500">Resumo tecnico</p>
              <div className="mt-4 grid gap-3 sm:grid-cols-2 xl:grid-cols-4">
                <div className="rounded-[1.25rem] bg-slate-50 px-4 py-3">
                  <p className="text-[11px] font-semibold uppercase tracking-[0.18em] text-slate-500">Intervalo</p>
                  <p className="mt-2 text-lg font-semibold text-slate-900">
                    {job?.intervaloSegundos ? `${job.intervaloSegundos}s` : '-'}
                  </p>
                </div>
                <div className="rounded-[1.25rem] bg-slate-50 px-4 py-3">
                  <p className="text-[11px] font-semibold uppercase tracking-[0.18em] text-slate-500">Em execucao</p>
                  <p className="mt-2 text-lg font-semibold text-slate-900">{job?.emExecucao ? 'Sim' : 'Nao'}</p>
                </div>
                <div className="rounded-[1.25rem] bg-slate-50 px-4 py-3">
                  <p className="text-[11px] font-semibold uppercase tracking-[0.18em] text-slate-500">Ultimo inicio</p>
                  <p className="mt-2 text-sm font-semibold text-slate-900">{formatDate(job?.ultimaExecucaoIniciadaEm)}</p>
                </div>
                <div className="rounded-[1.25rem] bg-slate-50 px-4 py-3">
                  <p className="text-[11px] font-semibold uppercase tracking-[0.18em] text-slate-500">Ultimo fim</p>
                  <p className="mt-2 text-sm font-semibold text-slate-900">{formatDate(job?.ultimaExecucaoFinalizadaEm)}</p>
                </div>
              </div>
            </section>
          </>
        )}
      </div>
    </main>
  );
}
