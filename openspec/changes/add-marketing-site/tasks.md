## 1. App hand-off

- [x] 1.1 Failing frontend tests: `index.html` carries `noindex`; `/login?mode=register` shows account creation
- [x] 1.2 Add the meta tag and read `mode` from the query; verify tests, lint, and build

## 2. Site skeleton and SEO contract

- [x] 2.1 Scaffold `seeka-marketing` (Astro, Tailwind, sitemap); failing tests over `dist` for metadata, JSON-LD, sitemap, robots, 404, and sign-up links
- [x] 2.2 `BaseLayout`, `robots.txt`, 404 page, and the redirect middleware with its tests; verify green

## 3. Content

- [x] 3.1 Landing page from the approved copy
- [x] 3.2 Guides index and four guides with `Article` JSON-LD
- [x] 3.3 Visual check in the browser at mobile and desktop widths

## 4. Delivery

- [x] 4.1 Failing Terraform tests for the `seeka-marketing` project, apex and `www` domains, and DNS; then the resources
- [x] 4.2 `marketing-ci.yml` (build, test, Lighthouse budgets) and `marketing-deploy.yml` (release, approval, deploy, smoke test)
- [x] 4.3 Release; confirm the apex serves the site, `www` redirects, and the app sends `noindex`
- [x] 4.4 `openspec validate add-marketing-site --strict`
