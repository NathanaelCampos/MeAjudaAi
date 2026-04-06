import { Suspense } from 'react';
import { MyServicesPageClient } from './page-client';

export default function ServicesPage() {
  return (
    <Suspense fallback={null}>
      <MyServicesPageClient />
    </Suspense>
  );
}
