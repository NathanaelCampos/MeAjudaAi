'use client';

import useSWR from 'swr';
import { apiFetch } from '@/lib/api';
import { BackgroundJobAdminItemResponse } from '@/types/api';

export function useJobCatalog() {
  return useSWR<BackgroundJobAdminItemResponse[]>('/api/admin/jobs', apiFetch);
}
