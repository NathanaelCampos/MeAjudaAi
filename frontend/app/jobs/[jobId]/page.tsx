import { JobDetailPageClient } from './page-client';

export default async function JobDetailPage({
  params,
}: {
  params: Promise<{ jobId: string }>;
}) {
  const { jobId } = await params;

  return <JobDetailPageClient jobId={decodeURIComponent(jobId)} />;
}
