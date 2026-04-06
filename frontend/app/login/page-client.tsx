'use client';

import { FormEvent, useState } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import { useAuth } from '@/providers/auth-provider';

export function LoginPageClient() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const { login, isPending } = useAuth();
  const [email, setEmail] = useState('admin@meajudaai.local');
  const [senha, setSenha] = useState('Admin@123');
  const [erro, setErro] = useState<string | null>(null);
  const nextPath = searchParams.get('next') || '/';

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setErro(null);

    try {
      await login({ email, senha });
      window.location.assign(nextPath);
    } catch (error) {
      setErro(error instanceof Error ? error.message : 'Falha ao autenticar.');
    }
  }

  return (
    <main className="flex min-h-screen items-center justify-center bg-[radial-gradient(circle_at_top,#f2dfc2_0%,#f5f2ea_35%,#e8eef6_100%)] px-4 py-8">
      <div className="grid w-full max-w-5xl gap-6 lg:grid-cols-[1.1fr_0.9fr]">
        <section className="rounded-[2rem] border border-white/70 bg-[#1f2937] p-6 text-white shadow-[0_30px_90px_rgba(15,23,42,0.16)]">
          <p className="text-xs font-semibold uppercase tracking-[0.24em] text-amber-300">Me Ajuda Ai</p>
          <h1 className="mt-4 text-3xl font-semibold leading-tight sm:text-4xl">
            Painel administrativo para fila, alertas e retries.
          </h1>
          <p className="mt-4 max-w-lg text-sm leading-7 text-slate-300">
            Este acesso usa o mesmo backend validado no ambiente de observabilidade. O objetivo aqui e dar leitura operacional rapida em celular e desktop.
          </p>
          <div className="mt-8 rounded-[1.5rem] border border-white/10 bg-white/5 p-4 text-sm text-slate-300">
            <p className="font-semibold text-white">Credenciais padrao</p>
            <p className="mt-2">Email: admin@meajudaai.local</p>
            <p>Senha: Admin@123</p>
          </div>
        </section>

        <section className="rounded-[2rem] border border-white/70 bg-white/85 p-6 shadow-[0_30px_90px_rgba(15,23,42,0.08)] backdrop-blur">
          <p className="text-xs font-semibold uppercase tracking-[0.2em] text-slate-500">Entrar</p>
          <h2 className="mt-2 text-2xl font-semibold text-slate-900">Autenticacao do admin</h2>
          {nextPath !== '/' ? (
            <p className="mt-2 text-sm leading-6 text-slate-500">
              Entre para continuar no fluxo solicitado.
            </p>
          ) : null}

          <form className="mt-6 space-y-4" onSubmit={handleSubmit}>
            <label className="block">
              <span className="mb-2 block text-sm font-medium text-slate-700">Email</span>
              <input
                className="w-full rounded-2xl border border-slate-200 bg-white px-4 py-3 text-slate-900 outline-none transition focus:border-slate-900"
                type="email"
                value={email}
                onChange={(event) => setEmail(event.target.value)}
                autoComplete="email"
              />
            </label>

            <label className="block">
              <span className="mb-2 block text-sm font-medium text-slate-700">Senha</span>
              <input
                className="w-full rounded-2xl border border-slate-200 bg-white px-4 py-3 text-slate-900 outline-none transition focus:border-slate-900"
                type="password"
                value={senha}
                onChange={(event) => setSenha(event.target.value)}
                autoComplete="current-password"
              />
            </label>

            {erro ? (
              <div className="rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
                {erro}
              </div>
            ) : null}

            <button
              className="w-full rounded-2xl bg-slate-900 px-4 py-3 text-sm font-semibold text-white transition hover:bg-slate-800 disabled:cursor-not-allowed disabled:opacity-60"
              type="submit"
              disabled={isPending}
            >
              {isPending ? 'Entrando...' : 'Entrar no painel'}
            </button>
          </form>
        </section>
      </div>
    </main>
  );
}
