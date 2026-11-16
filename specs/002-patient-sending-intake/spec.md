# Feature Specification: Haemophilus patient + sending intake (PatientSending)

**Feature Branch**: `speckit-init`

**Created**: 2026-08-26

**Status**: Draft (status-quo inventory)

**Input**: User description: "Feature title: \"Haemophilus patient + sending intake\" (backlog ref A2-H, Group A — Core laboratory workflow). Document the current, as-implemented behavior of the Haemophilus combined Patient-and-Sending intake workflow (PatientSending) as it exists today, where base patient and epidemiological data plus the received material are captured together from the sender's form. Read HaemophilusWeb/Controllers/PatientSendingController.cs, PatientSendingControllerBase.cs, PatientController.cs, the Patient and Sending models and their validators, and the related Views. Cover create/edit/list, validation, linking a sending to a sender and a patient, and roles. Note the shared concept that Sender is shared with the Meningococci database. Document observable behavior only — do not propose changes."

> **Scope note**: This is an *as-is* specification. It documents the behavior of the Haemophilus
> combined Patient-and-Sending intake workflow (`PatientSending`) exactly as implemented on the
> current branch (backlog ref A2-H). It does **not** propose changes. Where the implementation
> deviates from what the backlog prompt assumed, the deviation is recorded factually (see
> Assumptions). The parallel Meningococci intake (`MeningoPatientSending`) shares the same base
> controller and behaves analogously; it is out of scope here except where the two workflows
> share data (the Sender directory).

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Capture a new submission with its patient in one combined form (Priority: P1)

A laboratory user records a newly received laboratory submission ("Einsendung") for the
Haemophilus workflow. From a single "Einsendung erfassen" form they capture, together, the
received-material details (the Sending) and the base patient plus epidemiological/clinical data
(the Patient). The submission is attributed to a sender (the submitting institution) and to a
patient, and on save the system creates the isolate record and assigns its laboratory number so
downstream processing can begin (the stem number is entered later during the isolate work-up, see
spec 004).

**Why this priority**: This combined intake is the entry point of the entire Haemophilus
laboratory workflow — nothing downstream (isolate work-up, reporting, exports) can exist until a
submission and its patient are recorded together. It is the foundational capability of the
feature.

**Independent Test**: Sign in as a standard laboratory user, open "Neue Einsendung erfassen",
select a sender, fill in the required sending and patient fields, and save; confirm the
submission is persisted, appears in the submissions list, and has been assigned a laboratory
number.

**Acceptance Scenarios**:

1. **Given** an authenticated standard user on the submissions list, **When** they choose "Neue
   Einsendung erfassen", **Then** a combined form opens pre-filled with defaults (receiving date
   = today, sampling date = today minus 7 days, sender species = "H. influenzae", Hib vaccination
   status = "not stated").
2. **Given** the combined form, **When** the user selects a sender, enters a sender laboratory
   number and sender species, chooses a sampling location, sets receiving/sampling dates, and
   enters patient initials, gender (and remaining patient data), and saves, **Then** a new patient
   is created, a new submission linked to that patient and the selected sender is persisted, a
   laboratory number is assigned (the stem number stays empty and is listed as "H -"), and the user
   is returned to the submissions list.
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
choice, and that choosing "use existing patient" attaches the new submission to the existing
patient rather than creating a new one.

**Acceptance Scenarios**:

1. **Given** exactly one existing patient matches the entered initials, birth date, and postal
   code, **When** the user submits the intake form, **Then** the save is halted and a warning
   asks the user to choose between "Neuen Patienten anlegen" (create new patient) and "Bestehenden
   Patienten verwenden" (use existing patient) before saving again.
2. **Given** the duplicate-resolution prompt, **When** the user selects "use existing patient" and
   saves, **Then** the submission is attached to the existing patient (the existing patient's data
   is updated from the form) and no duplicate patient is created.
3. **Given** the duplicate-resolution prompt, **When** the user selects "create new patient" and
   saves, **Then** a new patient is created and the submission is linked to it.
4. **Given** two or more existing patients already match the entered initials, birth date, and
   postal code, **When** the user submits the form, **Then** the save is rejected with a message
   that a third patient with the same identifying details cannot be stored.

---

### User Story 3 - Browse, edit, soft-delete, and restore submissions (Priority: P3)

A user browses the list of recorded submissions with search, sorting, and paging, opens a
submission to edit its patient and sending data, removes a submission from active use (soft
delete), and can restore it later. Editing and deleting operate on the same combined
patient-and-sending record.

**Why this priority**: Correcting and curating existing submissions is essential to data quality
but is secondary to the initial capture and duplicate handling.

**Independent Test**: From the submissions list, use search/sort/paging to find a submission, open
it for editing, change a field and save, then delete it and confirm it leaves the active list, and
restore it.

**Acceptance Scenarios**:

1. **Given** recorded submissions, **When** the user opens the submissions list, **Then** a paged,
   sortable, server-side-searchable table is shown with, per submission, the patient initials,
   birth date, stem number, receiving date, sampling location, invasive flag, the laboratory number
   (linking to the isolate), actions to edit and to create a report ("Befund erstellen"), and a
   report-status icon (column "Befund"); patient postal code, sender postal code, and sender
   laboratory number are hidden columns that only the searches use.
2. **Given** a submission, **When** the user opens it for editing, changes patient or sending
   fields, and saves with valid data, **Then** both the patient and the sending changes are
   persisted and the user is returned to the list.
3. **Given** a submission, **When** the user confirms deletion on the delete-confirmation page
   (which shows initials, birth date, stem number, laboratory number, and receiving date), **Then**
   the submission is flagged as deleted, saved without field-level validation, and removed from the
   active submissions list.
4. **Given** a deleted submission, **When** the user restores it, **Then** its deleted flag is
   cleared (again saved without field-level validation) and it returns to the active list. (Today
   the restore is reachable only by the restore address with the internal submission number or by
   saving the submission's edit page by direct address, because the "Gelöschte Einsendungen" page
   lists active rather than deleted submissions; see Assumptions and spec 020.)

---

### Edge Cases

- **Missing required patient fields**: Saving without patient initials, or with initials not in the
  `E.M.` dotted form, or without a gender, fails validation; the combined form is redisplayed and
  nothing is persisted. The browser checks these fields before submitting; the server repeats the
  checks, but its initials check is weaker (see "Initials format on the server").
- **Initials format on the server**: The browser requires the whole value to consist of letter-dot
  pairs. The server only requires one letter followed by a dot anywhere in the value, so initials such
  as "Max M." sent past the browser check are stored (upper-cased, "MAX M."). [SUSPECTED-DEFECT]
  (backlog DATA-030)
- **Missing required sending fields**: Saving without a sender, without a receiving date, without a
  sender laboratory number, or without a sender species fails validation; the form is redisplayed. A
  missing patient link and a missing invasive value cannot occur: the patient link is set by the
  system on save and "Invasiv" is derived from the sampling location. When the sender is missing, the
  server-side redisplay shows only the sender message; the browser shows all messages before
  submitting.
- **Receiving before sampling**: Saving with a receiving date earlier than the sampling date fails
  validation with "Das Eingangsdatum muss nach dem Entnahmedatum liegen".
- **"Other" sampling location without free text**: When the sampling location is "other", the free-text
  "other sampling location" must be provided; otherwise validation fails.
- **Duplicate patient (one match)**: The save is paused and the user must pick a duplicate-resolution
  option before the record is stored.
- **Duplicate patient (two or more matches)**: A third patient with the same identifying details is
  rejected when the form is saved without a duplicate-resolution choice.
- **Duplicate-resolution choice skips the third-patient check**: When a request already carries a
  duplicate-resolution choice (two pages left at the duplicate prompt for the same patient, or a
  crafted request), the two-or-more check is skipped: "Neuen Patienten anlegen" stores a third
  patient with the same initials, birth date, and postal code, and "Bestehenden Patienten verwenden"
  ends in HTTP 500 ("Die Sequenz enthält mehr als ein Element."). [SUSPECTED-DEFECT] (backlog
  DATA-031)
- **Delete/restore bypasses validation**: Toggling the deleted flag saves the submission even if the
  record would otherwise fail normal field validation, so an incomplete legacy record can still be
  deleted or restored.
- **Sender availability**: The sender picker offers active (non-deleted) senders plus the sender
  currently selected on the submission, so an already-attached but now-deleted sender still shows for
  that submission.
- **Laboratory number reassignment**: If the laboratory number assigned on save differs from the one
  implied at entry, a warning with the new laboratory number is surfaced after saving.
- **Missing or unknown submission number**: Opening the edit, delete-confirmation, or restore address
  without a submission number or with an unknown one ends in HTTP 500 (null reference) instead of a
  "bad request" or "not found" answer. [SUSPECTED-DEFECT] (backlog UX-010; restore also DATA-008)
- **Editing a deleted submission restores it**: The edit page of a soft-deleted submission opens by
  direct address; saving it clears the deleted flag, because the form does not carry the flag (see
  spec 020 US3.3). [SUSPECTED-DEFECT] (backlog DATA-007)

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST provide a single combined intake form ("Einsendung erfassen") that
  captures, together, the received-material data (Sending) and the base patient plus
  epidemiological/clinical data (Patient) for the Haemophilus workflow.
- **FR-002**: The system MUST pre-fill the new-intake form with defaults: receiving date = today,
  sampling date = today minus 7 days, sender species = "H. influenzae", and Hib vaccination status =
  "not stated".
- **FR-003**: On creating an intake, the system MUST link the submission to exactly one sender
  (Einsender) and to exactly one patient, and MUST create the submission's isolate with a laboratory
  number as part of the save. The laboratory number is the next sequential number of the year of
  saving, not of the sampling or receiving date (a submission sampled in 2020 and saved in 2026
  receives e.g. "KL001/26"). No stem number is assigned; it stays empty ("H -" in the list) until it is
  entered on the isolate page (spec 004). The code for automatic stem numbers exists but is never
  enabled for either pathogen. [POTENTIAL-REFACTORING] (backlog REF-018)
- **FR-004**: The system MUST offer, for sender selection, the active (non-deleted) senders plus the
  sender already attached to the submission being edited.
- **FR-005**: The sender directory used for selection MUST be the single directory shared with the
  Meningococci database; a sender is the same record whether referenced from a Haemophilus or a
  Meningococci submission.
- **FR-006**: The system MUST validate the Sending on save, requiring a sender ("'Einsendernummer'
  darf kein Nullwert sein."), a receiving date ("'Eingangsdatum' darf kein Nullwert sein."), a sender
  laboratory number ("'Labnr. Einsender' darf nicht leer sein."), and a sender species ("'Spezies
  Einsender' darf nicht leer sein."); requiring the receiving date to be on or after the sampling date
  (message "Das Eingangsdatum muss nach dem Entnahmedatum liegen"); and requiring the free-text
  "Anderer Entnahmeort" when the sampling location is "Anderer nicht invasiv" or "Anderer invasiv"
  ("Anderer Entnahmeort darf nicht leer sein."). The rule set also names the patient link and the
  invasive value, but neither can fail: the patient link is set by the system on save and "Invasiv" is
  derived from the sampling location ("Ja" for "Blut", "Liquor", "Anderer invasiv").
- **FR-007**: The system MUST validate the Patient on save, requiring initials ("'Initialen' darf
  nicht leer sein."), requiring the initials to match the dotted form (e.g. "E.M."; message "Die
  Initialen müssen in der Form 'E.M.' eingegeben werden."), and requiring a gender ("'Geschlecht'
  darf nicht leer sein."). The browser enforces the dotted form for the whole value; the server only
  checks that the value contains a letter followed by a dot (see Edge Cases, "Initials format on the
  server"). [SUSPECTED-DEFECT] (backlog DATA-030)
- **FR-008**: When required fields are missing or invalid, the system MUST reject the save, persist
  nothing, and redisplay the combined form with validation messages.
- **FR-009**: On intake, the system MUST detect an existing patient with the same initials, birth
  date, and postal code; when exactly one match exists it MUST pause the save and require the user to
  choose to create a new patient or to use the existing patient before saving.
- **FR-010**: When the user chooses "use existing patient", the system MUST attach the submission to
  the existing patient and update that patient from the entered form data rather than creating a
  duplicate; when the user chooses "create new patient", it MUST create a new patient.
- **FR-011**: When two or more patients already match the entered initials, birth date, and postal
  code, the system MUST reject the save with the message "Es existieren bereits zwei Patienten mit
  den selben Initialen, Geburtsdatum und Postleitzahl. Ein dritter Patient mit den selben
  Eigenschaften kann leider nicht gespeichert werden." The check runs only when the request carries
  no duplicate-resolution choice; with a choice it is skipped (see Edge Cases). [SUSPECTED-DEFECT]
  (backlog DATA-031)
- **FR-012**: After a successful save where the assigned laboratory number differs from the one
  implied at entry, the system MUST warn the user and include the new laboratory number.
- **FR-013**: The system MUST provide a submissions list ("Einsendungen") showing, per submission,
  patient initials, birth date, stem number, receiving date, sampling location, invasive flag, the
  laboratory number ("Labor-Nr", linking to the isolate), and a report-status icon (column "Befund",
  e.g. "Kein Befund erstellt"), with server-side paging (8 rows per page), sorting (default: "Labor-Nr"
  descending; the browser remembers the last sort and search), and full-text search. Patient postal
  code, sender postal code, and sender laboratory number are loaded as hidden columns: the full-text
  search covers them, and "Erweiterte Suche" replaces the search box with fields for "Initialen",
  "Geburtsdatum", "Patient PLZ", "Einsender Labornummer", and "Einsender PLZ".
- **FR-014**: Each submissions-list row MUST offer an action to edit the submission and an action to
  create its report ("Befund erstellen").
- **FR-015**: The system MUST let an authorized user edit an existing submission's patient and sending
  fields together and persist both on a valid save, returning to the list.
- **FR-016**: The system MUST support removal of a submission as a soft delete (a recoverable
  "deleted" flag); deleted submissions MUST NOT appear in the active submissions list.
- **FR-017**: The delete-confirmation view MUST show the submission's initials, birth date, stem
  number, laboratory number, and receiving date before the user confirms.
- **FR-018**: The system MUST allow the deleted flag to be toggled (delete and restore) without
  enforcing the normal field-level validation on the submission.
- **FR-019**: The system MUST provide a patient-merge action that reassigns all submissions from one
  patient to another and removes the emptied patient, after showing a confirmation of both patients
  and their submissions.
- **FR-020**: All intake actions (create, edit, list/data, delete, restore, merge) MUST require the
  user to hold the standard laboratory-user role ("Standardbenutzer"); the RKI export additionally
  permits the public-health role ("RKI"). Unauthenticated or unauthorized requests MUST be denied.
- **FR-021**: Create and edit bind every patient and sending property from the posted form; there is
  no list of intended fields. A request can therefore set values the form does not offer, e.g.
  "Sending.Deleted=true" stores a new submission as deleted, and on edit the posted patient and
  submission numbers decide which records are overwritten (from code, not exercised).
  [SUSPECTED-DEFECT] (backlog SEC-013) The
  combined form carries anti-forgery tokens, but the create and edit saves do not verify them; a save
  without token is stored (current behavior; see spec 021). [SUSPECTED-DEFECT] (backlog SEC-005)

### Key Entities *(include if feature involves data)*

- **Patient**: The person a submission concerns, plus base epidemiological/clinical data. Key
  attributes: identifier, initials (required, stored upper-cased, dotted form), birth date, postal
  code, gender (required), city, county, country (defaults to a standard country), federal state, and
  Haemophilus-specific clinical data (clinical information flags, other clinical information, Hib
  vaccination status and date). Identified for duplicate detection by initials + birth date + postal
  code.
- **Sending ("Einsendung" / submission)**: A received laboratory submission attributed to exactly one
  sender and one patient. Key attributes: identifier, sender reference, patient reference, sampling
  date, receiving date, sampling location (and free-text "other" when applicable), material, invasive
  flag (derived from sampling location), sender laboratory number, sender species, sender serotype,
  DEMIS id, remark, and a "deleted" soft-delete flag. Owns an Isolate carrying the assigned stem
  number and laboratory number.
- **Sender ("Einsender")**: The submitting institution a submission is attributed to. Shared across
  both the Haemophilus and the Meningococci databases; selectable when it is active or already
  attached to the submission. Only referenced here for linking and selection.
- **Isolate**: The laboratory work-up record created for a submission on save; carries the year (of
  saving), yearly sequential isolate number, the resulting laboratory number, and the stem number
  (empty after intake). Referenced here only insofar as intake creates it and assigns its laboratory
  number.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: An authorized user can record a new valid submission together with its patient in one
  form and, on save, see it in the submissions list with an assigned laboratory number, without a
  second data-entry step (the stem number follows during the isolate work-up).
- **SC-002**: When intake would create a patient that duplicates an existing one (same initials,
  birth date, and postal code), the user is always prompted to reuse or create before any record is
  stored, and choosing to reuse never creates a duplicate patient.
- **SC-003**: A third patient with identical initials, birth date, and postal code is not stored when
  the form is saved without a duplicate-resolution choice (with a choice it is, see FR-011).
- **SC-004**: Every submission is attributed to exactly one sender and one patient, and the sender
  chosen is the same shared record available to the Meningococci workflow.
- **SC-005**: Attempting to save an intake with any missing/invalid required field (patient initials
  format, gender, sender, receiving date, sender laboratory number, sender species, or a receiving
  date before the sampling date) never persists the record and always returns an actionable
  validation message (the server accepts initials the browser would reject, see FR-007).
- **SC-006**: 100% of deletions are recoverable: any soft-deleted submission can be restored to the
  active list (currently only by direct address, see spec 020), and delete/restore succeeds even for
  records that would otherwise fail field validation.

## Assumptions

- **Naming deviation ("Deleted" action)**: The controller action that renders the "deleted"
  submissions view queries *non-deleted* submissions rather than deleted ones. This is documented as
  observed behavior; it is recorded factually and not treated as a proposed change. Its consequence
  (no page lists deleted submissions; restore only by direct address) is specified in spec 020.
  [SUSPECTED-DEFECT]
- **Exports and patient-merge are out of scope here**: Besides the intake create/edit/list/delete
  actions, the same controller also hosts three spreadsheet exports (Laboratory, RKI, PubMLST) and a
  patient-merge flow. This spec documents those only where they touch intake — the shared roles and
  the merge action's effect on submissions — and leaves their internal behavior to be inventoried
  separately.
- **Standard role names**: "Standardbenutzer" is the standard laboratory-user role and "RKI" the
  public-health role, per the application's default role set.
- **Meningococci parity**: The parallel `MeningoPatientSending` workflow shares the same base
  controller and behaves analogously; only the shared Sender directory is in scope here.
- **Identifiers assigned at save**: The laboratory number is generated by the system during the
  create save, not entered by the user; the create form shows the number expected at that moment.
  The stem number is not generated at intake (FR-003).
