import { fireEvent, render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { Provider } from 'react-redux'
import { MemoryRouter } from 'react-router-dom'
import { beforeEach, describe, expect, it } from 'vitest'
import { setupStore } from '@/app/store'
import { respond } from '@/test/server'
import { OnboardingPage } from './OnboardingPage'

const extracted = {
  fileName: 'cv.pdf',
  uploadedAt: '2026-10-25T12:00:00Z',
  status: 'Extracted',
  draft: {
    fullName: 'Ama Mensah',
    seniority: 'Senior',
    yearsOfExperience: 8,
    experienceSummary: 'Eight years leading HR operations for distributed teams.',
    skills: ['Recruitment', 'Payroll'],
    targetJobTitles: ['HR Manager'],
  },
}

function renderSetup(timeZone: string) {
  return render(
    <Provider store={setupStore()}>
      <MemoryRouter>
        <OnboardingPage timeZone={timeZone} />
      </MemoryRouter>
    </Provider>,
  )
}

function pdf(name = 'cv.pdf') {
  return new File(['%PDF-1.7'], name, { type: 'application/pdf' })
}

async function skipToForm(user = userEvent.setup()) {
  await user.click(await screen.findByRole('button', { name: /skip/i }))
  return user
}

describe('OnboardingPage', () => {
  beforeEach(() => {
    respond('get', '/api/candidate/resume', undefined, 404)
  })

  describe('résumé upload', () => {
    it('starts with a résumé upload before the form', async () => {
      renderSetup('UTC')

      expect(await screen.findByLabelText(/résumé \(pdf\)/i)).toBeInTheDocument()
      expect(screen.queryByLabelText(/full name/i)).not.toBeInTheDocument()
    })

    it('can be skipped to type the profile by hand', async () => {
      renderSetup('UTC')

      await skipToForm()

      expect(screen.getByLabelText(/full name/i)).toHaveValue('')
    })

    it('fills the form from the résumé and sends an idempotency key', async () => {
      const upload = respond('post', '/api/candidate/resume', extracted)
      const user = userEvent.setup()
      renderSetup('UTC')

      await user.upload(await screen.findByLabelText(/résumé \(pdf\)/i), pdf())
      await user.click(screen.getByRole('button', { name: /read my résumé/i }))

      expect(await screen.findByLabelText(/full name/i)).toHaveValue('Ama Mensah')
      expect(screen.getByLabelText(/seniority/i)).toHaveValue('Senior')
      expect(screen.getByLabelText(/years of experience/i)).toHaveValue(8)
      expect(screen.getByLabelText(/skills/i)).toHaveValue('Recruitment, Payroll')
      expect(screen.getByLabelText(/target job titles/i)).toHaveValue('HR Manager')
      expect(screen.getByText(/check every field before saving/i)).toBeInTheDocument()
      expect(upload.calls[0].headers.get('Idempotency-Key')).toMatch(/^[0-9a-f-]{36}$/)
    })

    it('reuses the key when the same file is retried, and makes a new one for a new file', async () => {
      const failed = respond('post', '/api/candidate/resume', { title: 'Unavailable' }, 503)
      const user = userEvent.setup()
      renderSetup('UTC')
      const input = await screen.findByLabelText(/résumé \(pdf\)/i)

      await user.upload(input, pdf())
      await user.click(screen.getByRole('button', { name: /read my résumé/i }))
      await screen.findByText(/unavailable/i)
      await user.click(screen.getByRole('button', { name: /read my résumé/i }))
      await user.upload(input, pdf('other.pdf'))
      await user.click(screen.getByRole('button', { name: /read my résumé/i }))
      await screen.findAllByText(/unavailable/i)

      const keys = failed.calls.map((call) => call.headers.get('Idempotency-Key'))
      expect(keys).toHaveLength(3)
      expect(keys[1]).toBe(keys[0])
      expect(keys[2]).not.toBe(keys[0])
    })

    it('accepts a résumé dragged onto the drop zone', async () => {
      const upload = respond('post', '/api/candidate/resume', extracted)
      const user = userEvent.setup()
      renderSetup('UTC')

      const zone = await screen.findByText(/drag your résumé here/i)
      fireEvent.dragOver(zone, { dataTransfer: { files: [] } })
      fireEvent.drop(zone, { dataTransfer: { files: [pdf('dropped.pdf')] } })

      expect(screen.getByText('dropped.pdf')).toBeInTheDocument()
      await user.click(screen.getByRole('button', { name: /read my résumé/i }))
      expect(await screen.findByLabelText(/full name/i)).toHaveValue('Ama Mensah')
      expect(upload.calls[0].headers.get('Idempotency-Key')).toMatch(/^[0-9a-f-]{36}$/)
    })

    it('falls back to the empty form when the PDF has no readable text', async () => {
      respond('post', '/api/candidate/resume', { ...extracted, status: 'Unreadable', draft: null })
      const user = userEvent.setup()
      renderSetup('UTC')

      await user.upload(await screen.findByLabelText(/résumé \(pdf\)/i), pdf())
      await user.click(screen.getByRole('button', { name: /read my résumé/i }))

      expect(await screen.findByText(/couldn't read any text/i)).toBeInTheDocument()
      expect(screen.getByLabelText(/full name/i)).toHaveValue('')
    })

    it('refills the form from a stored draft when onboarding is reopened', async () => {
      respond('get', '/api/candidate/resume', extracted)
      renderSetup('UTC')

      expect(await screen.findByLabelText(/full name/i)).toHaveValue('Ama Mensah')
      expect(screen.getByText(/cv\.pdf/)).toBeInTheDocument()
    })

    it('shows why a file was rejected', async () => {
      respond(
        'post',
        '/api/candidate/resume',
        { errors: { file: ['Upload your résumé as a PDF.'] } },
        400,
      )
      const user = userEvent.setup()
      renderSetup('UTC')

      await user.upload(await screen.findByLabelText(/résumé \(pdf\)/i), pdf())
      await user.click(screen.getByRole('button', { name: /read my résumé/i }))

      expect(await screen.findByText('Upload your résumé as a PDF.')).toBeInTheDocument()
    })
  })

  it('suggests timezone and residence from the device', async () => {
    renderSetup('Europe/Lisbon')
    await skipToForm()

    expect(screen.getByLabelText(/^timezone/i)).toHaveValue('Europe/Lisbon')
    expect(screen.getByRole('combobox', { name: /country of residence/i })).toHaveTextContent(
      'Portugal',
    )
  })

  it('pre-fills no fixed country or GMT hours', async () => {
    renderSetup('UTC')
    await skipToForm()

    expect(screen.getByRole('combobox', { name: /country of residence/i })).toHaveTextContent(
      /choose a country/i,
    )
    expect(screen.getByLabelText(/available working hours/i)).toHaveValue('')
  })

  it('blocks saving until a residence is chosen', async () => {
    const save = respond('put', '/api/candidate', {})
    renderSetup('UTC')
    const user = await skipToForm()

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
