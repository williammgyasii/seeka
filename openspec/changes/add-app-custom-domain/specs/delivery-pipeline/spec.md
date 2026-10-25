## ADDED Requirements

### Requirement: Branded production origin

The production web app SHALL be served at `https://app.seekajobs.tech`, with the
domain and its DNS record owned by Terraform. Requests to the generated
production Pages host SHALL redirect permanently to the same path and query on
the branded host. Preview deployment hosts SHALL NOT be redirected.

#### Scenario: Branded address serves the app and API
- **WHEN** a person opens `https://app.seekajobs.tech` or calls `/api/health` on it
- **THEN** the app loads and the API answers through the same origin

#### Scenario: Old address redirects
- **WHEN** a person opens a path such as `/verify-email/confirm?userId=…&token=…` on `seeka-cth.pages.dev`
- **THEN** the response is a 301 to the same path and query on `https://app.seekajobs.tech`

#### Scenario: Preview deployments stay reachable
- **WHEN** a person opens a preview host such as `abc123.seeka-cth.pages.dev`
- **THEN** the preview is served without a redirect

#### Scenario: Links use the branded address
- **WHEN** the API builds a verification link in production
- **THEN** the link starts with `https://app.seekajobs.tech`
