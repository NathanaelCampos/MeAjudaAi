'use client';

import useSWR from 'swr';
import { apiFetch } from '@/lib/api';
import { PaginacaoResponse, ProfissionalResumoResponse } from '@/types/api';

export interface ProfessionalSearchFilters {
  nome?: string;
  profissaoId?: string;
  cidadeId?: string;
  ordenacao?: string;
  pagina?: number;
  tamanhoPagina?: number;
}

function buildSearchUrl(filters: ProfessionalSearchFilters) {
  const params = new URLSearchParams();

  if (filters.nome?.trim()) {
    params.set('nome', filters.nome.trim());
  }
  if (filters.profissaoId?.trim()) {
    params.set('profissaoId', filters.profissaoId.trim());
  }
  if (filters.cidadeId?.trim()) {
    params.set('cidadeId', filters.cidadeId.trim());
  }
  if (filters.ordenacao?.trim()) {
    params.set('ordenacao', filters.ordenacao.trim());
  }
  params.set('pagina', String(filters.pagina ?? 1));
  params.set('tamanhoPagina', String(filters.tamanhoPagina ?? 12));

  return `/api/profissionais/buscar?${params.toString()}`;
}

export function useProfessionalSearch(filters: ProfessionalSearchFilters) {
  return useSWR<PaginacaoResponse<ProfissionalResumoResponse>>(buildSearchUrl(filters), apiFetch);
}
