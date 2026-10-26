import { describe, expect, it } from 'vitest'
import { fieldPath, openingStep } from './steps'

describe('openingStep', () => {
  it('starts with the résumé when nothing is saved and nothing was uploaded', () => {
    expect(openingStep([], false)).toBe('upload')
  })

  it('starts with About you when a résumé is stored but no step is saved', () => {
    expect(openingStep([], true)).toBe('about')
  })

  it('resumes at the first unfinished step', () => {
    expect(openingStep(['about', 'experience'], true)).toBe('education')
    expect(openingStep(['about', 'projects'], false)).toBe('experience')
  })

  it('opens preferences when every step is saved but onboarding is not finished', () => {
    expect(openingStep(['about', 'experience', 'education', 'projects', 'preferences'], true)).toBe(
      'preferences',
    )
  })
})

describe('fieldPath', () => {
  it('turns a server field path into a form field path', () => {
    expect(fieldPath('experience[1].endMonth')).toBe('experience.1.endMonth')
    expect(fieldPath('experience[0].highlights[2]')).toBe('experience.0.highlights')
    expect(fieldPath('fullName')).toBe('fullName')
  })
})
