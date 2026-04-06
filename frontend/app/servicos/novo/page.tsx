import { Suspense } from 'react';
import { NewServicePageClient } from './page-client';

export default function NewServicePage() {
  return (
    <Suspense fallback={null}>
      <NewServicePageClient />
    </Suspense>
  );
}
