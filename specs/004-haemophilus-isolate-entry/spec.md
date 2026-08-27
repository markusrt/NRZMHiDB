# Feature Specification: Haemophilus isolate analysis results entry

**Feature Branch**: `004-haemophilus-isolate-entry`

**Created**: 2026-08-27

**Status**: Draft (status-quo inventory)

**Input**: User description: "Feature title: \"Haemophilus isolate analysis results entry\" (backlog ref A3-H, Group A — Core laboratory workflow). Document the current, as-implemented behavior of recording Haemophilus laboratory analysis results for a sending as an Isolate entry, as it exists today. Read HaemophilusWeb/Controllers/IsolateController.cs and IsolateControllerBase.cs, the Isolate and IsolateBase models, and the related Views. Capture how results are entered, which fields exist, validation, and when an isolate is considered ready for reporting. Note that coordination of who performs which analysis happens outside the system (no in-app task planning). Document observable behavior only — do not propose changes."

> **Scope note**: This is an *as-is* specification. It documents the behavior of the Haemophilus
> isolate analysis-results entry (`Isolate`) exactly as implemented on the current branch (backlog
> ref A3-H). It does **not** propose changes. Each isolate is created during the combined
> patient-and-sending intake (A2-H) and is keyed 1:1 to its submission (`SendingId` is the isolate
> key); this feature covers the subsequent *editing* of that isolate to record laboratory analysis
> results. The parallel Meningococci isolate entry (`MeningoIsolate`, A3-M) shares the same base
> controller (`IsolateControllerBase<…>`) and base fields (`IsolateCommon`); it is out of scope
> here except where the shared abstraction is noted. Coordination of *who* performs *which*
> analysis happens outside the system — there is no in-app task planning.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Record laboratory analysis results for a submission's isolate (Priority: P1)

A laboratory user opens the isolate that was created for a received submission and records the
results of the laboratory work-up: whether the strain grew and the type of growth, the
identification and typing tests (oxidase, factor test, agglutination, ß-lactamase, PCR-based
capsule typing, molecular markers, MALDI-TOF, 16S rDNA, sequencing, MLST, etc.), the overall
species/serotype evaluation, and a free-text remark. On a valid save the results are persisted and
the user is returned to the submissions list.

**Why this priority**: Recording the analysis results on the isolate is the core purpose of this
feature — without it there is nothing to interpret or report. Every other capability here builds on
being able to enter and persist results.

**Independent Test**: Sign in as a standard laboratory user, open a submission's isolate for
editing, enter growth, the identification/typing test results, an evaluation, and a valid
laboratory number, and save; confirm the results are persisted and the user is returned to the
submissions list.

**Acceptance Scenarios**:

1. **Given** an authenticated standard user, **When** they open an existing isolate for editing,
   **Then** the isolate edit form is shown pre-filled with the isolate's current values, with the
   submission's sampling location, material, invasive flag, patient id, and patient age at sampling
   shown as read-only context, and with the available antibiotics and current clinical breakpoints
   made available for the E-Test section.
2. **Given** the isolate edit form, **When** the user records growth, the identification and typing
   test results, an evaluation, a valid laboratory number, and saves via the primary submit,
   **Then** the results are persisted and the user is returned to the submissions list.
3. **Given** a valid form, **When** the user saves via the secondary submit ("Änderungen speichern
   und Befund erstellen"), **Then** the results are persisted and the user is taken to report
   creation for that isolate.
4. **Given** the isolate edit form, **When** the user saves with any required or conditionally
   required field missing or invalid, **Then** nothing is persisted and the form is redisplayed
   with validation messages.

---

### User Story 2 - Enter E-Test measurements with automatic susceptibility interpretation (Priority: P2)

While recording results, the user enters Epsilometer-test (E-Test) measurements for antibiotics.
For each measurement the user selects an antibiotic and the applicable EUCAST clinical breakpoint,
and enters the measured value; the form derives the susceptibility result (susceptible /
intermediate / resistant / not determined) from the measurement against the selected breakpoint.

**Why this priority**: Antibiotic susceptibility is a key clinical output of the work-up, but it is
secondary to being able to record the core identification results at all.

**Independent Test**: On the isolate edit form, add an E-Test row, select an antibiotic and its
breakpoint, enter a measurement, and confirm the susceptibility result is derived from the
measurement and breakpoint and is persisted with the isolate on save.

**Acceptance Scenarios**:

1. **Given** the isolate edit form, **When** the user selects an antibiotic for an E-Test row,
   **Then** the breakpoint choices are limited to the clinical breakpoints defined for that
   antibiotic in the Haemophilus database.
2. **Given** a selected antibiotic and breakpoint, **When** the user enters a measurement, **Then**
   the susceptibility result is derived (resistant above the resistant breakpoint, susceptible at or
   below the susceptible breakpoint, intermediate in between, or not determined when no EUCAST value
   is available).
3. **Given** one or more E-Test rows, **When** the user saves the isolate, **Then** the E-Test
   measurements and their derived results are persisted with the isolate.

---

### User Story 3 - Set the evaluation and mark the isolate ready for reporting (Priority: P3)

The user sets the overall evaluation (the species/serotype conclusion, e.g. "Hib", "NTHi"). The
form pre-selects the evaluation from the agglutination result and warns when the chosen evaluation
disagrees with the agglutination. From a valid isolate the user can proceed to create the report;
whether a report has been produced is tracked by the isolate's report status.

**Why this priority**: Reaching a reportable conclusion is the endpoint of the work-up, but it
depends on the results and susceptibility data recorded in the earlier stories.

**Independent Test**: On the isolate edit form, choose an agglutination serogroup and confirm the
evaluation is pre-selected accordingly; change the evaluation to disagree and confirm a mismatch
warning is shown; save and proceed to report creation.

**Acceptance Scenarios**:

1. **Given** the isolate edit form, **When** the user selects an agglutination serogroup (A–F),
   **Then** the evaluation is set to the matching Haemophilus serotype; for a non-serogroup
   agglutination result the evaluation is set to non-encapsulated (NTHi).
2. **Given** an agglutination serogroup is selected, **When** the chosen evaluation does not match
   the serogroup, **Then** a discrepancy warning is displayed for the evaluation.
3. **Given** a valid isolate, **When** the user saves via the secondary submit, **Then** the user is
   taken to create the report for that isolate, where the report status (none / preliminary / final)
   is subsequently set.

---

### Edge Cases

- **Laboratory number format**: Saving with an empty laboratory number, or one not in the `39/14`
  form (digits, slash, two digits), fails validation ("Die Labornummer muss in der Form '39/14'
  eingegeben werden.").
- **Growth type required when growth present**: When growth is set to "yes", a type of growth must
  be selected; otherwise validation fails ("Die Art des Wachstums muss angegeben werden.").
- **Conditionally required detail fields**: When 16S rDNA is positive, its best match and match
  percentage are required; when a MALDI-TOF (VITEK or Biotyper) result is "determined", its best
  match and confidence are required; when MLST is "determined", the sequence type is required; when
  Real-Time PCR is positive, its RIDOM evaluation is required. Missing any of these fails validation
  ("... darf nicht leer sein.").
- **Duplicate stem number**: Saving a stem number already used by another isolate fails with "Diese
  Stammnummer ist bereits vergeben" on the stem-number field.
- **Duplicate laboratory number**: Saving a laboratory number already used by another isolate fails
  with "Diese Labornummer ist bereits vergeben" on the laboratory-number field.
- **Evaluation vs agglutination discrepancy**: Choosing an evaluation that disagrees with the
  selected agglutination serogroup surfaces a warning but does not block saving.
- **Unknown or missing isolate id**: Requesting the edit form with no id returns a bad-request
  response; requesting a non-existent isolate returns a not-found response.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST provide an isolate edit form that lets an authorized user record the
  laboratory analysis results for the isolate belonging to a received submission, pre-filled with the
  isolate's current values.
- **FR-002**: The isolate MUST correspond one-to-one to its submission (the submission is the
  isolate's key); the system does not create isolates from this feature — an isolate already exists
  for each submission from intake (A2-H) and this feature edits it.
- **FR-003**: The isolate edit form MUST display the submission's sampling location, material,
  invasive flag, patient id, and patient age at sampling as read-only context.
- **FR-004**: The system MUST allow entry of the growth outcome and, when growth is present, the type
  of growth, and MUST require a type of growth to be selected whenever growth is "yes".
- **FR-005**: The system MUST allow entry of the identification and typing results, including at
  least: oxidase, factor test, agglutination, ß-lactamase, penicillin ADT, outer-membrane proteins
  P2 and P6, bexA, fucK, serotype PCR, genome sequencing, ftsI (with up to three evaluations),
  MLST (with sequence type), Real-Time PCR (with device and RIDOM evaluation), 16S rDNA (with best
  match and match percentage), MALDI-TOF via VITEK MS and via Biotyper (each with best match and
  confidence), and the api NH identification.
- **FR-006**: The system MUST allow entry of Epsilometer-test (E-Test) measurements, each associating
  an antibiotic and an applicable EUCAST clinical breakpoint with a measured value, and MUST derive
  the susceptibility result (susceptible / intermediate / resistant / not determined) from the
  measurement against the selected breakpoint.
- **FR-007**: The E-Test section MUST offer only the antibiotics and clinical breakpoints defined for
  the Haemophilus database, with the current breakpoints presented first.
- **FR-008**: The system MUST allow the user to set an overall evaluation (species/serotype
  conclusion), MUST pre-select the evaluation from the agglutination result (serogroups A–F map to
  the matching Haemophilus serotype; other results map to non-encapsulated / NTHi), and MUST warn
  when the chosen evaluation disagrees with the selected agglutination serogroup without blocking the
  save.
- **FR-009**: The system MUST allow entry of a laboratory number, a stem number, a report date, and a
  free-text remark on the isolate.
- **FR-010**: The system MUST require a laboratory number and MUST require it to match the `39/14`
  form (digits, slash, two digits); an empty or malformed laboratory number MUST fail validation.
- **FR-011**: The system MUST enforce conditionally required detail fields on save: 16S rDNA best
  match and percentage when 16S rDNA is positive; MALDI-TOF (VITEK and Biotyper) best match and
  confidence when the respective result is "determined"; MLST sequence type when MLST is
  "determined"; and the Real-Time PCR RIDOM evaluation when Real-Time PCR is positive.
- **FR-012**: The system MUST reject a save whose stem number or laboratory number duplicates that of
  another isolate, reporting the conflict against the corresponding field ("Diese Stammnummer ist
  bereits vergeben" / "Diese Labornummer ist bereits vergeben").
- **FR-013**: When required or conditionally required fields are missing or invalid, the system MUST
  reject the save, persist nothing, and redisplay the form with validation messages.
- **FR-014**: On a valid save via the primary submit, the system MUST persist the results and return
  the user to the submissions list; on a valid save via the secondary submit, the system MUST persist
  the results and take the user to report creation for that isolate.
- **FR-015**: The isolate MUST carry a report status of none, preliminary, or final; the status is
  set by the reporting flow (A4-H), not by this feature. "Ready for reporting" is expressed by the
  isolate saving validly and the user being able to proceed to report creation from it.
- **FR-016**: All isolate edit actions MUST require the user to hold the standard laboratory-user
  role; unauthenticated or unauthorized requests MUST be denied.
- **FR-017**: Requesting the edit form without an isolate id MUST return a bad-request response, and
  requesting a non-existent isolate MUST return a not-found response.
- **FR-018**: The system MUST protect the isolate edit against over-posting via an anti-forgery token
  on the edit form.
- **FR-019**: Coordination of which laboratory user performs which analysis MUST remain outside the
  system; the system provides no in-app task planning or assignment for isolate analyses.

### Key Entities *(include if feature involves data)*

- **Isolate**: The laboratory work-up record for a single Haemophilus submission, keyed one-to-one to
  that submission. Carries the identification/typing results (growth and type of growth, oxidase,
  factor test, agglutination, ß-lactamase, penicillin ADT, outer-membrane proteins P2/P6, bexA, fucK,
  serotype PCR, genome sequencing, ftsI and its evaluations, MLST and sequence type, Real-Time PCR
  with device and RIDOM evaluation, 16S rDNA with best match and percentage, MALDI-TOF via VITEK and
  Biotyper with best match and confidence, api NH), the overall evaluation, the E-Test collection,
  the stem number and laboratory number, the yearly sequential isolate number and year, a report
  date, a report status, and a remark. Base fields are shared with the Meningococci isolate via the
  common isolate abstraction.
- **Epsilometer test (E-Test)**: A single antibiotic-susceptibility measurement belonging to an
  isolate, associating an antibiotic and an applicable EUCAST clinical breakpoint with a measured
  value and a derived susceptibility result.
- **EUCAST clinical breakpoint**: Authoritative reference data (per antibiotic, with validity dates
  and susceptible/resistant thresholds) used to select antibiotics and derive E-Test susceptibility
  results; filtered to the Haemophilus database. Managed elsewhere (C3); referenced here only for
  selection and interpretation.
- **Submission ("Einsendung" / Sending)**: The received submission the isolate belongs to; supplies
  the read-only context (sampling location, material, invasive flag, patient id, patient age at
  sampling). Referenced here only insofar as the isolate is keyed to it.
- **Report status**: The isolate's reporting state — none, preliminary, or final — set by the
  reporting flow rather than by isolate entry.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: An authorized user can open a submission's isolate, record its laboratory analysis
  results, and save them in a single form, then be returned to the submissions list.
- **SC-002**: Attempting to save an isolate with an empty or malformed laboratory number, a missing
  type of growth when growth is "yes", or any missing conditionally required detail field, never
  persists the isolate and always returns an actionable validation message.
- **SC-003**: An E-Test measurement always yields a susceptibility result consistent with the
  measured value and the selected clinical breakpoint, and that result is persisted with the isolate.
- **SC-004**: A stem number or laboratory number that duplicates another isolate's is never stored,
  and the conflict is always reported against the correct field.
- **SC-005**: Selecting an agglutination serogroup always pre-selects the matching evaluation, and a
  disagreeing evaluation is always flagged to the user while still allowing the save.
- **SC-006**: From a valid isolate the user can always proceed to report creation, and every isolate
  carries a report status of none, preliminary, or final.

## Assumptions

- **Isolate created at intake, not here**: Each isolate is created during the combined
  patient-and-sending intake (A2-H) and is keyed 1:1 to its submission (`SendingId`). This feature
  documents editing only; there is no create/delete action for isolates in this controller.
- **Reporting sets the report status**: The report status (none / preliminary / final) and the
  interpretation are produced by the reporting flow (A4-H) and interpretation engine (A5-H); this
  feature only records the inputs and lets the user proceed to report creation.
- **Susceptibility derivation is form-side**: The E-Test susceptibility result is derived on the edit
  form from the measurement against the selected breakpoint; this is documented as observed behavior.
- **Shared isolate abstraction**: The base controller (`IsolateControllerBase<…>`) and base fields
  (`IsolateCommon`) are shared with the Meningococci isolate (A3-M) per the constitution's noted
  legacy debt (parallel Meningococci/Haemophilus domains); Haemophilus-specific fields (e.g. api NH)
  live on the Haemophilus isolate.
- **Standard role name**: The standard laboratory-user role is the application's default user role;
  it is required for all isolate edit actions.
- **No in-app task planning**: Coordinating who performs which analysis is out of scope by design;
  the system offers no assignment or scheduling of analyses.
</content>
</invoke>
