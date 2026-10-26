export type Session = {
  userId: string
  email: string
  displayName: string
  workspaceId: string
  /** True once the workspace has a saved candidate profile. */
  onboardingComplete: boolean
  /** The API refuses every workspace endpoint until this is true. */
  emailVerified: boolean
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

/** Facts read from a résumé; anything it does not state is null or empty. */
export type ResumeDraft = {
  fullName: string | null
  seniority: string | null
  yearsOfExperience: number | null
  experienceSummary: string | null
  skills: string[]
  targetJobTitles: string[]
}

export type Resume = {
  fileName: string
  uploadedAt: string
  status: 'Extracted' | 'Unreadable' | 'ExtractionFailed'
  draft: ResumeDraft | null
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

export type Contact = JobDetail['contacts'][number]
export type Draft = JobDetail['drafts'][number]
