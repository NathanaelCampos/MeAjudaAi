import { BackgroundJobFilaAlertaResponse } from '../types/api';

export function AlertsCarousel({ alerts }: { alerts: BackgroundJobFilaAlertaResponse[] }) {
  if (!alerts.length) {
    return <p className="text-sm text-neutral-500">Nenhum alerta ativo no momento.</p>;
  }

  return (
    <div className="space-y-3">
      {alerts.map((alert) => (
        <article key={alert.jobId} className="rounded-2xl border border-neutral-200 bg-white p-4 shadow-sm">
          <header className="flex items-center justify-between">
            <h4 className="text-base font-semibold text-slate-900">{alert.jobId}</h4>
            <span className="text-xs font-semibold uppercase" style={{ color: alert.cor }}>
              {alert.nivelAlerta}
            </span>
          </header>
          <p className="mt-2 text-sm text-slate-700">{alert.mensagem}</p>
          <dl className="mt-3 grid grid-cols-2 gap-2 text-xs text-neutral-500">
            <div>
              <dt>Fila média</dt>
              <dd className="text-slate-900">{alert.tempoMedioFilaSegundos.toFixed(0)}s</dd>
            </div>
            <div>
              <dt>Processamento médio</dt>
              <dd className="text-slate-900">{alert.tempoMedioProcessamentoSegundos.toFixed(0)}s</dd>
            </div>
            <div>
              <dt>Pendentes</dt>
              <dd className="text-slate-900">{alert.totalPendentes}</dd>
            </div>
            <div>
              <dt>Falhas</dt>
              <dd className="text-slate-900">{alert.totalFalhas}</dd>
            </div>
          </dl>
        </article>
      ))}
    </div>
  );
}
