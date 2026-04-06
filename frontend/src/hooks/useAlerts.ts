'use client';

import useSWR from 'swr';
import { apiFetch } from '@/lib/api';
import { BackgroundJobFilaAlertaResponse } from '@/types/api';

export function useAlerts() {
  return useSWR<BackgroundJobFilaAlertaResponse[]>('/api/admin/jobs/fila/alertas', apiFetch);
}
