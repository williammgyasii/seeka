## Why

Anyone can register with an email address they don't own. Seeka will send
outreach and notifications on a person's behalf, so the address must be proven
before the workspace is usable. `seekajobs.tech` is now enabled for Cloudflare
Email Sending on the paid Workers plan, so Seeka can send its own mail.

## What Changes

- Registration sends a verification email from `Seeka <verify@seekajobs.tech>`
  with a one-day link, through the Cloudflare Email Sending REST API.
- Hard gate: until the address is verified, the API refuses every workspace
  endpoint (403), and the web app shows only a "check your inbox" page. The
  person stays signed in so they can act on that page.
- The "check your inbox" page can resend the email and correct a mistyped
  address, so a typo never locks anyone out.
- Opening the link verifies the address on any device, signed in or not.
- The session reports `emailVerified`; the web app's order becomes sign up, then
  verify email, then onboarding, then dashboard.
- Development without an email token logs the link instead of sending, so test
  addresses never bounce against the real domain.
- Accounts created before this change start unverified and see the gate on their
  next visit.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `job-seeker-workflow`: adds a "Verified email before workspace use"
  requirement; registration now sends a verification email.

## Impact

- Backend:
  - `MembershipManager`: send, resend, change-email, and verify use cases.
  - `IAccountAccess`: confirmation token generation and checks.
  - New `IEmailAccess` with a Cloudflare implementation and a logging
    implementation.
  - New engine `AccountEmails` that composes the email text.
  - Email-verified authorization policy as the default for controllers.
  - Four auth endpoints.
- Frontend:
  - `Session.emailVerified`.
  - `RequireSession` gate.
  - `/verify-email` page (inside the setup layout) and `/verify-email/confirm`
    landing page (public).
- Configuration: `Email:CloudflareAccountId`, `Email:From`, `App:PublicUrl`, and
  secret `Email:CloudflareApiToken` (a Cloudflare token scoped to Email Sending).
  In production the token is a GitHub environment secret passed to a Wrangler
  secret, then a container environment variable, like the database string.
- No database migration: Identity's `EmailConfirmed` column already exists.
