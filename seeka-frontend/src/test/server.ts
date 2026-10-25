import { http, HttpResponse, type JsonBodyType } from 'msw'
import { setupServer } from 'msw/node'

export const server = setupServer()

type Method = 'get' | 'post' | 'put'

export function respond(method: Method, path: string, body?: JsonBodyType, status = 200) {
  const calls: Request[] = []
  server.use(
    http[method](path, ({ request }) => {
      calls.push(request)
      return body === undefined
        ? new HttpResponse(null, { status: status === 200 ? 204 : status })
        : HttpResponse.json(body, { status })
    }),
  )
  return {
    calls,
    get count() {
      return calls.length
    },
  }
}
