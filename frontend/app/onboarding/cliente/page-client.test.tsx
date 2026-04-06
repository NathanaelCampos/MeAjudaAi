'use client';

import { render, screen } from '@testing-library/react';
import { ClientOnboardingPageClient } from './page-client';

const replace = vi.fn();
const authState = {
  hydrated: true,
  session: {
    token: 'token',
    nome: 'Clara Cliente',
    email: 'clara@teste.com',
    role: 'Cliente',
  } as null | {
    token?: string;
    nome?: string;
    email?: string;
    role?: string;
  },
};

vi.mock('next/navigation', () => ({
  useRouter: () => ({
    replace,
  }),
}));

vi.mock('@/providers/auth-provider', () => ({
  useAuth: () => authState,
}));

vi.mock('@/components/NotificationPreferencesForm', () => ({
  NotificationPreferencesForm: (props: { submitLabel: string }) => (
    <div>{props.submitLabel}</div>
  ),
}));

describe('ClientOnboardingPageClient', () => {
  beforeEach(() => {
    replace.mockReset();
    authState.hydrated = true;
    authState.session = {
      token: 'token',
      nome: 'Clara Cliente',
      email: 'clara@teste.com',
      role: 'Cliente',
    };
  });

  it('renderiza o resumo da conta do cliente autenticado', () => {
    render(<ClientOnboardingPageClient />);

    expect(screen.getByText('Clara Cliente')).toBeInTheDocument();
    expect(screen.getByText('clara@teste.com')).toBeInTheDocument();
    expect(screen.getByText('Salvar e começar a explorar')).toBeInTheDocument();
  });

  it('redireciona para login quando não há sessão', () => {
    authState.session = null;

    render(<ClientOnboardingPageClient />);

    expect(replace).toHaveBeenCalledWith('/login?next=%2Fonboarding%2Fcliente');
  });

  it('redireciona profissional para o onboarding correto', () => {
    authState.session = {
      token: 'token',
      nome: 'Paulo Profissional',
      email: 'paulo@teste.com',
      role: 'Profissional',
    };

    render(<ClientOnboardingPageClient />);

    expect(replace).toHaveBeenCalledWith('/onboarding/profissional');
  });
});
