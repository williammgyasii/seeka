## MODIFIED Requirements

### Requirement: Landing page sends visitors to sign up

The landing page SHALL state the promise above the fold: stop applying cold;
Seeka tells you which jobs fit, then gets your application to a real person
inside the company. Its primary action SHALL link to
`https://app.seekajobs.tech/login?mode=register`. The page SHALL say that Seeka
does not guarantee a job or a reply and that the user sends every message. It
SHALL NOT show testimonials, counts, or results Seeka cannot prove.

#### Scenario: Primary action
- **WHEN** a visitor clicks the primary call to action
- **THEN** they reach the app's sign-in page with account creation selected

#### Scenario: Returning user
- **WHEN** a visitor clicks "Sign in"
- **THEN** they reach `https://app.seekajobs.tech/login`

#### Scenario: No job guarantee
- **WHEN** a visitor reads the landing page or its FAQ in search results
- **THEN** they see "Will Seeka get me a job?" answered "No", with what Seeka
  does instead and that the user sends the message
