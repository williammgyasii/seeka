import { useFieldArray, useForm, useWatch } from 'react-hook-form'
import type { CandidateProfile, Certification, Education, ResumeDraft } from '@/shared/api'
import { Field, Input } from '@/shared/ui'
import { draftDetails, errorAt, isSaved, orEmpty, text, year } from './formValues'
import { ItemList } from './ItemList'
import { StepFrame } from './StepFrame'
import { itemSummary, yearRange } from './summaries'
import { useStepSave } from './useStepSave'

type School = {
  school: string
  degree: string
  fieldOfStudy: string
  startYear: string
  endYear: string
}

type Certificate = { name: string; issuer: string; year: string }

type Values = { education: School[]; certifications: Certificate[] }

const schoolForm = (item: Education): School => ({
  school: item.school,
  degree: orEmpty(item.degree),
  fieldOfStudy: orEmpty(item.fieldOfStudy),
  startYear: orEmpty(item.startYear),
  endYear: orEmpty(item.endYear),
})

const certificateForm = (item: Certification): Certificate => ({
  name: item.name,
  issuer: orEmpty(item.issuer),
  year: orEmpty(item.year),
})

const schoolBody = (item: School): Education => ({
  school: item.school.trim(),
  degree: text(item.degree),
  fieldOfStudy: text(item.fieldOfStudy),
  startYear: year(item.startYear),
  endYear: year(item.endYear),
})

const certificateBody = (item: Certificate): Certification => ({
  name: item.name.trim(),
  issuer: text(item.issuer),
  year: year(item.year),
})

export function EducationStep({
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
  const source = isSaved(profile, 'education') ? profile : draftDetails(draft)
  const form = useForm<Values>({
    defaultValues: {
      education: source.education.map(schoolForm),
      certifications: source.certifications.map(certificateForm),
    },
  })
  const schools = useFieldArray({ control: form.control, name: 'education' })
  const certificates = useFieldArray({ control: form.control, name: 'certifications' })
  const { submit, saving, failure } = useStepSave('education', form.setError)
  const values = useWatch({ control: form.control, name: 'education' })

  const send = async (body: { education: Education[]; certifications: Certification[] }) => {
    if (await submit(body)) onDone()
  }

  const save = form.handleSubmit((values) =>
    send({
      education: values.education.map(schoolBody),
      certifications: values.certifications.map(certificateBody),
    }),
  )

  return (
    <StepFrame
      title="Education and certifications"
      description="Schools and certifications you can name in an interview. Years are optional."
      onSubmit={save}
      onBack={onBack}
      onSkip={() => send({ education: [], certifications: [] })}
      saving={saving}
      failure={failure}
    >
      <ItemList
        title="Education"
        itemName="School"
        addLabel="Add a school"
        emptyText="No schools yet."
        keys={schools.fields.map((field) => field.id)}
        onAdd={() =>
          schools.append({ school: '', degree: '', fieldOfStudy: '', startYear: '', endYear: '' })
        }
        onRemove={schools.remove}
        error={form.formState.errors.education?.message}
        summarize={(index) => ({
          title: itemSummary(values[index]?.degree, values[index]?.school, 'New school'),
          detail: yearRange(values[index]?.startYear, values[index]?.endYear),
        })}
        hasError={(index) => Boolean(form.formState.errors.education?.[index])}
        renderItem={(index) => (
          <div className="grid gap-4 sm:grid-cols-2">
            <Field label="School" error={errorAt(form, `education.${index}.school`)}>
              <Input {...form.register(`education.${index}.school`)} />
            </Field>
            <Field label="Degree" error={errorAt(form, `education.${index}.degree`)}>
              <Input {...form.register(`education.${index}.degree`)} />
            </Field>
            <Field label="Field of study" error={errorAt(form, `education.${index}.fieldOfStudy`)}>
              <Input {...form.register(`education.${index}.fieldOfStudy`)} />
            </Field>
            <div className="grid grid-cols-2 gap-3">
              <Field label="Start year" error={errorAt(form, `education.${index}.startYear`)}>
                <Input type="number" {...form.register(`education.${index}.startYear`)} />
              </Field>
              <Field label="End year" error={errorAt(form, `education.${index}.endYear`)}>
                <Input type="number" {...form.register(`education.${index}.endYear`)} />
              </Field>
            </div>
          </div>
        )}
      />
      <ItemList
        title="Certifications"
        itemName="Certificate"
        addLabel="Add a certification"
        emptyText="No certifications yet."
        keys={certificates.fields.map((field) => field.id)}
        onAdd={() => certificates.append({ name: '', issuer: '', year: '' })}
        onRemove={certificates.remove}
        error={form.formState.errors.certifications?.message}
        renderItem={(index) => (
          <div className="grid gap-4 sm:grid-cols-[2fr_2fr_1fr]">
            <Field label="Certification" error={errorAt(form, `certifications.${index}.name`)}>
              <Input {...form.register(`certifications.${index}.name`)} />
            </Field>
            <Field label="Issuer" error={errorAt(form, `certifications.${index}.issuer`)}>
              <Input {...form.register(`certifications.${index}.issuer`)} />
            </Field>
            <Field label="Year" error={errorAt(form, `certifications.${index}.year`)}>
              <Input type="number" {...form.register(`certifications.${index}.year`)} />
            </Field>
          </div>
        )}
      />
    </StepFrame>
  )
}
