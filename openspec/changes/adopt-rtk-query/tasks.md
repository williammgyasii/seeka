## 1. Failing tests

- [x] 1.1 Add `errorMessage` tests for ProblemDetails, validation reasons, network errors, serialized errors, and the fallback
- [x] 1.2 Add MSW and store tests for the cache contract: signing in caches the session, signing out clears the cache, import refreshes the pipeline (subscribed and on return), stage change refreshes the job and pipeline, and outreach mutations refresh the job
- [x] 1.3 Render `App.test.tsx` inside the Redux `Provider` and verify the suite fails

## 2. Data layer

- [x] 2.1 Add `@reduxjs/toolkit`, `react-redux`, and `msw`; add `baseApi`, `app/store.ts`, and `errorMessage`
- [x] 2.2 Inject the endpoints per feature (`authApi`, `jobsApi`, `pipelineApi`, `onboardingApi`, `outreachApi`)

## 3. Components

- [x] 3.1 Move `main.tsx`, `AppLayout`, and every page to the generated hooks, and make `ErrorMessage` take `unknown`
- [x] 3.2 Remove `@tanstack/react-query`, `queryKeys`, `client.ts`, and the old `api.ts` files

## 4. Delivery

- [x] 4.1 Update `.cursor/rules/frontend-architecture.mdc` and the README
- [x] 4.2 Run the tests, lint, and build, then exercise sign-in, import, stage change, outreach, and sign-out in the running app
