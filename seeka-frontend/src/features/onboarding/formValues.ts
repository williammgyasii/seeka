import type { FieldValues, Path, UseFormReturn } from 'react-hook-form'
import type { CandidateProfile, OnboardingStep, ProfileDetails, ResumeDraft } from '@/shared/api'

/** Inputs hold strings; the API wants null for "not stated". */
export const text = (value: string) => value.trim() || null

export const year = (value: string) => (value.trim() ? Number(value) : null)

export const lines = (value: string) =>
  value
    .split('\n')
    .map((line) => line.trim())
    .filter(Boolean)

export const orEmpty = (value: string | number | null | undefined) =>
  value === null || value === undefined ? '' : String(value)

export const noDetails: ProfileDetails = {
  experience: [],
  education: [],
  certifications: [],
  languages: [],
  links: [],
  projects: [],
  volunteering: [],
}

/** A saved step shows what the user saved; an unsaved one shows the résumé draft. */
export function isSaved(profile: CandidateProfile, step: OnboardingStep) {
  return profile.completedSteps.includes(step)
}

export function draftDetails(draft: ResumeDraft | null): ProfileDetails {
  return draft?.details ?? noDetails
}

export function errorAt<T extends FieldValues>(form: UseFormReturn<T>, name: Path<T>) {
  return form.getFieldState(name, form.formState).error?.message
}
