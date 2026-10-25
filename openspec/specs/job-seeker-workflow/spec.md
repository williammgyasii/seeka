## Purpose

Enable a professional to securely assess a real vacancy, organise it in a
workspace-owned pipeline, and prepare truthful recruiter outreach with a clear
record of what happened.

## Requirements

### Requirement: Secure personal workspace access

The system SHALL let a person register and sign in using a secure server-managed
session, SHALL create a personal workspace for a new account, and SHALL
authorise every resource through the authenticated user's active membership.
Registration SHALL require a full name, email, password, and a phone number that
is valid for its country, and SHALL store the phone number in E.164 format. The
web app SHALL send signed-out visitors on protected pages to sign-in. The API
SHALL rate-limit every endpoint per caller and SHALL apply a stricter limit to
sign-in and registration.

#### Scenario: New account receives a personal workspace
- **WHEN** a person registers with a valid full name, email, password, and phone number
- **THEN** the system creates the account, personal workspace, owner membership, and authenticated session atomically, and stores the phone number in E.164 format

#### Scenario: Invalid phone number is rejected
- **WHEN** a person registers with a phone number that is not valid for its country, such as `+1 555 000 0000`
- **THEN** the system creates nothing and returns a validation error for the `phoneNumber` field

#### Scenario: Missing full name is rejected
- **WHEN** a person registers with a blank full name
- **THEN** the system creates nothing and returns a validation error for the `displayName` field

#### Scenario: Cross-workspace access is denied
- **WHEN** an authenticated user requests a resource belonging only to another workspace
- **THEN** the system returns a not-found or forbidden response without exposing the resource

#### Scenario: Signed-out visitor opens a protected page
- **WHEN** a visitor without a session opens a protected page such as `/pipeline`
- **THEN** the web app shows sign-in, and after signing in returns the visitor to `/pipeline`

#### Scenario: Signed-in visitor opens sign-in
- **WHEN** a visitor with a session opens `/login`
- **THEN** the web app goes to the dashboard

#### Scenario: Session reports onboarding progress
- **WHEN** a signed-in user requests their session
- **THEN** the response says whether the user's workspace has a saved candidate profile

#### Scenario: Unfinished onboarding is resumed
- **WHEN** a user who has registered but not saved a candidate profile signs in or opens any protected page other than onboarding
- **THEN** the web app goes to onboarding, and after the profile is saved it allows the rest of the app

#### Scenario: Auth endpoints are rate-limited per IP
- **WHEN** one IP address sends more than 10 sign-in or registration requests within a minute
- **THEN** further requests in that minute return 429 Too Many Requests

#### Scenario: Every endpoint is rate-limited per caller
- **WHEN** one signed-in user, or one IP address when signed out, sends more than 120 API requests within a minute
- **THEN** further requests in that minute return 429 Too Many Requests, and other callers are unaffected

### Requirement: Candidate profile and preferences

The system SHALL store an editable candidate profile and job preferences,
including residence, IANA timezone, target roles, seniority, skills,
experience summary, employment types, available working hours, and optional
minimum compensation with currency. Profile setup SHALL NOT pre-fill a fixed
country, timezone, or working-hours zone; it SHALL suggest the timezone and
residence from the user's device, and the user SHALL choose residence from a
list of English country names.

#### Scenario: Ghanaian candidate saves preferences
- **WHEN** a user saves Ghana as residence and Africa/Accra as timezone with valid job preferences
- **THEN** the system persists the values in the user's active workspace and returns them for review

#### Scenario: Setup suggests residence and timezone from the device
- **WHEN** a new user opens profile setup on a device whose timezone is `Europe/Lisbon`
- **THEN** the timezone field shows `Europe/Lisbon` and residence shows Portugal, and both remain editable before saving

#### Scenario: Shared timezone suggests its main country
- **WHEN** a new user's device timezone is `Africa/Lagos`, which several countries share
- **THEN** residence suggests Nigeria, the timezone's main country, and stays editable

#### Scenario: Timezone without a country leaves residence empty
- **WHEN** the device timezone belongs to no country, such as `UTC`
- **THEN** residence starts empty and the user must choose one before saving

#### Scenario: Residence comes from the country list
- **WHEN** a user picks a residence in profile setup
- **THEN** the saved value is the English country name shown in the list, such as `United Kingdom`, not free text

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
