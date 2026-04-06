import { ServiceDetailPageClient } from './page-client';

export default async function ServiceDetailPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = await params;

  return <ServiceDetailPageClient id={decodeURIComponent(id)} />;
}
