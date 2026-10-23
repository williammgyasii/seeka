## Context

Both applications are framework templates with no domain behavior or established
architecture. The workspace must become a deployable modular monolith while
keeping the first release small enough to validate one complete user journey.
See `proposal.md` for motivation and `specs/job-seeker-workflow/spec.md` for the
observable contract.

## Goals / Non-Goals

**Goals:**

- Create a runnable vertical slice whose UI, API, database, and tests agree.
- Keep policy engines independent from HTTP, EF Core, and external providers.
- Establish workspace ownership, provenance, assessments, and activity as
  foundations that later integrations can safely reuse.
- Make local setup deterministic through containers and an explicit migration
  command.

**Non-Goals:**

- Résumé binary parsing, public ATS ingestion, contact enrichment, live email,
  follow-up automation, and live billing are later changes.
- LinkedIn automation and unauthorised scraping are never part of this design.
- The first slice does not introduce microservices, event streaming, or a
  generic repository layer.

## Decisions

### Use a modular monolith organised by volatility

The API hosts account, candidate, jobs, and outreach modules in one process.
HTTP endpoints translate requests, use-case services sequence work, pure engines
own eligibility and match policy, and EF Core performs atomic resource access.
This keeps deployment simple and policy tests fast. Separate services were
rejected because no current scaling or team boundary justifies the operational
cost.

### Resolve tenancy from the authenticated account

Each account initially owns one personal workspace. Request handlers resolve its
membership from the authenticated user and apply the workspace key to every
query and insert; client-supplied workspace identifiers never grant access.
Global query filters were considered but rejected as the sole defence because
background and administrative flows can accidentally bypass them. Explicit
workspace predicates plus integration tests make the boundary visible.

### Use Identity cookies with same-origin development proxying

ASP.NET Core Identity manages password hashing and lockout. A secure HttpOnly,
SameSite=Lax cookie carries the browser session. The frontend and API share an
origin through the development and production proxies, and mutation endpoints
reject cross-site requests using Fetch Metadata and Origin checks. Bearer tokens
were rejected for the browser-first product because storing them client-side
increases exposure without providing a current benefit.

### Model the first slice explicitly

The database includes users, workspaces, memberships, profiles, preferences,
companies, postings, source records, eligibility and match assessments, saved
jobs, contacts, outreach drafts, and pipeline activities. Enums are stored as
strings and assessment input/output is stored as structured JSON where the shape
is versioned. Unique constraints cover membership, canonical source URL per
provider, one saved job per workspace, and idempotent activity keys.

Later entities named in the product brief are added with the capability that
uses them rather than as empty tables. This avoids speculative schemas while
the established workspace and provenance keys remain compatible.

### Keep eligibility and matching deterministic in version one

Eligibility extracts and classifies explicit geographic statements with
conflict detection. It never upgrades generic “remote”, “Africa”, or “EMEA” to
Eligible without sufficient evidence. Match scoring uses documented weighted
components, stores model version `v1`, and excludes unknown compensation from
earned points while preserving Unknown in the response.

An AI provider interface is reserved for later explanations and extraction.
Deterministic policy is preferred now because it is inspectable, testable, and
works without credentials.

### Generate outreach from controlled templates

The first implementation creates grounded, editable drafts from explicitly
selected profile, job, and contact fields. No network send occurs. Provider
interfaces are introduced only when a working integration is added, avoiding
shallow abstractions that rename calls.

### Keep Hangfire operational but out of user-visible first-slice behavior

Hangfire uses PostgreSQL persistent storage and exposes no unauthorised
dashboard. Its server is wired for later scheduled ingestion and messaging, but
the first slice runs synchronously because it has no long-running provider call.

## Risks / Trade-offs

- [Rule-based text classification misses nuanced wording] → Return Unknown or
  Needs confirmation, show evidence, and allow later manual/AI reassessment.
- [Cookie auth across local ports can cause CSRF/CORS mistakes] → Use the
  same-origin `/api` proxy, SameSite cookies, and server-side Fetch Metadata and
  Origin validation for mutations.
- [A broad initial schema can slow delivery] → Persist only entities exercised
  by the vertical slice and add later entities with their capabilities.
- [Template outreach can sound generic] → Keep drafts short, editable, grounded,
  and label provider connectivity honestly.
- [PostgreSQL-dependent integration tests are heavier] → Keep policy tests pure
  and use container-backed tests only for tenancy and persistence boundaries.

## Migration Plan

1. Add projects, dependencies, policy tests, and application code.
2. Create the initial PostgreSQL migration and explicit `db migrate` command.
3. Start PostgreSQL with Docker Compose, apply migrations, and run API tests.
4. Start the API and frontend, exercise the full browser workflow, then document
   implemented and deferred integrations.
5. Roll back this pre-production change by removing its containers and schema;
   there is no existing production data to migrate.
