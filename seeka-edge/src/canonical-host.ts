const LEGACY_HOST = 'seeka-cth.pages.dev'
const CANONICAL_ORIGIN = 'https://app.seekajobs.tech'

/**
 * Only the exact production pages.dev host moves; preview subdomains stay
 * reachable so deployments can be checked before they are promoted.
 */
export function canonicalRedirect(request: Request): Response | null {
  const url = new URL(request.url)
  if (url.hostname !== LEGACY_HOST) return null
  return Response.redirect(`${CANONICAL_ORIGIN}${url.pathname}${url.search}`, 301)
}
