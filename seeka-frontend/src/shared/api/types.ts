export type Session = {
  userId: string
  email: string
  displayName: string
  workspaceId: string
  /** True once the user finishes onboarding, not when the first step saves. */
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

/** Months are `YYYY-MM`; a current role has no end month. */
export type WorkExperience = {
  company: string
  title: string
  location: string | null
  startMonth: string | null
  endMonth: string | null
  current: boolean
  highlights: string[]
}

export type Education = {
  school: string
  degree: string | null
  fieldOfStudy: string | null
  startYear: number | null
  endYear: number | null
}

export type Certification = { name: string; issuer: string | null; year: number | null }

export const languageLevels = ['Native', 'Fluent', 'Professional', 'Conversational', 'Basic'] as const

export const seniorityLevels = [
  'Entry',
  'Junior',
  'Mid-level',
  'Senior',
  'Lead',
  'Manager',
  'Director',
  'Executive',
] as const

export const employmentTypes = [
  'Full-time',
  'Part-time',
  'Contract',
  'Freelance',
  'Internship',
  'Temporary',
] as const

export type LanguageSkill = { language: string; level: string | null }

export type ProfileLink = { label: string | null; url: string }

export type Project = { name: string; description: string | null; url: string | null }

export type Volunteering = {
  organization: string
  role: string | null
  startMonth: string | null
  endMonth: string | null
  current: boolean
  description: string | null
}

export type ProfileDetails = {
  experience: WorkExperience[]
  education: Education[]
  certifications: Certification[]
  languages: LanguageSkill[]
  links: ProfileLink[]
  projects: Project[]
  volunteering: Volunteering[]
}

/** Facts read from a résumé; anything it does not state is null or empty. */
export type ResumeDraft = {
  fullName: string | null
  headline: string | null
  seniority: string | null
  yearsOfExperience: number | null
  experienceSummary: string | null
  skills: string[]
  targetJobTitles: string[]
  details: ProfileDetails
}

export type Resume = {
  fileName: string
  uploadedAt: string
  status: 'Extracted' | 'Unreadable' | 'ExtractionFailed'
  draft: ResumeDraft | null
}

export type AboutYou = {
  fullName: string
  headline: string | null
  countryOfResidence: string
  timeZone: string
  seniority: string | null
  yearsOfExperience: number
  experienceSummary: string | null
  skills: string[]
  languages: LanguageSkill[]
  links: ProfileLink[]
}

export type JobPreferences = {
  targetJobTitles: string[]
  employmentTypes: string[]
  minimumCompensation: number | null
  maximumCompensation: number | null
  compensationCurrency: string | null
}

/** A monthly pay range; the top band has no maximum. */
export type PayBand = { minimum: number; maximum: number | null }

/** Bands by ISO 4217 currency, and each country's default currency keyed by ISO 3166 code. */
export type PayBandOptions = {
  byCurrency: Record<string, PayBand[]>
  byCountry: Record<string, string>
}

export type OnboardingStep = 'about' | 'experience' | 'education' | 'projects' | 'preferences'

/** The profile as saved so far; a step's data counts only once it is in `completedSteps`. */
export type CandidateProfile = {
  about: AboutYou
  experience: WorkExperience[]
  education: Education[]
  certifications: Certification[]
  projects: Project[]
  volunteering: Volunteering[]
  preferences: JobPreferences | null
  completedSteps: OnboardingStep[]
  onboardingComplete: boolean
  suggestedYearsOfExperience: number | null
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
