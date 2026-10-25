import { humanize } from '@/shared/lib/format'
import { StatusBadge, type StatusTone } from './StatusBadge'

const tones: Record<string, StatusTone> = {
  Eligible: 'success',
  Ineligible: 'danger',
  NeedsConfirmation: 'warning',
}

export function EligibilityBadge({ status }: { status?: string }) {
  return (
    <StatusBadge tone={(status && tones[status]) || 'neutral'}>
      {humanize(status ?? 'Unknown')}
    </StatusBadge>
  )
}
