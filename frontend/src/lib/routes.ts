export function isPublicRoute(pathname: string) {
  return (
    pathname === '/login' ||
    pathname === '/cadastro' ||
    pathname === '/explorar' ||
    pathname.startsWith('/profissionais/')
  );
}

export function isProductRoute(pathname: string) {
  return (
    pathname === '/cadastro' ||
    pathname === '/conta' ||
    pathname === '/explorar' ||
    pathname.startsWith('/onboarding/') ||
    pathname.startsWith('/profissionais/') ||
    pathname === '/servicos' ||
    pathname.startsWith('/servicos/')
  );
}

export function usesMinimalShell(pathname: string) {
  return (
    pathname === '/login' ||
    pathname === '/cadastro' ||
    pathname === '/conta' ||
    pathname === '/explorar' ||
    pathname.startsWith('/profissionais/') ||
    pathname.startsWith('/onboarding/') ||
    pathname.startsWith('/servicos')
  );
}
