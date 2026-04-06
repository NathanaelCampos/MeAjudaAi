import { Suspense } from 'react';
import { RetriesPageClient } from './page-client';

export default function RetriesPage() {
  return (
    <Suspense fallback={null}>
      <RetriesPageClient />
    </Suspense>
  );
}
