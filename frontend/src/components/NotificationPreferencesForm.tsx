'use client';

import { useEffect, useMemo, useState } from 'react';
import { ApiError } from '@/lib/api';
import {
  useNotificationPreferences,
  useNotificationPreferencesActions,
} from '@/hooks/useNotificationPreferences';
import { PreferenciaNotificacaoResponse, TipoNotificacao } from '@/types/api';
import { useToast } from '@/providers/toast-provider';

function normalizeTipo(tipo: TipoNotificacao | string): TipoNotificacao | string {
  if (typeof tipo === 'number') {
    return tipo;
  }

  if (/^\d+$/.test(tipo)) {
    return Number(tipo) as TipoNotificacao;
  }

  return tipo;
}

function notificationLabel(tipo: TipoNotificacao | string) {
  switch (normalizeTipo(tipo)) {
    case 1:
    case 'ServicoSolicitado':
      return {
        title: 'Serviço solicitado',
        description: 'Avisos quando uma nova solicitação entra no seu fluxo.',
      };
    case 2:
    case 'ServicoAceito':
      return {
        title: 'Serviço aceito',
        description: 'Atualizações quando o profissional aceita o serviço.',
      };
    case 3:
    case 'ServicoConcluido':
      return {
        title: 'Serviço concluído',
        description: 'Confirmações e encerramento do serviço.',
      };
    case 4:
    case 'AvaliacaoAprovada':
      return {
        title: 'Avaliação aprovada',
        description: 'Confirmações relacionadas às avaliações do perfil.',
      };
    case 5:
    case 'ImpulsionamentoAtivado':
      return {
        title: 'Impulsionamento ativado',
        description: 'Avisos sobre impulsionamentos e campanhas pagas.',
      };
    case 6:
    case 'AlertaFila':
      return {
        title: 'Alertas operacionais',
        description: 'Alertas internos do sistema e status críticos.',
      };
    default:
      return {
        title: String(tipo),
        description: 'Preferência de notificação configurável.',
      };
  }
}

interface Props {
  title: string;
  description: string;
  submitLabel: string;
  successTitle: string;
  successMessage: string;
  emptyStateLabel?: string;
  onSaved?: () => void;
}

export function NotificationPreferencesForm({
  title,
  description,
  submitLabel,
  successTitle,
  successMessage,
  emptyStateLabel = 'Nenhuma preferência de notificação foi carregada.',
  onSaved,
}: Props) {
  const { data, error, isLoading } = useNotificationPreferences();
  const { isPending, savePreferences } = useNotificationPreferencesActions();
  const { showToast } = useToast();
  const [draft, setDraft] = useState<PreferenciaNotificacaoResponse[]>([]);

  useEffect(() => {
    if (data) {
      setDraft(data);
    }
  }, [data]);

  const hasItems = useMemo(() => draft.length > 0, [draft.length]);

  function updateItem(
    tipo: TipoNotificacao | string,
    patch: Partial<Pick<PreferenciaNotificacaoResponse, 'ativoEmail' | 'ativoInterno'>>,
  ) {
    setDraft((current) =>
      current.map((item) => (item.tipo === tipo ? { ...item, ...patch } : item)),
    );
  }

  async function handleSubmit() {
    try {
      await savePreferences({
        preferencias: draft.map((item) => ({
          tipo: item.tipo,
          ativoInterno: item.ativoInterno,
          ativoEmail: item.ativoEmail,
        })),
      });

      showToast({
        title: successTitle,
        message: successMessage,
        variant: 'success',
      });

      onSaved?.();
    } catch (error) {
      const message =
        error instanceof ApiError
          ? error.message
          : 'Não foi possível salvar as preferências de notificação.';

      showToast({
        title: 'Falha ao salvar preferências',
        message,
        variant: 'error',
      });
    }
  }

  return (
    <section className="rounded-[2rem] border border-white/80 bg-white/90 p-6 shadow-[0_24px_60px_rgba(15,23,42,0.08)]">
      <div>
        <p className="text-sm font-semibold text-slate-900">{title}</p>
        <p className="mt-1 text-sm text-slate-500">{description}</p>
      </div>

      {isLoading ? (
        <div className="mt-4 rounded-[1.4rem] border border-slate-200 bg-slate-50 px-4 py-4 text-sm text-slate-500">
          Carregando preferências...
        </div>
      ) : null}

      {error ? (
        <div className="mt-4 rounded-[1.4rem] border border-rose-200 bg-rose-50 px-4 py-4 text-sm text-rose-700">
          Não foi possível carregar suas preferências agora.
        </div>
      ) : null}

      {!isLoading && !error && !hasItems ? (
        <div className="mt-4 rounded-[1.4rem] border border-slate-200 bg-slate-50 px-4 py-4 text-sm text-slate-500">
          {emptyStateLabel}
        </div>
      ) : null}

      {hasItems ? (
        <div className="mt-4 space-y-3">
          {draft.map((item) => {
            const copy = notificationLabel(item.tipo);

            return (
              <div
                key={String(item.tipo)}
                className="rounded-[1.4rem] border border-slate-200 bg-slate-50 p-4"
              >
                <div className="flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
                  <div className="min-w-0">
                    <p className="text-sm font-semibold text-slate-900">{copy.title}</p>
                    <p className="mt-1 text-sm text-slate-500">{copy.description}</p>
                  </div>

                  <div className="grid gap-2 sm:grid-cols-2">
                    <label className="flex items-center gap-2 rounded-full border border-slate-200 bg-white px-3 py-2 text-sm text-slate-700">
                      <input
                        type="checkbox"
                        checked={item.ativoInterno}
                        onChange={(event) =>
                          updateItem(item.tipo, { ativoInterno: event.target.checked })
                        }
                      />
                      Notificação no app
                    </label>

                    <label className="flex items-center gap-2 rounded-full border border-slate-200 bg-white px-3 py-2 text-sm text-slate-700">
                      <input
                        type="checkbox"
                        checked={item.ativoEmail}
                        onChange={(event) =>
                          updateItem(item.tipo, { ativoEmail: event.target.checked })
                        }
                      />
                      E-mail
                    </label>
                  </div>
                </div>
              </div>
            );
          })}
        </div>
      ) : null}

      <button
        type="button"
        disabled={!hasItems || isPending}
        onClick={handleSubmit}
        className="mt-6 w-full rounded-2xl bg-slate-900 px-4 py-3 text-sm font-semibold text-white transition hover:bg-slate-800 disabled:cursor-not-allowed disabled:opacity-50"
      >
        {isPending ? 'Salvando preferências...' : submitLabel}
      </button>
    </section>
  );
}
