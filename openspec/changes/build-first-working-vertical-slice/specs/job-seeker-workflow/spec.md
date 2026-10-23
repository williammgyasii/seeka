## Purpose

Enable a professional to securely assess a real vacancy, organise it in a
workspace-owned pipeline, and prepare truthful recruiter outreach with a clear
record of what happened.

## ADDED Requirements

### Requirement: Secure personal workspace access
The system SHALL let a person register and sign in using a secure server-managed
session, SHALL create a personal workspace for a new account, and SHALL
authorise every resource through the authenticated user's active membership.

#### Scenario: New account receives a personal workspace
- **WHEN** a person registers with a valid email and password
- **THEN** the system creates the account, personal workspace, owner membership, and authenticated session atomically

#### Scenario: Cross-workspace access is denied
- **WHEN** an authenticated user requests a resource belonging only to another workspace
- **THEN** the system returns a not-found or forbidden response without exposing the resource

### Requirement: Candidate profile and preferences
The system SHALL store an editable candidate profile and job preferences,
including residence, IANA timezone, target roles, seniority, skills,
experience summary, employment types, available working hours, and optional
minimum compensation with currency.

#### Scenario: Ghanaian candidate saves preferences
- **WHEN** a user saves Ghana as residence and Africa/Accra as timezone with valid job preferences
- **THEN** the system persists the values in the user's active workspace and returns them for review

### Requirement: Manual job import with provenance
The system SHALL accept a manually supplied HTTP or HTTPS source URL and job
description, retain the original content and source provenance, and avoid
fabricating undisclosed fields such as salary or posted date.

#### Scenario: User imports a real vacancy
- **WHEN** a user submits a valid source URL, employer, title, and job description
- **THEN** the system creates or returns a workspace-visible job record with first-seen and last-checked timestamps

#### Scenario: Unsupported URL scheme is rejected
- **WHEN** a user submits a source URL using a scheme other than HTTP or HTTPS
- **THEN** the system rejects the import without fetching or storing the URL

### Requirement: Transparent geographic eligibility
The system SHALL assess eligibility independently from professional fit using
Eligible, Needs confirmation, Ineligible, or Unknown; SHALL retain restriction
text, evidence excerpts, reason, source URL, timestamp, and decision origin;
and SHALL surface conflicting or incomplete evidence for review.

#### Scenario: Ghana is explicitly included
- **WHEN** a posting explicitly permits candidates resident in Ghana and contains no conflicting restriction
- **THEN** the assessment is Eligible and cites the supporting excerpt

#### Scenario: Remote has no hiring geography
- **WHEN** a posting says remote but provides no supported hiring geography
- **THEN** the assessment is Unknown or Needs confirmation and does not infer worldwide eligibility

#### Scenario: Restrictions conflict
- **WHEN** one part of a posting includes Ghana but another requires residence in an incompatible country
- **THEN** the assessment is Needs confirmation and includes both excerpts

#### Scenario: Ghana is explicitly excluded
- **WHEN** a posting explicitly excludes Ghana or requires residence in an incompatible country
- **THEN** the assessment is Ineligible and cites the restriction

### Requirement: Versioned professional-fit assessment
The system SHALL calculate a versioned assessment separately from eligibility
using target-role alignment, relevant experience, required skills, seniority,
working-hour compatibility, and disclosed compensation compatibility; SHALL
show component scores and missing requirements; and SHALL keep undisclosed
compensation unknown rather than treating it as compatible.

#### Scenario: Match is explained
- **WHEN** a user assesses an imported job against a completed profile
- **THEN** the system stores and displays the model version, total score, component scores, explanation, and missing requirements

#### Scenario: Salary is not disclosed
- **WHEN** a job has no disclosed compensation
- **THEN** compensation compatibility is Unknown and does not add points for meeting the user's minimum

### Requirement: Independent pipeline and application tracking
The system SHALL let a user save a job in stages Saved, Researching, Ready to
Contact, Contacted, Replied, Interviewing, Offer, or Closed while tracking
formal application status independently.

#### Scenario: Contact without application
- **WHEN** a user moves a saved job to Contacted without recording an application
- **THEN** the pipeline stage changes and application status remains Not applied

### Requirement: Honest contact and outreach workflow
The system SHALL support manual contacts, label manually supplied verification
as Unknown unless evidence says otherwise, generate editable email, LinkedIn
connection, LinkedIn message, and follow-up drafts grounded only in saved
profile and job facts, and require user review before any external send.

#### Scenario: Manual contact is added
- **WHEN** a user supplies a contact name, role, company, and optional professional URL or email
- **THEN** the contact is stored for the workspace without inventing missing details

#### Scenario: Grounded draft is generated
- **WHEN** a user requests outreach for a saved job and selected contact
- **THEN** the drafts mention the actual role and supported candidate experience without invented metrics, credentials, relationships, or contact facts

### Requirement: Activity is auditable
The system SHALL append workspace-scoped activity for job imports, assessment,
pipeline changes, contact additions, draft generation or editing, and manually
recorded outreach, including actor and timestamp.

#### Scenario: Outreach is recorded
- **WHEN** a user records that an approved draft was copied or sent manually
- **THEN** the system appends an activity entry without claiming provider-confirmed delivery or reply
