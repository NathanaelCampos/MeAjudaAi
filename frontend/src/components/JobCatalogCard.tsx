import clsx from 'clsx';
import Link from 'next/link';
import { useState } from 'react';
import { BackgroundJobAdminItemResponse } from '@/types/api';

function formatDate(value?: string | null) {
  if (!value) {
    return '-';
  }

  return new Date(value).toLocaleString('pt-BR');
}

interface JobCatalogCardProps {
  item: BackgroundJobAdminItemResponse;
  busyAction?: string | null;
  onExecute: (jobId: string) => void | Promise<void>;
  onEnqueue: (jobId: string) => void | Promise<void>;
  onCancelAll: (jobId: string) => void | Promise<void>;
  onSchedule: (jobId: string, processarAposUtc: string) => void | Promise<void>;
}

export function JobCatalogCard({
  item,
  busyAction,
  onExecute,
  onEnqueue,
  onCancelAll,
  onSchedule,
}: JobCatalogCardProps) {
  const [scheduledAt, setScheduledAt] = useState('');
  const executing = busyAction === `executar:${item.jobId}`;
  const enqueueing = busyAction === `enfileirar:${item.jobId}`;
  const cancelling = busyAction === `cancelar-todos:${item.jobId}`;
  const scheduling = busyAction === `agendar:${item.jobId}`;

  async function handleSchedule() {
    if (!scheduledAt) {
      return;
    }

    await onSchedule(item.jobId, new Date(scheduledAt).toISOString());
    setScheduledAt('');
  }

  return (
    <article className="rounded-[1.5rem] border border-slate-200/80 bg-white p-4 shadow-sm">
      <div className="flex items-start justify-between gap-3">
        <div>
          <p className="text-[11px] font-semibold uppercase tracking-[0.18em] text-slate-400">{item.jobId}</p>
          <h3 className="mt-1 text-lg font-semibold text-slate-900">{item.nome}</h3>
        </div>
        <span
          className={clsx(
            'rounded-full px-3 py-1 text-[11px] font-semibold uppercase tracking-wide',
            item.habilitado ? 'bg-emerald-50 text-emerald-700' : 'bg-slate-100 text-slate-600',
          )}
        >
          {item.habilitado ? 'Habilitado' : 'Desabilitado'}
        </span>
      </div>

      <div className="mt-4 grid grid-cols-2 gap-3 text-sm">
        <div>
          <dt className="text-slate-500">Status</dt>
          <dd className="text-slate-900">{item.ultimoStatus}</dd>
        </div>
        <div>
          <dt className="text-slate-500">Intervalo</dt>
          <dd className="text-slate-900">{item.intervaloSegundos ? `${item.intervaloSegundos}s` : '-'}</dd>
        </div>
        <div>
          <dt className="text-slate-500">Execucoes</dt>
          <dd className="text-slate-900">{item.totalExecucoes}</dd>
        </div>
        <div>
          <dt className="text-slate-500">Sucessos / Falhas</dt>
          <dd className="text-slate-900">
            {item.totalSucessos} / {item.totalFalhas}
          </dd>
        </div>
        <div>
          <dt className="text-slate-500">Iniciada</dt>
          <dd className="text-slate-900">{formatDate(item.ultimaExecucaoIniciadaEm)}</dd>
        </div>
        <div>
          <dt className="text-slate-500">Finalizada</dt>
          <dd className="text-slate-900">{formatDate(item.ultimaExecucaoFinalizadaEm)}</dd>
        </div>
      </div>

      <div className="mt-4 rounded-[1.25rem] bg-slate-50 p-3">
        <p className="text-[11px] font-semibold uppercase tracking-[0.18em] text-slate-500">Ultima mensagem</p>
        <p className="mt-2 text-sm leading-6 text-slate-700">
          {item.ultimaMensagemErro?.trim() || 'Sem erro registrado para este job.'}
        </p>
      </div>

      <div className="mt-4 flex flex-wrap gap-2">
        <Link
          href={`/jobs/${item.jobId}`}
          className="rounded-full border border-slate-200 bg-white px-4 py-2 text-sm font-medium text-slate-700 transition hover:bg-slate-50"
        >
          Ver detalhes
        </Link>
        <button
          type="button"
          onClick={() => onExecute(item.jobId)}
          disabled={executing}
          className="rounded-full bg-slate-900 px-4 py-2 text-sm font-medium text-white transition hover:bg-slate-800 disabled:cursor-not-allowed disabled:opacity-50"
        >
          {executing ? 'Executando...' : 'Executar agora'}
        </button>
        <button
          type="button"
          onClick={() => onEnqueue(item.jobId)}
          disabled={enqueueing}
          className="rounded-full border border-sky-200 bg-sky-50 px-4 py-2 text-sm font-medium text-sky-700 transition hover:bg-sky-100 disabled:cursor-not-allowed disabled:opacity-50"
        >
          {enqueueing ? 'Enfileirando...' : 'Enfileirar'}
        </button>
        <button
          type="button"
          onClick={() => onCancelAll(item.jobId)}
          disabled={cancelling}
          className="rounded-full border border-rose-200 bg-rose-50 px-4 py-2 text-sm font-medium text-rose-700 transition hover:bg-rose-100 disabled:cursor-not-allowed disabled:opacity-50"
        >
          {cancelling ? 'Cancelando...' : 'Cancelar pendentes'}
        </button>
      </div>

      <div className="mt-4 rounded-[1.25rem] border border-slate-200 bg-slate-50 p-3">
        <p className="text-[11px] font-semibold uppercase tracking-[0.18em] text-slate-500">Agendar execucao</p>
        <div className="mt-3 flex flex-col gap-2 sm:flex-row">
          <input
            type="datetime-local"
            value={scheduledAt}
            onChange={(event) => setScheduledAt(event.target.value)}
            className="min-w-0 flex-1 rounded-2xl border border-slate-200 bg-white px-4 py-3 text-sm text-slate-900 outline-none transition focus:border-slate-900"
          />
          <button
            type="button"
            onClick={() => void handleSchedule()}
            disabled={!scheduledAt || scheduling}
            className="rounded-2xl border border-amber-200 bg-amber-50 px-4 py-3 text-sm font-medium text-amber-700 transition hover:bg-amber-100 disabled:cursor-not-allowed disabled:opacity-50"
          >
            {scheduling ? 'Agendando...' : 'Agendar'}
          </button>
        </div>
      </div>
    </article>
  );
}
