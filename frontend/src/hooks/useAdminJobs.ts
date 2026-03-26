import useSWR from 'swr';

const fetcher = (url: string) => fetch(url).then((res) => res.json());

export function useAdminJobs(limit = 10) {
  const { data, error, isLoading } = useSWR(`/api/admin/jobs/fila?limit=${limit}`, fetcher);

  return {
    data: data ?? null,
    error,
    isLoading,
  };
}
