import { baseApi, type Contact, type Draft } from '@/shared/api'

export type ContactValues = {
  name: string
  title: string
  professionalProfileUrl: string
  professionalEmail: string
  relevanceReason: string
}

export type DraftChanges = { subject?: string; body: string; isApproved: boolean }

type JobScoped = { savedJobId: string }

const refreshJob = (_result: unknown, _error: unknown, { savedJobId }: JobScoped) => [
  { type: 'Job' as const, id: savedJobId },
]

export const outreachApi = baseApi.injectEndpoints({
  endpoints: (build) => ({
    addContact: build.mutation<Contact, JobScoped & { contact: ContactValues }>({
      query: ({ savedJobId, contact }) => ({
        url: `jobs/${savedJobId}/contacts`,
        method: 'POST',
        body: contact,
      }),
      invalidatesTags: refreshJob,
    }),
    generateDrafts: build.mutation<Draft[], JobScoped & { contactId: string }>({
      query: ({ savedJobId, contactId }) => ({
        url: `jobs/${savedJobId}/drafts/${contactId}`,
        method: 'POST',
      }),
      invalidatesTags: refreshJob,
    }),
    updateDraft: build.mutation<void, JobScoped & { draftId: string; changes: DraftChanges }>({
      query: ({ draftId, changes }) => ({ url: `drafts/${draftId}`, method: 'PUT', body: changes }),
      invalidatesTags: refreshJob,
    }),
    recordOutreach: build.mutation<void, string>({
      query: (savedJobId) => ({
        url: `jobs/${savedJobId}/activities`,
        method: 'POST',
        body: {
          type: 'outreach_copied',
          description: 'Recorded outreach as copied or sent manually.',
          idempotencyKey: crypto.randomUUID(),
        },
      }),
      invalidatesTags: (_result, _error, savedJobId) => [{ type: 'Job', id: savedJobId }],
    }),
  }),
})

export const {
  useAddContactMutation,
  useGenerateDraftsMutation,
  useUpdateDraftMutation,
  useRecordOutreachMutation,
} = outreachApi
