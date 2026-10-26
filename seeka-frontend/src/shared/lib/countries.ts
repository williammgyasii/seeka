import {
  getAllCountries,
  getCountry,
  getCountryForTimezone,
  getTimezone,
} from 'countries-and-timezones'

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

/**
 * The IANA timezones in use in a country, or none for an unknown code. IANA folds
 * some zones into a neighbour's (Africa/Accra into Africa/Abidjan), so a zone owned
 * by another country is shown by this country's own alias when it has one.
 */
export function timeZonesForCountry(code: string): string[] {
  const aliases = getCountry(code, { deprecated: true })?.timezones ?? []
  const ownAlias = (zone: string) =>
    aliases.find(
      (alias) =>
        alias.includes('/') &&
        getTimezone(alias)?.aliasOf === zone &&
        getCountryForTimezone(alias, { deprecated: true })?.id === code,
    )
  return (getCountry(code)?.timezones ?? [])
    .map((zone) =>
      getCountryForTimezone(zone)?.id === code ? zone : (ownAlias(zone) ?? zone),
    )
    .sort()
}
