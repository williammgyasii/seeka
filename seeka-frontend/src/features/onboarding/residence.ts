import { countryCodeForTimeZone, countryCodes, countryName } from '@/shared/lib/countries'

export function suggestResidence(timeZone: string): string | null {
  const code = countryCodeForTimeZone(timeZone)
  return code ? countryName(code) : null
}

export function countryOptions(): string[] {
  return countryCodes()
    .map(countryName)
    .sort((a, b) => a.localeCompare(b, 'en'))
}
