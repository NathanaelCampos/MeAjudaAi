import clsx from 'clsx';

export interface JobCardProps {
  jobId: string;
  nome: string;
  status: 'Ativo' | 'Inativo';
  intervalo?: number;
  pendentes: number;
  falhas: number;
}

export function JobCard(props: JobCardProps) {
  return (
    <article className={clsx('rounded-2xl border border-neutral-200 p-4 shadow-sm bg-white', props.pendentes > 0 && 'ring-2 ring-amber-300')}>
      <header className="flex items-center justify-between">
        <h3 className="text-lg font-semibold text-slate-900">{props.nome}</h3>
        <span className="text-xs font-semibold uppercase tracking-wide text-emerald-600">{props.status}</span>
      </header>
      <dl className="mt-3 grid grid-cols-2 gap-2 text-sm">
        <div>
          <dt className="text-neutral-500">Job ID</dt>
          <dd className="text-slate-800">{props.jobId}</dd>
        </div>
        <div>
          <dt className="text-neutral-500">Intervalo</dt>
          <dd className="text-slate-800">{props.intervalo ?? 0}s</dd>
        </div>
        <div>
          <dt className="text-neutral-500">Pendentes</dt>
          <dd className="text-slate-800">{props.pendentes}</dd>
        </div>
        <div>
          <dt className="text-neutral-500">Falhas</dt>
          <dd className="text-slate-800">{props.falhas}</dd>
        </div>
      </dl>
    </article>
  );
}
