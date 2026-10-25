## ADDED Requirements

### Requirement: On-demand insider discovery

The system SHALL find people inside a job's company only when the person asks,
SHALL decide whom to look for with a deterministic targeting policy, SHALL reuse
cached provider results keyed by company domain, target role, and provider, and
SHALL enforce a per-user lookup limit. Saved contacts SHALL stay
workspace-scoped.

#### Scenario: Find insiders on request
- **WHEN** a person selects "Find insiders" on a saved job
- **THEN** the system looks up the targeted roles at the job's company domain and shows the results

#### Scenario: Cached results are reused
- **WHEN** another lookup asks for the same company domain, role, and provider within the cache period
- **THEN** the system returns the cached results without calling the provider

#### Scenario: Verification stays honest
- **WHEN** the provider gives no evidence about an email address
- **THEN** the contact's email verification is `Unknown`

#### Scenario: Lookup limit reached
- **WHEN** a person has used their lookup limit
- **THEN** the system refuses further lookups and says when the limit resets
