import { baseApi } from './baseApi'
import type { Session } from './types'

export const sessionApi = baseApi.injectEndpoints({
  endpoints: (build) => ({
    getSession: build.query<Session, void>({
      query: () => 'auth/session',
      providesTags: ['Session'],
    }),
  }),
})

export const { useGetSessionQuery } = sessionApi
