import { cn } from '@/shared/lib/utils'
import { stepOrder, stepTitles, type StepId } from './steps'

export function StepProgress({ step }: { step: StepId }) {
  const position = stepOrder.indexOf(step) + 1
  const total = stepOrder.length
  return (
    <div className="grid shrink-0 gap-2">
      <div
        role="progressbar"
        aria-label={`Step ${position} of ${total}: ${stepTitles[step]}`}
        aria-valuemin={1}
        aria-valuemax={total}
        aria-valuenow={position}
        className="h-2 overflow-hidden rounded-full bg-muted"
      >
        <div
          className="h-full rounded-full bg-success transition-[width] motion-reduce:transition-none"
          style={{ width: `${(position / total) * 100}%` }}
        />
      </div>
      <ol className="hidden flex-wrap gap-x-4 gap-y-1 text-xs text-muted-foreground sm:flex">
        {stepOrder.map((id, index) => (
          <li key={id} className={cn(id === step && 'font-semibold text-foreground')}>
            {index + 1}. {stepTitles[id]}
          </li>
        ))}
      </ol>
    </div>
  )
}
