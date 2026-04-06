'use client';

import { FormEvent, useEffect, useState } from 'react';
import type { Route } from 'next';
import Link from 'next/link';
import { useRouter, useSearchParams } from 'next/navigation';
import { ProfessionalCard } from '@/components/ProfessionalCard';
import { useCidades } from '@/hooks/useCidades';
import { useProfessionalSearch } from '@/hooks/useProfessionalSearch';
import { useProfissoes } from '@/hooks/useProfissoes';

function Placeholder({ label }: { label: string }) {
  return (
    <div className="rounded-[1.8rem] border border-dashed border-slate-300 bg-white/72 p-5 text-sm text-slate-400 shadow-[0_18px_44px_rgba(15,23,42,0.04)]">
      {label}
    </div>
  );
}

const sortOptions = [
  { value: '1', label: 'Relevância' },
  { value: '2', label: 'Nome A-Z' },
  { value: '3', label: 'Nome Z-A' },
  { value: '4', label: 'Melhor nota de serviço' },
  { value: '5', label: 'Melhor atendimento' },
  { value: '6', label: 'Melhor nota de preço' },
] as const;

const pageSizeOptions = [6, 12, 18, 24] as const;

function buildExploreQuery(params: {
  nome?: string;
  profissaoId?: string;
  cidadeId?: string;
  ordenacao?: string;
  pagina?: number;
  tamanhoPagina?: number;
}): Route {
  const search = new URLSearchParams();

  if (params.nome?.trim()) {
    search.set('nome', params.nome.trim());
  }
  if (params.profissaoId?.trim()) {
    search.set('profissaoId', params.profissaoId.trim());
  }
  if (params.cidadeId?.trim()) {
    search.set('cidadeId', params.cidadeId.trim());
  }
  if (params.ordenacao?.trim() && params.ordenacao.trim() !== '1') {
    search.set('ordenacao', params.ordenacao.trim());
  }

  const pagina = params.pagina ?? 1;
  const tamanhoPagina = params.tamanhoPagina ?? 12;

  if (pagina > 1) {
    search.set('pagina', String(pagina));
  }
  if (tamanhoPagina !== 12) {
    search.set('tamanhoPagina', String(tamanhoPagina));
  }

  const query = search.toString();
  return (query ? `/explorar?${query}` : '/explorar') as Route;
}

export function ExplorePageClient() {
  const router = useRouter();
  const searchParams = useSearchParams();

  const [nome, setNome] = useState(searchParams.get('nome') ?? '');
  const [profissaoId, setProfissaoId] = useState(searchParams.get('profissaoId') ?? '');
  const [cidadeId, setCidadeId] = useState(searchParams.get('cidadeId') ?? '');
  const [ordenacao, setOrdenacao] = useState(searchParams.get('ordenacao') ?? '1');
  const [tamanhoPagina, setTamanhoPagina] = useState(
    Number(searchParams.get('tamanhoPagina') ?? '12') || 12,
  );

  useEffect(() => {
    setNome(searchParams.get('nome') ?? '');
    setProfissaoId(searchParams.get('profissaoId') ?? '');
    setCidadeId(searchParams.get('cidadeId') ?? '');
    setOrdenacao(searchParams.get('ordenacao') ?? '1');
    setTamanhoPagina(Number(searchParams.get('tamanhoPagina') ?? '12') || 12);
  }, [searchParams]);

  const { data: profissoes } = useProfissoes();
  const { data: cidades } = useCidades();
  const paginaAtual = Number(searchParams.get('pagina') ?? '1') || 1;
  const { data, isLoading, error } = useProfessionalSearch({
    nome: searchParams.get('nome') ?? '',
    profissaoId: searchParams.get('profissaoId') ?? '',
    cidadeId: searchParams.get('cidadeId') ?? '',
    ordenacao: searchParams.get('ordenacao') ?? '1',
    pagina: paginaAtual,
    tamanhoPagina,
  });

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    router.replace(
      buildExploreQuery({
        nome,
        profissaoId,
        cidadeId,
        ordenacao,
        pagina: 1,
        tamanhoPagina,
      }),
    );
  }

  function handlePageChange(nextPage: number) {
    router.replace(
      buildExploreQuery({
        nome: searchParams.get('nome') ?? '',
        profissaoId: searchParams.get('profissaoId') ?? '',
        cidadeId: searchParams.get('cidadeId') ?? '',
        ordenacao: searchParams.get('ordenacao') ?? '1',
        pagina: nextPage,
        tamanhoPagina,
      }),
    );
  }

  function handleReset() {
    setNome('');
    setProfissaoId('');
    setCidadeId('');
    setOrdenacao('1');
    setTamanhoPagina(12);
    router.replace('/explorar');
  }

  const totalRegistros = data?.totalRegistros ?? 0;
  const totalPaginas = data?.totalPaginas ?? 0;
  const inicio = totalRegistros ? (paginaAtual - 1) * tamanhoPagina + 1 : 0;
  const fim = totalRegistros ? Math.min(paginaAtual * tamanhoPagina, totalRegistros) : 0;

  return (
    <main className="min-h-screen bg-[linear-gradient(180deg,#fff7ed_0%,#f8fafc_100%)] px-4 py-6">
      <div className="mx-auto flex max-w-6xl flex-col gap-6">
        <section className="overflow-hidden rounded-[2.4rem] border border-white/80 bg-[linear-gradient(135deg,#111827_0%,#1f2937_42%,#ea580c_115%)] p-6 text-white shadow-[0_28px_80px_rgba(15,23,42,0.16)]">
          <div className="flex flex-col gap-6 lg:flex-row lg:items-end lg:justify-between">
            <div className="max-w-2xl">
              <p className="text-xs font-semibold uppercase tracking-[0.26em] text-amber-200">Me Ajuda Ai</p>
              <h1 data-display="true" className="mt-3 text-[2.6rem] font-semibold leading-[0.96] sm:text-[3.5rem]">
                Ache ajuda boa, perto de voce, sem perder tempo.
              </h1>
              <p className="mt-4 max-w-xl text-sm leading-7 text-white/78">
                Busca mobile-first por profissão, cidade e nome, com leitura pensada para o celular e conversão direta para contratação.
              </p>
            </div>

            <div className="grid gap-3 sm:grid-cols-2 lg:max-w-sm">
              <div className="rounded-[1.5rem] border border-white/12 bg-white/10 px-4 py-4 backdrop-blur">
                <p className="text-[11px] font-semibold uppercase tracking-[0.2em] text-amber-100">Resposta rápida</p>
                <p className="mt-2 text-2xl font-semibold text-white">+5 áreas</p>
                <p className="mt-1 text-sm text-white/70">Profissões já populadas para começar a usar agora.</p>
              </div>
              <Link
                href="/login"
                className="inline-flex min-h-[96px] items-end rounded-[1.5rem] border border-white/12 bg-white/10 px-4 py-4 text-sm font-semibold text-white transition hover:bg-white/15"
              >
                Entrar no painel
              </Link>
            </div>
          </div>
        </section>

        <section className="rounded-[2rem] border border-white/70 bg-white/88 p-5 shadow-[0_24px_60px_rgba(15,23,42,0.08)]">
          <div className="mb-4 flex flex-col gap-2 sm:flex-row sm:items-end sm:justify-between">
            <div>
              <p className="text-[11px] font-semibold uppercase tracking-[0.2em] text-slate-400">Busca inteligente</p>
              <h2 data-display="true" className="mt-1 text-3xl font-semibold text-slate-900">Filtre do seu jeito</h2>
            </div>
            <p className="max-w-md text-sm leading-6 text-slate-500">
              Ajuste profissão, cidade, ordenação e paginação sem perder o contexto da busca.
            </p>
          </div>

          <form className="grid gap-3 lg:grid-cols-[1.3fr_1fr_1fr_1fr_auto]" onSubmit={handleSubmit}>
            <input
              className="rounded-[1.35rem] border border-slate-200 bg-white px-4 py-3.5 text-sm text-slate-900 outline-none transition focus:border-slate-900"
              placeholder="Nome ou palavra-chave"
              value={nome}
              onChange={(event) => setNome(event.target.value)}
            />

            <select
              className="rounded-[1.35rem] border border-slate-200 bg-white px-4 py-3.5 text-sm text-slate-900 outline-none transition focus:border-slate-900"
              value={profissaoId}
              onChange={(event) => setProfissaoId(event.target.value)}
            >
              <option value="">Todas as profissoes</option>
              {(profissoes ?? []).map((profissao) => (
                <option key={profissao.id} value={profissao.id}>
                  {profissao.nome}
                </option>
              ))}
            </select>

            <select
              className="rounded-[1.35rem] border border-slate-200 bg-white px-4 py-3.5 text-sm text-slate-900 outline-none transition focus:border-slate-900"
              value={cidadeId}
              onChange={(event) => setCidadeId(event.target.value)}
            >
              <option value="">Todas as cidades</option>
              {(cidades ?? []).map((cidade) => (
                <option key={cidade.id} value={cidade.id}>
                  {cidade.nome} - {cidade.uf}
                </option>
              ))}
            </select>

            <select
              className="rounded-[1.35rem] border border-slate-200 bg-white px-4 py-3.5 text-sm text-slate-900 outline-none transition focus:border-slate-900"
              value={ordenacao}
              onChange={(event) => setOrdenacao(event.target.value)}
            >
              {sortOptions.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>

            <button
              type="submit"
              className="rounded-[1.35rem] bg-slate-900 px-5 py-3.5 text-sm font-semibold text-white transition hover:bg-slate-800"
            >
              Buscar
            </button>
          </form>

          <div className="mt-4 flex flex-wrap items-center gap-3 text-sm text-slate-500">
            <span className="rounded-full bg-amber-50 px-3 py-1.5 font-semibold text-amber-700">
              {totalRegistros} profissionais encontrados
            </span>
            <span>
              Mostrando {inicio}-{fim}
            </span>
            <span>{totalPaginas} páginas</span>
            <label className="inline-flex items-center gap-2 rounded-full border border-slate-200 bg-white px-3 py-1.5">
              <span>Por página</span>
              <select
                className="rounded-full bg-white px-1 text-sm text-slate-700 outline-none"
                value={tamanhoPagina}
                onChange={(event) => {
                  const nextSize = Number(event.target.value);
                  setTamanhoPagina(nextSize);
                  router.replace(
                    buildExploreQuery({
                      nome: searchParams.get('nome') ?? '',
                      profissaoId: searchParams.get('profissaoId') ?? '',
                      cidadeId: searchParams.get('cidadeId') ?? '',
                      ordenacao: searchParams.get('ordenacao') ?? '1',
                      pagina: 1,
                      tamanhoPagina: nextSize,
                    }),
                  );
                }}
              >
                {pageSizeOptions.map((option) => (
                  <option key={option} value={option}>
                    {option}
                  </option>
                ))}
              </select>
            </label>
            <button
              type="button"
              onClick={handleReset}
              className="rounded-full border border-slate-200 bg-white px-3 py-1.5 text-sm font-medium text-slate-700 transition hover:bg-slate-50"
            >
              Limpar filtros
            </button>
          </div>
        </section>

        {isLoading ? (
          <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-3">
            {Array.from({ length: 6 }).map((_, index) => (
              <Placeholder key={index} label="Carregando profissionais..." />
            ))}
          </div>
        ) : error ? (
          <Placeholder label="Falha ao carregar os profissionais." />
        ) : !data?.itens.length ? (
          <Placeholder label="Nenhum profissional encontrado para os filtros atuais." />
        ) : (
          <>
            <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-3">
              {data.itens.map((item) => (
                <ProfessionalCard key={item.id} item={item} />
              ))}
            </div>

            {data.totalPaginas > 1 ? (
              <section className="rounded-[2rem] border border-white/70 bg-white/88 p-4 shadow-[0_24px_60px_rgba(15,23,42,0.08)]">
                <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
                  <p className="text-sm text-slate-500">
                    Página {paginaAtual} de {data.totalPaginas}
                  </p>

                  <div className="flex flex-wrap gap-2">
                    <button
                      type="button"
                      disabled={paginaAtual <= 1}
                      onClick={() => handlePageChange(paginaAtual - 1)}
                      className="rounded-full border border-slate-200 bg-white px-4 py-2 text-sm font-medium text-slate-700 transition hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-50"
                    >
                      Anterior
                    </button>

                    {Array.from({ length: data.totalPaginas })
                      .map((_, index) => index + 1)
                      .filter((page) =>
                        page === 1 ||
                        page === data.totalPaginas ||
                        Math.abs(page - paginaAtual) <= 1,
                      )
                      .map((page, index, pages) => {
                        const previous = pages[index - 1];
                        const showGap = previous && page - previous > 1;

                        return (
                          <div key={page} className="flex items-center gap-2">
                            {showGap ? <span className="px-1 text-slate-400">...</span> : null}
                            <button
                              type="button"
                              onClick={() => handlePageChange(page)}
                              className={`rounded-full px-4 py-2 text-sm font-medium transition ${
                                page === paginaAtual
                                  ? 'bg-slate-900 text-white'
                                  : 'border border-slate-200 bg-white text-slate-700 hover:bg-slate-50'
                              }`}
                            >
                              {page}
                            </button>
                          </div>
                        );
                      })}

                    <button
                      type="button"
                      disabled={paginaAtual >= data.totalPaginas}
                      onClick={() => handlePageChange(paginaAtual + 1)}
                      className="rounded-full border border-slate-200 bg-white px-4 py-2 text-sm font-medium text-slate-700 transition hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-50"
                    >
                      Próxima
                    </button>
                  </div>
                </div>
              </section>
            ) : null}
          </>
        )}
      </div>
    </main>
  );
}
