import useSWR from 'swr';

const fetcher = (url: string) => fetch(url).then((res) => res.json());

export function useMetrics() {
  return useSWR('/api/admin/jobs/fila/metricas', fetcher);
}
