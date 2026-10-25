## ADDED Requirements

### Requirement: Verified email before workspace use

The system SHALL send a verification link to the email address given at
registration, and SHALL refuse all workspace access until that address is
verified. A person with an unverified address SHALL remain signed in and SHALL
be able to resend the link and correct the address. A verification link SHALL
expire after one day and SHALL work on any device, signed in or not.

#### Scenario: Registration sends a verification email
- **WHEN** a person registers successfully
- **THEN** the system sends a verification email containing a one-time link to the registered address, and the session reports the email as unverified

#### Scenario: Unverified account cannot use the workspace
- **WHEN** a signed-in person with an unverified email calls any workspace endpoint, such as the pipeline or candidate profile
- **THEN** the API returns 403 Forbidden and exposes no workspace data

#### Scenario: Unverified account sees only the check-your-inbox page
- **WHEN** a signed-in person with an unverified email opens any page of the web app
- **THEN** the web app shows the check-your-inbox page with the address the link was sent to

#### Scenario: Valid link verifies the address
- **WHEN** a person opens an unexpired verification link, on any device
- **THEN** the system marks the address verified, and the web app continues to onboarding if signed in or offers sign-in if not

#### Scenario: Expired or altered link is rejected
- **WHEN** a person opens a verification link that is expired, already superseded, or altered
- **THEN** the system leaves the address unverified and the web app explains that the link is invalid and offers a new one

#### Scenario: Resend a verification email
- **WHEN** a signed-in person with an unverified email asks for a new link
- **THEN** the system sends a new verification email, subject to the sign-in rate limit

#### Scenario: Correct a mistyped address
- **WHEN** a signed-in person with an unverified email submits a different, valid, unused address
- **THEN** the system replaces the address, keeps it unverified, and sends a verification email to the new address

#### Scenario: Address already in use
- **WHEN** a person corrects their address to one that another account uses
- **THEN** the system keeps the current address and returns a validation error for the `email` field

#### Scenario: Verified address cannot be changed here
- **WHEN** a person whose email is already verified submits a different address through the correction flow
- **THEN** the system refuses the change

#### Scenario: Email delivery fails at registration
- **WHEN** the email provider fails while a person registers
- **THEN** the account is still created, the person is signed in, and the check-your-inbox page lets them resend
