'use client';

import useSWR from 'swr';
import { apiFetch } from '@/lib/api';
import { BackgroundJobFilaItemResponse } from '@/types/api';

export interface AdminJobsFilters {
  limit?: number;
  jobId?: string;
  status?: string;
}

function buildQueueUrl(filters: AdminJobsFilters) {
  const params = new URLSearchParams();

  if (filters.limit) {
    params.set('limit', String(filters.limit));
  }

  if (filters.jobId?.trim()) {
    params.set('jobId', filters.jobId.trim());
  }

  if (filters.status?.trim()) {
    params.set('status', filters.status.trim());
  }

  const query = params.toString();
  return query ? `/api/admin/jobs/fila?${query}` : '/api/admin/jobs/fila';
}

export function useAdminJobs(filters: AdminJobsFilters = {}) {
  const { data, error, isLoading } = useSWR<BackgroundJobFilaItemResponse[]>(
    buildQueueUrl(filters),
    apiFetch,
  );

  return {
    data: data ?? null,
    error,
    isLoading,
  };
}
