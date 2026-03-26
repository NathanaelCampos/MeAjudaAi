import clsx from 'clsx';

export interface MetricsPanelProps {
  pendentes: number;
  processando: number;
  sucesso: number;
  falhas: number;
  cancelados: number;
  tempoFila: number;
  tempoProcessamento: number;
}

const highlight = 'text-sm font-semibold text-slate-900';

export function MetricsPanel(props: MetricsPanelProps) {
  return (
    <section className="space-y-4 bg-slate-50 p-4 rounded-2xl border border-slate-100">
      <header className="flex items-center justify-between">
        <p className="text-xs font-medium uppercase text-neutral-600">Métricas da fila</p>
      </header>
      <div className="grid grid-cols-2 gap-3">
        <Metric label="Pendentes" value={props.pendentes} />
        <Metric label="Processando" value={props.processando} />
        <Metric label="Sucesso" value={props.sucesso} />
        <Metric label="Falhas" value={props.falhas} warning />
        <Metric label="Cancelados" value={props.cancelados} />
      </div>
      <div className="grid grid-cols-2 gap-3 mt-3">
        <Metric label="Tempo médio fila" value={`${props.tempoFila.toFixed(0)}s`} small />
        <Metric label="Tempo médio process." value={`${props.tempoProcessamento.toFixed(0)}s`} small />
      </div>
    </section>
  );
}

function Metric({ label, value, warning, small }: { label: string; value: React.ReactNode; warning?: boolean; small?: boolean }) {
  return (
    <div className="rounded-xl bg-white px-3 py-2 shadow-sm border border-slate-100">
      <p className={warning ? 'text-xs font-semibold uppercase text-orange-600' : 'text-xs uppercase text-neutral-500'}>{label}</p>
      <p className={clsx(highlight, small && 'text-sm')}>{value}</p>
    </div>
  );
}
