type ProblemBody =
  | { title?: string; error?: string; errors?: Record<string, string[]> }
  | null
  | undefined

function problemText(body: ProblemBody, status: number): string {
  const reasons = Object.values(body?.errors ?? {}).flat()
  if (reasons.length) return reasons.join(' ')
  return body?.title ?? body?.error ?? `Request failed (${status})`
}

export function errorMessage(error: unknown): string {
  if (typeof error !== 'object' || error === null) return 'Something went wrong.'

  if ('status' in error) {
    if (typeof error.status === 'number')
      return problemText(('data' in error ? error.data : null) as ProblemBody, error.status)
    return 'Could not reach Seeka. Check your connection and try again.'
  }

  if ('message' in error && typeof error.message === 'string') return error.message
  return 'Something went wrong.'
}
