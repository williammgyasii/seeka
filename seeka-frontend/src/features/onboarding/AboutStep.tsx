import { zodResolver } from '@hookform/resolvers/zod'
import { useFieldArray, useForm, useWatch } from 'react-hook-form'
import { z } from 'zod'
import {
  languageLevels,
  seniorityLevels,
  type CandidateProfile,
  type ResumeDraft,
} from '@/shared/api'
import { splitList } from '@/shared/lib/format'
import { Field, Input, Textarea } from '@/shared/ui'
import { ChoiceField } from './ChoiceField'
import { draftDetails, errorAt, isSaved, orEmpty, text } from './formValues'
import { ItemList } from './ItemList'
import { countryOptions, residenceTimeZones, suggestResidence, timeZoneFor } from './residence'
import { StepFrame } from './StepFrame'
import { useStepSave } from './useStepSave'

const schema = z.object({
  fullName: z.string().trim().min(1, 'Enter your full name.'),
  headline: z.string(),
  countryOfResidence: z.string().min(1, 'Choose your country of residence.'),
  timeZone: z.string().min(1, 'Choose your timezone.'),
  seniority: z.string(),
  yearsOfExperience: z.number({ message: 'Enter a number of years.' }).min(0).max(60),
  experienceSummary: z.string(),
  skills: z.string(),
  languages: z.array(z.object({ language: z.string(), level: z.string() })),
  links: z.array(z.object({ label: z.string(), url: z.string() })),
})

type Values = z.infer<typeof schema>

const countries = countryOptions()

function initialValues(
  profile: CandidateProfile,
  draft: ResumeDraft | null,
  timeZone: string,
): Values {
  if (isSaved(profile, 'about')) {
    const about = profile.about
    return {
      ...about,
      headline: orEmpty(about.headline),
      seniority: orEmpty(about.seniority),
      experienceSummary: orEmpty(about.experienceSummary),
      skills: about.skills.join(', '),
      languages: about.languages.map((item) => ({ ...item, level: orEmpty(item.level) })),
      links: about.links.map((item) => ({ ...item, label: orEmpty(item.label) })),
    }
  }
  const details = draftDetails(draft)
  const residence = suggestResidence(timeZone) ?? ''
  return {
    fullName: orEmpty(draft?.fullName),
    headline: orEmpty(draft?.headline),
    countryOfResidence: residence,
    timeZone: residence ? timeZoneFor(residence, timeZone) : '',
    seniority: orEmpty(draft?.seniority),
    yearsOfExperience: draft?.yearsOfExperience ?? 0,
    experienceSummary: orEmpty(draft?.experienceSummary),
    skills: draft?.skills.join(', ') ?? '',
    languages: details.languages.map((item) => ({ ...item, level: orEmpty(item.level) })),
    links: details.links.map((item) => ({ ...item, label: orEmpty(item.label) })),
  }
}

function toBody(values: Values) {
  return {
    ...values,
    fullName: values.fullName.trim(),
    headline: text(values.headline),
    seniority: text(values.seniority),
    experienceSummary: text(values.experienceSummary),
    skills: splitList(values.skills),
    languages: values.languages.map((item) => ({ language: item.language, level: text(item.level) })),
    links: values.links.map((item) => ({ label: text(item.label), url: item.url.trim() })),
  }
}

export function AboutStep({
  profile,
  draft,
  timeZone,
  onDone,
}: {
  profile: CandidateProfile
  draft: ResumeDraft | null
  timeZone: string
  onDone: () => void
}) {
  const form = useForm<Values>({
    resolver: zodResolver(schema),
    defaultValues: initialValues(profile, draft, timeZone),
  })
  const languages = useFieldArray({ control: form.control, name: 'languages' })
  const links = useFieldArray({ control: form.control, name: 'links' })
  const { submit, saving, failure } = useStepSave('about', form.setError)
  const residence = useWatch({ control: form.control, name: 'countryOfResidence' })
  const timeZones = residenceTimeZones(residence)

  const save = form.handleSubmit(async (values) => {
    if (await submit(toBody(values))) onDone()
  })

  return (
    <StepFrame
      title="About you"
      description="Who you are and where you work from. Seeka uses these facts in every assessment."
      onSubmit={save}
      saving={saving}
      failure={failure}
    >
      <div className="grid gap-4 sm:grid-cols-2">
        <Field label="Full name" error={errorAt(form, 'fullName')}>
          <Input {...form.register('fullName')} />
        </Field>
        <Field label="Headline" error={errorAt(form, 'headline')}>
          <Input placeholder="Example: HR operations lead" {...form.register('headline')} />
        </Field>
        <ChoiceField
          control={form.control}
          name="countryOfResidence"
          label="Country of residence"
          placeholder="Choose a country"
          options={countries}
          onValueChange={(country) =>
            form.setValue('timeZone', timeZoneFor(country, form.getValues('timeZone')))
          }
        />
        <ChoiceField
          control={form.control}
          name="timeZone"
          label="Timezone"
          placeholder={residence ? 'Choose a timezone' : 'Choose a country first'}
          options={timeZones}
          disabled={timeZones.length <= 1}
        />
        <ChoiceField
          control={form.control}
          name="seniority"
          label="Seniority"
          placeholder="Choose a level"
          options={seniorityLevels}
        />
        <Field label="Years of experience" error={errorAt(form, 'yearsOfExperience')}>
          <Input
            type="number"
            min="0"
            {...form.register('yearsOfExperience', { valueAsNumber: true })}
          />
        </Field>
      </div>
      <Field label="Skills (comma-separated)" error={errorAt(form, 'skills')}>
        <Input {...form.register('skills')} />
      </Field>
      <Field label="Experience summary" error={errorAt(form, 'experienceSummary')}>
        <Textarea
          placeholder="Example: 5 years supporting executives and HR teams…"
          {...form.register('experienceSummary')}
        />
      </Field>
      <ItemList
        title="Languages"
        itemName="Language"
        addLabel="Add a language"
        emptyText="No languages yet."
        keys={languages.fields.map((field) => field.id)}
        onAdd={() => languages.append({ language: '', level: '' })}
        onRemove={languages.remove}
        renderItem={(index) => (
          <div className="grid gap-4 sm:grid-cols-2">
            <Field label="Language" error={errorAt(form, `languages.${index}.language`)}>
              <Input {...form.register(`languages.${index}.language`)} />
            </Field>
            <ChoiceField
              control={form.control}
              name={`languages.${index}.level`}
              label="Level"
              placeholder="Choose a level"
              options={languageLevels}
            />
          </div>
        )}
      />
      <ItemList
        title="Links"
        itemName="Link"
        addLabel="Add a link"
        emptyText="No links yet. LinkedIn or a portfolio helps recruiters check your work."
        keys={links.fields.map((field) => field.id)}
        onAdd={() => links.append({ label: '', url: '' })}
        onRemove={links.remove}
        renderItem={(index) => (
          <div className="grid gap-4 sm:grid-cols-2">
            <Field label="Link label" error={errorAt(form, `links.${index}.label`)}>
              <Input placeholder="Example: LinkedIn" {...form.register(`links.${index}.label`)} />
            </Field>
            <Field label="Link URL" error={errorAt(form, `links.${index}.url`)}>
              <Input
                type="url"
                placeholder="https://"
                {...form.register(`links.${index}.url`)}
              />
            </Field>
          </div>
        )}
      />
    </StepFrame>
  )
}
