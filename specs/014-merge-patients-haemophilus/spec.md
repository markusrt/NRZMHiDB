# Feature Specification: Merge patients (Haemophilus)

**Feature Branch**: `speckit-init`

**Created**: 2026-10-22

**Status**: Draft (status-quo inventory)

**Input**: User description: "Feature title: \"Merge patients (Haemophilus)\" (backlog ref C1-H, Group C — Reference data & administration). Document the current, as-implemented behavior of merging two Haemophilus patient records as it exists today. Read the MergePatient / MergePatients logic in HaemophilusWeb/Controllers/PatientSendingController.cs (reassigning sendings from the deleted patient to the kept patient, then removing the deleted patient) and PatientSendingControllerBase.cs. Capture the user flow, what is reassigned, what is deleted, and roles. Document observable behavior only — do not propose changes."

> **Scope note**: This is an *as-is* specification (backlog ref C1-H). It records the current, observable behavior of
> the Haemophilus "Patienten Zusammenfügen" (merge patients) function: entering two Haemophilus patient numbers,
> the lookup and validation, the confirmation page, the choice of the patient to keep, moving the submissions of the
> other patient to the kept patient, deleting the other patient record, and the success page. It does **not**
> propose changes. The request/confirmation/success flow is implemented once on the shared patient-and-submission
> controller base (`PatientSendingControllerBase`) and is used by both pathogen databases; only the per-pathogen
> lookup and the reassign-and-delete step are pathogen specific. The Meningococci variant (backlog ref C1-M) is
> structurally identical and is specified separately; it is in scope here only where the shared base or copied
> Meningococci links surface in the Haemophilus pages. Out of scope: submission intake, editing, soft-delete and
> restore (spec 002, A2-H), the intake duplicate-patient check (spec 002), sender maintenance (spec 001 — merging does
> not change a submission's sender), and duplicate-patient handling in exports (spec 012).

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Merge a duplicate Haemophilus patient into the patient to keep (Priority: P1)

A Standard User who knows that two Haemophilus patient records describe the same person opens Administration →
Haemophilus → "Patienten Zusammenfügen", enters both patient numbers, reviews both patients and their submissions on a
confirmation page, chooses which patient is kept ("Zusammenfügen zu"), and confirms. All active submissions of the
other patient are attached to the kept patient and the other patient record is deleted.

**Why this priority**: This is the only purpose of the function and it is a data-losing operation on identifiable
patient data (Constitution Principle I): the duplicate patient record is irreversibly removed.

**Independent Test**: Create two Haemophilus patients A and B, each with at least one active submission, and give B
different attribute values (for example a different gender or clinical information). Merge with A as kept patient.
Verify that all former submissions of B now belong to A, that B no longer exists, that A's attributes are unchanged,
and that the moved submissions keep their laboratory numbers, stem numbers, sender, isolate data, and report status.

**Acceptance Scenarios**:

1. **Given** a Standard User, **When** the Administration menu is opened, **Then** the "Haemophilus" section offers
   "Patienten Zusammenfügen", which opens the page titled "Patienten Zusammenfügen (Haemophilus)" with the heading
   "Patientennummern", two numeric fields "Patienten-Nr. 1" and "Patienten-Nr. 2", and the button
   "Zusammenfügen vorbereiten".
2. **Given** two different patient numbers that each have at least one active (not soft-deleted) Haemophilus
   submission, **When** the user selects "Zusammenfügen vorbereiten", **Then** the confirmation page (heading
   "Bestätigung") shows for each patient a summary line in the form
   `Patienten-Nr.: <n>,  Initialen: <initials>, Geschlecht: <gender>, Postleitzahl: <postal code>` and, under
   "Einsendungen Patient 1" / "Einsendungen Patient 2", one line per active submission in the form
   `Labornummer: <lab no.>, Labnr. Einsender: <sender lab no.>, Eingangsdatum: <receiving date>, Stammnummer: <stem no.>`.
3. **Given** the confirmation page, **When** it is shown, **Then** it offers the choice "Zusammenfügen zu" with the
   options "Patient 1" and "Patient 2" (Patient 1 preselected), the red button
   "Zusammenfügen und doppelten Patienten löschen", and an "Abbrechen" link.
4. **Given** the user keeps "Patient 1" and confirms, **When** the merge runs, **Then** every active submission of
   Patient 2 is reassigned to Patient 1 and the Patient 2 record is deleted, in a single save.
5. **Given** the user selects "Patient 2" and confirms, **When** the merge runs, **Then** every active submission of
   Patient 1 is reassigned to Patient 2 and the Patient 1 record is deleted.
6. **Given** a merge has run, **When** the result is inspected, **Then** the kept patient's own data (initials, date
   of birth, postal code, gender, city, county, country, federal state, clinical information, other clinical
   information, Hib vaccination status and date) is unchanged, and none of the deleted patient's values have been
   carried over.
7. **Given** a merge has run, **When** a moved submission is opened, listed, reported, or exported, **Then** it shows
   the kept patient's data, while its own submission and isolate data (sender, sender laboratory number, dates,
   laboratory number, stem number, laboratory results, report status and date) is unchanged.

---

### User Story 2 - Be prevented from merging invalid patient numbers (Priority: P2)

The function checks the two entered numbers before any confirmation: they must differ, and each must identify a
Haemophilus patient that currently has at least one active submission.

**Why this priority**: These checks are the only safeguards before the destructive step; they determine which
inputs can reach the confirmation page at all.

**Independent Test**: Submit the request form with identical numbers, with a number that does not exist, with the
number of a patient whose submissions are all soft-deleted, and with a Meningococci patient number; verify the
messages and that no confirmation page is shown.

**Acceptance Scenarios**:

1. **Given** both fields contain the same number, **When** the user selects "Zusammenfügen vorbereiten", **Then** the
   form is shown again with the message "Bitte geben Sie zum Zusammenfügen zwei verschiedenen Patientennummern ein"
   at "Patienten-Nr. 1" and nothing else happens.
2. **Given** a number that does not identify a Haemophilus patient, **When** the request is submitted, **Then** the
   form is shown again with "Unbekannter Patient" at the affected field; if both numbers are unknown, both fields
   show the message.
3. **Given** a number of an existing Haemophilus patient whose submissions are all soft-deleted, or who has no
   submissions, **When** the request is submitted, **Then** that number is also reported as "Unbekannter Patient".
4. **Given** a non-numeric entry, **When** the request is submitted, **Then** it is rejected as an invalid number by
   the input conversion and the form is shown again.

---

### User Story 3 - Cancel or leave the merge pages (Priority: P3)

From the confirmation page the user can abandon the merge; after a successful merge the success page reports which
patient received the submissions and which was deleted and offers a way back.

**Why this priority**: These are the exits of the flow; they do not change data but determine where the user lands.

**Independent Test**: Open the confirmation page and choose "Abbrechen"; complete a merge and choose
"Zurück zur Übersicht"; verify the target pages and that cancelling changed no data.

**Acceptance Scenarios**:

1. **Given** the confirmation page, **When** the user chooses "Abbrechen", **Then** no data is changed and the
   **Meningococci** merge page ("Patienten Zusammenfügen (Meningokokken)") opens, not the Haemophilus one.
2. **Given** a successful merge, **When** the success page (heading "Erfolgreich") is shown, **Then** it repeats both
   patient summary lines as they were before the merge and states "Alle Einsendungen wurden diesem Patienten
   zugeordnet." for the kept patient and "Der Patient wurde gelöscht." for the deleted patient; it does not list the
   moved submissions.
3. **Given** the success page, **When** the user chooses "Zurück zur Übersicht", **Then** the **Meningococci**
   submission list opens, not the Haemophilus list.
4. **Given** a successful merge, **When** the browser resubmits the confirmation request (for example via reload),
   **Then** the deleted patient is now reported as "Unbekannter Patient" and no further change is made.

### Edge Cases

- Opening the request page from the menu (no numbers submitted yet) treats both numbers as `0`; because `0` equals
  `0`, the "two different patient numbers" message is already displayed at "Patienten-Nr. 1" before the user has
  entered anything, and both fields show `0`. (Derived from the request handling; not exercised in a running
  system.) [SUSPECTED-DEFECT]
- Patient numbers are separate per pathogen database. The Haemophilus page looks up Haemophilus patients only; a
  Meningococci patient number either is "Unbekannter Patient" or, if the same number exists in Haemophilus,
  silently addresses an unrelated Haemophilus patient. The confirmation page is the only point where the user can
  notice this.
- The Haemophilus patient number is shown read-only ("Patienten-Nr.") on the Haemophilus isolate edit page and as a
  column of the Haemophilus Laboratory export. The Haemophilus submission list has no patient-number column or
  patient-number search (the data is sent to the list but not displayed), unlike the Meningococci list, which offers
  a "Patienten-Nr." search. [POTENTIAL-REFACTORING]
- Only active (not soft-deleted) submissions are shown on the confirmation page and moved. Soft-deleted submissions
  of the patient to be deleted stay attached to that patient. When the patient is then deleted, the database's
  cascading rule for submissions tries to delete those soft-deleted submissions as well, which is blocked by their
  isolate records; the whole save fails, nothing is changed, and the user sees the generic error page. A
  soft-deleted submission without an isolate record would instead be permanently deleted together with the patient
  without being shown. (Derived from the schema and code; not exercised.) [SUSPECTED-DEFECT]
- The confirmation summary shows only patient number, initials, gender, and postal code. Date of birth, residence,
  clinical information, and Hib vaccination are not shown, so the user cannot compare the attributes that will be
  discarded with the deleted patient. There is no field-by-field merge: the kept record wins entirely.
- The data prepared for the confirmation page carries Patient 1's number in the slot for Patient 2's number. The
  defect is not observable today, because the confirmation form re-displays the number the user submitted for
  "Patienten-Nr. 2" rather than the prepared value; it would surface if that re-display behavior changed.
  [SUSPECTED-DEFECT]
- The merge step re-reads the active submissions at confirmation time. A submission added to, removed from, or
  restored for the patient to be deleted between showing the confirmation page and confirming is handled according
  to the state at confirmation, even though it was not (or was) shown.
- The request, confirmation, and execution all use the same page address and accept any request method; the
  submitted forms carry an anti-forgery token, but it is not checked. A single request carrying both numbers, the
  kept-patient choice, and the confirmation flag performs the merge immediately, without the confirmation page being
  shown. [SUSPECTED-DEFECT]
- No record of the merge is kept: there is no log entry, no history of the former patient number on the moved
  submissions, and no undo. The deleted patient can only be recovered from a database backup. This does not meet the
  auditability expectation of Constitution Principle I for data-losing merges. [SUSPECTED-DEFECT]
- The Haemophilus confirmation and success pages are copies of the Meningococci pages and still contain the
  Meningococci targets for "Abbrechen" and "Zurück zur Übersicht" (see User Story 3). [SUSPECTED-DEFECT]
- The user flow (validation, lookup, confirmation, kept-patient choice, success page) lives once on the shared
  patient-and-submission controller base, while the per-pathogen lookup and reassign-and-delete step exist as two
  structurally identical copies (Haemophilus and Meningococci) and the three pages exist as two near-identical view
  sets. The merge logic itself sits in the controller rather than in a service or domain component. This reflects
  both items of the constitution's "Known Architectural Constraints (Legacy Debt)": parallel Meningococci/Haemophilus
  domains and god-controllers. [POTENTIAL-REFACTORING]
- Merging does not change how exports group submissions of the same person: the export duplicate rules (spec 012)
  compare patient attributes, not patient records. After a merge, the moved submissions are exported with the kept
  patient's attributes, which can change their grouping when the two records' attributes differed.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The navigation MUST offer "Patienten Zusammenfügen" under the "Haemophilus" heading of the
  Administration menu to accounts holding the Standard User or Administrator role; it MUST NOT be offered to the
  Public Health role.
- **FR-002**: The merge function (request, confirmation, and execution) MUST be permitted only for the Standard User
  role (`Standardbenutzer`); an Administrator-only or Public Health account MUST be refused.
- **FR-003**: The request page MUST collect two whole-number patient numbers, "Patienten-Nr. 1" and
  "Patienten-Nr. 2".
- **FR-004**: The system MUST reject a request whose two numbers are equal with the message "Bitte geben Sie zum
  Zusammenfügen zwei verschiedenen Patientennummern ein" and MUST perform no lookup in that case.
- **FR-005**: For each number the system MUST look up the active (not soft-deleted) Haemophilus submissions of that
  patient; a number without any such submission MUST be reported as "Unbekannter Patient" at its field, and no
  confirmation page MUST be shown while either number is unknown.
- **FR-006**: The confirmation page MUST show, for each patient, the summary of patient number, initials, gender,
  and postal code (taken from the patient of that patient's first listed submission) and one line per active
  submission with laboratory number, sender laboratory number, receiving date, and stem number.
- **FR-007**: The confirmation page MUST let the user choose the kept patient ("Zusammenfügen zu": "Patient 1" or
  "Patient 2", default "Patient 1") and MUST require an explicit confirmation via
  "Zusammenfügen und doppelten Patienten löschen" before any data is changed.
- **FR-008**: On confirmation, the system MUST re-check FR-004 and FR-005 and then MUST reassign every active
  Haemophilus submission of the non-selected patient to the selected patient.
- **FR-009**: In the same save as FR-008, the system MUST delete the non-selected patient record.
- **FR-010**: The merge MUST NOT change the kept patient's data, MUST NOT copy any value from the deleted patient, and
  MUST NOT change any submission or isolate data other than the submission's patient assignment.
- **FR-011**: The merge MUST NOT reassign soft-deleted submissions of the deleted patient; when such submissions
  exist, the save MUST fail as a whole (see Edge Cases) so that neither the reassignment nor the deletion takes
  effect.
- **FR-012**: After a successful merge the success page MUST show both pre-merge patient summaries, marking the kept
  patient with "Alle Einsendungen wurden diesem Patienten zugeordnet." and the other with "Der Patient wurde
  gelöscht.".
- **FR-013**: The "Abbrechen" link of the Haemophilus confirmation page MUST lead to the Meningococci merge page and
  the "Zurück zur Übersicht" button of the Haemophilus success page MUST lead to the Meningococci submission list
  (current behavior, see Edge Cases).
- **FR-014**: The merge MUST NOT write any audit record, log entry, or former-patient reference, and MUST NOT offer an
  undo.
- **FR-015**: The merge MUST NOT require the request to be sent as a form submission nor verify the anti-forgery
  token (current behavior, see Edge Cases).
- **FR-016**: This feature MUST document the current merge behavior only and MUST NOT introduce new matching,
  merging, or audit rules.

### Merge Outcome

| Data item | Kept patient (selected) | Patient not selected |
| --- | --- | --- |
| Patient record and all its attributes | Unchanged | Deleted permanently |
| Active submissions | Stay attached | Reassigned to the kept patient |
| Soft-deleted submissions | Stay attached | Not reassigned; their presence makes the merge fail (see Edge Cases) |
| Isolate data, laboratory/stem numbers, sender, report status of submissions | Unchanged | Unchanged (follow the reassigned submission) |
| Merge history / audit trail | None | None |

### Roles and Boundaries

| Actor or role | Current observable involvement |
| --- | --- |
| Standard User (`Standardbenutzer`) | Sees the menu entry and can request, confirm, and execute merges. |
| Administrator | Sees the Administration menu entry, but the merge function refuses the role unless the account also holds the Standard User role (consistent with spec 011). |
| Public Health (`RKI`) | Is not offered the entry and is refused by the merge function. |

### Shared Concept: Patient merge on the shared base

- The request/confirmation/success flow, the validation of the two numbers, and the kept-patient choice are
  implemented once on the shared patient-and-submission controller base (`PatientSendingControllerBase`) and are
  identical for Haemophilus and Meningococci.
- Each pathogen supplies only (a) the lookup of a patient's active submissions and (b) the reassign-and-delete step;
  the Haemophilus and Meningococci versions of both are structurally identical, differing only in the pathogen's
  submission and patient data.
- The Meningococci variant (C1-M) is specified separately; behavior stated here for the shared flow applies equally
  there unless the C1-M specification records a difference.

### Key Entities *(include if feature involves data)*

- **Haemophilus patient**: The person record identified by its patient number; holds initials, date of birth, postal
  code, gender, residence, federal state, clinical information, and Hib vaccination. The selected patient is kept
  unchanged; the other one is deleted.
- **Haemophilus submission (Einsendung)**: Belongs to exactly one patient and carries the sender, dates, sender
  laboratory number, and a soft-delete flag; the merge changes only its patient assignment and only when it is
  active.
- **Haemophilus isolate**: The laboratory record of a submission (laboratory number, stem number, results, report
  status); not changed by the merge, but blocks the deletion of a soft-deleted submission still attached to the
  deleted patient.
- **Merge request**: Two patient numbers, the kept-patient choice ("Patient 1"/"Patient 2"), and a confirmation flag;
  it is not stored.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: In 100% of tested successful merges, the number of active submissions of the kept patient afterwards
  equals the sum of both patients' active submissions shown on the confirmation page, and the non-selected patient no
  longer exists.
- **SC-002**: In 100% of tested merges, the kept patient's attributes and every moved submission's own submission and
  isolate data are identical before and after the merge, except for the patient assignment.
- **SC-003**: 100% of requests with equal numbers, unknown numbers, or numbers of patients without active
  submissions end on the request page with the documented message and change zero records.
- **SC-004**: 100% of confirmation-page cancellations change zero records.
- **SC-005**: In 100% of tested merges where the patient to be deleted has a soft-deleted submission with an isolate
  record, zero records are changed.
- **SC-006**: Across the three roles, observed access matches the Roles and Boundaries table in 100% of attempts.
- **SC-007**: Inspection after a merge finds zero audit, log, or history records of the merge.

## Assumptions

- Current application code, views, schema migrations, and role constants are the source of truth; no automated test
  covers the merge function, so no statement here is confirmed by a test.
- Behavior on first opening the request page (the "two different numbers" message shown for the default `0`/`0`
  values) is derived from how the request is bound and validated and was not observed in a running system.
- The failure on soft-deleted submissions is derived from the schema (submissions are deleted with their patient,
  isolates are not deleted with their submission) and from the assumption that every submission recorded through
  intake (spec 002) has an isolate record; it was not exercised. The user-facing result is assumed to be the
  application's generic error page.
- The latent mix-up of Patient 2's number in the confirmation data is assumed to be masked by the standard behavior
  of re-displaying submitted form values; this was not verified in a running system.
- As in spec 011, accounts are assumed to hold exactly one role, so an Administrator account is refused.
- "Active submission" means a submission not soft-deleted as described in spec 002.
- Merging is assumed to be used for records of the same person that the intake duplicate check (spec 002) did not
  or could not consolidate; the system does not verify that the two patients are the same person.
