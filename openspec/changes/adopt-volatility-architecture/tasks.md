## 1. Failing tests

- [x] 1.1 Rewrite `ArchitectureTests` for the Löwy rules (project references, controller → manager only, no manager → manager, no accessor → accessor, one top-level type per file) and verify they fail to compile or fail
- [x] 1.2 Replace `ImportJobTests` with `JobSearchManagerTests` (import plus workspace resolution) and add `MembershipManagerTests`, and verify they fail

## 2. Engines

- [x] 2.1 Rename `Seeka.Domain` to `Seeka.Engines`, split it to one type per file, and add `Intake/IntakeRules`

## 3. Managers

- [x] 3.1 Rename `Seeka.Application` to `Seeka.Managers`, split the contracts and access records to one type per file, and fold `IJobReader` into `IPipelineAccess` and `IWorkspaceMembershipReader` into `IMembershipAccess`
- [x] 3.2 Replace the use-case classes with `MembershipManager`, `JobSearchManager`, and `OutreachManager`

## 4. Resource access

- [x] 4.1 Rename `Seeka.Infrastructure` to `Seeka.ResourceAccess`, split the entities, move the accessors into `Accessors/`, and verify `has-pending-model-changes` reports none

## 5. Client

- [x] 5.1 Replace `Endpoints/` with MVC controllers and verify every route, status code, and JSON shape against the running API

## 6. Delivery and rules

- [x] 6.1 Update the solution, Dockerfile, test project, README, and `.cursor/rules/backend-architecture.mdc`
- [x] 6.2 Run `dotnet test`, the vulnerability scan, and `dotnet publish`, then smoke-test the API and UI end to end
