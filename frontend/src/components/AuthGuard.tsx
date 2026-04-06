'use client';

import { ReactNode, useEffect } from 'react';
import { usePathname, useRouter } from 'next/navigation';
import { isPublicRoute } from '@/lib/routes';
import { useAuth } from '@/providers/auth-provider';

export function AuthGuard({ children }: { children: ReactNode }) {
  const pathname = usePathname();
  const router = useRouter();
  const { hydrated, isAuthenticated } = useAuth();
  const publicRoute = isPublicRoute(pathname);

  useEffect(() => {
    if (!hydrated) {
      return;
    }

    if (!isAuthenticated && !publicRoute) {
      const search = typeof window !== 'undefined' ? window.location.search : '';
      const nextPath = `${pathname}${search}`;
      router.replace(`/login?next=${encodeURIComponent(nextPath)}`);
      return;
    }

    if (isAuthenticated && pathname === '/login') {
      router.replace('/');
    }
  }, [hydrated, isAuthenticated, pathname, publicRoute, router]);

  if (!hydrated) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-[linear-gradient(180deg,#f5f2ea_0%,#eef1f6_100%)] px-6">
        <div className="rounded-[1.8rem] border border-white/70 bg-white/80 px-6 py-5 text-sm text-slate-500 shadow-[0_20px_60px_rgba(15,23,42,0.08)]">
          Inicializando painel...
        </div>
      </div>
    );
  }

  if (!isAuthenticated && !publicRoute) {
    return null;
  }

  if (isAuthenticated && pathname === '/login') {
    return null;
  }

  return <>{children}</>;
}
