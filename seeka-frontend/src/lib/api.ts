export type Session = {
  userId: string
  email: string
  displayName: string
  workspaceId: string
}

export type PipelineItem = {
  savedJobId: string
  companyName: string
  title: string
  stage: string
  applicationStatus: string
  eligibilityStatus?: string
  matchScore?: number
  updatedAt: string
}

export type CandidateSetup = {
  profile: {
    fullName: string
    countryOfResidence: string
    timeZone: string
    seniority: string
    yearsOfExperience: number
    experienceSummary: string
    skills: string[]
  }
  preferences: {
    targetJobTitles: string[]
    employmentTypes: string[]
    availableWorkingHours: string
    minimumCompensation?: number
    compensationCurrency?: string
  }
}

export type JobDetail = {
  savedJob: {
    id: string
    stage: string
    applicationStatus: string
  }
  job: {
    id: string
    companyName: string
    title: string
    description: string
    originalUrl: string
    locationText?: string
    restrictionText?: string
    employmentType?: string
    salaryMinimum?: number
    salaryMaximum?: number
    salaryCurrency?: string
    lastCheckedAt: string
  }
  eligibility?: {
    status: string
    decisionReason: string
    evidenceExcerpts: string[]
    checkedAt: string
    origin: string
  }
  match?: {
    modelVersion: string
    totalScore: number
    componentsJson: string
    missingRequirements: string[]
  }
  contacts: Array<{
    id: string
    name: string
    title: string
    professionalEmail?: string
    professionalProfileUrl?: string
    verificationStatus: string
    relevanceReason: string
  }>
  drafts: Array<{
    id: string
    channel: string
    subject?: string
    body: string
    isApproved: boolean
  }>
  activities: Array<{
    id: string
    type: string
    description: string
    occurredAt: string
  }>
}

async function request<T>(path: string, options?: RequestInit): Promise<T> {
  const response = await fetch(path, {
    credentials: 'include',
    headers: {
      'Content-Type': 'application/json',
      ...options?.headers,
    },
    ...options,
  })

  if (!response.ok) {
    const problem = await response.json().catch(() => null)
    throw new Error(problem?.title ?? problem?.error ?? `Request failed (${response.status})`)
  }
  if (response.status === 204) return undefined as T
  return response.json() as Promise<T>
}

export const api = {
  session: () => request<Session>('/api/auth/session'),
  register: (body: { email: string; password: string; displayName: string }) =>
    request<Session>('/api/auth/register', {
      method: 'POST',
      body: JSON.stringify(body),
    }),
  login: (body: { email: string; password: string }) =>
    request<Session>('/api/auth/login', {
      method: 'POST',
      body: JSON.stringify(body),
    }),
  logout: () => request<void>('/api/auth/logout', { method: 'POST' }),
  getCandidate: () => request<CandidateSetup>('/api/candidate'),
  saveCandidate: (body: Record<string, unknown>) =>
    request<CandidateSetup>('/api/candidate', {
      method: 'PUT',
      body: JSON.stringify(body),
    }),
  pipeline: () => request<PipelineItem[]>('/api/pipeline'),
  importJob: (body: Record<string, unknown>) =>
    request<JobDetail>('/api/jobs/import', {
      method: 'POST',
      body: JSON.stringify(body),
    }),
  job: (id: string) => request<JobDetail>(`/api/jobs/${id}`),
  updateStage: (id: string, stage: string) =>
    request<void>(`/api/jobs/${id}/stage/${stage}`, { method: 'PUT' }),
  addContact: (id: string, body: Record<string, unknown>) =>
    request<JobDetail['contacts'][number]>(`/api/jobs/${id}/contacts`, {
      method: 'POST',
      body: JSON.stringify(body),
    }),
  generateDrafts: (jobId: string, contactId: string) =>
    request<JobDetail['drafts']>(`/api/jobs/${jobId}/drafts/${contactId}`, {
      method: 'POST',
    }),
  updateDraft: (draftId: string, body: Record<string, unknown>) =>
    request<void>(`/api/drafts/${draftId}`, {
      method: 'PUT',
      body: JSON.stringify(body),
    }),
  recordActivity: (jobId: string, body: Record<string, unknown>) =>
    request<void>(`/api/jobs/${jobId}/activities`, {
      method: 'POST',
      body: JSON.stringify(body),
    }),
}
