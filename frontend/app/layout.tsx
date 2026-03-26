import './globals.css';

export const metadata = {
  title: 'Me Ajuda Ai - Jobs Dashboard',
  description: 'Observability mobile-first para jobs em background',
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="pt-BR">
      <body className="bg-slate-50 text-slate-900">
        {children}
      </body>
    </html>
  );
}
