## 1. Foundation and policy tests

- [x] 1.1 Replace template structure with API, domain/application, and test projects; verify `dotnet test` discovers the new test suite
- [x] 1.2 Add failing eligibility tests for explicit Ghana inclusion, remote-only ambiguity, explicit exclusion, and conflicting evidence; verify each fails for missing policy behavior
- [x] 1.3 Add failing match tests for component transparency and unknown compensation; verify each fails for missing policy behavior
- [x] 1.4 Add a failing tenant-isolation integration test; verify cross-workspace lookup is not returned

## 2. Backend vertical slice

- [x] 2.1 Add supported PostgreSQL, Identity, OpenAPI, and Hangfire dependencies; verify restore and vulnerability audit succeed
- [x] 2.2 Implement workspace-scoped entities, relationships, indexes, and DbContext; verify model tests pass
- [x] 2.3 Implement eligibility and match engines minimally; verify the policy tests pass
- [x] 2.4 Implement registration, sign-in, sign-out, session, and same-origin cookie protections; verify authentication in the browser workflow
- [x] 2.5 Implement profile/preferences, manual job import and assessment, pipeline, contact, outreach-draft, and activity endpoints; verify the browser integration workflow passes
- [x] 2.6 Create the initial PostgreSQL migration and explicit migration command; verify a clean database can be created

## 3. Frontend vertical slice

- [x] 3.1 Add Router, Query, form/schema, Tailwind, shadcn-compatible primitives, and test dependencies; verify install and build succeed
- [x] 3.2 Add failing component tests for authentication and the first-job workflow; verify they fail because the UI is absent
- [x] 3.3 Implement responsive authentication, onboarding, dashboard, discovery/import, job detail, contact, outreach, and pipeline routes; verify component tests pass
- [x] 3.4 Connect forms to the API with session cookies, same-origin CSRF protections, loading, validation, and honest not-connected states; verify the browser can complete the workflow

## 4. Operations and handoff

- [x] 4.1 Add Dockerfiles, Docker Compose, `.env.example`, and explicit setup commands; verify containers become healthy
- [x] 4.2 Add structured logging, rate limits, upload/URL safeguards used by the slice, and protected operational endpoints; verify focused security tests pass
- [x] 4.3 Write the README with setup, migrations, integrations, jobs, deployment, implemented/simulated/deferred features; verify commands against a clean local run
- [x] 4.4 Run backend tests, frontend tests, lint, builds, OpenSpec validation, and a browser smoke test; record any remaining limitations
