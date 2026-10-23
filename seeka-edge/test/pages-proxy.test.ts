import { describe, expect, it, vi } from 'vitest'
import { proxyApiRequest } from '../src/pages-proxy'

describe('Pages API proxy', () => {
  it('forwards the original request through the service binding', async () => {
    const expected = new Response('proxied', {
      status: 201,
      headers: { 'set-cookie': '__Host-seeka-session=value; Secure; Path=/' },
    })
    const fetch = vi.fn(async () => expected)
    const request = new Request('https://seeka.pages.dev/api/auth/register', {
      method: 'POST',
      body: '{}',
      headers: { 'content-type': 'application/json' },
    })

    const response = await proxyApiRequest(request, { fetch })

    expect(fetch).toHaveBeenCalledWith(request)
    expect(response.status).toBe(201)
    expect(response.headers.get('set-cookie')).toContain('__Host-seeka-session=')
    expect(await response.text()).toBe('proxied')
  })
})
