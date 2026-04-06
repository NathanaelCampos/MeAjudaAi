import { Suspense } from 'react';
import { QueuePageClient } from './page-client';

export default function FilaPage() {
  return (
    <Suspense fallback={null}>
      <QueuePageClient />
    </Suspense>
  );
}
