## 1. Prerequisites and source control

- [x] 1.1 Complete `use-neon-development-database` and verify migrations plus the local workflow against Neon
- [x] 1.2 Switch GitHub CLI to `williammgyasii`, delete the superseded `Seeka-App`, `Seeka-Server`, and `Seeka-Website` repositories, initialise Git with `main`, add secret-safe ignore rules, and create public `williammgyasii/seeka`; verify the personal account owns the empty remote without pushing
- [x] 1.3 Reauthenticate Wrangler and verify the selected Cloudflare account has Workers Paid and Containers access before provisioning resources

## 2. Runtime tests first

- [x] 2.1 Add failing backend tests for forwarded HTTPS, same-origin mutation checks, persistent Data Protection configuration, `/api/health`, and disabled Cloudflare Hangfire processing; verify failures describe missing production behavior
- [x] 2.2 Add failing Worker tests for stable container routing, secret injection boundaries, and private-route configuration; verify they fail before the adapter exists
- [x] 2.3 Add failing Pages Function tests for transparent `/api/*` service-binding proxying, including cookie headers; verify they fail before the proxy exists

## 3. Cloudflare runtime adapters

- [x] 3.1 Implement the minimum ASP.NET Core proxy, health, Data Protection, CSRF, and Hangfire environment changes; verify backend tests and migration generation pass
- [x] 3.2 Implement the typed Worker/Durable Object container adapter with current Wrangler configuration and a non-public Worker route; verify Worker tests, type generation, typecheck, and dry-run pass
- [x] 3.3 Implement the Pages Function service-binding proxy and Direct Upload configuration; verify proxy tests and local Pages/Worker integration pass
- [x] 3.4 Build the ASP.NET production image for the supported Cloudflare container architecture and verify it serves health on port 8080

## 4. Terraform infrastructure

- [x] 4.1 Add version-pinned Cloudflare Terraform configuration for the Pages project, Worker identity, and stable bindings with no secrets in state inputs; verify format, init, validate, and provider lock-file checks pass
- [x] 4.2 Add the R2 S3 backend with native lockfile plus a documented one-time private-bucket bootstrap; verify concurrent state access is rejected
- [x] 4.3 Add Terraform tests or mock-provider plans for names, public-route disablement, Direct Upload ownership, and service binding; verify the focused infrastructure checks pass

## 5. GitHub Actions

- [x] 5.1 Split least-privilege CI into backend, frontend, edge, infrastructure, and repository-policy workflows that all run for pull requests and `main`; verify workflow linting and local commands pass
- [x] 5.2 Add independent `v*` tag production workflows for Terraform apply, Neon migration plus Wrangler backend rollout, and Pages deployment; verify each checks out the tagged revision, uses the protected environment, performs focused smoke verification, and passes workflow linting
- [x] 5.3 Add a protected production environment and required CI branch check without storing secrets in repository files; verify GitHub reports the intended policy

## 6. Provision and verify

- [x] 6.1 Store the Cloudflare and Neon values in the GitHub production environment and derive masked, one-hour, bucket-scoped R2 state credentials during infrastructure deployment; verify only long-lived secret names and access policy are observable
- [x] 6.2 Bootstrap R2 and apply the reviewed Terraform plan; verify Pages, Worker identity, binding, and generated endpoints match state
- [ ] 6.3 Provision stable infrastructure before the first release, publish an approved `v*` tag, and verify the independent deployments plus registration, sign-in, persistence, health, and container-restart session continuity through the Pages URL
- [x] 6.4 Document setup, ownership boundaries, costs, rollback, secret rotation, and the Hangfire limitation; verify a clean operator can reproduce all non-secret steps
- [ ] 6.5 Run all checks, validate OpenSpec strictly, update task status, create the approved split commits by concern, and stop before push pending explicit approval
