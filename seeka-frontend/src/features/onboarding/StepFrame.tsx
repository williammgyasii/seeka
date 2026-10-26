import type { FormEventHandler, ReactNode } from 'react'
import { Button, Card, ErrorMessage } from '@/shared/ui'

/**
 * Fills the height it is given: the heading and the buttons stay put and only
 * the fields scroll, so "Save and continue" is always on screen.
 */
export function StepFrame({
  title,
  description,
  onSubmit,
  onBack,
  onSkip,
  saving,
  failure,
  submitLabel = 'Save and continue',
  children,
}: {
  title: string
  description: string
  onSubmit: FormEventHandler<HTMLFormElement>
  onBack?: () => void
  onSkip?: () => void
  saving: boolean
  failure?: unknown
  submitLabel?: string
  children: ReactNode
}) {
  return (
    <Card className="min-h-0 flex-1 gap-0 py-0">
      <form className="flex min-h-0 flex-1 flex-col" onSubmit={onSubmit} noValidate>
        <div className="grid shrink-0 gap-1 border-b border-border px-5 py-4">
          <h2 className="text-lg font-semibold tracking-tight">{title}</h2>
          <p className="text-sm text-muted-foreground">{description}</p>
        </div>
        {/* relative: Radix's hidden native <select> is absolutely positioned and would otherwise stretch the page. */}
        <div className="relative grid min-h-0 flex-1 content-start gap-6 overflow-y-auto px-5 py-5">
          {children}
        </div>
        <div className="grid shrink-0 gap-3 border-t border-border px-5 py-3">
          {failure ? <ErrorMessage error={failure} /> : null}
          <div className="flex flex-wrap items-center justify-between gap-3">
            <div>
              {onBack && (
                <Button type="button" variant="outline" onClick={onBack} disabled={saving}>
                  Back
                </Button>
              )}
            </div>
            <div className="flex flex-wrap gap-2">
              {onSkip && (
                <Button type="button" variant="ghost" onClick={onSkip} disabled={saving}>
                  Skip this step
                </Button>
              )}
              <Button type="submit" disabled={saving}>
                {saving ? 'Saving…' : submitLabel}
              </Button>
            </div>
          </div>
        </div>
      </form>
    </Card>
  )
}
