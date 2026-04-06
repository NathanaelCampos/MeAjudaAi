'use client';

import useSWR from 'swr';
import { apiFetch } from '@/lib/api';
import { BackgroundJobFilaAlertasHistoricoResponse } from '@/types/api';

export function useAlertsHistory(days = 7) {
  return useSWR<BackgroundJobFilaAlertasHistoricoResponse[]>(
    `/api/admin/jobs/fila/alertas/historico?dias=${days}`,
    apiFetch,
  );
}
