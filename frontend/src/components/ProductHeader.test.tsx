'use client';

import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { ProductHeader } from './ProductHeader';

const replace = vi.fn();
const logout = vi.fn();

const navigationState = {
  pathname: '/explorar',
};

const authState = {
  isAuthenticated: false,
  session: null as null | { nome?: string },
};

vi.mock('next/navigation', () => ({
  usePathname: () => navigationState.pathname,
  useRouter: () => ({
    replace,
  }),
}));

vi.mock('@/providers/auth-provider', () => ({
  useAuth: () => ({
    isAuthenticated: authState.isAuthenticated,
    session: authState.session,
    logout,
  }),
}));

describe('ProductHeader', () => {
  beforeEach(() => {
    replace.mockReset();
    logout.mockReset();
    navigationState.pathname = '/explorar';
    authState.isAuthenticated = false;
    authState.session = null;
  });

  it('mostra navegação pública sem o item Conta quando não há sessão', () => {
    render(<ProductHeader />);

    expect(screen.getByRole('link', { name: 'Explorar' })).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Meus serviços' })).toBeInTheDocument();
    expect(screen.queryByRole('link', { name: 'Conta' })).not.toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Criar conta' })).toBeInTheDocument();
  });

  it('mostra ações autenticadas e executa logout com redirecionamento', async () => {
    const user = userEvent.setup();
    authState.isAuthenticated = true;
    authState.session = { nome: 'Marina Cliente' };
    navigationState.pathname = '/conta';

    render(<ProductHeader />);

    expect(screen.getByText('Marina Cliente')).toBeInTheDocument();
    expect(screen.getAllByRole('link', { name: 'Conta' }).length).toBeGreaterThan(0);

    await user.click(screen.getByRole('button', { name: 'Sair' }));

    expect(logout).toHaveBeenCalled();
    expect(replace).toHaveBeenCalledWith('/explorar');
  });
});
