import type { ReactNode } from 'react';
import type { Metadata, Viewport } from 'next';
import { Fraunces, Manrope } from 'next/font/google';
import { Providers } from './providers';
import '../styles/globals.css';

const manrope = Manrope({
  subsets: ['latin'],
  variable: '--font-sans',
});

const fraunces = Fraunces({
  subsets: ['latin'],
  variable: '--font-display',
});

export const metadata: Metadata = {
  title: 'Me Ajuda Ai',
  description: 'Encontre profissionais confiáveis, solicite serviços e acompanhe tudo pelo celular.',
  applicationName: 'Me Ajuda Ai',
  manifest: '/manifest.webmanifest',
  appleWebApp: {
    capable: true,
    statusBarStyle: 'black-translucent',
    title: 'Me Ajuda Ai',
  },
  formatDetection: {
    telephone: false,
  },
};

export const viewport: Viewport = {
  themeColor: '#0f172a',
  width: 'device-width',
  initialScale: 1,
  viewportFit: 'cover',
};

export default function RootLayout({ children }: { children: ReactNode }) {
  return (
    <html lang="pt-BR">
      <body className={`${manrope.variable} ${fraunces.variable} bg-slate-50 text-slate-900`}>
        <Providers>{children}</Providers>
      </body>
    </html>
  );
}
