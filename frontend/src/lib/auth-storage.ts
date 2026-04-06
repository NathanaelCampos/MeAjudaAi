const AUTH_STORAGE_KEY = 'meajudaai.admin.auth';

export type UserRole = 'Cliente' | 'Profissional' | 'Administrador';

export interface AuthSession {
  usuarioId: string;
  nome: string;
  email: string;
  token: string;
  expiraEmUtc: string;
  role?: UserRole;
}

export function readAuthSession(): AuthSession | null {
  if (typeof window === 'undefined') {
    return null;
  }

  const rawValue = window.localStorage.getItem(AUTH_STORAGE_KEY);
  if (!rawValue) {
    return null;
  }

  try {
    return JSON.parse(rawValue) as AuthSession;
  } catch {
    window.localStorage.removeItem(AUTH_STORAGE_KEY);
    return null;
  }
}

export function writeAuthSession(session: AuthSession) {
  if (typeof window === 'undefined') {
    return;
  }

  window.localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify(session));
}

export function clearAuthSession() {
  if (typeof window === 'undefined') {
    return;
  }

  window.localStorage.removeItem(AUTH_STORAGE_KEY);
}
