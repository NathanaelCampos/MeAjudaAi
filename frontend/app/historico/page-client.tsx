'use client';

import { FormEvent, useEffect, useMemo, useState } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import { HistoryCard } from '@/components/HistoryCard';
import { useAlertsHistory } from '@/hooks/useAlertsHistory';

function Placeholder({ label }: { label: string }) {
  return (
    <div className="rounded-[1.5rem] border border-dashed border-slate-300 bg-white/60 p-5 text-sm text-slate-400">
      {label}
    </div>
  );
}

export function HistoryPageClient() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const [days, setDays] = useState(searchParams.get('dias') ?? '14');
  const [jobId, setJobId] = useState(searchParams.get('jobId') ?? '');

  useEffect(() => {
    setDays(searchParams.get('dias') ?? '14');
    setJobId(searchParams.get('jobId') ?? '');
  }, [searchParams]);

  const activeDays = Number(searchParams.get('dias') ?? '14') || 14;
  const activeJobId = searchParams.get('jobId')?.trim().toLowerCase() ?? '';
  const { data, isLoading, error } = useAlertsHistory(activeDays);

  const filteredData = useMemo(() => {
    if (!data) {
      return [];
    }

    if (!activeJobId) {
      return data;
    }

    return data.filter((item) => item.jobId.toLowerCase().includes(activeJobId));
  }, [activeJobId, data]);

  const totalAlertas = filteredData.reduce((sum, item) => sum + item.totalAlertas, 0);
  const totalFalhas = filteredData.reduce((sum, item) => sum + item.totalFalhas, 0);

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    const params = new URLSearchParams();
    if (days.trim()) {
      params.set('dias', days.trim());
    }
    if (jobId.trim()) {
      params.set('jobId', jobId.trim());
    }

    const query = params.toString();
    router.replace(query ? `/historico?${query}` : '/historico');
  }

  return (
    <main className="min-h-screen bg-[linear-gradient(180deg,#f5f2ea_0%,#eef1f6_100%)] px-4 py-6">
      <div className="mx-auto flex max-w-5xl flex-col gap-6">
        <section className="rounded-[2rem] border border-white/60 bg-white/75 p-5 shadow-[0_20px_60px_rgba(15,23,42,0.08)]">
          <p className="text-xs font-semibold uppercase tracking-[0.2em] text-slate-500">Historico de alertas</p>
          <h2 className="mt-2 text-2xl font-semibold text-slate-900">Ultimos {activeDays} dias</h2>
          <p className="mt-2 max-w-2xl text-sm leading-6 text-slate-600">
            Esta visao ajuda a entender quais jobs degradaram de forma recorrente ao longo dos dias.
          </p>

          <form className="mt-5 grid gap-3 md:grid-cols-[140px_1fr_auto]" onSubmit={handleSubmit}>
            <input
              className="rounded-2xl border border-slate-200 bg-white px-4 py-3 text-sm text-slate-900 outline-none transition focus:border-slate-900"
              placeholder="14"
              value={days}
              onChange={(event) => setDays(event.target.value)}
              inputMode="numeric"
            />
            <input
              className="rounded-2xl border border-slate-200 bg-white px-4 py-3 text-sm text-slate-900 outline-none transition focus:border-slate-900"
              placeholder="Filtrar por jobId"
              value={jobId}
              onChange={(event) => setJobId(event.target.value)}
            />
            <button
              type="submit"
              className="rounded-2xl bg-slate-900 px-4 py-3 text-sm font-semibold text-white transition hover:bg-slate-800"
            >
              Aplicar
            </button>
          </form>

          <div className="mt-4 grid gap-3 sm:grid-cols-3">
            <div className="rounded-[1.25rem] bg-slate-50 px-4 py-3">
              <p className="text-[11px] font-semibold uppercase tracking-[0.18em] text-slate-500">Registros</p>
              <p className="mt-2 text-xl font-semibold text-slate-900">{filteredData.length}</p>
            </div>
            <div className="rounded-[1.25rem] bg-slate-50 px-4 py-3">
              <p className="text-[11px] font-semibold uppercase tracking-[0.18em] text-slate-500">Alertas</p>
              <p className="mt-2 text-xl font-semibold text-slate-900">{totalAlertas}</p>
            </div>
            <div className="rounded-[1.25rem] bg-slate-50 px-4 py-3">
              <p className="text-[11px] font-semibold uppercase tracking-[0.18em] text-slate-500">Falhas</p>
              <p className="mt-2 text-xl font-semibold text-slate-900">{totalFalhas}</p>
            </div>
          </div>
        </section>

        {isLoading ? (
          <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-3">
            {Array.from({ length: 3 }).map((_, idx) => (
              <Placeholder key={idx} label="Carregando historico..." />
            ))}
          </div>
        ) : error ? (
          <Placeholder label="Falha ao carregar historico." />
        ) : !filteredData.length ? (
          <Placeholder label="Nenhum historico disponivel." />
        ) : (
          <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-3">
            {filteredData.map((item) => (
              <HistoryCard key={`${item.jobId}-${item.data}`} item={item} />
            ))}
          </div>
        )}
      </div>
    </main>
  );
}
