import { Suspense } from 'react';
import { ExplorePageClient } from './page-client';

export default function ExplorePage() {
  return (
    <Suspense fallback={null}>
      <ExplorePageClient />
    </Suspense>
  );
}
