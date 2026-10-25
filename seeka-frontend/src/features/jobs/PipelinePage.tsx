import { Plus } from 'lucide-react'
import { Link } from 'react-router-dom'
import { useGetPipelineQuery } from '@/shared/api'
import { humanize } from '@/shared/lib/format'
import {
  Badge,
  Button,
  Card,
  CardContent,
  EligibilityBadge,
  Page,
  PageHeader,
  StatusBadge,
} from '@/shared/ui'
import { pipelineStages } from './jobsApi'

export function PipelinePage() {
  const pipeline = useGetPipelineQuery()
  return (
    <Page>
      <PageHeader
        eyebrow="Pipeline"
        title="Every opportunity, one next step."
        description="Formal application status remains separate from recruiter contact."
        action={
          <Button asChild size="lg">
            <Link to="/jobs/import">
              <Plus /> Add job
            </Link>
          </Button>
        }
      />
      <div className="grid gap-4 lg:grid-cols-4">
        {pipelineStages.map((stage) => {
          const jobs = (pipeline.data ?? []).filter((item) => item.stage === stage)
          return (
            <section key={stage} className="min-h-44 rounded-xl bg-muted p-3">
              <div className="flex items-center justify-between px-1 pb-3">
                <h2 className="text-sm font-semibold">{humanize(stage)}</h2>
                <Badge variant="outline">{jobs.length}</Badge>
              </div>
              <div className="grid gap-3">
                {jobs.map((item) => (
                  <Link key={item.savedJobId} to={`/jobs/${item.savedJobId}`}>
                    <Card size="sm" className="transition hover:-translate-y-0.5 hover:ring-ring">
                      <CardContent>
                        <p className="font-semibold">{item.title}</p>
                        <p className="mt-1 text-muted-foreground">{item.companyName}</p>
                        <div className="mt-3 flex flex-wrap gap-2">
                          <EligibilityBadge status={item.eligibilityStatus} />
                          {item.matchScore !== undefined && (
                            <StatusBadge tone="info">{item.matchScore}% fit</StatusBadge>
                          )}
                        </div>
                      </CardContent>
                    </Card>
                  </Link>
                ))}
              </div>
            </section>
          )
        })}
      </div>
    </Page>
  )
}
