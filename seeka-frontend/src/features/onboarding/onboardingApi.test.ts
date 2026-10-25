import { describe, expect, it } from 'vitest'
import { setupStore } from '@/app/store'
import { sessionApi } from '@/shared/api'
import { respond } from '@/test/server'
import { onboardingApi } from './onboardingApi'

const incomplete = {
  userId: 'u1',
  email: 'ama@example.com',
  displayName: 'Ama',
  workspaceId: 'w1',
  onboardingComplete: false,
}

describe('onboardingApi', () => {
  it('marks onboarding complete as soon as the profile saves, then refreshes the session', async () => {
    const session = respond('get', '/api/auth/session', incomplete)
    respond('put', '/api/candidate', {})
    const store = setupStore()
    store.dispatch(sessionApi.endpoints.getSession.initiate())
    await expect.poll(() => session.count).toBe(1)

    await store.dispatch(onboardingApi.endpoints.saveCandidate.initiate({}))

    const cached = sessionApi.endpoints.getSession.select()(store.getState())
    expect(cached.data?.onboardingComplete).toBe(true)
    await expect.poll(() => session.count).toBe(2)
  })
})
