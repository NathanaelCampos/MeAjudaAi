import { render, screen } from '@testing-library/react';
import { ProfessionalCard } from './ProfessionalCard';
import { ProfissionalResumoResponse } from '@/types/api';

const professional: ProfissionalResumoResponse = {
  id: 'prof-1',
  usuarioId: 'user-1',
  nomeExibicao: 'Ana Reformas',
  descricao: 'Profissional especializada em pequenos reparos e manutenção residencial.',
  aceitaContatoPeloApp: true,
  perfilVerificado: true,
  estaImpulsionado: true,
  notaMediaAtendimento: 4.8,
  notaMediaServico: 4.9,
  notaMediaPreco: 4.6,
  profissoes: [{ id: 'p1', nome: 'Marido de aluguel' }],
  especialidades: [{ id: 'e1', nome: 'Elétrica residencial' }],
  areasAtendimento: [
    {
      cidadeId: 'c1',
      cidadeNome: 'São Paulo',
      uf: 'SP',
      bairroId: null,
      bairroNome: null,
      cidadeInteira: true,
    },
  ],
};

describe('ProfessionalCard', () => {
  it('renderiza badges, notas e link para o perfil', () => {
    render(<ProfessionalCard item={professional} />);

    expect(screen.getByText('Ana Reformas')).toBeInTheDocument();
    expect(screen.getByText('Verificado')).toBeInTheDocument();
    expect(screen.getByText('Destaque')).toBeInTheDocument();
    expect(screen.getByText('Marido de aluguel')).toBeInTheDocument();
    expect(screen.getByText('Elétrica residencial')).toBeInTheDocument();
    expect(screen.getByText('São Paulo • SP')).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Ver perfil' })).toHaveAttribute(
      'href',
      '/profissionais/prof-1',
    );
  });
});
