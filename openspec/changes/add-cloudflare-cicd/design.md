## Context

The application is a React static client and an ASP.NET Core API with secure
cookie authentication, PostgreSQL persistence, and an explicit migration
command. The client already calls relative `/api` paths. The directory is not a
Git repository; GitHub CLI has both accounts available, with `wgyasi` active and
`williammgyasii` selected as the personal destination. Cloudflare CLI
authentication has expired. See `proposal.md` for motivation.

Cloudflare Containers is available only on Workers Paid. A Container is attached
to a Durable Object class and reached through a Worker; it is not an always-on
Docker host. Cloudflare documents Wrangler as the tool that builds and rolls out
container images. Terraform can own Worker identities and Pages projects while
Wrangler owns Worker versions and deployments.

## Goals / Non-Goals

**Goals:**

- Make every deployment originate from a verified GitHub revision.
- Keep infrastructure ownership declarative and application rollouts immutable.
- Preserve same-origin secure-cookie authentication without a custom domain.
- Make Terraform state remote, locked, and secret-safe.
- Retain a useful rollback path for frontend and backend artifacts.

**Non-Goals:**

- A production custom domain, multiple environments, or preview containers.
- Highly available multi-instance API routing in the first deployment.
- Running durable background scheduling inside a scale-to-zero container.
- Committing, pushing, tagging, or enabling live deployment without separate approval.

## Decisions

### Use one public personal GitHub monorepo with protected `main`

Switch GitHub CLI to `williammgyasii`, initialise the repository, and create
`williammgyasii/seeka` as public. Remove the superseded `Seeka-App`,
`Seeka-Server`, and `Seeka-Website` repositories so this monorepo is the single
source of truth. Component CI runs for pull requests and commits to `main`.
Production CD runs only for immutable `v*` release tags through a protected
GitHub environment. Repository creation is a bootstrap operation; workflows,
release tags, and branch policy become the ongoing delivery controls.

### Separate CI and CD by deployable component

Backend, frontend, edge, and infrastructure CI workflows have no cloud mutation
permission. Each workflow runs on every pull request and commit to `main`, while
restoring only its own dependencies and running focused tests, builds, lint,
OpenSpec, Terraform, or container checks.

Infrastructure, backend, and frontend CD workflows independently receive scoped
credentials through the protected production environment and run only for
`v*` tags. Infrastructure applies Terraform. Backend runs the explicit EF
migration, deploys the Worker and container with Wrangler, and checks API
health. Frontend deploys Pages and checks the public application. Each workflow
checks out the tagged revision, so releases are traceable without deploying
every commit to `main`.

Independent CD intentionally has no cross-workflow ordering. Stable resources
must be provisioned before the first release, and later infrastructure changes
must remain backward-compatible while component deployments converge.

### Split infrastructure ownership from artifact ownership

Terraform owns the Pages Direct Upload project, the Worker identity, and stable
account-level configuration. Wrangler owns generated Worker versions,
deployments, container images, rollouts, and Pages deployment artifacts.
Terraform and Wrangler never manage the same version/deployment object.

The Direct Upload choice is deliberate: GitHub Actions is the build authority,
so Cloudflare Git integration would create a second deployment path.

### Bootstrap remote Terraform state in private R2

Create one private `seeka-terraform-state` R2 bucket as a documented bootstrap
step, then use Terraform's S3 backend with R2's endpoint and native lockfile.
State credentials live only in local/GitHub secrets. Provider versions and the
dependency lock file are committed; plans contain no secret variables.

### Keep the browser same-origin through a Pages Function

The Pages deployment includes a catch-all `/api/*` Function with a service
binding to the private API Worker. It forwards the request and response without
changing cookie headers. The browser therefore sees only the generated
`pages.dev` origin, and the existing relative API client remains unchanged. The
generated Pages subdomain is recorded as a deployment variable rather than
assumed from the project name. The Worker disables its public `workers.dev`
route.

### Route to one named ASP.NET Container instance

The Worker uses the current Durable Object Container API and one stable instance
name for the first deployment. The Durable Object starts the image on demand,
waits for port 8080, injects runtime secrets, and forwards requests. This is the
smallest topology compatible with secure cookies and the current API.
Horizontal scaling is deferred because it needs explicit routing, connection
budgets, and session/key verification under concurrency.

### Adapt ASP.NET Core to the proxy and ephemeral runtime

Trust forwarded headers only from the known edge path so HTTPS redirects and
secure-cookie behavior use the original scheme. Add an `/api/health` check and
persist ASP.NET Core Data Protection keys in PostgreSQL so container replacement
does not invalidate sessions. Enforce same-origin mutation checks at the API
boundary.

Add configuration that omits `AddHangfireServer` in Cloudflare. Hangfire storage
remains compatible, but no claim is made that polling survives scale-to-zero.
Future scheduled work should use an always-on processor or Cloudflare-native
triggers that invoke bounded jobs.

### Treat database migrations as forward-compatible release steps

Complete the pending Neon change first. CD invokes the existing `--migrate`
command before artifact rollout. Migrations must use expand/contract changes so
the active prior API remains compatible if a later artifact stage fails.
Database rollback is not coupled to application rollback.

## Risks / Trade-offs

- [Cloudflare Container deploys are not transactional] → Deploy backend before
  frontend, smoke-test the public route, and retain Wrangler version rollback.
- [A cold container increases first-request latency] → Use a reasonable
  inactivity timeout and measure before paying for longer uptime.
- [One named instance limits throughput] → Treat it as the initial capacity
  boundary and add measured multi-instance routing later.
- [Pages Function proxy adds an edge hop] → Use a service binding, which stays
  inside Cloudflare and preserves the required browser origin.
- [Public source increases secret exposure risk] → Commit placeholders only,
  use scoped GitHub environment secrets, and scan generated plans/configuration.
- [R2 is a bootstrap dependency for its own Terraform backend] → Document the
  one-time CLI creation and import/manage only resources that do not create an
  ownership cycle.
- [Persistent Hangfire work is disabled] → Surface that limitation explicitly;
  do not enqueue production jobs until a durable processor exists.

## Migration Plan

1. Complete and verify `use-neon-development-database`.
2. Switch GitHub CLI to the personal account, initialise Git, and create the
   public remote without committing or pushing.
3. Add failing tests for the Pages proxy, Worker routing, persistent sessions,
   forwarded scheme, and Cloudflare Hangfire configuration.
4. Implement the edge adapters and backend runtime changes until tests pass.
5. Add Terraform, remote-state bootstrap, CI/CD workflows, and documentation;
   validate and dry-run locally.
6. Reauthenticate Cloudflare, create narrowly scoped credentials, bootstrap R2,
   and store deployment secrets in the protected GitHub environment.
7. Apply Terraform before the first release, then create an approved immutable
   `v*` tag to trigger the independent backend and frontend deployments.
8. Smoke-test registration, sign-in, persistence, and container replacement at
   the Pages URL before relying on later release tags.

Rollback restores the previous Pages deployment and Worker version. Terraform
changes are reverted through a reviewed plan. Forward-compatible database
migrations remain in place unless a separate, tested corrective migration is
required.
