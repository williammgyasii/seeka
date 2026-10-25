## 1. Backend: phone policy and registration

- [x] 1.1 Write failing `PhoneNumberPolicyTests`: `+351912345678` and `+233241234567` normalise to themselves, `+351 912 345 678` normalises to E.164, `+15550000000` and `12345` are rejected; verify they fail
- [x] 1.2 Add `libphonenumber-csharp` to `Seeka.Engines`, implement `PhoneNumberPolicy`, verify the tests and `ArchitectureTests` pass
- [x] 1.3 Write failing `MembershipManagerTests`: blank name returns a `displayName` error, invalid phone returns a `phoneNumber` error, neither creates an account; a valid registration passes the E.164 phone to account access; verify they fail
- [x] 1.4 Add `PhoneNumber` to `RegisterRequest`, validate in `RegisterAsync`, store it in `AccountAccess`; verify the tests pass

- [x] 1.5 Write a failing test that the session reports `onboardingComplete` false before and true after a candidate profile is saved; add the flag to `SessionResponse` and `AccountAccess`; verify it passes

## 2. Backend: rate limits

- [x] 2.1 Write a failing test: the 121st request in a minute from one caller is rejected while another caller still gets through; verify it fails (covered as `ApiRateLimitsTests` on the limiter itself, plus a live 429 on login in 5.2)
- [x] 2.2 Add the global limiter partitioned by user id then IP, move `UseAuthentication()` before `UseRateLimiter()`; verify the test and the existing auth tests pass

## 3. Frontend: country helpers and phone field

- [x] 3.1 Move timezone-to-country logic to `shared/lib/countries.ts` (code and name), keep `residence.ts` tests green
- [x] 3.2 Write failing tests for the sign-up form: phone country defaults from the timezone, an invalid number shows an error and sends nothing, a valid submission sends `displayName` and an E.164 `phoneNumber`; verify they fail
- [x] 3.3 Add `libphonenumber-js`, build `PhoneNumberField`, make full name and phone required in `AuthPage`; verify the tests pass

## 4. Frontend: route guard

- [x] 4.1 Write failing tests: signed out on `/pipeline` shows sign-in and returns to `/pipeline` after signing in; signed in on `/login` shows the dashboard; verify they fail
- [x] 4.2 Add `RequireSession` around the `AppLayout` route and the `from` redirect in `AuthPage`; verify the tests pass
- [x] 4.3 Write failing tests: a session with `onboardingComplete: false` on `/dashboard` shows onboarding; saving the profile refetches the session; verify they fail, then add the gate and the `Session` invalidation and verify they pass
- [x] 4.4 Narrow the login brand panel (column ratio and headline size); check desktop and phone widths
- [x] 4.5 Write failing tests: onboarding shows no app navigation but offers sign-out; a finished user on `/onboarding` goes to the dashboard. Move onboarding into `OnboardingLayout`, share `SignOutButton`, add the reverse redirect; verify in the running app

## 5. Delivery

- [x] 5.1 Run backend and frontend tests, lint, and build; record the bundle size change in design.md
- [x] 5.2 In the running app: sign up with a phone number and confirm the E.164 value in Postgres, try an invalid number, open a protected page signed out and signed in, and trigger a 429 on login
- [x] 5.3 Update the README and both architecture rules; `openspec validate add-signup-phone-and-route-guard --strict`
