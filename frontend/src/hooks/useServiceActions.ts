'use client';

import { useState } from 'react';
import { useSWRConfig } from 'swr';
import { apiSend } from '@/lib/api';
import { ServicoResponse } from '@/types/api';

type ServiceAction = 'aceitar' | 'iniciar' | 'concluir' | 'cancelar';

function isServiceKey(key: unknown) {
  return typeof key === 'string' && key.startsWith('/api/servicos');
}

export function useServiceActions() {
  const { mutate } = useSWRConfig();
  const [busyAction, setBusyAction] = useState<string | null>(null);

  async function runAction(servicoId: string, action: ServiceAction) {
    setBusyAction(`${action}:${servicoId}`);

    try {
      const response = await apiSend<ServicoResponse>(`/api/servicos/${servicoId}/${action}`, {
        method: 'PUT',
      });

      await mutate(isServiceKey);

      return response;
    } finally {
      setBusyAction(null);
    }
  }

  return {
    busyAction,
    acceptService: (servicoId: string) => runAction(servicoId, 'aceitar'),
    startService: (servicoId: string) => runAction(servicoId, 'iniciar'),
    concludeService: (servicoId: string) => runAction(servicoId, 'concluir'),
    cancelService: (servicoId: string) => runAction(servicoId, 'cancelar'),
  };
}
