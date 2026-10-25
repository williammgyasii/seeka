## 1. Brand tokens

- [x] 1.1 Replace the neutral values in `src/index.css` with the ink & citrus palette for `:root` and `.dark`, add `--highlight` and `--highlight-foreground` to both and to `@theme inline`; verify lint, tests, and build pass
- [x] 1.2 Screenshot every screen in the running app and review the recolor before starting the login slice

## 2. Login redesign

- [x] 2.1 Add the glow-drift and grain `@utility` classes in `index.css`, built from tokens, with reduced-motion fallbacks; verify lint passes
- [x] 2.2 Rebuild `AuthPage` layout: brand band on small screens, left column from `lg`, entry animation, global copy with no country names; verify the existing auth tests pass and check login and register at phone and desktop widths with reduced motion on and off

## 3. Global onboarding defaults

- [x] 3.1 Write failing tests for `residence.ts`: `Europe/Lisbon` suggests Portugal, `Africa/Lagos` suggests Nigeria, `UTC` suggests nothing, the options list is sorted and uses "United States" and "United Kingdom"; verify they fail
- [x] 3.2 Add `countries-and-timezones`, implement `residence.ts`, and verify the tests pass
- [x] 3.3 Write a failing `OnboardingPage` test: with timezone `Europe/Lisbon` the form shows that timezone and residence Portugal; with `UTC` residence is empty and saving is blocked; verify it fails
- [x] 3.4 Use the device timezone, the residence `Select`, and empty working hours in `OnboardingPage`; verify the tests pass and onboarding saves in the running app

## 4. Delivery

- [x] 4.1 Update `.cursor/rules/frontend-architecture.mdc` (the `highlight` token and when to use it) and the README; run tests, lint, build, and `openspec validate add-seeka-brand-and-global-login --strict`
