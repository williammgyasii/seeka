import { readFile } from 'node:fs/promises'
import { describe, expect, it, vi } from 'vitest'
import {
  API_INSTANCE_NAME,
  containerEnvironment,
  routeToApi,
} from '../src/routing'

describe('API container routing', () => {
  it('routes every request to the stable API instance', async () => {
    const fetch = vi.fn(async () => new Response('ok'))
    const getByName = vi.fn(() => ({ fetch }))
    const request = new Request('https://seeka.invalid/api/health')

    const response = await routeToApi(request, { getByName })

    expect(getByName).toHaveBeenCalledWith(API_INSTANCE_NAME)
    expect(fetch).toHaveBeenCalledWith(request)
    expect(await response.text()).toBe('ok')
  })

  it('passes only the backend runtime values into the container', () => {
    const environment = containerEnvironment({
      DATABASE_CONNECTION_STRING: 'postgres-secret',
      EMAIL_API_TOKEN: 'email-secret',
      EMAIL_ACCOUNT_ID: 'account-id',
      APP_PUBLIC_URL: 'https://seeka.example',
      OPENAI_API_KEY: 'openai-secret',
      R2_ACCOUNT_ID: 'account-id',
      R2_ACCESS_KEY_ID: 'r2-key-id',
      R2_SECRET_ACCESS_KEY: 'r2-secret',
      INTERNAL_DEPLOY_TOKEN: 'must-not-enter-container',
    })

    expect(environment).toEqual({
      ASPNETCORE_ENVIRONMENT: 'Cloudflare',
      ASPNETCORE_URLS: 'http://+:8080',
      ConnectionStrings__Seeka: 'postgres-secret',
      Hangfire__RunServer: 'false',
      App__PublicUrl: 'https://seeka.example',
      Email__CloudflareAccountId: 'account-id',
      Email__CloudflareApiToken: 'email-secret',
      OpenAI__ApiKey: 'openai-secret',
      Storage__R2AccountId: 'account-id',
      Storage__R2AccessKeyId: 'r2-key-id',
      Storage__R2SecretAccessKey: 'r2-secret',
    })
  })

  it('disables the public workers.dev route in configuration', async () => {
    const path = new URL('../wrangler.jsonc', import.meta.url)
    const text = await readFile(path, 'utf8').catch(() => '{}')
    const config = JSON.parse(text.replace(/^\s*\/\/.*$/gm, '')) as {
      workers_dev?: boolean
      containers?: Array<{ class_name?: string }>
    }

    expect(config.workers_dev).toBe(false)
    expect(config.containers?.[0]?.class_name).toBe('SeekaApiContainer')
  })
})
