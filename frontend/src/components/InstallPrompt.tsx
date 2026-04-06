'use client';

import { useEffect, useState } from 'react';

interface BeforeInstallPromptEvent extends Event {
  prompt: () => Promise<void>;
  userChoice: Promise<{ outcome: 'accepted' | 'dismissed'; platform: string }>;
}

function isStandaloneMode() {
  if (typeof window === 'undefined') {
    return false;
  }

  return window.matchMedia('(display-mode: standalone)').matches;
}

export function InstallPrompt() {
  const [installEvent, setInstallEvent] = useState<BeforeInstallPromptEvent | null>(null);
  const [dismissed, setDismissed] = useState(false);
  const [isStandalone, setIsStandalone] = useState(false);
  const [isInstalling, setIsInstalling] = useState(false);

  useEffect(() => {
    setIsStandalone(isStandaloneMode());

    function handleBeforeInstallPrompt(event: Event) {
      event.preventDefault();
      setInstallEvent(event as BeforeInstallPromptEvent);
    }

    function handleAppInstalled() {
      setInstallEvent(null);
      setDismissed(true);
      setIsStandalone(true);
    }

    window.addEventListener('beforeinstallprompt', handleBeforeInstallPrompt);
    window.addEventListener('appinstalled', handleAppInstalled);

    return () => {
      window.removeEventListener('beforeinstallprompt', handleBeforeInstallPrompt);
      window.removeEventListener('appinstalled', handleAppInstalled);
    };
  }, []);

  async function handleInstall() {
    if (!installEvent) {
      return;
    }

    setIsInstalling(true);

    try {
      await installEvent.prompt();
      const choice = await installEvent.userChoice;

      if (choice.outcome === 'accepted') {
        setDismissed(true);
      }

      setInstallEvent(null);
    } finally {
      setIsInstalling(false);
    }
  }

  if (dismissed || isStandalone || !installEvent) {
    return null;
  }

  return (
    <div className="fixed inset-x-0 bottom-[calc(5.8rem+env(safe-area-inset-bottom))] z-40 px-4 md:bottom-6">
      <div className="mx-auto max-w-md rounded-[1.6rem] border border-amber-200 bg-[linear-gradient(135deg,#fff7ed_0%,#ffffff_100%)] p-4 shadow-[0_24px_60px_rgba(15,23,42,0.14)]">
        <div className="flex items-start gap-3">
          <div className="mt-1 h-2.5 w-2.5 shrink-0 rounded-full bg-amber-500 shadow-[0_0_0_4px_rgba(245,158,11,0.15)]" />
          <div className="min-w-0 flex-1">
            <p className="text-sm font-semibold text-slate-900">Instalar app</p>
            <p className="mt-1 text-sm leading-6 text-slate-600">
              Adicione o painel na tela inicial para abrir mais rápido e usar com aparência de app.
            </p>

            <div className="mt-3 flex flex-wrap gap-2">
              <button
                type="button"
                onClick={() => void handleInstall()}
                disabled={isInstalling}
                className="rounded-full bg-slate-900 px-4 py-2 text-sm font-semibold text-white transition hover:bg-slate-800 disabled:cursor-not-allowed disabled:opacity-50"
              >
                {isInstalling ? 'Abrindo...' : 'Instalar'}
              </button>
              <button
                type="button"
                onClick={() => setDismissed(true)}
                className="rounded-full border border-slate-200 bg-white px-4 py-2 text-sm font-medium text-slate-700 transition hover:bg-slate-50"
              >
                Agora nao
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
