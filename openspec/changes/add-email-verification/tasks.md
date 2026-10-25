## 1. Backend: email boundary

- [ ] 1.1 Write failing `AccountEmailsTests`: the verification email names the person, contains the link in both text and HTML, and HTML-encodes the name; verify they fail, implement the engine, verify they pass
- [ ] 1.2 Add `IEmailAccess` and `OutgoingEmail`; write failing `CloudflareEmailAccessTests` against a stub `HttpMessageHandler` (bearer header, account URL, `from.address`, text and HTML, failure on 429 and 5xx); implement and verify they pass
- [ ] 1.3 Add `LoggingEmailAccess` and register it when no token is configured; verify `ArchitectureTests` still pass

## 2. Backend: verification use cases

- [ ] 2.1 Write failing `MembershipManagerTests`: registration sends one verification email to the new address; a provider failure still returns the session; verify they fail, implement, verify they pass
- [ ] 2.2 Write failing tests for resend (sends to the current address), correct-address (rejects verified accounts, rejects an address in use with an `email` error, sends to the new address), and verify (valid token confirms; a wrong or expired token does not); implement in the manager and `AccountAccess`, verify they pass
- [ ] 2.3 Add `emailVerified` to `SessionResponse`; extend `AccountSessionTests` to cover it; verify the test passes

## 3. Backend: the gate

- [ ] 3.1 Write a failing API test with `WebApplicationFactory`: an unverified session gets 403 from `GET /api/pipeline` and 200 from `GET /api/auth/session`; a verified session gets 200 from both; verify it fails
- [ ] 3.2 Add the `EmailVerified` fallback policy and the `SignedIn` policy on the auth endpoints, plus the resend, correct-address, and verify endpoints; verify the API test and all backend tests pass

## 4. Frontend

- [ ] 4.1 Write failing tests: an unverified session on `/dashboard` shows the check-your-inbox page with the address; resend calls the API; correcting the address shows the new address; a verified user on `/verify-email` moves on; verify they fail
- [ ] 4.2 Add `emailVerified` to `Session`, the verification gate in `RequireSession`, rename `OnboardingLayout` to `SetupLayout`, build the `/verify-email` page; verify the tests pass
- [ ] 4.3 Write failing tests for `/verify-email/confirm`: success while signed in continues to onboarding, success while signed out offers sign-in, failure explains and offers a new link; implement; verify they pass

## 5. Delivery

- [ ] 5.1 Run backend and frontend tests, lint, and build
- [ ] 5.2 In the running app with the logging email access: register, see the gate, confirm the API returns 403 for the pipeline, open the logged link in another browser while signed out, then continue to onboarding in the first browser
- [ ] 5.3 With a real token in user-secrets: send one verification email to an address William controls and confirm it arrives and is not marked as spam
- [ ] 5.4 Wire `EMAIL_API_TOKEN` through `backend-deploy.yml` and `containerEnvironment()` with a test in `worker-routing.test.ts`; update the README and both architecture rules; `openspec validate add-email-verification --strict`
