## Why

Seeka currently assumes one kind of search: a Ghana resident looking for remote
work. Users are also looking for onsite roles and for visa sponsorship, and
each search type defines "a job I can take" differently. Eligibility and
timezone compatibility are hardcoded to that one case. Timezone compatibility
only recognises `Africa/Accra`.

## What Changes

- Add an onboarding flow that captures search intent: remote, onsite,
  sponsorship, or a combination of them.
- Capture the facts each mode needs: residence and timezone for remote; current
  location, target locations, and willingness to relocate for onsite;
  citizenship and work authorisation for sponsorship.
- Generalise eligibility so each mode has its own evidence rules and keeps the
  existing Eligible, Needs confirmation, Ineligible, and Unknown outcomes.
  Sponsorship is expected to be Unknown most often, because postings rarely
  state it.
- Replace the hardcoded `Africa/Accra` working-hour check with real timezone
  compatibility.
- Remove the Ghana-only assumption from the product, while keeping Ghana as a
  supported case.

## Depends on

Nothing.

## Open questions for the deep dive

- Can a user hold several intents at once, and how does a job get assessed
  against more than one?
- Which sponsorship evidence counts (explicit statements, employer sponsorship
  history)?
- How much of onboarding is required before a user sees any value?

## Capabilities

### New Capabilities

- `search-intent`: Onboarding and per-mode eligibility policy.

### Modified Capabilities

- `job-seeker-workflow`: Candidate profile gains search intent. Geographic
  eligibility becomes mode-aware rather than residence-only.

## Impact

- `Domain/Assessments.cs` eligibility and working-hour logic.
- Candidate profile entity, migration, API, and onboarding UI.
