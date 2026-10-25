## 1. Rules

- [x] 1.1 Add `backend-architecture.mdc` and `frontend-architecture.mdc` project rules and verify they are scoped to `seeka-backend/**` and `seeka-frontend/**`

## 2. Backend projects

- [x] 2.1 Create `Seeka.Domain`, `Seeka.Application`, `Seeka.Infrastructure`, and `Seeka.Api` projects with inward-only references and verify `dotnet build Seeka.slnx` succeeds
- [x] 2.2 Split Domain into `Eligibility/`, `Matching/`, `Outreach/`, and `Pipeline/` and verify the existing engine tests pass unchanged apart from namespaces
- [x] 2.3 Move persistence to `Infrastructure/Persistence` and verify `dotnet ef migrations has-pending-model-changes` reports no changes
- [x] 2.4 Add an architecture test that fails if Domain or Application reference EF Core or ASP.NET Core, and verify it passes

## 3. Use cases and access

- [x] 3.1 Extract `ImportJob` behind `IJobAccess` with fake-access unit tests and verify they pass
- [x] 3.2 Extract the candidate, pipeline, outreach, and account use cases behind access interfaces and verify `FirstSliceManager` is deleted
- [x] 3.3 Serve pipeline, job detail, and candidate reads through Application reader interfaces returning Application records and verify the workspace isolation test passes against the new reader

## 4. API

- [x] 4.1 Move endpoints into `Endpoints/<Area>Endpoints.cs`, leaving `Program.cs` as composition only, and verify every route from the previous `Program.cs` is still mapped

## 5. Delivery

- [x] 5.1 Update the Dockerfile, solution, deploy workflow, and README for `seeka-backend/Seeka.Api` and verify `docker compose build api` succeeds

## 6. Frontend

- [x] 6.1 Add the `@/` alias and `shared/` (ui, api, lib) and verify `npm run build` succeeds
- [x] 6.2 Move each page from `App.tsx` into `features/<area>/` with its query hooks and verify `npm test`, `npm run lint`, and `npm run build` pass
- [x] 6.3 Exercise sign-in, onboarding, import, pipeline, and job detail in the running app and verify they behave as before
