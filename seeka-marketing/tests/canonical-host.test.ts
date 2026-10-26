import { describe, expect, it } from 'vitest'
import { canonicalRedirect } from '../src/lib/canonical-host'

function redirectFor(url: string) {
  return canonicalRedirect(new Request(url))
}

describe('canonical host', () => {
  it.each([
    'https://www.seekajobs.tech/guides/?a=1',
    'https://seeka-marketing.pages.dev/guides/?a=1',
    'https://seeka-marketing-4xy.pages.dev/guides/?a=1',
  ])('moves %s to the apex with the path and query', (url) => {
    const response = redirectFor(url)

    expect(response?.status).toBe(301)
    expect(response?.headers.get('Location')).toBe('https://seekajobs.tech/guides/?a=1')
  })

  it.each([
    'https://seekajobs.tech/',
    'https://abc123.seeka-marketing.pages.dev/',
    'http://localhost:4321/',
  ])('leaves %s alone', (url) => {
    expect(redirectFor(url)).toBeNull()
  })
})
