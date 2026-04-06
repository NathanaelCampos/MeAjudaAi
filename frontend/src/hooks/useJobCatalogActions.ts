'use client';

import { useState, useTransition } from 'react';
import { useSWRConfig } from 'swr';
import { apiSend } from '@/lib/api';

function isAdminJobsKey(key: unknown) {
  return typeof key === 'string' && key.startsWith('/api/admin/jobs');
}

export function useJobCatalogActions() {
  const { mutate } = useSWRConfig();
  const [busyAction, setBusyAction] = useState<string | null>(null);
  const [feedback, setFeedback] = useState<string | null>(null);
  const [isPending, startTransition] = useTransition();

  async function refreshAdminJobs() {
    await mutate(isAdminJobsKey);
  }

  async function executeJob(jobId: string) {
    setBusyAction(`executar:${jobId}`);
    setFeedback(null);

    try {
      await apiSend(`/api/admin/jobs/${jobId}/executar`, { method: 'POST' });
      setFeedback(`Job ${jobId} executado manualmente.`);
      await refreshAdminJobs();
    } finally {
      startTransition(() => setBusyAction(null));
    }
  }

  async function enqueueJob(jobId: string) {
    setBusyAction(`enfileirar:${jobId}`);
    setFeedback(null);

    try {
      await apiSend(`/api/admin/jobs/${jobId}/enfileirar`, { method: 'POST' });
      setFeedback(`Job ${jobId} enviado para a fila.`);
      await refreshAdminJobs();
    } finally {
      startTransition(() => setBusyAction(null));
    }
  }

  async function cancelAll(jobId: string) {
    setBusyAction(`cancelar-todos:${jobId}`);
    setFeedback(null);

    try {
      await apiSend(`/api/admin/jobs/${jobId}/cancelar-todos`, { method: 'POST' });
      setFeedback(`Execucoes pendentes de ${jobId} canceladas.`);
      await refreshAdminJobs();
    } finally {
      startTransition(() => setBusyAction(null));
    }
  }

  async function scheduleJob(jobId: string, processarAposUtc: string) {
    setBusyAction(`agendar:${jobId}`);
    setFeedback(null);

    try {
      await apiSend(`/api/admin/jobs/${jobId}/agendar`, {
        method: 'POST',
        body: {
          processarAposUtc,
        },
      });
      setFeedback(`Job ${jobId} agendado para ${new Date(processarAposUtc).toLocaleString('pt-BR')}.`);
      await refreshAdminJobs();
    } finally {
      startTransition(() => setBusyAction(null));
    }
  }

  return {
    busyAction,
    feedback,
    isPending,
    executeJob,
    enqueueJob,
    cancelAll,
    scheduleJob,
    refreshAdminJobs,
    clearFeedback: () => setFeedback(null),
  };
}
