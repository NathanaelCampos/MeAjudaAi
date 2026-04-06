'use client';

import {
  createContext,
  ReactNode,
  useContext,
  useEffect,
  useMemo,
  useState,
  useTransition,
} from 'react';
import { buildApiUrl } from '@/lib/api';
import {
  AuthSession,
  clearAuthSession,
  readAuthSession,
  UserRole,
  writeAuthSession,
} from '@/lib/auth-storage';

interface LoginPayload {
  email: string;
  senha: string;
}

interface RegisterPayload {
  nome: string;
  email: string;
  telefone: string;
  senha: string;
  tipoPerfil: number;
}

interface AuthContextValue {
  session: AuthSession | null;
  hydrated: boolean;
  isPending: boolean;
  isAuthenticated: boolean;
  login: (payload: LoginPayload) => Promise<void>;
  register: (payload: RegisterPayload) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextValue | null>(null);

function decodeRoleFromToken(token: string): UserRole | undefined {
  try {
    const payload = token.split('.')[1];

    if (!payload) {
      return undefined;
    }

    const normalized = payload.replace(/-/g, '+').replace(/_/g, '/');
    const json = JSON.parse(window.atob(normalized));
    const roleClaim =
      json.role ||
      json['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ||
      json['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/role'];

    if (roleClaim === 'Cliente' || roleClaim === 'Profissional' || roleClaim === 'Administrador') {
      return roleClaim;
    }

    return undefined;
  } catch {
    return undefined;
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [session, setSession] = useState<AuthSession | null>(null);
  const [hydrated, setHydrated] = useState(false);
  const [isPending, startTransition] = useTransition();

  useEffect(() => {
    const currentSession = readAuthSession();
    setSession(currentSession);
    setHydrated(true);
  }, []);

  async function login(payload: LoginPayload) {
    const response = await fetch(buildApiUrl('/api/auth/login'), {
      method: 'POST',
      headers: {
        Accept: 'application/json',
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(payload),
    });

    if (!response.ok) {
      let message = 'Nao foi possivel autenticar.';

      try {
        const body = (await response.json()) as { mensagem?: string };
        if (body?.mensagem) {
          message = body.mensagem;
        }
      } catch {
        // ignored
      }

      throw new Error(message);
    }

    const authResponse = (await response.json()) as AuthSession;
    const sessionWithRole: AuthSession = {
      ...authResponse,
      role: decodeRoleFromToken(authResponse.token),
    };

    startTransition(() => {
      writeAuthSession(sessionWithRole);
      setSession(sessionWithRole);
    });
  }

  async function register(payload: RegisterPayload) {
    const response = await fetch(buildApiUrl('/api/auth/registrar'), {
      method: 'POST',
      headers: {
        Accept: 'application/json',
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(payload),
    });

    if (!response.ok) {
      let message = 'Nao foi possivel criar a conta.';

      try {
        const body = (await response.json()) as { mensagem?: string; errors?: Record<string, string[]> };
        if (body?.mensagem) {
          message = body.mensagem;
        } else if (body?.errors) {
          const firstError = Object.values(body.errors).flat()[0];
          if (firstError) {
            message = firstError;
          }
        }
      } catch {
        // ignored
      }

      throw new Error(message);
    }

    const authResponse = (await response.json()) as AuthSession;
    const sessionWithRole: AuthSession = {
      ...authResponse,
      role: decodeRoleFromToken(authResponse.token) ?? (payload.tipoPerfil === 2 ? 'Profissional' : 'Cliente'),
    };

    startTransition(() => {
      writeAuthSession(sessionWithRole);
      setSession(sessionWithRole);
    });
  }

  function logout() {
    clearAuthSession();
    setSession(null);
  }

  const value = useMemo<AuthContextValue>(
    () => ({
      session,
      hydrated,
      isPending,
      isAuthenticated: !!session?.token,
      login,
      register,
      logout,
    }),
    [hydrated, isPending, session],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);

  if (!context) {
    throw new Error('useAuth precisa ser usado dentro de AuthProvider');
  }

  return context;
}
