import { useId } from 'react'
import { Controller, type Control, type FieldValues, type Path } from 'react-hook-form'
import { Label, Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/shared/ui'

/** A labelled dropdown bound to a form field; the value is '' until a choice is made. */
export function ChoiceField<T extends FieldValues>({
  control,
  name,
  label,
  placeholder,
  options,
  optionLabel = (option) => option,
  disabled = false,
  onValueChange,
}: {
  control: Control<T>
  name: Path<T>
  label: string
  placeholder: string
  options: readonly string[]
  optionLabel?: (option: string) => string
  disabled?: boolean
  onValueChange?: (value: string) => void
}) {
  const labelId = useId()
  return (
    <div className="grid gap-2">
      <Label id={labelId}>{label}</Label>
      <Controller
        control={control}
        name={name}
        render={({ field, fieldState }) => (
          <>
            <Select
              value={field.value}
              disabled={disabled}
              onValueChange={(value) => {
                field.onChange(value)
                onValueChange?.(value)
              }}
            >
              <SelectTrigger
                aria-labelledby={labelId}
                aria-invalid={fieldState.invalid}
                className="w-full"
              >
                <SelectValue placeholder={placeholder} />
              </SelectTrigger>
              <SelectContent>
                {options.map((option) => (
                  <SelectItem key={option} value={option}>
                    {optionLabel(option)}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            {fieldState.error && (
              <span className="text-xs text-destructive">{fieldState.error.message}</span>
            )}
          </>
        )}
      />
    </div>
  )
}
