'use client';

import Link from 'next/link';
import { FormEvent, useEffect, useState } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import { ServiceRequestCard } from '@/components/ServiceRequestCard';
import { useMyProfessionalServices } from '@/hooks/useMyProfessionalServices';
import { useProfessionalServiceActions } from '@/hooks/useProfessionalServiceActions';
import { ApiError } from '@/lib/api';
import { useToast } from '@/providers/toast-provider';

const statusOptions = [
  { value: '', label: 'Todos' },
  { value: 'Solicitado', label: 'Solicitado' },
  { value: 'Aceito', label: 'Aceito' },
  { value: 'EmExecucao', label: 'Em execução' },
  { value: 'Concluido', label: 'Concluído' },
  { value: 'Cancelado', label: 'Cancelado' },
];

function Placeholder({ label }: { label: string }) {
  return (
    <div className="rounded-[1.6rem] border border-dashed border-slate-300 bg-white/70 p-5 text-sm text-slate-400">
      {label}
    </div>
  );
}

function canAccept(status: number | string) {
  return status === 1 || status === 'Solicitado';
}

function canStart(status: number | string) {
  return status === 2 || status === 'Aceito';
}

function canConclude(status: number | string) {
  return status === 3 || status === 'EmExecucao';
}

function canCancel(status: number | string) {
  return status === 1 || status === 2 || status === 3 || status === 'Solicitado' || status === 'Aceito' || status === 'EmExecucao';
}

export function ProfessionalServicesPageClient() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const [status, setStatus] = useState(searchParams.get('status') ?? '');
  const activeStatus = searchParams.get('status') ?? '';

  useEffect(() => {
    setStatus(searchParams.get('status') ?? '');
  }, [searchParams]);

  const { showToast } = useToast();
  const { data, isLoading, error } = useMyProfessionalServices(activeStatus);
  const { busyAction, acceptService, startService, concludeService, cancelService } = useProfessionalServiceActions();

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    const params = new URLSearchParams();
    if (status.trim()) {
      params.set('status', status.trim());
    }

    const query = params.toString();
    router.replace(query ? `/servicos/profissional?${query}` : '/servicos/profissional');
  }

  async function runAction(
    action: () => Promise<unknown>,
    title: string,
    fallbackMessage: string,
  ) {
    try {
      await action();
      showToast({
        title,
        message: 'A lista foi atualizada com o estado mais recente.',
        variant: 'success',
      });
    } catch (actionError) {
      const message = actionError instanceof ApiError ? actionError.message : fallbackMessage;
      showToast({
        title: 'Falha na operação',
        message,
        variant: 'error',
      });
    }
  }

  return (
    <main className="min-h-screen bg-[linear-gradient(180deg,#eef1f6_0%,#fff7ed_100%)] px-4 py-6">
      <div className="mx-auto flex max-w-5xl flex-col gap-6">
        <section className="rounded-[2.2rem] border border-white/80 bg-white/85 p-6 shadow-[0_28px_80px_rgba(15,23,42,0.08)]">
          <div className="flex flex-wrap items-center justify-between gap-3">
            <div>
              <p className="text-xs font-semibold uppercase tracking-[0.22em] text-slate-500">Área do profissional</p>
              <h1 className="mt-2 text-3xl font-semibold text-slate-900">Serviços recebidos</h1>
              <p className="mt-3 max-w-2xl text-sm leading-7 text-slate-600">
                Aceite, inicie, conclua ou cancele solicitações recebidas diretamente no app.
              </p>
            </div>

            <Link
              href="/servicos"
              className="rounded-full border border-slate-200 bg-white px-4 py-2 text-sm font-medium text-slate-700 transition hover:bg-slate-50"
            >
              Ver lado do cliente
            </Link>
          </div>

          <form className="mt-5 grid gap-3 sm:grid-cols-[1fr_auto]" onSubmit={handleSubmit}>
            <select
              className="rounded-2xl border border-slate-200 bg-white px-4 py-3 text-sm text-slate-900 outline-none transition focus:border-slate-900"
              value={status}
              onChange={(event) => setStatus(event.target.value)}
            >
              {statusOptions.map((option) => (
                <option key={option.value || 'todos'} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>

            <button
              type="submit"
              className="rounded-2xl bg-slate-900 px-4 py-3 text-sm font-semibold text-white transition hover:bg-slate-800"
            >
              Filtrar
            </button>
          </form>
        </section>

        {isLoading ? (
          <div className="grid gap-4 md:grid-cols-2">
            {Array.from({ length: 4 }).map((_, index) => (
              <Placeholder key={index} label="Carregando solicitações recebidas..." />
            ))}
          </div>
        ) : error ? (
          <Placeholder label="Falha ao carregar os serviços do profissional." />
        ) : !data?.length ? (
          <Placeholder label="Nenhum serviço recebido para este filtro." />
        ) : (
          <div className="grid gap-4 md:grid-cols-2">
            {data.map((item) => (
              <ServiceRequestCard
                key={item.id}
                item={item}
                heading={item.nomeCliente}
                actions={
                  <>
                    {canAccept(item.status) ? (
                      <button
                        type="button"
                        onClick={() =>
                          void runAction(
                            () => acceptService(item.id),
                            'Solicitação aceita',
                            'Não foi possível aceitar o serviço.',
                          )
                        }
                        disabled={busyAction === `aceitar:${item.id}`}
                        className="rounded-full bg-slate-900 px-4 py-2 text-sm font-semibold text-white transition hover:bg-slate-800 disabled:cursor-not-allowed disabled:opacity-50"
                      >
                        {busyAction === `aceitar:${item.id}` ? 'Aceitando...' : 'Aceitar'}
                      </button>
                    ) : null}

                    {canStart(item.status) ? (
                      <button
                        type="button"
                        onClick={() =>
                          void runAction(
                            () => startService(item.id),
                            'Serviço iniciado',
                            'Não foi possível iniciar o serviço.',
                          )
                        }
                        disabled={busyAction === `iniciar:${item.id}`}
                        className="rounded-full border border-sky-200 bg-sky-50 px-4 py-2 text-sm font-medium text-sky-700 transition hover:bg-sky-100 disabled:cursor-not-allowed disabled:opacity-50"
                      >
                        {busyAction === `iniciar:${item.id}` ? 'Iniciando...' : 'Iniciar'}
                      </button>
                    ) : null}

                    {canConclude(item.status) ? (
                      <button
                        type="button"
                        onClick={() =>
                          void runAction(
                            () => concludeService(item.id),
                            'Serviço concluído',
                            'Não foi possível concluir o serviço.',
                          )
                        }
                        disabled={busyAction === `concluir:${item.id}`}
                        className="rounded-full border border-emerald-200 bg-emerald-50 px-4 py-2 text-sm font-medium text-emerald-700 transition hover:bg-emerald-100 disabled:cursor-not-allowed disabled:opacity-50"
                      >
                        {busyAction === `concluir:${item.id}` ? 'Concluindo...' : 'Concluir'}
                      </button>
                    ) : null}

                    {canCancel(item.status) ? (
                      <button
                        type="button"
                        onClick={() =>
                          void runAction(
                            () => cancelService(item.id),
                            'Serviço cancelado',
                            'Não foi possível cancelar o serviço.',
                          )
                        }
                        disabled={busyAction === `cancelar:${item.id}`}
                        className="rounded-full border border-rose-200 bg-rose-50 px-4 py-2 text-sm font-medium text-rose-700 transition hover:bg-rose-100 disabled:cursor-not-allowed disabled:opacity-50"
                      >
                        {busyAction === `cancelar:${item.id}` ? 'Cancelando...' : 'Cancelar'}
                      </button>
                    ) : null}
                  </>
                }
              />
            ))}
          </div>
        )}
      </div>
    </main>
  );
}
