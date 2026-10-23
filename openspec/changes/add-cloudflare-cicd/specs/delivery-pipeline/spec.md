## Purpose

Provide repeatable quality gates and controlled cloud delivery so every deployed
Seeka version is tested, traceable, securely configured, and recoverable.

## ADDED Requirements

### Requirement: Components pass independent automated verification

Independent component pipelines SHALL build and test the backend and frontend,
lint the frontend, validate Terraform, and prove the production container image
builds on pull requests and commits to `main`.

#### Scenario: Verification fails
- **WHEN** any required build, test, lint, infrastructure validation, or container build fails
- **THEN** the affected component pipeline reports failure without deploying

#### Scenario: Pull request is healthy
- **WHEN** all required verification steps pass for a pull request
- **THEN** each affected component reports a successful CI result suitable for branch protection

### Requirement: Production deployment is release-tagged and traceable

Independent infrastructure, backend, and frontend deployment pipelines SHALL
run only for immutable version tags matching `v*`, SHALL check out that exact
tagged revision, and SHALL use the protected production environment. A commit
or merge to `main` SHALL NOT deploy production directly.

#### Scenario: Main revision passes CI
- **WHEN** a revision is merged or pushed to `main`
- **THEN** affected component CI runs and no production deployment starts

#### Scenario: Version release is published
- **WHEN** an authorised operator publishes a `v*` tag for a revision from `main`
- **THEN** each independent deployment pipeline checks out and deploys that exact tagged revision

#### Scenario: Migration fails
- **WHEN** the production database migration command fails
- **THEN** backend artifact deployment stops while independently managed frontend and infrastructure deployments retain their own status

#### Scenario: First production release
- **WHEN** the first release tag is prepared
- **THEN** the operator provisions the stable Terraform resources before starting the independent application deployments

### Requirement: Browser and API share a secure application origin

The deployed frontend SHALL use its generated HTTPS Pages address and SHALL
forward browser API requests through an internal service binding to a backend
that is not directly exposed through a public Worker development route.

#### Scenario: Authenticated browser request
- **WHEN** the browser sends an authenticated request to `/api` on the Pages address
- **THEN** the request reaches the backend through the service binding while the secure session cookie remains same-origin

#### Scenario: Direct backend access
- **WHEN** an internet client attempts to use a default public Worker route
- **THEN** no public backend development route is available

### Requirement: Infrastructure is reproducible without committed secrets

Cloud resources and their non-secret configuration SHALL be declared in
Terraform, remote state SHALL be protected from concurrent modification, and
credentials SHALL be supplied only through protected deployment secrets.

#### Scenario: Infrastructure plan is reviewed
- **WHEN** infrastructure configuration changes
- **THEN** CI produces a validated Terraform plan without exposing credential values

#### Scenario: Fresh environment is provisioned
- **WHEN** an authorised operator applies the reviewed Terraform configuration
- **THEN** the required Pages, Worker, binding, and supporting resources are created or reconciled predictably

### Requirement: Stateful authentication survives container replacement

The deployed backend SHALL persist cryptographic key material required to read
existing authentication sessions outside the container filesystem.

#### Scenario: Container restarts
- **WHEN** Cloudflare replaces or wakes the backend container
- **THEN** a valid, unexpired session issued before replacement remains readable

### Requirement: Scale-to-zero does not claim durable scheduling

The Cloudflare deployment SHALL NOT run persistent Hangfire processing while
container lifetime cannot guarantee continuous polling.

#### Scenario: Backend container starts
- **WHEN** the backend starts in the Cloudflare environment
- **THEN** request handling is enabled and Hangfire job processing is disabled
