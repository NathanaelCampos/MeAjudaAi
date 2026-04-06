import { Suspense } from 'react';
import { HistoryPageClient } from './page-client';

export default function HistoricoPage() {
  return (
    <Suspense fallback={null}>
      <HistoryPageClient />
    </Suspense>
  );
}
