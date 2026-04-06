'use client';

import { useState } from 'react';
import { useSWRConfig } from 'swr';
import { apiSend } from '@/lib/api';
import { CriarServicoRequest, ServicoResponse } from '@/types/api';

export function useCreateService() {
  const { mutate } = useSWRConfig();
  const [isPending, setIsPending] = useState(false);
  const [feedback, setFeedback] = useState<string | null>(null);

  async function createService(payload: CriarServicoRequest) {
    setFeedback(null);
    setIsPending(true);

    try {
      const response = await apiSend<ServicoResponse>('/api/servicos', {
        method: 'POST',
        body: payload,
      });

      setFeedback('Solicitacao enviada com sucesso.');
      await mutate('/api/servicos/me/cliente');

      return response;
    } finally {
      setIsPending(false);
    }
  }

  return {
    createService,
    feedback,
    isPending,
    clearFeedback: () => setFeedback(null),
  };
}
