'use client';

import Link from 'next/link';
import { usePathname } from 'next/navigation';
import clsx from 'clsx';

type AppRoute = '/' | '/jobs' | '/fila' | '/historico' | '/retries';

const items = [
  { href: '/', label: 'Home', shortLabel: 'Home', icon: '●' },
  { href: '/jobs', label: 'Jobs', shortLabel: 'Jobs', icon: '◆' },
  { href: '/fila', label: 'Fila', shortLabel: 'Fila', icon: '◌' },
  { href: '/historico', label: 'Historico', shortLabel: 'Hist.', icon: '◐' },
  { href: '/retries', label: 'Retries', shortLabel: 'Retry', icon: '↺' },
] satisfies ReadonlyArray<{ href: AppRoute; label: string; shortLabel: string; icon: string }>;

function isActive(pathname: string, href: AppRoute) {
  if (href === '/') {
    return pathname === '/';
  }

  return pathname === href || pathname.startsWith(`${href}/`);
}

export function AppBottomNav() {
  const pathname = usePathname();

  return (
    <nav className="fixed inset-x-0 bottom-0 z-30 px-3 pb-[calc(0.8rem+env(safe-area-inset-bottom))] pt-2 backdrop-blur md:hidden">
      <div className="mx-auto grid max-w-md grid-cols-5 gap-2 rounded-[1.7rem] border border-white/70 bg-[rgba(15,23,42,0.84)] px-2 py-2 shadow-[0_22px_70px_rgba(15,23,42,0.28)]">
        {items.map((item) => {
          const active = isActive(pathname, item.href);

          return (
            <Link
              key={item.href}
              href={item.href}
              className={clsx(
                'flex min-h-[58px] flex-col items-center justify-center rounded-[1.2rem] px-2 py-2 text-[11px] font-semibold transition',
                active
                  ? 'bg-white text-slate-900 shadow-[0_10px_30px_rgba(255,255,255,0.14)]'
                  : 'text-white/62 hover:bg-white/8',
              )}
            >
              <span className={clsx('mb-1 text-sm leading-none', active ? 'text-amber-500' : 'text-white/55')}>
                {item.icon}
              </span>
              <span>{item.shortLabel}</span>
            </Link>
          );
        })}
      </div>
    </nav>
  );
}
