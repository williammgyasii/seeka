import { describe, expect, it, vi } from 'vitest'
import { setupStore } from '@/app/store'
import { pipelineApi } from '@/shared/api'
import { respond } from '@/test/server'
import { jobsApi } from './jobsApi'

const detail = { savedJob: { id: 'job-1' } }

describe('jobsApi', () => {
  it('refreshes the pipeline after importing a job', async () => {
    const store = setupStore()
    const pipelineReads = respond('get', '/api/pipeline', [])
    respond('post', '/api/jobs/import', detail, 201)
    store.dispatch(pipelineApi.endpoints.getPipeline.initiate())
    await vi.waitFor(() => expect(pipelineReads.count).toBe(1))

    await store.dispatch(jobsApi.endpoints.importJob.initiate({ title: 'HR Coordinator' }))

    await vi.waitFor(() => expect(pipelineReads.count).toBe(2))
  })

  it('shows a fresh pipeline when the user returns after importing a job', async () => {
    const store = setupStore()
    const pipelineReads = respond('get', '/api/pipeline', [])
    respond('post', '/api/jobs/import', detail, 201)
    const dashboard = store.dispatch(pipelineApi.endpoints.getPipeline.initiate())
    await vi.waitFor(() => expect(pipelineReads.count).toBe(1))
    dashboard.unsubscribe()

    await store.dispatch(jobsApi.endpoints.importJob.initiate({ title: 'HR Coordinator' }))
    store.dispatch(pipelineApi.endpoints.getPipeline.initiate())

    await vi.waitFor(() => expect(pipelineReads.count).toBe(2))
  })

  it('refreshes the job and the pipeline after a stage change', async () => {
    const store = setupStore()
    const pipelineReads = respond('get', '/api/pipeline', [])
    const jobReads = respond('get', '/api/jobs/job-1', detail)
    const stageChanges = respond('put', '/api/jobs/job-1/stage/Contacted')
    store.dispatch(pipelineApi.endpoints.getPipeline.initiate())
    store.dispatch(jobsApi.endpoints.getJob.initiate('job-1'))
    await vi.waitFor(() => expect(jobReads.count + pipelineReads.count).toBe(2))

    await store.dispatch(
      jobsApi.endpoints.changeStage.initiate({ savedJobId: 'job-1', stage: 'Contacted' }),
    )

    expect(stageChanges.count).toBe(1)
    await vi.waitFor(() => {
      expect(jobReads.count).toBe(2)
      expect(pipelineReads.count).toBe(2)
    })
  })
})
