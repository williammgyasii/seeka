import { getCountries, getCountryCallingCode, type CountryCode } from 'libphonenumber-js'
import { countryName } from '@/shared/lib/countries'

export const callingCodes = getCountries()
  .map((code) => ({ code, name: countryName(code), dial: `+${getCountryCallingCode(code)}` }))
  .sort((a, b) => a.name.localeCompare(b.name, 'en'))

export function isPhoneCountry(code: string | null): code is CountryCode {
  return code !== null && callingCodes.some((item) => item.code === code)
}
