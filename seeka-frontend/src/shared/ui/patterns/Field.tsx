import type { ReactNode } from 'react'
import { Label } from '../primitives/label'

export function Field({
  label,
  error,
  children,
}: {
  label: string
  error?: string
  children: ReactNode
}) {
  return (
    <Label className="grid gap-2">
      {label}
      {children}
      {error && <span className="text-xs font-normal text-destructive">{error}</span>}
    </Label>
  )
}
