import Link from 'next/link';
import clsx from 'clsx';
import { ProfissionalResumoResponse } from '@/types/api';

function formatScore(value?: number | null) {
  return typeof value === 'number' ? value.toFixed(1) : '-';
}

export function ProfessionalCard({ item }: { item: ProfissionalResumoResponse }) {
  return (
    <article className="rounded-[1.8rem] border border-white/70 bg-white/85 p-5 shadow-[0_24px_60px_rgba(15,23,42,0.08)] backdrop-blur">
      <div className="flex items-start justify-between gap-3">
        <div>
          <div className="flex flex-wrap items-center gap-2">
            <h3 className="text-xl font-semibold text-slate-900">{item.nomeExibicao}</h3>
            {item.perfilVerificado ? (
              <span className="rounded-full bg-emerald-50 px-3 py-1 text-[11px] font-semibold uppercase tracking-wide text-emerald-700">
                Verificado
              </span>
            ) : null}
            {item.estaImpulsionado ? (
              <span className="rounded-full bg-amber-50 px-3 py-1 text-[11px] font-semibold uppercase tracking-wide text-amber-700">
                Destaque
              </span>
            ) : null}
          </div>
          <p className="mt-2 line-clamp-3 text-sm leading-6 text-slate-600">{item.descricao}</p>
        </div>
      </div>

      <div className="mt-4 flex flex-wrap gap-2">
        {item.profissoes.map((profissao) => (
          <span key={profissao.id} className="rounded-full bg-slate-100 px-3 py-1 text-xs font-medium text-slate-700">
            {profissao.nome}
          </span>
        ))}
      </div>

      {item.especialidades.length ? (
        <div className="mt-3 flex flex-wrap gap-2">
          {item.especialidades.slice(0, 3).map((especialidade) => (
            <span key={especialidade.id} className="rounded-full bg-rose-50 px-3 py-1 text-xs font-medium text-rose-700">
              {especialidade.nome}
            </span>
          ))}
        </div>
      ) : null}

      <div className="mt-5 grid grid-cols-3 gap-3">
        <div className="rounded-[1.25rem] bg-slate-50 px-3 py-3">
          <p className="text-[11px] font-semibold uppercase tracking-[0.16em] text-slate-500">Atendimento</p>
          <p className="mt-2 text-lg font-semibold text-slate-900">{formatScore(item.notaMediaAtendimento)}</p>
        </div>
        <div className="rounded-[1.25rem] bg-slate-50 px-3 py-3">
          <p className="text-[11px] font-semibold uppercase tracking-[0.16em] text-slate-500">Servico</p>
          <p className="mt-2 text-lg font-semibold text-slate-900">{formatScore(item.notaMediaServico)}</p>
        </div>
        <div className="rounded-[1.25rem] bg-slate-50 px-3 py-3">
          <p className="text-[11px] font-semibold uppercase tracking-[0.16em] text-slate-500">Preco</p>
          <p className="mt-2 text-lg font-semibold text-slate-900">{formatScore(item.notaMediaPreco)}</p>
        </div>
      </div>

      <div className="mt-5 flex items-center justify-between gap-3">
        <p className="text-sm text-slate-500">
          {item.areasAtendimento[0]
            ? `${item.areasAtendimento[0].cidadeNome} • ${item.areasAtendimento[0].uf}`
            : 'Area de atendimento nao informada'}
        </p>
        <Link
          href={`/profissionais/${item.id}`}
          className={clsx(
            'rounded-full bg-slate-900 px-4 py-2 text-sm font-semibold text-white transition hover:bg-slate-800',
          )}
        >
          Ver perfil
        </Link>
      </div>
    </article>
  );
}
