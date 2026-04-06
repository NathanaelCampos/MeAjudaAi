'use client';

import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { ProfessionalOnboardingPageClient } from './page-client';

const replace = vi.fn();
const showToast = vi.fn();
const apiSend = vi.fn();

const authState = {
  hydrated: true,
  session: {
    token: 'token',
    nome: 'Paula Profissional',
    role: 'Profissional',
  } as null | {
    token?: string;
    nome?: string;
    role?: string;
  },
};

vi.mock('next/navigation', () => ({
  useRouter: () => ({
    replace,
  }),
}));

vi.mock('@/lib/api', async () => {
  const actual = await vi.importActual<typeof import('@/lib/api')>('@/lib/api');

  return {
    ...actual,
    apiSend: (...args: unknown[]) => apiSend(...args),
  };
});

vi.mock('@/providers/auth-provider', () => ({
  useAuth: () => authState,
}));

vi.mock('@/providers/toast-provider', () => ({
  useToast: () => ({
    showToast,
  }),
}));

vi.mock('@/hooks/useProfissoes', () => ({
  useProfissoes: () => ({
    data: [
      { id: 'p1', nome: 'Pintora', slug: 'pintora' },
      { id: 'p2', nome: 'Eletricista', slug: 'eletricista' },
    ],
  }),
}));

vi.mock('@/hooks/useCidades', () => ({
  useCidades: () => ({
    data: [{ id: 'c1', nome: 'Recife', uf: 'PE', estadoId: 'e1', codigoIbge: '1' }],
  }),
}));

vi.mock('@/hooks/useEspecialidades', () => ({
  useEspecialidades: () => ({
    data: [{ id: 'e1', profissaoId: 'p1', nome: 'Pintura interna' }],
  }),
}));

vi.mock('@/hooks/useBairros', () => ({
  useBairros: () => ({
    data: [{ id: 'b1', cidadeId: 'c1', nome: 'Boa Viagem' }],
  }),
}));

describe('ProfessionalOnboardingPageClient', () => {
  beforeEach(() => {
    replace.mockReset();
    showToast.mockReset();
    apiSend.mockReset();
    authState.hydrated = true;
    authState.session = {
      token: 'token',
      nome: 'Paula Profissional',
      role: 'Profissional',
    };
  });

  it('redireciona para login quando a sessão não existe', () => {
    authState.session = null;

    render(<ProfessionalOnboardingPageClient />);

    expect(replace).toHaveBeenCalledWith('/login?next=%2Fonboarding%2Fprofissional');
  });

  it('salva o onboarding básico e redireciona para a área profissional', async () => {
    apiSend.mockResolvedValue(undefined);

    render(<ProfessionalOnboardingPageClient />);

    fireEvent.change(screen.getByPlaceholderText('Nome de exibição'), {
      target: { value: 'Paula Profissional' },
    });
    fireEvent.change(
      screen.getByPlaceholderText('Descreva sua experiência, especialidades e diferencial.'),
      {
        target: { value: 'Atuo com pintura residencial, acabamento fino e atendimento rápido em toda a cidade.' },
      },
    );

    fireEvent.click(screen.getByRole('button', { name: 'Pintora' }));
    fireEvent.click(screen.getByRole('button', { name: 'Pintura interna' }));

    const selects = screen.getAllByRole('combobox');
    fireEvent.change(selects[0], { target: { value: 'c1' } });

    fireEvent.click(screen.getByRole('button', { name: 'Concluir onboarding' }));

    await waitFor(() => expect(apiSend).toHaveBeenCalledTimes(6));
    expect(apiSend).toHaveBeenNthCalledWith(
      1,
      '/api/profissionais/me',
      expect.objectContaining({
        method: 'PUT',
      }),
    );
    expect(apiSend).toHaveBeenNthCalledWith(
      2,
      '/api/profissionais/me/profissoes',
      expect.objectContaining({
        method: 'PUT',
      }),
    );
    expect(apiSend).toHaveBeenNthCalledWith(
      3,
      '/api/profissionais/me/especialidades',
      expect.objectContaining({
        method: 'PUT',
      }),
    );

    await waitFor(() =>
      expect(showToast).toHaveBeenCalledWith({
        title: 'Onboarding concluído',
        message:
          'Seu perfil profissional está pronto para aparecer melhor na busca e receber solicitações com mais contexto.',
        variant: 'success',
      }),
    );
    expect(replace).toHaveBeenCalledWith('/servicos/profissional');
  });
});
