'use client';

import Link from 'next/link';
import { ReviewList } from '@/components/ReviewList';
import { useProfessionalReviews } from '@/hooks/useAvaliacoes';
import { useProfessionalDetails } from '@/hooks/useProfessionalDetails';

function scoreLabel(value?: number | null) {
  return typeof value === 'number' ? value.toFixed(1) : '-';
}

function Placeholder({ label }: { label: string }) {
  return (
    <div className="rounded-[1.6rem] border border-dashed border-slate-300 bg-white/70 p-5 text-sm text-slate-400">
      {label}
    </div>
  );
}

export function ProfessionalDetailPageClient({ id }: { id: string }) {
  const { data, isLoading, error } = useProfessionalDetails(id);
  const { data: reviews } = useProfessionalReviews(id);

  return (
    <main className="min-h-screen bg-[linear-gradient(180deg,#fff7ed_0%,#f8fafc_100%)] px-4 py-6">
      <div className="mx-auto flex max-w-6xl flex-col gap-6">
        <section className="rounded-[2.2rem] border border-white/80 bg-white/85 p-6 shadow-[0_28px_80px_rgba(15,23,42,0.08)]">
          <div className="flex flex-wrap items-center justify-between gap-3">
            <Link
              href="/explorar"
              className="rounded-full border border-slate-200 bg-white px-4 py-2 text-sm font-medium text-slate-700 transition hover:bg-slate-50"
            >
              Voltar para explorar
            </Link>

            <div className="flex gap-2">
              <Link
                href={`/servicos/novo?profissionalId=${encodeURIComponent(id)}`}
                className="rounded-full bg-slate-900 px-4 py-2 text-sm font-semibold text-white transition hover:bg-slate-800"
              >
                Solicitar serviço
              </Link>
              <Link
                href="/servicos"
                className="rounded-full border border-slate-200 bg-white px-4 py-2 text-sm font-medium text-slate-700 transition hover:bg-slate-50"
              >
                Meus serviços
              </Link>
              {data?.perfilVerificado ? (
                <span className="rounded-full bg-emerald-50 px-3 py-1 text-[11px] font-semibold uppercase tracking-wide text-emerald-700">
                  Verificado
                </span>
              ) : null}
              {data?.estaImpulsionado ? (
                <span className="rounded-full bg-amber-50 px-3 py-1 text-[11px] font-semibold uppercase tracking-wide text-amber-700">
                  Em destaque
                </span>
              ) : null}
            </div>
          </div>

          {isLoading ? (
            <div className="mt-5">
              <Placeholder label="Carregando perfil profissional..." />
            </div>
          ) : error || !data ? (
            <div className="mt-5">
              <Placeholder label="Nao foi possivel carregar este profissional." />
            </div>
          ) : (
            <>
              <div className="mt-5 grid gap-6 lg:grid-cols-[1.2fr_0.8fr]">
                <div>
                  <h1 className="text-4xl font-semibold leading-tight text-slate-900">{data.nomeExibicao}</h1>
                  <p className="mt-4 text-base leading-8 text-slate-600">{data.descricao}</p>

                  <div className="mt-5 flex flex-wrap gap-2">
                    {data.profissoes.map((profissao) => (
                      <span key={profissao.id} className="rounded-full bg-slate-100 px-3 py-1 text-xs font-medium text-slate-700">
                        {profissao.nome}
                      </span>
                    ))}
                    {data.especialidades.map((especialidade) => (
                      <span key={especialidade.id} className="rounded-full bg-rose-50 px-3 py-1 text-xs font-medium text-rose-700">
                        {especialidade.nome}
                      </span>
                    ))}
                  </div>
                </div>

                <div className="grid gap-3 sm:grid-cols-3 lg:grid-cols-1">
                  <div className="rounded-[1.5rem] bg-slate-50 px-4 py-4">
                    <p className="text-[11px] font-semibold uppercase tracking-[0.18em] text-slate-500">Atendimento</p>
                    <p className="mt-2 text-2xl font-semibold text-slate-900">{scoreLabel(data.notaMediaAtendimento)}</p>
                  </div>
                  <div className="rounded-[1.5rem] bg-slate-50 px-4 py-4">
                    <p className="text-[11px] font-semibold uppercase tracking-[0.18em] text-slate-500">Servico</p>
                    <p className="mt-2 text-2xl font-semibold text-slate-900">{scoreLabel(data.notaMediaServico)}</p>
                  </div>
                  <div className="rounded-[1.5rem] bg-slate-50 px-4 py-4">
                    <p className="text-[11px] font-semibold uppercase tracking-[0.18em] text-slate-500">Preco</p>
                    <p className="mt-2 text-2xl font-semibold text-slate-900">{scoreLabel(data.notaMediaPreco)}</p>
                  </div>
                </div>
              </div>

              <section className="mt-6 grid gap-4 lg:grid-cols-2">
                <div className="rounded-[1.7rem] border border-slate-200 bg-slate-50/70 p-5">
                  <p className="text-xs font-semibold uppercase tracking-[0.2em] text-slate-500">Atendimento</p>
                  <div className="mt-4 space-y-3 text-sm text-slate-600">
                    {data.areasAtendimento.length ? (
                      data.areasAtendimento.map((area) => (
                        <div key={`${area.cidadeId}-${area.bairroId ?? 'cidade'}`} className="rounded-2xl bg-white px-4 py-3">
                          <p className="font-medium text-slate-900">
                            {area.cidadeNome} - {area.uf}
                          </p>
                          <p className="mt-1">
                            {area.cidadeInteira ? 'Atende a cidade inteira' : area.bairroNome || 'Bairro nao informado'}
                          </p>
                        </div>
                      ))
                    ) : (
                      <p>Area de atendimento nao informada.</p>
                    )}
                  </div>
                </div>

                <div className="rounded-[1.7rem] border border-slate-200 bg-slate-50/70 p-5">
                  <p className="text-xs font-semibold uppercase tracking-[0.2em] text-slate-500">Contato</p>
                  <div className="mt-4 space-y-3 text-sm text-slate-600">
                    {data.whatsApp ? <p>WhatsApp: {data.whatsApp}</p> : null}
                    {data.instagram ? <p>Instagram: {data.instagram}</p> : null}
                    {data.facebook ? <p>Facebook: {data.facebook}</p> : null}
                    {data.outraFormaContato ? <p>Outro contato: {data.outraFormaContato}</p> : null}
                    <p>{data.aceitaContatoPeloApp ? 'Aceita contato pelo app.' : 'Contato pelo app indisponivel.'}</p>
                  </div>
                </div>
              </section>

              <section className="rounded-[1.8rem] border border-slate-200 bg-white/90 p-5">
                <p className="text-xs font-semibold uppercase tracking-[0.2em] text-slate-500">Portfolio</p>
                {data.portfolio.length ? (
                  <div className="mt-4 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
                    {data.portfolio.map((foto) => (
                      <article key={foto.id} className="overflow-hidden rounded-[1.4rem] border border-slate-200 bg-slate-50">
                        <img
                          src={foto.urlArquivo}
                          alt={foto.legenda || data.nomeExibicao}
                          className="h-52 w-full object-cover"
                        />
                        <div className="p-4">
                          <p className="text-sm font-medium text-slate-800">{foto.legenda || 'Sem legenda'}</p>
                        </div>
                      </article>
                    ))}
                  </div>
                ) : (
                  <p className="mt-4 text-sm text-slate-500">Portfolio ainda nao disponivel.</p>
                )}
              </section>

              <section className="rounded-[1.8rem] border border-slate-200 bg-white/90 p-5">
                <p className="text-xs font-semibold uppercase tracking-[0.2em] text-slate-500">Formas de recebimento</p>
                {data.formasRecebimento.length ? (
                  <div className="mt-4 flex flex-wrap gap-2">
                    {data.formasRecebimento.map((forma) => (
                      <span key={forma.id} className="rounded-full bg-emerald-50 px-3 py-2 text-sm font-medium text-emerald-700">
                        {forma.descricao}
                      </span>
                    ))}
                  </div>
                ) : (
                  <p className="mt-4 text-sm text-slate-500">Nenhuma forma de recebimento informada.</p>
                )}
              </section>

              <ReviewList reviews={reviews} />
            </>
          )}
        </section>
      </div>
    </main>
  );
}
