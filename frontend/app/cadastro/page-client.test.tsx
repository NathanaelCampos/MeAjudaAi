'use client';

import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { RegisterPageClient } from './page-client';

const replace = vi.fn();
const register = vi.fn();

vi.mock('next/navigation', () => ({
  useRouter: () => ({
    replace,
  }),
}));

vi.mock('@/providers/auth-provider', () => ({
  useAuth: () => ({
    register,
    isPending: false,
  }),
}));

describe('RegisterPageClient', () => {
  beforeEach(() => {
    replace.mockReset();
    register.mockReset();
  });

  it('cadastra cliente e redireciona para o onboarding do cliente', async () => {
    register.mockResolvedValueOnce(undefined);

    render(<RegisterPageClient />);

    fireEvent.change(screen.getByPlaceholderText('Nome completo'), {
      target: { value: 'Carla Cliente' },
    });
    fireEvent.change(screen.getByPlaceholderText('Email'), {
      target: { value: 'carla@teste.com' },
    });
    fireEvent.change(screen.getByPlaceholderText('Telefone'), {
      target: { value: '81999999999' },
    });
    fireEvent.change(screen.getByPlaceholderText('Senha'), {
      target: { value: '123456' },
    });

    fireEvent.click(screen.getByRole('button', { name: 'Criar conta' }));

    await waitFor(() =>
      expect(register).toHaveBeenCalledWith({
        nome: 'Carla Cliente',
        email: 'carla@teste.com',
        telefone: '81999999999',
        senha: '123456',
        tipoPerfil: 1,
      }),
    );
    expect(replace).toHaveBeenCalledWith('/onboarding/cliente');
  });

  it('cadastra profissional e redireciona para o onboarding profissional', async () => {
    register.mockResolvedValueOnce(undefined);

    render(<RegisterPageClient />);

    fireEvent.click(screen.getByRole('button', { name: /Profissional/i }));

    fireEvent.change(screen.getByPlaceholderText('Nome completo'), {
      target: { value: 'Pedro Profissional' },
    });
    fireEvent.change(screen.getByPlaceholderText('Email'), {
      target: { value: 'pedro@teste.com' },
    });
    fireEvent.change(screen.getByPlaceholderText('Telefone'), {
      target: { value: '81988888888' },
    });
    fireEvent.change(screen.getByPlaceholderText('Senha'), {
      target: { value: 'abcdef' },
    });

    fireEvent.click(screen.getByRole('button', { name: 'Criar conta' }));

    await waitFor(() =>
      expect(register).toHaveBeenCalledWith(
        expect.objectContaining({
          tipoPerfil: 2,
        }),
      ),
    );
    expect(replace).toHaveBeenCalledWith('/onboarding/profissional');
  });
});
