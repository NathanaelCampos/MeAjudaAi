'use client';

import useSWR from 'swr';
import { apiFetch } from '@/lib/api';
import { ProfissaoResponse } from '@/types/api';

export function useProfissoes() {
  return useSWR<ProfissaoResponse[]>('/api/profissoes', apiFetch);
}
