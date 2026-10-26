## ADDED Requirements

### Requirement: Résumé upload pre-fills onboarding

The system SHALL let a verified user upload a PDF résumé of up to 5 MB, SHALL
keep it privately as the workspace's one current résumé, and SHALL return
profile facts extracted from its text to pre-fill onboarding. Uploads SHALL be
idempotent by key. Extracted facts SHALL be used only after the user saves the
profile, and the extraction SHALL never decide fit.

#### Scenario: Upload pre-fills the profile form
- **WHEN** a user uploads a readable PDF résumé during onboarding
- **THEN** the onboarding form is filled with the extracted name, seniority, years of experience, summary, skills, and target titles, and nothing is saved until the user saves the form

#### Scenario: Unknown facts are left blank
- **WHEN** the résumé does not state a fact, such as years of experience
- **THEN** that field is left blank rather than guessed

#### Scenario: New upload replaces the old résumé
- **WHEN** a user uploads a résumé while the workspace already has one
- **THEN** the new file becomes the current résumé and the old file is deleted

#### Scenario: Unreadable PDF falls back to typing
- **WHEN** a user uploads a PDF with no readable text, such as a scan
- **THEN** the file is kept, the user is told the text could not be read, and the empty form is shown

#### Scenario: Extraction failure falls back to typing
- **WHEN** the AI service fails or times out
- **THEN** the file is kept, the user is told extraction failed, and the empty form is shown

#### Scenario: Wrong file is rejected
- **WHEN** a user uploads a file that is not a PDF, or is larger than 5 MB
- **THEN** nothing is stored and the user is told why

#### Scenario: Extraction is rate-limited
- **WHEN** one user uploads more than 5 résumés within an hour
- **THEN** further uploads that hour return 429 Too Many Requests

#### Scenario: Résumé is private to its workspace
- **WHEN** a signed-in user downloads their résumé
- **THEN** the API returns only their own workspace's current file, and the storage bucket has no public URL

#### Scenario: Retried upload is replayed
- **WHEN** an upload repeats an `Idempotency-Key` that the workspace already completed
- **THEN** the system returns the first upload's result without storing another file or calling the AI again

#### Scenario: Duplicate upload while the first is running
- **WHEN** an upload repeats an `Idempotency-Key` whose first request is still running
- **THEN** the system returns 409 Conflict and does no work

#### Scenario: Upload without a key
- **WHEN** an upload has no valid `Idempotency-Key` header
- **THEN** nothing is stored and the system returns 400

#### Scenario: Reload keeps the draft
- **WHEN** a user reopens onboarding after an extraction and before saving
- **THEN** the form is pre-filled from the stored draft without another AI call

#### Scenario: Skipping the upload
- **WHEN** a user chooses to skip the upload
- **THEN** onboarding shows the empty form, as it does today
