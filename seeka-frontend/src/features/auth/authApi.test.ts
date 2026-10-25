import { describe, expect, it } from 'vitest'
import { setupStore } from '@/app/store'
import { pipelineApi } from '@/shared/api'
import { respond } from '@/test/server'
import { authApi } from './authApi'

const session = { userId: 'user-1', email: 'ama@example.com', displayName: 'Ama' }

describe('authApi', () => {
  it('caches the session from sign-in without asking the API again', async () => {
    const store = setupStore()
    respond('post', '/api/auth/login', session)
    const sessionReads = respond('get', '/api/auth/session', session)

    await store.dispatch(
      authApi.endpoints.signIn.initiate({ email: session.email, password: 'long-password' }),
    )

    expect(authApi.endpoints.getSession.select()(store.getState()).data).toEqual(session)
    expect(sessionReads.count).toBe(0)
  })

  it('forgets every cached response after sign-out', async () => {
    const store = setupStore()
    respond('get', '/api/pipeline', [{ savedJobId: 'job-1' }])
    respond('post', '/api/auth/logout')
    await store.dispatch(pipelineApi.endpoints.getPipeline.initiate())

    await store.dispatch(authApi.endpoints.signOut.initiate())

    expect(pipelineApi.endpoints.getPipeline.select()(store.getState()).data).toBeUndefined()
  })
})
