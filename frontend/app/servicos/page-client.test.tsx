'use client';

import { fireEvent, render, screen } from '@testing-library/react';
import { MyServicesPageClient } from './page-client';

const replace = vi.fn();
const searchParams = new URLSearchParams('status=Aceito');

vi.mock('next/navigation', () => ({
  useRouter: () => ({
    replace,
  }),
  useSearchParams: () => searchParams,
}));

vi.mock('@/hooks/useMyClientServices', () => ({
  useMyClientServices: () => ({
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
        status: 'Aceito',
        dataCriacao: '2026-04-06T12:00:00Z',
        dataAceite: null,
        dataInicio: null,
        dataConclusao: null,
        dataCancelamento: null,
      },
    ],
  }),
}));

vi.mock('@/components/ServiceRequestCard', () => ({
  ServiceRequestCard: ({ item }: { item: { titulo: string } }) => <div>{item.titulo}</div>,
}));

describe('MyServicesPageClient', () => {
  beforeEach(() => {
    replace.mockReset();
  });

  it('renderiza a lista do cliente e aplica filtro por status', () => {
    render(<MyServicesPageClient />);

    expect(screen.getByText('Meus serviços')).toBeInTheDocument();
    expect(screen.getByText('Pintura da sala')).toBeInTheDocument();

    fireEvent.change(screen.getByRole('combobox'), { target: { value: 'Concluido' } });
    fireEvent.click(screen.getByRole('button', { name: 'Filtrar' }));

    expect(replace).toHaveBeenCalledWith('/servicos?status=Concluido');
  });
});
