## Why

The Clean Architecture split (`restructure-clean-architecture`) put the
dependency rule in place, but the backend still slices by use case, not by
volatility. There are ten single-action classes, a separate reader path that
skips them, and minimal-API lambdas that call access interfaces directly. Six
MVP features (insider discovery, résumé extraction, fit verdicts, ATS sourcing)
will add providers and workflows. Before they land, each kind of change needs
one obvious home, and the call rules need to be enforced by tests, not by
review.

## What Changes

- Adopt Juval Löwy's volatility layers with strict, closed call rules:
  Client → Manager → Engine / ResourceAccess → Resource.
- Rename the backend projects to the layer they hold:
  - `Seeka.Domain` → `Seeka.Engines`
  - `Seeka.Application` → `Seeka.Managers`
  - `Seeka.Infrastructure` → `Seeka.ResourceAccess`
  - `Seeka.Api` stays the Client.
- Replace the ten use-case classes and the reader path with three managers,
  one per volatile workflow area:
  - `MembershipManager`: account, session, and candidate setup.
  - `JobSearchManager`: import, pipeline, job detail, stage, and activity.
  - `OutreachManager`: contacts, drafts, and draft edits.
- Reads go through managers too. Controllers never reach an accessor.
- Managers resolve the workspace from the authenticated user ID. Controllers
  pass only the user ID from claims.
- Accessors are the repositories: one interface per accessor, business verbs,
  and no generic `IRepository<T>`. `IJobReader` folds into `IPipelineAccess`,
  and `IWorkspaceMembershipReader` becomes `IMembershipAccess`.
- Move URL canonicalization and input normalization out of use cases into an
  `Intake` engine.
- Replace minimal-API `Map*` endpoints with MVC controllers (`[ApiController]`,
  `[HttpGet]`, `[HttpPost]`, ...). Routes, status codes, and JSON are
  unchanged.
- One top-level type per file. Split the multi-type files (engines, contracts,
  entities).
- Enforce the rules with architecture tests: project references, controller →
  manager only, no manager → manager, no accessor → accessor, and one type per
  file.
- Rewrite `.cursor/rules/backend-architecture.mdc` and the README architecture
  section.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

None. This is a structural refactor (`skip_specs: true`).

## Impact

- Every backend file moves or is renamed. EF migrations move to the
  `Seeka.ResourceAccess` assembly. Migration IDs are unchanged, so applied
  migrations are unaffected.
- Behavior is identical, with two improvements from `[ApiController]`:
  - A malformed JSON body now returns `400` ValidationProblemDetails instead
    of `500`.
  - Unauthenticated `/api` calls now return `401` instead of a `302` redirect
    to a login page that doesn't exist. In .NET 10, cookie auth returns
    `401` instead of redirecting for endpoints marked as API endpoints, and
    `[ApiController]` adds that marker.
- The Dockerfile, solution, and README change. The deploy workflow and
  `--project seeka-backend/Seeka.Api` commands are unchanged.
- A known bug that exists before this change and is out of scope: replaying
  an activity idempotency key returns `500`.
