import type { OnboardingStep } from '@/shared/api'

export type StepId = 'upload' | OnboardingStep

/** Onboarding order; must match the server's `OnboardingSteps.All` after the upload. */
export const stepOrder: StepId[] = [
  'upload',
  'about',
  'experience',
  'education',
  'projects',
  'preferences',
]

export const stepTitles: Record<StepId, string> = {
  upload: 'Your résumé',
  about: 'About you',
  experience: 'Experience',
  education: 'Education and certifications',
  projects: 'Projects and volunteering',
  preferences: 'Job preferences',
}

/**
 * Where onboarding opens: the first unfinished step, or the upload when the
 * user has neither saved a step nor uploaded a résumé. Preferences is the
 * fallback because it holds the Finish button.
 */
export function openingStep(completed: OnboardingStep[], hasResume: boolean): StepId {
  if (completed.length === 0 && !hasResume) return 'upload'
  return (
    stepOrder.find((step) => step !== 'upload' && !completed.includes(step)) ?? 'preferences'
  )
}

export function nextStep(step: StepId): StepId | null {
  return stepOrder[stepOrder.indexOf(step) + 1] ?? null
}

export function previousStep(step: StepId): StepId | null {
  return stepOrder[stepOrder.indexOf(step) - 1] ?? null
}

/**
 * Maps a server path like `experience[1].endMonth` to the form path
 * `experience.1.endMonth`. A trailing index points into a list the form edits
 * as one field (skills, highlights), so it maps to that field.
 */
export function fieldPath(serverPath: string): string {
  return serverPath.replace(/\[\d+\]$/, '').replace(/\[(\d+)\]/g, '.$1')
}
