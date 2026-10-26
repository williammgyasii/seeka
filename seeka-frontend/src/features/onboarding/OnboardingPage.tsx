import { zodResolver } from '@hookform/resolvers/zod'
import { useState } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { useNavigate } from 'react-router-dom'
import { z } from 'zod'
import type { Resume } from '@/shared/api'
import { splitList } from '@/shared/lib/format'
import {
  Alert,
  AlertDescription,
  Button,
  ErrorMessage,
  Field,
  Input,
  Label,
  Loading,
  Page,
  PageHeader,
  SectionCard,
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
  Textarea,
} from '@/shared/ui'
import { useSaveCandidateMutation } from './onboardingApi'
import { deviceTimeZone } from '@/shared/lib/countries'
import { countryOptions, suggestResidence } from './residence'
import { useGetCurrentResumeQuery } from './resumeApi'
import { ResumeUpload } from './ResumeUpload'

const setupSchema = z.object({
  fullName: z.string().min(2),
  countryOfResidence: z.string().min(1, 'Choose your country of residence.'),
  timeZone: z.string().min(3),
  targetJobTitles: z.string().min(2),
  seniority: z.string().min(2),
  skills: z.string().min(2),
  yearsOfExperience: z.number().min(0),
  experienceSummary: z.string().min(20),
  employmentTypes: z.string().min(2),
  availableWorkingHours: z.string().min(2),
  minimumCompensation: z.string().optional(),
  compensationCurrency: z.string().optional(),
})

type SetupValues = z.infer<typeof setupSchema>

const countries = countryOptions()

/** Only facts the résumé stated replace a field; the rest keep their defaults. */
function draftValues(resume: Resume): Partial<SetupValues> {
  const draft = resume.draft
  if (!draft) return {}
  const values: Partial<SetupValues> = {}
  if (draft.fullName) values.fullName = draft.fullName
  if (draft.seniority) values.seniority = draft.seniority
  if (draft.yearsOfExperience !== null) values.yearsOfExperience = draft.yearsOfExperience
  if (draft.experienceSummary) values.experienceSummary = draft.experienceSummary
  if (draft.skills.length) values.skills = draft.skills.join(', ')
  if (draft.targetJobTitles.length) values.targetJobTitles = draft.targetJobTitles.join(', ')
  return values
}

function resumeNote(resume: Resume): string {
  switch (resume.status) {
    case 'Extracted':
      return `Filled in from ${resume.fileName}. Check every field before saving.`
    case 'Unreadable':
      return `We couldn't read any text in ${resume.fileName}, which happens with scanned PDFs. Please fill in the form.`
    default:
      return `We couldn't read ${resume.fileName} right now. Please fill in the form, or upload it again later.`
  }
}

const header = (
  <PageHeader
    eyebrow="Profile setup"
    title="Give assessments reliable facts."
    description="Seeka uses these facts in match and outreach assessments. Nothing is saved until you save the form."
  />
)

export function OnboardingPage({ timeZone = deviceTimeZone() }: { timeZone?: string }) {
  const current = useGetCurrentResumeQuery()
  if (current.isLoading)
    return (
      <Page>
        {header}
        <Loading />
      </Page>
    )
  // A stored draft refills the form after a reload, without another AI call.
  return <ProfileSetup timeZone={timeZone} stored={current.data ?? undefined} />
}

function ProfileSetup({ timeZone, stored }: { timeZone: string; stored?: Resume }) {
  const navigate = useNavigate()
  const [step, setStep] = useState<'upload' | 'form'>(stored ? 'form' : 'upload')
  const [applied, setApplied] = useState(stored)
  const form = useForm<SetupValues>({
    resolver: zodResolver(setupSchema),
    defaultValues: {
      countryOfResidence: suggestResidence(timeZone) ?? '',
      timeZone,
      targetJobTitles: 'Administrative Assistant, HR Coordinator',
      seniority: 'Mid-level',
      employmentTypes: 'Full-time, Contract',
      availableWorkingHours: '',
      compensationCurrency: 'USD',
      yearsOfExperience: 0,
      ...(stored && draftValues(stored)),
    },
  })
  const applyResume = (resume: Resume) => {
    form.reset({ ...form.getValues(), ...draftValues(resume) })
    setApplied(resume)
  }

  const [saveCandidate, mutation] = useSaveCandidateMutation()
  const save = async (values: SetupValues) => {
    const result = await saveCandidate({
      ...values,
      skills: splitList(values.skills),
      targetJobTitles: splitList(values.targetJobTitles),
      employmentTypes: splitList(values.employmentTypes),
      minimumCompensation: values.minimumCompensation
        ? Number(values.minimumCompensation)
        : null,
    })
    if (!result.error) navigate('/jobs/import')
  }

  if (step === 'upload')
    return (
      <Page>
        {header}
        <ResumeUpload
          onRead={(resume) => {
            applyResume(resume)
            setStep('form')
          }}
          onSkip={() => setStep('form')}
        />
      </Page>
    )

  return (
    <Page>
      {header}
      {applied && (
        <Alert>
          <AlertDescription className="flex flex-wrap items-center justify-between gap-3">
            <span>{resumeNote(applied)}</span>
            <Button type="button" variant="outline" size="sm" onClick={() => setStep('upload')}>
              Upload a different résumé
            </Button>
          </AlertDescription>
        </Alert>
      )}
      <form
        className="grid gap-6 xl:grid-cols-[1fr_.75fr]"
        onSubmit={form.handleSubmit(save)}
      >
        <SectionCard
          title="Candidate profile"
          description="Facts Seeka may use in match and outreach assessments."
        >
          <div className="grid gap-4 sm:grid-cols-2">
            <Field label="Full name">
              <Input {...form.register('fullName')} />
            </Field>
            <div className="grid gap-2">
              <Label id="residence-label">Country of residence</Label>
              <Controller
                control={form.control}
                name="countryOfResidence"
                render={({ field, fieldState }) => (
                  <>
                    <Select value={field.value} onValueChange={field.onChange}>
                      <SelectTrigger
                        aria-labelledby="residence-label"
                        aria-invalid={fieldState.invalid}
                        className="w-full"
                      >
                        <SelectValue placeholder="Choose a country" />
                      </SelectTrigger>
                      <SelectContent>
                        {countries.map((country) => (
                          <SelectItem key={country} value={country}>
                            {country}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                    {fieldState.error && (
                      <span className="text-xs text-destructive">{fieldState.error.message}</span>
                    )}
                  </>
                )}
              />
            </div>
            <Field label="Timezone">
              <Input {...form.register('timeZone')} />
            </Field>
            <Field label="Seniority">
              <Input {...form.register('seniority')} />
            </Field>
            <Field label="Years of experience">
              <Input
                type="number"
                min="0"
                {...form.register('yearsOfExperience', { valueAsNumber: true })}
              />
            </Field>
            <Field label="Skills (comma-separated)">
              <Input {...form.register('skills')} />
            </Field>
          </div>
          <Field label="Experience summary">
            <Textarea
              placeholder="Example: 5 years supporting executives and HR teams…"
              {...form.register('experienceSummary')}
            />
          </Field>
        </SectionCard>
        <SectionCard
          className="self-start"
          title="Job preferences"
          description="Used as fit inputs, not hard promises."
        >
          <Field label="Target job titles">
            <Input {...form.register('targetJobTitles')} />
          </Field>
          <Field label="Preferred employment types">
            <Input {...form.register('employmentTypes')} />
          </Field>
          <Field label="Available working hours">
            <Input
              placeholder="Example: 09:00–17:00 in your timezone"
              {...form.register('availableWorkingHours')}
            />
          </Field>
          <div className="grid grid-cols-[1fr_100px] gap-3">
            <Field label="Minimum compensation">
              <Input type="number" min="0" {...form.register('minimumCompensation')} />
            </Field>
            <Field label="Currency">
              <Input maxLength={3} {...form.register('compensationCurrency')} />
            </Field>
          </div>
          {mutation.error && <ErrorMessage error={mutation.error} />}
          <Button size="lg" disabled={mutation.isLoading}>
            {mutation.isLoading ? 'Saving…' : 'Save and import a job'}
          </Button>
        </SectionCard>
      </form>
    </Page>
  )
}
