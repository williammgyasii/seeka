import { render, screen } from '@testing-library/react'
import { Provider } from 'react-redux'
import { MemoryRouter } from 'react-router-dom'
import { describe, expect, it } from 'vitest'
import { respond } from '@/test/server'
import App from './App'
import { setupStore } from './store'

function renderApp(path = '/') {
  return render(
    <Provider store={setupStore()}>
      <MemoryRouter initialEntries={[path]}>
        <App />
      </MemoryRouter>
    </Provider>,
  )
}

describe('Seeka workflow', () => {
  it('presents honest account access for a new visitor', () => {
    renderApp('/login')

    expect(
      screen.getByRole('heading', { name: /find work that can actually hire you/i }),
    ).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /sign in/i })).toBeInTheDocument()
  })

  it('provides a manual real-job import screen', async () => {
    respond('get', '/api/auth/session', {
      userId: 'u1',
      email: 'ama@example.com',
      displayName: 'Ama',
      workspaceId: 'w1',
      onboardingComplete: true,
    })
    renderApp('/jobs/import')

    expect(
      await screen.findByRole('heading', { name: /import a real job/i }),
    ).toBeInTheDocument()
    expect(screen.getByLabelText(/original job url/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/job description/i)).toBeInTheDocument()
  })
})
