## Why

Seeka currently consists of untouched frontend and backend templates, so a job
seeker cannot complete any product workflow. The first release needs one honest,
secure path from account creation through a recorded recruiter outreach draft
before provider integrations or automation are added.

## What Changes

- Replace the template applications with a working, responsive SaaS workflow.
- Add cookie-based registration and sign-in with a personal workspace.
- Add candidate profiles and job preferences for country, timezone, roles,
  skills, working hours, employment type, and minimum compensation.
- Add manual job import with source provenance and workspace-scoped persistence.
- Add deterministic, versioned geographic eligibility and job-fit assessments
  that expose evidence, component scores, and missing requirements.
- Add saved-job pipeline stages, manual contacts, editable outreach drafts, and
  an append-only activity history.
- Add PostgreSQL persistence, explicit migrations, local Docker Compose,
  OpenAPI, structured logging, and focused automated tests.
- Establish provider interfaces for later AI, ATS, contact enrichment, email,
  and billing integrations without presenting any provider as connected.

## Capabilities

### New Capabilities

- `job-seeker-workflow`: Account access, personal workspace ownership,
  candidate setup, manual job assessment, pipeline tracking, contacts, outreach
  drafts, and activity recording for the first end-to-end workflow.

### Modified Capabilities

None.

## Impact

- Replaces the Vite starter UI and ASP.NET weather endpoint.
- Adds React Router, TanStack Query, React Hook Form, Zod, Tailwind CSS,
  shadcn-compatible UI primitives, and frontend tests.
- Adds ASP.NET Core Identity, EF Core with PostgreSQL, Hangfire PostgreSQL
  storage, OpenAPI, domain/application modules, migrations, and backend tests.
- Adds a local PostgreSQL container, environment contract, setup command, and
  project documentation at the workspace root.
