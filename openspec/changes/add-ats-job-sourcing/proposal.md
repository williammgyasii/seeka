## Why

Users should not have to find every vacancy themselves. Seeka should provide
jobs as well as accept pasted ones. Greenhouse, Lever, and Ashby publish public
job-board APIs for each company, designed for this use.

## What Changes

- Ingest jobs from public ATS boards for a curated list of companies. This is
  not global search.
- Store sourced jobs in the same job record as pasted jobs, with ATS
  provenance, first-seen and last-checked timestamps, and closed-posting
  detection.
- Run sourced jobs through extraction and the verdict so users see jobs that
  fit their search intent.
- Keep manual paste as the second way in.
- For the MVP, refresh on demand. Production has no reliable scheduler
  (Hangfire processing is disabled on Cloudflare).

## Depends on

- `add-job-fit-verdict`, so sourced jobs are assessed rather than listed raw.

## Open questions for the deep dive

- Who curates the company list, how it is seeded, and how it grows.
- Refresh trigger: an admin action, a user action, or a Cloudflare Cron
  Trigger later.
- How to control AI extraction cost across large feeds.
- Whether sourced jobs are shared across workspaces, like the insider cache.

## Capabilities

### New Capabilities

- `job-sourcing`: Curated ATS ingestion, provenance, refresh, and closed
  posting detection.

### Modified Capabilities

- `job-seeker-workflow`: Jobs can enter the workspace from a sourced feed as
  well as manual import.

## Impact

- New Infrastructure access for each ATS provider.
- New job feed UI.
- Scheduling decision deferred.
