'use client';

import useSWR, { useSWRConfig } from 'swr';
import { useState } from 'react';
import { apiFetch, apiSend } from '@/lib/api';
import { AvaliacaoResponse, CriarAvaliacaoRequest } from '@/types/api';

export function useProfessionalReviews(profissionalId?: string) {
  const path = profissionalId ? `/api/avaliacoes/profissional/${profissionalId}` : null;
  return useSWR<AvaliacaoResponse[]>(path, apiFetch);
}

export function useCreateReview() {
  const { mutate } = useSWRConfig();
  const [isPending, setIsPending] = useState(false);

  async function createReview(payload: CriarAvaliacaoRequest) {
    setIsPending(true);

    try {
      const response = await apiSend<AvaliacaoResponse>('/api/avaliacoes', {
        method: 'POST',
        body: payload,
      });

      await mutate(`/api/avaliacoes/profissional/${response.profissionalId}`);
      await mutate((key: string) => typeof key === 'string' && key.includes(`/api/servicos/${payload.servicoId}`));

      return response;
    } finally {
      setIsPending(false);
    }
  }

  return {
    createReview,
    isPending,
  };
}
