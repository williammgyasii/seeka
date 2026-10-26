import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { Provider } from 'react-redux'
import { MemoryRouter } from 'react-router-dom'
import { describe, expect, it } from 'vitest'
import { setupStore } from '@/app/store'
import { respond } from '@/test/server'
import { AuthPage } from './AuthPage'

const session = {
  userId: 'u1',
  email: 'ama@example.com',
  displayName: 'Ama Mensah',
  workspaceId: 'w1',
  onboardingComplete: false,
  emailVerified: false,
}

async function openSignUp(timeZone = 'Africa/Accra') {
  respond('get', '/api/auth/session', null, 401)
  const user = userEvent.setup()
  render(
    <Provider store={setupStore()}>
      <MemoryRouter initialEntries={['/login']}>
        <AuthPage timeZone={timeZone} />
      </MemoryRouter>
    </Provider>,
  )
  await user.click(screen.getByRole('button', { name: /create an account/i }))
  return user
}

describe('Sign-up form', () => {
  it('defaults the phone country code from the device timezone', async () => {
    await openSignUp('Europe/Lisbon')

    expect(screen.getByRole('combobox', { name: /country calling code/i })).toHaveTextContent(
      '+351',
    )
  })

  it('requires a full name and a valid phone number before sending anything', async () => {
    const register = respond('post', '/api/auth/register', session)
    const user = await openSignUp()

    await user.type(screen.getByLabelText(/email address/i), 'ama@example.com')
    await user.type(screen.getByLabelText(/^password/i), 'Correct-Horse-9')
    await user.type(screen.getByLabelText(/phone number/i), '123')
    await user.click(screen.getByRole('button', { name: /create account/i }))

    expect(await screen.findByText(/enter your full name/i)).toBeInTheDocument()
    expect(screen.getByText(/enter a valid phone number/i)).toBeInTheDocument()
    expect(register.count).toBe(0)
  })

  it('sends the full name and the phone number in E.164', async () => {
    const register = respond('post', '/api/auth/register', session)
    const user = await openSignUp()

    await user.type(screen.getByLabelText(/full name/i), 'Ama Mensah')
    await user.type(screen.getByLabelText(/email address/i), 'ama@example.com')
    await user.type(screen.getByLabelText(/^password/i), 'Correct-Horse-9')
    await user.type(screen.getByLabelText(/phone number/i), '024 123 4567')
    await user.click(screen.getByRole('button', { name: /create account/i }))

    await expect.poll(() => register.count).toBe(1)
    expect(await register.calls[0].json()).toEqual({
      email: 'ama@example.com',
      password: 'Correct-Horse-9',
      displayName: 'Ama Mensah',
      phoneNumber: '+233241234567',
    })
  })
})

describe('Entry from the marketing site', () => {
  function openAt(path: string) {
    respond('get', '/api/auth/session', null, 401)
    render(
      <Provider store={setupStore()}>
        <MemoryRouter initialEntries={[path]}>
          <AuthPage timeZone="UTC" />
        </MemoryRouter>
      </Provider>,
    )
  }

  it('opens on account creation with ?mode=register', () => {
    openAt('/login?mode=register')

    expect(screen.getByText(/create your workspace/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/full name/i)).toBeInTheDocument()
  })

  it('opens on sign-in without it', () => {
    openAt('/login')

    expect(screen.getByText(/welcome back/i)).toBeInTheDocument()
  })
})
