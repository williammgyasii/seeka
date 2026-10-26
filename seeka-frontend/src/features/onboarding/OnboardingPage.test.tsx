import { fireEvent, render, screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { Provider } from 'react-redux'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { beforeEach, describe, expect, it } from 'vitest'
import { setupStore } from '@/app/store'
import type { CandidateProfile } from '@/shared/api'
import { respond } from '@/test/server'
import { OnboardingPage } from './OnboardingPage'
import { emptyProfile, extracted } from './testData'

function renderSetup(timeZone = 'UTC') {
  return render(
    <Provider store={setupStore()}>
      <MemoryRouter initialEntries={['/onboarding']}>
        <Routes>
          <Route path="/onboarding" element={<OnboardingPage timeZone={timeZone} />} />
          <Route path="/dashboard" element={<h1>Dashboard</h1>} />
        </Routes>
      </MemoryRouter>
    </Provider>,
  )
}

function pdf(name = 'cv.pdf') {
  return new File(['%PDF-1.7'], name, { type: 'application/pdf' })
}

function profile(overrides: Partial<CandidateProfile>): CandidateProfile {
  return { ...emptyProfile, ...overrides }
}

/** The step heading appears once the profile and résumé have loaded. */
async function stepHeading(name: RegExp) {
  return screen.findByRole('heading', { level: 2, name })
}

async function uploadResume(user = userEvent.setup()) {
  await user.upload(await screen.findByLabelText(/résumé \(pdf\)/i), pdf())
  await user.click(screen.getByRole('button', { name: /read my résumé/i }))
  await stepHeading(/about you/i)
  return user
}

describe('OnboardingPage', () => {
  beforeEach(() => {
    respond('get', '/api/candidate/resume', undefined, 404)
    respond('get', '/api/candidate/profile', emptyProfile)
    respond('get', '/api/candidate/job-titles', [
      'human resources manager',
      'human resources officer',
    ])
    respond('get', '/api/candidate/pay-bands', {
      byCurrency: {
        GHS: bands(3_000, 6_000, 10_000, 20_000, 40_000),
        USD: bands(1_000, 2_500, 5_000, 8_000, 12_000),
      },
      byCountry: { GH: 'GHS', UG: 'USD', US: 'USD' },
    })
  })

  describe('résumé upload', () => {
    it('starts with a résumé upload before any step', async () => {
      renderSetup()

      expect(await screen.findByLabelText(/résumé \(pdf\)/i)).toBeInTheDocument()
      expect(screen.queryByLabelText(/full name/i)).not.toBeInTheDocument()
      expect(screen.getByRole('progressbar', { name: /step 1 of 6/i })).toBeInTheDocument()
    })

    it('can be skipped to type the profile by hand', async () => {
      const user = userEvent.setup()
      renderSetup()

      await user.click(await screen.findByRole('button', { name: /skip, i'll type it/i }))

      expect(await stepHeading(/about you/i)).toBeInTheDocument()
      expect(screen.getByLabelText(/full name/i)).toHaveValue('')
    })

    it('fills About you from the résumé and sends an idempotency key', async () => {
      const upload = respond('post', '/api/candidate/resume', extracted)
      renderSetup()

      await uploadResume()

      expect(screen.getByLabelText(/full name/i)).toHaveValue('Ama Mensah')
      expect(screen.getByLabelText(/headline/i)).toHaveValue('HR operations lead')
      expect(screen.getByRole('combobox', { name: /seniority/i })).toHaveTextContent('Senior')
      expect(screen.getByLabelText(/years of experience/i)).toHaveValue(8)
      expect(screen.getByLabelText(/skills/i)).toHaveValue('Recruitment, Payroll')
      expect(screen.getByLabelText(/^language$/i)).toHaveValue('Twi')
      expect(screen.getByLabelText(/link url/i)).toHaveValue('https://linkedin.com/in/ama')
      expect(screen.getByText(/check every step before saving/i)).toBeInTheDocument()
      expect(upload.calls[0].headers.get('Idempotency-Key')).toMatch(/^[0-9a-f-]{36}$/)
    })

    it('reuses the key when the same file is retried, and makes a new one for a new file', async () => {
      const failed = respond('post', '/api/candidate/resume', { title: 'Unavailable' }, 503)
      const user = userEvent.setup()
      renderSetup()
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
      respond('post', '/api/candidate/resume', extracted)
      const user = userEvent.setup()
      renderSetup()

      const zone = await screen.findByText(/drag your résumé here/i)
      fireEvent.dragOver(zone, { dataTransfer: { files: [] } })
      fireEvent.drop(zone, { dataTransfer: { files: [pdf('dropped.pdf')] } })

      expect(screen.getByText('dropped.pdf')).toBeInTheDocument()
      await user.click(screen.getByRole('button', { name: /read my résumé/i }))
      expect(await screen.findByLabelText(/full name/i)).toHaveValue('Ama Mensah')
    })

    it('falls back to empty steps when the PDF has no readable text', async () => {
      respond('post', '/api/candidate/resume', { ...extracted, status: 'Unreadable', draft: null })
      const user = userEvent.setup()
      renderSetup()

      await user.upload(await screen.findByLabelText(/résumé \(pdf\)/i), pdf())
      await user.click(screen.getByRole('button', { name: /read my résumé/i }))

      expect(await screen.findByText(/couldn't read any text/i)).toBeInTheDocument()
      expect(screen.getByLabelText(/full name/i)).toHaveValue('')
    })

    it('shows why a file was rejected', async () => {
      respond(
        'post',
        '/api/candidate/resume',
        { errors: { file: ['Upload your résumé as a PDF.'] } },
        400,
      )
      const user = userEvent.setup()
      renderSetup()

      await user.upload(await screen.findByLabelText(/résumé \(pdf\)/i), pdf())
      await user.click(screen.getByRole('button', { name: /read my résumé/i }))

      expect(await screen.findByText('Upload your résumé as a PDF.')).toBeInTheDocument()
    })
  })

  describe('steps', () => {
    it('reopens at About you, pre-filled, when a résumé is stored and no step is saved', async () => {
      respond('get', '/api/candidate/resume', extracted)
      renderSetup()

      expect(await stepHeading(/about you/i)).toBeInTheDocument()
      expect(screen.getByLabelText(/full name/i)).toHaveValue('Ama Mensah')
      expect(screen.getByText(/cv\.pdf/)).toBeInTheDocument()
    })

    it('resumes at the first unfinished step after a reload', async () => {
      respond('get', '/api/candidate/resume', extracted)
      respond('get', '/api/candidate/profile', profile({ completedSteps: ['about', 'experience'] }))
      renderSetup()

      expect(await stepHeading(/education and certifications/i)).toBeInTheDocument()
      expect(screen.getByRole('progressbar', { name: /step 4 of 6/i })).toBeInTheDocument()
      await userEvent.setup().click(screen.getByRole('button', { name: /university of ghana/i }))
      expect(screen.getByLabelText(/school/i)).toHaveValue('University of Ghana')
      expect(screen.getByLabelText(/^certification$/i)).toHaveValue('SHRM-CP')
    })

    it('saves About you to its own endpoint and moves to Experience, pre-filled from the résumé', async () => {
      respond('get', '/api/candidate/resume', extracted)
      const save = respond(
        'put',
        '/api/candidate/profile/about',
        profile({ completedSteps: ['about'] }),
      )
      const user = userEvent.setup()
      renderSetup('Africa/Accra')
      await stepHeading(/about you/i)

      await user.click(screen.getByRole('button', { name: /save and continue/i }))

      expect(await stepHeading(/^experience$/i)).toBeInTheDocument()
      const body = await save.calls[0].json()
      expect(body).toMatchObject({
        fullName: 'Ama Mensah',
        countryOfResidence: 'Ghana',
        timeZone: 'Africa/Accra',
        skills: ['Recruitment', 'Payroll'],
        languages: [{ language: 'Twi', level: 'Fluent' }],
      })
      await user.click(screen.getByRole('button', { name: /hr manager at acme ltd/i }))
      expect(screen.getByLabelText(/company/i)).toHaveValue('Acme Ltd')
      expect(screen.getByLabelText(/i currently work here/i)).toBeChecked()
      expect(screen.getByLabelText(/highlights/i)).toHaveValue(
        'Ran payroll for 300 staff\nCut hiring time by a third',
      )
    })

    it('shows a saved step as the user saved it, not as the résumé had it', async () => {
      respond('get', '/api/candidate/resume', extracted)
      respond(
        'get',
        '/api/candidate/profile',
        profile({
          completedSteps: ['about', 'experience', 'education', 'projects'],
          experience: [
            {
              company: 'Edited Co',
              title: 'HR Lead',
              location: null,
              startMonth: '2022-01',
              endMonth: null,
              current: true,
              highlights: [],
            },
          ],
        }),
      )
      const user = userEvent.setup()
      renderSetup()
      await stepHeading(/job preferences/i)

      await user.click(screen.getByRole('button', { name: /back/i }))
      await user.click(await screen.findByRole('button', { name: /back/i }))
      await user.click(await screen.findByRole('button', { name: /back/i }))

      expect(await stepHeading(/^experience$/i)).toBeInTheDocument()
      await user.click(screen.getByRole('button', { name: /hr lead at edited co/i }))
      expect(screen.getByLabelText(/company/i)).toHaveValue('Edited Co')
    })

    it('shows each job closed, summarised by title, company and dates', async () => {
      respond('get', '/api/candidate/resume', extracted)
      respond('get', '/api/candidate/profile', profile({ completedSteps: ['about'] }))
      renderSetup()
      await stepHeading(/^experience$/i)

      const job = screen.getByRole('button', { name: /hr manager at acme ltd/i })
      expect(job).toHaveAttribute('aria-expanded', 'false')
      expect(job).toHaveTextContent('Jun 2021 – Present')
      expect(screen.queryByLabelText(/company/i)).not.toBeInTheDocument()
    })

    it('opens a job as soon as it is added', async () => {
      respond('get', '/api/candidate/resume', extracted)
      respond('get', '/api/candidate/profile', profile({ completedSteps: ['about'] }))
      const user = userEvent.setup()
      renderSetup()
      await stepHeading(/^experience$/i)

      await user.click(screen.getByRole('button', { name: /add a job/i }))

      expect(screen.getByRole('button', { name: /new job/i })).toHaveAttribute('aria-expanded', 'true')
      expect(screen.getByLabelText(/company/i)).toHaveValue('')
    })

    it('adds and removes jobs', async () => {
      respond('get', '/api/candidate/profile', profile({ completedSteps: ['about'] }))
      const save = respond(
        'put',
        '/api/candidate/profile/experience',
        profile({ completedSteps: ['about', 'experience'] }),
      )
      const user = userEvent.setup()
      renderSetup()
      await stepHeading(/^experience$/i)

      await user.click(screen.getByRole('button', { name: /add a job/i }))
      await user.type(screen.getByLabelText(/company/i), 'Beta')
      await user.type(screen.getByLabelText(/job title/i), 'Analyst')
      await user.click(screen.getByRole('button', { name: /add a job/i }))
      const second = screen.getAllByRole('group', { name: /job/i })[1]
      await user.click(within(second).getByRole('button', { name: /remove/i }))
      await user.click(screen.getByRole('button', { name: /save and continue/i }))

      await stepHeading(/education and certifications/i)
      expect(await save.calls[0].json()).toEqual({
        experience: [
          {
            company: 'Beta',
            title: 'Analyst',
            location: null,
            startMonth: null,
            endMonth: null,
            current: false,
            highlights: [],
          },
        ],
      })
    })

    it('shows a server error under the field it names', async () => {
      respond('get', '/api/candidate/resume', extracted)
      respond('get', '/api/candidate/profile', profile({ completedSteps: ['about'] }))
      respond(
        'put',
        '/api/candidate/profile/experience',
        { errors: { 'experience[0].startMonth': ['Use year and month, like 2021-06.'] } },
        400,
      )
      const user = userEvent.setup()
      renderSetup()
      await stepHeading(/^experience$/i)

      await user.click(screen.getByRole('button', { name: /save and continue/i }))

      const job = await screen.findByRole('group', { name: /job 1/i })
      expect(await within(job).findByText('Use year and month, like 2021-06.')).toBeInTheDocument()
      expect(screen.getByRole('button', { name: /needs a fix/i })).toHaveAttribute(
        'aria-expanded',
        'true',
      )
      expect(screen.getByRole('heading', { level: 2, name: /^experience$/i })).toBeInTheDocument()
    })

    it('Skip saves an optional step empty', async () => {
      respond('get', '/api/candidate/resume', extracted)
      respond(
        'get',
        '/api/candidate/profile',
        profile({ completedSteps: ['about', 'experience', 'education'] }),
      )
      const save = respond(
        'put',
        '/api/candidate/profile/projects',
        profile({ completedSteps: ['about', 'experience', 'education', 'projects'] }),
      )
      const user = userEvent.setup()
      renderSetup()
      await stepHeading(/projects and volunteering/i)

      await user.click(screen.getByRole('button', { name: /skip this step/i }))

      expect(await stepHeading(/job preferences/i)).toBeInTheDocument()
      expect(await save.calls[0].json()).toEqual({ projects: [], volunteering: [] })
    })

    it('Finish saves preferences, finishes onboarding, and opens the dashboard', async () => {
      respond(
        'get',
        '/api/candidate/profile',
        profile({ completedSteps: ['about', 'experience', 'education', 'projects'] }),
      )
      const preferences = respond(
        'put',
        '/api/candidate/profile/preferences',
        profile({ completedSteps: ['about', 'experience', 'education', 'projects', 'preferences'] }),
      )
      const complete = respond('post', '/api/candidate/onboarding/complete')
      const user = userEvent.setup()
      renderSetup()
      await stepHeading(/job preferences/i)

      await chooseTitle(user, 'hr', 'human resources manager')
      await user.click(screen.getByRole('button', { name: /finish/i }))

      expect(await screen.findByRole('heading', { name: /dashboard/i })).toBeInTheDocument()
      expect(await preferences.calls[0].json()).toMatchObject({
        targetJobTitles: ['human resources manager'],
      })
      expect(complete.count).toBe(1)
    })
  })

  describe('About you', () => {
    it('suggests timezone and residence from the device', async () => {
      const user = userEvent.setup()
      renderSetup('Europe/Lisbon')
      await user.click(await screen.findByRole('button', { name: /skip, i'll type it/i }))

      expect(await screen.findByRole('combobox', { name: /^timezone/i })).toHaveTextContent(
        'Europe/Lisbon',
      )
      expect(screen.getByRole('combobox', { name: /country of residence/i })).toHaveTextContent(
        'Portugal',
      )
    })

    it('sets and locks the timezone for a country with one timezone', async () => {
      const user = userEvent.setup()
      renderSetup('Africa/Accra')
      await user.click(await screen.findByRole('button', { name: /skip, i'll type it/i }))

      const timeZone = await screen.findByRole('combobox', { name: /^timezone/i })
      expect(timeZone).toHaveTextContent('Africa/Accra')
      expect(timeZone).toBeDisabled()
    })

    it("offers only the country's timezones, keeping the device's", async () => {
      const user = userEvent.setup()
      renderSetup('America/Chicago')
      await user.click(await screen.findByRole('button', { name: /skip, i'll type it/i }))

      const timeZone = await screen.findByRole('combobox', { name: /^timezone/i })
      expect(timeZone).toHaveTextContent('America/Chicago')
      await user.click(timeZone)
      expect(screen.getByRole('option', { name: 'America/New_York' })).toBeInTheDocument()
      expect(screen.queryByRole('option', { name: 'Africa/Accra' })).not.toBeInTheDocument()
    })

    it('moves the timezone when the country changes', async () => {
      const user = userEvent.setup()
      renderSetup('Europe/Lisbon')
      await user.click(await screen.findByRole('button', { name: /skip, i'll type it/i }))

      await user.click(await screen.findByRole('combobox', { name: /country of residence/i }))
      await user.click(screen.getByRole('option', { name: 'Ghana' }))

      expect(screen.getByRole('combobox', { name: /^timezone/i })).toHaveTextContent('Africa/Accra')
    })

    it('pre-fills no fixed country', async () => {
      const user = userEvent.setup()
      renderSetup()
      await user.click(await screen.findByRole('button', { name: /skip, i'll type it/i }))

      expect(
        await screen.findByRole('combobox', { name: /country of residence/i }),
      ).toHaveTextContent(/choose a country/i)
    })

    it('asks for seniority as a level from a fixed list', async () => {
      const user = userEvent.setup()
      renderSetup()
      await user.click(await screen.findByRole('button', { name: /skip, i'll type it/i }))

      expect(await screen.findByRole('combobox', { name: /seniority/i })).toHaveTextContent(
        /choose a level/i,
      )
      expect(
        Array.from(document.querySelectorAll('select option'), (option) => option.textContent),
      ).toEqual(
        expect.arrayContaining([
          'Entry',
          'Junior',
          'Mid-level',
          'Senior',
          'Lead',
          'Manager',
          'Director',
          'Executive',
        ]),
      )
    })

    it('blocks saving until a residence is chosen', async () => {
      const save = respond('put', '/api/candidate/profile/about', emptyProfile)
      const user = userEvent.setup()
      renderSetup()
      await user.click(await screen.findByRole('button', { name: /skip, i'll type it/i }))

      await user.type(await screen.findByLabelText(/full name/i), 'Ama Mensah')
      await user.click(screen.getByRole('button', { name: /save and continue/i }))

      expect(await screen.findByText(/choose your country of residence/i)).toBeInTheDocument()
      expect(save.count).toBe(0)
    })
  })

  describe('Job preferences', () => {
    async function openPreferences(countryOfResidence = 'Ghana') {
      respond(
        'get',
        '/api/candidate/profile',
        profile({
          about: { ...emptyProfile.about, countryOfResidence },
          completedSteps: ['about', 'experience', 'education', 'projects'],
        }),
      )
      const save = respond('put', '/api/candidate/profile/preferences', emptyProfile)
      respond('post', '/api/candidate/onboarding/complete')
      const user = userEvent.setup()
      renderSetup()
      await stepHeading(/job preferences/i)
      return { user, save }
    }

    it('finds titles from the list, shows each chosen one, and can remove it', async () => {
      const { user } = await openPreferences()

      await chooseTitle(user, 'hr', 'human resources manager')
      await chooseTitle(user, 'hr', 'human resources officer')
      await user.click(screen.getByRole('button', { name: /remove human resources officer/i }))

      expect(screen.getByText('human resources manager')).toBeInTheDocument()
      expect(screen.queryByText('human resources officer')).not.toBeInTheDocument()
    })

    it('picks a title with the keyboard, and reopens the list after Escape', async () => {
      const { user } = await openPreferences()
      const search = screen.getByRole('combobox', { name: /target job titles/i })

      await user.type(search, 'hr')
      await screen.findByRole('option', { name: 'human resources manager' })
      await user.keyboard('{Escape}')
      expect(search).toHaveAttribute('aria-expanded', 'false')
      await user.type(search, ' ')
      await user.keyboard('{ArrowDown}{Enter}')

      expect(screen.getByRole('button', { name: /remove human resources officer/i })).toBeInTheDocument()
    })

    it('asks for a title before finishing', async () => {
      const { user, save } = await openPreferences()

      await user.click(screen.getByRole('button', { name: /finish/i }))

      expect(await screen.findByText(/add at least one role/i)).toBeInTheDocument()
      expect(save.count).toBe(0)
    })

    it('saves employment types and a monthly pay band in the country currency', async () => {
      const { user, save } = await openPreferences('Ghana')
      await chooseTitle(user, 'hr', 'human resources manager')

      await user.click(screen.getByRole('checkbox', { name: 'Full-time' }))
      await user.click(screen.getByRole('checkbox', { name: 'Contract' }))
      expect(screen.getByRole('combobox', { name: /currency/i })).toHaveTextContent('GHS')
      await choose(user, /monthly pay/i, 'GHS 6,000 – 10,000')
      await user.click(screen.getByRole('button', { name: /finish/i }))

      await waitFor(() => expect(save.count).toBe(1))
      expect(await save.calls[0].json()).toEqual({
        targetJobTitles: ['human resources manager'],
        employmentTypes: ['Full-time', 'Contract'],
        minimumCompensation: 6000,
        maximumCompensation: 10000,
        compensationCurrency: 'GHS',
      })
    })

    it('does not ask for working hours', async () => {
      await openPreferences()

      expect(screen.queryByRole('combobox', { name: /start time/i })).not.toBeInTheDocument()
      expect(screen.queryByRole('combobox', { name: /end time/i })).not.toBeInTheDocument()
    })

    it('offers the bands as choices, lowest to an open top band', async () => {
      const { user } = await openPreferences('Ghana')

      await user.click(screen.getByRole('combobox', { name: /monthly pay/i }))

      expect(screen.getAllByRole('option').map((option) => option.textContent)).toEqual([
        'Under GHS 3,000',
        'GHS 3,000 – 6,000',
        'GHS 6,000 – 10,000',
        'GHS 10,000 – 20,000',
        'GHS 20,000 – 40,000',
        'GHS 40,000+',
      ])
    })

    it('saves the top band with no maximum', async () => {
      const { user, save } = await openPreferences('Ghana')
      await chooseTitle(user, 'hr', 'human resources manager')

      await choose(user, /monthly pay/i, 'GHS 40,000+')
      await user.click(screen.getByRole('button', { name: /finish/i }))

      await waitFor(() => expect(save.count).toBe(1))
      expect(await save.calls[0].json()).toMatchObject({
        minimumCompensation: 40000,
        maximumCompensation: null,
        compensationCurrency: 'GHS',
      })
    })

    it('switches to the other currency bands and clears the chosen band', async () => {
      const { user, save } = await openPreferences('Ghana')
      await chooseTitle(user, 'hr', 'human resources manager')
      await choose(user, /monthly pay/i, 'GHS 6,000 – 10,000')

      await choose(user, /currency/i, /USD/)
      expect(screen.getByRole('combobox', { name: /monthly pay/i })).not.toHaveTextContent('GHS')
      await choose(user, /monthly pay/i, 'USD 2,500 – 5,000')
      await user.click(screen.getByRole('button', { name: /finish/i }))

      await waitFor(() => expect(save.count).toBe(1))
      expect(await save.calls[0].json()).toMatchObject({
        minimumCompensation: 2500,
        maximumCompensation: 5000,
        compensationCurrency: 'USD',
      })
    })

    it('defaults to USD for a country without its own bands', async () => {
      await openPreferences('Uganda')

      expect(screen.getByRole('combobox', { name: /currency/i })).toHaveTextContent('USD')
    })

    it('leaves pay out when no band is chosen', async () => {
      const { user, save } = await openPreferences('Ghana')
      await chooseTitle(user, 'hr', 'human resources manager')

      await user.click(screen.getByRole('button', { name: /finish/i }))

      await waitFor(() => expect(save.count).toBe(1))
      expect(await save.calls[0].json()).toMatchObject({
        minimumCompensation: null,
        maximumCompensation: null,
        compensationCurrency: null,
      })
    })
  })
})

async function chooseTitle(user: ReturnType<typeof userEvent.setup>, query: string, title: string) {
  const search = screen.getByRole('combobox', { name: /target job titles/i })
  await user.clear(search)
  await user.type(search, query)
  await user.click(await screen.findByRole('option', { name: title }))
}

function bands(...edges: number[]) {
  return [0, ...edges].map((minimum, i) => ({ minimum, maximum: edges[i] ?? null }))
}

async function choose(user: ReturnType<typeof userEvent.setup>, field: RegExp, option: string | RegExp) {
  await user.click(screen.getByRole('combobox', { name: field }))
  await user.click(screen.getByRole('option', { name: option }))
}
