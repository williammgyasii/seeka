## Context

See proposal.md for why. Current state that shapes the approach:

- **Identity:** `AppUser` already has `EmailConfirmed`, and Identity's default
  token provider can generate and check confirmation tokens. Registration signs
  the person in immediately.
- **Production runtime:** the API runs as a Cloudflare container behind the
  `seeka-api` Worker. Secrets travel as GitHub secret → `wrangler secret put` →
  `containerEnvironment()` → container environment variable.
- **No background jobs in production:** `BackgroundJobPolicy` turns the Hangfire
  server off in the `Cloudflare` environment, so a queued job there would never
  run.
- **Routing:** the session endpoint already drives every redirect in the web app
  (`RequireSession`).

## Goals / Non-Goals

**Goals:**
- The API enforces the gate. The web app only explains it.
- Every path out of "unverified" works without support: resend and correct.

**Non-Goals:**
- Phone (SMS) verification.
- Changing a verified email. That needs re-verification of the old address and
  belongs in Settings later.
- Password reset by email. It reuses the email access built here, but is its own
  change.
- An email outbox or retries.

## Decisions

### "Hard gate" means signed in but locked out, not refused at sign-in

Identity's `RequireConfirmedEmail` would refuse sign-in outright. Then resend and
correct-address would need anonymous endpoints that take an email address, which
lets anyone probe which addresses have accounts (account enumeration) and lets
anyone trigger mail to any address. Instead the person signs in as usual, and
every workspace endpoint refuses them until they verify. Nothing in the workspace
is reachable, so the gate is just as hard. The difference is that the recovery
actions are tied to the session.

### The gate is a default authorization policy that reads the database

- A requirement `EmailVerified` becomes the fallback policy for `[Authorize]`.
- Endpoints a person needs before verifying are marked
  `[Authorize(Policy = Policies.SignedIn)]`, which only checks the session:
  - `GET /api/auth/session`
  - `POST /api/auth/logout`
  - `POST /api/auth/verification-email` (resend)
  - `PUT /api/auth/email` (correct the address)
- `POST /api/auth/verify-email` is anonymous.
- The handler asks `MembershipManager.IsEmailVerifiedAsync(userId)`. That is one
  indexed lookup per request, cached for the request.

Alternative: put an `email_verified` claim in the cookie. Rejected. The cookie
would say "unverified" until it was refreshed, and verifying on a phone can't
refresh the laptop's cookie. The database owns the truth, so the gate reads it.

### Sending is inline and failure-tolerant

Production has no job runner (see Context), so the manager sends during the
request. If the provider fails, registration still succeeds and the failure is
logged. The resend button is the recovery. A job or outbox can be added when
production runs a worker.

### Email access is a provider boundary

- `Seeka.Managers/Access/Email/IEmailAccess.SendAsync(OutgoingEmail)`.
- `Seeka.ResourceAccess/Accessors/CloudflareEmailAccess` posts to
  `/accounts/{id}/email/sending/send` with a typed `HttpClient` and a bearer
  token. It treats 429 and 5xx as failures to log; there is no retry loop inside
  a request.
- `LoggingEmailAccess` writes the recipient and the link to the log. It is
  registered when no token is configured, which covers local development and
  tests.

Message wording is policy, so it lives in an engine:
`Seeka.Engines/Accounts/AccountEmails.Verification(displayName, link)` returns
the subject, text, and HTML. Both text and HTML are sent, because some mail
clients only show text and spam filters penalise HTML-only mail.

### The link points at the web app, which posts the token

The link is `{App:PublicUrl}/verify-email/confirm?userId=…&token=…` (URL-encoded).
The page `POST`s to `/api/auth/verify-email`, so the state change is never a
`GET`. Identity's `ConfirmEmailAsync` checks the token: one-day lifespan, and
bound to the security stamp, so changing the address invalidates older links.

### Correcting the address reuses Identity

`PUT /api/auth/email`:
- Refused unless the email is unverified.
- Validates the address and checks that no other account uses it.
- Sets both `Email` and `UserName`, keeps the address unverified, rotates the
  security stamp, and sends a new link.
- Refreshes the person's own cookie so they stay signed in.

### Web app order

`Session` gains `emailVerified`. `RequireSession` checks verification first,
then onboarding:
- Unverified and anywhere else: go to `/verify-email`.
- Verified and on `/verify-email`: go onward.

`/verify-email` uses the setup layout, which is the current `OnboardingLayout`
renamed `SetupLayout`. `/verify-email/confirm` sits outside the guards, so it
works signed out.

## Risks / Trade-offs

- [Cloudflare Email Sending is beta] → It sits behind `IEmailAccess`, so a second
  provider is one accessor away.
- [The provider is down during registration] → The account is created anyway,
  and the person can resend.
- [Resend abuse] → Requires a session, uses the 10-per-minute auth limit, and only
  sends to the person's own address.
- [Mail scanners prefetch links] → The link only opens a page. Verification
  happens on the page's `POST`.
- [Existing test accounts use `example.com`] → They stay unverified; delete them
  or verify them in the database. Development logs links instead of sending.
- [Bounces hurt sender reputation] → No real sends from development or tests.

## Migration Plan

1. Create a Cloudflare API token scoped to Email Sending on the account.
2. Store it as the `EMAIL_API_TOKEN` GitHub environment secret.
3. Map the secret in `backend-deploy.yml` with `wrangler secret put`, and in
   `containerEnvironment()` as `Email__CloudflareApiToken`.
4. Set `Email__CloudflareAccountId`, `Email__From`, and `App__PublicUrl` as
   non-secret variables.

No schema migration. Rollback: redeploy the previous image. `EmailConfirmed`
values that were set stay harmless.
