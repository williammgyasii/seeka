export const API_INSTANCE_NAME = 'seeka-api-primary'

type ContainerStub = {
  fetch(request: Request): Promise<Response>
}

type ContainerNamespace = {
  getByName(name: string): ContainerStub
}

export type SeekaEdgeEnvironment = {
  API_CONTAINER: ContainerNamespace
  DATABASE_CONNECTION_STRING: string
  INTERNAL_DEPLOY_TOKEN?: string
}

export function containerEnvironment(
  environment: Pick<SeekaEdgeEnvironment, 'DATABASE_CONNECTION_STRING'> &
    Record<string, unknown>,
): Record<string, string> {
  return {
    ASPNETCORE_ENVIRONMENT: 'Cloudflare',
    ASPNETCORE_URLS: 'http://+:8080',
    ConnectionStrings__Seeka: environment.DATABASE_CONNECTION_STRING,
    Hangfire__RunServer: 'false',
  }
}

export function routeToApi(
  request: Request,
  namespace: ContainerNamespace,
): Promise<Response> {
  return namespace.getByName(API_INSTANCE_NAME).fetch(request)
}
