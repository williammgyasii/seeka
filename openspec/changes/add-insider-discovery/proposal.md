## Why

Seeka's core promise is that a candidate sends their application to a real
person inside the company instead of applying cold. Today contacts are typed in
by hand, so Seeka cannot find anyone. This is also the riskiest MVP assumption:
whether an enrichment provider returns useful people for the companies our
users target. It should be proven before the rest of the MVP is built.

## What Changes

- Add a user-triggered "Find insiders" action on a saved job. Lookups happen
  only on demand, never automatically.
- Add a deterministic insider-targeting policy that decides who to look for
  from the job (for example the hiring manager for the function and a
  technical recruiter).
- Add a contact-enrichment access boundary for one external provider, kept
  swappable behind an interface.
- Resolve and store a company domain for each job, because providers and the
  cache key on domain rather than employer name.
- Add a shared, time-limited cache of provider results keyed by company domain,
  target role, and provider. Cached results are reference data. Contacts a user
  saves, with their notes, drafts, and activity, stay workspace-scoped.
- Carry provider-reported email verification through honestly. Keep `Unknown`
  when the provider gives no evidence.
- Add a per-user lookup limit as a spend guard. This is not billing.

## Depends on

Nothing. Works on manually pasted jobs today.

## Open questions for the deep dive

- Which provider, and do its terms allow reusing results across our customers?
  The shared cache depends on the answer.
- Cache expiry (30 to 90 days) and how freshness is shown to the user.
- How the company domain is resolved: provider company search, or user
  confirmation.
- Lookup limit size, and what the user sees when the limit is reached.
- Data-protection basis for storing third-party personal data (GDPR, Ghana Data
  Protection Act).

## Capabilities

### New Capabilities

- `insider-discovery`: Targeting, on-demand provider lookup, shared result
  cache, company domain resolution, and lookup limits.

### Modified Capabilities

- `job-seeker-workflow`: Contacts can originate from a provider lookup with
  provider-reported verification, not only manual entry.

## Impact

- New Domain engine (insider targeting), Application use case (find insiders),
  and Infrastructure access (enrichment provider, cache tables).
- New provider API credential managed as a secret.
- Per-lookup provider cost.
