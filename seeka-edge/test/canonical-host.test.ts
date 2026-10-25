import { describe, expect, it } from 'vitest'
import { canonicalRedirect } from '../src/canonical-host'

describe('Canonical app host', () => {
  it('sends the old pages.dev address to the same path on app.seekajobs.tech', () => {
    const response = canonicalRedirect(
      new Request('https://seeka-cth.pages.dev/verify-email/confirm?userId=u1&token=a%2Bb'),
    )

    expect(response?.status).toBe(301)
    expect(response?.headers.get('location')).toBe(
      'https://app.seekajobs.tech/verify-email/confirm?userId=u1&token=a%2Bb',
    )
  })

  it.each([
    'https://app.seekajobs.tech/dashboard',
    'https://abc123.seeka-cth.pages.dev/dashboard',
    'http://localhost:8788/dashboard',
  ])('lets %s through', (url) => {
    expect(canonicalRedirect(new Request(url))).toBeNull()
  })
})
