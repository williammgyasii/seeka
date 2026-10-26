import { describe, expect, it } from 'vitest'
import { itemSummary, monthRange, yearRange } from './summaries'

describe('monthRange', () => {
  it('names a current role as running to the present', () => {
    expect(monthRange('2021-06', '', true)).toBe('Jun 2021 – Present')
  })

  it('shows a finished role from start to end', () => {
    expect(monthRange('2018-06', '2021-02', false)).toBe('Jun 2018 – Feb 2021')
  })

  it('shows whichever end is known, and nothing when neither is', () => {
    expect(monthRange('2019-01', '', false)).toBe('Jan 2019')
    expect(monthRange('', '2020-03', false)).toBe('Mar 2020')
    expect(monthRange('', '', false)).toBe('')
  })

  it('ignores a month that is not YYYY-MM', () => {
    expect(monthRange('June 2021', '2022-13', false)).toBe('')
  })
})

describe('yearRange', () => {
  it('joins the years that are known', () => {
    expect(yearRange('2012', '2016')).toBe('2012 – 2016')
    expect(yearRange('2012', '')).toBe('2012')
    expect(yearRange('', '')).toBe('')
  })
})

describe('itemSummary', () => {
  it('reads "role at place", falling back to a placeholder until either is typed', () => {
    expect(itemSummary('HR Lead', 'Acme', 'New job')).toBe('HR Lead at Acme')
    expect(itemSummary('', 'Acme', 'New job')).toBe('Acme')
    expect(itemSummary('  ', '', 'New job')).toBe('New job')
  })
})
