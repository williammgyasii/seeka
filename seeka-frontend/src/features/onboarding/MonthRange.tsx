import type { FieldValues, Path, UseFormReturn } from 'react-hook-form'
import { Field, Input } from '@/shared/ui'
import { errorAt } from './formValues'

/** Start and end month for an item at `prefix`; a current item has no end month. */
export function MonthRange<T extends FieldValues>({
  form,
  prefix,
  currentLabel,
}: {
  form: UseFormReturn<T>
  prefix: string
  currentLabel: string
}) {
  const path = (field: string) => `${prefix}.${field}` as Path<T>
  const current = Boolean(form.watch(path('current')))
  return (
    <div className="grid gap-4 sm:grid-cols-[1fr_1fr_auto] sm:items-start">
      <Field label="Start month" error={errorAt(form, path('startMonth'))}>
        <Input type="month" {...form.register(path('startMonth'))} />
      </Field>
      <Field label="End month" error={errorAt(form, path('endMonth'))}>
        <Input type="month" disabled={current} {...form.register(path('endMonth'))} />
      </Field>
      <label className="flex h-9 items-center gap-2 text-sm sm:mt-6">
        <input type="checkbox" className="size-4 accent-primary" {...form.register(path('current'))} />
        {currentLabel}
      </label>
    </div>
  )
}
