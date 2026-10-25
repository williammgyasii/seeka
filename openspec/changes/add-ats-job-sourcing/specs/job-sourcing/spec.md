## ADDED Requirements

### Requirement: Jobs sourced from public ATS boards

The system SHALL ingest jobs from the public Greenhouse, Lever, and Ashby boards
of a curated company list, SHALL store them in the same job record as pasted jobs
with ATS provenance and first-seen and last-checked times, and SHALL mark a job
closed when its posting disappears. Refresh SHALL happen on demand.

#### Scenario: Sourced job keeps its provenance
- **WHEN** a refresh finds a new posting on a curated company's board
- **THEN** the job is stored with its ATS, board URL, and first-seen time, and is assessed like a pasted job

#### Scenario: Removed posting is closed
- **WHEN** a refresh no longer finds a previously sourced posting
- **THEN** the job is marked closed rather than deleted

#### Scenario: Manual paste still works
- **WHEN** a person imports a job by pasting it
- **THEN** the import works as before, alongside sourced jobs
