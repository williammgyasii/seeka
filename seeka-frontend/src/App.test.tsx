import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { describe, expect, it } from 'vitest'
import App from './App'

function renderApp(path = '/') {
  return render(
    <QueryClientProvider client={new QueryClient()}>
      <MemoryRouter initialEntries={[path]}>
        <App />
      </MemoryRouter>
    </QueryClientProvider>,
  )
}

describe('Seeka workflow', () => {
  it('presents honest account access for a new visitor', () => {
    renderApp('/login')

    expect(
      screen.getByRole('heading', { name: /find work that can hire you/i }),
    ).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /sign in/i })).toBeInTheDocument()
  })

  it('provides a manual real-job import screen', () => {
    renderApp('/jobs/import')

    expect(
      screen.getByRole('heading', { name: /import a real job/i }),
    ).toBeInTheDocument()
    expect(screen.getByLabelText(/original job url/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/job description/i)).toBeInTheDocument()
  })
})
