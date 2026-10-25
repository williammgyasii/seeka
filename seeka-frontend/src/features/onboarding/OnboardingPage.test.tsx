import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { Provider } from 'react-redux'
import { MemoryRouter } from 'react-router-dom'
import { describe, expect, it } from 'vitest'
import { setupStore } from '@/app/store'
import { respond } from '@/test/server'
import { OnboardingPage } from './OnboardingPage'

function renderSetup(timeZone: string) {
  return render(
    <Provider store={setupStore()}>
      <MemoryRouter>
        <OnboardingPage timeZone={timeZone} />
      </MemoryRouter>
    </Provider>,
  )
}

describe('OnboardingPage', () => {
  it('suggests timezone and residence from the device', () => {
    renderSetup('Europe/Lisbon')

    expect(screen.getByLabelText(/^timezone/i)).toHaveValue('Europe/Lisbon')
    expect(screen.getByRole('combobox', { name: /country of residence/i })).toHaveTextContent(
      'Portugal',
    )
  })

  it('pre-fills no fixed country or GMT hours', () => {
    renderSetup('UTC')

    expect(screen.getByRole('combobox', { name: /country of residence/i })).toHaveTextContent(
      /choose a country/i,
    )
    expect(screen.getByLabelText(/available working hours/i)).toHaveValue('')
  })

  it('blocks saving until a residence is chosen', async () => {
    const save = respond('put', '/api/candidate', {})
    const user = userEvent.setup()
    renderSetup('UTC')

    await user.type(screen.getByLabelText(/full name/i), 'Ama Mensah')
    await user.type(screen.getByLabelText(/skills/i), 'Recruitment, Scheduling')
    await user.type(
      screen.getByLabelText(/experience summary/i),
      'Four years coordinating hiring for a distributed HR team.',
    )
    await user.type(screen.getByLabelText(/available working hours/i), '09:00–17:00 UTC')
    await user.click(screen.getByRole('button', { name: /save and import/i }))

    expect(await screen.findByText(/choose your country of residence/i)).toBeInTheDocument()
    expect(save.count).toBe(0)
  })
})
