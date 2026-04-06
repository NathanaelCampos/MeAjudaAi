import { render, screen } from '@testing-library/react';
import { ServiceRequestCard, formatServiceStatus } from './ServiceRequestCard';
import { ServicoResponse } from '@/types/api';

const service: ServicoResponse = {
  id: 'svc-1',
  clienteId: 'client-1',
  profissionalId: 'professional-1',
  nomeCliente: 'Carlos',
  nomeProfissional: 'Ana Reformas',
  profissaoId: 'p1',
  nomeProfissao: 'Pintura',
  especialidadeId: 'e1',
  nomeEspecialidade: 'Pintura interna',
  cidadeId: 'c1',
  cidadeNome: 'Recife',
  uf: 'PE',
  bairroId: 'b1',
  bairroNome: 'Boa Viagem',
  titulo: 'Pintura do quarto',
  descricao: 'Pintura completa com preparação de parede e acabamento.',
  valorCombinado: 850,
  status: 3,
  dataCriacao: '2026-04-06T12:00:00Z',
  dataAceite: null,
  dataInicio: null,
  dataConclusao: null,
  dataCancelamento: null,
};

describe('ServiceRequestCard', () => {
  it('renderiza informações principais e ação padrão', () => {
    render(
      <ServiceRequestCard
        item={service}
        actions={<button type="button">Aceitar agora</button>}
      />,
    );

    expect(screen.getByText('Pintura do quarto')).toBeInTheDocument();
    expect(screen.getByText('Em execução')).toBeInTheDocument();
    expect(screen.getByText('Pintura')).toBeInTheDocument();
    expect(screen.getByText('Recife - PE')).toBeInTheDocument();
    expect(screen.getByText(/R\$\s*850,00/)).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Ver detalhe' })).toHaveAttribute('href', '/servicos/svc-1');
    expect(screen.getByRole('button', { name: 'Aceitar agora' })).toBeInTheDocument();
  });

  it('normaliza o status para labels legíveis', () => {
    expect(formatServiceStatus('Solicitado')).toBe('Solicitado');
    expect(formatServiceStatus(2)).toBe('Aceito');
    expect(formatServiceStatus(4)).toBe('Concluído');
    expect(formatServiceStatus('Outro')).toBe('Outro');
  });
});
