import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { NotificationPreferencesForm } from './NotificationPreferencesForm';

const showToast = vi.fn();
const savePreferences = vi.fn();
const mockedPreferences = [
  {
    tipo: 1,
    ativoInterno: true,
    ativoEmail: false,
  },
  {
    tipo: 2,
    ativoInterno: false,
    ativoEmail: true,
  },
];

vi.mock('@/hooks/useNotificationPreferences', () => ({
  useNotificationPreferences: () => ({
    data: mockedPreferences,
    error: null,
    isLoading: false,
  }),
  useNotificationPreferencesActions: () => ({
    isPending: false,
    savePreferences,
  }),
}));

vi.mock('@/providers/toast-provider', () => ({
  useToast: () => ({
    showToast,
  }),
}));

describe('NotificationPreferencesForm', () => {
  beforeEach(() => {
    showToast.mockReset();
    savePreferences.mockReset();
  });

  it('salva o estado atualizado das preferências e mostra feedback de sucesso', async () => {
    const onSaved = vi.fn();

    savePreferences.mockResolvedValueOnce([]);

    render(
      <NotificationPreferencesForm
        title="Preferências"
        description="Escolha como deseja ser avisado."
        submitLabel="Salvar"
        successTitle="Tudo certo"
        successMessage="Preferências salvas"
        onSaved={onSaved}
      />,
    );

    const internalCheckbox = screen.getAllByRole('checkbox', { name: /Notificação no app/i })[0];
    const emailCheckbox = screen.getAllByRole('checkbox', { name: /E-mail/i })[0];

    fireEvent.click(internalCheckbox);
    fireEvent.click(emailCheckbox);
    fireEvent.click(screen.getByRole('button', { name: 'Salvar' }));

    await waitFor(() =>
      expect(savePreferences).toHaveBeenCalledWith({
        preferencias: [
          {
            tipo: 1,
            ativoInterno: false,
            ativoEmail: true,
          },
          {
            tipo: 2,
            ativoInterno: false,
            ativoEmail: true,
          },
        ],
      }),
    );
    await waitFor(() =>
      expect(showToast).toHaveBeenCalledWith({
        title: 'Tudo certo',
        message: 'Preferências salvas',
        variant: 'success',
      }),
    );
    expect(onSaved).toHaveBeenCalled();
  });
});
