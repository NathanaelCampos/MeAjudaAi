'use client';

import Link from 'next/link';
import { ApiError } from '@/lib/api';
import { ReviewForm } from '@/components/ReviewForm';
import { useAuth } from '@/providers/auth-provider';
import { useToast } from '@/providers/toast-provider';
import { formatServiceStatus } from '@/components/ServiceRequestCard';
import { useServiceActions } from '@/hooks/useServiceActions';
import { useServiceDetails } from '@/hooks/useServiceDetails';

function formatCurrency(value?: number | null) {
  if (typeof value !== 'number') {
    return 'A combinar';
  }

  return value.toLocaleString('pt-BR', {
    style: 'currency',
    currency: 'BRL',
  });
}

function formatDate(value?: string | null) {
  if (!value) {
    return '-';
  }

  return new Date(value).toLocaleString('pt-BR');
}

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

function canReview(status: number | string) {
  return status === 4 || status === 'Concluido';
}

export function ServiceDetailPageClient({ id }: { id: string }) {
  const { session } = useAuth();
  const { showToast } = useToast();
  const { data, isLoading, error } = useServiceDetails(id);
  const { busyAction, acceptService, startService, concludeService, cancelService } = useServiceActions();

  const isClient = !!session?.usuarioId && data?.clienteId === session.usuarioId;
  const isProfessional = !!session?.usuarioId && data?.profissionalId === session.usuarioId;

  async function runAction(action: () => Promise<unknown>, successTitle: string, fallbackMessage: string) {
    try {
      await action();
      showToast({
        title: successTitle,
        message: 'Os dados do serviço foram atualizados.',
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
    <main className="min-h-screen bg-[linear-gradient(180deg,#fff7ed_0%,#f8fafc_100%)] px-4 py-6">
      <div className="mx-auto flex max-w-5xl flex-col gap-6">
        <section className="rounded-[2.2rem] border border-white/80 bg-white/85 p-6 shadow-[0_28px_80px_rgba(15,23,42,0.08)]">
          <div className="flex flex-wrap items-center justify-between gap-3">
            <div>
              <p className="text-xs font-semibold uppercase tracking-[0.22em] text-slate-500">Detalhe do serviço</p>
              <h1 className="mt-2 text-3xl font-semibold text-slate-900">{data?.titulo ?? 'Acompanhamento completo'}</h1>
              <p className="mt-3 max-w-2xl text-sm leading-7 text-slate-600">
                Timeline do pedido, partes envolvidas, valores e ações disponíveis de acordo com o status atual.
              </p>
            </div>

            <div className="flex flex-wrap gap-2">
              <Link
                href="/servicos"
                className="rounded-full border border-slate-200 bg-white px-4 py-2 text-sm font-medium text-slate-700 transition hover:bg-slate-50"
              >
                Meus serviços
              </Link>
              <Link
                href="/servicos/profissional"
                className="rounded-full border border-slate-200 bg-white px-4 py-2 text-sm font-medium text-slate-700 transition hover:bg-slate-50"
              >
                Área profissional
              </Link>
            </div>
          </div>
        </section>

        {isLoading ? (
          <Placeholder label="Carregando detalhes do serviço..." />
        ) : error || !data ? (
          <Placeholder label="Não foi possível carregar este serviço." />
        ) : (
          <>
            <section className="grid gap-4 lg:grid-cols-[1.15fr_0.85fr]">
              <div className="rounded-[2rem] border border-white/80 bg-white/90 p-5 shadow-[0_20px_50px_rgba(15,23,42,0.08)]">
                <div className="flex flex-wrap items-start justify-between gap-3">
                  <div>
                    <p className="text-[11px] font-semibold uppercase tracking-[0.18em] text-slate-400">
                      {data.nomeCliente} → {data.nomeProfissional}
                    </p>
                    <h2 className="mt-1 text-2xl font-semibold text-slate-900">{data.titulo}</h2>
                  </div>
                  <span className="rounded-full bg-slate-100 px-3 py-1 text-[11px] font-semibold uppercase tracking-wide text-slate-700">
                    {formatServiceStatus(data.status)}
                  </span>
                </div>

                <p className="mt-4 text-sm leading-7 text-slate-600">{data.descricao}</p>

                <div className="mt-5 grid gap-3 sm:grid-cols-2">
                  <div className="rounded-[1.25rem] bg-slate-50 px-4 py-3">
                    <p className="text-[11px] font-semibold uppercase tracking-[0.16em] text-slate-500">Profissão</p>
                    <p className="mt-2 text-lg font-semibold text-slate-900">{data.nomeProfissao || '-'}</p>
                  </div>
                  <div className="rounded-[1.25rem] bg-slate-50 px-4 py-3">
                    <p className="text-[11px] font-semibold uppercase tracking-[0.16em] text-slate-500">Valor</p>
                    <p className="mt-2 text-lg font-semibold text-slate-900">{formatCurrency(data.valorCombinado)}</p>
                  </div>
                  <div className="rounded-[1.25rem] bg-slate-50 px-4 py-3">
                    <p className="text-[11px] font-semibold uppercase tracking-[0.16em] text-slate-500">Cidade</p>
                    <p className="mt-2 text-lg font-semibold text-slate-900">
                      {data.cidadeNome} - {data.uf}
                    </p>
                  </div>
                  <div className="rounded-[1.25rem] bg-slate-50 px-4 py-3">
                    <p className="text-[11px] font-semibold uppercase tracking-[0.16em] text-slate-500">Bairro</p>
                    <p className="mt-2 text-lg font-semibold text-slate-900">{data.bairroNome || '-'}</p>
                  </div>
                </div>

                <div className="mt-5 flex flex-wrap gap-2">
                  {isProfessional && canAccept(data.status) ? (
                    <button
                      type="button"
                      onClick={() => void runAction(() => acceptService(data.id), 'Serviço aceito', 'Não foi possível aceitar o serviço.')}
                      disabled={busyAction === `aceitar:${data.id}`}
                      className="rounded-full bg-slate-900 px-4 py-2 text-sm font-semibold text-white transition hover:bg-slate-800 disabled:cursor-not-allowed disabled:opacity-50"
                    >
                      {busyAction === `aceitar:${data.id}` ? 'Aceitando...' : 'Aceitar'}
                    </button>
                  ) : null}

                  {isProfessional && canStart(data.status) ? (
                    <button
                      type="button"
                      onClick={() => void runAction(() => startService(data.id), 'Serviço iniciado', 'Não foi possível iniciar o serviço.')}
                      disabled={busyAction === `iniciar:${data.id}`}
                      className="rounded-full border border-sky-200 bg-sky-50 px-4 py-2 text-sm font-medium text-sky-700 transition hover:bg-sky-100 disabled:cursor-not-allowed disabled:opacity-50"
                    >
                      {busyAction === `iniciar:${data.id}` ? 'Iniciando...' : 'Iniciar'}
                    </button>
                  ) : null}

                  {isProfessional && canConclude(data.status) ? (
                    <button
                      type="button"
                      onClick={() => void runAction(() => concludeService(data.id), 'Serviço concluído', 'Não foi possível concluir o serviço.')}
                      disabled={busyAction === `concluir:${data.id}`}
                      className="rounded-full border border-emerald-200 bg-emerald-50 px-4 py-2 text-sm font-medium text-emerald-700 transition hover:bg-emerald-100 disabled:cursor-not-allowed disabled:opacity-50"
                    >
                      {busyAction === `concluir:${data.id}` ? 'Concluindo...' : 'Concluir'}
                    </button>
                  ) : null}

                  {(isClient || isProfessional) && canCancel(data.status) ? (
                    <button
                      type="button"
                      onClick={() => void runAction(() => cancelService(data.id), 'Serviço cancelado', 'Não foi possível cancelar o serviço.')}
                      disabled={busyAction === `cancelar:${data.id}`}
                      className="rounded-full border border-rose-200 bg-rose-50 px-4 py-2 text-sm font-medium text-rose-700 transition hover:bg-rose-100 disabled:cursor-not-allowed disabled:opacity-50"
                    >
                      {busyAction === `cancelar:${data.id}` ? 'Cancelando...' : 'Cancelar'}
                    </button>
                  ) : null}
                </div>
              </div>

              <div className="rounded-[2rem] border border-white/80 bg-[#1f2937] p-5 text-white shadow-[0_20px_50px_rgba(15,23,42,0.14)]">
                <p className="text-xs font-semibold uppercase tracking-[0.2em] text-amber-300">Timeline</p>
                <div className="mt-4 space-y-3">
                  <div className="rounded-[1.25rem] bg-white/10 p-4">
                    <p className="text-sm font-semibold">Criado</p>
                    <p className="mt-1 text-sm text-white/75">{formatDate(data.dataCriacao)}</p>
                  </div>
                  <div className="rounded-[1.25rem] bg-white/10 p-4">
                    <p className="text-sm font-semibold">Aceite</p>
                    <p className="mt-1 text-sm text-white/75">{formatDate(data.dataAceite)}</p>
                  </div>
                  <div className="rounded-[1.25rem] bg-white/10 p-4">
                    <p className="text-sm font-semibold">Início</p>
                    <p className="mt-1 text-sm text-white/75">{formatDate(data.dataInicio)}</p>
                  </div>
                  <div className="rounded-[1.25rem] bg-white/10 p-4">
                    <p className="text-sm font-semibold">Conclusão</p>
                    <p className="mt-1 text-sm text-white/75">{formatDate(data.dataConclusao)}</p>
                  </div>
                  <div className="rounded-[1.25rem] bg-white/10 p-4">
                    <p className="text-sm font-semibold">Cancelamento</p>
                    <p className="mt-1 text-sm text-white/75">{formatDate(data.dataCancelamento)}</p>
                  </div>
                </div>
              </div>
            </section>

            <section className="grid gap-4 md:grid-cols-2">
              <div className="rounded-[2rem] border border-white/80 bg-white/90 p-5 shadow-[0_20px_50px_rgba(15,23,42,0.08)]">
                <p className="text-xs font-semibold uppercase tracking-[0.2em] text-slate-500">Cliente</p>
                <h3 className="mt-2 text-2xl font-semibold text-slate-900">{data.nomeCliente}</h3>
                <p className="mt-3 text-sm leading-7 text-slate-600">
                  Responsável pela solicitação e acompanhamento do serviço.
                </p>
              </div>

              <div className="rounded-[2rem] border border-white/80 bg-white/90 p-5 shadow-[0_20px_50px_rgba(15,23,42,0.08)]">
                <p className="text-xs font-semibold uppercase tracking-[0.2em] text-slate-500">Profissional</p>
                <h3 className="mt-2 text-2xl font-semibold text-slate-900">{data.nomeProfissional}</h3>
                <p className="mt-3 text-sm leading-7 text-slate-600">
                  Responsável por aceitar, executar e concluir a demanda.
                </p>
              </div>
            </section>

            {isClient && canReview(data.status) ? (
              <ReviewForm servicoId={data.id} profissionalNome={data.nomeProfissional} />
            ) : null}
          </>
        )}
      </div>
    </main>
  );
}
