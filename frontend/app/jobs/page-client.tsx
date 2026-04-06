'use client';

import { JobCatalogCard } from '@/components/JobCatalogCard';
import { useJobCatalog } from '@/hooks/useJobCatalog';
import { useJobCatalogActions } from '@/hooks/useJobCatalogActions';
import { ApiError } from '@/lib/api';
import { useToast } from '@/providers/toast-provider';

function Placeholder({ label }: { label: string }) {
  return (
    <div className="rounded-[1.5rem] border border-dashed border-slate-300 bg-white/60 p-5 text-sm text-slate-400">
      {label}
    </div>
  );
}

export function JobCatalogPageClient() {
  const { data, error, isLoading } = useJobCatalog();
  const { showToast } = useToast();
  const {
    busyAction,
    feedback,
    clearFeedback,
    executeJob,
    enqueueJob,
    cancelAll,
    scheduleJob,
    refreshAdminJobs,
  } = useJobCatalogActions();

  async function handleAction(action: () => Promise<void>, fallbackMessage: string) {
    clearFeedback();

    try {
      await action();
      showToast({
        title: 'Operacao concluida',
        message: 'O catalogo foi atualizado com o estado mais recente.',
        variant: 'success',
      });
    } catch (actionError) {
      const message = actionError instanceof ApiError ? actionError.message : fallbackMessage;
      showToast({
        title: 'Falha na operacao',
        message,
        variant: 'error',
      });
    }
  }

  return (
    <main className="min-h-screen bg-[linear-gradient(180deg,#f7f3ea_0%,#eef1f6_100%)] px-4 py-6">
      <div className="mx-auto flex max-w-5xl flex-col gap-6">
        <section className="rounded-[2rem] border border-white/60 bg-white/75 p-5 shadow-[0_20px_60px_rgba(15,23,42,0.08)]">
          <p className="text-xs font-semibold uppercase tracking-[0.2em] text-slate-500">Catalogo de jobs</p>
          <h2 className="mt-2 text-2xl font-semibold text-slate-900">Acoes por job</h2>
          <p className="mt-2 max-w-2xl text-sm leading-6 text-slate-600">
            Execute, enfileire ou cancele pendentes por identificador de job sem sair do painel mobile.
          </p>

          <div className="mt-5">
            <button
              type="button"
              onClick={() => void refreshAdminJobs()}
              className="rounded-full border border-slate-200 bg-white px-4 py-2 text-sm font-medium text-slate-700 transition hover:bg-slate-50"
            >
              Atualizar catalogo
            </button>
          </div>

          {feedback ? (
            <div className="mt-4 rounded-[1.25rem] border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-800">
              {feedback}
            </div>
          ) : null}
        </section>

        {isLoading ? (
          <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-3">
            {Array.from({ length: 3 }).map((_, idx) => (
              <Placeholder key={idx} label="Carregando jobs..." />
            ))}
          </div>
        ) : error ? (
          <Placeholder label="Falha ao carregar o catalogo de jobs." />
        ) : !data?.length ? (
          <Placeholder label="Nenhum job administrativo encontrado." />
        ) : (
          <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-3">
            {data.map((item) => (
              <JobCatalogCard
                key={item.jobId}
                item={item}
                busyAction={busyAction}
                onExecute={() => handleAction(() => executeJob(item.jobId), 'Nao foi possivel executar o job.')}
                onEnqueue={() => handleAction(() => enqueueJob(item.jobId), 'Nao foi possivel enfileirar o job.')}
                onCancelAll={() => handleAction(() => cancelAll(item.jobId), 'Nao foi possivel cancelar os pendentes do job.')}
                onSchedule={(jobId, processarAposUtc) =>
                  handleAction(
                    () => scheduleJob(jobId, processarAposUtc),
                    'Nao foi possivel agendar o job.',
                  )
                }
              />
            ))}
          </div>
        )}
      </div>
    </main>
  );
}
