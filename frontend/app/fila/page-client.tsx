'use client';

import { useRouter, useSearchParams } from 'next/navigation';
import { FormEvent, useEffect, useState } from 'react';
import { QueueExecutionCard } from '@/components/QueueExecutionCard';
import { useAdminJobs } from '@/hooks/useAdminJobs';
import { useQueueActions } from '@/hooks/useQueueActions';
import { ApiError } from '@/lib/api';
import { useToast } from '@/providers/toast-provider';

const statusOptions = ['', 'Pendente', 'Processando', 'Sucesso', 'Falha', 'Cancelado'];

function Placeholder({ label }: { label: string }) {
  return (
    <div className="rounded-[1.5rem] border border-dashed border-slate-300 bg-white/60 p-5 text-sm text-slate-400">
      {label}
    </div>
  );
}

export function QueuePageClient() {
  const router = useRouter();
  const searchParams = useSearchParams();

  const [jobId, setJobId] = useState(searchParams.get('jobId') ?? '');
  const [status, setStatus] = useState(searchParams.get('status') ?? '');
  const [limit, setLimit] = useState(searchParams.get('limit') ?? '20');

  useEffect(() => {
    setJobId(searchParams.get('jobId') ?? '');
    setStatus(searchParams.get('status') ?? '');
    setLimit(searchParams.get('limit') ?? '20');
  }, [searchParams]);

  const { data, isLoading, error } = useAdminJobs({
    limit: Number(limit) || 20,
    jobId: searchParams.get('jobId') ?? '',
    status: searchParams.get('status') ?? '',
  });
  const {
    busyAction,
    clearFeedback,
    feedback,
    isPending,
    cancelExecution,
    processQueue,
    refreshQueueState,
    reopenExecution,
  } = useQueueActions();
  const { showToast } = useToast();

  async function handleProcessQueue() {
    clearFeedback();

    try {
      await processQueue();
      showToast({
        title: 'Fila processada',
        message: 'A fila foi processada e os dados foram atualizados.',
        variant: 'success',
      });
    } catch (actionError) {
      const message = actionError instanceof ApiError ? actionError.message : 'Nao foi possivel processar a fila.';
      showToast({
        title: 'Falha ao processar fila',
        message,
        variant: 'error',
      });
    }
  }

  async function handleCancel(execucaoId: string) {
    clearFeedback();

    try {
      await cancelExecution(execucaoId);
      showToast({
        title: 'Execucao cancelada',
        message: 'O estado da fila foi atualizado.',
        variant: 'success',
      });
    } catch (actionError) {
      const message = actionError instanceof ApiError ? actionError.message : 'Nao foi possivel cancelar a execucao.';
      showToast({
        title: 'Falha ao cancelar execucao',
        message,
        variant: 'error',
      });
    }
  }

  async function handleReopen(execucaoId: string) {
    clearFeedback();

    try {
      await reopenExecution(execucaoId);
      showToast({
        title: 'Execucao reaberta',
        message: 'A execucao voltou para o fluxo operacional.',
        variant: 'success',
      });
    } catch (actionError) {
      const message = actionError instanceof ApiError ? actionError.message : 'Nao foi possivel reabrir a execucao.';
      showToast({
        title: 'Falha ao reabrir execucao',
        message,
        variant: 'error',
      });
    }
  }

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    const params = new URLSearchParams();
    if (jobId.trim()) {
      params.set('jobId', jobId.trim());
    }
    if (status.trim()) {
      params.set('status', status.trim());
    }
    if (limit.trim()) {
      params.set('limit', limit.trim());
    }

    const query = params.toString();
    router.replace(query ? `/fila?${query}` : '/fila');
  }

  return (
    <main className="min-h-screen bg-[linear-gradient(180deg,#eef1f6_0%,#f6efe4_100%)] px-4 py-6">
      <div className="mx-auto flex max-w-5xl flex-col gap-6">
        <section className="rounded-[2rem] border border-white/60 bg-white/75 p-5 shadow-[0_20px_60px_rgba(15,23,42,0.08)]">
          <p className="text-xs font-semibold uppercase tracking-[0.2em] text-slate-500">Fila detalhada</p>
          <h2 className="mt-2 text-2xl font-semibold text-slate-900">Execucoes recentes</h2>
          <p className="mt-2 max-w-2xl text-sm leading-6 text-slate-600">
            Filtre por job, status e quantidade para inspecionar o comportamento operacional da fila.
          </p>

          <div className="mt-5 flex flex-wrap gap-3">
            <button
              type="button"
              onClick={handleProcessQueue}
              disabled={busyAction === 'processar-fila' || isPending}
              className="rounded-full bg-slate-900 px-4 py-2 text-sm font-semibold text-white transition hover:bg-slate-800 disabled:cursor-not-allowed disabled:opacity-50"
            >
              {busyAction === 'processar-fila' || isPending ? 'Processando...' : 'Processar fila agora'}
            </button>

            <button
              type="button"
              onClick={() => void refreshQueueState()}
              className="rounded-full border border-slate-200 bg-white px-4 py-2 text-sm font-medium text-slate-700 transition hover:bg-slate-50"
            >
              Atualizar dados
            </button>
          </div>

          <form className="mt-5 grid gap-3 md:grid-cols-[1.4fr_1fr_120px_auto]" onSubmit={handleSubmit}>
            <input
              className="rounded-2xl border border-slate-200 bg-white px-4 py-3 text-sm text-slate-900 outline-none transition focus:border-slate-900"
              placeholder="jobId, ex: emails-outbox"
              value={jobId}
              onChange={(event) => setJobId(event.target.value)}
            />

            <select
              className="rounded-2xl border border-slate-200 bg-white px-4 py-3 text-sm text-slate-900 outline-none transition focus:border-slate-900"
              value={status}
              onChange={(event) => setStatus(event.target.value)}
            >
              {statusOptions.map((option) => (
                <option key={option || 'todos'} value={option}>
                  {option || 'Todos os status'}
                </option>
              ))}
            </select>

            <input
              className="rounded-2xl border border-slate-200 bg-white px-4 py-3 text-sm text-slate-900 outline-none transition focus:border-slate-900"
              placeholder="20"
              value={limit}
              onChange={(event) => setLimit(event.target.value)}
              inputMode="numeric"
            />

            <button
              className="rounded-2xl bg-slate-900 px-4 py-3 text-sm font-semibold text-white transition hover:bg-slate-800"
              type="submit"
            >
              Aplicar
            </button>
          </form>

          {feedback ? (
            <div className="mt-4 rounded-[1.25rem] border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-800">
              {feedback}
            </div>
          ) : null}
        </section>

        {isLoading ? (
          <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-3">
            {Array.from({ length: 3 }).map((_, idx) => (
              <Placeholder key={idx} label="Carregando execucoes..." />
            ))}
          </div>
        ) : error ? (
          <Placeholder label="Falha ao carregar a fila." />
        ) : !data?.length ? (
          <Placeholder label="Nenhuma execucao encontrada para os filtros atuais." />
        ) : (
          <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-3">
            {data.map((item) => (
              <QueueExecutionCard
                key={item.execucaoId}
                item={item}
                isBusy={busyAction === `cancelar:${item.execucaoId}` || busyAction === `reabrir:${item.execucaoId}`}
                onCancel={handleCancel}
                onReopen={handleReopen}
              />
            ))}
          </div>
        )}
      </div>
    </main>
  );
}
