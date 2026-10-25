## Why

Six MVP features are about to land on a codebase where one 552-line
`FirstSliceManager` sequences every use case and queries EF Core directly,
`Program.cs` holds every endpoint, and `App.tsx` holds every page. Application
depends on Infrastructure, which is the wrong direction. Nothing stops Domain
from taking a database dependency. Restructuring now is cheaper than after
OpenAI, enrichment, and ATS providers are added.

## What Changes

- Split the backend into four projects under `seeka-backend/`:
  `Seeka.Domain`, `Seeka.Application`, `Seeka.Infrastructure`, `Seeka.Api`.
  The compiler enforces the dependency rule.
- Split Domain engines into `Eligibility/`, `Matching/`, `Outreach/`, and
  `Pipeline/`.
- Replace `FirstSliceManager` with one class per write use case. Each depends
  on access interfaces that Application declares and Infrastructure implements.
- Serve reads through Application reader interfaces implemented in
  Infrastructure. API responses use Application records instead of EF
  entities. Property names are unchanged. Unused nested navigation properties
  are no longer serialized.
- Move endpoints from `Program.cs` into `Endpoints/<Area>Endpoints.cs`.
- Restructure the frontend into `app/`, `features/<area>/`, and `shared/` with
  an `@/` import alias.
- Add project rules `.cursor/rules/backend-architecture.mdc` and
  `.cursor/rules/frontend-architecture.mdc`.
- Update the Dockerfile, solution, deploy workflow, and README for the new
  project paths.

No user-visible behavior changes.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

None. Pure refactor (`skip_specs: true`).

## Impact

- All backend source files move. The EF migrations assembly becomes
  `Seeka.Infrastructure`, and the published entry point becomes `Seeka.Api.dll`.
- Commands change from `--project seeka-backend` to
  `--project seeka-backend/Seeka.Api`.
- The frontend `App.tsx` is split into feature folders. Routes and UI are
  unchanged.
