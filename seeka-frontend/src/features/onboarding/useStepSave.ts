import { useState } from 'react'
import type { FieldValues, UseFormSetError } from 'react-hook-form'
import type { OnboardingStep } from '@/shared/api'
import { useSaveStepMutation } from './onboardingApi'
import { showFieldErrors } from './serverErrors'

/** Saves one step; field errors land under their fields, anything else in `failure`. */
export function useStepSave<T extends FieldValues>(
  step: OnboardingStep,
  setError: UseFormSetError<T>,
) {
  const [save, state] = useSaveStepMutation()
  const [failure, setFailure] = useState<unknown>(null)

  const submit = async (body: unknown) => {
    setFailure(null)
    const result = await save({ step, body })
    if (!result.error) return true
    if (!showFieldErrors(result.error, setError)) setFailure(result.error)
    return false
  }

  return { submit, saving: state.isLoading, failure, setFailure }
}
