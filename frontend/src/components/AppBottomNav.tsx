'use client';

import Link from 'next/link';
import { usePathname } from 'next/navigation';
import clsx from 'clsx';

type AppRoute = '/' | '/jobs' | '/fila' | '/historico' | '/retries';

const items = [
  { href: '/', label: 'Home', shortLabel: 'Home' },
  { href: '/jobs', label: 'Jobs', shortLabel: 'Jobs' },
  { href: '/fila', label: 'Fila', shortLabel: 'Fila' },
  { href: '/historico', label: 'Historico', shortLabel: 'Hist.' },
  { href: '/retries', label: 'Retries', shortLabel: 'Retry' },
] satisfies ReadonlyArray<{ href: AppRoute; label: string; shortLabel: string }>;

function isActive(pathname: string, href: AppRoute) {
  if (href === '/') {
    return pathname === '/';
  }

  return pathname === href || pathname.startsWith(`${href}/`);
}

export function AppBottomNav() {
  const pathname = usePathname();

  return (
    <nav className="fixed inset-x-0 bottom-0 z-30 border-t border-slate-200/80 bg-white/90 px-2 pb-[calc(0.65rem+env(safe-area-inset-bottom))] pt-2 backdrop-blur md:hidden">
      <div className="mx-auto grid max-w-md grid-cols-5 gap-2">
        {items.map((item) => {
          const active = isActive(pathname, item.href);

          return (
            <Link
              key={item.href}
              href={item.href}
              className={clsx(
                'flex min-h-[58px] flex-col items-center justify-center rounded-[1.2rem] px-2 py-2 text-[11px] font-semibold transition',
                active
                  ? 'bg-slate-900 text-white shadow-[0_10px_30px_rgba(15,23,42,0.18)]'
                  : 'text-slate-500 hover:bg-slate-50',
              )}
            >
              <span
                className={clsx(
                  'mb-1 h-1.5 w-6 rounded-full transition',
                  active ? 'bg-amber-300' : 'bg-slate-200',
                )}
              />
              <span>{item.shortLabel}</span>
            </Link>
          );
        })}
      </div>
    </nav>
  );
}
