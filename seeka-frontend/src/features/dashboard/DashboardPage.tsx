import { Plus } from 'lucide-react'
import { Link } from 'react-router-dom'
import { useGetPipelineQuery } from '@/shared/api'
import {
  Button,
  Card,
  CardContent,
  EmptyState,
  Loading,
  Metric,
  Page,
  PageHeader,
  SectionCard,
} from '@/shared/ui'
import { JobList } from './JobList'

export function DashboardPage() {
  const pipeline = useGetPipelineQuery()
  const items = pipeline.data ?? []
  const strongMatches = items.filter((item) => (item.matchScore ?? 0) >= 70).length
  const needsConfirmation = items.filter(
    (item) => item.eligibilityStatus === 'NeedsConfirmation',
  ).length
  const contacted = items.filter((item) => item.stage === 'Contacted').length

  return (
    <Page>
      <PageHeader
        eyebrow="Today"
        title="Your remote job search, clearly prioritised."
        description="Eligibility is checked separately from fit so a strong-looking role never hides a location restriction."
        action={
          <Button asChild size="lg">
            <Link to="/jobs/import">
              <Plus /> Import job
            </Link>
          </Button>
        }
      />
      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <Metric label="Jobs in pipeline" value={items.length} detail="Workspace total" />
        <Metric label="Strong matches" value={strongMatches} detail="Score 70 or above" />
        <Metric
          label="Confirm eligibility"
          value={needsConfirmation}
          detail="Evidence needs review"
        />
        <Metric label="Contacted" value={contacted} detail="Application tracked separately" />
      </div>
      <div className="grid gap-6 xl:grid-cols-[1.4fr_.6fr]">
        <SectionCard title="Recent opportunities" description="Latest pipeline activity">
          {pipeline.isLoading ? (
            <Loading />
          ) : pipeline.isError ? (
            <EmptyState
              title="Sign in to load your workspace"
              description="The API protects every pipeline record with workspace membership."
            />
          ) : items.length ? (
            <JobList items={items.slice(0, 5)} />
          ) : (
            <EmptyState
              title="No jobs yet"
              description="Import a real posting to assess eligibility and professional fit."
              action={
                <Button asChild variant="outline">
                  <Link to="/jobs/import">Import your first job</Link>
                </Button>
              }
            />
          )}
        </SectionCard>
        <Card className="bg-primary text-primary-foreground">
          <CardContent>
            <p className="text-xs font-semibold tracking-[.2em] uppercase opacity-70">
              How Seeka reads a role
            </p>
            <ol className="mt-5 grid gap-5">
              {[
                ['1', 'Can this employer hire you where you live?'],
                ['2', 'How closely does the work match your profile?'],
                ['3', 'Who is relevant, and what can you truthfully say?'],
              ].map(([number, text]) => (
                <li key={number} className="flex gap-3 text-sm opacity-90">
                  <span className="grid size-7 shrink-0 place-items-center rounded-full bg-primary-foreground/15 text-xs font-bold">
                    {number}
                  </span>
                  {text}
                </li>
              ))}
            </ol>
          </CardContent>
        </Card>
      </div>
    </Page>
  )
}
