import { describe, expect, it } from 'vitest'
import { setupStore } from '@/app/store'
import { sessionApi } from '@/shared/api'
import { respond } from '@/test/server'
import { verificationApi } from './verificationApi'

const unverified = {
  userId: 'u1',
  email: 'ama@example.com',
  displayName: 'Ama',
  workspaceId: 'w1',
  onboardingComplete: false,
  emailVerified: false,
}

async function signedInStore() {
  const session = respond('get', '/api/auth/session', unverified)
  const store = setupStore()
  store.dispatch(sessionApi.endpoints.getSession.initiate())
  await expect.poll(() => session.count).toBe(1)
  return { store, session }
}

describe('verificationApi', () => {
  it("marks the signed-in user's cached session verified at once, then refreshes it", async () => {
    const { store, session } = await signedInStore()
    respond('post', '/api/auth/verify-email')

    await store.dispatch(
      verificationApi.endpoints.verifyEmail.initiate({ userId: 'u1', token: 't' }),
    )

    const cached = sessionApi.endpoints.getSession.select()(store.getState())
    expect(cached.data?.emailVerified).toBe(true)
    await expect.poll(() => session.count).toBe(2)
  })

  it("leaves another user's cached session alone", async () => {
    const { store } = await signedInStore()
    respond('post', '/api/auth/verify-email')

    await store.dispatch(
      verificationApi.endpoints.verifyEmail.initiate({ userId: 'someone-else', token: 't' }),
    )

    const cached = sessionApi.endpoints.getSession.select()(store.getState())
    expect(cached.data?.emailVerified).toBe(false)
  })
})
