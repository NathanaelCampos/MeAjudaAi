'use client';

import {
  createContext,
  ReactNode,
  useCallback,
  useContext,
  useMemo,
  useRef,
  useState,
} from 'react';

type ToastVariant = 'success' | 'error' | 'info';

interface ToastItem {
  id: string;
  title: string;
  message?: string;
  variant: ToastVariant;
}

interface ShowToastInput {
  title: string;
  message?: string;
  variant?: ToastVariant;
}

interface ToastContextValue {
  showToast: (input: ShowToastInput) => void;
  dismissToast: (id: string) => void;
}

const ToastContext = createContext<ToastContextValue | null>(null);

function toneClasses(variant: ToastVariant) {
  switch (variant) {
    case 'success':
      return 'border-emerald-200 bg-emerald-50 text-emerald-900';
    case 'error':
      return 'border-rose-200 bg-rose-50 text-rose-900';
    default:
      return 'border-sky-200 bg-sky-50 text-sky-900';
  }
}

export function ToastProvider({ children }: { children: ReactNode }) {
  const [toasts, setToasts] = useState<ToastItem[]>([]);
  const timersRef = useRef<Map<string, number>>(new Map());

  const dismissToast = useCallback((id: string) => {
    const timer = timersRef.current.get(id);
    if (timer) {
      window.clearTimeout(timer);
      timersRef.current.delete(id);
    }

    setToasts((current) => current.filter((toast) => toast.id !== id));
  }, []);

  const showToast = useCallback(
    ({ title, message, variant = 'info' }: ShowToastInput) => {
      const id = `${Date.now()}-${Math.random().toString(36).slice(2, 8)}`;

      setToasts((current) => [...current, { id, title, message, variant }]);

      const timer = window.setTimeout(() => {
        dismissToast(id);
      }, 4200);

      timersRef.current.set(id, timer);
    },
    [dismissToast],
  );

  const value = useMemo<ToastContextValue>(
    () => ({
      showToast,
      dismissToast,
    }),
    [dismissToast, showToast],
  );

  return (
    <ToastContext.Provider value={value}>
      {children}

      <div className="pointer-events-none fixed inset-x-0 bottom-4 z-50 flex justify-center px-4">
        <div className="flex w-full max-w-md flex-col gap-3">
          {toasts.map((toast) => (
            <div
              key={toast.id}
              className={`pointer-events-auto rounded-[1.5rem] border px-4 py-3 shadow-[0_18px_40px_rgba(15,23,42,0.12)] backdrop-blur ${toneClasses(
                toast.variant,
              )}`}
            >
              <div className="flex items-start justify-between gap-3">
                <div>
                  <p className="text-sm font-semibold">{toast.title}</p>
                  {toast.message ? <p className="mt-1 text-sm leading-6 opacity-80">{toast.message}</p> : null}
                </div>
                <button
                  type="button"
                  onClick={() => dismissToast(toast.id)}
                  className="rounded-full px-2 py-1 text-xs font-semibold uppercase tracking-wide opacity-70 transition hover:opacity-100"
                >
                  Fechar
                </button>
              </div>
            </div>
          ))}
        </div>
      </div>
    </ToastContext.Provider>
  );
}

export function useToast() {
  const context = useContext(ToastContext);

  if (!context) {
    throw new Error('useToast precisa ser usado dentro de ToastProvider');
  }

  return context;
}
