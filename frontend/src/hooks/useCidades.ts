'use client';

import useSWR from 'swr';
import { apiFetch } from '@/lib/api';
import { CidadeResponse } from '@/types/api';

export function useCidades() {
  return useSWR<CidadeResponse[]>('/api/cidades', apiFetch);
}
