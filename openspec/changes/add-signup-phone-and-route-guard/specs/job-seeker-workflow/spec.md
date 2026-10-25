## MODIFIED Requirements

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

#### Scenario: Onboarding is its own page
- **WHEN** a user with unfinished onboarding views onboarding
- **THEN** the page shows no app navigation, and offers sign-out

#### Scenario: Finished onboarding is not shown again
- **WHEN** a user who has saved a candidate profile opens onboarding
- **THEN** the web app goes to the dashboard

#### Scenario: Auth endpoints are rate-limited per IP
- **WHEN** one IP address sends more than 10 sign-in or registration requests within a minute
- **THEN** further requests in that minute return 429 Too Many Requests

#### Scenario: Every endpoint is rate-limited per caller
- **WHEN** one signed-in user, or one IP address when signed out, sends more than 120 API requests within a minute
- **THEN** further requests in that minute return 429 Too Many Requests, and other callers are unaffected
