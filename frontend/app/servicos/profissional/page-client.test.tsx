'use client';

import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { ProfessionalServicesPageClient } from './page-client';

const replace = vi.fn();
const showToast = vi.fn();
const acceptService = vi.fn();
const startService = vi.fn();
const concludeService = vi.fn();
const cancelService = vi.fn();

const searchParams = new URLSearchParams('');

vi.mock('next/navigation', () => ({
  useRouter: () => ({
    replace,
  }),
  useSearchParams: () => searchParams,
}));

vi.mock('@/providers/toast-provider', () => ({
  useToast: () => ({
    showToast,
  }),
}));

vi.mock('@/hooks/useMyProfessionalServices', () => ({
  useMyProfessionalServices: () => ({
    isLoading: false,
    error: null,
    data: [
      {
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
        bairroId: null,
        bairroNome: null,
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
    ],
  }),
}));

vi.mock('@/hooks/useProfessionalServiceActions', () => ({
  useProfessionalServiceActions: () => ({
    busyAction: null,
    acceptService,
    startService,
    concludeService,
    cancelService,
  }),
}));

vi.mock('@/components/ServiceRequestCard', () => ({
  ServiceRequestCard: ({
    item,
    actions,
  }: {
    item: { titulo: string };
    actions?: React.ReactNode;
  }) => (
    <div>
      <span>{item.titulo}</span>
      <div>{actions}</div>
    </div>
  ),
}));

describe('ProfessionalServicesPageClient', () => {
  beforeEach(() => {
    replace.mockReset();
    showToast.mockReset();
    acceptService.mockReset();
    startService.mockReset();
    concludeService.mockReset();
    cancelService.mockReset();
  });

  it('aceita um serviço e mostra feedback de sucesso', async () => {
    acceptService.mockResolvedValueOnce(undefined);

    render(<ProfessionalServicesPageClient />);

    fireEvent.click(screen.getByRole('button', { name: 'Aceitar' }));

    await waitFor(() => expect(acceptService).toHaveBeenCalledWith('svc-1'));
    expect(showToast).toHaveBeenCalledWith({
      title: 'Solicitação aceita',
      message: 'A lista foi atualizada com o estado mais recente.',
      variant: 'success',
    });
  });
});
