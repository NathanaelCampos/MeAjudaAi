'use client';

import { ReactNode } from 'react';
import { AppShell } from '@/components/AppShell';
import { InstallPrompt } from '@/components/InstallPrompt';
import { ServiceWorkerRegister } from '@/components/ServiceWorkerRegister';
import { AuthProvider } from '@/providers/auth-provider';
import { ToastProvider } from '@/providers/toast-provider';

export function Providers({ children }: { children: ReactNode }) {
  return (
    <ToastProvider>
      <ServiceWorkerRegister />
      <InstallPrompt />
      <AuthProvider>
        <AppShell>{children}</AppShell>
      </AuthProvider>
    </ToastProvider>
  );
}
