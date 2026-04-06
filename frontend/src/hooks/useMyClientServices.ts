'use client';

import useSWR from 'swr';
import { apiFetch } from '@/lib/api';
import { ServicoResponse } from '@/types/api';

function buildUrl(status?: string) {
  if (!status?.trim()) {
    return '/api/servicos/me/cliente';
  }

  return `/api/servicos/me/cliente?status=${encodeURIComponent(status.trim())}`;
}

export function useMyClientServices(status?: string) {
  return useSWR<ServicoResponse[]>(buildUrl(status), apiFetch);
}
