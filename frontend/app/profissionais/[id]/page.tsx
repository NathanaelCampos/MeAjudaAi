import { ProfessionalDetailPageClient } from './page-client';

export default async function ProfessionalDetailPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = await params;

  return <ProfessionalDetailPageClient id={decodeURIComponent(id)} />;
}
