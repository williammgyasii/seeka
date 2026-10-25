## Why

Seeka serves remote job seekers everywhere, but the first screens say otherwise:
the login page is a plain neutral card that names one country, and onboarding
pre-fills Ghana, `Africa/Accra`, and GMT hours for every new user. A user who
misses that default gets every eligibility check run against the wrong country.
The neutral shadcn theme was always a placeholder for a Seeka brand.

## What Changes

- Replace the neutral theme tokens with the Seeka "ink & citrus" palette: a deep
  indigo-ink primary, a citrus-lime highlight, cool-tinted neutrals, and matching
  dark-mode tokens (no theme toggle yet). Every page recolors through tokens.
- Redesign the login and register page on the new tokens: a brand panel visible
  at every screen size, an animated gradient with grain, stronger type, and motion
  that stops when the user prefers reduced motion. Remove the country-specific copy.
- Onboarding no longer pre-fills a hard-coded country, timezone, or GMT hours. It
  suggests the timezone and residence from the browser, and residence is chosen
  from a list of English country names instead of typed free text.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `job-seeker-workflow`: the candidate profile requirement gains suggested,
  editable residence and timezone defaults taken from the user's device, and
  residence values come from a fixed list of country names.

## Impact

- `seeka-frontend/src/index.css` (tokens), `features/auth/AuthPage.tsx`,
  `features/onboarding/OnboardingPage.tsx`, and a small residence helper with tests.
- Possibly one new frontend dependency for country and timezone data (see design).
- No backend or API change. `EligibilityEngine` already matches residence by
  case-insensitive country name, so a fixed list of English names makes its input
  more reliable, not different.
