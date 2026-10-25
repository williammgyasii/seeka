import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react'

export const baseApi = createApi({
  reducerPath: 'api',
  baseQuery: fetchBaseQuery({
    baseUrl: new URL('/api', window.location.origin).href,
    credentials: 'include',
  }),
  tagTypes: ['Session', 'Pipeline', 'Job'],
  refetchOnMountOrArgChange: 30,
  endpoints: () => ({}),
})
