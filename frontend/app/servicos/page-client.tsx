'use client';

import Link from 'next/link';
import { FormEvent, useEffect, useState } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import { ServiceRequestCard } from '@/components/ServiceRequestCard';
import { useMyClientServices } from '@/hooks/useMyClientServices';

const statusOptions = [
  { value: '', label: 'Todos' },
  { value: 'Solicitado', label: 'Solicitado' },
  { value: 'Aceito', label: 'Aceito' },
  { value: 'EmExecucao', label: 'Em execução' },
  { value: 'Concluido', label: 'Concluído' },
  { value: 'Cancelado', label: 'Cancelado' },
];

function Placeholder({ label }: { label: string }) {
  return (
    <div className="rounded-[1.6rem] border border-dashed border-slate-300 bg-white/70 p-5 text-sm text-slate-400">
      {label}
    </div>
  );
}

export function MyServicesPageClient() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const [status, setStatus] = useState(searchParams.get('status') ?? '');

  useEffect(() => {
    setStatus(searchParams.get('status') ?? '');
  }, [searchParams]);

  const activeStatus = searchParams.get('status') ?? '';
  const { data, isLoading, error } = useMyClientServices(activeStatus);

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    const params = new URLSearchParams();
    if (status.trim()) {
      params.set('status', status.trim());
    }

    const query = params.toString();
    router.replace(query ? `/servicos?${query}` : '/servicos');
  }

  return (
    <main className="min-h-screen bg-[linear-gradient(180deg,#fff7ed_0%,#f8fafc_100%)] px-4 py-6">
      <div className="mx-auto flex max-w-5xl flex-col gap-6">
        <section className="rounded-[2.2rem] border border-white/80 bg-white/85 p-6 shadow-[0_28px_80px_rgba(15,23,42,0.08)]">
          <div className="flex flex-wrap items-center justify-between gap-3">
            <div>
              <p className="text-xs font-semibold uppercase tracking-[0.22em] text-slate-500">Área do cliente</p>
              <h1 className="mt-2 text-3xl font-semibold text-slate-900">Meus serviços</h1>
              <p className="mt-3 max-w-2xl text-sm leading-7 text-slate-600">
                Acompanhe solicitações enviadas, aceite, andamento e conclusão.
              </p>
            </div>

            <Link
              href="/explorar"
              className="rounded-full border border-slate-200 bg-white px-4 py-2 text-sm font-medium text-slate-700 transition hover:bg-slate-50"
            >
              Explorar profissionais
            </Link>
          </div>

          <form className="mt-5 grid gap-3 sm:grid-cols-[1fr_auto]" onSubmit={handleSubmit}>
            <select
              className="rounded-2xl border border-slate-200 bg-white px-4 py-3 text-sm text-slate-900 outline-none transition focus:border-slate-900"
              value={status}
              onChange={(event) => setStatus(event.target.value)}
            >
              {statusOptions.map((option) => (
                <option key={option.value || 'todos'} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>

            <button
              type="submit"
              className="rounded-2xl bg-slate-900 px-4 py-3 text-sm font-semibold text-white transition hover:bg-slate-800"
            >
              Filtrar
            </button>
          </form>
        </section>

        {isLoading ? (
          <div className="grid gap-4 md:grid-cols-2">
            {Array.from({ length: 4 }).map((_, index) => (
              <Placeholder key={index} label="Carregando solicitações..." />
            ))}
          </div>
        ) : error ? (
          <Placeholder label="Falha ao carregar os serviços do cliente." />
        ) : !data?.length ? (
          <Placeholder label="Nenhum serviço encontrado para este filtro." />
        ) : (
          <div className="grid gap-4 md:grid-cols-2">
            {data.map((item) => (
              <ServiceRequestCard key={item.id} item={item} />
            ))}
          </div>
        )}
      </div>
    </main>
  );
}
