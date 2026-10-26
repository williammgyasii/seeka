import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import type { CandidateProfile, Resume } from '@/shared/api'
import { deviceTimeZone } from '@/shared/lib/countries'
import {
  Alert,
  AlertDescription,
  Button,
  ErrorMessage,
  Loading,
  Page,
  PageHeader,
} from '@/shared/ui'
import { AboutStep } from './AboutStep'
import { EducationStep } from './EducationStep'
import { ExperienceStep } from './ExperienceStep'
import { useGetProfileQuery } from './onboardingApi'
import { PreferencesStep } from './PreferencesStep'
import { ProjectsStep } from './ProjectsStep'
import { useGetCurrentResumeQuery } from './resumeApi'
import { ResumeUpload } from './ResumeUpload'
import { StepProgress } from './StepProgress'
import { nextStep, openingStep, previousStep, type StepId } from './steps'

function resumeNote(resume: Resume): string {
  switch (resume.status) {
    case 'Extracted':
      return `Filled in from ${resume.fileName}. Check every step before saving.`
    case 'Unreadable':
      return `We couldn't read any text in ${resume.fileName}, which happens with scanned PDFs. Please fill in each step.`
    default:
      return `We couldn't read ${resume.fileName} right now. Please fill in each step, or upload it again later.`
  }
}

const header = (
  <PageHeader
    eyebrow="Profile setup"
    title="Give assessments reliable facts."
    description="Seeka uses these facts in match and outreach assessments. Each step saves when you continue, so you can stop and come back."
  />
)

/** One line instead of the page header, so the step gets the screen height. */
const compactHeader = (
  <header className="shrink-0">
    <p className="text-xs font-semibold tracking-[.2em] text-muted-foreground uppercase">
      Profile setup
    </p>
    <h1 className="mt-1 font-heading text-xl font-semibold tracking-tight">
      Give assessments reliable facts.
    </h1>
    <p className="hidden text-sm text-muted-foreground sm:block">
      Each step saves when you continue, so you can stop and come back.
    </p>
  </header>
)

export function OnboardingPage({ timeZone = deviceTimeZone() }: { timeZone?: string }) {
  const resume = useGetCurrentResumeQuery()
  const profile = useGetProfileQuery()

  if (resume.isLoading || profile.isLoading)
    return (
      <Page>
        {header}
        <Loading />
      </Page>
    )
  if (!profile.data)
    return (
      <Page>
        {header}
        <ErrorMessage error={profile.error} />
      </Page>
    )
  // A stored draft refills the steps after a reload, without another AI call.
  return <Stepper timeZone={timeZone} stored={resume.data ?? undefined} profile={profile.data} />
}

function Stepper({
  timeZone,
  stored,
  profile,
}: {
  timeZone: string
  stored?: Resume
  profile: CandidateProfile
}) {
  const navigate = useNavigate()
  const [step, setStep] = useState<StepId>(() =>
    openingStep(profile.completedSteps, Boolean(stored)),
  )
  const [resume, setResume] = useState(stored)
  const draft = resume?.draft ?? null

  const back = () => setStep(previousStep(step) ?? step)
  const done = () => setStep(nextStep(step) ?? step)
  // Remount the step forms when a new résumé arrives, so its draft fills them.
  const formKey = `${step}-${resume?.uploadedAt ?? 'none'}`

  return (
    <div className="flex min-h-0 flex-1 flex-col gap-4">
      {compactHeader}
      <StepProgress step={step} />
      {resume && step !== 'upload' && (
        <Alert className="shrink-0 py-2">
          <AlertDescription className="flex flex-wrap items-center justify-between gap-3">
            <span>{resumeNote(resume)}</span>
            <Button type="button" variant="outline" size="sm" onClick={() => setStep('upload')}>
              Upload a different résumé
            </Button>
          </AlertDescription>
        </Alert>
      )}
      {step === 'upload' && (
        <ResumeUpload
          onRead={(read) => {
            setResume(read)
            setStep('about')
          }}
          onSkip={() => setStep('about')}
        />
      )}
      {step === 'about' && (
        <AboutStep key={formKey} profile={profile} draft={draft} timeZone={timeZone} onDone={done} />
      )}
      {step === 'experience' && (
        <ExperienceStep key={formKey} profile={profile} draft={draft} onBack={back} onDone={done} />
      )}
      {step === 'education' && (
        <EducationStep key={formKey} profile={profile} draft={draft} onBack={back} onDone={done} />
      )}
      {step === 'projects' && (
        <ProjectsStep key={formKey} profile={profile} draft={draft} onBack={back} onDone={done} />
      )}
      {step === 'preferences' && (
        <PreferencesStep
          key={formKey}
          profile={profile}
          draft={draft}
          onBack={back}
          // Same target as RequireSession's redirect for finished users, which can win the race.
          onFinished={() => navigate('/dashboard', { replace: true })}
        />
      )}
    </div>
  )
}
