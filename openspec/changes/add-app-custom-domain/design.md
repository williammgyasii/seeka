## Context

- Terraform under `infra/cloudflare` owns the Pages project and the Worker.
  Wrangler owns deployments.
- Release workflows (infrastructure, backend, frontend) run independently and
  may finish in any order.
- The session cookie is `__Host-seeka-session`, which is bound to one exact host.
- `CLOUDFLARE_PAGES_URL` feeds `APP_PUBLIC_URL` (verification links) and the
  smoke tests.

## Decisions

### Two releases, not one

A custom domain is usable only after Cloudflare issues its certificate, which
happens after Terraform creates it. If the redirect or the new
`CLOUDFLARE_PAGES_URL` shipped in the same release, `pages.dev` could redirect
to a host that does not load yet, and verification emails could link to it.

1. Release 1: Terraform creates the domain and DNS record. Confirm
   `https://app.seekajobs.tech` loads and `/api/health` answers.
2. Release 2: set `CLOUDFLARE_PAGES_URL` to `https://app.seekajobs.tech` and ship
   the redirect.

`wrangler pages deploy` cannot pass a runtime variable to Functions, so the
redirect cannot be gated on the variable inside one release.

### Terraform owns the DNS record

- `data.cloudflare_zone` looks the zone up by name, so no zone ID is committed.
- `cloudflare_pages_domain` attaches `app.seekajobs.tech` to the project.
- `cloudflare_dns_record` is a proxied CNAME from `app` to the project's
  generated subdomain, so it follows the project if it is recreated.

### Redirect only the production pages.dev host

A root Pages middleware returns 301 to `https://app.seekajobs.tech` with the same
path and query when the host is exactly `seeka-cth.pages.dev`. Any other host
(the custom domain, preview subdomains, local development) passes through.
`/api` requests are redirected too: a signed-in browser on the old host has no
valid cookie for the new host anyway, and 301 keeps the method for `GET` only,
which is the only method a stale bookmark or link sends.

Alternative: keep both hosts live. Rejected because each host gets its own
`__Host-` cookie, so a person would appear signed out after switching, and
search engines would index two copies.

## Risks / Trade-offs

- [Certificate takes minutes to issue] → Release 2 waits until the domain loads.
- [Someone is mid-session on pages.dev at release 2] → They are redirected and
  sign in once on the new host.
- [Links in already-sent verification emails point at pages.dev] → The redirect
  keeps the path and query, so `/verify-email/confirm?...` still works.

## Migration Plan

1. Merge and release this change's Terraform (release 1).
2. Verify `https://app.seekajobs.tech/` and `/api/health`.
3. Set the `production` variable `CLOUDFLARE_PAGES_URL=https://app.seekajobs.tech`.
4. Merge the redirect and release (release 2).

Rollback: set the variable back and redeploy the previous Pages deployment. The
domain and DNS record can stay; they are harmless without the redirect.
