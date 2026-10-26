## Why

Onboarding asks a new user to type fourteen profile fields that their résumé
already contains. Seeka should read the résumé and fill them in, so the first
step is an upload rather than a long form. Keeping the résumé also gives later
work (the job-fit verdict) real evidence to judge fit against.

## What Changes

- Onboarding starts with a résumé upload (PDF, up to 5 MB), with an option to
  skip and type the profile by hand.
- The server keeps the PDF in private storage, one current résumé per
  workspace. A new upload replaces the old one and deletes its file.
- The server reads the PDF's text itself, then asks OpenAI to extract profile
  facts (name, seniority, years of experience, summary, skills, target titles)
  as structured output.
- The AI only extracts facts. It never decides fit or gives a recommendation.
- Extracted facts pre-fill the existing onboarding form. The user's Save is the
  confirmation; nothing extracted is used until it is saved.
- A PDF with no readable text, or an AI failure, still keeps the file and falls
  back to the empty form.
- Extraction is rate-limited per user as a spend guard. This is not billing.
- The OpenAI key is a secret (user-secrets locally, environment secret in
  production), never exposed to the browser.

## Capabilities

### New Capabilities

- `resume-profile`: Résumé upload and storage, text reading, AI fact
  extraction, and pre-filled onboarding.

### Modified Capabilities

None. The onboarding upload step is specified in `resume-profile`.

## Impact

- New Infrastructure access: PDF text reading (PdfPig), OpenAI, and R2 file
  storage through the S3-compatible API.
- New private R2 bucket managed by Terraform, and new production secrets for
  OpenAI and R2.
- New database table for the workspace's current résumé.
- Onboarding UI gains an upload step.
- Per-call AI cost.
