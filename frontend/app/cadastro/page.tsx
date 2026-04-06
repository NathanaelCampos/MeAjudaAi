import { Suspense } from 'react';
import { RegisterPageClient } from './page-client';

export default function RegisterPage() {
  return (
    <Suspense fallback={null}>
      <RegisterPageClient />
    </Suspense>
  );
}
