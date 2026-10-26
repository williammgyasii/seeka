import { canonicalRedirect } from '../src/lib/canonical-host'

type Context = { request: Request; next: () => Promise<Response> }

export const onRequest = ({ request, next }: Context) => canonicalRedirect(request) ?? next()
