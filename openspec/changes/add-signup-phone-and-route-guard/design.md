## Context

Identity cookie sessions, lockout after 5 failures, `[Authorize]` controllers, and
the `Sec-Fetch-Site` check exist (see proposal). `AppUser` inherits Identity's
nullable `PhoneNumber` column. `RegisterAsync` throws `ArgumentException` for a
blank name, which surfaces as a generic 400 rather than a field error. In
`Program.cs`, `UseRateLimiter()` runs before `UseAuthentication()`. The frontend
renders `AppLayout` for every protected route without checking the session.

## Goals / Non-Goals

**Goals:**
- Required, country-valid phone numbers stored as E.164, with field-level errors.
- Visitors land on the right page for their session state.
- Every endpoint has a per-caller limit.

**Non-Goals:**
- JWT, refresh tokens, or a second auth scheme. Revisit when a mobile app or third-party API client exists.
- SMS verification or unique phone numbers.
- Confirm-password field, HSTS from the API, per-email limits (lockout already
  covers per-account guessing), and a Cloudflare edge rate-limit rule. The edge
  rule is the follow-up that makes limits hold across container instances.
- Asking existing accounts for a phone number.

## Decisions

### Phone policy is an engine

`Seeka.Engines/Accounts/PhoneNumberPolicy` parses with `libphonenumber-csharp`,
checks `IsValidNumber`, and returns the E.164 string or nothing. It has no I/O, so
it fits the engine rules and is tested directly. The client sends E.164 already;
the server re-validates because the browser can be bypassed.

Alternative: a regex for `^\+[1-9]\d{7,14}$`. Rejected: it accepts numbers no
country issues.

### Registration validates before touching the database

`MembershipManager.RegisterAsync` checks the full name and phone first and returns
`AccountCreation.Rejected` with `displayName` / `phoneNumber` keys, the same shape
Identity's password errors already use. The controller already turns `Rejected`
into a `ValidationProblem`, and the frontend's `errorMessage` already lists
`errors`. Nothing is created on rejection. `AccountAccess` gains a phone argument
and sets `AppUser.PhoneNumber`.

### Global limiter partitions by user, then IP

```
GlobalLimiter: fixed window, 120 requests / minute
  partition = user id claim when authenticated, else remote IP
"auth" policy (unchanged): 10 / minute per IP on login and register
```

Both run; the stricter wins on auth endpoints. `UseAuthentication()` moves before
`UseRateLimiter()` so the global limiter can read the user id. Limits are in
memory per instance, so N instances allow N times the limit; the edge rule is the
fix. 120 per minute is roomy for one person clicking through the app (a page load
is 1 to 3 requests) and tight for a script.

Alternative: one shared global bucket. Rejected: one abusive caller would block
everyone.

### Route guards are layout routes; routing follows the session

Two layout routes read `useGetSessionQuery()` (now in `shared/api/sessionApi`,
because the layout, both guards, and onboarding all need it):

- `app/RequireSession.tsx` wraps `AppLayout`. Loading shows `Loading`; a 401
  renders `<Navigate to="/login" state={{ from }} />`; another error shows the
  error instead of pretending the user is signed out; otherwise `<Outlet />`.
- `app/RedirectIfSignedIn.tsx` wraps `/login` and sends a signed-in visitor to
  `from`, or `/dashboard`.

`AuthPage` no longer navigates. Sign-in and sign-up write the session into the
cache, `RedirectIfSignedIn` moves the visitor to `from`, and `RequireSession`
sends a new account on to onboarding. One source of truth avoids a race between
a form's `navigate()` and the guard's redirect. The guards are user experience,
not security: `[Authorize]` on the API is the lock, so bypassing them yields 401s
and no data.

### Onboarding gate reads the session

`SessionResponse` gains `OnboardingComplete`, computed in `AccountAccess` from
whether the user's workspace has a candidate profile row. The session endpoint
already runs on every app load, so the gate costs no extra request, and the flag
comes from the store that owns the rows rather than from client state.
`RequireSession` sends a user with `onboardingComplete: false` to `/onboarding`
from any other protected path. When `saveCandidate` succeeds it patches the cached
session's flag to `true` at once and invalidates the `Session` tag. Invalidation
alone would race: onboarding navigates to job import before the refetch returns,
and the guard would bounce the user back on the stale `false`. The patch reflects
a write the server just confirmed; the refetch then replaces it with the server's
own answer.

Alternative: the guard queries `GET /api/candidate` separately. Rejected: a
second request on every load and a second loading state for one boolean.

### Sessions, stated precisely

The Identity cookie is an encrypted, signed ticket, not a server-side session
row. Sign-out deletes it from the browser; a copied cookie stays valid until
expiry (12 hours, sliding) unless the security stamp changes, which Identity
re-validates every 30 minutes. A server-side ticket store (`ITicketStore`) is the
path if instant revocation of stolen cookies is needed later.

### Phone field

`features/auth/PhoneNumberField.tsx`: a country `Select` showing
"Portugal +351" (codes from `libphonenumber-js`, names from `Intl.DisplayNames`)
and a plain `tel` input. The default country comes from the
device timezone. On submit the form sends `parsePhoneNumber(...).number` (E.164);
the zod schema checks `isValidPhoneNumber` so most errors show before the request.
The timezone-to-country code moves from `features/onboarding/residence.ts` to
`shared/lib/countries.ts`, because features may not import each other.

## Risks / Trade-offs

- [`libphonenumber-js` adds bundle weight] → Measured: the single JS chunk grew
  from 658 kB to 783 kB minified (240 kB gzipped), with the default `min`
  metadata. Follow-up: lazy-load the sign-up form or split routes, which also
  clears Vite's 500 kB chunk warning.
- [Client password check is weaker than Identity's] → The form checks length 10;
  Identity also requires an uppercase letter, so that error appears only after
  submit. Follow-up: mirror the server rules in the zod schema.
- [Google's phone metadata changes as countries renumber] → Keep both libraries
  updated; the server is the authority if they disagree.
- [Required phone but existing accounts have none] → Code reading
  `PhoneNumber` treats it as nullable.
- [In-memory limits are per instance] → Edge rule follow-up.
