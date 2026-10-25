import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { Provider } from 'react-redux'
import { MemoryRouter } from 'react-router-dom'
import { describe, expect, it } from 'vitest'
import App from '@/app/App'
import { setupStore } from '@/app/store'
import { respond } from '@/test/server'

const unverified = {
  userId: 'u1',
  email: 'ama@exmaple.com',
  displayName: 'Ama Mensah',
  workspaceId: 'w1',
  onboardingComplete: false,
  emailVerified: false,
}

async function openInbox() {
  respond('get', '/api/auth/session', unverified)
  const user = userEvent.setup()
  render(
    <Provider store={setupStore()}>
      <MemoryRouter initialEntries={['/verify-email']}>
        <App />
      </MemoryRouter>
    </Provider>,
  )
  await screen.findByRole('heading', { name: /check your inbox/i })
  return user
}

describe('Check-your-inbox page', () => {
  it('sends a new link on request and says so', async () => {
    const resend = respond('post', '/api/auth/verification-email', undefined, 202)
    const user = await openInbox()

    await user.click(screen.getByRole('button', { name: /send a new link/i }))

    expect(await screen.findByText(/we sent a new link/i)).toBeInTheDocument()
    expect(resend.count).toBe(1)
  })

  it('explains when the email could not be sent', async () => {
    respond(
      'post',
      '/api/auth/verification-email',
      { title: "We couldn't send the email. Try again in a minute." },
      503,
    )
    const user = await openInbox()

    await user.click(screen.getByRole('button', { name: /send a new link/i }))

    expect(await screen.findByText(/couldn't send the email/i)).toBeInTheDocument()
  })

  it('corrects a mistyped address and shows the new one', async () => {
    const change = respond('put', '/api/auth/email', undefined, 202)
    const user = await openInbox()

    await user.click(screen.getByRole('button', { name: /wrong address/i }))
    respond('get', '/api/auth/session', { ...unverified, email: 'ama@example.com' })
    await user.type(screen.getByLabelText(/new email address/i), 'ama@example.com')
    await user.click(screen.getByRole('button', { name: /update address/i }))

    expect(
      await screen.findByText('ama@example.com', { selector: 'strong' }),
    ).toBeInTheDocument()
    expect(await change.calls[0].json()).toEqual({ email: 'ama@example.com' })
  })

  it('shows the server error for an address already in use', async () => {
    respond(
      'put',
      '/api/auth/email',
      { errors: { email: ['Another account already uses this email.'] } },
      400,
    )
    const user = await openInbox()

    await user.click(screen.getByRole('button', { name: /wrong address/i }))
    await user.type(screen.getByLabelText(/new email address/i), 'taken@example.com')
    await user.click(screen.getByRole('button', { name: /update address/i }))

    expect(await screen.findByText(/another account already uses this email/i)).toBeInTheDocument()
    expect(screen.getByText('ama@exmaple.com', { selector: 'strong' })).toBeInTheDocument()
  })
})
