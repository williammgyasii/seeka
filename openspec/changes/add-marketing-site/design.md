## Context

The app is a React single-page app on the `seeka` Pages project at
`app.seekajobs.tech`. Crawlers that do not run JavaScript see an empty `<div>`,
and every useful route sits behind sign-in. The apex `seekajobs.tech` has no
records. Releases already deploy Pages with Wrangler after production approval,
and Terraform owns the Cloudflare resources.

## Goals / Non-Goals

**Goals:** a fast static site at the apex that explains Seeka in five seconds,
ranks for long-tail searches made by stuck applicants, and hands visitors to
sign-up.

**Non-Goals:** a CMS, a newsletter, paid acquisition, and broad listing terms.

## Decisions

### Astro, static output, in its own folder and Pages project

```
 seekajobs.tech  ------------>  seeka-marketing (Pages, static HTML)
 www.seekajobs.tech  --301-->   seekajobs.tech
       |  "Get started" / "Sign in"
       v
 app.seekajobs.tech  -------->  seeka (Pages, React app, noindex)
```

- Astro renders plain HTML at build time and ships no JavaScript unless a
  component asks for it. Crawlers get full content, and performance budgets are
  easy to hold.
- Alternatives: prerendering the React app (couples marketing releases to the
  app and keeps the SPA's JavaScript weight); hand-written HTML (no shared
  layout, sitemap, or Markdown guides).
- A separate Pages project keeps the volatile marketing copy apart from the app.
  They share brand colours by value, not by importing app code.

### Redirect in a Pages middleware

`functions/_middleware.ts` redirects `www.seekajobs.tech` and the production
`seeka-marketing*.pages.dev` host to the apex. The app's `canonical-host.ts`
does the same job. A zone redirect rule would need extra token permissions for
no gain at this traffic.

### SEO contract lives in one layout

`BaseLayout.astro` takes `title`, `description`, `path`, and a JSON-LD object,
and writes the canonical URL, Open Graph and Twitter tags. Pages cannot forget
them. `@astrojs/sitemap` builds the sitemap from the routes; `robots.txt` is
static.

### Tests read the built HTML

Vitest builds nothing itself; `npm test` runs after `npm run build` and parses
`dist/**/*.html` with `node-html-parser`, asserting the spec's metadata,
structured data, links, and the redirect function. Lighthouse CI (`@lhci/cli`)
runs the score budgets against `dist` in CI.

### Search terms mapped to pages

| Page | Primary search intent |
|---|---|
| `/` | "stop applying cold", "get a referral for a job", "AI job application assistant" |
| `/guides/how-to-get-a-referral/` | "how to get a referral when you don't know anyone" |
| `/guides/find-the-hiring-manager/` | "how to find the hiring manager for a job" |
| `/guides/message-a-recruiter/` | "how to message a recruiter", "cold email recruiter template" |
| `/guides/no-response-to-applications/` | "applied to 100 jobs no response" |

### Landing copy (message first)

- **Headline:** "You applied. Nobody replied."
- **Subhead:** "Stop applying cold. Seeka tells you which jobs fit, then gets
  your application to a real person inside the company."
- **Primary action:** "Get started free". **Secondary:** "See how it works".
- **Problem:** online applications land in a pile that a screen filters before a
  person reads them. Sending more of them makes the pile bigger, not the odds.
- **How it works (three steps):** upload your résumé and Seeka fills in your
  profile; paste a job and get a straight answer on fit, with what is missing;
  find the people inside the company and send a message written from your
  real experience.
- **Honesty block:** Seeka drafts, you send. Nothing goes out without you, and
  nothing is invented about your experience.
- **Closing action:** "Your next application goes to a person." + primary action.

No testimonials or numbers until they exist.

## Risks / Trade-offs

- [Rankings take months and depend on content, not tooling] → the guides are
  real advice that stands alone; more guides follow from Search Console data.
- [Features described before all of them ship (fit verdict, insider search)] →
  copy describes what the product does today or says "coming soon"; review the
  page as each backlog change lands.
- [Two Pages projects drift in branding] → colours and font are copied once and
  named the same; the brand changes rarely.

## Migration Plan

1. Terraform release creates the project, domains, and DNS.
2. The same release deploys the site; the smoke test hits the apex.
3. Rollback: re-deploy the previous release; the app is unaffected.

## Open Questions

- Search Console ownership (needs the owner's Google account).
