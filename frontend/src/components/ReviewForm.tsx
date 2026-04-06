'use client';

import { FormEvent, useState } from 'react';
import { ApiError } from '@/lib/api';
import { useCreateReview } from '@/hooks/useAvaliacoes';
import { useToast } from '@/providers/toast-provider';
import { NotaAvaliacao } from '@/types/api';

const atendimentoOptions: Array<{ value: NotaAvaliacao; label: string }> = [
  { value: 1, label: 'Muito ruim' },
  { value: 2, label: 'Ruim' },
  { value: 3, label: 'Regular' },
  { value: 4, label: 'Bom' },
  { value: 5, label: 'Excelente' },
];

const precoOptions: Array<{ value: NotaAvaliacao; label: string }> = [
  { value: 1, label: 'Muito caro' },
  { value: 2, label: 'Caro' },
  { value: 3, label: 'Justo' },
  { value: 4, label: 'Bom custo-benefício' },
  { value: 5, label: 'Excelente custo-benefício' },
];

export function ReviewForm({
  servicoId,
  profissionalNome,
}: {
  servicoId: string;
  profissionalNome: string;
}) {
  const { showToast } = useToast();
  const { createReview, isPending } = useCreateReview();
  const [notaAtendimento, setNotaAtendimento] = useState<NotaAvaliacao>(5);
  const [notaServico, setNotaServico] = useState<NotaAvaliacao>(5);
  const [notaPreco, setNotaPreco] = useState<NotaAvaliacao>(4);
  const [comentario, setComentario] = useState('');

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    try {
      await createReview({
        servicoId,
        notaAtendimento,
        notaServico,
        notaPreco,
        comentario: comentario.trim(),
      });

      setComentario('');
      showToast({
        title: 'Avaliação enviada',
        message: `Sua avaliação sobre ${profissionalNome} foi registrada e seguirá para moderação do comentário.`,
        variant: 'success',
      });
    } catch (error) {
      const message =
        error instanceof ApiError ? error.message : 'Não foi possível enviar sua avaliação agora.';

      showToast({
        title: 'Falha ao enviar avaliação',
        message,
        variant: 'error',
      });
    }
  }

  return (
    <section className="rounded-[2rem] border border-white/80 bg-white/90 p-5 shadow-[0_20px_50px_rgba(15,23,42,0.08)]">
      <div>
        <p className="text-xs font-semibold uppercase tracking-[0.2em] text-slate-500">Avaliação</p>
        <h3 className="mt-2 text-2xl font-semibold text-slate-900">Conte como foi a experiência</h3>
        <p className="mt-2 text-sm leading-7 text-slate-600">
          Sua nota ajuda outros clientes. O comentário passa por moderação antes de aparecer publicamente.
        </p>
      </div>

      <form className="mt-5 space-y-4" onSubmit={handleSubmit}>
        <div className="grid gap-4 md:grid-cols-3">
          <label className="space-y-2 text-sm text-slate-700">
            <span className="font-medium">Atendimento</span>
            <select
              className="w-full rounded-2xl border border-slate-200 bg-white px-4 py-3 text-sm text-slate-900 outline-none transition focus:border-slate-900"
              value={notaAtendimento}
              onChange={(event) => setNotaAtendimento(Number(event.target.value) as NotaAvaliacao)}
            >
              {atendimentoOptions.map((option) => (
                <option key={`atendimento-${option.value}`} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>
          </label>

          <label className="space-y-2 text-sm text-slate-700">
            <span className="font-medium">Serviço executado</span>
            <select
              className="w-full rounded-2xl border border-slate-200 bg-white px-4 py-3 text-sm text-slate-900 outline-none transition focus:border-slate-900"
              value={notaServico}
              onChange={(event) => setNotaServico(Number(event.target.value) as NotaAvaliacao)}
            >
              {atendimentoOptions.map((option) => (
                <option key={`servico-${option.value}`} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>
          </label>

          <label className="space-y-2 text-sm text-slate-700">
            <span className="font-medium">Preço</span>
            <select
              className="w-full rounded-2xl border border-slate-200 bg-white px-4 py-3 text-sm text-slate-900 outline-none transition focus:border-slate-900"
              value={notaPreco}
              onChange={(event) => setNotaPreco(Number(event.target.value) as NotaAvaliacao)}
            >
              {precoOptions.map((option) => (
                <option key={`preco-${option.value}`} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>
          </label>
        </div>

        <textarea
          className="min-h-[140px] w-full rounded-2xl border border-slate-200 bg-white px-4 py-3 text-slate-900 outline-none transition focus:border-slate-900"
          placeholder="O que foi bom? O que poderia ter sido melhor? Seu comentário ajuda outros clientes."
          value={comentario}
          onChange={(event) => setComentario(event.target.value)}
          maxLength={1000}
        />

        <button
          type="submit"
          disabled={isPending}
          className="w-full rounded-2xl bg-slate-900 px-4 py-3 text-sm font-semibold text-white transition hover:bg-slate-800 disabled:cursor-not-allowed disabled:opacity-50"
        >
          {isPending ? 'Enviando avaliação...' : 'Enviar avaliação'}
        </button>
      </form>
    </section>
  );
}
