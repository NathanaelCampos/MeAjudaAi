'use client';

import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { AuthProvider, useAuth } from './auth-provider';
import type { AuthSession } from '@/lib/auth-storage';

const readAuthSession = vi.fn();
const writeAuthSession = vi.fn();
const clearAuthSession = vi.fn();

vi.mock('@/lib/auth-storage', () => ({
  readAuthSession: () => readAuthSession(),
  writeAuthSession: (session: AuthSession) => writeAuthSession(session),
  clearAuthSession: () => clearAuthSession(),
}));

function Consumer() {
  const { session, hydrated, isAuthenticated, login, logout } = useAuth();

  return (
    <div>
      <span data-testid="hydrated">{hydrated ? 'sim' : 'nao'}</span>
      <span data-testid="authenticated">{isAuthenticated ? 'sim' : 'nao'}</span>
      <span data-testid="role">{session?.role ?? 'sem-role'}</span>
      <span data-testid="nome">{session?.nome ?? 'sem-nome'}</span>
      <button
        type="button"
        onClick={() =>
          login({
            email: 'cliente@teste.com',
            senha: '123456',
          })
        }
      >
        Entrar
      </button>
      <button type="button" onClick={logout}>
        Sair
      </button>
    </div>
  );
}

function createTokenWithRole(role: string) {
  const header = btoa(JSON.stringify({ alg: 'HS256', typ: 'JWT' }));
  const payload = btoa(JSON.stringify({ role }));
  return `${header}.${payload}.signature`;
}

describe('AuthProvider', () => {
  beforeEach(() => {
    readAuthSession.mockReset();
    writeAuthSession.mockReset();
    clearAuthSession.mockReset();
    vi.restoreAllMocks();
  });

  it('hidrata a sessão do storage no carregamento', async () => {
    readAuthSession.mockReturnValue({
      usuarioId: 'u1',
      nome: 'Cliente já logado',
      email: 'cliente@teste.com',
      token: 'token',
      expiraEmUtc: '2026-04-06T12:00:00Z',
      role: 'Cliente',
    } satisfies AuthSession);

    render(
      <AuthProvider>
        <Consumer />
      </AuthProvider>,
    );

    await waitFor(() => expect(screen.getByTestId('hydrated')).toHaveTextContent('sim'));
    expect(screen.getByTestId('authenticated')).toHaveTextContent('sim');
    expect(screen.getByTestId('nome')).toHaveTextContent('Cliente já logado');
    expect(screen.getByTestId('role')).toHaveTextContent('Cliente');
  });

  it('faz login, decodifica o papel e limpa a sessão no logout', async () => {
    const user = userEvent.setup();
    readAuthSession.mockReturnValue(null);

    vi.spyOn(global, 'fetch').mockResolvedValue(
      new Response(
        JSON.stringify({
          usuarioId: 'u2',
          nome: 'Profissional Teste',
          email: 'pro@teste.com',
          token: createTokenWithRole('Profissional'),
          expiraEmUtc: '2026-04-06T12:00:00Z',
        }),
        {
          status: 200,
          headers: {
            'Content-Type': 'application/json',
          },
        },
      ),
    );

    render(
      <AuthProvider>
        <Consumer />
      </AuthProvider>,
    );

    await user.click(screen.getByRole('button', { name: 'Entrar' }));

    await waitFor(() =>
      expect(writeAuthSession).toHaveBeenCalledWith(
        expect.objectContaining({
          nome: 'Profissional Teste',
          role: 'Profissional',
        }),
      ),
    );
    await waitFor(() => expect(screen.getByTestId('authenticated')).toHaveTextContent('sim'));
    expect(screen.getByTestId('role')).toHaveTextContent('Profissional');

    await user.click(screen.getByRole('button', { name: 'Sair' }));
    expect(clearAuthSession).toHaveBeenCalled();
    await waitFor(() => expect(screen.getByTestId('authenticated')).toHaveTextContent('nao'));
  });
});
