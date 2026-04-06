import { BackgroundJobRetryLogResponse } from '@/types/api';

export function RetryLogCard({ item }: { item: BackgroundJobRetryLogResponse }) {
  return (
    <article className="rounded-[1.5rem] border border-slate-200/80 bg-white p-4 shadow-sm">
      <div className="flex items-start justify-between gap-3">
        <div>
          <p className="text-[11px] font-semibold uppercase tracking-[0.18em] text-slate-400">{item.jobId}</p>
          <h3 className="mt-1 text-base font-semibold text-slate-900">{item.tipo}</h3>
        </div>
        <span className="text-xs text-slate-500">
          {new Date(item.dataCriacao).toLocaleString('pt-BR')}
        </span>
      </div>

      <p className="mt-4 text-sm leading-6 text-slate-600">{item.mensagem}</p>
      <p className="mt-3 text-xs text-slate-400">Execucao: {item.execucaoId}</p>
    </article>
  );
}
