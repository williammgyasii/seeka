import { canonicalRedirect } from '../../src/canonical-host'

export const onRequest: PagesFunction = (context) =>
  canonicalRedirect(context.request) ?? context.next()
