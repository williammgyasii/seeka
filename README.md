# Seeka

Seeka is a production-minded modular monolith for Africa-based professionals
seeking remote jobs. The working slice separates geographic eligibility from
professional fit, then carries a real vacancy through manual contact research
and truthful outreach drafting.

## Implemented

- ASP.NET Core Identity registration, cookie sign-in, lockout, and personal
  workspace creation
- Workspace-scoped candidate profiles and job preferences
- Manual job import with source provenance and URL-scheme validation
- Rules-based geographic eligibility with evidence and conflict detection
- Versioned job-fit scoring with component scores and unknown compensation
- Saved-job pipeline with application status kept separate
- Manual contacts with honest `Unknown` email verification
- Grounded email, LinkedIn, and follow-up drafts for user review
- Append-only pipeline activity
- PostgreSQL/EF Core migration, OpenAPI, Hangfire PostgreSQL storage, rate
  limiting, problem responses, and JSON structured logs
- Responsive React workflow with Router, TanStack Query, React Hook Form, Zod,
  Tailwind CSS, and shadcn-compatible UI primitives

## Honest development states

- Subscription is clearly labelled as simulated; no payment is taken.
- Résumé extraction, public ATS ingestion, AI, contact enrichment, connected
  email, replies, and automatic follow-ups are not connected.
- LinkedIn drafts are copy-only. Seeka does not automate LinkedIn.
- No integration is presented as live without credentials and provider events.

## Requirements

- Docker Desktop with Compose
- Neon CLI authenticated to the development organisation
- .NET 10 SDK
- Node.js 24+ and npm

.NET 10 is an active LTS release through November 2028. Package versions were
checked against the official .NET support policy, Microsoft ASP.NET Core 10
Identity guidance, npm registry releases, and Hangfire's ASP.NET Core guidance.
`Hangfire.PostgreSql` is a community storage adapter because Hangfire's official
storage documentation does not ship a first-party PostgreSQL adapter.

## Docker setup

```bash
cp .env.example .env
# Populate ConnectionStrings__Seeka with the direct Neon connection string.
docker compose build
docker compose run --rm api --migrate
docker compose up -d api web
```

Open the app at `http://localhost:5173`. The API is also available at
`http://localhost:5099`; in Development its OpenAPI document is
`/openapi/v1.json`.

Migrations never run implicitly on application startup. The explicit setup
command is:

```bash
docker compose run --rm api --migrate
```

## Host development

Store the direct Neon connection in ASP.NET Core user secrets, then apply
migrations and run the API:

```bash
dotnet user-secrets set \
  "ConnectionStrings:Seeka" \
  "Host=your-neon-host;Port=5432;Database=seeka;Username=seeka_owner;Password=...;SSL Mode=Require" \
  --project seeka-backend
dotnet run --project seeka-backend -- --migrate
dotnet run --project seeka-backend
```

Run the frontend in another terminal:

```bash
cd seeka-frontend
npm install
npm run dev
```

Vite proxies `/api` to `http://localhost:5099`, keeping browser cookie requests
same-origin during development. The populated `.env` file and user-secrets store
are local-only. Rotate the Neon role credential immediately if either value is
exposed; then update both stores before restarting the API.

## Tests and checks

```bash
dotnet test Seeka.slnx
dotnet list seeka-backend/seeka-backend.csproj package --vulnerable --include-transitive

cd seeka-frontend
npm test
npm run lint
npm run build

cd ..
openspec validate build-first-working-vertical-slice --type change --strict
```

Focused backend tests cover explicit Ghana inclusion, remote-only ambiguity,
location conflicts, incompatible residence requirements, unknown salary,
transparent match components, workspace isolation, and grounded outreach.

## Architecture

- `Program.cs`: HTTP translation, authentication, and endpoint mapping
- `Application/`: one-use-case sequencing and workspace resolution
- `Domain/`: deterministic eligibility, match, and outreach policy with no web
  or database dependency
- `Infrastructure/`: EF Core entities, constraints, migrations, and atomic
  PostgreSQL access
- `seeka-frontend/src/`: routed workflow, API client, forms, and reusable UI

Every resource query derives its workspace from the authenticated Identity user
and membership. A client-supplied workspace ID never grants access.

## Background jobs and integrations

Hangfire uses the same PostgreSQL server with persistent tables. Its dashboard
is intentionally not exposed. The first slice has no provider call that needs a
background job; scheduled ATS refresh, matching, notifications, and opted-in
follow-ups belong to later OpenSpec changes.

Next integration order:

1. Configured-company Greenhouse public job boards (not global search)
2. Scheduled availability checks and matching
3. Authorised contact enrichment with caching and usage accounting
4. Connected email with signed webhooks and reconciliation
5. Explicitly opted-in follow-up sequences with stop-condition checks
6. Entitlements and atomic usage metering
7. Live billing only after Ghana merchant eligibility is established

## Deployment notes

Production uses Cloudflare Pages for the React application and a private Worker
that routes to the ASP.NET Core Cloudflare Container. A Pages Function proxies
`/api/*` through a service binding, so secure cookies remain on the generated
`https://seeka-cth.pages.dev` origin. Terraform under `infra/cloudflare` owns stable
Pages and Worker identities; Wrangler owns application versions, container
images, and Pages artifacts.

Component CI workflows run focused checks for backend, frontend, edge, and
Terraform changes on pull requests and `main`. Commits to `main` do not deploy.
Publishing an immutable GitHub release whose tag starts with `v` independently
triggers:

- infrastructure reconciliation with Terraform;
- database migration followed by Worker/container deployment;
- frontend and Pages Function deployment.

Provision infrastructure before the first release. Later infrastructure
changes must remain backward-compatible because release workflows are
independent and may finish in any order.

The protected GitHub `production` environment requires:

- variable `CLOUDFLARE_ACCOUNT_ID`;
- variable `CLOUDFLARE_PAGES_URL`;
- secrets `CLOUDFLARE_API_TOKEN` and `NEON_CONNECTION_STRING`.

The Cloudflare token must be deployment-scoped and permitted to delegate R2
access. Infrastructure CD derives one-hour credentials restricted to object
read/write access on `seeka-terraform-state`; it masks them and never stores
them as GitHub secrets. Rotate the parent token in Cloudflare and GitHub
together. Never place credentials in Terraform variables, plans, repository
files, logs, or browser environment variables.

Release rollback means restoring a previous Pages deployment and Worker
version. Database migrations are forward-only release steps and must use
expand/contract changes rather than destructive rollback. Cloudflare Containers,
Workers Paid, Pages Functions, R2 state storage, Neon, and provider traffic may
incur usage charges.

ASP.NET Data Protection keys persist in PostgreSQL so sessions survive container
replacement. Hangfire storage remains configured, but its processing server is
disabled in Cloudflare because scale-to-zero cannot guarantee durable polling.
Do not enqueue production background work until an always-on or
Cloudflare-native scheduler is implemented.
