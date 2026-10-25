import { baseApi, sessionApi, type Session } from '@/shared/api'

export type Credentials = { email: string; password: string }
/** `phoneNumber` is E.164, e.g. +233241234567. */
export type Registration = Credentials & { displayName: string; phoneNumber: string }

async function cacheSession(
  _: unknown,
  { dispatch, queryFulfilled }: { dispatch: (action: unknown) => unknown; queryFulfilled: Promise<{ data: Session }> },
) {
  const result = await queryFulfilled.catch(() => null)
  if (result)
    dispatch(
      sessionApi.util.upsertQueryEntries([
        { endpointName: 'getSession', arg: undefined, value: result.data },
      ]),
    )
}

export const authApi = sessionApi.injectEndpoints({
  endpoints: (build) => ({
    signIn: build.mutation<Session, Credentials>({
      query: (body) => ({ url: 'auth/login', method: 'POST', body }),
      onQueryStarted: cacheSession,
    }),
    register: build.mutation<Session, Registration>({
      query: (body) => ({ url: 'auth/register', method: 'POST', body }),
      onQueryStarted: cacheSession,
    }),
    signOut: build.mutation<void, void>({
      query: () => ({ url: 'auth/logout', method: 'POST' }),
      async onQueryStarted(_, { dispatch, queryFulfilled }) {
        const result = await queryFulfilled.catch(() => null)
        if (result) dispatch(baseApi.util.resetApiState())
      },
    }),
  }),
})

export const { useSignInMutation, useRegisterMutation, useSignOutMutation } = authApi
