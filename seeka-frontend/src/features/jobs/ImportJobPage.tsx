import { zodResolver } from '@hookform/resolvers/zod'
import { Search } from 'lucide-react'
import { useForm } from 'react-hook-form'
import { useNavigate } from 'react-router-dom'
import { z } from 'zod'
import { splitList } from '@/shared/lib/format'
import {
  Alert,
  AlertDescription,
  Button,
  Card,
  CardContent,
  ErrorMessage,
  Field,
  Input,
  Page,
  PageHeader,
  Textarea,
} from '@/shared/ui'
import { useImportJobMutation } from './jobsApi'

const importSchema = z.object({
  companyName: z.string().min(2),
  companyDomain: z.string().optional(),
  title: z.string().min(2),
  sourceUrl: z.url(),
  description: z.string().min(30),
  locationText: z.string().optional(),
  restrictionText: z.string().optional(),
  employmentType: z.string().optional(),
  requiredSkills: z.string().optional(),
  seniority: z.string().optional(),
  workingHours: z.string().optional(),
  salaryMinimum: z.string().optional(),
  salaryMaximum: z.string().optional(),
  salaryCurrency: z.string().optional(),
})

type ImportValues = z.infer<typeof importSchema>

export function ImportJobPage() {
  const navigate = useNavigate()
  const form = useForm<ImportValues>({
    resolver: zodResolver(importSchema),
    defaultValues: {
      companyName: '',
      title: '',
      sourceUrl: '',
      description: '',
      salaryCurrency: 'USD',
    },
  })
  const [submitImport, mutation] = useImportJobMutation()
  const importJob = async (values: ImportValues) => {
    const result = await submitImport({
      ...values,
      requiredSkills: splitList(values.requiredSkills ?? ''),
      salaryMinimum: values.salaryMinimum ? Number(values.salaryMinimum) : null,
      salaryMaximum: values.salaryMaximum ? Number(values.salaryMaximum) : null,
      postedAt: null,
    })
    if (!result.error) navigate(`/jobs/${result.data.savedJob.id}`)
  }

  return (
    <Page>
      <PageHeader
        eyebrow="Job discovery"
        title="Import a real job."
        description="Paste details from the original posting. Seeka stores provenance and will not invent a date, salary, or hiring geography."
      />
      <Card>
        <CardContent className="grid gap-6">
          <Alert className="border-info/30 bg-info/10 text-info">
            <Search />
            <AlertDescription className="text-info/90">
              Public ATS providers are not global search engines. The first slice keeps manual
              import fully usable; configured Greenhouse boards come next.
            </AlertDescription>
          </Alert>
          <form className="grid gap-5" onSubmit={form.handleSubmit(importJob)}>
            <div className="grid gap-4 md:grid-cols-2">
              <Field label="Company">
                <Input {...form.register('companyName')} />
              </Field>
              <Field label="Company domain (optional)">
                <Input placeholder="example.com" {...form.register('companyDomain')} />
              </Field>
              <Field label="Job title">
                <Input {...form.register('title')} />
              </Field>
              <Field label="Original job URL">
                <Input type="url" {...form.register('sourceUrl')} />
              </Field>
              <Field label="Location shown">
                <Input placeholder="Remote — EMEA" {...form.register('locationText')} />
              </Field>
              <Field label="Employment type">
                <Input placeholder="Full-time" {...form.register('employmentType')} />
              </Field>
            </div>
            <Field label="Original restriction text">
              <Textarea
                placeholder="Copy any country, residence, work-authorisation, or timezone wording exactly."
                {...form.register('restrictionText')}
              />
            </Field>
            <Field label="Job description">
              <Textarea className="min-h-52" {...form.register('description')} />
            </Field>
            <div className="grid gap-4 md:grid-cols-3">
              <Field label="Required skills">
                <Input
                  placeholder="Recruitment, Excel, calendars"
                  {...form.register('requiredSkills')}
                />
              </Field>
              <Field label="Seniority">
                <Input placeholder="Mid-level" {...form.register('seniority')} />
              </Field>
              <Field label="Working hours">
                <Input placeholder="UTC hours" {...form.register('workingHours')} />
              </Field>
              <Field label="Salary minimum">
                <Input type="number" {...form.register('salaryMinimum')} />
              </Field>
              <Field label="Salary maximum">
                <Input type="number" {...form.register('salaryMaximum')} />
              </Field>
              <Field label="Currency">
                <Input maxLength={3} {...form.register('salaryCurrency')} />
              </Field>
            </div>
            {mutation.error && <ErrorMessage error={mutation.error} />}
            <div className="flex justify-end">
              <Button size="lg" disabled={mutation.isLoading}>
                {mutation.isLoading ? 'Assessing…' : 'Import and assess job'}
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </Page>
  )
}
