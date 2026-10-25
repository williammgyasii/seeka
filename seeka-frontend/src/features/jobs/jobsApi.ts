import { baseApi, type JobDetail } from '@/shared/api'

export const pipelineStages = [
  'Saved',
  'Researching',
  'ReadyToContact',
  'Contacted',
  'Replied',
  'Interviewing',
  'Offer',
  'Closed',
]

export type StageChange = { savedJobId: string; stage: string }

export const jobsApi = baseApi.injectEndpoints({
  endpoints: (build) => ({
    getJob: build.query<JobDetail, string>({
      query: (savedJobId) => `jobs/${savedJobId}`,
      providesTags: (_result, _error, savedJobId) => [{ type: 'Job', id: savedJobId }],
    }),
    importJob: build.mutation<JobDetail, Record<string, unknown>>({
      query: (body) => ({ url: 'jobs/import', method: 'POST', body }),
      invalidatesTags: ['Pipeline'],
    }),
    changeStage: build.mutation<void, StageChange>({
      query: ({ savedJobId, stage }) => ({ url: `jobs/${savedJobId}/stage/${stage}`, method: 'PUT' }),
      invalidatesTags: (_result, _error, { savedJobId }) => [
        { type: 'Job', id: savedJobId },
        'Pipeline',
      ],
    }),
  }),
})

export const { useGetJobQuery, useImportJobMutation, useChangeStageMutation } = jobsApi
