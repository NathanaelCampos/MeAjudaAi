'use client';

import Link from 'next/link';
import { FormEvent, useMemo, useState } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import { ApiError } from '@/lib/api';
import { useCreateService } from '@/hooks/useCreateService';
import { useProfessionalDetails } from '@/hooks/useProfessionalDetails';
import { useToast } from '@/providers/toast-provider';

export function NewServicePageClient() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const profissionalId = searchParams.get('profissionalId') ?? '';
  const { data: profissional, isLoading, error } = useProfessionalDetails(profissionalId);
  const { createService, isPending } = useCreateService();
  const { showToast } = useToast();

  const defaultArea = profissional?.areasAtendimento[0];
  const defaultProfissao = profissional?.profissoes[0];
  const defaultEspecialidade = profissional?.especialidades[0];

  const [titulo, setTitulo] = useState('');
  const [descricao, setDescricao] = useState('');
  const [valorCombinado, setValorCombinado] = useState('');

  const canSubmit = useMemo(
    () => !!profissional && titulo.trim().length >= 3 && descricao.trim().length >= 10,
    [descricao, profissional, titulo],
  );

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (!profissional || !defaultArea) {
      return;
    }

    try {
      const response = await createService({
        profissionalId: profissional.id,
        profissaoId: defaultProfissao?.id ?? null,
        especialidadeId: defaultEspecialidade?.id ?? null,
        cidadeId: defaultArea.cidadeId,
        bairroId: defaultArea.bairroId ?? null,
        titulo: titulo.trim(),
        descricao: descricao.trim(),
        valorCombinado: valorCombinado ? Number(valorCombinado) : null,
      });

      showToast({
        title: 'Solicitação enviada',
        message: `O serviço "${response.titulo}" foi criado com sucesso.`,
        variant: 'success',
      });

      router.replace('/servicos');
    } catch (submitError) {
      const message = submitError instanceof ApiError ? submitError.message : 'Nao foi possivel criar o servico.';
      showToast({
        title: 'Falha ao solicitar serviço',
        message,
        variant: 'error',
      });
    }
  }

  return (
    <main className="min-h-screen bg-[linear-gradient(180deg,#fff7ed_0%,#f8fafc_100%)] px-4 py-6">
      <div className="mx-auto flex max-w-4xl flex-col gap-6">
        <section className="rounded-[2.2rem] border border-white/80 bg-white/85 p-6 shadow-[0_28px_80px_rgba(15,23,42,0.08)]">
          <div className="flex flex-wrap items-center justify-between gap-3">
            <div>
              <p className="text-xs font-semibold uppercase tracking-[0.22em] text-slate-500">Nova solicitação</p>
              <h1 className="mt-2 text-3xl font-semibold text-slate-900">Contratar profissional</h1>
              <p className="mt-3 max-w-2xl text-sm leading-7 text-slate-600">
                Descreva o que você precisa e envie a solicitação direto pelo app.
              </p>
            </div>

            <Link
              href={profissionalId ? `/profissionais/${profissionalId}` : '/explorar'}
              className="rounded-full border border-slate-200 bg-white px-4 py-2 text-sm font-medium text-slate-700 transition hover:bg-slate-50"
            >
              Voltar
            </Link>
          </div>
        </section>

        {isLoading ? (
          <section className="rounded-[2rem] border border-dashed border-slate-300 bg-white/70 p-5 text-sm text-slate-400">
            Carregando profissional...
          </section>
        ) : error || !profissional || !defaultArea ? (
          <section className="rounded-[2rem] border border-dashed border-slate-300 bg-white/70 p-5 text-sm text-slate-400">
            Nao foi possivel preparar esta solicitacao.
          </section>
        ) : (
          <section className="rounded-[2rem] border border-white/80 bg-white/90 p-6 shadow-[0_24px_60px_rgba(15,23,42,0.08)]">
            <div className="rounded-[1.6rem] bg-slate-50 px-4 py-4">
              <p className="text-[11px] font-semibold uppercase tracking-[0.18em] text-slate-500">Profissional selecionado</p>
              <p className="mt-2 text-xl font-semibold text-slate-900">{profissional.nomeExibicao}</p>
              <p className="mt-1 text-sm text-slate-600">
                {defaultProfissao?.nome ?? 'Profissão não informada'} • {defaultArea.cidadeNome} - {defaultArea.uf}
              </p>
            </div>

            <form className="mt-5 space-y-4" onSubmit={handleSubmit}>
              <label className="block">
                <span className="mb-2 block text-sm font-medium text-slate-700">Título</span>
                <input
                  className="w-full rounded-2xl border border-slate-200 bg-white px-4 py-3 text-slate-900 outline-none transition focus:border-slate-900"
                  value={titulo}
                  onChange={(event) => setTitulo(event.target.value)}
                  placeholder="Ex: Reforma rápida no banheiro"
                />
              </label>

              <label className="block">
                <span className="mb-2 block text-sm font-medium text-slate-700">Descrição</span>
                <textarea
                  className="min-h-[140px] w-full rounded-2xl border border-slate-200 bg-white px-4 py-3 text-slate-900 outline-none transition focus:border-slate-900"
                  value={descricao}
                  onChange={(event) => setDescricao(event.target.value)}
                  placeholder="Explique o que precisa, prazos e detalhes importantes."
                />
              </label>

              <label className="block">
                <span className="mb-2 block text-sm font-medium text-slate-700">Valor combinado (opcional)</span>
                <input
                  className="w-full rounded-2xl border border-slate-200 bg-white px-4 py-3 text-slate-900 outline-none transition focus:border-slate-900"
                  value={valorCombinado}
                  onChange={(event) => setValorCombinado(event.target.value)}
                  inputMode="decimal"
                  placeholder="Ex: 350"
                />
              </label>

              <button
                type="submit"
                disabled={!canSubmit || isPending}
                className="w-full rounded-2xl bg-slate-900 px-4 py-3 text-sm font-semibold text-white transition hover:bg-slate-800 disabled:cursor-not-allowed disabled:opacity-50"
              >
                {isPending ? 'Enviando...' : 'Enviar solicitação'}
              </button>
            </form>
          </section>
        )}
      </div>
    </main>
  );
}
