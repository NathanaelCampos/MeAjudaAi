'use client';

import useSWR from 'swr';
import { apiFetch } from '@/lib/api';
import { BackgroundJobRetryLogResponse } from '@/types/api';

export function useRetryLogs(top = 20) {
  return useSWR<BackgroundJobRetryLogResponse[]>(
    `/api/admin/jobs/fila/logs/retries?top=${top}`,
    apiFetch,
  );
}
