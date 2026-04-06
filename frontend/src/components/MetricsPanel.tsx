import type { ReactNode } from 'react';
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

const highlight = 'text-lg font-semibold text-slate-900';

export function MetricsPanel(props: MetricsPanelProps) {
  return (
    <section className="space-y-5 rounded-[1.6rem] bg-transparent">
      <header className="flex items-center justify-between">
        <div>
          <p className="text-xs font-semibold uppercase tracking-[0.2em] text-slate-500">Métricas da fila</p>
          <h2 className="mt-1 text-xl font-semibold text-slate-900">Saude operacional</h2>
        </div>
      </header>
      <div className="grid grid-cols-2 gap-3 xl:grid-cols-5">
        <Metric label="Pendentes" value={props.pendentes} accent="amber" />
        <Metric label="Processando" value={props.processando} accent="sky" />
        <Metric label="Sucesso" value={props.sucesso} accent="emerald" />
        <Metric label="Falhas" value={props.falhas} accent="rose" />
        <Metric label="Cancelados" value={props.cancelados} accent="slate" />
      </div>
      <div className="grid grid-cols-2 gap-3 mt-3">
        <Metric label="Tempo medio fila" value={`${props.tempoFila.toFixed(0)}s`} small accent="amber" />
        <Metric label="Tempo medio process." value={`${props.tempoProcessamento.toFixed(0)}s`} small accent="sky" />
      </div>
    </section>
  );
}

function Metric({
  label,
  value,
  small,
  accent = 'slate',
}: {
  label: string;
  value: ReactNode;
  small?: boolean;
  accent?: 'amber' | 'sky' | 'emerald' | 'rose' | 'slate';
}) {
  const accentClass = {
    amber: 'border-amber-100 bg-amber-50/70 text-amber-700',
    sky: 'border-sky-100 bg-sky-50/70 text-sky-700',
    emerald: 'border-emerald-100 bg-emerald-50/70 text-emerald-700',
    rose: 'border-rose-100 bg-rose-50/70 text-rose-700',
    slate: 'border-slate-200 bg-white text-slate-700',
  }[accent];

  return (
    <div className={clsx('rounded-[1.3rem] border px-4 py-3 shadow-sm', accentClass)}>
      <p className="text-[11px] font-semibold uppercase tracking-[0.18em]">{label}</p>
      <p className={clsx(highlight, small && 'text-sm')}>{value}</p>
    </div>
  );
}
