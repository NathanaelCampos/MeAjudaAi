'use client';

import useSWR from 'swr';
import { apiFetch } from '@/lib/api';
import { BairroResponse } from '@/types/api';

export function useBairros(cidadeId?: string) {
  return useSWR<BairroResponse[]>(
    cidadeId ? `/api/cidades/${cidadeId}/bairros` : null,
    apiFetch,
  );
}
