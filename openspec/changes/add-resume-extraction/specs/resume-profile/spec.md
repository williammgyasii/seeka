## ADDED Requirements

### Requirement: Confirmed résumé facts

The system SHALL let a person upload a PDF résumé, SHALL extract roles, skills,
seniority, and years of experience with the excerpt each came from, and SHALL use
extracted facts in assessments only after the person confirms them. The
extraction SHALL never decide fit, and SHALL be subject to a per-user usage
limit.

#### Scenario: Facts carry their evidence
- **WHEN** a person uploads a résumé
- **THEN** each extracted fact shows the excerpt it came from, waiting for review

#### Scenario: Unconfirmed facts are not used
- **WHEN** a job is assessed before the person confirms their extracted facts
- **THEN** the assessment does not use the unconfirmed facts

#### Scenario: Usage limit reached
- **WHEN** a person has used their extraction limit
- **THEN** the system refuses further extraction and says why
