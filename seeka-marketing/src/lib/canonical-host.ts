const CANONICAL_ORIGIN = 'https://seekajobs.tech'

// Preview deployments live one label deeper (<hash>.seeka-marketing.pages.dev)
// and must stay reachable, so only the production pages.dev host matches.
const MOVED_HOSTS = [/^www\.seekajobs\.tech$/, /^seeka-marketing(-[a-z0-9]+)?\.pages\.dev$/]

export function canonicalRedirect(request: Request): Response | null {
  const url = new URL(request.url)
  if (!MOVED_HOSTS.some((host) => host.test(url.hostname))) return null
  return Response.redirect(`${CANONICAL_ORIGIN}${url.pathname}${url.search}`, 301)
}
