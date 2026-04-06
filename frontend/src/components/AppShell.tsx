'use client';

import { ReactNode } from 'react';
import { usePathname } from 'next/navigation';
import { AppBottomNav } from '@/components/AppBottomNav';
import { AppHeader } from '@/components/AppHeader';
import { AuthGuard } from '@/components/AuthGuard';
import { ProductHeader } from '@/components/ProductHeader';
import { isProductRoute, usesMinimalShell } from '@/lib/routes';

export function AppShell({ children }: { children: ReactNode }) {
  const pathname = usePathname();
  const showAdminHeader = !usesMinimalShell(pathname);
  const showProductHeader = isProductRoute(pathname);

  return (
    <AuthGuard>
      {showAdminHeader ? <AppHeader /> : null}
      {showProductHeader ? <ProductHeader /> : null}
      <div className={showAdminHeader ? 'pb-28 md:pb-0' : ''}>
        <div className="relative">
          <div className="pointer-events-none absolute inset-x-0 top-0 h-40 bg-[radial-gradient(circle_at_top,rgba(217,119,6,0.14),transparent_66%)]" />
          {children}
        </div>
      </div>
      {showAdminHeader ? <AppBottomNav /> : null}
    </AuthGuard>
  );
}
