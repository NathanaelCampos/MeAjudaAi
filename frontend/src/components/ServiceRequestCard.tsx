import Link from 'next/link';
import type { ReactNode } from 'react';
import { ServicoResponse } from '@/types/api';

function formatCurrency(value?: number | null) {
  if (typeof value !== 'number') {
    return 'A combinar';
  }

  return value.toLocaleString('pt-BR', {
    style: 'currency',
    currency: 'BRL',
  });
}

export function formatServiceStatus(status: number | string) {
  switch (status) {
    case 1:
    case 'Solicitado':
      return 'Solicitado';
    case 2:
    case 'Aceito':
      return 'Aceito';
    case 3:
    case 'EmExecucao':
      return 'Em execução';
    case 4:
    case 'Concluido':
      return 'Concluído';
    case 5:
    case 'Cancelado':
      return 'Cancelado';
    default:
      return String(status);
  }
}

interface ServiceRequestCardProps {
  item: ServicoResponse;
  heading?: string;
  actions?: ReactNode;
}

export function ServiceRequestCard({
  item,
  heading,
  actions,
}: ServiceRequestCardProps) {
  return (
    <article className="rounded-[1.8rem] border border-white/80 bg-white/90 p-5 shadow-[0_20px_50px_rgba(15,23,42,0.08)]">
      <div className="flex items-start justify-between gap-3">
        <div>
          <p className="text-[11px] font-semibold uppercase tracking-[0.18em] text-slate-400">{heading || item.nomeProfissional}</p>
          <h3 className="mt-1 text-lg font-semibold text-slate-900">{item.titulo}</h3>
        </div>
        <span className="rounded-full bg-slate-100 px-3 py-1 text-[11px] font-semibold uppercase tracking-wide text-slate-700">
          {formatServiceStatus(item.status)}
        </span>
      </div>

      <p className="mt-3 text-sm leading-6 text-slate-600">{item.descricao}</p>

      <div className="mt-4 grid grid-cols-2 gap-3 text-sm">
        <div>
          <dt className="text-slate-500">Profissão</dt>
          <dd className="text-slate-900">{item.nomeProfissao || '-'}</dd>
        </div>
        <div>
          <dt className="text-slate-500">Local</dt>
          <dd className="text-slate-900">
            {item.cidadeNome} - {item.uf}
          </dd>
        </div>
        <div>
          <dt className="text-slate-500">Valor</dt>
          <dd className="text-slate-900">{formatCurrency(item.valorCombinado)}</dd>
        </div>
        <div>
          <dt className="text-slate-500">Criado em</dt>
          <dd className="text-slate-900">{new Date(item.dataCriacao).toLocaleDateString('pt-BR')}</dd>
        </div>
      </div>

      <div className="mt-5 flex flex-wrap gap-2">
        <Link
          href={`/servicos/${item.id}`}
          className="rounded-full border border-slate-200 bg-white px-4 py-2 text-sm font-medium text-slate-700 transition hover:bg-slate-50"
        >
          Ver detalhe
        </Link>
        {actions}
      </div>
    </article>
  );
}
