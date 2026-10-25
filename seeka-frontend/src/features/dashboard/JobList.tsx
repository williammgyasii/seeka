import { Link } from 'react-router-dom'
import type { PipelineItem } from '@/shared/api'
import { EligibilityBadge, StatusBadge } from '@/shared/ui'

export function JobList({ items }: { items: PipelineItem[] }) {
  return (
    <div className="divide-y">
      {items.map((item) => (
        <Link
          key={item.savedJobId}
          to={`/jobs/${item.savedJobId}`}
          className="flex flex-wrap items-center justify-between gap-4 py-4 first:pt-0 last:pb-0"
        >
          <div>
            <p className="font-semibold">{item.title}</p>
            <p className="mt-1 text-sm text-muted-foreground">{item.companyName}</p>
          </div>
          <div className="flex gap-2">
            <EligibilityBadge status={item.eligibilityStatus} />
            {item.matchScore !== undefined && (
              <StatusBadge tone="info">{item.matchScore}% fit</StatusBadge>
            )}
          </div>
        </Link>
      ))}
    </div>
  )
}
