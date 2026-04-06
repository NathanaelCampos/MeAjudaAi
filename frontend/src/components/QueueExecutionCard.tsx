import clsx from 'clsx';
import { BackgroundJobFilaItemResponse } from '@/types/api';

function formatDate(value?: string | null) {
  if (!value) {
    return '-';
  }

  return new Date(value).toLocaleString('pt-BR');
}

interface QueueExecutionCardProps {
  item: BackgroundJobFilaItemResponse;
  isBusy?: boolean;
  onCancel?: (execucaoId: string) => void | Promise<void>;
  onReopen?: (execucaoId: string) => void | Promise<void>;
}

export function QueueExecutionCard({ item, isBusy = false, onCancel, onReopen }: QueueExecutionCardProps) {
  const statusTone =
    item.status === 'Sucesso'
      ? 'bg-emerald-50 text-emerald-700 border-emerald-200'
      : item.status === 'Falha'
        ? 'bg-rose-50 text-rose-700 border-rose-200'
        : item.status === 'Processando'
          ? 'bg-sky-50 text-sky-700 border-sky-200'
          : item.status === 'Cancelado'
            ? 'bg-slate-100 text-slate-600 border-slate-200'
            : 'bg-amber-50 text-amber-700 border-amber-200';

  const canCancel = ['Pendente', 'Processando'].includes(item.status);
  const canReopen = ['Falha', 'Cancelado'].includes(item.status);

  return (
    <article className="rounded-[1.5rem] border border-slate-200/80 bg-white p-4 shadow-sm">
      <div className="flex items-start justify-between gap-3">
        <div>
          <p className="text-[11px] font-semibold uppercase tracking-[0.18em] text-slate-400">{item.jobId}</p>
          <h3 className="mt-1 text-lg font-semibold text-slate-900">{item.nomeJob}</h3>
        </div>
        <span className={clsx('rounded-full border px-3 py-1 text-[11px] font-semibold uppercase tracking-wide', statusTone)}>
          {item.status}
        </span>
      </div>

      <div className="mt-4 grid grid-cols-2 gap-3 text-sm">
        <div>
          <dt className="text-slate-500">Origem</dt>
          <dd className="text-slate-900">{item.origem}</dd>
        </div>
        <div>
          <dt className="text-slate-500">Criado em</dt>
          <dd className="text-slate-900">{formatDate(item.dataCriacao)}</dd>
        </div>
        <div>
          <dt className="text-slate-500">Tentativas</dt>
          <dd className="text-slate-900">{item.tentativasProcessamento}</dd>
        </div>
        <div>
          <dt className="text-slate-500">Processados</dt>
          <dd className="text-slate-900">{item.registrosProcessados}</dd>
        </div>
        <div>
          <dt className="text-slate-500">Inicio</dt>
          <dd className="text-slate-900">{formatDate(item.dataInicioProcessamento)}</dd>
        </div>
        <div>
          <dt className="text-slate-500">Fim</dt>
          <dd className="text-slate-900">{formatDate(item.dataFinalizacao)}</dd>
        </div>
      </div>

      <div className="mt-4 rounded-[1.25rem] bg-slate-50 p-3">
        <p className="text-[11px] font-semibold uppercase tracking-[0.18em] text-slate-500">Mensagem</p>
        <p className="mt-2 text-sm leading-6 text-slate-700">
          {item.mensagemResultado?.trim() || 'Sem mensagem adicional para esta execucao.'}
        </p>
      </div>

      {canCancel || canReopen ? (
        <div className="mt-4 flex flex-wrap gap-2">
          {canCancel ? (
            <button
              type="button"
              onClick={() => onCancel?.(item.execucaoId)}
              disabled={isBusy}
              className="rounded-full border border-rose-200 bg-rose-50 px-4 py-2 text-sm font-medium text-rose-700 transition hover:bg-rose-100 disabled:cursor-not-allowed disabled:opacity-50"
            >
              {isBusy ? 'Processando...' : 'Cancelar'}
            </button>
          ) : null}

          {canReopen ? (
            <button
              type="button"
              onClick={() => onReopen?.(item.execucaoId)}
              disabled={isBusy}
              className="rounded-full border border-sky-200 bg-sky-50 px-4 py-2 text-sm font-medium text-sky-700 transition hover:bg-sky-100 disabled:cursor-not-allowed disabled:opacity-50"
            >
              {isBusy ? 'Processando...' : 'Reabrir'}
            </button>
          ) : null}
        </div>
      ) : null}
    </article>
  );
}
