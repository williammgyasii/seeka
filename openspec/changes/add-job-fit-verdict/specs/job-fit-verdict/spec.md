## ADDED Requirements

### Requirement: Versioned job-fit verdict

The system SHALL extract structured job fields from the description with an
excerpt for each field, and SHALL give a versioned verdict of Apply, Stretch, or
Don't apply that combines eligibility, fit, and résumé evidence, with strengths
and gaps tied to evidence.

#### Scenario: Extracted fields show their evidence
- **WHEN** a job description is assessed
- **THEN** every extracted field shows the excerpt it came from

#### Scenario: Ineligible jobs are never Apply
- **WHEN** a job's eligibility is Ineligible
- **THEN** the verdict is not Apply

#### Scenario: Strengths and gaps
- **WHEN** a verdict is shown
- **THEN** it lists requirements the résumé supports and requirements it does not mention, each with evidence
