## ADDED Requirements

### Requirement: Search intent drives eligibility

Onboarding SHALL capture the person's search intent (remote, onsite,
sponsorship, or a combination) and the facts each mode needs. Eligibility SHALL
apply each mode's own evidence rules with the outcomes Eligible, Needs
confirmation, Ineligible, and Unknown, and working-hour compatibility SHALL use
the person's real timezone.

#### Scenario: Remote intent uses residence and timezone
- **WHEN** a person with remote intent assesses a job that hires only in certain countries
- **THEN** eligibility compares the job's locations with the person's residence

#### Scenario: Sponsorship is usually unknown
- **WHEN** a job description says nothing about visa sponsorship
- **THEN** sponsorship eligibility is Unknown rather than Ineligible

#### Scenario: Any timezone is supported
- **WHEN** a person outside `Africa/Accra` assesses a job with stated working hours
- **THEN** compatibility is computed from the person's own timezone
