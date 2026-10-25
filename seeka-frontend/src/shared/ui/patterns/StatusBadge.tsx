import type { ReactNode } from 'react'
import { cn } from '@/shared/lib/utils'
import { Badge } from '../primitives/badge'

export type StatusTone = 'success' | 'warning' | 'danger' | 'info' | 'neutral'

const toneClasses: Record<StatusTone, string> = {
  success: 'bg-success/10 text-success',
  warning: 'bg-warning/10 text-warning',
  danger: '',
  info: 'bg-info/10 text-info',
  neutral: '',
}

export function StatusBadge({
  tone = 'neutral',
  className,
  children,
}: {
  tone?: StatusTone
  className?: string
  children: ReactNode
}) {
  return (
    <Badge
      variant={tone === 'danger' ? 'destructive' : 'secondary'}
      className={cn(toneClasses[tone], className)}
    >
      {children}
    </Badge>
  )
}
