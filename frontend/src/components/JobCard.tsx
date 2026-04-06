import clsx from 'clsx';

export interface JobCardProps {
  jobId: string;
  nome: string;
  status: string;
  origem: string;
  processados: number;
  tentativas: number;
  mensagemResultado?: string;
  criadoEm: string;
}

export function JobCard(props: JobCardProps) {
  const statusTone =
    props.status === 'Sucesso'
      ? 'bg-emerald-50 text-emerald-700'
      : props.status === 'Falha'
        ? 'bg-rose-50 text-rose-700'
        : props.status === 'Processando'
          ? 'bg-sky-50 text-sky-700'
          : 'bg-amber-50 text-amber-700';

  return (
    <article
      className={clsx(
        'rounded-[1.5rem] border border-slate-200/80 bg-white p-4 shadow-sm transition-transform duration-200 hover:-translate-y-0.5',
        props.status === 'Falha' && 'ring-2 ring-rose-200',
      )}
    >
      <header className="flex items-center justify-between">
        <div>
          <p className="text-[11px] font-semibold uppercase tracking-[0.18em] text-slate-400">{props.jobId}</p>
          <h3 className="mt-1 text-lg font-semibold text-slate-900">{props.nome}</h3>
        </div>
        <span className={clsx('rounded-full px-3 py-1 text-[11px] font-semibold uppercase tracking-wide', statusTone)}>{props.status}</span>
      </header>

      <dl className="mt-4 grid grid-cols-2 gap-3 text-sm">
        <div>
          <dt className="text-neutral-500">Origem</dt>
          <dd className="text-slate-800">{props.origem}</dd>
        </div>
        <div>
          <dt className="text-neutral-500">Criado em</dt>
          <dd className="text-slate-800">{new Date(props.criadoEm).toLocaleString('pt-BR')}</dd>
        </div>
        <div>
          <dt className="text-neutral-500">Tentativas</dt>
          <dd className="text-slate-800">{props.tentativas}</dd>
        </div>
        <div>
          <dt className="text-neutral-500">Processados</dt>
          <dd className="text-slate-800">{props.processados}</dd>
        </div>
      </dl>

      <p className="mt-4 line-clamp-2 text-sm text-slate-600">
        {props.mensagemResultado?.trim() || 'Sem mensagem adicional para esta execucao.'}
      </p>
    </article>
  );
}
