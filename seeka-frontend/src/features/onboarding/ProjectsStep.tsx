import { useFieldArray, useForm, useWatch } from 'react-hook-form'
import type { CandidateProfile, Project, ResumeDraft, Volunteering } from '@/shared/api'
import { Field, Input, Textarea } from '@/shared/ui'
import { draftDetails, errorAt, isSaved, orEmpty, text } from './formValues'
import { ItemList } from './ItemList'
import { MonthRange } from './MonthRange'
import { StepFrame } from './StepFrame'
import { itemSummary, monthRange } from './summaries'
import { useStepSave } from './useStepSave'

type ProjectValues = { name: string; description: string; url: string }

type Role = {
  organization: string
  role: string
  startMonth: string
  endMonth: string
  current: boolean
  description: string
}

type Values = { projects: ProjectValues[]; volunteering: Role[] }

const projectForm = (item: Project): ProjectValues => ({
  name: item.name,
  description: orEmpty(item.description),
  url: orEmpty(item.url),
})

const roleForm = (item: Volunteering): Role => ({
  ...item,
  role: orEmpty(item.role),
  startMonth: orEmpty(item.startMonth),
  endMonth: orEmpty(item.endMonth),
  description: orEmpty(item.description),
})

const projectBody = (item: ProjectValues): Project => ({
  name: item.name.trim(),
  description: text(item.description),
  url: text(item.url),
})

const roleBody = (item: Role): Volunteering => ({
  organization: item.organization.trim(),
  role: text(item.role),
  startMonth: text(item.startMonth),
  endMonth: item.current ? null : text(item.endMonth),
  current: item.current,
  description: text(item.description),
})

export function ProjectsStep({
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
  const source = isSaved(profile, 'projects') ? profile : draftDetails(draft)
  const form = useForm<Values>({
    defaultValues: {
      projects: source.projects.map(projectForm),
      volunteering: source.volunteering.map(roleForm),
    },
  })
  const projects = useFieldArray({ control: form.control, name: 'projects' })
  const roles = useFieldArray({ control: form.control, name: 'volunteering' })
  const { submit, saving, failure } = useStepSave('projects', form.setError)
  const projectValues = useWatch({ control: form.control, name: 'projects' })
  const roleValues = useWatch({ control: form.control, name: 'volunteering' })

  const send = async (body: { projects: Project[]; volunteering: Volunteering[] }) => {
    if (await submit(body)) onDone()
  }

  const save = form.handleSubmit((values) =>
    send({
      projects: values.projects.map(projectBody),
      volunteering: values.volunteering.map(roleBody),
    }),
  )

  return (
    <StepFrame
      title="Projects and volunteering"
      description="Work outside a job title that shows what you can do. Optional."
      onSubmit={save}
      onBack={onBack}
      onSkip={() => send({ projects: [], volunteering: [] })}
      saving={saving}
      failure={failure}
    >
      <ItemList
        title="Projects"
        itemName="Project"
        addLabel="Add a project"
        emptyText="No projects yet."
        keys={projects.fields.map((field) => field.id)}
        onAdd={() => projects.append({ name: '', description: '', url: '' })}
        onRemove={projects.remove}
        error={form.formState.errors.projects?.message}
        summarize={(index) => ({
          title: projectValues[index]?.name.trim() || 'New project',
          detail: projectValues[index]?.description.trim() || undefined,
        })}
        hasError={(index) => Boolean(form.formState.errors.projects?.[index])}
        renderItem={(index) => (
          <>
            <div className="grid gap-4 sm:grid-cols-2">
              <Field label="Project name" error={errorAt(form, `projects.${index}.name`)}>
                <Input {...form.register(`projects.${index}.name`)} />
              </Field>
              <Field label="Project link" error={errorAt(form, `projects.${index}.url`)}>
                <Input type="url" placeholder="https://" {...form.register(`projects.${index}.url`)} />
              </Field>
            </div>
            <Field label="What it was" error={errorAt(form, `projects.${index}.description`)}>
              <Textarea {...form.register(`projects.${index}.description`)} />
            </Field>
          </>
        )}
      />
      <ItemList
        title="Volunteering"
        itemName="Volunteering role"
        addLabel="Add volunteering"
        emptyText="No volunteering yet."
        keys={roles.fields.map((field) => field.id)}
        onAdd={() =>
          roles.append({
            organization: '',
            role: '',
            startMonth: '',
            endMonth: '',
            current: false,
            description: '',
          })
        }
        onRemove={roles.remove}
        error={form.formState.errors.volunteering?.message}
        summarize={(index) => ({
          title: itemSummary(
            roleValues[index]?.role,
            roleValues[index]?.organization,
            'New volunteering role',
          ),
          detail: monthRange(
            roleValues[index]?.startMonth,
            roleValues[index]?.endMonth,
            Boolean(roleValues[index]?.current),
          ),
        })}
        hasError={(index) => Boolean(form.formState.errors.volunteering?.[index])}
        renderItem={(index) => (
          <>
            <div className="grid gap-4 sm:grid-cols-2">
              <Field label="Organization" error={errorAt(form, `volunteering.${index}.organization`)}>
                <Input {...form.register(`volunteering.${index}.organization`)} />
              </Field>
              <Field label="Role" error={errorAt(form, `volunteering.${index}.role`)}>
                <Input {...form.register(`volunteering.${index}.role`)} />
              </Field>
            </div>
            <MonthRange form={form} prefix={`volunteering.${index}`} currentLabel="I still do this" />
            <Field label="What you did" error={errorAt(form, `volunteering.${index}.description`)}>
              <Textarea {...form.register(`volunteering.${index}.description`)} />
            </Field>
          </>
        )}
      />
    </StepFrame>
  )
}
