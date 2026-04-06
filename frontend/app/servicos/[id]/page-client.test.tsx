'use client';

import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { ServiceDetailPageClient } from './page-client';

const showToast = vi.fn();
const acceptService = vi.fn();
const startService = vi.fn();
const concludeService = vi.fn();
const cancelService = vi.fn();

const authState = {
  session: {
    usuarioId: 'profissional-1',
  },
};

vi.mock('@/providers/auth-provider', () => ({
  useAuth: () => authState,
}));

vi.mock('@/providers/toast-provider', () => ({
  useToast: () => ({
    showToast,
  }),
}));

vi.mock('@/hooks/useServiceDetails', () => ({
  useServiceDetails: () => ({
    isLoading: false,
    error: null,
    data: {
      id: 'svc-1',
      clienteId: 'cliente-1',
      profissionalId: 'profissional-1',
      nomeCliente: 'Carlos',
      nomeProfissional: 'Ana Reformas',
      profissaoId: 'p1',
      nomeProfissao: 'Pintura',
      especialidadeId: null,
      nomeEspecialidade: null,
      cidadeId: 'c1',
      cidadeNome: 'Recife',
      uf: 'PE',
      bairroId: 'b1',
      bairroNome: 'Boa Viagem',
      titulo: 'Pintura da sala',
      descricao: 'Aplicação de tinta e acabamento.',
      valorCombinado: 500,
      status: 'Solicitado',
      dataCriacao: '2026-04-06T12:00:00Z',
      dataAceite: null,
      dataInicio: null,
      dataConclusao: null,
      dataCancelamento: null,
    },
  }),
}));

vi.mock('@/hooks/useServiceActions', () => ({
  useServiceActions: () => ({
    busyAction: null,
    acceptService,
    startService,
    concludeService,
    cancelService,
  }),
}));

describe('ServiceDetailPageClient', () => {
  beforeEach(() => {
    showToast.mockReset();
    acceptService.mockReset();
    startService.mockReset();
    concludeService.mockReset();
    cancelService.mockReset();
    authState.session = {
      usuarioId: 'profissional-1',
    };
  });

  it('mostra ação contextual do profissional e executa aceite com feedback', async () => {
    acceptService.mockResolvedValueOnce(undefined);

    render(<ServiceDetailPageClient id="svc-1" />);

    expect(screen.getAllByText('Pintura da sala').length).toBeGreaterThan(0);
    expect(screen.getByRole('button', { name: 'Aceitar' })).toBeInTheDocument();

    fireEvent.click(screen.getByRole('button', { name: 'Aceitar' }));

    await waitFor(() => expect(acceptService).toHaveBeenCalledWith('svc-1'));
    expect(showToast).toHaveBeenCalledWith({
      title: 'Serviço aceito',
      message: 'Os dados do serviço foram atualizados.',
      variant: 'success',
    });
  });

  it('permite cancelamento para o cliente autenticado', () => {
    authState.session = {
      usuarioId: 'cliente-1',
    };

    render(<ServiceDetailPageClient id="svc-1" />);

    expect(screen.getByRole('button', { name: 'Cancelar' })).toBeInTheDocument();
  });
});
