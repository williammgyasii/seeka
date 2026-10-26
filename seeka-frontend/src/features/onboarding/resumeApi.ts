import { baseApi, type Resume } from '@/shared/api'

export type ResumeUpload = { file: File; idempotencyKey: string }

export const resumeApi = baseApi.injectEndpoints({
  endpoints: (build) => ({
    getCurrentResume: build.query<Resume | null, void>({
      query: () => ({
        url: 'candidate/resume',
        // No résumé yet is an answer, not an error.
        validateStatus: (response) => response.ok || response.status === 404,
        responseHandler: async (response) => (response.status === 404 ? null : response.json()),
      }),
      providesTags: ['Resume'],
    }),
    uploadResume: build.mutation<Resume, ResumeUpload>({
      query: ({ file, idempotencyKey }) => {
        const body = new FormData()
        body.append('file', file)
        return {
          url: 'candidate/resume',
          method: 'POST',
          body,
          headers: { 'Idempotency-Key': idempotencyKey },
        }
      },
      async onQueryStarted(_, { dispatch, queryFulfilled }) {
        const result = await queryFulfilled.catch(() => null)
        if (result)
          dispatch(resumeApi.util.upsertQueryData('getCurrentResume', undefined, result.data))
      },
    }),
  }),
})

export const { useGetCurrentResumeQuery, useUploadResumeMutation } = resumeApi
