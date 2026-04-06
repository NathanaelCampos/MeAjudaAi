'use client';

import { fireEvent, render, screen } from '@testing-library/react';
import { ExplorePageClient } from './page-client';

const replace = vi.fn();

const params = new URLSearchParams('nome=ana&profissaoId=p1&cidadeId=c1&ordenacao=4&pagina=2&tamanhoPagina=18');

vi.mock('next/navigation', () => ({
  useRouter: () => ({
    replace,
  }),
  useSearchParams: () => params,
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
    data: [
      { id: 'c1', estadoId: 'e1', nome: 'Recife', uf: 'PE', codigoIbge: '1' },
      { id: 'c2', estadoId: 'e2', nome: 'Olinda', uf: 'PE', codigoIbge: '2' },
    ],
  }),
}));

vi.mock('@/hooks/useProfessionalSearch', () => ({
  useProfessionalSearch: () => ({
    isLoading: false,
    error: null,
    data: {
      paginaAtual: 2,
      tamanhoPagina: 18,
      totalRegistros: 32,
      totalPaginas: 3,
      itens: [
        {
          id: 'prof-1',
          usuarioId: 'u1',
          nomeExibicao: 'Ana Reformas',
          descricao: 'Especialista em pintura residencial.',
          aceitaContatoPeloApp: true,
          perfilVerificado: true,
          estaImpulsionado: false,
          notaMediaAtendimento: 4.9,
          notaMediaServico: 4.8,
          notaMediaPreco: 4.5,
          profissoes: [{ id: 'p1', nome: 'Pintora' }],
          especialidades: [],
          areasAtendimento: [
            {
              cidadeId: 'c1',
              cidadeNome: 'Recife',
              uf: 'PE',
              bairroId: null,
              bairroNome: null,
              cidadeInteira: true,
            },
          ],
        },
      ],
    },
  }),
}));

vi.mock('@/components/ProfessionalCard', () => ({
  ProfessionalCard: ({ item }: { item: { nomeExibicao: string } }) => (
    <div>{item.nomeExibicao}</div>
  ),
}));

describe('ExplorePageClient', () => {
  beforeEach(() => {
    replace.mockReset();
  });

  it('renderiza resumo da paginação e muda de página', () => {
    render(<ExplorePageClient />);

    expect(screen.getByText('32 profissionais encontrados')).toBeInTheDocument();
    expect(screen.getByText('Mostrando 19-32')).toBeInTheDocument();
    expect(screen.getByText('Ana Reformas')).toBeInTheDocument();

    fireEvent.click(screen.getByRole('button', { name: 'Próxima' }));

    expect(replace).toHaveBeenCalledWith(
      '/explorar?nome=ana&profissaoId=p1&cidadeId=c1&ordenacao=4&pagina=3&tamanhoPagina=18',
    );
  });

  it('limpa os filtros e volta para a rota base', () => {
    render(<ExplorePageClient />);

    fireEvent.click(screen.getByRole('button', { name: 'Limpar filtros' }));

    expect(replace).toHaveBeenCalledWith('/explorar');
  });
});
