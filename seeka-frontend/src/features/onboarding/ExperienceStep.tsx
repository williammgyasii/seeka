import { useFieldArray, useForm, useWatch } from 'react-hook-form'
import type { CandidateProfile, ResumeDraft, WorkExperience } from '@/shared/api'
import { Field, Input, Textarea } from '@/shared/ui'
import { draftDetails, errorAt, isSaved, lines, orEmpty, text } from './formValues'
import { ItemList } from './ItemList'
import { MonthRange } from './MonthRange'
import { StepFrame } from './StepFrame'
import { itemSummary, monthRange } from './summaries'
import { useStepSave } from './useStepSave'

type Job = {
  company: string
  title: string
  location: string
  startMonth: string
  endMonth: string
  current: boolean
  highlights: string
}

type Values = { experience: Job[] }

const blank: Job = {
  company: '',
  title: '',
  location: '',
  startMonth: '',
  endMonth: '',
  current: false,
  highlights: '',
}

const toForm = (job: WorkExperience): Job => ({
  ...job,
  location: orEmpty(job.location),
  startMonth: orEmpty(job.startMonth),
  endMonth: orEmpty(job.endMonth),
  highlights: job.highlights.join('\n'),
})

const toBody = (job: Job): WorkExperience => ({
  company: job.company.trim(),
  title: job.title.trim(),
  location: text(job.location),
  startMonth: text(job.startMonth),
  endMonth: job.current ? null : text(job.endMonth),
  current: job.current,
  highlights: lines(job.highlights),
})

export function ExperienceStep({
  profile,
  draft,
  onBack,
  onDone,
}: {
  profile: CandidateProfile
  draft: ResumeDraft | null
  onBack: () => void
  onDone: () => void
}) {
  const saved = isSaved(profile, 'experience')
  const form = useForm<Values>({
    defaultValues: {
      experience: (saved ? profile.experience : draftDetails(draft).experience).map(toForm),
    },
  })
  const jobs = useFieldArray({ control: form.control, name: 'experience' })
  const { submit, saving, failure } = useStepSave('experience', form.setError)
  const values = useWatch({ control: form.control, name: 'experience' })

  const send = async (experience: WorkExperience[]) => {
    if (await submit({ experience })) onDone()
  }

  return (
    <StepFrame
      title="Experience"
      description="Your jobs, newest first. Seeka quotes these highlights when it drafts a message, so keep them true and specific."
      onSubmit={form.handleSubmit((values) => send(values.experience.map(toBody)))}
      onBack={onBack}
      onSkip={() => send([])}
      saving={saving}
      failure={failure}
    >
      <ItemList
        title="Jobs"
        itemName="Job"
        addLabel="Add a job"
        emptyText="No jobs yet."
        keys={jobs.fields.map((field) => field.id)}
        onAdd={() => jobs.append(blank)}
        onRemove={jobs.remove}
        error={form.formState.errors.experience?.message}
        summarize={(index) => ({
          title: itemSummary(values[index]?.title, values[index]?.company, 'New job'),
          detail: monthRange(
            values[index]?.startMonth,
            values[index]?.endMonth,
            Boolean(values[index]?.current),
          ),
        })}
        hasError={(index) => Boolean(form.formState.errors.experience?.[index])}
        renderItem={(index) => (
          <>
            <div className="grid gap-4 sm:grid-cols-2">
              <Field label="Company" error={errorAt(form, `experience.${index}.company`)}>
                <Input {...form.register(`experience.${index}.company`)} />
              </Field>
              <Field label="Job title" error={errorAt(form, `experience.${index}.title`)}>
                <Input {...form.register(`experience.${index}.title`)} />
              </Field>
              <Field label="Location" error={errorAt(form, `experience.${index}.location`)}>
                <Input {...form.register(`experience.${index}.location`)} />
              </Field>
            </div>
            <MonthRange
              form={form}
              prefix={`experience.${index}`}
              currentLabel="I currently work here"
            />
            <Field
              label="Highlights (one per line)"
              error={errorAt(form, `experience.${index}.highlights`)}
            >
              <Textarea {...form.register(`experience.${index}.highlights`)} />
            </Field>
          </>
        )}
      />
    </StepFrame>
  )
}
