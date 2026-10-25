import type { ReactNode } from 'react'
import { useParams } from 'react-router-dom'
import type { JobDetail } from '@/shared/api'
import { humanize } from '@/shared/lib/format'
import {
  Button,
  EmptyState,
  Label,
  Loading,
  Page,
  PageHeader,
  SectionCard,
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/ui'
import { EligibilityCard, FitCard } from './AssessmentCards'
import { pipelineStages, useChangeStageMutation, useGetJobQuery } from './jobsApi'

export function JobDetailPage({ outreach }: { outreach: (detail: JobDetail) => ReactNode }) {
  const { id = '' } = useParams()
  const detail = useGetJobQuery(id)
  const [changeStage] = useChangeStageMutation()

  if (detail.isLoading) return <Loading />
  if (detail.isError || !detail.data)
    return <EmptyState title="Job not found" description="It may belong to another workspace." />
  const data = detail.data

  return (
    <Page>
      <PageHeader
        eyebrow={data.job.companyName}
        title={data.job.title}
        description={data.job.locationText ?? 'Location not disclosed'}
        action={
          <Button asChild variant="outline" size="lg">
            <a href={data.job.originalUrl} target="_blank" rel="noreferrer">
              Original source
            </a>
          </Button>
        }
      />
      <div className="grid gap-6 xl:grid-cols-[1.25fr_.75fr]">
        <div className="grid gap-6">
          <EligibilityCard eligibility={data.eligibility} />
          <FitCard match={data.match} />
          <SectionCard title="Job description" description="Original imported content">
            <p className="leading-7 whitespace-pre-wrap">{data.job.description}</p>
          </SectionCard>
          {outreach(data)}
        </div>
        <aside className="grid content-start gap-6">
          <SectionCard title="Pipeline" description="Application is tracked separately">
            <div className="grid gap-2">
              <Label id="stage-label">Stage</Label>
              <Select
                value={data.savedJob.stage}
                onValueChange={(stage) => changeStage({ savedJobId: id, stage })}
              >
                <SelectTrigger aria-labelledby="stage-label" className="w-full">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {pipelineStages.map((stage) => (
                    <SelectItem key={stage} value={stage}>
                      {humanize(stage)}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <p className="text-muted-foreground">
              Application: {humanize(data.savedJob.applicationStatus)}
            </p>
          </SectionCard>
          <SectionCard title="Activity" description="Workspace audit trail">
            <ol className="grid gap-4">
              {data.activities.map((activity) => (
                <li key={activity.id} className="border-l-2 pl-4">
                  <p className="font-medium">{activity.description}</p>
                  <p className="mt-1 text-xs text-muted-foreground">
                    {new Date(activity.occurredAt).toLocaleString()}
                  </p>
                </li>
              ))}
            </ol>
          </SectionCard>
        </aside>
      </div>
    </Page>
  )
}
