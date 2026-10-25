## Why

Seeka now owns `seekajobs.tech`, but the app still lives at the generated
`seeka-cth.pages.dev` address. Verification emails, bookmarks, and anything a
user shares should point at a stable branded address that does not change if the
Pages project is ever recreated.

## What Changes

- Serve the production web app at `https://app.seekajobs.tech`.
- Terraform attaches the custom domain to the `seeka` Pages project and owns a
  proxied `app` CNAME in the `seekajobs.tech` zone.
- `CLOUDFLARE_PAGES_URL` becomes `https://app.seekajobs.tech`, which moves the
  verification-link base URL, both smoke tests, and the environment URL.
- Requests to `seeka-cth.pages.dev` redirect permanently to the same path on
  `app.seekajobs.tech`, so there is one origin and one session cookie.
  Preview deployments (`<hash>.seeka-cth.pages.dev`) are not redirected.

## Capabilities

### New Capabilities

### Modified Capabilities

- `delivery-pipeline`: adds a branded production origin and the canonical-host
  redirect.

## Impact

- `infra/cloudflare`: zone lookup, Pages domain, DNS record, and tests.
- The deploy token needs Zone Read and DNS Edit on `seekajobs.tech` (done).
- `seeka-edge/pages/functions/_middleware.ts`: the redirect.
- GitHub `production` variable `CLOUDFLARE_PAGES_URL`.
- No application code changes; the browser only calls relative `/api` paths.
