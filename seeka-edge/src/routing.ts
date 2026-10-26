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
  EMAIL_API_TOKEN: string
  EMAIL_ACCOUNT_ID: string
  APP_PUBLIC_URL: string
  OPENAI_API_KEY: string
  R2_ACCOUNT_ID: string
  R2_ACCESS_KEY_ID: string
  R2_SECRET_ACCESS_KEY: string
  INTERNAL_DEPLOY_TOKEN?: string
}

type ContainerValues = Pick<
  SeekaEdgeEnvironment,
  | 'DATABASE_CONNECTION_STRING'
  | 'EMAIL_API_TOKEN'
  | 'EMAIL_ACCOUNT_ID'
  | 'APP_PUBLIC_URL'
  | 'OPENAI_API_KEY'
  | 'R2_ACCOUNT_ID'
  | 'R2_ACCESS_KEY_ID'
  | 'R2_SECRET_ACCESS_KEY'
>

export function containerEnvironment(
  environment: ContainerValues & Record<string, unknown>,
): Record<string, string> {
  return {
    ASPNETCORE_ENVIRONMENT: 'Cloudflare',
    ASPNETCORE_URLS: 'http://+:8080',
    ConnectionStrings__Seeka: environment.DATABASE_CONNECTION_STRING,
    Hangfire__RunServer: 'false',
    App__PublicUrl: environment.APP_PUBLIC_URL,
    Email__CloudflareAccountId: environment.EMAIL_ACCOUNT_ID,
    Email__CloudflareApiToken: environment.EMAIL_API_TOKEN,
    OpenAI__ApiKey: environment.OPENAI_API_KEY,
    Storage__R2AccountId: environment.R2_ACCOUNT_ID,
    Storage__R2AccessKeyId: environment.R2_ACCESS_KEY_ID,
    Storage__R2SecretAccessKey: environment.R2_SECRET_ACCESS_KEY,
  }
}

export function routeToApi(
  request: Request,
  namespace: ContainerNamespace,
): Promise<Response> {
  return namespace.getByName(API_INSTANCE_NAME).fetch(request)
}
