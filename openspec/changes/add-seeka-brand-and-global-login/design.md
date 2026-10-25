## Context

The design system from `adopt-shadcn-design-system` routes every color through
tokens in `src/index.css`, and lint rejects palette classes, so a brand palette
is a token edit. The login page (`AuthPage.tsx`) hides its brand panel below the
`lg` breakpoint, so phones and small laptops see only a plain card. Onboarding
(`OnboardingPage.tsx`) hard-codes Ghana, `Africa/Accra`, and GMT hours.
`EligibilityEngine` matches residence by case-insensitive substring of the
country name, so the saved name must be the one postings use.

## Goals / Non-Goals

**Goals:**
- One palette, defined once as tokens, light and dark.
- A login page that feels branded at every screen size without a motion library.
- Residence and timezone suggested from the device, with a fixed country list.

**Non-Goals:**
- A theme toggle (dark tokens exist, nothing switches to them yet).
- Country aliases in the eligibility engine ("UK", "USA"). Worth a backend change later.
- Route-level code splitting to recover bundle size.

## Decisions

### Palette: ink & citrus, as tokens

| Token | Light | Dark | Used for |
|---|---|---|---|
| `--primary` | indigo ink `oklch(0.29 0.09 270)` | lighter indigo `oklch(0.74 0.13 270)` | Buttons, brand panel, links |
| `--highlight` (new) | citrus lime `oklch(0.9 0.19 125)` | same lime | Brand moments: login glow, logo mark, focus accents |
| `--background`, `--muted`, `--border` | neutrals with a faint indigo tint (chroma ≈ 0.005–0.015) | deep ink `oklch(0.17 0.03 270)` family | Surfaces |
| `--accent` | pale indigo tint | dark indigo tint | Hover and selected rows (shadcn's meaning) |
| `--ring` | primary | primary | Focus rings |

Lime is a new `highlight` token instead of shadcn's `accent`, because shadcn
uses `accent` for every hover and menu focus; a lime hover on every ghost
button would be loud. `success`, `warning`, `info`, and `destructive` are
retuned only so they sit well next to indigo. Exact values are adjusted by eye
during review; the table is the starting point.

### Login: CSS-only motion

- The brand panel becomes a top band on small screens and the left column from `lg` up.
- Background: two or three blurred radial glows (`primary`, `highlight`) that
  drift slowly with a CSS keyframe, plus a grain layer from an inline SVG noise
  filter. Defined as `@utility` classes in `index.css` so they use tokens.
- Entry: the card and headline fade and slide in with `tw-animate-css`
  (`animate-in fade-in slide-in-from-bottom-2`), already installed.
- `motion-reduce:` disables the drift and entry animation.
- Copy is global: no country names.

Alternative considered: Framer Motion. Rejected for about 40 kB on a bundle
already over 600 kB, for effects CSS handles.

### Residence: `countries-and-timezones` for data, `Intl` for names

- The package supplies the 248 country codes and the timezone-to-country map.
  `getCountryForTimezone` returns the timezone's main country (`Africa/Lagos`
  gives Nigeria, `Europe/Zurich` gives Switzerland) and nothing for `UTC`.
- Names come from `Intl.DisplayNames(['en'], { type: 'region' })`, because the
  package's own names differ from what postings say ("United States of America"
  vs "United States").
- One pure module, `features/onboarding/residence.ts`, exposes
  `countryOptions()` (sorted English names) and `suggestResidence(timeZone)`.
  It is the only file importing the package. Tests cover it directly.
- The device timezone comes from `Intl.DateTimeFormat().resolvedOptions().timeZone`,
  read once when the form mounts, so tests can pass a timezone in.
- Residence uses the design-system `Select`. Working hours default to empty
  with a placeholder instead of "GMT".

Alternatives considered: a hand-written map (small, but drifts and misses zones)
and the browser language region (free, but often `en-US` regardless of location).

## Risks / Trade-offs

- [About 20 `Intl` names are written in CLDR's short style and won't
  substring-match common posting text: "Bosnia & Herzegovina", "St. Lucia",
  "Congo - Kinshasa", "Myanmar (Burma)", "Türkiye", "Côte d’Ivoire" with a curly
  apostrophe] → Accepted for this change; the large markets (United States,
  United Kingdom, Nigeria, India, Portugal...) match. The engine-alias change
  should normalise "&"/"and", "St."/"Saint", and known alternate names.
- [Bundle grew from 618 kB to 658 kB] → Matches the estimate; lazy routes later.
- [A 248-item `Select` has no type-to-search box] → Radix supports typeahead by
  first letters; a searchable combobox is a later polish.
- [Lime on white fails contrast for text] → Lime is never a text color on light
  surfaces; text on lime uses `--highlight-foreground` (ink).
- [Recoloring touches every screen at once] → Review screenshots of every page
  after the token slice, before the login slice starts.
