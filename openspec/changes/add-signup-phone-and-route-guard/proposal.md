## Why

Sign-up collects only email, password, and an optional display name, so Seeka has
no way to reach a user other than email. The web app also has no route guard: a
signed-out visitor who opens `/dashboard` sees the app shell with failing requests
instead of the sign-in page. Rate limiting covers only the two auth endpoints.

The server session cookie, account lockout, `[Authorize]` on every API controller,
and the `Sec-Fetch-Site` check already exist and stay as they are. JWT is not
adopted: the browser and API share one origin, and a server session is revocable
at sign-out without extra machinery.

## What Changes

- Sign-up requires full name, email, password, and phone number. The phone number
  is chosen as a country calling code plus a national number, validated for that
  country on the server, and stored in E.164 format (`+351912345678`). It is not
  verified and not unique.
- Invalid sign-up input returns field-level validation errors (`displayName`,
  `phoneNumber`) that the form shows under the matching field.
- The web app sends signed-out visitors on protected pages to sign-in and returns
  them to the page they asked for afterward; a signed-in visitor on the sign-in
  page goes to the dashboard.
- Every API endpoint gets a rate limit per signed-in user, or per IP when signed
  out, on top of the stricter per-IP limit on sign-in and sign-up.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `job-seeker-workflow`: the secure workspace access requirement gains the sign-up
  fields, phone validation, the protected-page redirect, and API-wide rate limits.

## Impact

- Backend: a phone engine (`libphonenumber-csharp`), `RegisterRequest` gains
  `PhoneNumber`, `MembershipManager.RegisterAsync` validates before creating the
  account, `AccountAccess` stores `AppUser.PhoneNumber` (existing Identity column, no
  migration), `Program.cs` adds the global limiter and moves authentication before
  rate limiting.
- Frontend: `libphonenumber-js`, a phone field in `features/auth`, a session guard
  in `app/`, timezone-to-country logic moved from `features/onboarding` to
  `shared/lib` so auth can reuse it.
- API: `POST /api/auth/register` requires `phoneNumber`; existing accounts keep a
  null phone number.
