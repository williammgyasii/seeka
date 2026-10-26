# Proposal

## Why

People will pay for Seeka only if they know what they are buying. The landing
page says what Seeka does, but not what it does not do. A visitor can read
"gets your application to a real person" as a promise of a job. Saying the
limits plainly builds trust and keeps the page clear of claims we cannot prove.

## What Changes

- Add a "No job guarantee" card to the "Seeka drafts. You send." section of the
  landing page.
- Add "Will Seeka get me a job?" as the first FAQ question, which also lands in
  the page's FAQ structured data.
- No change to the hero, the price, or the app.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `marketing-site`: the landing page states what Seeka does not promise.

## How we sell it

- **Promise:** Seeka helps you reach the person who can hire you with a better
  message; it does not promise a job or a reply.
- **Who it is for:** anyone deciding whether Seeka is worth signing up for.
- **What it does NOT do:** guarantee a job or a reply, send messages for you, or
  find contacts automatically (coming soon).
- **Marketing change:** the landing page at `https://seekajobs.tech/`, in the
  "Seeka drafts. You send." section and the FAQ.

## Impact

- `seeka-marketing/src/pages/index.astro`
- `seeka-marketing/tests/site.test.ts`
