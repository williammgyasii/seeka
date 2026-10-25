import type { JobDetail } from '@/shared/api'
import { Alert, AlertDescription, AlertTitle, EligibilityBadge, SectionCard } from '@/shared/ui'

type MatchComponent = {
  Name: string
  EarnedPoints: number
  AvailablePoints: number
  Explanation: string
}

export function EligibilityCard({ eligibility }: { eligibility: JobDetail['eligibility'] }) {
  return (
    <SectionCard title="Geographic eligibility" description="Separate from job fit">
      <div className="flex items-center gap-3">
        <EligibilityBadge status={eligibility?.status} />
        <span className="text-xs text-muted-foreground">
          Rules assessment · checked{' '}
          {eligibility ? new Date(eligibility.checkedAt).toLocaleString() : '—'}
        </span>
      </div>
      <p className="leading-6">{eligibility?.decisionReason}</p>
      {eligibility?.evidenceExcerpts.map((excerpt) => (
        <blockquote
          key={excerpt}
          className="rounded-lg border-l-4 border-primary bg-muted p-3 text-muted-foreground italic"
        >
          “{excerpt}”
        </blockquote>
      ))}
    </SectionCard>
  )
}

export function FitCard({ match }: { match: JobDetail['match'] }) {
  const components = parseComponents(match)
  return (
    <SectionCard
      title={`Professional fit · ${match?.totalScore ?? 0}/100`}
      description={`Assessment model ${match?.modelVersion ?? '—'} — not a hiring probability`}
    >
      {components.map((component) => (
        <div key={component.Name}>
          <div className="mb-1.5 flex justify-between">
            <span className="font-medium">{component.Name}</span>
            <span className="text-muted-foreground">
              {component.EarnedPoints}/{component.AvailablePoints}
            </span>
          </div>
          <div className="h-2 rounded-full bg-muted">
            <div
              className="h-2 rounded-full bg-primary"
              style={{
                width: `${(component.EarnedPoints / component.AvailablePoints) * 100}%`,
              }}
            />
          </div>
          <p className="mt-1 text-xs text-muted-foreground">{component.Explanation}</p>
        </div>
      ))}
      {!!match?.missingRequirements.length && (
        <Alert className="border-warning/30 bg-warning/10 text-warning">
          <AlertTitle>Missing requirements</AlertTitle>
          <AlertDescription className="text-warning/90">
            {match.missingRequirements.join(', ')}
          </AlertDescription>
        </Alert>
      )}
    </SectionCard>
  )
}

function parseComponents(match: JobDetail['match']): MatchComponent[] {
  try {
    return match ? (JSON.parse(match.componentsJson) as MatchComponent[]) : []
  } catch {
    return []
  }
}
