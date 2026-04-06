import Link from 'next/link';
import clsx from 'clsx';
import { ProfissionalResumoResponse } from '@/types/api';

function formatScore(value?: number | null) {
  return typeof value === 'number' ? value.toFixed(1) : '-';
}

export function ProfessionalCard({ item }: { item: ProfissionalResumoResponse }) {
  return (
    <article className="overflow-hidden rounded-[2rem] border border-white/75 bg-white/88 p-5 shadow-[0_26px_70px_rgba(15,23,42,0.08)] backdrop-blur">
      <div className="flex items-start justify-between gap-3">
        <div className="min-w-0">
          <div className="mb-4 flex items-center gap-2 text-[11px] font-semibold uppercase tracking-[0.2em] text-slate-400">
            <span className="inline-flex h-2.5 w-2.5 rounded-full bg-amber-500 shadow-[0_0_0_4px_rgba(245,158,11,0.14)]" />
            Profissional em destaque local
          </div>
          <div className="flex flex-wrap items-center gap-2">
            <h3 data-display="true" className="text-[1.65rem] font-semibold leading-none text-slate-900">{item.nomeExibicao}</h3>
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
          <p className="mt-3 line-clamp-3 text-sm leading-6 text-slate-600">{item.descricao}</p>
        </div>
      </div>

      <div className="mt-5 flex flex-wrap gap-2">
        {item.profissoes.map((profissao) => (
          <span key={profissao.id} className="rounded-full bg-slate-100 px-3 py-1.5 text-xs font-semibold text-slate-700">
            {profissao.nome}
          </span>
        ))}
      </div>

      {item.especialidades.length ? (
        <div className="mt-3 flex flex-wrap gap-2">
          {item.especialidades.slice(0, 3).map((especialidade) => (
            <span key={especialidade.id} className="rounded-full bg-rose-50 px-3 py-1.5 text-xs font-semibold text-rose-700">
              {especialidade.nome}
            </span>
          ))}
        </div>
      ) : null}

      <div className="mt-5 grid grid-cols-3 gap-3">
        <div className="rounded-[1.35rem] bg-[linear-gradient(180deg,#fff7ed_0%,#f8fafc_100%)] px-3 py-3">
          <p className="text-[11px] font-semibold uppercase tracking-[0.16em] text-slate-500">Atendimento</p>
          <p className="mt-2 text-lg font-semibold text-slate-900">{formatScore(item.notaMediaAtendimento)}</p>
        </div>
        <div className="rounded-[1.35rem] bg-[linear-gradient(180deg,#fff7ed_0%,#f8fafc_100%)] px-3 py-3">
          <p className="text-[11px] font-semibold uppercase tracking-[0.16em] text-slate-500">Servico</p>
          <p className="mt-2 text-lg font-semibold text-slate-900">{formatScore(item.notaMediaServico)}</p>
        </div>
        <div className="rounded-[1.35rem] bg-[linear-gradient(180deg,#fff7ed_0%,#f8fafc_100%)] px-3 py-3">
          <p className="text-[11px] font-semibold uppercase tracking-[0.16em] text-slate-500">Preco</p>
          <p className="mt-2 text-lg font-semibold text-slate-900">{formatScore(item.notaMediaPreco)}</p>
        </div>
      </div>

      <div className="mt-5 flex items-center justify-between gap-3 border-t border-slate-100 pt-4">
        <p className="text-sm text-slate-500">
          {item.areasAtendimento[0]
            ? `${item.areasAtendimento[0].cidadeNome} • ${item.areasAtendimento[0].uf}`
            : 'Area de atendimento nao informada'}
        </p>
        <Link
          href={`/profissionais/${item.id}`}
          className={clsx(
            'rounded-full bg-slate-900 px-4 py-2.5 text-sm font-semibold text-white transition hover:bg-slate-800',
          )}
        >
          Ver perfil
        </Link>
      </div>
    </article>
  );
}
