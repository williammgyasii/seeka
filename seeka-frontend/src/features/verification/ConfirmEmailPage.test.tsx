import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { Provider } from 'react-redux'
import { MemoryRouter } from 'react-router-dom'
import { describe, expect, it } from 'vitest'
import App from '@/app/App'
import { setupStore } from '@/app/store'
import { respond } from '@/test/server'

const session = {
  userId: 'u1',
  email: 'ama@example.com',
  displayName: 'Ama Mensah',
  workspaceId: 'w1',
  onboardingComplete: false,
  emailVerified: false,
}

const link = '/verify-email/confirm?userId=u1&token=a%2Bb%2Fc%3D'

function openLink(path = link) {
  render(
    <Provider store={setupStore()}>
      <MemoryRouter initialEntries={[path]}>
        <App />
      </MemoryRouter>
    </Provider>,
  )
}

describe('Verification link page', () => {
  it('posts the user id and the decoded token once', async () => {
    respond('get', '/api/auth/session', null, 401)
    const verify = respond('post', '/api/auth/verify-email')
    openLink()

    await screen.findByRole('heading', { name: /your email is verified/i })
    expect(verify.count).toBe(1)
    expect(await verify.calls[0].json()).toEqual({ userId: 'u1', token: 'a+b/c=' })
  })

  it('continues to onboarding when signed in', async () => {
    respond('get', '/api/auth/session', { ...session, emailVerified: true })
    respond('post', '/api/auth/verify-email')
    const user = userEvent.setup()
    openLink()

    await screen.findByRole('heading', { name: /your email is verified/i })
    await user.click(await screen.findByRole('link', { name: /continue/i }))

    expect(
      await screen.findByRole('heading', { name: /give assessments reliable facts/i }),
    ).toBeInTheDocument()
  })

  it('offers sign-in when signed out', async () => {
    respond('get', '/api/auth/session', null, 401)
    respond('post', '/api/auth/verify-email')
    openLink()

    await screen.findByRole('heading', { name: /your email is verified/i })
    expect(await screen.findByRole('link', { name: /sign in/i })).toHaveAttribute('href', '/login')
  })

  it('explains an invalid or expired link and offers a new one', async () => {
    respond('get', '/api/auth/session', session)
    respond(
      'post',
      '/api/auth/verify-email',
      { title: 'This verification link is invalid or has expired.' },
      400,
    )
    openLink()

    expect(
      await screen.findByRole('heading', { name: /this link has expired or is invalid/i }),
    ).toBeInTheDocument()
    expect(screen.getByRole('link', { name: /get a new link/i })).toHaveAttribute(
      'href',
      '/verify-email',
    )
  })

  it('treats a link without a token as invalid without calling the API', async () => {
    respond('get', '/api/auth/session', null, 401)
    const verify = respond('post', '/api/auth/verify-email')
    openLink('/verify-email/confirm?userId=u1')

    expect(
      await screen.findByRole('heading', { name: /this link has expired or is invalid/i }),
    ).toBeInTheDocument()
    expect(verify.count).toBe(0)
  })
})
