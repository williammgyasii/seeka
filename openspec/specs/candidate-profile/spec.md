# candidate-profile Specification

## Purpose
Build the candidate's full profile from the uploaded résumé and confirm it
step by step, with seniority, timezone, target job titles, and pay chosen from
fixed lists, so fit checks and outreach drafts work from clean, complete facts.

## Requirements

### Requirement: Résumé fills every profile section

Résumé extraction SHALL report work experience with highlights, education,
certifications, languages with a level from a fixed list, links, projects,
and volunteering, in addition to the existing facts. The draft SHALL keep
only what the résumé states, drop items missing a field the step requires
(such as a job's company or title, or a school), blank a date that is not a valid `YYYY-MM` month, and keep only `https` links.

#### Scenario: Full résumé pre-fills every step
- **WHEN** a user uploads a résumé listing two jobs, a degree, a certification, two languages, and a LinkedIn link
- **THEN** each onboarding step opens pre-filled with those items

#### Scenario: Unclear date keeps the job
- **WHEN** the model returns a job whose start month is "Spring 2021"
- **THEN** the job is kept with its start month blank

#### Scenario: Item without a name is dropped
- **WHEN** the model returns an education entry with no school
- **THEN** the draft leaves that entry out

### Requirement: Onboarding saves step by step

Onboarding SHALL be grouped steps (about, experience, education, projects,
preferences) after the résumé upload, with a progress bar. Each step SHALL
save on its own and be marked finished, including when saved empty with
Skip. Reopening onboarding SHALL resume at the first unfinished step. The
résumé draft SHALL pre-fill only steps the user has never saved.

#### Scenario: Saving a step marks it finished
- **WHEN** a user saves the experience step
- **THEN** the experience is stored and the step is listed as finished

#### Scenario: Skip saves an empty step
- **WHEN** a user with no projects clicks Skip on the projects step
- **THEN** the step is finished with no projects or volunteering

#### Scenario: Reload resumes where the user left
- **WHEN** a user who finished about and experience reloads onboarding
- **THEN** the education step opens

#### Scenario: New upload keeps saved edits
- **WHEN** a user who saved the experience step uploads a different résumé
- **THEN** the experience step still shows what the user saved

#### Scenario: Retried save
- **WHEN** the same step is saved twice with the same data
- **THEN** the stored profile is the same as after one save

### Requirement: Steps fit the screen and long lists collapse

Onboarding SHALL fit the viewport height: the progress bar, step title, and
step buttons SHALL stay visible while only the fields scroll. Jobs, schools,
projects, and volunteering SHALL show as collapsed rows summarised by role,
place, and dates. An added item SHALL open at once, and an item the server
rejects SHALL open and be marked until it is fixed. Short lists (languages,
links, certifications) SHALL stay open.

#### Scenario: Jobs start collapsed
- **WHEN** the experience step opens with two jobs from the résumé
- **THEN** each job shows as a closed row reading its title, company, and dates

#### Scenario: Rejected job opens
- **WHEN** the server rejects the end month of a closed job
- **THEN** that job opens, marked as needing a fix, with the error under its end month

#### Scenario: Save stays in reach
- **WHEN** a step's fields are taller than the screen
- **THEN** the fields scroll and the Save button stays visible

### Requirement: Each step is validated by field

The server SHALL reject a step with a 400 that names each invalid field by
path, such as `experience[1].endMonth`. It SHALL reject an end before its
start, an end month on a current role, a link that is not `https`, a month
not in `YYYY-MM` form, and more items or longer text than the shared limits.
The about step SHALL require name, residence, and timezone.

#### Scenario: End before start
- **WHEN** a user saves a job that ends in 2020-01 and starts in 2021-06
- **THEN** the response is 400 with an error for `experience[0].endMonth`

#### Scenario: Current role with an end date
- **WHEN** a user saves a current role that also has an end month
- **THEN** the response is 400 with an error for that role's `endMonth`

#### Scenario: Insecure link
- **WHEN** a user saves a portfolio link starting with `http://`
- **THEN** the response is 400 with an error for that link's `url`

### Requirement: Seniority is a level from a fixed list

Seniority SHALL be one of Entry, Junior, Mid-level, Senior, Lead, Manager,
Director, or Executive, or blank. The résumé reader, the About you form, and
the server SHALL all use this list. A profile saved earlier with free text
SHALL read as the highest level its text names, or blank when it names none.
The résumé summary SHALL use no pronouns.

#### Scenario: Off-list value
- **WHEN** a user saves About you with seniority "People Operations Specialist"
- **THEN** the response is 400 with "Choose a level from the list." at `seniority`

#### Scenario: Earlier free text
- **WHEN** a profile saved before this change has seniority "Senior HR Business Partner"
- **THEN** it reads as "Senior"

#### Scenario: Résumé names no level
- **WHEN** the résumé reader reports a seniority that is not on the list
- **THEN** the draft leaves seniority blank

### Requirement: Timezone follows the country of residence

When a user picks a country of residence, the About you form SHALL offer only
that country's timezones. A country with one timezone SHALL set it and lock
the field. A country with several SHALL keep the device timezone when it is
one of them, and otherwise ask. The server SHALL reject a timezone that is not
a real IANA timezone name.

#### Scenario: One-zone country
- **WHEN** a user picks Ghana
- **THEN** the timezone is Africa/Accra and cannot be changed

#### Scenario: Several zones
- **WHEN** a user on a device set to America/Chicago picks United States
- **THEN** the timezone offers only United States zones, with America/Chicago chosen

#### Scenario: Made-up timezone
- **WHEN** a user saves About you with timezone "Mars/Olympus"
- **THEN** the response is 400 with an error at `timeZone`

### Requirement: Target job titles come from ESCO

Target job titles SHALL come from the ESCO occupation list. A search SHALL
match an occupation's preferred or alternative English labels and return its
preferred label. The server SHALL serve the search to signed-in users only,
and SHALL reject a title that is not an ESCO label. A title saved as an
alternative label SHALL be stored as its preferred label.

#### Scenario: Search by a common name
- **WHEN** a signed-in user searches "hr manager"
- **THEN** the results include "human resources manager"

#### Scenario: Made-up title
- **WHEN** a user saves preferences with the title "Chief Vibes Officer"
- **THEN** the response is 400 with an error at `targetJobTitles[0]`

#### Scenario: Not signed in
- **WHEN** a request without a session searches job titles
- **THEN** the response is 401

### Requirement: Preferences are chosen from lists

Employment types SHALL be chosen from Full-time, Part-time, Contract,
Freelance, Internship, and Temporary. The server SHALL reject values outside
this list. Preferences SHALL NOT ask for working hours.

#### Scenario: Unknown employment type
- **WHEN** a user saves preferences with the employment type "Gig"
- **THEN** the response is 400 with an error at `employmentTypes[0]`

### Requirement: Pay is a monthly band in a chosen currency

Pay SHALL be optional and chosen from six monthly bands that Seeka defines
for GHS, NGN, KES, ZAR, USD, GBP, EUR, and CAD. A band is stored as its
minimum and maximum; the top band has no maximum. The currency SHALL default
to the country of residence's currency, or USD when it has no bands, and the
user MAY change it. The server SHALL reject any other amount or currency.

#### Scenario: Default currency
- **WHEN** a user who lives in Ghana opens preferences with no currency saved
- **THEN** the currency is GHS and the pay choices are the GHS bands

#### Scenario: Country without bands
- **WHEN** a user who lives in Uganda opens preferences
- **THEN** the currency is USD

#### Scenario: Top band
- **WHEN** a user chooses "GHS 40,000+"
- **THEN** the minimum 40000 is stored with no maximum

#### Scenario: Amount that is not a band
- **WHEN** a user saves GHS 5000 to 9000
- **THEN** the response is 400 with an error at `minimumCompensation`

### Requirement: Years of experience come from job dates

The profile SHALL suggest total years of experience from the work experience
dates, counting overlapping periods once, running current roles to today, and
rounding down. The user MAY save a different number.

#### Scenario: Overlapping jobs
- **WHEN** one job runs 2018-01 to 2021-01 and another 2020-01 to 2022-01
- **THEN** the suggestion is 4 years

#### Scenario: Current role
- **WHEN** the only job started 2023-01 and is current, and today is 2026-10-25
- **THEN** the suggestion is 3 years

#### Scenario: Undated jobs
- **WHEN** no job has a start month
- **THEN** there is no suggestion

### Requirement: Onboarding finishes explicitly

Onboarding SHALL be complete only after the user finishes it, which the
server SHALL allow once the about and preferences steps are saved, and
otherwise refuse with a 400 naming the missing steps. The session SHALL
report onboarding complete from that moment. Profiles completed before this
change SHALL stay complete.

#### Scenario: Finish with required steps saved
- **WHEN** a user who saved about and preferences clicks Finish
- **THEN** onboarding is complete and the app opens

#### Scenario: Finish too early
- **WHEN** a user who saved only about tries to finish
- **THEN** the response is 400 naming `preferences`, and onboarding stays open

#### Scenario: Saving a step does not finish onboarding
- **WHEN** a new user saves the about step
- **THEN** the session still reports onboarding incomplete

### Requirement: The profile is private to its workspace

Profile reads and step saves SHALL apply only to the signed-in user's own
workspace, and SHALL require a verified email like every workspace endpoint.

#### Scenario: Another user's profile
- **WHEN** two users save profiles
- **THEN** each reads back only their own profile
