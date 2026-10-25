import { describe, expect, it } from 'vitest'
import { errorMessage } from './errorMessage'

describe('errorMessage', () => {
  it('shows the ProblemDetails title the API wrote', () => {
    expect(
      errorMessage({ status: 409, data: { title: 'Complete the candidate profile first.' } }),
    ).toBe('Complete the candidate profile first.')
  })

  it('lists the reasons from a validation problem instead of its generic title', () => {
    expect(
      errorMessage({
        status: 400,
        data: {
          title: 'One or more validation errors occurred.',
          errors: {
            PasswordRequiresDigit: ["Passwords must have at least one digit ('0'-'9')."],
            PasswordRequiresUpper: ["Passwords must have at least one uppercase ('A'-'Z')."],
          },
        },
      }),
    ).toBe(
      "Passwords must have at least one digit ('0'-'9'). Passwords must have at least one uppercase ('A'-'Z').",
    )
  })

  it('shows a plain error field when there is no title', () => {
    expect(errorMessage({ status: 400, data: { error: 'Invalid stage.' } })).toBe('Invalid stage.')
  })

  it('falls back to the status when the body says nothing', () => {
    expect(errorMessage({ status: 401, data: null })).toBe('Request failed (401)')
  })

  it('explains a network failure without leaking the raw error', () => {
    expect(errorMessage({ status: 'FETCH_ERROR', error: 'TypeError: Failed to fetch' })).toBe(
      'Could not reach Seeka. Check your connection and try again.',
    )
  })

  it('shows the message of a serialized or thrown error', () => {
    expect(errorMessage({ message: 'Aborted' })).toBe('Aborted')
    expect(errorMessage(new Error('Boom'))).toBe('Boom')
  })

  it('has a fallback for anything else', () => {
    expect(errorMessage(undefined)).toBe('Something went wrong.')
  })
})
