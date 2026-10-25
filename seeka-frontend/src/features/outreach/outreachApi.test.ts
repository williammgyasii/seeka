import { describe, expect, it, vi } from 'vitest'
import { setupStore } from '@/app/store'
import { baseApi } from '@/shared/api'
import { respond } from '@/test/server'
import { outreachApi } from './outreachApi'

type AppStore = ReturnType<typeof setupStore>

const jobProbe = baseApi.injectEndpoints({
  endpoints: (build) => ({
    probeJob: build.query<unknown, string>({
      query: (savedJobId) => `jobs/${savedJobId}`,
      providesTags: (_result, _error, savedJobId) => [{ type: 'Job', id: savedJobId }],
    }),
  }),
})

const contact = {
  name: 'Kofi Boateng',
  title: 'Recruiter',
  professionalProfileUrl: '',
  professionalEmail: '',
  relevanceReason: 'Hires for this team',
}

const mutations: [string, (store: AppStore) => Promise<unknown>][] = [
  [
    'adding a contact',
    (store) => {
      respond('post', '/api/jobs/job-1/contacts', { id: 'contact-1' }, 201)
      return store.dispatch(
        outreachApi.endpoints.addContact.initiate({ savedJobId: 'job-1', contact }),
      )
    },
  ],
  [
    'generating drafts',
    (store) => {
      respond('post', '/api/jobs/job-1/drafts/contact-1', [])
      return store.dispatch(
        outreachApi.endpoints.generateDrafts.initiate({
          savedJobId: 'job-1',
          contactId: 'contact-1',
        }),
      )
    },
  ],
  [
    'editing a draft',
    (store) => {
      respond('put', '/api/drafts/draft-1')
      return store.dispatch(
        outreachApi.endpoints.updateDraft.initiate({
          savedJobId: 'job-1',
          draftId: 'draft-1',
          changes: { body: 'Edited', isApproved: true },
        }),
      )
    },
  ],
  [
    'recording outreach',
    (store) => {
      respond('post', '/api/jobs/job-1/activities', undefined, 201)
      return store.dispatch(outreachApi.endpoints.recordOutreach.initiate('job-1'))
    },
  ],
]

describe('outreachApi', () => {
  it.each(mutations)('refreshes the job after %s', async (_, mutate) => {
    const store = setupStore()
    const jobReads = respond('get', '/api/jobs/job-1', { savedJob: { id: 'job-1' } })
    store.dispatch(jobProbe.endpoints.probeJob.initiate('job-1'))
    await vi.waitFor(() => expect(jobReads.count).toBe(1))

    await mutate(store)

    await vi.waitFor(() => expect(jobReads.count).toBe(2))
  })

  it('sends a fresh idempotency key with each outreach record', async () => {
    const store = setupStore()
    const records = respond('post', '/api/jobs/job-1/activities', undefined, 201)

    await store.dispatch(outreachApi.endpoints.recordOutreach.initiate('job-1'))
    await store.dispatch(outreachApi.endpoints.recordOutreach.initiate('job-1'))

    const keys = await Promise.all(
      records.calls.map(async (request) => (await request.json()).idempotencyKey),
    )
    expect(keys[0]).toBeTruthy()
    expect(keys[0]).not.toBe(keys[1])
  })
})
