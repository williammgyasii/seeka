import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { Provider } from 'react-redux'
import { MemoryRouter } from 'react-router-dom'
import { describe, expect, it } from 'vitest'
import { respond } from '@/test/server'
import App from './App'
import { setupStore } from './store'

const session = {
  userId: 'u1',
  email: 'ama@example.com',
  displayName: 'Ama Mensah',
  workspaceId: 'w1',
  onboardingComplete: true,
}

function renderAt(path: string) {
  return render(
    <Provider store={setupStore()}>
      <MemoryRouter initialEntries={[path]}>
        <App />
      </MemoryRouter>
    </Provider>,
  )
}

describe('Protected pages', () => {
  it('send a signed-out visitor to sign-in and back to the page they asked for', async () => {
    respond('get', '/api/auth/session', null, 401)
    respond('post', '/api/auth/login', session)
    respond('get', '/api/pipeline', [])
    const user = userEvent.setup()
    renderAt('/pipeline')

    expect(
      await screen.findByRole('heading', { name: /sign in to seeka/i }),
    ).toBeInTheDocument()

    await user.type(screen.getByLabelText(/email address/i), 'ama@example.com')
    await user.type(screen.getByLabelText(/^password/i), 'Correct-Horse-9')
    await user.click(screen.getByRole('button', { name: /^sign in$/i }))

    expect(
      await screen.findByRole('heading', { name: /every opportunity, one next step/i }),
    ).toBeInTheDocument()
  })

  it('send a signed-in visitor on sign-in to the dashboard', async () => {
    respond('get', '/api/auth/session', session)
    respond('get', '/api/pipeline', [])
    renderAt('/login')

    expect(
      await screen.findByRole('heading', { name: /your remote job search/i }),
    ).toBeInTheDocument()
  })

  it('send a user with unfinished onboarding to onboarding', async () => {
    respond('get', '/api/auth/session', { ...session, onboardingComplete: false })
    renderAt('/dashboard')

    expect(
      await screen.findByRole('heading', { name: /give assessments reliable facts/i }),
    ).toBeInTheDocument()
  })

  it('let a user with unfinished onboarding stay on onboarding', async () => {
    respond('get', '/api/auth/session', { ...session, onboardingComplete: false })
    renderAt('/onboarding')

    expect(
      await screen.findByRole('heading', { name: /give assessments reliable facts/i }),
    ).toBeInTheDocument()
  })
})
