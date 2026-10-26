## Context

`add-resume-extraction` stores the résumé, reads its text, and asks OpenAI for
six flat facts. `ResumeDraftPolicy` cleans them into a `ResumeDraft` kept as
JSON on the résumé row. Onboarding is one form that `PUT /api/candidate`
saves in one go, through `MembershipManager`. A session counts onboarding as
complete as soon as a `CandidateProfile` row exists.

## Goals / Non-Goals

**Goals:**
- Keep every résumé section the user will later use in drafts and fit checks.
- Let the user review the profile in a few steps without losing work.
- Keep what the model read separate from what the user saved.

**Non-Goals:**
- Editing the profile after onboarding (a later Profile page will reuse the
  step components).
- New job preference fields, plans, or connecting an email account.
- Re-extracting résumés uploaded before this change.

## Decisions

### Steps and what each saves

| Step | Saves | Required to finish |
|---|---|---|
| `about` | name, headline, residence, timezone, seniority, years, summary, skills, languages, links | yes |
| `experience` | work experience | no |
| `education` | education and certifications | no |
| `projects` | projects and volunteering | no |
| `preferences` | today's job preference fields | yes, matching needs a target role |

Each step has its own `PUT /api/candidate/profile/{step}` that replaces that
step's data and adds the step to `CompletedSteps`. An empty list is a valid
save, which is how Skip works. Replacing a whole step is idempotent, so a
retried request needs no idempotency key.

`POST /api/candidate/onboarding/complete` sets `OnboardingCompletedAt` when
`about` and `preferences` are saved, and otherwise returns a 400 naming the
missing steps. The session flag reads `OnboardingCompletedAt`, not the row.

*Alternative rejected:* saving everything at the end in one request loses an
hour of editing to a closed tab or a failed request.

### Draft pre-fills, profile owns

The résumé draft only fills a step the user has never saved. Once a step is
in `CompletedSteps`, the profile is the source of truth for it, and a new
upload does not overwrite it.

### One JSON document for the lists

`CandidateProfile` gains `Headline`, `DetailsJson` (`jsonb`),
`CompletedSteps` (`text[]`), and `OnboardingCompletedAt`. The seven lists
live in `DetailsJson`, following how `Resume.DraftJson` is stored. The
profile is always read and written whole by its owner, so the lists do not
need their own tables. Years, seniority, and skills stay columns because
matching reads them.

*Alternative rejected:* a table per section adds seven tables and joins for
data nobody queries across users.

### Item types live in the engine

`Seeka.Engines.Profiles` holds the item records (`WorkExperience`,
`Education`, `Certification`, `LanguageSkill`, `ProfileLink`, `Project`,
`Volunteering`), the shared limits, and two policies:

- `ResumeDraftPolicy` quietly cleans model output: drops items missing a
  field the step requires (a job's company or title, a school), trims text, keeps only `YYYY-MM` months and `https`
  links, caps counts. A bad date blanks that date, not the item.
- `CandidateProfilePolicy` rejects bad user input with field paths such as
  `experience[2].endMonth`, using the same limits.

Both use the same limits so a draft always passes validation.

### Years of experience from dates

`ExperienceYears.Total` merges the date ranges of jobs with a start month,
counts overlaps once, runs current jobs to today, and rounds down to whole
years. The profile view returns it as `suggestedYearsOfExperience`; the user
can still type a different number.

### Language levels are a fixed list

`Native`, `Fluent`, `Professional`, `Conversational`, `Basic`. The model must
pick one or null, and the UI shows a dropdown.

### A manager for the profile

`CandidateProfileManager` owns the use case: find the workspace, validate
with the engine, save through `ICandidateAccess`, and finish onboarding.
`MembershipManager` keeps accounts only. `ICandidateAccess.FindSetupAsync`
stays as it is for matching and outreach.

## Risks / Trade-offs

- [Bigger model output is slower and costs more] → output is capped by the
  schema limits and `max_completion_tokens`; the upload rate limit stays.
- [Model returns months in another format] → the draft blanks the date and
  the step asks the user to check it.
- [Last save wins across two tabs] → accepted; only the owner edits.
- [Existing profiles have no `CompletedSteps`] → the migration marks `about`
  and `preferences` complete and sets `OnboardingCompletedAt` from
  `UpdatedAt`.

## Migration Plan

One EF Core migration adds the four columns and back-fills existing rows. The
release applies it the same way as earlier migrations. Rolling back means
redeploying the previous release; the added columns are ignored by it.
