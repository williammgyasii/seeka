## Context

All shared frontend state is server state: the session, the pipeline, and one
job's detail. The only client state is local `useState` in `AuthPage` (the
login or register toggle) and `DraftEditor` (unsaved text). Requests go through
the Vite proxy (dev) or the same origin (prod) with the `__Host-` session
cookie.

## Decisions

### One empty `baseApi`, endpoints injected per feature

```
shared/api/baseApi.ts        createApi({ baseUrl: '/api', tagTypes, endpoints: () => ({}) })
shared/api/pipelineApi.ts    getPipeline              (dashboard and jobs both read it)
features/auth/authApi.ts     getSession, signIn, register, signOut
features/jobs/jobsApi.ts     getJob, importJob, changeStage
features/onboarding/onboardingApi.ts  saveCandidate
features/outreach/outreachApi.ts      addContact, generateDrafts, updateDraft, recordOutreach
app/store.ts                 configureStore with baseApi.reducer and baseApi.middleware
```

One `createApi` means one cache, one middleware, and tags that work across
features. With several `createApi` calls, outreach could not invalidate a job
cached by another API slice.

### Tags

| Endpoint | Provides | Invalidates |
|---|---|---|
| `getSession` | `Session` | |
| `getPipeline` | `Pipeline` | |
| `getJob(id)` | `Job:id` | |
| `importJob` | | `Pipeline` |
| `changeStage` | | `Job:id`, `Pipeline` |
| `addContact`, `generateDrafts`, `updateDraft`, `recordOutreach` | | `Job:id` |
| `signIn`, `register` | writes the `getSession` cache entry | |
| `signOut` | resets the whole API state | |

`updateDraft` takes `savedJobId` in its argument so it can name the job tag,
even though the URL only needs `draftId`.

### Errors

`fetchBaseQuery` returns errors as values, not thrown `Error`s:
`{ status: number, data }` for HTTP errors, and `{ status: 'FETCH_ERROR',
error }` for network failures. `errorMessage(error: unknown)` reads, in order:
ProblemDetails `title`, then `error`, then `Request failed (<status>)`, then
`message` for serialized or thrown errors. `ErrorMessage` takes `unknown` and
calls it.

### Mutations in components

Components `await` the trigger and check the result instead of calling
`.unwrap()`. `.unwrap()` rejects on error, and the component would need a
`catch` only to silence it. The hook's `error` already drives the message.

```tsx
const result = await importJob(body)
if (!result.error) navigate(`/jobs/${result.data.savedJob.id}`)
```

The result type is `{ data, error?: undefined } | { data?: undefined, error }`.
Checking `error` narrows it; `'data' in result` does not.

### Cache timing

TanStack used `staleTime: 30_000` and no refetch on focus. The RTK Query
equivalent is `refetchOnMountOrArgChange: 30` on `baseApi`. RTK Query does not
refetch on focus unless `setupListeners` is called, so we don't call it.

### Testing

- `errorMessage` gets plain unit tests.
- Tag behavior gets store tests: a real store from `src/test/store.ts`, MSW
  handlers that count requests, a query subscription held open, then the
  mutation. The test asserts the query was fetched again. This tests the
  contract (which cache entries refresh), not RTK Query internals.
- Component tests render inside `<Provider store>`.

## Risks

- **Relative URLs in Node.** `fetchBaseQuery` builds a `Request`, and Node
  rejects relative URLs. In tests the base URL must resolve against
  `window.location.origin`.
- **Coupling by tag name.** The `Job` tag is a shared string. Renaming it
  silently stops refreshes, and the store tests catch that.
