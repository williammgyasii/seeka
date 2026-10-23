import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  BriefcaseBusiness,
  CircleUserRound,
  LayoutDashboard,
  LogOut,
  Mail,
  Plus,
  Search,
  Settings,
  Users,
} from 'lucide-react'
import { useState } from 'react'
import { useForm, type UseFormReturn } from 'react-hook-form'
import {
  Link,
  NavLink,
  Navigate,
  Outlet,
  Route,
  Routes,
  useNavigate,
  useParams,
} from 'react-router-dom'
import { z } from 'zod'
import { DraftEditor } from './components/DraftEditor'
import { Badge, Button, Card, Field, Input, Textarea } from './components/ui'
import { api, type JobDetail, type PipelineItem } from './lib/api'
import { cn } from './lib/utils'

const authSchema = z.object({
  displayName: z.string().optional(),
  email: z.email(),
  password: z.string().min(10),
})

type AuthValues = z.infer<typeof authSchema>

function AuthPage() {
  const [mode, setMode] = useState<'login' | 'register'>('login')
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const form = useForm<AuthValues>({
    resolver: zodResolver(authSchema),
    defaultValues: { email: '', password: '', displayName: '' },
  })
  const mutation = useMutation({
    mutationFn: (values: AuthValues) =>
      mode === 'login'
        ? api.login({ email: values.email, password: values.password })
        : api.register({
            email: values.email,
            password: values.password,
            displayName: values.displayName ?? '',
          }),
    onSuccess: (session) => {
      queryClient.setQueryData(['session'], session)
      navigate(mode === 'register' ? '/onboarding' : '/dashboard')
    },
  })

  return (
    <main className="grid min-h-screen bg-slate-950 lg:grid-cols-[1.15fr_.85fr]">
      <section className="relative hidden overflow-hidden p-12 text-white lg:flex lg:flex-col lg:justify-between">
        <div className="absolute inset-0 bg-[radial-gradient(circle_at_20%_20%,rgba(16,185,129,.35),transparent_35%),radial-gradient(circle_at_75%_75%,rgba(59,130,246,.22),transparent_40%)]" />
        <Brand className="relative" />
        <div className="relative max-w-xl">
          <p className="mb-4 text-sm font-bold uppercase tracking-[.25em] text-emerald-300">
            Remote work, with the fine print checked
          </p>
          <h1 className="text-5xl font-semibold leading-tight tracking-tight">
            Find work that can hire you.
          </h1>
          <p className="mt-5 max-w-lg text-lg leading-8 text-slate-300">
            Seeka separates geographic eligibility from professional fit, then helps you
            organise honest, relevant outreach.
          </p>
        </div>
        <p className="relative text-sm text-slate-400">
          Built first for Ghana-based professionals. Designed for everywhere.
        </p>
      </section>
      <section className="flex items-center justify-center bg-slate-50 px-5 py-10">
        <Card className="w-full max-w-md p-7 sm:p-9">
          <Brand className="mb-9 text-slate-950 lg:hidden" />
          <p className="text-sm font-semibold text-emerald-700">
            {mode === 'login' ? 'Welcome back' : 'Create your workspace'}
          </p>
          <h2 className="mt-2 text-3xl font-semibold tracking-tight text-slate-950">
            {mode === 'login' ? 'Sign in to Seeka' : 'Start your job search'}
          </h2>
          <form
            className="mt-7 grid gap-4"
            onSubmit={form.handleSubmit((values) => mutation.mutate(values))}
          >
            {mode === 'register' && (
              <Field label="Full name" error={form.formState.errors.displayName?.message}>
                <Input autoComplete="name" {...form.register('displayName')} />
              </Field>
            )}
            <Field label="Email address" error={form.formState.errors.email?.message}>
              <Input type="email" autoComplete="email" {...form.register('email')} />
            </Field>
            <Field label="Password" error={form.formState.errors.password?.message}>
              <Input
                type="password"
                autoComplete={mode === 'login' ? 'current-password' : 'new-password'}
                {...form.register('password')}
              />
            </Field>
            {mutation.error && <ErrorMessage error={mutation.error} />}
            <Button className="mt-2 w-full" disabled={mutation.isPending}>
              {mutation.isPending
                ? 'Please wait…'
                : mode === 'login'
                  ? 'Sign in'
                  : 'Create account'}
            </Button>
          </form>
          <button
            className="mt-6 w-full text-sm font-medium text-slate-600 hover:text-emerald-700"
            onClick={() => setMode(mode === 'login' ? 'register' : 'login')}
          >
            {mode === 'login'
              ? 'New to Seeka? Create an account'
              : 'Already have an account? Sign in'}
          </button>
        </Card>
      </section>
    </main>
  )
}

function Brand({ className }: { className?: string }) {
  return (
    <div className={cn('flex items-center gap-3 font-semibold', className)}>
      <span className="grid size-9 place-items-center rounded-xl bg-emerald-500 text-lg font-black text-slate-950">
        S
      </span>
      <span className="text-xl tracking-tight">seeka</span>
    </div>
  )
}

const navItems = [
  { to: '/dashboard', label: 'Dashboard', icon: LayoutDashboard },
  { to: '/jobs/import', label: 'Job discovery', icon: Search },
  { to: '/pipeline', label: 'Pipeline', icon: BriefcaseBusiness },
  { to: '/settings', label: 'Settings & billing', icon: Settings },
]

function AppLayout() {
  const queryClient = useQueryClient()
  const navigate = useNavigate()
  const session = useQuery({ queryKey: ['session'], queryFn: api.session, retry: false })
  const logout = useMutation({
    mutationFn: api.logout,
    onSuccess: () => {
      queryClient.clear()
      navigate('/login')
    },
  })

  return (
    <div className="min-h-screen bg-slate-50 text-slate-950">
      <aside className="fixed inset-y-0 left-0 hidden w-64 border-r border-slate-200 bg-white p-5 lg:flex lg:flex-col">
        <Brand className="px-2 py-2" />
        <nav className="mt-8 grid gap-1">
          {navItems.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              className={({ isActive }) =>
                cn(
                  'flex items-center gap-3 rounded-xl px-3 py-2.5 text-sm font-medium',
                  isActive
                    ? 'bg-emerald-50 text-emerald-800'
                    : 'text-slate-600 hover:bg-slate-50 hover:text-slate-950',
                )
              }
            >
              <item.icon size={18} />
              {item.label}
            </NavLink>
          ))}
        </nav>
        <div className="mt-auto rounded-2xl bg-slate-950 p-4 text-white">
          <p className="text-xs font-semibold uppercase tracking-wider text-emerald-300">
            Manual mode
          </p>
          <p className="mt-2 text-sm text-slate-300">
            ATS, email, enrichment, AI, and billing providers are not connected.
          </p>
        </div>
      </aside>
      <div className="lg:pl-64">
        <header className="sticky top-0 z-20 flex h-16 items-center justify-between border-b border-slate-200 bg-white/90 px-5 backdrop-blur lg:px-8">
          <Brand className="lg:hidden" />
          <nav className="flex gap-1 overflow-x-auto lg:hidden">
            {navItems.slice(0, 3).map((item) => (
              <NavLink key={item.to} to={item.to} className="rounded-lg p-2 text-slate-600">
                <item.icon size={19} />
                <span className="sr-only">{item.label}</span>
              </NavLink>
            ))}
          </nav>
          <div className="ml-auto hidden items-center gap-3 lg:flex">
            <div className="text-right">
              <p className="text-sm font-semibold">
                {session.data?.displayName ?? 'Your workspace'}
              </p>
              <p className="text-xs text-slate-500">{session.data?.email}</p>
            </div>
            <CircleUserRound className="text-slate-400" />
            <Button
              variant="ghost"
              aria-label="Sign out"
              onClick={() => logout.mutate()}
            >
              <LogOut size={17} />
            </Button>
          </div>
        </header>
        <main className="mx-auto max-w-7xl p-5 lg:p-8">
          <Outlet />
        </main>
      </div>
    </div>
  )
}

function DashboardPage() {
  const pipeline = useQuery({ queryKey: ['pipeline'], queryFn: api.pipeline, retry: false })
  const items = pipeline.data ?? []
  const strongMatches = items.filter((item) => (item.matchScore ?? 0) >= 70).length
  const needsConfirmation = items.filter(
    (item) => item.eligibilityStatus === 'NeedsConfirmation',
  ).length
  const contacted = items.filter((item) => item.stage === 'Contacted').length

  return (
    <Page>
      <PageHeader
        eyebrow="Today"
        title="Your remote job search, clearly prioritised."
        description="Eligibility is checked separately from fit so a strong-looking role never hides a location restriction."
        action={
          <Button asChild>
            <Link to="/jobs/import">
              <Plus size={17} /> Import job
            </Link>
          </Button>
        }
      />
      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <Metric label="Jobs in pipeline" value={items.length} detail="Workspace total" />
        <Metric label="Strong matches" value={strongMatches} detail="Score 70 or above" />
        <Metric
          label="Confirm eligibility"
          value={needsConfirmation}
          detail="Evidence needs review"
        />
        <Metric label="Contacted" value={contacted} detail="Application tracked separately" />
      </div>
      <div className="mt-7 grid gap-6 xl:grid-cols-[1.4fr_.6fr]">
        <Card>
          <SectionTitle title="Recent opportunities" description="Latest pipeline activity" />
          {pipeline.isError ? (
            <EmptyState
              title="Sign in to load your workspace"
              description="The API protects every pipeline record with workspace membership."
            />
          ) : items.length ? (
            <JobList items={items.slice(0, 5)} />
          ) : (
            <EmptyState
              title="No jobs yet"
              description="Import a real posting to assess eligibility and professional fit."
              action={
                <Button asChild variant="secondary">
                  <Link to="/jobs/import">Import your first job</Link>
                </Button>
              }
            />
          )}
        </Card>
        <Card className="bg-slate-950 text-white">
          <p className="text-xs font-bold uppercase tracking-[.2em] text-emerald-300">
            How Seeka reads a role
          </p>
          <ol className="mt-5 grid gap-5">
            {[
              ['1', 'Can this employer hire you where you live?'],
              ['2', 'How closely does the work match your profile?'],
              ['3', 'Who is relevant, and what can you truthfully say?'],
            ].map(([number, text]) => (
              <li key={number} className="flex gap-3 text-sm text-slate-300">
                <span className="grid size-7 shrink-0 place-items-center rounded-full bg-white/10 text-xs font-bold text-white">
                  {number}
                </span>
                {text}
              </li>
            ))}
          </ol>
        </Card>
      </div>
    </Page>
  )
}

const setupSchema = z.object({
  fullName: z.string().min(2),
  countryOfResidence: z.string().min(2),
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

function OnboardingPage() {
  const navigate = useNavigate()
  const form = useForm<SetupValues>({
    resolver: zodResolver(setupSchema),
    defaultValues: {
      countryOfResidence: 'Ghana',
      timeZone: 'Africa/Accra',
      targetJobTitles: 'Administrative Assistant, HR Coordinator',
      seniority: 'Mid-level',
      employmentTypes: 'Full-time, Contract',
      availableWorkingHours: '08:00–18:00 GMT',
      compensationCurrency: 'USD',
      yearsOfExperience: 0,
    },
  })
  const mutation = useMutation({
    mutationFn: (values: SetupValues) =>
      api.saveCandidate({
        ...values,
        skills: splitList(values.skills),
        targetJobTitles: splitList(values.targetJobTitles),
        employmentTypes: splitList(values.employmentTypes),
        minimumCompensation: values.minimumCompensation
          ? Number(values.minimumCompensation)
          : null,
      }),
    onSuccess: () => navigate('/jobs/import'),
  })

  return (
    <Page>
      <PageHeader
        eyebrow="Profile setup"
        title="Give assessments reliable facts."
        description="Review every field before saving. Résumé extraction is not connected yet, so this working slice uses manual entry."
      />
      <form
        className="grid gap-6 xl:grid-cols-[1fr_.75fr]"
        onSubmit={form.handleSubmit((values) => mutation.mutate(values))}
      >
        <Card className="grid gap-5">
          <SectionTitle
            title="Candidate profile"
            description="Facts Seeka may use in match and outreach assessments."
          />
          <div className="grid gap-4 sm:grid-cols-2">
            <Field label="Full name">
              <Input {...form.register('fullName')} />
            </Field>
            <Field label="Country of residence">
              <Input {...form.register('countryOfResidence')} />
            </Field>
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
        </Card>
        <Card className="grid gap-5 self-start">
          <SectionTitle title="Job preferences" description="Used as fit inputs, not hard promises." />
          <Field label="Target job titles">
            <Input {...form.register('targetJobTitles')} />
          </Field>
          <Field label="Preferred employment types">
            <Input {...form.register('employmentTypes')} />
          </Field>
          <Field label="Available working hours">
            <Input {...form.register('availableWorkingHours')} />
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
          <Button disabled={mutation.isPending}>
            {mutation.isPending ? 'Saving…' : 'Save and import a job'}
          </Button>
        </Card>
      </form>
    </Page>
  )
}

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

function ImportJobPage() {
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
  const mutation = useMutation({
    mutationFn: (values: ImportValues) =>
      api.importJob({
        ...values,
        requiredSkills: splitList(values.requiredSkills ?? ''),
        salaryMinimum: values.salaryMinimum ? Number(values.salaryMinimum) : null,
        salaryMaximum: values.salaryMaximum ? Number(values.salaryMaximum) : null,
        postedAt: null,
      }),
    onSuccess: (detail) => navigate(`/jobs/${detail.savedJob.id}`),
  })

  return (
    <Page>
      <PageHeader
        eyebrow="Job discovery"
        title="Import a real job."
        description="Paste details from the original posting. Seeka stores provenance and will not invent a date, salary, or hiring geography."
      />
      <Card>
        <div className="mb-6 flex items-start gap-3 rounded-xl border border-blue-200 bg-blue-50 p-4 text-sm text-blue-800">
          <Search className="mt-0.5 shrink-0" size={18} />
          Public ATS providers are not global search engines. The first slice keeps manual
          import fully usable; configured Greenhouse boards come next.
        </div>
        <form
          className="grid gap-5"
          onSubmit={form.handleSubmit((values) => mutation.mutate(values))}
        >
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
              <Input placeholder="Recruitment, Excel, calendars" {...form.register('requiredSkills')} />
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
            <Button disabled={mutation.isPending}>
              {mutation.isPending ? 'Assessing…' : 'Import and assess job'}
            </Button>
          </div>
        </form>
      </Card>
    </Page>
  )
}

function PipelinePage() {
  const pipeline = useQuery({ queryKey: ['pipeline'], queryFn: api.pipeline, retry: false })
  const stages = [
    'Saved',
    'Researching',
    'ReadyToContact',
    'Contacted',
    'Replied',
    'Interviewing',
    'Offer',
    'Closed',
  ]
  return (
    <Page>
      <PageHeader
        eyebrow="Pipeline"
        title="Every opportunity, one next step."
        description="Formal application status remains separate from recruiter contact."
        action={
          <Button asChild>
            <Link to="/jobs/import">
              <Plus size={17} /> Add job
            </Link>
          </Button>
        }
      />
      <div className="grid gap-4 lg:grid-cols-4">
        {stages.map((stage) => {
          const jobs = (pipeline.data ?? []).filter((item) => item.stage === stage)
          return (
            <section key={stage} className="min-h-44 rounded-2xl bg-slate-100 p-3">
              <div className="flex items-center justify-between px-1 pb-3">
                <h2 className="text-sm font-bold text-slate-700">{stageLabel(stage)}</h2>
                <Badge>{jobs.length}</Badge>
              </div>
              <div className="grid gap-3">
                {jobs.map((item) => (
                  <Link key={item.savedJobId} to={`/jobs/${item.savedJobId}`}>
                    <Card className="p-4 transition hover:-translate-y-0.5 hover:border-emerald-300">
                      <p className="font-semibold">{item.title}</p>
                      <p className="mt-1 text-sm text-slate-500">{item.companyName}</p>
                      <div className="mt-3 flex gap-2">
                        <EligibilityBadge status={item.eligibilityStatus} />
                        {item.matchScore !== undefined && <Badge tone="blue">{item.matchScore}% fit</Badge>}
                      </div>
                    </Card>
                  </Link>
                ))}
              </div>
            </section>
          )
        })}
      </div>
    </Page>
  )
}

type ContactValues = {
  name: string
  title: string
  professionalProfileUrl: string
  professionalEmail: string
  relevanceReason: string
}

function JobDetailPage() {
  const { id = '' } = useParams()
  const queryClient = useQueryClient()
  const detail = useQuery({ queryKey: ['job', id], queryFn: () => api.job(id) })
  const stageMutation = useMutation({
    mutationFn: (stage: string) => api.updateStage(id, stage),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['job', id] })
      queryClient.invalidateQueries({ queryKey: ['pipeline'] })
    },
  })
  const contactForm = useForm<ContactValues>({
    defaultValues: {
      name: '',
      title: '',
      professionalProfileUrl: '',
      professionalEmail: '',
      relevanceReason: '',
    },
  })
  const contactMutation = useMutation({
    mutationFn: (values: ContactValues) => api.addContact(id, values),
    onSuccess: () => {
      contactForm.reset()
      queryClient.invalidateQueries({ queryKey: ['job', id] })
    },
  })
  const draftMutation = useMutation({
    mutationFn: (contactId: string) => api.generateDrafts(id, contactId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['job', id] }),
  })
  const recordMutation = useMutation({
    mutationFn: () =>
      api.recordActivity(id, {
        type: 'outreach_copied',
        description: 'Recorded outreach as copied or sent manually.',
        idempotencyKey: crypto.randomUUID(),
      }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['job', id] }),
  })
  const updateDraftMutation = useMutation({
    mutationFn: ({
      draftId,
      value,
    }: {
      draftId: string
      value: { subject?: string; body: string; isApproved: boolean }
    }) => api.updateDraft(draftId, value),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['job', id] }),
  })

  if (detail.isLoading) return <Loading />
  if (detail.isError || !detail.data)
    return <EmptyState title="Job not found" description="It may belong to another workspace." />
  const data = detail.data
  const components = parseComponents(data)

  return (
    <Page>
      <PageHeader
        eyebrow={data.job.companyName}
        title={data.job.title}
        description={data.job.locationText ?? 'Location not disclosed'}
        action={
          <Button asChild variant="secondary">
            <a href={data.job.originalUrl} target="_blank" rel="noreferrer">
              Original source
            </a>
          </Button>
        }
      />
      <div className="grid gap-6 xl:grid-cols-[1.25fr_.75fr]">
        <div className="grid gap-6">
          <Card>
            <SectionTitle title="Geographic eligibility" description="Separate from job fit" />
            <div className="mt-5 flex items-center gap-3">
              <EligibilityBadge status={data.eligibility?.status} />
              <span className="text-xs text-slate-500">
                Rules assessment · checked{' '}
                {data.eligibility ? new Date(data.eligibility.checkedAt).toLocaleString() : '—'}
              </span>
            </div>
            <p className="mt-4 text-sm leading-6 text-slate-700">
              {data.eligibility?.decisionReason}
            </p>
            <div className="mt-4 grid gap-2">
              {data.eligibility?.evidenceExcerpts.map((excerpt) => (
                <blockquote
                  key={excerpt}
                  className="rounded-xl border-l-4 border-emerald-500 bg-slate-50 p-3 text-sm italic text-slate-600"
                >
                  “{excerpt}”
                </blockquote>
              ))}
            </div>
          </Card>
          <Card>
            <SectionTitle
              title={`Professional fit · ${data.match?.totalScore ?? 0}/100`}
              description={`Assessment model ${data.match?.modelVersion ?? '—'} — not a hiring probability`}
            />
            <div className="mt-5 grid gap-4">
              {components.map((component) => (
                <div key={component.Name}>
                  <div className="mb-1.5 flex justify-between text-sm">
                    <span className="font-medium">{component.Name}</span>
                    <span className="text-slate-500">
                      {component.EarnedPoints}/{component.AvailablePoints}
                    </span>
                  </div>
                  <div className="h-2 rounded-full bg-slate-100">
                    <div
                      className="h-2 rounded-full bg-emerald-600"
                      style={{
                        width: `${(component.EarnedPoints / component.AvailablePoints) * 100}%`,
                      }}
                    />
                  </div>
                  <p className="mt-1 text-xs text-slate-500">{component.Explanation}</p>
                </div>
              ))}
            </div>
            {!!data.match?.missingRequirements.length && (
              <div className="mt-5 rounded-xl bg-amber-50 p-4">
                <p className="text-sm font-semibold text-amber-900">Missing requirements</p>
                <p className="mt-1 text-sm text-amber-800">
                  {data.match.missingRequirements.join(', ')}
                </p>
              </div>
            )}
          </Card>
          <Card>
            <SectionTitle title="Job description" description="Original imported content" />
            <p className="mt-4 whitespace-pre-wrap text-sm leading-7 text-slate-700">
              {data.job.description}
            </p>
          </Card>
          <ContactAndOutreach
            data={data}
            contactForm={contactForm}
            contactMutation={contactMutation}
            draftMutation={draftMutation}
            recordMutation={recordMutation}
            onSaveDraft={(draftId, value) =>
              updateDraftMutation.mutate({ draftId, value })
            }
          />
        </div>
        <aside className="grid content-start gap-6">
          <Card>
            <SectionTitle title="Pipeline" description="Application is tracked separately" />
            <label className="mt-4 grid gap-1.5 text-sm font-medium">
              Stage
              <select
                className="rounded-xl border border-slate-200 bg-white px-3 py-2.5"
                value={data.savedJob.stage}
                onChange={(event) => stageMutation.mutate(event.target.value)}
              >
                {[
                  'Saved',
                  'Researching',
                  'ReadyToContact',
                  'Contacted',
                  'Replied',
                  'Interviewing',
                  'Offer',
                  'Closed',
                ].map((stage) => (
                  <option key={stage} value={stage}>
                    {stageLabel(stage)}
                  </option>
                ))}
              </select>
            </label>
            <p className="mt-4 text-sm text-slate-500">
              Application: {stageLabel(data.savedJob.applicationStatus)}
            </p>
          </Card>
          <Card>
            <SectionTitle title="Activity" description="Workspace audit trail" />
            <ol className="mt-5 grid gap-4">
              {data.activities.map((activity) => (
                <li key={activity.id} className="border-l-2 border-slate-200 pl-4">
                  <p className="text-sm font-medium">{activity.description}</p>
                  <p className="mt-1 text-xs text-slate-500">
                    {new Date(activity.occurredAt).toLocaleString()}
                  </p>
                </li>
              ))}
            </ol>
          </Card>
        </aside>
      </div>
    </Page>
  )
}

function ContactAndOutreach({
  data,
  contactForm,
  contactMutation,
  draftMutation,
  recordMutation,
  onSaveDraft,
}: {
  data: JobDetail
  contactForm: UseFormReturn<ContactValues>
  contactMutation: { mutate: (values: ContactValues) => void }
  draftMutation: { mutate: (contactId: string) => void }
  recordMutation: { mutate: () => void }
  onSaveDraft: (
    draftId: string,
    value: { subject?: string; body: string; isApproved: boolean },
  ) => void
}) {
  return (
    <Card>
      <SectionTitle
        title="Contact research & outreach"
        description="Manual entry is active. Email finding and sending are not connected."
      />
      <form
        className="mt-5 grid gap-3 rounded-xl bg-slate-50 p-4 md:grid-cols-2"
        onSubmit={contactForm.handleSubmit((values) => contactMutation.mutate(values))}
      >
        <Field label="Contact name">
          <Input {...contactForm.register('name')} required />
        </Field>
        <Field label="Role / title">
          <Input {...contactForm.register('title')} required />
        </Field>
        <Field label="Professional profile URL">
          <Input type="url" {...contactForm.register('professionalProfileUrl')} />
        </Field>
        <Field label="Professional email">
          <Input type="email" {...contactForm.register('professionalEmail')} />
        </Field>
        <Field label="Why this person is relevant">
          <Textarea {...contactForm.register('relevanceReason')} required />
        </Field>
        <div className="flex items-end">
          <Button className="w-full">
            <Users size={17} /> Add manual contact
          </Button>
        </div>
      </form>
      <div className="mt-5 grid gap-3">
        {data.contacts.map((contact) => (
          <div
            key={contact.id}
            className="flex flex-wrap items-center justify-between gap-3 rounded-xl border border-slate-200 p-4"
          >
            <div>
              <p className="font-semibold">{contact.name}</p>
              <p className="text-sm text-slate-500">
                {contact.title} · Email {contact.verificationStatus.toLowerCase()}
              </p>
              <p className="mt-1 text-xs text-slate-500">{contact.relevanceReason}</p>
            </div>
            <Button variant="secondary" onClick={() => draftMutation.mutate(contact.id)}>
              <Mail size={16} /> Generate drafts
            </Button>
          </div>
        ))}
      </div>
      {!!data.drafts.length && (
        <div className="mt-7 grid gap-4">
          <SectionTitle title="Drafts ready for review" description="No message is sent automatically" />
          {data.drafts.map((draft) => (
            <DraftEditor
              key={draft.id}
              draft={draft}
              onSave={(value) => onSaveDraft(draft.id, value)}
              onRecordCopy={() => recordMutation.mutate()}
            />
          ))}
        </div>
      )}
    </Card>
  )
}

function SettingsPage() {
  return (
    <Page>
      <PageHeader
        eyebrow="Settings & billing"
        title="Your account and providers."
        description="Credential-backed features stay visibly off until a real provider is configured."
      />
      <div className="grid gap-6 lg:grid-cols-2">
        <Card>
          <SectionTitle title="Connected providers" description="Server-side credentials only" />
          <div className="mt-5 grid gap-3">
            {['AI extraction and writing', 'Contact enrichment', 'Email sending', 'Live billing'].map(
              (provider) => (
                <div
                  key={provider}
                  className="flex items-center justify-between rounded-xl border border-slate-200 p-4"
                >
                  <span className="text-sm font-medium">{provider}</span>
                  <Badge>Not connected</Badge>
                </div>
              ),
            )}
          </div>
        </Card>
        <Card>
          <SectionTitle title="Subscription" description="Development environment" />
          <div className="mt-5 rounded-xl bg-emerald-50 p-5">
            <Badge tone="green">Simulated development plan</Badge>
            <p className="mt-3 text-sm leading-6 text-emerald-900">
              No live payment provider is implemented. Merchant eligibility for a Ghana-based
              business must be established before selecting one.
            </p>
          </div>
          <div className="mt-5 grid gap-3">
            <Button variant="secondary" disabled>
              Export account data — coming next
            </Button>
            <Button variant="secondary" disabled>
              Delete account — coming next
            </Button>
          </div>
        </Card>
      </div>
    </Page>
  )
}

function JobList({ items }: { items: PipelineItem[] }) {
  return (
    <div className="mt-4 divide-y divide-slate-100">
      {items.map((item) => (
        <Link
          key={item.savedJobId}
          to={`/jobs/${item.savedJobId}`}
          className="flex flex-wrap items-center justify-between gap-4 py-4"
        >
          <div>
            <p className="font-semibold">{item.title}</p>
            <p className="mt-1 text-sm text-slate-500">{item.companyName}</p>
          </div>
          <div className="flex gap-2">
            <EligibilityBadge status={item.eligibilityStatus} />
            {item.matchScore !== undefined && <Badge tone="blue">{item.matchScore}% fit</Badge>}
          </div>
        </Link>
      ))}
    </div>
  )
}

function EligibilityBadge({ status }: { status?: string }) {
  const tone =
    status === 'Eligible'
      ? 'green'
      : status === 'Ineligible'
        ? 'red'
        : status === 'NeedsConfirmation'
          ? 'amber'
          : 'slate'
  return <Badge tone={tone}>{stageLabel(status ?? 'Unknown')}</Badge>
}

function Metric({
  label,
  value,
  detail,
}: {
  label: string
  value: number
  detail: string
}) {
  return (
    <Card>
      <p className="text-sm font-medium text-slate-500">{label}</p>
      <p className="mt-3 text-3xl font-semibold tracking-tight">{value}</p>
      <p className="mt-2 text-xs text-slate-400">{detail}</p>
    </Card>
  )
}

function Page({ children }: { children: React.ReactNode }) {
  return <div className="grid gap-7">{children}</div>
}

function PageHeader({
  eyebrow,
  title,
  description,
  action,
}: {
  eyebrow: string
  title: string
  description: string
  action?: React.ReactNode
}) {
  return (
    <header className="flex flex-wrap items-end justify-between gap-5">
      <div className="max-w-3xl">
        <p className="text-xs font-bold uppercase tracking-[.2em] text-emerald-700">{eyebrow}</p>
        <h1 className="mt-2 text-3xl font-semibold tracking-tight sm:text-4xl">{title}</h1>
        <p className="mt-3 leading-7 text-slate-600">{description}</p>
      </div>
      {action}
    </header>
  )
}

function SectionTitle({ title, description }: { title: string; description: string }) {
  return (
    <div>
      <h2 className="text-lg font-semibold tracking-tight">{title}</h2>
      <p className="mt-1 text-sm text-slate-500">{description}</p>
    </div>
  )
}

function EmptyState({
  title,
  description,
  action,
}: {
  title: string
  description: string
  action?: React.ReactNode
}) {
  return (
    <div className="grid place-items-center px-4 py-12 text-center">
      <BriefcaseBusiness className="text-slate-300" size={36} />
      <p className="mt-4 font-semibold">{title}</p>
      <p className="mt-1 max-w-sm text-sm text-slate-500">{description}</p>
      {action && <div className="mt-5">{action}</div>}
    </div>
  )
}

function ErrorMessage({ error }: { error: Error }) {
  return <p className="rounded-xl bg-red-50 p-3 text-sm text-red-700">{error.message}</p>
}

function Loading() {
  return <div className="animate-pulse rounded-2xl bg-slate-100 p-12 text-slate-500">Loading…</div>
}

function splitList(value: string) {
  return value
    .split(',')
    .map((item) => item.trim())
    .filter(Boolean)
}

function stageLabel(value: string) {
  return value
    .replaceAll('_', ' ')
    .replace(/([a-z])([A-Z])/g, '$1 $2')
    .replace(/^./, (character) => character.toUpperCase())
}

type MatchComponent = {
  Name: string
  EarnedPoints: number
  AvailablePoints: number
  Explanation: string
}

function parseComponents(data: JobDetail): MatchComponent[] {
  try {
    return data.match ? (JSON.parse(data.match.componentsJson) as MatchComponent[]) : []
  } catch {
    return []
  }
}

function App() {
  return (
    <Routes>
      <Route path="/login" element={<AuthPage />} />
      <Route path="/" element={<AppLayout />}>
        <Route index element={<Navigate to="/dashboard" replace />} />
        <Route path="dashboard" element={<DashboardPage />} />
        <Route path="onboarding" element={<OnboardingPage />} />
        <Route path="jobs/import" element={<ImportJobPage />} />
        <Route path="jobs/:id" element={<JobDetailPage />} />
        <Route path="pipeline" element={<PipelinePage />} />
        <Route path="settings" element={<SettingsPage />} />
      </Route>
      <Route path="*" element={<Navigate to="/dashboard" replace />} />
    </Routes>
  )
}

export default App
