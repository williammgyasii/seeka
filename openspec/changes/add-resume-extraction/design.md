## Context

Onboarding is one form (`PUT /api/candidate`, `CandidateSetupRequest`) whose
Save creates the candidate profile and unlocks the app. Profile fields are free
text and lists; seniority has no fixed vocabulary. The API runs as a .NET
container behind the Cloudflare edge Worker, with Postgres on Neon. There is no
file storage and no AI call today. See `proposal.md` for why.

## Goals / Non-Goals

**Goals:**
- A user can upload a résumé and get a pre-filled form in one request.
- Every failure after the upload is accepted falls back to the manual form.
- AI output never reaches the profile without the user's Save.

**Non-Goals:**
- Résumé version history, presigned direct-to-R2 uploads, OCR of scanned PDFs.
- Re-running extraction on an already stored résumé (later, if needed).
- Using résumé text in assessments (that is `add-job-fit-verdict`).

## Decisions

### Request flow

```
POST /api/candidate/resume   (multipart + Idempotency-Key; verified user; 5/hour)
  ResumeController → ResumeManager.UploadAsync
    1. ResumeUploadPolicy (engine)  reject non-PDF, > 5 MB, or a missing key
    2. ResumeAccess.Find(key)       completed → replay; running → 409
    3. ResumeAccess.Claim(key)      insert a Pending row; unique (workspace, key)
    4. ResumeFileAccess.Save        resumes/{workspaceId}/{resumeId}.pdf
    5. PdfTextAccess.Read           PdfPig; empty text → Unreadable
    6. ResumeExtractionAccess       OpenAI structured output → raw facts
    7. ResumeDraftPolicy (engine)   raw facts → profile draft
    8. ResumeAccess.Complete        store status + draft; mark older rows
                                    replaced and return their file keys
    9. ResumeFileAccess.Delete      the replaced files
  ← 200 { status: Extracted | Unreadable | ExtractionFailed, draft? }
GET  /api/candidate/resume        current résumé: file name, status, draft
GET  /api/candidate/resume/file   streams the current PDF
```

### Idempotency key on upload
The browser makes one UUID when the user picks a file and sends it as
`Idempotency-Key` on every attempt for that file. The `(WorkspaceId, Key)`
unique index is the lock: the first request inserts a Pending row, a
concurrent duplicate fails the insert and gets 409, and a later duplicate
finds the completed row and gets its stored result. The stored draft is what
makes the replay possible, and it also refills the form after a page reload.
A Pending row older than five minutes counts as abandoned (a crashed request)
and may be reclaimed. If storage throws, the manager removes its Pending row
before the error surfaces, so a retry can start again. Outgoing calls need no
key: re-writing the same R2 object is harmless, and the OpenAI call has no
side effect beyond cost, so it gets one retry on 429 or 5xx.

Rows are kept per upload, not per workspace, so a stale retry of an older key
replays its old result instead of replacing the newer résumé. Only the newest
completed row is current; replaced rows keep their result but lose their file.

The manager sequences; the engines hold the rules; each access type does one
reach into one resource. The engines have no PDF, HTTP, or SDK types, so they
are tested with plain values.

### Read text locally with PdfPig, then send text to OpenAI
Chosen over sending the PDF to OpenAI. Text is cheaper, the prompt input is
inspectable and testable, and the PDF never leaves our storage. Cost: scans
yield no text; they return `Unreadable` and the user types the profile.

### Structured output with a strict JSON schema
The OpenAI call uses `response_format: json_schema` with `strict: true`, every
field nullable. The model returns `null` for facts the résumé does not state,
and the draft policy leaves those fields blank. The prompt says to extract,
not to judge. The model name is configuration (`OpenAI:Model`, default
`gpt-5.4-mini`), not code.

### ResumeDraftPolicy owns cleanup
Trims text, drops empty and duplicate skills and titles (case-insensitive),
caps list lengths, rejects years of experience outside 0–60, and truncates the
summary to the form's limit. This is the testable rule set; the AI access only
transports.

### Separate manager and controller
Résumé processing changes for different reasons (AI, files) than account and
profile membership, so it gets its own `ResumeManager` and `ResumeController`
rather than growing `MembershipManager`.

### Store the file first, extract second
The file is saved before text reading so a failed extraction never forces a
re-upload. The response status tells the UI which message to show; failures
after storage are a 200 with a status, not a 5xx, because the upload itself
succeeded.

### R2 through the S3-compatible API
The container talks to R2 with the AWS S3 SDK (`ForcePathStyle`, R2 endpoint)
using an R2 API token scoped to one bucket. A Worker binding was rejected
because the backend, not the edge, owns the file. Development uses a local
folder access type behind the same interface, chosen by the same
environment-and-configuration rule as email delivery.

### Rate limit, not a usage table
A new `resume` rate-limit policy (5 per user per hour) guards spend. A
per-user usage count can come later if limits need to be shown or sold.

## Risks / Trade-offs

- [Personal data goes to OpenAI] → only text is sent, API data is not used for
  training by default, and the upload copy says that the résumé is read by AI.
- [Old file left behind if delete fails] → the database row is the truth;
  delete failures are logged, and keys are workspace-prefixed for cleanup.
- [A 5 MB body passes through the edge and container] → acceptable at this
  size; presigned uploads are the path if limits grow.
- [Model output drifts] → strict schema plus the draft policy bound what can
  reach the form, and the user reviews before Save.
- [Leaked or exhausted OpenAI key] → project key with a monthly spend limit,
  rotated on exposure.

## Migration Plan

1. Terraform adds the private `seeka-resumes` bucket.
2. Create an R2 API token scoped to that bucket; store its key pair and the
   OpenAI key as `production` secrets passed to the container like
   `EMAIL_API_TOKEN`.
3. An EF migration adds the `Resumes` table; the release applies it.
4. Rollback: the upload step is additive; reverting the frontend restores the
   manual form, and the table and bucket can stay.
