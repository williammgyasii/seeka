## Context

See proposal.md for the motivation. The current state is four projects
(`Domain`, `Application`, `Infrastructure`, `Api`). Application has one class
per write use case plus `IJobReader` for reads. Endpoints are minimal-API
lambdas that resolve the workspace through `WorkspaceContext` and call either
a use case or an access interface. EF Core 10 on Neon PostgreSQL. The JSON
contract is consumed by `seeka-frontend` and must not change.

## Goals / Non-Goals

**Goals:**
- Each layer is a project named after its Löwy role, so the compiler enforces
  the direction.
- Strict call rules, enforced by tests.
- Three managers whose boundaries follow what changes together.
- No god files: one top-level type per file, with folders per layer and area.

**Non-Goals:**
- Changing any route, status code, or JSON shape.
- Fixing the idempotency replay `500`. The auth redirect `302` is fixed as a
  side effect of `[ApiController]`.
- Adding a Utilities project. `TimeProvider` and `ILogger` are the utilities
  for now.
- Async queued manager-to-manager calls. None exist yet.

## Decisions

### Layers and projects

| Löwy layer | Project | Holds |
|---|---|---|
| Client | `Seeka.Api` | `Controllers/`, `Http/` (edge policy, exception handler, claims) |
| Manager | `Seeka.Managers` | `<Area>/<Area>Manager.cs`, `Contracts/<Area>/`, `Access/<Accessor>/` (interfaces and their records) |
| Engine | `Seeka.Engines` | `Eligibility/`, `Matching/`, `Outreach/`, `Intake/`, `Pipeline/` |
| ResourceAccess | `Seeka.ResourceAccess` | `Accessors/`, `Persistence/` (DbContext, `Entities/`, `Migrations/`), `Mapping/`, `BackgroundJobs/` |

The accessor interfaces live in `Seeka.Managers`, so ResourceAccess points
inward. This is Clean Architecture's dependency inversion inside Löwy's
layering. The alternative, a separate `Seeka.ResourceAccess.Contracts`
project, adds a project with no behavior of its own.

### Call rules (closed architecture)

- A controller depends only on managers and calls one manager method per
  request.
- A manager calls engines and accessors. It never calls another manager.
- An engine has no I/O and never references ResourceAccess. Löwy allows
  engine → accessor; Seeka forbids it so engines stay pure.
- An accessor depends only on the `DbContext` or Identity. It never calls
  another accessor and holds no business rules.

### Three managers

Grouped by volatility, not by entity:
- `MembershipManager`: identity, session, and the candidate profile. These
  change with the onboarding work (`add-search-intent-onboarding`).
- `JobSearchManager`: import, pipeline, detail, stage, and activity. These
  change with sourcing and fit verdicts.
- `OutreachManager`: contacts and drafts. These change with insider discovery
  and tailoring.

### Workspace resolution moves into managers

`WorkspaceContext` was an Application class that controllers had to call.
Strict Löwy says a client calls only managers. Each manager method takes
`Guid userId` and calls `IMembershipAccess.RequireWorkspaceAsync`, which
throws `UnauthorizedAccessException` (mapped to `401`) when there is no
membership. The controller reads `userId` from claims through
`ClaimsPrincipal.RequireUserId()`.

### Intake engine

URL validation and canonicalization decide job identity (deduplication), so
they are policy. They move to `Seeka.Engines/Intake/IntakeRules`. Managers
still check request shape (required fields), which is input validation, not
policy.

### MVC

- `AddControllers` with `JsonStringEnumConverter`.
- `SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true`, so
  validation stays in managers and blank-name and other messages are unchanged.
- `[EnableRateLimiting("auth")]` on register and login.
- `[Authorize]` on every controller except the anonymous account and health
  actions.

## Risks / Trade-offs

- **Coarser managers grow.** Three managers hold what ten classes held.
  Mitigation: each stays under about 200 lines. If one passes that, it is a
  sign of a missing engine.
- **Renamed migrations assembly.** EF identifies migrations by ID, not
  assembly, so history rows still match.
  `has-pending-model-changes` verifies the model.
- **One type per file adds files.** That is the trade for findability.
  Records next to their interface keep each folder small.
- **`[ApiController]` model-state 400s** replace the old `500` on malformed
  JSON. This is an intended improvement.
