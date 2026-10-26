## 1. Engine

- [x] 1.1 Tests: `ResumeDraftPolicy` cleans the new sections (drops unnamed items, blanks bad months, keeps https links, caps counts)
- [x] 1.2 Tests: `CandidateProfilePolicy` rejects bad input by field path
- [x] 1.3 Tests: `ExperienceYears.Total` merges overlaps, runs current roles to today, ignores undated jobs
- [x] 1.4 Tests: `OnboardingSteps` reports missing required steps
- [x] 1.5 Implement `Seeka.Engines.Profiles` and extend `ResumeDraftPolicy` until the tests pass

## 2. Extraction

- [x] 2.1 Test: `OpenAiResumeExtractionAccess` parses every section from a strict-schema reply
- [x] 2.2 Extend the schema, instructions, and parser

## 3. Profile storage, manager, and API

- [x] 3.1 Tests: API saves each step, marks it finished, is idempotent, keeps workspaces apart, and returns field errors
- [x] 3.2 Tests: finishing needs about and preferences; saving a step does not finish onboarding; the session flag reads the finish time
- [x] 3.3 Add the entity columns and the migration with the back-fill
- [x] 3.4 Implement `CandidateProfileManager`, the access methods, and the endpoints; remove `PUT /api/candidate` and the candidate methods on `MembershipManager`

## 4. Onboarding steps

- [x] 4.1 Tests: the stepper pre-fills from the draft, Skip finishes a step, a reload resumes at the first unfinished step, field errors show under the field, Finish opens the app
- [x] 4.2 Implement the stepper and one component per step
- [x] 4.3 Check the flow in the running app with a made-up résumé
- [x] 4.4 Fit onboarding to the viewport: progress, title, and buttons pinned; only fields scroll
- [x] 4.5 Tests then build: jobs, schools, projects, and volunteering collapse into accordions that open on add and on a server error
- [x] 4.7 Import the ESCO English occupation list into a generated JSON file, with attribution
- [x] 4.8 Backend tests then build: job title catalog and signed-in search endpoint, title/employment/hours/currency/range checks, `MaximumCompensation` column, real-timezone check, country currency lookup
- [x] 4.9 Frontend tests then build: timezone follows country, title picker, employment and hours dropdowns, min and max with the default currency
- [x] 4.10 Tests then build: remove working hours (form, API, column); monthly pay bands per currency with USD fallback, `GET /api/candidate/pay-bands`
- [x] 4.11 Retire the `AvailableWorkingHours` column: unmapped, given a default of '' so the live release keeps working
- [ ] 4.12 Next release, once no deployed version reads it: a migration that drops `AvailableWorkingHours`
- [x] 4.6 Tests then build: seniority is a fixed level in the résumé reader, the About you dropdown, and the server; earlier free text reads as a level; the summary uses no pronouns

## 5. Marketing

- [x] 5.1 Site test: the "Start from your résumé" step says Seeka fills in every job, school, and certification
- [x] 5.2 Update the landing page copy
- [x] 5.3 Site test and copy: the fit score names roles from a standard list, seniority, timezone, and pay range in your own currency

## 6. Release

- [ ] 6.1 Ship in a release and upload the test résumé on `app.seekajobs.tech`; confirm every section comes through
- [ ] 6.2 Sync the spec into `openspec/specs/`
