import { baseApi, sessionApi, type CandidateSetup } from '@/shared/api'

export const onboardingApi = baseApi.injectEndpoints({
  endpoints: (build) => ({
    saveCandidate: build.mutation<CandidateSetup, Record<string, unknown>>({
      query: (body) => ({ url: 'candidate', method: 'PUT', body }),
      invalidatesTags: ['Session'],
      // Patch the cached flag at once, so the route guard doesn't bounce the user
      // back to onboarding while the session refetch is still in flight.
      async onQueryStarted(_, { dispatch, queryFulfilled }) {
        const result = await queryFulfilled.catch(() => null)
        if (result)
          dispatch(
            sessionApi.util.updateQueryData('getSession', undefined, (session) => {
              session.onboardingComplete = true
            }),
          )
      },
    }),
  }),
})

export const { useSaveCandidateMutation } = onboardingApi
