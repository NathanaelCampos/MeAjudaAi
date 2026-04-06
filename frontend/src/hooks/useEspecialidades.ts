'use client';

import useSWR from 'swr';
import { apiFetch } from '@/lib/api';
import { EspecialidadeResponse } from '@/types/api';

export function useEspecialidades(profissaoIds: string[]) {
  const ids = profissaoIds.filter(Boolean).sort();

  return useSWR<EspecialidadeResponse[]>(
    ids.length ? ['especialidades', ...ids] : null,
    async () => {
      const responses = await Promise.all(
        ids.map((profissaoId) =>
          apiFetch<EspecialidadeResponse[]>(`/api/profissoes/${profissaoId}/especialidades`),
        ),
      );

      return responses.flat();
    },
  );
}
