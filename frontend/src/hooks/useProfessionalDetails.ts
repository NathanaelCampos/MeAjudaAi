'use client';

import useSWR from 'swr';
import { apiFetch } from '@/lib/api';
import { ProfissionalDetalhesResponse } from '@/types/api';

export function useProfessionalDetails(id: string) {
  return useSWR<ProfissionalDetalhesResponse>(
    id ? `/api/profissionais/${id}/detalhes` : null,
    apiFetch,
  );
}
