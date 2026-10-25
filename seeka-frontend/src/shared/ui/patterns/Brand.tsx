import { cn } from '@/shared/lib/utils'

type BrandProps = {
  className?: string
  onPrimary?: boolean
}

export function Brand({ className, onPrimary = false }: BrandProps) {
  return (
    <div className={cn('flex items-center gap-3 font-semibold', className)}>
      <span
        className={cn(
          'grid size-9 place-items-center rounded-lg text-lg font-black',
          onPrimary ? 'bg-highlight text-highlight-foreground' : 'bg-primary text-highlight',
        )}
      >
        S
      </span>
      <span className="text-xl tracking-tight">Seeka</span>
    </div>
  )
}
