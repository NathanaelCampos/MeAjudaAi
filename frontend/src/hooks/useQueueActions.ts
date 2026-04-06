'use client';

import { useState, useTransition } from 'react';
import { useSWRConfig } from 'swr';
import { apiSend } from '@/lib/api';
import {
  BackgroundJobFilaItemResponse,
  ProcessarFilaBackgroundJobAdminResponse,
} from '@/types/api';

function isQueueKey(key: unknown) {
  return typeof key === 'string' && key.startsWith('/api/admin/jobs/fila');
}

export function useQueueActions() {
  const { mutate } = useSWRConfig();
  const [feedback, setFeedback] = useState<string | null>(null);
  const [busyAction, setBusyAction] = useState<string | null>(null);
  const [isPending, startTransition] = useTransition();

  async function refreshQueueState() {
    await mutate(isQueueKey);
  }

  async function processQueue() {
    setBusyAction('processar-fila');
    setFeedback(null);

    try {
      const response = await apiSend<ProcessarFilaBackgroundJobAdminResponse>('/api/admin/jobs/fila/processar', {
        method: 'POST',
      });

      setFeedback(
        response.execucoesProcessadas > 0
          ? `${response.execucoesProcessadas} execucao(oes) processada(s).`
          : 'Fila processada sem execucoes pendentes no momento.',
      );

      await refreshQueueState();
    } finally {
      startTransition(() => setBusyAction(null));
    }
  }

  async function cancelExecution(execucaoId: string) {
    setBusyAction(`cancelar:${execucaoId}`);
    setFeedback(null);

    try {
      const response = await apiSend<BackgroundJobFilaItemResponse>(`/api/admin/jobs/fila/${execucaoId}/cancelar`, {
        method: 'PUT',
      });

      setFeedback(`Execucao ${response.jobId} cancelada.`);
      await refreshQueueState();
    } finally {
      startTransition(() => setBusyAction(null));
    }
  }

  async function reopenExecution(execucaoId: string) {
    setBusyAction(`reabrir:${execucaoId}`);
    setFeedback(null);

    try {
      const response = await apiSend<BackgroundJobFilaItemResponse>(`/api/admin/jobs/fila/${execucaoId}/reabrir`, {
        method: 'PUT',
      });

      setFeedback(`Execucao ${response.jobId} reaberta.`);
      await refreshQueueState();
    } finally {
      startTransition(() => setBusyAction(null));
    }
  }

  return {
    busyAction,
    feedback,
    isPending,
    cancelExecution,
    processQueue,
    refreshQueueState,
    reopenExecution,
    clearFeedback: () => setFeedback(null),
  };
}
