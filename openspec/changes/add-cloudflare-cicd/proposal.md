## Why

Seeka has a locally verified application but no source-control home, automated
quality gate, reproducible cloud infrastructure, or deployment path. Establishing
those controls now makes every later feature deployable without replacing the
architecture or relying on manual dashboard configuration.

## What Changes

- Move the repository to the authenticated personal GitHub account as a public
  repository.
- Add independent component CI workflows that build and test the frontend,
  backend, edge adapter, and Terraform configuration on pull requests and
  commits to `main`.
- Add independent infrastructure, backend, and frontend CD workflows that
  deploy the exact revision named by an immutable `v*` release tag; commits to
  `main` never deploy directly.
- Provision Cloudflare Pages and a private Worker/Container topology with
  Terraform; deploy immutable application artifacts with Wrangler.
- Keep browser API traffic same-origin through a Pages Function service binding.
- Persist ASP.NET Core Data Protection keys across container restarts and disable
  Hangfire processing where scale-to-zero cannot provide a durable scheduler.
- Use generated `pages.dev` and Cloudflare service endpoints without a custom
  domain.

## Capabilities

### New Capabilities

- `delivery-pipeline`: Automated verification and controlled deployment of Seeka
  from GitHub to Cloudflare.

### Modified Capabilities

None.

## Impact

The change affects Git/GitHub configuration, GitHub Actions, Cloudflare account
resources, Terraform state, the frontend's same-origin API edge, a new Worker
container adapter, backend proxy/session configuration, secret handling,
deployment documentation, and the pending Neon development database setup.
Cloudflare Containers requires the Workers Paid plan.
