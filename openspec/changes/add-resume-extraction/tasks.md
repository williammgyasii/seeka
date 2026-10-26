## 1. Engines (no I/O)

- [x] 1.1 `ResumeUploadPolicy` accepts PDFs up to 5 MB with a valid key, rejects other types, sizes, and missing keys, and treats Pending claims older than five minutes as abandoned; verify with engine unit tests
- [x] 1.2 `ResumeDraftPolicy` trims, de-duplicates, caps lists, blanks out-of-range years and missing facts; verify with engine unit tests

## 2. Manager

- [x] 2.1 `ResumeManager.UploadAsync` claims the key, stores, reads, extracts, drafts, and completes, returning `Extracted`, `Unreadable`, or `ExtractionFailed`; verify with manager tests using fake access types
- [x] 2.2 Repeated keys replay the stored result, running keys return in-progress, storage failures release the claim, and replaced files are deleted; verify with manager tests

## 3. Resource access

- [x] 3.1 `PdfTextAccess` (PdfPig) returns text for a text PDF and empty text for a PDF without text; verify with generated fixture PDFs
- [x] 3.2 `ResumeAccess`, the `Resumes` entity, and an EF migration with a unique `(WorkspaceId, IdempotencyKey)` index; verify with access tests and the migration building
- [x] 3.3 `OpenAiResumeExtractionAccess` sends text with a strict JSON schema, retries once on 429/5xx, and maps the reply; verify with a stubbed HTTP handler
- [x] 3.4 `R2ResumeFileAccess` and a local-folder access for Development, chosen by configuration; verify with a policy test and a local save/read/delete test

## 4. API

- [x] 4.1 `POST /api/candidate/resume`, `GET /api/candidate/resume`, and `GET /api/candidate/resume/file` with the verified policy and a 5-per-hour `resume` rate limit; verify with `WebApplicationFactory` tests (400, 409, replay, 403 unverified)

## 5. Frontend

- [x] 5.1 Onboarding upload step with skip, an idempotency key per chosen file, status messages, and the pre-filled form, including after reload; verify with component tests
- [x] 5.2 Run the flow in the browser locally with a real résumé and confirm the form is pre-filled and nothing saves before Save

## 6. Production

- [x] 6.1 Terraform private `seeka-resumes` bucket; verify with `terraform test`
- [ ] 6.2 Production secrets for OpenAI and the bucket-scoped R2 token, passed through the edge Worker; verify with the edge routing test and a release
- [ ] 6.3 Upload a résumé on `app.seekajobs.tech` and confirm the pre-filled form
- [ ] 6.4 `openspec validate add-resume-extraction --strict`
