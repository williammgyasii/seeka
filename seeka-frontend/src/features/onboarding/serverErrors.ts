import type { FieldValues, Path, UseFormSetError } from 'react-hook-form'
import { fieldPath } from './steps'

/**
 * Puts each field error from a 400 under the field it names. Returns false
 * when the error is not a field-level validation problem, so the caller shows
 * it as a general message instead.
 */
export function showFieldErrors<T extends FieldValues>(
  error: unknown,
  setError: UseFormSetError<T>,
): boolean {
  const errors = validationErrors(error)
  if (!errors) return false
  for (const [path, messages] of Object.entries(errors))
    setError(fieldPath(path) as Path<T>, { type: 'server', message: messages.join(' ') })
  return true
}

function validationErrors(error: unknown): Record<string, string[]> | null {
  if (typeof error !== 'object' || error === null || !('status' in error)) return null
  if (error.status !== 400 || !('data' in error)) return null
  const data = error.data as { errors?: Record<string, string[]> } | null
  return data?.errors && Object.keys(data.errors).length > 0 ? data.errors : null
}
