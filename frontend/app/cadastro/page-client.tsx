'use client';

import Link from 'next/link';
import { FormEvent, useState } from 'react';
import { useRouter } from 'next/navigation';
import { useAuth } from '@/providers/auth-provider';

const profileOptions = [
  { value: 1, label: 'Cliente', description: 'Para buscar profissionais e solicitar serviços.' },
  { value: 2, label: 'Profissional', description: 'Para receber solicitações e gerenciar atendimentos.' },
];

export function RegisterPageClient() {
  const router = useRouter();
  const { register, isPending } = useAuth();
  const [nome, setNome] = useState('');
  const [email, setEmail] = useState('');
  const [telefone, setTelefone] = useState('');
  const [senha, setSenha] = useState('');
  const [tipoPerfil, setTipoPerfil] = useState<number>(1);
  const [erro, setErro] = useState<string | null>(null);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setErro(null);

    try {
      await register({
        nome: nome.trim(),
        email: email.trim(),
        telefone: telefone.trim(),
        senha,
        tipoPerfil,
      });

      if (tipoPerfil === 2) {
        router.replace('/onboarding/profissional');
        return;
      }

      router.replace('/onboarding/cliente');
    } catch (error) {
      setErro(error instanceof Error ? error.message : 'Falha ao criar a conta.');
    }
  }

  return (
    <main className="min-h-screen bg-[radial-gradient(circle_at_top,#f2dfc2_0%,#f5f2ea_35%,#e8eef6_100%)] px-4 py-8">
      <div className="mx-auto grid w-full max-w-5xl gap-6 lg:grid-cols-[1.1fr_0.9fr]">
        <section className="rounded-[2rem] border border-white/70 bg-[linear-gradient(135deg,#0f172a_0%,#1f2937_45%,#f59e0b_135%)] p-6 text-white shadow-[0_30px_90px_rgba(15,23,42,0.16)]">
          <p className="text-xs font-semibold uppercase tracking-[0.24em] text-amber-300">Criar conta</p>
          <h1 className="mt-4 text-3xl font-semibold leading-tight sm:text-4xl">
            Entre no Me Ajuda Ai do jeito certo para o seu perfil.
          </h1>
          <p className="mt-4 max-w-lg text-sm leading-7 text-slate-200">
            Clientes encontram profissionais e contratam. Profissionais montam o perfil, recebem solicitações e acompanham o trabalho pelo app.
          </p>

          <div className="mt-8 grid gap-3">
            {profileOptions.map((option) => (
              <div key={option.value} className="rounded-[1.4rem] border border-white/10 bg-white/5 p-4">
                <p className="text-sm font-semibold text-white">{option.label}</p>
                <p className="mt-1 text-sm text-slate-300">{option.description}</p>
              </div>
            ))}
          </div>
        </section>

        <section className="rounded-[2rem] border border-white/70 bg-white/85 p-6 shadow-[0_30px_90px_rgba(15,23,42,0.08)] backdrop-blur">
          <p className="text-xs font-semibold uppercase tracking-[0.2em] text-slate-500">Cadastro</p>
          <h2 className="mt-2 text-2xl font-semibold text-slate-900">Criar nova conta</h2>

          <form className="mt-6 space-y-4" onSubmit={handleSubmit}>
            <input
              className="w-full rounded-2xl border border-slate-200 bg-white px-4 py-3 text-slate-900 outline-none transition focus:border-slate-900"
              placeholder="Nome completo"
              value={nome}
              onChange={(event) => setNome(event.target.value)}
            />

            <input
              className="w-full rounded-2xl border border-slate-200 bg-white px-4 py-3 text-slate-900 outline-none transition focus:border-slate-900"
              placeholder="Email"
              type="email"
              value={email}
              onChange={(event) => setEmail(event.target.value)}
            />

            <input
              className="w-full rounded-2xl border border-slate-200 bg-white px-4 py-3 text-slate-900 outline-none transition focus:border-slate-900"
              placeholder="Telefone"
              value={telefone}
              onChange={(event) => setTelefone(event.target.value)}
            />

            <input
              className="w-full rounded-2xl border border-slate-200 bg-white px-4 py-3 text-slate-900 outline-none transition focus:border-slate-900"
              placeholder="Senha"
              type="password"
              value={senha}
              onChange={(event) => setSenha(event.target.value)}
            />

            <div className="grid gap-3 sm:grid-cols-2">
              {profileOptions.map((option) => (
                <button
                  key={option.value}
                  type="button"
                  onClick={() => setTipoPerfil(option.value)}
                  className={`rounded-[1.4rem] border px-4 py-4 text-left transition ${
                    tipoPerfil === option.value
                      ? 'border-slate-900 bg-slate-900 text-white'
                      : 'border-slate-200 bg-white text-slate-700 hover:bg-slate-50'
                  }`}
                >
                  <p className="text-sm font-semibold">{option.label}</p>
                  <p className="mt-1 text-sm opacity-80">{option.description}</p>
                </button>
              ))}
            </div>

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
              {isPending ? 'Criando conta...' : 'Criar conta'}
            </button>
          </form>

          <p className="mt-4 text-sm text-slate-500">
            Já tem conta?{' '}
            <Link href="/login" className="font-semibold text-slate-900 underline underline-offset-4">
              Entrar
            </Link>
          </p>
        </section>
      </div>
    </main>
  );
}
