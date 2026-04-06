'use client';

import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { ReviewForm } from './ReviewForm';

const createReview = vi.fn();
const showToast = vi.fn();

vi.mock('@/hooks/useAvaliacoes', () => ({
  useCreateReview: () => ({
    createReview,
    isPending: false,
  }),
}));

vi.mock('@/providers/toast-provider', () => ({
  useToast: () => ({
    showToast,
  }),
}));

describe('ReviewForm', () => {
  beforeEach(() => {
    createReview.mockReset();
    showToast.mockReset();
  });

  it('envia avaliação do serviço e mostra feedback de sucesso', async () => {
    createReview.mockResolvedValueOnce(undefined);

    render(<ReviewForm servicoId="svc-1" profissionalNome="Ana Reformas" />);

    const selects = screen.getAllByRole('combobox');

    fireEvent.change(selects[0], { target: { value: '4' } });
    fireEvent.change(selects[1], { target: { value: '5' } });
    fireEvent.change(selects[2], { target: { value: '3' } });
    fireEvent.change(
      screen.getByPlaceholderText(
        'O que foi bom? O que poderia ter sido melhor? Seu comentário ajuda outros clientes.',
      ),
      {
        target: { value: 'Chegou no horário e entregou um acabamento muito bom.' },
      },
    );

    fireEvent.click(screen.getByRole('button', { name: 'Enviar avaliação' }));

    await waitFor(() =>
      expect(createReview).toHaveBeenCalledWith({
        servicoId: 'svc-1',
        notaAtendimento: 4,
        notaServico: 5,
        notaPreco: 3,
        comentario: 'Chegou no horário e entregou um acabamento muito bom.',
      }),
    );
    expect(showToast).toHaveBeenCalledWith({
      title: 'Avaliação enviada',
      message:
        'Sua avaliação sobre Ana Reformas foi registrada e seguirá para moderação do comentário.',
      variant: 'success',
    });
  });
});
