'use client';

import { useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { NotificationPreferencesForm } from '@/components/NotificationPreferencesForm';
import { useAuth } from '@/providers/auth-provider';

export function ClientOnboardingPageClient() {
  const router = useRouter();
  const { session, hydrated } = useAuth();

  useEffect(() => {
    if (!hydrated) {
      return;
    }

    if (!session?.token) {
      router.replace('/login?next=%2Fonboarding%2Fcliente');
      return;
    }

    if (session.role === 'Profissional') {
      router.replace('/onboarding/profissional');
      return;
    }
  }, [hydrated, router, session]);

  return (
    <main className="min-h-screen bg-[linear-gradient(180deg,#fff7ed_0%,#f8fafc_100%)] px-4 py-6">
      <div className="mx-auto flex max-w-4xl flex-col gap-6">
        <section className="rounded-[2.2rem] border border-white/80 bg-white/85 p-6 shadow-[0_28px_80px_rgba(15,23,42,0.08)]">
          <p className="text-xs font-semibold uppercase tracking-[0.22em] text-slate-500">Onboarding do cliente</p>
          <h1 className="mt-2 text-3xl font-semibold text-slate-900">Ajuste sua conta antes de começar</h1>
          <p className="mt-3 max-w-2xl text-sm leading-7 text-slate-600">
            Sua conta já está criada. Agora escolha como quer receber avisos sobre solicitações,
            aceite, conclusão e atualizações do app.
          </p>

          <div className="mt-5 grid gap-3 sm:grid-cols-3">
            <div className="rounded-[1.4rem] border border-slate-200 bg-slate-50 p-4">
              <p className="text-xs font-semibold uppercase tracking-[0.18em] text-slate-500">Nome</p>
              <p className="mt-2 text-sm font-medium text-slate-900">{session?.nome || 'Conta autenticada'}</p>
            </div>
            <div className="rounded-[1.4rem] border border-slate-200 bg-slate-50 p-4">
              <p className="text-xs font-semibold uppercase tracking-[0.18em] text-slate-500">E-mail</p>
              <p className="mt-2 text-sm font-medium text-slate-900">{session?.email || 'Não informado'}</p>
            </div>
            <div className="rounded-[1.4rem] border border-slate-200 bg-slate-50 p-4">
              <p className="text-xs font-semibold uppercase tracking-[0.18em] text-slate-500">Perfil</p>
              <p className="mt-2 text-sm font-medium text-slate-900">{session?.role || 'Cliente'}</p>
            </div>
          </div>
        </section>

        <NotificationPreferencesForm
          title="Preferências iniciais"
          description="Você pode alterar isso depois em Conta. O importante aqui é começar com os alertas certos ligados."
          submitLabel="Salvar e começar a explorar"
          successTitle="Preferências configuradas"
          successMessage="Sua conta está pronta. Agora você já pode explorar profissionais e solicitar serviços."
          onSaved={() => router.replace('/explorar')}
        />
      </div>
    </main>
  );
}
