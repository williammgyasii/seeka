import {
  countryCodeForTimeZone,
  countryCodes,
  countryName,
  timeZonesForCountry,
} from '@/shared/lib/countries'

const codeByName = new Map(countryCodes().map((code) => [countryName(code), code]))

export function suggestResidence(timeZone: string): string | null {
  const code = countryCodeForTimeZone(timeZone)
  return code ? countryName(code) : null
}

export function countryOptions(): string[] {
  return [...codeByName.keys()].sort((a, b) => a.localeCompare(b, 'en'))
}

/** ISO 3166 code for a residence as the form names it, such as "Ghana". */
export function residenceCode(residence: string): string | null {
  return codeByName.get(residence) ?? null
}

export function residenceTimeZones(residence: string): string[] {
  const code = residenceCode(residence)
  return code ? timeZonesForCountry(code) : []
}

/**
 * The timezone to show after the residence changes: the only one there is,
 * or the current one when it is still in the list, or none so the user picks.
 */
export function timeZoneFor(residence: string, current: string): string {
  const zones = residenceTimeZones(residence)
  if (zones.length === 1) return zones[0]
  return zones.includes(current) ? current : ''
}
