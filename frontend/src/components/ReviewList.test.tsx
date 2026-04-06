'use client';

import { render, screen } from '@testing-library/react';
import { ReviewList } from './ReviewList';

describe('ReviewList', () => {
  it('renderiza estado vazio quando não há avaliações', () => {
    render(<ReviewList reviews={[]} />);

    expect(
      screen.getByText('Ainda não existem avaliações públicas para este profissional.'),
    ).toBeInTheDocument();
  });

  it('renderiza avaliações públicas com notas e comentário', () => {
    render(
      <ReviewList
        reviews={[
          {
            id: 'a1',
            clienteId: 'c1',
            profissionalId: 'p1',
            nomeCliente: 'Carla Cliente',
            notaAtendimento: 5,
            notaServico: 4,
            notaPreco: 3,
            comentario: 'Ótima experiência e acabamento muito bom.',
            statusModeracaoComentario: 2,
            dataCriacao: '2026-04-06T12:00:00Z',
          },
        ]}
      />,
    );

    expect(screen.getByText('Carla Cliente')).toBeInTheDocument();
    expect(screen.getByText('Atendimento 5/5')).toBeInTheDocument();
    expect(screen.getByText('Serviço 4/5')).toBeInTheDocument();
    expect(screen.getByText('Preço 3/5')).toBeInTheDocument();
    expect(screen.getByText('Ótima experiência e acabamento muito bom.')).toBeInTheDocument();
  });
});
