import {
  baseApi,
  sessionApi,
  type CandidateProfile,
  type OnboardingStep,
  type PayBandOptions,
} from '@/shared/api'

export type StepSave = { step: OnboardingStep; body: unknown }

export const onboardingApi = baseApi.injectEndpoints({
  endpoints: (build) => ({
    getProfile: build.query<CandidateProfile, void>({
      query: () => 'candidate/profile',
      providesTags: ['Profile'],
    }),
    saveStep: build.mutation<CandidateProfile, StepSave>({
      query: ({ step, body }) => ({ url: `candidate/profile/${step}`, method: 'PUT', body }),
      // The response is the whole profile, so it replaces the cache instead of a refetch.
      async onQueryStarted(_, { dispatch, queryFulfilled }) {
        const result = await queryFulfilled.catch(() => null)
        if (result)
          dispatch(onboardingApi.util.upsertQueryData('getProfile', undefined, result.data))
      },
    }),
    searchJobTitles: build.query<string[], string>({
      query: (q) => ({ url: 'candidate/job-titles', params: { q } }),
    }),
    getPayBands: build.query<PayBandOptions, void>({
      query: () => 'candidate/pay-bands',
    }),
    completeOnboarding: build.mutation<void, void>({
      query: () => ({ url: 'candidate/onboarding/complete', method: 'POST' }),
      invalidatesTags: (_result, error) => (error ? [] : ['Session', 'Profile']),
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

export const {
  useGetProfileQuery,
  useSaveStepMutation,
  useSearchJobTitlesQuery,
  useGetPayBandsQuery,
  useCompleteOnboardingMutation,
} = onboardingApi
