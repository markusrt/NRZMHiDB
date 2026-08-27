# Feature Specification: Meningococci patient + sending intake (MeningoPatientSending)

**Feature Branch**: `003-meningo-patient-sending`

**Created**: 2026-08-27

**Status**: Draft (status-quo inventory)

**Input**: User description: "Feature title: \"Meningococci patient + sending intake\" (backlog ref A2-M, Group A — Core laboratory workflow). Document the current, as-implemented behavior of the Meningococci combined Patient-and-Sending intake workflow (MeningoPatientSending) as it exists today, where base patient and epidemiological data plus the received material are captured together from the sender's form. Read HaemophilusWeb/Controllers/MeningoPatientSendingController.cs, PatientSendingControllerBase.cs, MeningoPatientController.cs, the MeningoPatient and MeningoSending models and their validators, and the related Views. Cover create/edit/list, validation, linking a sending to a sender and a patient, and roles. Note the shared concept that Sender is shared with the Haemophilus database. Document observable behavior only — do not propose changes."

> **Scope note**: This is an *as-is* specification. It documents the behavior of the Meningococci
> combined Patient-and-Sending intake workflow (`MeningoPatientSending`) exactly as implemented on
> the current branch (backlog ref A2-M). It does **not** propose changes. The workflow shares its
> base controller (`PatientSendingControllerBase`) with the parallel Haemophilus intake
> (`PatientSending`); shared behavior is documented here as it applies to the Meningococci
> workflow, and Meningococci-specific differences are called out. Where the implementation deviates
> from what the backlog prompt assumed, the deviation is recorded factually (see Assumptions). The
> two workflows share one data concept — the Sender directory — which is in scope here only insofar
> as it is shared.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Capture a new submission with its patient in one combined form (Priority: P1)

A laboratory user records a newly received laboratory submission ("Einsendung") for the
Meningococci workflow. From a single "Einsendung erfassen" form they capture, together, the
received-material details (the Sending) and the base patient plus epidemiological/clinical data
(the Patient). The submission is attributed to a sender (the submitting institution) and to a
patient, and on save the system assigns the laboratory identifiers (stem number and laboratory
number) so downstream processing can begin.

**Why this priority**: This combined intake is the entry point of the entire Meningococci
laboratory workflow — nothing downstream (isolate work-up, reporting, exports) can exist until a
submission and its patient are recorded together. It is the foundational capability of the feature.

**Independent Test**: Sign in as a standard laboratory user, open "Einsendung erfassen", select a
sender, fill in the required sending and patient fields, and save; confirm the submission is
persisted, appears in the submissions list, and has been assigned a stem number and laboratory
number.

**Acceptance Scenarios**:

1. **Given** an authenticated standard user on the submissions list, **When** they choose to record
   a new submission, **Then** a combined form opens pre-filled with defaults (receiving date =
   today, sampling date = today minus 7 days, sampling location = "Blut", sender species =
   "N. meningitidis").
2. **Given** the combined form, **When** the user selects a sender, enters a sender laboratory
   number and sender species, chooses a sampling location, sets receiving/sampling dates, and enters
   patient initials, gender (and remaining patient data), and saves, **Then** a new patient is
   created, a new submission linked to that patient and the selected sender is persisted, a stem
   number and laboratory number are assigned, and the user is returned to the submissions list.
3. **Given** a saved submission whose assigned laboratory number differs from the one implied at
   entry, **When** the save completes, **Then** the user is shown a warning stating the laboratory
   number changed, including the new laboratory number.
4. **Given** the combined form, **When** the user saves with any required field missing or invalid,
   **Then** nothing is persisted and the form is redisplayed with validation messages.

---

### User Story 2 - Detect and resolve a duplicate patient during intake (Priority: P2)

While recording a new submission, the system checks whether a patient with the same identifying
details already exists (same initials, birth date, and postal code). If exactly one match exists,
the user is asked whether to create a new patient or attach the submission to the existing one. If
two or more matches already exist, the system refuses to add a third such patient.

**Why this priority**: Duplicate patients corrupt case counts and epidemiological reporting.
Catching duplicates at intake keeps patient records clean, but it is secondary to being able to
record a submission at all.

**Independent Test**: Record a submission for a patient, then start a second submission with the
same initials, birth date, and postal code; confirm the system prompts for a duplicate-resolution
choice, and that choosing "use existing patient" attaches the new submission to the existing patient
rather than creating a new one.

**Acceptance Scenarios**:

1. **Given** exactly one existing patient matches the entered initials, birth date, and postal code,
   **When** the user submits the intake form, **Then** the save is halted and a warning asks the
   user to choose between creating a new patient and using the existing patient before saving again.
2. **Given** the duplicate-resolution prompt, **When** the user selects "use existing patient" and
   saves, **Then** the submission is attached to the existing patient (the existing patient's data
   is updated from the form) and no duplicate patient is created.
3. **Given** the duplicate-resolution prompt, **When** the user selects "create new patient" and
   saves, **Then** a new patient is created and the submission is linked to it.
4. **Given** two or more existing patients already match the entered initials, birth date, and postal
   code, **When** the user submits the form, **Then** the save is rejected with a message that a
   third patient with the same identifying details cannot be stored.

---

### User Story 3 - Browse, edit, soft-delete, and restore submissions (Priority: P3)

A user browses the list of recorded submissions with search, sorting, and paging, opens a submission
to edit its patient and sending data, removes a submission from active use (soft delete), and can
restore it later. Editing and deleting operate on the same combined patient-and-sending record.

**Why this priority**: Correcting and curating existing submissions is essential to data quality but
is secondary to the initial capture and duplicate handling.

**Independent Test**: From the submissions list, use search/sort/paging to find a submission, open it
for editing, change a field and save, then delete it and confirm it leaves the active list, and
restore it.

**Acceptance Scenarios**:

1. **Given** recorded submissions, **When** the user opens the submissions list, **Then** a paged,
   sortable, server-side-searchable table is shown with, per submission, the patient initials, birth
   date, stem number, receiving date, sampling location, invasive flag, a report-status icon, the
   laboratory number (linking to the isolate), patient postal code, sender postal code, and sender
   laboratory number, plus actions to edit and to create a report ("Befund erstellen").
2. **Given** a submission, **When** the user opens it for editing, changes patient or sending fields,
   and saves with valid data, **Then** both the patient and the sending changes are persisted and the
   user is returned to the list.
3. **Given** a submission, **When** the user confirms deletion on the delete-confirmation page,
   **Then** the submission is flagged as deleted, saved without field-level validation, and removed
   from the active submissions list.
4. **Given** a deleted submission, **When** the user restores it, **Then** its deleted flag is
   cleared (again saved without field-level validation) and it returns to the active list.

---

### Edge Cases

- **Missing required patient fields**: Saving without patient initials, or with initials not in the
  `E.M.` dotted form, or without a gender, fails validation; the combined form is redisplayed and
  nothing is persisted.
- **Birth date too early**: A birth date on or before 1 January 1800 fails patient validation.
- **Country/federal-state consistency**: If the patient's federal state is "Ausland" (overseas), the
  country must not be the default country (Germany); conversely, if the state is any value other than
  "Ausland", the country must be the default country. Either mismatch fails validation.
- **Missing required sending fields**: Saving without a sender, without a patient link, without a
  receiving date, without a sender laboratory number, or without a sender species, fails validation;
  the form is redisplayed.
- **Receiving before sampling**: Saving with a receiving date earlier than the sampling date fails
  validation with "Das Eingangsdatum muss nach dem Entnahmedatum liegen".
- **Sampling before birth**: Saving with a sampling date earlier than the patient's birth date fails
  validation with "Das Entnahmedatum muss nach dem Geburtsdatum des Patienten liegen".
- **"Other invasive" sampling location without free text**: When the sampling location is "Anderer
  invasiv", the free-text "other invasive sampling location" must be provided; otherwise validation
  fails.
- **"Other non-invasive" sampling location without free text**: When the sampling location is "Anderer
  nicht invasiv", the free-text "other non-invasive sampling location" must be provided; otherwise
  validation fails.
- **Duplicate patient (one match)**: The save is paused and the user must pick a duplicate-resolution
  option before the record is stored.
- **Duplicate patient (two or more matches)**: A third patient with the same identifying details is
  rejected and cannot be stored.
- **Delete/restore bypasses validation**: Toggling the deleted flag saves the submission even if the
  record would otherwise fail normal field validation, so an incomplete legacy record can still be
  deleted or restored.
- **Laboratory number reassignment**: If the laboratory number assigned on save differs from the one
  implied at entry, a warning with the new laboratory number is surfaced after saving.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST provide a single combined intake form ("Einsendung erfassen") that
  captures, together, the received-material data (Sending) and the base patient plus
  epidemiological/clinical data (Patient) for the Meningococci workflow.
- **FR-002**: The system MUST pre-fill the new-intake form with defaults: receiving date = today,
  sampling date = today minus 7 days, sampling location = "Blut" (Blood), and sender species =
  "N. meningitidis".
- **FR-003**: On creating an intake, the system MUST link the submission to exactly one sender
  (Einsender) and to exactly one patient, and MUST assign the submission a stem number and a
  laboratory number as part of the save.
- **FR-004**: The system MUST offer, for sender selection, the active (non-deleted) senders plus the
  sender already attached to the submission being edited.
- **FR-005**: The sender directory used for selection MUST be the single directory shared with the
  Haemophilus database; a sender is the same record whether referenced from a Meningococci or a
  Haemophilus submission.
- **FR-006**: The system MUST validate the Sending on save, requiring a sender, a patient link, a
  receiving date, a sender laboratory number, and a sender species; requiring the receiving date to
  be on or after the sampling date (message "Das Eingangsdatum muss nach dem Entnahmedatum liegen");
  requiring the free-text "other invasive sampling location" when the sampling location is "Anderer
  invasiv"; and requiring the free-text "other non-invasive sampling location" when the sampling
  location is "Anderer nicht invasiv".
- **FR-007**: The system MUST validate the Patient on save, requiring initials, requiring the initials
  to match the dotted form (e.g. "E.M."), requiring a birth date after 1 January 1800, requiring a
  gender, and enforcing consistency between the federal state and the country (state "Ausland"
  requires a non-default country; any other state requires the default country, Germany).
- **FR-008**: On intake save the system MUST additionally reject a submission whose sampling date is
  earlier than the patient's birth date, with the message "Das Entnahmedatum muss nach dem
  Geburtsdatum des Patienten liegen".
- **FR-009**: When required fields are missing or invalid, the system MUST reject the save, persist
  nothing, and redisplay the combined form with validation messages.
- **FR-010**: On intake, the system MUST detect an existing patient with the same initials, birth
  date, and postal code; when exactly one match exists it MUST pause the save and require the user to
  choose to create a new patient or to use the existing patient before saving.
- **FR-011**: When the user chooses "use existing patient", the system MUST attach the submission to
  the existing patient and update that patient from the entered form data rather than creating a
  duplicate; when the user chooses "create new patient", it MUST create a new patient.
- **FR-012**: When two or more patients already match the entered initials, birth date, and postal
  code, the system MUST reject the save with a message that a third such patient cannot be stored.
- **FR-013**: After a successful save where the assigned laboratory number differs from the one
  implied at entry, the system MUST warn the user and include the new laboratory number.
- **FR-014**: The system MUST derive the "invasive" flag of a submission from its sampling location
  (locations marked invasive yield "Ja", all others "Nein") rather than accepting it as direct input.
- **FR-015**: The system MUST provide a submissions list showing, per submission, patient initials,
  birth date, stem number, receiving date, sampling location, invasive flag, a report-status icon,
  the laboratory number (linking to the isolate), patient postal code, sender postal code, and sender
  laboratory number, with server-side paging, sorting, full-text search, and per-column search.
- **FR-016**: Each submissions-list row MUST offer an action to edit the submission and an action to
  create its report ("Befund erstellen"), the latter opening the Meningococci report workflow.
- **FR-017**: The system MUST let an authorized user edit an existing submission's patient and sending
  fields together and persist both on a valid save, returning to the list.
- **FR-018**: The system MUST support removal of a submission as a soft delete (a recoverable
  "deleted" flag); deleted submissions MUST NOT appear in the active submissions list.
- **FR-019**: The system MUST allow the deleted flag to be toggled (delete and restore) without
  enforcing the normal field-level validation on the submission.
- **FR-020**: The system MUST provide a patient-merge action that reassigns all submissions from one
  patient to another and removes the emptied patient, after showing a confirmation of both patients
  and their submissions.
- **FR-021**: The system MUST provide spreadsheet exports over the recorded submissions — a Laboratory
  export, an RKI export, an IRIS export, and a State Authority (LGA) export — with the IRIS and RKI
  exports restricted to invasive submissions in which meningococci were found.
- **FR-022**: All intake actions (create, edit, list/data, delete, restore, merge) MUST require the
  user to hold the standard laboratory-user role ("Standardbenutzer"); the RKI export and the State
  Authority (LGA) export additionally permit the public-health role ("RKI"); the IRIS and Laboratory
  exports require the standard laboratory-user role. Unauthenticated or unauthorized requests MUST be
  denied.
- **FR-023**: The system MUST protect create and edit against over-posting by binding only the
  intended patient and sending fields from the combined form (e.g. anti-forgery tokens on the
  sub-forms).

### Key Entities *(include if feature involves data)*

- **MeningoPatient**: The person a submission concerns, plus base epidemiological/clinical data. Key
  attributes: identifier, initials (required, dotted form), birth date, postal code, gender
  (required), city, county, country (must be consistent with federal state), federal state, and
  Meningococci-specific clinical data (clinical-information flags and other free-text clinical
  information, risk-factor flags and other free-text risk factor). Identified for duplicate detection
  by initials + birth date + postal code.
- **MeningoSending ("Einsendung" / submission)**: A received laboratory submission attributed to
  exactly one sender and one patient. Key attributes: identifier, sender reference, patient reference,
  sampling date, receiving date, sampling location (with two free-text "other" variants — invasive and
  non-invasive), material, sender-reported serogroup, derived invasive flag, sender laboratory number,
  sender species, and a "deleted" soft-delete flag. Owns a MeningoIsolate carrying the assigned stem
  number and laboratory number.
- **Sender ("Einsender")**: The submitting institution a submission is attributed to. Shared across
  both the Haemophilus and the Meningococci databases; selectable when it is active or already
  attached to the submission. Only referenced here for linking and selection.
- **MeningoIsolate**: The laboratory work-up record created for a submission on save; carries the
  year, yearly sequential isolate number, stem number, and the resulting laboratory number. Referenced
  here only insofar as intake creates it and assigns its identifiers.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: An authorized user can record a new valid submission together with its patient in one
  form and, on save, see it in the submissions list with an assigned stem number and laboratory
  number, without a second data-entry step.
- **SC-002**: When intake would create a patient that duplicates an existing one (same initials, birth
  date, and postal code), the user is always prompted to reuse or create before any record is stored,
  and choosing to reuse never creates a duplicate patient.
- **SC-003**: A third patient with identical initials, birth date, and postal code is never stored.
- **SC-004**: Every submission is attributed to exactly one sender and one patient, and the sender
  chosen is the same shared record available to the Haemophilus workflow.
- **SC-005**: Attempting to save an intake with any missing/invalid required field (patient initials
  format, birth date, gender, state/country consistency, sender, receiving date, sender laboratory
  number, sender species, a receiving date before the sampling date, a sampling date before the birth
  date, or a missing free-text "other" sampling location) never persists the record and always returns
  an actionable validation message.
- **SC-006**: 100% of deletions are recoverable: any soft-deleted submission can be restored to the
  active list, and delete/restore succeeds even for records that would otherwise fail field validation.

## Assumptions

- **Naming deviation ("Deleted" action)**: The controller action that renders the "deleted"
  submissions view queries *non-deleted* submissions rather than deleted ones (inherited from the
  shared base controller). This is documented as observed behavior; it is recorded factually and not
  treated as a proposed change. [SUSPECTED-DEFECT]
- **Exports and patient-merge are out of scope here**: Besides the intake create/edit/list/delete
  actions, the same controller also hosts spreadsheet exports (Laboratory, RKI, IRIS, State
  Authority/LGA) and a patient-merge flow. This spec documents those only where they touch intake —
  the shared roles, the invasive/meningococci filtering, and the merge action's effect on submissions
  — and leaves their internal export layouts to be inventoried separately.
- **Standard role names**: "Standardbenutzer" is the standard laboratory-user role and "RKI" the
  public-health role, per the application's default role set.
- **Haemophilus parity**: The parallel `PatientSending` workflow shares the same base controller and
  behaves analogously; only the shared Sender directory is in scope here.
- **Identifiers assigned at save**: Stem number and laboratory number are generated by the system
  during the create save, not entered by the user.
- **Invasive flag is derived, not entered**: The invasive value is computed from the selected sampling
  location (via an "invasive location" marking on each location value), so it is a derived attribute
  rather than a user-entered field.
