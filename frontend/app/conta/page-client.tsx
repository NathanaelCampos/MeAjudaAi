'use client';

import { useEffect } from 'react';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { NotificationPreferencesForm } from '@/components/NotificationPreferencesForm';
import { useAuth } from '@/providers/auth-provider';

function formatDate(value?: string) {
  if (!value) {
    return 'Não informado';
  }

  const date = new Date(value);

  if (Number.isNaN(date.getTime())) {
    return value;
  }

  return new Intl.DateTimeFormat('pt-BR', {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(date);
}

export function AccountPageClient() {
  const router = useRouter();
  const { session, hydrated } = useAuth();

  useEffect(() => {
    if (!hydrated) {
      return;
    }

    if (!session?.token) {
      router.replace('/login?next=%2Fconta');
    }
  }, [hydrated, router, session]);

  return (
    <main className="min-h-screen bg-[linear-gradient(180deg,#fff7ed_0%,#f8fafc_100%)] px-4 py-6">
      <div className="mx-auto flex max-w-5xl flex-col gap-6">
        <section className="rounded-[2.2rem] border border-white/80 bg-white/85 p-6 shadow-[0_28px_80px_rgba(15,23,42,0.08)]">
          <p className="text-xs font-semibold uppercase tracking-[0.22em] text-slate-500">Conta</p>
          <h1 className="mt-2 text-3xl font-semibold text-slate-900">Sua área autenticada</h1>
          <p className="mt-3 max-w-2xl text-sm leading-7 text-slate-600">
            Revise os dados básicos da sessão e ajuste como quer receber comunicações do app.
          </p>

          <div className="mt-5 grid gap-3 sm:grid-cols-2 xl:grid-cols-4">
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
              <p className="mt-2 text-sm font-medium text-slate-900">{session?.role || 'Usuário'}</p>
            </div>
            <div className="rounded-[1.4rem] border border-slate-200 bg-slate-50 p-4">
              <p className="text-xs font-semibold uppercase tracking-[0.18em] text-slate-500">Sessão expira</p>
              <p className="mt-2 text-sm font-medium text-slate-900">{formatDate(session?.expiraEmUtc)}</p>
            </div>
          </div>

          <div className="mt-5 flex flex-wrap gap-3">
            <Link
              href="/explorar"
              className="rounded-full border border-slate-200 bg-white px-4 py-2 text-sm font-medium text-slate-700 transition hover:bg-slate-50"
            >
              Explorar profissionais
            </Link>
            <Link
              href="/servicos"
              className="rounded-full border border-slate-200 bg-white px-4 py-2 text-sm font-medium text-slate-700 transition hover:bg-slate-50"
            >
              Ver meus serviços
            </Link>
            {session?.role === 'Profissional' ? (
              <Link
                href="/onboarding/profissional"
                className="rounded-full border border-slate-200 bg-white px-4 py-2 text-sm font-medium text-slate-700 transition hover:bg-slate-50"
              >
                Editar onboarding profissional
              </Link>
            ) : null}
          </div>
        </section>

        <NotificationPreferencesForm
          title="Preferências de notificação"
          description="Controle o que chega por e-mail e o que deve aparecer dentro do app."
          submitLabel="Salvar preferências"
          successTitle="Conta atualizada"
          successMessage="Suas preferências de notificação foram salvas."
        />
      </div>
    </main>
  );
}
