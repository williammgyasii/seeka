# Seeka

Seeka is a production-minded modular monolith for Africa-based professionals
seeking remote jobs. The working slice separates geographic eligibility from
professional fit, then carries a real vacancy through manual contact research
and truthful outreach drafting.

## Implemented

- ASP.NET Core Identity registration (full name and a required phone number,
  validated with libphonenumber and stored as unverified E.164), cookie
  sign-in, lockout, and personal workspace creation
- Email verification through Cloudflare Email Sending
  (`Seeka <verify@seekajobs.tech>`): registration sends a one-day link, and every
  workspace endpoint returns 403 until the address is verified. Unverified users
  stay signed in and can resend the link or correct a mistyped address.
- Frontend route guards: signed-out visitors go to sign-in and return to the
  page they asked for; unverified accounts go to check-your-inbox, then accounts
  without a candidate profile go to onboarding. The API's `[Authorize]` remains
  the actual lock.
- Workspace-scoped candidate profiles and job preferences
- Manual job import with source provenance and URL-scheme validation
- Rules-based geographic eligibility with evidence and conflict detection
- Versioned job-fit scoring with component scores and unknown compensation
- Saved-job pipeline with application status kept separate
- Manual contacts with honest `Unknown` email verification
- Grounded email, LinkedIn, and follow-up drafts for user review
- Append-only pipeline activity
- PostgreSQL/EF Core migration, OpenAPI, Hangfire PostgreSQL storage, rate
  limiting (120 requests a minute per signed-in user or per IP on every
  endpoint, 10 a minute per IP on sign-in and sign-up; in memory per
  instance), problem responses, and JSON structured logs
- Responsive React workflow with Router, Redux Toolkit Query, React Hook Form, Zod,
  Tailwind CSS v4, and a shadcn/ui design system (Radix, Seeka "ink & citrus"
  tokens with dark-mode values)

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
  --project seeka-backend/Seeka.Api
dotnet run --project seeka-backend/Seeka.Api -- --migrate
dotnet run --project seeka-backend/Seeka.Api
```

Run the frontend in another terminal:

```bash
cd seeka-frontend
npm install
npm run dev
```

Without an email token, Development writes verification links to the API log
instead of sending them; open the logged link to verify. To send real mail
locally, keep the Email Sending token in the macOS Keychain and copy it into
user secrets:

```bash
dotnet user-secrets set "Email:CloudflareAccountId" "<cloudflare-account-id>" \
  --project seeka-backend/Seeka.Api
dotnet user-secrets set "Email:CloudflareApiToken" \
  "$(security find-generic-password -s cloudflare-seeka-email-sending -w)" \
  --project seeka-backend/Seeka.Api
```

Outside Development the token is required; the API never logs links there.

Vite proxies `/api` to `http://localhost:5099`, keeping browser cookie requests
same-origin during development. The populated `.env` file and user-secrets store
are local-only. Rotate the Neon role credential immediately if either value is
exposed; then update both stores before restarting the API.

## Tests and checks

```bash
dotnet test Seeka.slnx
dotnet list Seeka.slnx package --vulnerable --include-transitive

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

The backend follows Juval Löwy's volatility layers. Each layer is its own
project under `seeka-backend/`, and the compiler enforces the direction:

```
Seeka.Api (Client)  -->  Seeka.Managers  -->  Seeka.Engines
Seeka.ResourceAccess  -->  Seeka.Managers, Seeka.Engines
```

- `Seeka.Api` (Client): MVC controllers that call one manager per request
- `Seeka.Managers`: `MembershipManager`, `JobSearchManager`, and
  `OutreachManager` sequence each use case. Also holds the request and view
  contracts and the accessor interfaces.
- `Seeka.Engines`: deterministic eligibility, matching, outreach, and intake
  policy with no I/O
- `Seeka.ResourceAccess`: accessors (the repositories), EF Core entities,
  migrations, Identity, and Hangfire
- `seeka-frontend/src/`: `app/` (router, layout, Redux store),
  `features/<area>/` (pages, `<area>Api.ts` endpoints injected into the shared
  `baseApi`, tests), and `shared/` (UI, `baseApi`, helpers). Mutations refresh
  cached data through RTK Query tags. UI comes only from `@/shared/ui`:
  shadcn primitives, Seeka patterns on top, and color tokens in
  `src/index.css`. Lint rejects raw palette classes and deep UI imports.

`.cursor/rules/backend-architecture.mdc` and
`.cursor/rules/frontend-architecture.mdc` describe where new code goes.
`ArchitectureTests` fails if a layer gains an outer reference, a controller
injects anything but a manager, a manager calls a manager, an accessor calls an
accessor, a minimal-API endpoint is mapped, or a file declares more than one
type.

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
- secrets `CLOUDFLARE_API_TOKEN` and `NEON_CONNECTION_STRING`;
- secret `EMAIL_API_TOKEN`, a separate Cloudflare token scoped to Email Sending.
  The backend release puts it in the Worker with `wrangler secret put`, and passes
  `CLOUDFLARE_ACCOUNT_ID` and `CLOUDFLARE_PAGES_URL` as Worker variables; the
  Worker hands all three to the container.

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
