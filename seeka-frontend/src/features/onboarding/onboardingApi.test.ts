import { describe, expect, it } from 'vitest'
import { setupStore } from '@/app/store'
import { sessionApi } from '@/shared/api'
import { respond } from '@/test/server'
import { emptyProfile } from './testData'
import { onboardingApi } from './onboardingApi'

const incomplete = {
  userId: 'u1',
  email: 'ama@example.com',
  displayName: 'Ama',
  workspaceId: 'w1',
  onboardingComplete: false,
  emailVerified: true,
}

describe('onboardingApi', () => {
  it('saves a step to its own endpoint and caches the returned profile without refetching', async () => {
    const read = respond('get', '/api/candidate/profile', emptyProfile)
    const saved = { ...emptyProfile, completedSteps: ['experience'] }
    const save = respond('put', '/api/candidate/profile/experience', saved)
    const store = setupStore()
    store.dispatch(onboardingApi.endpoints.getProfile.initiate())
    await expect.poll(() => read.count).toBe(1)

    await store.dispatch(
      onboardingApi.endpoints.saveStep.initiate({ step: 'experience', body: { experience: [] } }),
    )

    expect(save.count).toBe(1)
    expect(await save.calls[0].json()).toEqual({ experience: [] })
    const cached = onboardingApi.endpoints.getProfile.select()(store.getState())
    expect(cached.data?.completedSteps).toEqual(['experience'])
    expect(read.count).toBe(1)
  })

  it('marks onboarding complete once finishing succeeds, then refreshes the session', async () => {
    const session = respond('get', '/api/auth/session', incomplete)
    respond('post', '/api/candidate/onboarding/complete')
    const store = setupStore()
    store.dispatch(sessionApi.endpoints.getSession.initiate())
    await expect.poll(() => session.count).toBe(1)

    await store.dispatch(onboardingApi.endpoints.completeOnboarding.initiate())

    const cached = sessionApi.endpoints.getSession.select()(store.getState())
    expect(cached.data?.onboardingComplete).toBe(true)
    await expect.poll(() => session.count).toBe(2)
  })

  it('leaves the session alone when finishing is refused', async () => {
    const session = respond('get', '/api/auth/session', incomplete)
    respond('post', '/api/candidate/onboarding/complete', { errors: { preferences: ['Save it'] } }, 400)
    const store = setupStore()
    store.dispatch(sessionApi.endpoints.getSession.initiate())
    await expect.poll(() => session.count).toBe(1)

    await store.dispatch(onboardingApi.endpoints.completeOnboarding.initiate())

    const cached = sessionApi.endpoints.getSession.select()(store.getState())
    expect(cached.data?.onboardingComplete).toBe(false)
  })
})
