## ADDED Requirements

### Requirement: Public site at the apex domain

The marketing site SHALL be served at `https://seekajobs.tech`, with its Pages
project, domains, and DNS owned by Terraform. Requests to `www.seekajobs.tech`
and to the production pages.dev host SHALL redirect permanently to the same path
and query on the apex. Preview deployment hosts SHALL NOT be redirected.

#### Scenario: Apex serves the landing page
- **WHEN** a person opens `https://seekajobs.tech/`
- **THEN** the landing page loads

#### Scenario: www redirects
- **WHEN** a person opens `https://www.seekajobs.tech/guides/?a=1`
- **THEN** the response is a 301 to `https://seekajobs.tech/guides/?a=1`

#### Scenario: Preview deployments stay reachable
- **WHEN** a person opens a preview host such as `abc123.seeka-marketing.pages.dev`
- **THEN** the preview is served without a redirect

### Requirement: Every page is search-ready

Every page SHALL have a unique title and meta description, a canonical URL on
`https://seekajobs.tech`, Open Graph and Twitter card tags, and JSON-LD
structured data. The site SHALL publish `sitemap.xml` listing every page and a
`robots.txt` that allows crawling and points to the sitemap.

#### Scenario: Page metadata
- **WHEN** any built page is inspected
- **THEN** it has one `<h1>`, a title, a description, an absolute canonical URL, `og:title`, `og:description`, `og:url`, `og:image`, and parseable JSON-LD

#### Scenario: Sitemap and robots
- **WHEN** a crawler fetches `/robots.txt` and `/sitemap-index.xml`
- **THEN** robots allows all paths and names the sitemap, and the sitemap lists the landing page and every guide

#### Scenario: Unknown page
- **WHEN** a person opens a path that does not exist
- **THEN** the site answers 404 with a page that links home and to the guides

### Requirement: Landing page sends visitors to sign up

The landing page SHALL state the promise above the fold: stop applying cold;
Seeka tells you which jobs fit, then gets your application to a real person
inside the company. Its primary action SHALL link to
`https://app.seekajobs.tech/login?mode=register`. The page SHALL NOT show
testimonials, counts, or results Seeka cannot prove.

#### Scenario: Primary action
- **WHEN** a visitor clicks the primary call to action
- **THEN** they reach the app's sign-in page with account creation selected

#### Scenario: Returning user
- **WHEN** a visitor clicks "Sign in"
- **THEN** they reach `https://app.seekajobs.tech/login`

### Requirement: Guides answer stuck-applicant searches

The site SHALL publish guide articles at `/guides/<slug>/` for searches made
after finding a job: getting a referral, finding the hiring manager, writing to
a recruiter, and getting no response. Each guide SHALL carry `Article` JSON-LD,
be listed on `/guides/`, and end with a call to action into the app.

#### Scenario: Guide index
- **WHEN** a visitor opens `/guides/`
- **THEN** every guide is listed with its title and summary

#### Scenario: Guide page
- **WHEN** a visitor opens a guide
- **THEN** it has `Article` structured data, a publish date, and a sign-up call to action

### Requirement: The app stays out of search results

Pages served by `app.seekajobs.tech` SHALL tell search engines not to index
them. The app's sign-in page SHALL open on account creation when the URL has
`?mode=register`.

#### Scenario: noindex
- **WHEN** a crawler fetches any app page
- **THEN** the HTML carries `<meta name="robots" content="noindex">`

#### Scenario: Register link
- **WHEN** a signed-out visitor opens `/login?mode=register`
- **THEN** the account-creation form is shown

### Requirement: Fast, accessible pages, checked on every change

Pull requests touching the site SHALL build it, run its tests, and run Lighthouse
against the built pages. A release SHALL deploy it after production approval.
Lighthouse scores SHALL be at least 95 for SEO, accessibility, and best practices,
and at least 90 for performance.

#### Scenario: Budget failure blocks the merge
- **WHEN** a change drops a Lighthouse score below its budget
- **THEN** the marketing CI check fails

#### Scenario: Release deploys the site
- **WHEN** a `v*` release is published and approved
- **THEN** the site is deployed to the `seeka-marketing` Pages project and the apex answers 200
