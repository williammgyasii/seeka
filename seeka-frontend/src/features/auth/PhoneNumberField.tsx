import type { ComponentProps } from 'react'
import { Input, Label, Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/shared/ui'
import { callingCodes } from './phoneCountries'

type PhoneNumberFieldProps = {
  country: string
  onCountryChange: (country: string) => void
  inputProps: ComponentProps<typeof Input>
  error?: string
}

export function PhoneNumberField({ country, onCountryChange, inputProps, error }: PhoneNumberFieldProps) {
  const selected = callingCodes.find((item) => item.code === country)
  return (
    <div className="grid gap-2">
      <Label id="phone-label" htmlFor="phone-number">
        Phone number
      </Label>
      <div className="flex gap-2">
        <Select value={country} onValueChange={onCountryChange}>
          <SelectTrigger aria-label="Country calling code" className="w-28 shrink-0">
            <SelectValue placeholder="Code">{selected?.dial}</SelectValue>
          </SelectTrigger>
          <SelectContent>
            {callingCodes.map((item) => (
              <SelectItem key={item.code} value={item.code}>
                {item.name} {item.dial}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
        <Input
          id="phone-number"
          type="tel"
          autoComplete="tel-national"
          aria-invalid={Boolean(error)}
          {...inputProps}
        />
      </div>
      {error && <span className="text-xs text-destructive">{error}</span>}
    </div>
  )
}
