# Feature Specification: Merge patients (Meningococci)

**Feature Branch**: `speckit-init`

**Created**: 2026-10-22

**Status**: Draft (status-quo inventory)

**Input**: User description: "Feature title: \"Merge patients (Meningococci)\" (backlog ref C1-M, Group C — Reference data & administration). Document the current, as-implemented behavior of merging two Meningococci patient records as it exists today. Read the MergePatient / MergePatients logic in HaemophilusWeb/Controllers/MeningoPatientSendingController.cs and PatientSendingControllerBase.cs. Capture the user flow, what is reassigned, what is deleted, and roles. Document observable behavior only — do not propose changes."

> **Scope note**: This is an *as-is* specification (backlog ref C1-M). It records the current, observable behavior of
> the Meningococci "Patienten Zusammenfügen" (merge patients) function. The request/confirmation/success flow is
> implemented once on the shared patient-and-submission controller base (`PatientSendingControllerBase`) and is
> specified in full in spec 014 (C1-H, "Merge patients (Haemophilus)"). This specification does **not** repeat that
> shared flow; it states the Meningococci entry point and pages, the Meningococci lookup and reassign-and-delete
> step, the Meningococci data affected, and — for each finding of spec 014 — whether it applies to the Meningococci
> variant the same, differently, or not at all. It does **not** propose changes. Out of scope: Meningococci submission
> intake, editing, soft-delete and restore (spec 003, A2-M), the intake duplicate-patient check (spec 003), sender
> maintenance (spec 001 — merging does not change a submission's sender), the Meningococci exports themselves
> (spec 011) and their absent duplicate handling (spec 012), and the Haemophilus merge (spec 014).

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Merge a duplicate Meningococci patient into the patient to keep (Priority: P1)

A Standard User who knows that two Meningococci patient records describe the same person opens Administration →
Meningokokken → "Patienten Zusammenfügen", enters both patient numbers, reviews both patients and their submissions on
the confirmation page, chooses which patient is kept ("Zusammenfügen zu"), and confirms. All active Meningococci
submissions of the other patient are attached to the kept patient and the other patient record is deleted. The steps,
labels, messages, and outcome are those of spec 014, User Story 1; only the pathogen differs.

**Why this priority**: This is the only purpose of the function and it irreversibly removes an identifiable patient
record (Constitution Principle I).

**Independent Test**: Create two Meningococci patients A and B, each with at least one active submission, and give B
different attribute values (for example a different gender, clinical information, or risk factors). Merge with A as
kept patient. Verify that all former submissions of B now belong to A, that B no longer exists, that A's attributes
are unchanged, and that the moved submissions keep their laboratory numbers ("MZ…"), stem numbers ("DE…"), sender,
sampling data, isolate data, and report status.

**Acceptance Scenarios**:

1. **Given** a Standard User, **When** the Administration menu is opened, **Then** the "Meningokokken" section offers
   "Patienten Zusammenfügen" (its only entry; there is no "Gelöschte Einsendungen" entry for Meningococci), which
   opens the page titled "Patienten Zusammenfügen (Meningokokken)" with the heading "Patientennummern", the fields
   "Patienten-Nr. 1" and "Patienten-Nr. 2", and the button "Zusammenfügen vorbereiten".
2. **Given** two different patient numbers that each have at least one active (not soft-deleted) Meningococci
   submission, **When** the user selects "Zusammenfügen vorbereiten", **Then** the confirmation page (heading
   "Bestätigung", title "Patienten Zusammenfügen (Meningokokken)") shows the patient summary lines and submission lines
   in the formats of spec 014, with Meningococci laboratory numbers prefixed "MZ" (or "NR" for laboratory numbers
   stored with a leading "-") and stem numbers prefixed "DE".
3. **Given** the confirmation page, **When** the user keeps "Patient 1" (preselected) or "Patient 2" and selects
   "Zusammenfügen und doppelten Patienten löschen", **Then** every active Meningococci submission of the non-selected
   patient is reassigned to the selected patient and the non-selected Meningococci patient record is deleted, in a
   single save.
4. **Given** a merge has run, **When** the result is inspected, **Then** the kept patient's own data (initials, date of
   birth, postal code, gender, city, county, country, federal state, clinical information, other clinical
   information, risk factors, other risk factor) is unchanged, and none of the deleted patient's values have been
   carried over.
5. **Given** a merge has run, **When** a moved submission is opened, listed, reported, or exported, **Then** it shows
   the kept patient's data and the kept patient's number, while its own submission and isolate data (sender, sender
   laboratory number, sampling and receiving dates, sampling location, material, sender serogroup, laboratory number,
   stem number, laboratory results, PubMLST link, report status and date) is unchanged.

---

### User Story 2 - Be prevented from merging invalid Meningococci patient numbers (Priority: P2)

The shared validation of spec 014, User Story 2 applies unchanged: the numbers must differ ("Bitte geben Sie zum
Zusammenfügen zwei verschiedenen Patientennummern ein") and each must identify a Meningococci patient with at least
one active submission ("Unbekannter Patient" otherwise).

**Why this priority**: These checks are the only safeguards before the destructive step.

**Independent Test**: Submit the Meningococci request form with identical numbers, with an unknown number, with the
number of a Meningococci patient whose submissions are all soft-deleted, and with a Haemophilus-only patient number;
verify the messages and that no confirmation page is shown.

**Acceptance Scenarios**:

1. **Given** equal numbers, **When** the request is submitted, **Then** the form is shown again with the
   "two different patient numbers" message at "Patienten-Nr. 1" and nothing else happens.
2. **Given** a number that does not identify a Meningococci patient with an active Meningococci submission, **When**
   the request is submitted, **Then** "Unbekannter Patient" is shown at the affected field; Haemophilus patients and
   submissions are never consulted.

---

### User Story 3 - Cancel or leave the Meningococci merge pages (Priority: P3)

From the confirmation page the user can abandon the merge; after a successful merge the success page (heading
"Erfolgreich") reports which patient received the submissions and which was deleted and offers a way back.

**Why this priority**: These are the exits of the flow; they do not change data but determine where the user lands.

**Independent Test**: Open the Meningococci confirmation page and choose "Abbrechen"; complete a merge and choose
"Zurück zur Übersicht"; verify the target pages and that cancelling changed no data.

**Acceptance Scenarios**:

1. **Given** the Meningococci confirmation page, **When** the user chooses "Abbrechen", **Then** no data is changed
   and the Meningococci merge request page opens (correct pathogen; differs from the Haemophilus pages, see spec 014).
2. **Given** a successful merge, **When** the success page is shown, **Then** it repeats both pre-merge patient summary
   lines with "Alle Einsendungen wurden diesem Patienten zugeordnet." for the kept patient and "Der Patient wurde
   gelöscht." for the other one.
3. **Given** the success page, **When** the user chooses "Zurück zur Übersicht", **Then** the Meningococci submission
   list opens (correct pathogen).

### Edge Cases

Findings of spec 014, checked against the Meningococci variant:

- **First opening shows the validation message — applies the same.** Opening the request page from the menu treats
  both numbers as `0`; the "two different patient numbers" message is shown at "Patienten-Nr. 1" before anything was
  entered, and both fields show `0`. The cause is the shared request handling. (Derived from code; not exercised.)
  [SUSPECTED-DEFECT]
- **Soft-deleted submissions of the deleted patient — applies the same in principle, but is harder to reach.** Only
  active Meningococci submissions are shown and moved. Soft-deleted ones stay attached to the patient to be deleted;
  the database rule that deletes a Meningococci patient's submissions with the patient is then blocked by their
  isolate records, the whole save fails, nothing changes, and the generic error page is shown (a soft-deleted
  submission without an isolate would be removed silently instead). Unlike Haemophilus, the Meningococci user
  interface offers no working way to soft-delete a submission today: the "Eintrag löschen" button on the Meningococci
  submission edit page leads to a deletion page that does not exist for Meningococci (error page), and there is no
  "Gelöschte Einsendungen" page for Meningococci. Soft-deleted Meningococci submissions can therefore only stem from
  requests outside the normal pages or direct database changes. (Derived from schema and code; not exercised.)
  [SUSPECTED-DEFECT]
- **Missing Meningococci deletion page — Meningococci specific.** The missing deletion page noted above also affects
  the soft-delete flow described in spec 003, User Story 3; it is recorded here because it determines whether the
  soft-delete failure of the merge can occur. (Derived from the set of existing pages; not exercised.)
  [SUSPECTED-DEFECT]
- **Patient 1 number in Patient 2's slot — applies the same.** The data prepared for the confirmation page carries
  Patient 1's number as Patient 2's number. It is masked because the confirmation form re-displays the number the user
  submitted for "Patienten-Nr. 2". [SUSPECTED-DEFECT]
- **No anti-forgery check, any request method — applies the same.** Request, confirmation, and execution share one
  page address, accept any request method, and do not check the anti-forgery token the forms carry; a single request
  with both numbers, the kept-patient choice, and the confirmation flag performs the merge without the confirmation
  page. [SUSPECTED-DEFECT]
- **No audit trail or undo — applies the same.** No log entry, former-patient reference, or undo exists; the deleted
  Meningococci patient can only be recovered from a database backup, which does not meet the auditability expectation
  of Constitution Principle I. [SUSPECTED-DEFECT]
- **Cancel/back links point to the other pathogen — does not apply.** The Meningococci pages' "Abbrechen" and
  "Zurück zur Übersicht" lead to the Meningococci merge page and list. The two view sets differ only in the page
  title, so the wrong Haemophilus targets are consistent with the Haemophilus pages having been copied from the
  Meningococci ones.
- **Patient number visibility in the list — differs.** The Meningococci submission list offers a "Patienten-Nr."
  field in its extended search and includes the patient number in the full-text search; the patient-number column
  itself is hidden. The patient number is also shown read-only ("Patienten-Nr.") on the Meningococci isolate edit
  page and as a column of the Meningococci Laboratory export. Users can therefore find both patients' submissions by
  patient number before merging, which the Haemophilus list does not allow.
- **Cross-pathogen numbers — applies the same.** Patient numbers are separate per pathogen database. A Haemophilus
  patient number entered on the Meningococci page is either "Unbekannter Patient" or silently addresses an unrelated
  Meningococci patient with the same number; the confirmation page is the only point to notice this.
- **Limited confirmation summary — applies the same, with Meningococci attributes.** The summary shows only patient
  number, initials, gender, and postal code; date of birth, residence, clinical information, and risk factors of the
  patient to be deleted are not shown and are discarded. The kept record wins entirely.
- **Stale confirmation — applies the same.** The submissions to move are re-read at confirmation time, so changes
  between showing the confirmation page and confirming are handled according to the state at confirmation.
- **Sampling before birth not re-checked — Meningococci specific.** Meningococci intake rejects a sampling date before
  the patient's date of birth. The merge does not apply this rule: a moved submission whose sampling date lies before
  the kept patient's date of birth is accepted, and the next edit of that submission fails validation with "Das
  Entnahmedatum muss nach dem Geburtsdatum des Patienten liegen" until patient or sampling data is corrected.
  (Derived from code; not exercised.)
- **Derived patient age changes — Meningococci relevant.** "Patientenalter bei Entnahme" in the Meningococci
  Laboratory and IRIS exports and on the isolate page is computed from the kept patient's date of birth, so it can
  change for moved submissions when the two records' dates of birth differed.
- **EpiScanGIS export grouping — Meningococci specific.** The token-protected EpiScanGIS export (not yet specified)
  writes at most one line per Meningococci patient number. After a merge, the moved submissions share the kept
  patient's number, so the export emits one line for the merged person instead of one line per former patient
  record.
- **Export duplicate rules — does not apply.** Unlike Haemophilus, no Meningococci export applies duplicate cleaning or
  marking (spec 012), so the merge does not change any such grouping; Meningococci exports simply show the kept
  patient's attributes for moved submissions.
- **Parallel implementation — applies the same.** The Meningococci lookup and reassign-and-delete step are structurally
  identical copies of the Haemophilus ones, differing only in the Meningococci submission/patient data; the three
  Meningococci pages and the three Haemophilus pages differ only in their title; the merge logic sits in the
  controller. This reflects both items of the constitution's "Known Architectural Constraints (Legacy Debt)"
  (parallel Meningococci/Haemophilus domains; god-controllers). [POTENTIAL-REFACTORING]

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The navigation MUST offer "Patienten Zusammenfügen" under the "Meningokokken" heading of the
  Administration menu to accounts holding the Standard User or Administrator role; it MUST NOT be offered to the
  Public Health role.
- **FR-002**: The Meningococci merge function (request, confirmation, and execution) MUST be permitted only for the
  Standard User role (`Standardbenutzer`), as in spec 014.
- **FR-003**: The Meningococci merge MUST follow the shared request, validation, confirmation, kept-patient choice,
  and success flow of spec 014 (FR-003 to FR-008, FR-012, FR-014, FR-015 there), with page titles
  "Patienten Zusammenfügen (Meningokokken)".
- **FR-004**: The lookup MUST consider only active (not soft-deleted) Meningococci submissions of the entered
  Meningococci patient number and MUST NOT consult Haemophilus data.
- **FR-005**: The confirmation and success pages MUST show Meningococci laboratory numbers with the "MZ" prefix (or
  "NR" for laboratory numbers stored with a leading "-") and stem numbers with the "DE" prefix.
- **FR-006**: On confirmation, the system MUST reassign every active Meningococci submission of the non-selected
  patient to the selected patient and, in the same save, delete the non-selected Meningococci patient record.
- **FR-007**: The merge MUST NOT change the kept patient's data (including clinical information and risk factors),
  MUST NOT copy any value from the deleted patient, and MUST NOT change any submission or isolate data other than the
  submission's patient assignment.
- **FR-008**: The merge MUST NOT reassign soft-deleted Meningococci submissions of the deleted patient; when such
  submissions exist, the save MUST fail as a whole so that neither reassignment nor deletion takes effect.
- **FR-009**: The merge MUST NOT re-check the Meningococci rule that the sampling date lies on or after the patient's
  date of birth for the moved submissions.
- **FR-010**: The "Abbrechen" link of the Meningococci confirmation page MUST lead to the Meningococci merge request
  page, and "Zurück zur Übersicht" on the Meningococci success page MUST lead to the Meningococci submission list.
- **FR-011**: The merge MUST NOT write any audit record, log entry, or former-patient reference, and MUST NOT offer an
  undo.
- **FR-012**: This feature MUST document the current merge behavior only and MUST NOT introduce new matching,
  merging, or audit rules.

### Merge Outcome

| Data item | Kept patient (selected) | Patient not selected |
| --- | --- | --- |
| Meningococci patient record and all its attributes (incl. clinical information, risk factors) | Unchanged | Deleted permanently |
| Active Meningococci submissions | Stay attached | Reassigned to the kept patient |
| Soft-deleted Meningococci submissions | Stay attached | Not reassigned; their presence makes the merge fail (see Edge Cases) |
| Isolate data, laboratory/stem numbers, PubMLST link, sender, report status of submissions | Unchanged | Unchanged (follow the reassigned submission) |
| Values derived from the patient (age at sampling, EpiScanGIS line per patient number) | Recomputed from the kept patient | Now derived from the kept patient |
| Merge history / audit trail | None | None |

### Roles and Boundaries

| Actor or role | Current observable involvement |
| --- | --- |
| Standard User (`Standardbenutzer`) | Sees the menu entry and can request, confirm, and execute Meningococci merges. |
| Administrator | Sees the Administration menu entry, but the merge function refuses the role unless the account also holds the Standard User role (as in spec 014). |
| Public Health (`RKI`) | Is not offered the entry and is refused by the merge function. |

### Differences from the Haemophilus merge (spec 014)

| Aspect | Meningococci (this spec) | Haemophilus (spec 014) |
| --- | --- | --- |
| Shared flow, validation, messages, roles | Same | Same |
| Page titles | "… (Meningokokken)" | "… (Haemophilus)" |
| "Abbrechen" / "Zurück zur Übersicht" targets | Meningococci pages (correct) | Meningococci pages (wrong pathogen) |
| Patient number in submission list | Hidden column, but searchable ("Patienten-Nr." extended search, full-text search) | Not displayed, not searchable |
| Soft-delete through the user interface | Not working (no deletion page); merge failure on soft-deleted submissions only reachable via data set outside the normal pages | Working; merge failure reachable through normal use |
| Patient attributes discarded with the deleted record | Clinical information, risk factors (and the common attributes) | Clinical information, Hib vaccination (and the common attributes) |
| Patient-dependent validation not re-checked | Sampling date not before date of birth | — |
| Export duplicate rules affected | None (no Meningococci duplicate handling) | Grouping may change |
| Per-patient export grouping | EpiScanGIS: one line per patient number | — |

### Key Entities *(include if feature involves data)*

- **Meningococci patient**: Person record identified by its Meningococci patient number; holds initials, date of
  birth, postal code, gender, residence, federal state, clinical information, other clinical information, risk
  factors, and other risk factor. The selected patient is kept unchanged; the other one is deleted.
- **Meningococci submission (Einsendung)**: Belongs to exactly one Meningococci patient; carries sender, dates,
  sampling location, material, sender laboratory number, sender serogroup, and a soft-delete flag. The merge changes
  only its patient assignment and only when it is active.
- **Meningococci isolate**: Laboratory record of a submission (laboratory number, stem number, results, PubMLST link,
  report status); not changed by the merge, but blocks the deletion of a soft-deleted submission still attached to the
  deleted patient.
- **Merge request**: Shared with spec 014 — two patient numbers, the kept-patient choice, and a confirmation flag; not
  stored.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: In 100% of tested successful Meningococci merges, the number of active submissions of the kept patient
  afterwards equals the sum of both patients' active submissions shown on the confirmation page, and the non-selected
  patient no longer exists.
- **SC-002**: In 100% of tested merges, the kept patient's attributes and every moved submission's own submission and
  isolate data are identical before and after the merge, except for the patient assignment.
- **SC-003**: 100% of requests with equal numbers, unknown numbers, or numbers of patients without active
  Meningococci submissions end on the request page with the documented message and change zero records.
- **SC-004**: 100% of cancellations change zero records and land on the Meningococci merge request page; 100% of
  "Zurück zur Übersicht" uses land on the Meningococci submission list.
- **SC-005**: In 100% of tested merges where the patient to be deleted has a soft-deleted submission with an isolate
  record, zero records are changed.
- **SC-006**: Across the three roles, observed access matches the Roles and Boundaries table in 100% of attempts.
- **SC-007**: Inspection after a merge finds zero audit, log, or history records of the merge.

## Assumptions

- Current application code, views, schema migrations, and role constants are the source of truth; no automated test
  covers the merge function, so no statement here is confirmed by a test.
- Every statement made in spec 014 about the shared flow (request binding, validation, confirmation data, request
  method and anti-forgery handling, success page) applies to the Meningococci variant, because the same shared code
  and view layout are used; the derivations and their unverified status are those listed in spec 014's Assumptions.
- The merge failure on soft-deleted submissions is derived from the Meningococci schema (submissions are deleted with
  their patient; isolates are not deleted with their submission) and from the assumption that every Meningococci
  submission recorded through intake has an isolate record; it was not exercised. The user-facing result is assumed
  to be the generic error page.
- The statement that Meningococci submissions cannot be soft-deleted through the normal pages is derived from the
  absence of a Meningococci deletion page (none has ever existed in the repository history) and of a Meningococci
  "Gelöschte Einsendungen" menu entry; it was not exercised. It is assumed that the legacy data import does not set
  the soft-delete flag (the importer does not touch it), so existing soft-deleted Meningococci submissions, if any,
  stem from requests outside the normal pages or direct database changes. Spec 003 (User Story 3, FR-018)
  records the same absence.
- The EpiScanGIS grouping effect is derived from that export's code (one line per patient number); the export itself
  is not yet specified.
- As in specs 011 and 014, accounts are assumed to hold exactly one role, so an Administrator account is refused.
- "Active submission" means a submission not soft-deleted as described in spec 003.
