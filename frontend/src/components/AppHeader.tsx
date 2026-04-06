'use client';

import Link from 'next/link';
import { usePathname, useRouter } from 'next/navigation';
import clsx from 'clsx';
import { useAuth } from '@/providers/auth-provider';

type AppRoute = '/' | '/jobs' | '/fila' | '/historico' | '/retries';

const items = [
  { href: '/', label: 'Dashboard' },
  { href: '/jobs', label: 'Jobs' },
  { href: '/fila', label: 'Fila' },
  { href: '/historico', label: 'Historico' },
  { href: '/retries', label: 'Retries' },
] satisfies ReadonlyArray<{ href: AppRoute; label: string }>;

function isActive(pathname: string, href: AppRoute) {
  if (href === '/') {
    return pathname === '/';
  }

  return pathname === href || pathname.startsWith(`${href}/`);
}

export function AppHeader() {
  const pathname = usePathname();
  const router = useRouter();
  const { logout, session } = useAuth();

  function handleLogout() {
    logout();
    router.replace('/login');
  }

  return (
    <header className="sticky top-0 z-20 border-b border-white/60 bg-[rgba(247,243,234,0.82)] backdrop-blur-xl">
      <div className="mx-auto flex max-w-5xl flex-col gap-4 px-4 pb-4 pt-3">
        <div className="flex items-start justify-between gap-4">
          <div className="min-w-0">
            <div className="flex items-center gap-2">
              <span className="inline-flex h-2.5 w-2.5 rounded-full bg-emerald-500 shadow-[0_0_0_4px_rgba(16,185,129,0.14)]" />
              <p className="text-[11px] font-semibold uppercase tracking-[0.26em] text-amber-700">Me Ajuda Ai</p>
            </div>
            <h1 className="mt-2 text-lg font-semibold leading-tight text-slate-900 sm:text-xl">Console operacional</h1>
            {session?.nome ? (
              <p className="mt-1 truncate text-xs text-slate-500">Sessao ativa: {session.nome}</p>
            ) : null}
          </div>

          <div className="flex shrink-0 flex-col items-end gap-2">
            <div className="rounded-full border border-white/80 bg-white/70 px-3 py-1 text-[11px] font-medium text-slate-500 shadow-sm">
              Mobile first
            </div>
            <button
              type="button"
              onClick={handleLogout}
              className="rounded-full border border-slate-200 bg-white px-4 py-2 text-xs font-semibold uppercase tracking-wide text-slate-700 transition hover:bg-slate-50"
            >
              Sair
            </button>
          </div>
        </div>

        <p className="max-w-2xl text-sm leading-6 text-slate-500">
          Fila, alertas, retries e acoes administrativas com leitura rapida em tela pequena.
        </p>

        <nav className="hidden gap-2 overflow-x-auto pb-1 md:flex">
          {items.map((item) => {
            const active = isActive(pathname, item.href);

            return (
              <Link
                key={item.href}
                href={item.href}
                className={clsx(
                  'whitespace-nowrap rounded-full px-4 py-2 text-sm font-medium transition-colors',
                  active
                    ? 'bg-slate-900 text-white'
                    : 'bg-white text-slate-700 shadow-sm ring-1 ring-slate-200 hover:bg-slate-50',
                )}
              >
                {item.label}
              </Link>
            );
          })}
        </nav>
      </div>
    </header>
  );
}
