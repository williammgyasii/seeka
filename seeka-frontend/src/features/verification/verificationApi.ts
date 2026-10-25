import { baseApi, sessionApi } from '@/shared/api'

export type EmailConfirmation = { userId: string; token: string }

export const verificationApi = baseApi.injectEndpoints({
  endpoints: (build) => ({
    resendVerificationEmail: build.mutation<void, void>({
      query: () => ({ url: 'auth/verification-email', method: 'POST' }),
    }),
    changeEmail: build.mutation<void, { email: string }>({
      query: (body) => ({ url: 'auth/email', method: 'PUT', body }),
      invalidatesTags: ['Session'],
    }),
    verifyEmail: build.mutation<void, EmailConfirmation>({
      query: (body) => ({ url: 'auth/verify-email', method: 'POST', body }),
      invalidatesTags: ['Session'],
      // The link may be opened while someone else is signed in on this browser,
      // so only that same user's cached session is marked verified.
      async onQueryStarted({ userId }, { dispatch, queryFulfilled }) {
        const result = await queryFulfilled.catch(() => null)
        if (result)
          dispatch(
            sessionApi.util.updateQueryData('getSession', undefined, (session) => {
              if (session.userId === userId) session.emailVerified = true
            }),
          )
      },
    }),
  }),
})

export const {
  useResendVerificationEmailMutation,
  useChangeEmailMutation,
  useVerifyEmailMutation,
} = verificationApi
