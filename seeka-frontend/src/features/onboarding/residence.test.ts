import { describe, expect, it } from 'vitest'
import { countryOptions, suggestResidence } from './residence'

describe('suggestResidence', () => {
  it('suggests the country a timezone belongs to', () => {
    expect(suggestResidence('Europe/Lisbon')).toBe('Portugal')
  })

  it('suggests the main country of a timezone several countries share', () => {
    expect(suggestResidence('Africa/Lagos')).toBe('Nigeria')
  })

  it('suggests nothing for a timezone without a country', () => {
    expect(suggestResidence('UTC')).toBeNull()
  })

  it('suggests nothing for an unknown timezone', () => {
    expect(suggestResidence('Mars/Olympus_Mons')).toBeNull()
  })
})

describe('countryOptions', () => {
  it('lists English names the way postings write them, sorted', () => {
    const options = countryOptions()

    expect(options).toContain('United States')
    expect(options).toContain('United Kingdom')
    expect(options).not.toContain('United States of America')
    expect(options).toEqual([...options].sort((a, b) => a.localeCompare(b, 'en')))
  })
})
