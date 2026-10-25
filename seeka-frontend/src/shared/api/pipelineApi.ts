import { baseApi } from './baseApi'
import type { PipelineItem } from './types'

export const pipelineApi = baseApi.injectEndpoints({
  endpoints: (build) => ({
    getPipeline: build.query<PipelineItem[], void>({
      query: () => 'pipeline',
      providesTags: ['Pipeline'],
    }),
  }),
})

export const { useGetPipelineQuery } = pipelineApi
