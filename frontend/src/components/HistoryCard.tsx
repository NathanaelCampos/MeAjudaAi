import { BackgroundJobFilaAlertasHistoricoResponse } from '@/types/api';

export function HistoryCard({ item }: { item: BackgroundJobFilaAlertasHistoricoResponse }) {
  return (
    <article className="rounded-[1.5rem] border border-slate-200/80 bg-white p-4 shadow-sm">
      <div className="flex items-start justify-between gap-3">
        <div>
          <p className="text-[11px] font-semibold uppercase tracking-[0.18em] text-slate-400">{item.jobId}</p>
          <h3 className="mt-1 text-lg font-semibold text-slate-900">
            {new Date(item.data).toLocaleDateString('pt-BR', {
              day: '2-digit',
              month: 'short',
              year: 'numeric',
            })}
          </h3>
        </div>
        <span className="rounded-full bg-amber-50 px-3 py-1 text-[11px] font-semibold uppercase tracking-wide text-amber-700">
          {item.totalAlertas} alertas
        </span>
      </div>

      <dl className="mt-4 grid grid-cols-2 gap-3 text-sm">
        <div>
          <dt className="text-slate-500">Pendentes</dt>
          <dd className="text-slate-900">{item.totalPendentes}</dd>
        </div>
        <div>
          <dt className="text-slate-500">Falhas</dt>
          <dd className="text-slate-900">{item.totalFalhas}</dd>
        </div>
        <div>
          <dt className="text-slate-500">Fila media</dt>
          <dd className="text-slate-900">{item.tempoMedioFilaSegundos.toFixed(0)}s</dd>
        </div>
        <div>
          <dt className="text-slate-500">Process. medio</dt>
          <dd className="text-slate-900">{item.tempoMedioProcessamentoSegundos.toFixed(0)}s</dd>
        </div>
      </dl>
    </article>
  );
}
