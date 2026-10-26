## Why

Seeka writes outreach and checks fit from the candidate's profile, but the
profile holds only a name, seniority, years, a summary, and skills. A draft
cannot quote a job the profile does not know about. The résumé the user
uploads already lists every job, school, certification, and language; Seeka
reads it and then throws most of it away.

## What Changes

- Résumé extraction reads every section: work experience with highlights,
  education, certifications, languages, links, projects, and volunteering.
- Onboarding becomes grouped steps with a progress bar: résumé, about you,
  experience, education and certifications, projects and volunteering, and
  the existing job preferences. Each step is pre-filled from the résumé and
  saved on its own, so leaving and coming back resumes where the user was.
- The server validates each step and names the exact field that is wrong.
- Years of experience is suggested from the job dates, with overlaps counted
  once, instead of being guessed by the model.
- Onboarding finishes with an explicit Finish, not when the first row is
  saved. Existing profiles stay finished.
- Candidate profile saving moves out of `MembershipManager` into its own
  manager. `PUT /api/candidate` is replaced by one endpoint per step.

## Capabilities

### New Capabilities

- `candidate-profile`: the full profile built from the résumé, saved step by
  step, and the rule for when onboarding is finished.

### Modified Capabilities

None. The job preference fields keep today's rules; the preferences change
(part 2) will replace them.

## How we sell it

- **Promise:** Upload your résumé and Seeka builds your whole profile, every
  job, school, and certification, so every message it drafts can point to
  something you actually did.
- **Who it is for:** anyone who does not want to retype their career into
  another form.
- **What it does NOT do:** invent or improve anything. Facts the résumé does
  not state stay blank, and the user checks every step before it is saved.
- **Second promise:** the fit score uses roles picked from a standard job
  title list (ESCO), plus seniority, timezone, and a pay range in the user's
  own currency. It does not promise that a job pays within that range.
- **Marketing change:** the "Start from your résumé" and "Find out if a job is
  worth it" steps on the landing page at `https://seekajobs.tech/`.

## Impact

- Backend: `Seeka.Engines/Profiles` (new), `ResumeDraftPolicy`,
  `OpenAiResumeExtractionAccess`, a new `CandidateProfileManager`,
  `CandidateController`, `CandidateAccess`, `AccountAccess` (onboarding flag),
  `CandidateProfile` entity and a migration.
- Frontend: `features/onboarding` becomes a stepper with one component per
  step; shared API types gain the profile.
- Marketing: landing page copy and a site test.
- Résumés uploaded before this change keep their short draft; the new
  sections start empty for them.
