'use client';

import { AvaliacaoResponse } from '@/types/api';

function formatDate(value: string) {
  return new Intl.DateTimeFormat('pt-BR', {
    dateStyle: 'medium',
  }).format(new Date(value));
}

function scoreLabel(value: number | string) {
  if (typeof value === 'number') {
    return `${value}/5`;
  }

  if (/^\d+$/.test(value)) {
    return `${value}/5`;
  }

  return String(value);
}

export function ReviewList({
  reviews,
  emptyLabel = 'Ainda não existem avaliações públicas para este profissional.',
}: {
  reviews?: AvaliacaoResponse[];
  emptyLabel?: string;
}) {
  if (!reviews?.length) {
    return (
      <section className="rounded-[1.8rem] border border-slate-200 bg-white/90 p-5">
        <p className="text-xs font-semibold uppercase tracking-[0.2em] text-slate-500">Avaliações</p>
        <p className="mt-4 text-sm text-slate-500">{emptyLabel}</p>
      </section>
    );
  }

  return (
    <section className="rounded-[1.8rem] border border-slate-200 bg-white/90 p-5">
      <p className="text-xs font-semibold uppercase tracking-[0.2em] text-slate-500">Avaliações</p>
      <div className="mt-4 space-y-4">
        {reviews.map((review) => (
          <article key={review.id} className="rounded-[1.5rem] border border-slate-200 bg-slate-50 p-4">
            <div className="flex flex-wrap items-center justify-between gap-3">
              <div>
                <p className="text-sm font-semibold text-slate-900">{review.nomeCliente}</p>
                <p className="mt-1 text-xs uppercase tracking-[0.18em] text-slate-400">
                  {formatDate(review.dataCriacao)}
                </p>
              </div>

              <div className="flex flex-wrap gap-2 text-xs font-semibold text-slate-600">
                <span className="rounded-full bg-white px-3 py-1">Atendimento {scoreLabel(review.notaAtendimento)}</span>
                <span className="rounded-full bg-white px-3 py-1">Serviço {scoreLabel(review.notaServico)}</span>
                <span className="rounded-full bg-white px-3 py-1">Preço {scoreLabel(review.notaPreco)}</span>
              </div>
            </div>

            <p className="mt-3 text-sm leading-7 text-slate-600">
              {review.comentario || 'Comentário enviado sem detalhes adicionais.'}
            </p>
          </article>
        ))}
      </div>
    </section>
  );
}
