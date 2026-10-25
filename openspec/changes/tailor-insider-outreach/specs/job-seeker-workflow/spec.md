## ADDED Requirements

### Requirement: Outreach tailored to the insider

The system SHALL tailor email and LinkedIn drafts to the insider type
(recruiter, hiring manager, or potential referrer) and SHALL cite only confirmed
résumé strengths that match the job. Drafts SHALL keep the existing honesty
rules: nothing invented, the person reviews every draft, and LinkedIn drafts are
copy-only.

#### Scenario: Draft fits the insider type
- **WHEN** a person drafts outreach to a hiring manager
- **THEN** the draft is written for a hiring manager, not a recruiter or referrer

#### Scenario: Only confirmed strengths are cited
- **WHEN** a draft mentions the candidate's strengths
- **THEN** each one comes from a confirmed résumé fact that matches the job
