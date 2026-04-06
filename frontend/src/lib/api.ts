import { readAuthSession } from '@/lib/auth-storage';

const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL?.replace(/\/$/, '') || 'http://localhost:5231';

export class ApiError extends Error {
  status: number;

  constructor(message: string, status: number) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
  }
}

export function buildApiUrl(path: string) {
  if (/^https?:\/\//i.test(path)) {
    return path;
  }

  const normalizedPath = path.startsWith('/') ? path : `/${path}`;
  return `${API_BASE_URL}${normalizedPath}`;
}

function buildHeaders(init?: { headers?: HeadersInit }) {
  const session = readAuthSession();
  const headers = new Headers(init?.headers);

  if (!headers.has('Accept')) {
    headers.set('Accept', 'application/json');
  }

  if (session?.token && !headers.has('Authorization')) {
    headers.set('Authorization', `Bearer ${session.token}`);
  }

  return headers;
}

async function parseError(response: Response) {
  try {
    const contentType = response.headers.get('content-type') ?? '';

    if (contentType.includes('application/json')) {
      const body = (await response.json()) as { mensagem?: string; message?: string };
      return body?.mensagem || body?.message || `API request failed with status ${response.status}`;
    }

    const text = await response.text();
    return text || `API request failed with status ${response.status}`;
  } catch {
    return `API request failed with status ${response.status}`;
  }
}

export async function apiRequest<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(buildApiUrl(path), {
    ...init,
    headers: buildHeaders(init),
  });

  if (!response.ok) {
    throw new ApiError(await parseError(response), response.status);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  const contentType = response.headers.get('content-type') ?? '';
  if (!contentType.includes('application/json')) {
    return undefined as T;
  }

  return response.json() as Promise<T>;
}

export async function apiFetch<T>(path: string): Promise<T> {
  return apiRequest<T>(path);
}

export async function apiSend<T>(
  path: string,
  init?: Omit<RequestInit, 'body'> & { body?: BodyInit | object | null },
): Promise<T> {
  const headers = buildHeaders(init);
  let body = init?.body;

  if (body && typeof body === 'object' && !(body instanceof FormData) && !(body instanceof URLSearchParams) && !(body instanceof Blob) && !(body instanceof ArrayBuffer)) {
    if (!headers.has('Content-Type')) {
      headers.set('Content-Type', 'application/json');
    }

    body = JSON.stringify(body);
  }

  return apiRequest<T>(path, {
    ...init,
    headers,
    body: body as BodyInit | null | undefined,
  });
}
