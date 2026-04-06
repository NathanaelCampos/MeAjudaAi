'use client';

import useSWR from 'swr';
import { apiFetch } from '@/lib/api';
import { BackgroundJobFilaMetricasResponse } from '@/types/api';

export function useMetrics() {
  return useSWR<BackgroundJobFilaMetricasResponse>('/api/admin/jobs/fila/metricas', apiFetch);
}
