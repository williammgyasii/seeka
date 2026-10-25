import { getAllCountries, getCountryForTimezone } from 'countries-and-timezones'

// The eligibility engine matches residence by name against posting text, so names
// come from Intl ("United States"), not the package ("United States of America").
const englishNames = new Intl.DisplayNames(['en'], { type: 'region' })

export function deviceTimeZone(): string {
  return Intl.DateTimeFormat().resolvedOptions().timeZone
}

/** ISO 3166 code of the timezone's main country, or null for zones like UTC. */
export function countryCodeForTimeZone(timeZone: string): string | null {
  return getCountryForTimezone(timeZone)?.id ?? null
}

export function countryName(code: string): string {
  return englishNames.of(code) ?? code
}

export function countryCodes(): string[] {
  return Object.keys(getAllCountries())
}
