'use client';

import useSWR from 'swr';
import { apiFetch } from '@/lib/api';
import { ServicoResponse } from '@/types/api';

export function useServiceDetails(id: string) {
  return useSWR<ServicoResponse>(id ? `/api/servicos/${id}` : null, apiFetch);
}
