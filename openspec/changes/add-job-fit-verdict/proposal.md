## Why

Users need a clear answer to "should I apply?", with the reasons and what is
missing from their résumé. The match engine needs structured job fields
(required skills, seniority, hours, compensation) that users will not type in
for every job, and that sourced jobs will not have at all.

## What Changes

- Use AI extraction to turn job description text into the structured job
  snapshot, with an excerpt for every extracted field.
- Add a deterministic verdict policy (Apply, Stretch, or Don't apply) that
  combines eligibility, fit, and résumé evidence.
- The verdict never says Apply when eligibility is Ineligible.
- Show strengths (requirements the résumé supports) and gaps (requirements it
  does not mention), each tied to evidence.
- Version the verdict policy alongside the existing match model version.

## Depends on

- `add-search-intent-onboarding` (mode-aware eligibility)
- `add-resume-extraction` (résumé facts and the OpenAI access boundary)

## Open questions for the deep dive

- Verdict thresholds, and how Unknown components (salary, sponsorship) affect
  the verdict.
- Whether users can correct extracted job fields, and how that is recorded.
- How the verdict is worded so it informs rather than discourages.

## Capabilities

### New Capabilities

- `job-fit-verdict`: Job field extraction, verdict policy, and strengths and
  gaps.

### Modified Capabilities

- `job-seeker-workflow`: The versioned professional-fit assessment is joined by
  a verdict. Job snapshots can come from AI extraction with evidence.

## Impact

- `Domain/Assessments.cs` gains a verdict engine. The match engine keeps
  deciding scores.
- Additional AI cost per assessed job.
