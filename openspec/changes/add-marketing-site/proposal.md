## Why

Seeka has no public front door. `seekajobs.tech` is unused, the app at
`app.seekajobs.tech` is a sign-in wall, and search engines see an empty React
shell. People who search for help at the moment applying stops working ("how
to get a referral", "find the hiring manager", "applied to 100 jobs and no
response") should land on a page that explains Seeka and sends them to sign up.

## What Changes

- A static marketing site in `seeka-marketing/` (Astro), served at
  `https://seekajobs.tech`. `www.seekajobs.tech` and the production pages.dev
  host redirect permanently to the apex.
- A landing page built on one promise: stop applying cold. Seeka tells you which
  jobs fit, then gets your application to a real person inside the company.
- Four guide articles for searches where job seekers get stuck after finding a
  job. These are the pages meant to rank; Seeka does not compete on "job boards".
- Technical SEO on every page: title, description, canonical URL, Open Graph and
  Twitter cards, JSON-LD structured data, `sitemap.xml`, and `robots.txt`.
- The app at `app.seekajobs.tech` is marked `noindex`, and its sign-in page opens
  on account creation when linked with `?mode=register`.
- CI checks the site on every pull request (build, tests, Lighthouse budgets);
  a release deploys it with production approval.

## Non-goals

- A blog engine, CMS, or newsletter.
- Paid ads, and analytics beyond Cloudflare's own.
- Ranking for broad listing terms such as "job boards" or "remote jobs".
- Testimonials, user counts, or results Seeka cannot yet prove.

## Capabilities

### New Capabilities

- `marketing-site`: the public site, its SEO contract, and how it hands visitors
  to the app.

### Modified Capabilities

## Impact

- New `seeka-marketing/` project and two workflows (`marketing-ci.yml`,
  `marketing-deploy.yml`).
- `infra/cloudflare`: a second Pages project, apex and `www` domains, DNS, tests.
- `seeka-frontend`: `noindex` meta tag and the `?mode=register` entry.
- Google Search Console verification needs the owner's Google account; it is a
  manual follow-up, not part of this change.
