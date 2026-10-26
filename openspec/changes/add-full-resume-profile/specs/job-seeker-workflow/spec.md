## MODIFIED Requirements

### Requirement: Candidate profile and preferences

The system SHALL store an editable candidate profile and job preferences,
including residence, IANA timezone, target roles, seniority, skills,
experience summary, employment types, and an optional monthly pay band with
its currency. Profile setup SHALL NOT pre-fill a fixed country or timezone;
it SHALL suggest the timezone and residence from the user's device, and the
user SHALL choose residence from a list of English country names.

#### Scenario: Ghanaian candidate saves preferences
- **WHEN** a user saves Ghana as residence and Africa/Accra as timezone with valid job preferences
- **THEN** the system persists the values in the user's active workspace and returns them for review

#### Scenario: Setup suggests residence and timezone from the device
- **WHEN** a new user opens profile setup on a device whose timezone is `Europe/Lisbon`
- **THEN** the timezone field shows `Europe/Lisbon` and residence shows Portugal, and both remain editable before saving

#### Scenario: Shared timezone suggests its main country
- **WHEN** a new user's device timezone is `Africa/Lagos`, which several countries share
- **THEN** residence suggests Nigeria, the timezone's main country, and stays editable

#### Scenario: Timezone without a country leaves residence empty
- **WHEN** the device timezone belongs to no country, such as `UTC`
- **THEN** residence starts empty and the user must choose one before saving

#### Scenario: Residence comes from the country list
- **WHEN** a user picks a residence in profile setup
- **THEN** the saved value is the English country name shown in the list, such as `United Kingdom`, not free text
