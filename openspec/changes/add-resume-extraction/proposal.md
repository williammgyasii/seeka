## Why

Seeka should judge fit against the candidate's actual résumé, not only skills
typed into a form, so it can point out strengths and gaps. Turning a résumé
into structured facts needs an AI extraction step, which is not connected
today.

## What Changes

- Let a user upload their current résumé (PDF) and keep it as the source of
  profile evidence.
- Add an OpenAI-backed extraction access boundary that turns the résumé into
  structured facts (roles, skills, seniority, years of experience), each with
  the excerpt it came from.
- The AI only extracts facts. It never decides fit or gives a recommendation.
- Require the user to review and confirm extracted facts before they feed
  assessments.
- Add a per-user AI usage limit as a spend guard.
- Manage the OpenAI key as a secret (user-secrets locally, environment secret
  in production), never exposed to the browser.

## Depends on

An OpenAI project-scoped key with a monthly spend limit.

## Open questions for the deep dive

- Where résumé files are stored (database or object storage such as R2) and
  for how long.
- Whether to keep several résumé versions.
- How to handle extraction failures and résumés that aren't in English.
- Personal-data handling and deletion.

## Capabilities

### New Capabilities

- `resume-profile`: Résumé upload, AI fact extraction with evidence, user
  confirmation, and usage limits.

### Modified Capabilities

- `job-seeker-workflow`: Candidate profile facts can originate from a confirmed
  résumé extraction.

## Impact

- New Infrastructure access for OpenAI and for file storage.
- New secret in local and production configuration.
- Per-call AI cost.
