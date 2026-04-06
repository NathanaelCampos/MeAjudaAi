'use client';

import useSWR, { useSWRConfig } from 'swr';
import { useState } from 'react';
import { apiFetch, apiSend } from '@/lib/api';
import { PreferenciaNotificacaoResponse } from '@/types/api';

const PREFERENCES_PATH = '/api/notificacoes/minhas/preferencias';

interface SaveNotificationPreferencesPayload {
  preferencias: Array<{
    tipo: number | string;
    ativoInterno: boolean;
    ativoEmail: boolean;
  }>;
}

export function useNotificationPreferences() {
  return useSWR<PreferenciaNotificacaoResponse[]>(PREFERENCES_PATH, apiFetch);
}

export function useNotificationPreferencesActions() {
  const { mutate } = useSWRConfig();
  const [isPending, setIsPending] = useState(false);

  async function savePreferences(payload: SaveNotificationPreferencesPayload) {
    setIsPending(true);

    try {
      const response = await apiSend<PreferenciaNotificacaoResponse[]>(PREFERENCES_PATH, {
        method: 'PUT',
        body: {
          preferencias: payload.preferencias,
        },
      });

      await mutate(PREFERENCES_PATH, response, false);
      return response;
    } finally {
      setIsPending(false);
    }
  }

  return {
    isPending,
    savePreferences,
  };
}
