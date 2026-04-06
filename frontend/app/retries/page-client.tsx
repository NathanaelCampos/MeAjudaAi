'use client';

import { FormEvent, useEffect, useMemo, useState } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import { RetryLogCard } from '@/components/RetryLogCard';
import { useRetryLogs } from '@/hooks/useRetryLogs';

function Placeholder({ label }: { label: string }) {
  return (
    <div className="rounded-[1.5rem] border border-dashed border-slate-300 bg-white/60 p-5 text-sm text-slate-400">
      {label}
    </div>
  );
}

export function RetriesPageClient() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const [top, setTop] = useState(searchParams.get('top') ?? '20');
  const [jobId, setJobId] = useState(searchParams.get('jobId') ?? '');
  const [tipo, setTipo] = useState(searchParams.get('tipo') ?? '');

  useEffect(() => {
    setTop(searchParams.get('top') ?? '20');
    setJobId(searchParams.get('jobId') ?? '');
    setTipo(searchParams.get('tipo') ?? '');
  }, [searchParams]);

  const activeTop = Number(searchParams.get('top') ?? '20') || 20;
  const activeJobId = searchParams.get('jobId')?.trim().toLowerCase() ?? '';
  const activeTipo = searchParams.get('tipo')?.trim().toLowerCase() ?? '';
  const { data, isLoading, error } = useRetryLogs(activeTop);

  const filteredData = useMemo(() => {
    if (!data) {
      return [];
    }

    return data.filter((item) => {
      const matchJob = !activeJobId || item.jobId.toLowerCase().includes(activeJobId);
      const matchTipo = !activeTipo || item.tipo.toLowerCase().includes(activeTipo);
      return matchJob && matchTipo;
    });
  }, [activeJobId, activeTipo, data]);

  const tiposDisponiveis = useMemo(() => {
    if (!data?.length) {
      return [];
    }

    return Array.from(new Set(data.map((item) => item.tipo))).sort((a, b) => a.localeCompare(b));
  }, [data]);

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    const params = new URLSearchParams();
    if (top.trim()) {
      params.set('top', top.trim());
    }
    if (jobId.trim()) {
      params.set('jobId', jobId.trim());
    }
    if (tipo.trim()) {
      params.set('tipo', tipo.trim());
    }

    const query = params.toString();
    router.replace(query ? `/retries?${query}` : '/retries');
  }

  return (
    <main className="min-h-screen bg-[linear-gradient(180deg,#eef1f6_0%,#f6efe4_100%)] px-4 py-6">
      <div className="mx-auto flex max-w-5xl flex-col gap-6">
        <section className="rounded-[2rem] border border-white/60 bg-white/75 p-5 shadow-[0_20px_60px_rgba(15,23,42,0.08)]">
          <p className="text-xs font-semibold uppercase tracking-[0.2em] text-slate-500">Retry logs</p>
          <h2 className="mt-2 text-2xl font-semibold text-slate-900">Falhas e reprocessamentos recentes</h2>
          <p className="mt-2 max-w-2xl text-sm leading-6 text-slate-600">
            Aqui ficam os eventos mais recentes de retry para investigacao operacional.
          </p>

          <form className="mt-5 grid gap-3 md:grid-cols-[120px_1fr_1fr_auto]" onSubmit={handleSubmit}>
            <input
              className="rounded-2xl border border-slate-200 bg-white px-4 py-3 text-sm text-slate-900 outline-none transition focus:border-slate-900"
              placeholder="20"
              value={top}
              onChange={(event) => setTop(event.target.value)}
              inputMode="numeric"
            />
            <input
              className="rounded-2xl border border-slate-200 bg-white px-4 py-3 text-sm text-slate-900 outline-none transition focus:border-slate-900"
              placeholder="Filtrar por jobId"
              value={jobId}
              onChange={(event) => setJobId(event.target.value)}
            />
            <select
              className="rounded-2xl border border-slate-200 bg-white px-4 py-3 text-sm text-slate-900 outline-none transition focus:border-slate-900"
              value={tipo}
              onChange={(event) => setTipo(event.target.value)}
            >
              <option value="">Todos os tipos</option>
              {tiposDisponiveis.map((option) => (
                <option key={option} value={option}>
                  {option}
                </option>
              ))}
            </select>
            <button
              type="submit"
              className="rounded-2xl bg-slate-900 px-4 py-3 text-sm font-semibold text-white transition hover:bg-slate-800"
            >
              Aplicar
            </button>
          </form>

          <div className="mt-4 grid gap-3 sm:grid-cols-3">
            <div className="rounded-[1.25rem] bg-slate-50 px-4 py-3">
              <p className="text-[11px] font-semibold uppercase tracking-[0.18em] text-slate-500">Logs exibidos</p>
              <p className="mt-2 text-xl font-semibold text-slate-900">{filteredData.length}</p>
            </div>
            <div className="rounded-[1.25rem] bg-slate-50 px-4 py-3">
              <p className="text-[11px] font-semibold uppercase tracking-[0.18em] text-slate-500">Tipos distintos</p>
              <p className="mt-2 text-xl font-semibold text-slate-900">{tiposDisponiveis.length}</p>
            </div>
            <div className="rounded-[1.25rem] bg-slate-50 px-4 py-3">
              <p className="text-[11px] font-semibold uppercase tracking-[0.18em] text-slate-500">Consulta</p>
              <p className="mt-2 text-xl font-semibold text-slate-900">Top {activeTop}</p>
            </div>
          </div>
        </section>

        {isLoading ? (
          <div className="grid gap-4 md:grid-cols-2">
            {Array.from({ length: 4 }).map((_, idx) => (
              <Placeholder key={idx} label="Carregando retry..." />
            ))}
          </div>
        ) : error ? (
          <Placeholder label="Falha ao carregar retries." />
        ) : !filteredData.length ? (
          <Placeholder label="Nenhum retry registrado." />
        ) : (
          <div className="grid gap-4 md:grid-cols-2">
            {filteredData.map((item) => (
              <RetryLogCard key={`${item.execucaoId}-${item.dataCriacao}-${item.tipo}`} item={item} />
            ))}
          </div>
        )}
      </div>
    </main>
  );
}
