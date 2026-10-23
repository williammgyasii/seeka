import { proxyApiRequest } from '../../../src/pages-proxy'

interface PagesEnvironment {
  SEEKA_API: Fetcher
}

export const onRequest: PagesFunction<PagesEnvironment> = (context) =>
  proxyApiRequest(context.request, context.env.SEEKA_API)
