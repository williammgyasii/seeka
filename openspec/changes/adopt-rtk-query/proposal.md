## Why

The frontend fetches through about twelve TanStack Query hooks. Each mutation
lists by hand which cache keys to refresh, and the outreach hooks need to know
the jobs feature's key shape to do it. Every feature will add endpoints:
insider discovery, résumé extraction, fit verdicts, and ATS sourcing. We want
one API definition that each feature extends, and cache refreshes declared as
data rather than written as callbacks.

## What Changes

- Replace TanStack Query with Redux Toolkit Query on a Redux store.
- Add one `baseApi` in `shared/api` (`createApi` plus `fetchBaseQuery`) with
  no endpoints of its own. It declares the tag types `Session`, `Pipeline`, and
  `Job`.
- Each feature adds its endpoints with `baseApi.injectEndpoints` in its own
  `<feature>Api.ts`. Components use the generated hooks
  (`useGetJobQuery`, `useAddContactMutation`, and so on).
- Replace manual invalidation with tags:
  - Queries provide tags: `getJob` provides `{ type: 'Job', id }`.
  - Mutations invalidate them: `addContact` invalidates `{ type: 'Job', id }`.
  - Features share tag names, not each other's modules.
- Signing in writes the session into the cache. Signing out resets the whole
  API cache.
- One `errorMessage(error)` helper turns RTK Query errors (ProblemDetails,
  network errors) into display text. `ErrorMessage` uses it.
- A validation problem now shows its reasons. A weak password shows "Passwords
  must have at least one digit…" instead of "One or more validation errors
  occurred."
- Importing a job now refreshes the pipeline. Before, the dashboard could show
  stale data for up to 30 seconds.
- Network tests use MSW (Mock Service Worker) against a real store.

## Not changing

- No Redux slices. Every piece of shared state today is server state, and RTK
  Query owns it. A slice is added only when real client state appears (for
  example, a multi-step onboarding draft).
- No frontend repository interfaces. The injected endpoints are the frontend's
  repositories, mirroring "accessors are the repositories" on the backend. A
  wrapper interface would only rename calls.
- Routes, payloads, and screens are unchanged. Hand-written `types.ts` stays.
  OpenAPI code generation is a possible later change.

## Impact

- `seeka-frontend`:
  - Dependencies: adds `@reduxjs/toolkit`, `react-redux`, and `msw` (dev);
    removes `@tanstack/react-query`.
  - Files: `shared/api`, every feature's `api.ts`, `main.tsx`, `ErrorMessage`,
    and the pages that call hooks.
- `.cursor/rules/frontend-architecture.mdc` and the README describe the new
  data layer.
