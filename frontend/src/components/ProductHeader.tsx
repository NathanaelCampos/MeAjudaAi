'use client';

import Link from 'next/link';
import { usePathname, useRouter } from 'next/navigation';
import clsx from 'clsx';
import { useAuth } from '@/providers/auth-provider';

const items = [
  { href: '/explorar', label: 'Explorar' },
  { href: '/servicos', label: 'Meus serviços' },
  { href: '/servicos/profissional', label: 'Profissional' },
  { href: '/conta', label: 'Conta' },
] as const;

function isActive(pathname: string, href: string) {
  if (href === '/explorar') {
    return pathname === '/explorar' || pathname.startsWith('/profissionais/');
  }

  if (href === '/conta') {
    return pathname === '/conta' || pathname.startsWith('/onboarding/');
  }

  return pathname === href || pathname.startsWith(`${href}/`);
}

export function ProductHeader() {
  const pathname = usePathname();
  const router = useRouter();
  const { isAuthenticated, logout, session } = useAuth();
  const visibleItems = isAuthenticated ? items : items.filter((item) => item.href !== '/conta');

  function handleLogout() {
    logout();
    router.replace('/explorar');
  }

  return (
    <header className="sticky top-0 z-20 border-b border-white/60 bg-[rgba(255,249,242,0.86)] backdrop-blur-xl">
      <div className="mx-auto flex max-w-6xl flex-col gap-4 px-4 pb-4 pt-3">
        <div className="flex items-start justify-between gap-4">
          <div className="min-w-0">
            <div className="flex items-center gap-2">
              <span className="inline-flex h-2.5 w-2.5 rounded-full bg-amber-500 shadow-[0_0_0_4px_rgba(245,158,11,0.16)]" />
              <p className="text-[11px] font-semibold uppercase tracking-[0.26em] text-slate-500">Produto</p>
            </div>
            <h1 data-display="true" className="mt-2 text-2xl font-semibold leading-tight text-slate-900 sm:text-[2rem]">
              Me Ajuda Ai
            </h1>
            <p className="mt-1 max-w-xl text-sm leading-6 text-slate-500">
              Descubra profissionais, solicite serviços e acompanhe tudo em uma experiência pensada para tela pequena.
            </p>
          </div>

          <div className="flex shrink-0 flex-col items-end gap-2">
            {isAuthenticated ? (
              <>
                <div className="rounded-full border border-white/80 bg-white/90 px-3 py-1 text-[11px] font-medium text-slate-500 shadow-sm">
                  {session?.nome || 'Sessão ativa'}
                </div>
                <div className="hidden flex-wrap justify-end gap-2 sm:flex">
                  <Link
                    href="/conta"
                    className="rounded-full border border-slate-200 bg-white px-4 py-2 text-xs font-semibold uppercase tracking-wide text-slate-700 transition hover:bg-slate-50"
                  >
                    Conta
                  </Link>
                  <Link
                    href="/"
                    className="rounded-full border border-slate-200 bg-white px-4 py-2 text-xs font-semibold uppercase tracking-wide text-slate-700 transition hover:bg-slate-50"
                  >
                    Painel
                  </Link>
                  <button
                    type="button"
                    onClick={handleLogout}
                    className="rounded-full border border-slate-200 bg-white px-4 py-2 text-xs font-semibold uppercase tracking-wide text-slate-700 transition hover:bg-slate-50"
                  >
                    Sair
                  </button>
                </div>
              </>
            ) : (
              <div className="hidden gap-2 sm:flex">
                <Link
                  href="/cadastro"
                  className="rounded-full bg-slate-900 px-4 py-2 text-xs font-semibold uppercase tracking-wide text-white transition hover:bg-slate-800"
                >
                  Criar conta
                </Link>
                <Link
                  href="/login"
                  className="rounded-full border border-slate-200 bg-white px-4 py-2 text-xs font-semibold uppercase tracking-wide text-slate-700 transition hover:bg-slate-50"
                >
                  Entrar
                </Link>
              </div>
            )}
          </div>
        </div>

        <div className="rounded-[1.8rem] border border-white/70 bg-white/72 p-2 shadow-[0_16px_40px_rgba(15,23,42,0.06)]">
          {!isAuthenticated ? (
            <div className="mb-2 flex gap-2 sm:hidden">
              <Link
                href="/cadastro"
                className="flex-1 rounded-[1.1rem] bg-slate-900 px-4 py-3 text-center text-xs font-semibold uppercase tracking-wide text-white transition hover:bg-slate-800"
              >
                Criar conta
              </Link>
              <Link
                href="/login"
                className="flex-1 rounded-[1.1rem] border border-slate-200 bg-white px-4 py-3 text-center text-xs font-semibold uppercase tracking-wide text-slate-700 transition hover:bg-slate-50"
              >
                Entrar
              </Link>
            </div>
          ) : null}

          <nav className="flex gap-2 overflow-x-auto pb-1">
          {visibleItems.map((item) => {
            const active = isActive(pathname, item.href);

            return (
              <Link
                key={item.href}
                href={item.href}
                className={clsx(
                  'whitespace-nowrap rounded-[1.1rem] px-4 py-2.5 text-sm font-semibold transition-colors',
                  active
                    ? 'bg-slate-900 text-white shadow-[0_12px_30px_rgba(15,23,42,0.16)]'
                    : 'bg-white text-slate-700 shadow-sm ring-1 ring-slate-200 hover:bg-slate-50',
                )}
              >
                {item.label}
              </Link>
            );
          })}
          </nav>
        </div>
      </div>
    </header>
  );
}
