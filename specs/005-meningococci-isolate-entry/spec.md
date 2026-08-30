# Feature Specification: Meningococci isolate analysis results entry

**Feature Branch**: `005-meningococci-isolate-entry`

**Created**: 2026-08-30

**Status**: Draft (status-quo inventory)

**Input**: User description: "Feature title: \"Meningococci isolate analysis results entry\" (backlog ref A3-M, Group A — Core laboratory workflow). Document the current, as-implemented behavior of recording Meningococci laboratory analysis results for a sending as a MeningoIsolate entry, as it exists today. Read HaemophilusWeb/Controllers/MeningoIsolateController.cs and IsolateControllerBase.cs, the MeningoIsolate and IsolateBase models, and the related Views. Capture how results are entered, which fields exist, validation, and when an isolate is considered ready for reporting. Note that coordination of who performs which analysis happens outside the system (no in-app task planning), and that Meningococci isolates additionally carry PubMLST typing data. Document observable behavior only — do not propose changes."

> **Scope note**: This is an *as-is* specification. It documents the Meningococci isolate
> analysis-results entry exactly as implemented on the current branch (backlog ref A3-M) and
> proposes no changes. An isolate is created with its sending during patient-and-sending intake
> (A2-M); this feature covers editing that existing one-to-one isolate record. The parallel
> Haemophilus isolate feature (A3-H) shares the common isolate fields and edit workflow but is out
> of scope. Coordination of who performs each analysis happens outside the system; there is no
> in-app task planning or assignment. Batch PubMLST matching is covered separately by D1; this
> feature includes the individual lookup and stored PubMLST data visible during isolate entry.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Record Meningococci analysis results (Priority: P1)

A laboratory user opens the isolate belonging to a received Meningococci sending and records the
available culture, identification, serogrouping, molecular typing, susceptibility, report-date,
and remark values. The form provides sending and patient context without allowing those contextual
values to be changed. On a valid save, the results are persisted and the user returns to the
Meningococci sendings list.

**Why this priority**: Recording the laboratory work-up is the core capability. Reporting and
PubMLST enrichment depend on the isolate results being available.

**Independent Test**: Sign in as a standard laboratory user, open an existing Meningococci isolate,
enter valid culture and analysis results, save, reopen the isolate, and confirm that the values were
persisted and that sending/patient context remained read-only.

**Acceptance Scenarios**:

1. **Given** an authenticated standard laboratory user and an existing Meningococci isolate,
   **When** the user opens it for editing, **Then** the current isolate values are pre-filled and
   sampling location, material, invasive classification, patient id, and patient age at sampling
   are displayed as read-only context.
2. **Given** the isolate edit form, **When** the user records valid results and uses the primary
   save action, **Then** the isolate and its valid susceptibility measurements are persisted and
   the user is returned to the Meningococci sendings list.
3. **Given** the isolate edit form, **When** submitted values fail validation, **Then** no changes
   are persisted and the form is redisplayed with validation messages, preserving access to the
   tab containing the error.
4. **Given** both agar growth results are "no" and the received material is not native material,
   **When** the user saves valid results, **Then** the sending material is changed to "no growth".
5. **Given** both agar growth results are "no" and the received material is native material,
   **When** the user saves valid results, **Then** the sending's native-material classification is
   retained.

---

### User Story 2 - Record E-Test susceptibility measurements (Priority: P2)

The laboratory user records Epsilometer-test (E-Test) measurements against antibiotics. Available
antibiotics and breakpoints are limited to Meningococci reference data. The form derives a
susceptibility result from each selected measurement and breakpoint and persists complete rows with
the isolate.

**Why this priority**: Susceptibility results are clinically important analysis data, but they rely
on the isolate edit workflow established by the primary story.

**Independent Test**: Enter an allowed measurement for an antibiotic, select its Meningococci
breakpoint, verify the derived susceptibility category, save, and confirm the row is retained when
the isolate is reopened.

**Acceptance Scenarios**:

1. **Given** the E-Test section, **When** the user selects an antibiotic, **Then** the measurement
   choices use the scale applicable to that antibiotic and the breakpoint choices are limited to
   Meningococci breakpoints for that antibiotic.
2. **Given** an antibiotic, a valid measurement, and a breakpoint, **When** the measurement is at
   or below the susceptible threshold, between the thresholds, or above the resistant threshold,
   **Then** the displayed result is susceptible, intermediate, or resistant respectively; a
   breakpoint without an available standard produces "not determined."
3. **Given** a measurement is entered, **When** its antibiotic or breakpoint is missing, or the
   measurement is not on the allowed scale, **Then** the isolate save is rejected with validation
   against the incomplete or invalid E-Test row.
4. **Given** existing E-Test rows, **When** the edit form opens, **Then** those rows are displayed
   with their antibiotic fixed, along with one empty row for another measurement and empty rows for
   configured primary antibiotics not already present.

---

### User Story 3 - Add and retain PubMLST typing data (Priority: P3)

Laboratory staff upload sequence data to the external PubMLST database outside this application.
After PubMLST makes that data available online, the laboratory user can query PubMLST for the
isolate using its prefixed stem number. A successful lookup fills a read-only set of PubMLST
identifiers, typing alleles, sequence type, clonal complex, and vaccine-reactivity values. Saving
the isolate stores a new matched PubMLST record or updates the stored record with the returned
values. The user can open the corresponding external PubMLST entry.

**Why this priority**: PubMLST data enriches Meningococci typing and is unique to this pathogen's
isolate entry, but core analysis results can be recorded without a PubMLST match.

**Independent Test**: Open an isolate with a stem number, run an individual lookup that returns a
match, verify the read-only fields and external-entry link, save, and confirm the matched data is
shown when the isolate is reopened.

**Acceptance Scenarios**:

1. **Given** an isolate with a prefixed stem number, **When** an individual PubMLST query finds a
   record, **Then** the form displays a success indication, enables a link to the external record,
   and fills database, PubMLST id, sequence type, clonal complex, PorA VR1/VR2, FetA VR, porB,
   fHbp, NHBA, nadA, penA, GyrA, parC, parE, rpoB, rplF, Bexsero reactivity, and Trumenba reactivity
   as read-only values.
2. **Given** a successful PubMLST lookup, **When** the isolate is saved validly, **Then** a matched
   record that is not yet stored is added, or the corresponding stored record is updated with the
   returned values.
3. **Given** no stem number or a lookup with no match, **When** the user queries PubMLST, **Then** a
   not-found indication is displayed and no external-entry link is enabled for a new match.
4. **Given** laboratory staff have uploaded sequence data but PubMLST has not yet made it available
  online, **When** the user queries PubMLST, **Then** no match is returned and the query must be
  repeated after the data becomes available online.
5. **Given** no PubMLST match for an isolate, **When** otherwise valid analysis results are saved,
   **Then** the save succeeds because PubMLST data is optional for isolate entry.

---

### User Story 4 - Save and proceed to reporting (Priority: P4)

After the entered isolate data passes validation, the laboratory user can choose the combined
"save changes and create report" action. The same results are persisted and the user is taken to
report creation for that isolate. The isolate's report status tracks whether no report, a
preliminary report, or a final report has been generated, but that status is set by the reporting
workflow rather than by results entry.

**Why this priority**: Proceeding to reporting is the endpoint of analysis entry, but it depends on
a valid persisted isolate and reporting is specified separately.

**Independent Test**: Submit a validation-passing isolate with the combined save-and-report action;
confirm that the edits persist and report creation opens for the same isolate without an additional
analysis-completeness gate.

**Acceptance Scenarios**:

1. **Given** a validation-passing isolate, **When** the user chooses "save changes and create
   report," **Then** the changes are persisted and report creation opens for that isolate.
2. **Given** an isolate with analyses that remain "not determined" but whose entered values satisfy
   all validation rules, **When** the user chooses the combined action, **Then** the isolate can
   proceed to report creation; there is no separate "analysis complete" state.
3. **Given** an invalid isolate, **When** the user chooses the combined action, **Then** the user
   remains on the edit form and does not proceed to report creation.

---

### Edge Cases

- **Missing or unknown isolate id**: Opening edit without an id returns a bad-request response;
  opening an id that does not identify an isolate returns a not-found response.
- **Laboratory number format**: An empty laboratory number or one not in the form `39/14` (digits,
  slash, two digits) fails validation. A laboratory number beginning with `-` is displayed read-only
  rather than as an editable number.
- **Duplicate identifiers**: A duplicate stem number or laboratory number is rejected and the
  conflict is reported against the corresponding field.
- **Growth-dependent identification**: Unless both agar growth results are explicitly "no,"
  oxidase cannot remain "not determined." When both are "no," oxidase, agglutination, ONPG,
  gamma-GT, and either MALDI-TOF result must remain "not determined"; serogroup PCR, siaA, ctrA,
  and cnl are permitted.
- **Conflicting serogenogroup PCR results**: At most one of csb-PCR, csc-PCR, and cswy-PCR may be
  positive.
- **Conditionally required details**: Positive 16S rDNA requires a best match and percentage;
  a determined MALDI-TOF result requires its best match and confidence; positive Real-Time PCR
  requires at least one RIDOM result.
- **Conditionally displayed molecular fields**: PorA VR1/VR2 are shown for positive porA-PCR,
  FetA VR is shown for positive fetA-PCR, and the cswy allele is shown for positive cswy-PCR.
- **No PubMLST match**: PubMLST typing is optional and its absence does not block saving or
  proceeding to report creation.
- **Uploaded but not yet online**: Sequence data uploaded by laboratory staff cannot be matched
  until PubMLST has made it available online; a lookup during that interval returns no match.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST let a standard laboratory user edit the existing Meningococci isolate
  belonging one-to-one to a sending; this workflow MUST NOT create a standalone isolate.
- **FR-002**: The edit form MUST be pre-filled with current isolate values and MUST display sampling
  location, material, invasive classification, patient id, and patient age at sampling as read-only
  context.
- **FR-003**: The general-results section MUST allow entry of laboratory number, stem number, growth
  on blood agar, growth on Martin-Lewis agar, oxidase, agglutination, gamma-GT, ONPG, serogroup PCR,
  MALDI-TOF Biotyper with best match and confidence, siaA, ctrA, cnl, report date, and a free-text
  remark. Existing MALDI-TOF VITEK MS values and their details MUST be displayed read-only.
- **FR-004**: The molecular-typing section MUST allow entry of rplF; Real-Time PCR result, RIDOM
  evaluation, and device; 16S rDNA result, best match, and match percentage; csb-PCR, csc-PCR,
  cswy-PCR, and cswy allele; porA-PCR with PorA VR1/VR2; and fetA-PCR with FetA VR.
- **FR-005**: The system MUST provide E-Test rows for susceptibility measurements and MUST limit
  selectable antibiotics and breakpoints to those defined for Meningococci, presenting the most
  recently valid breakpoints first.
- **FR-006**: For each E-Test measurement, the system MUST derive susceptible, intermediate,
  resistant, or not-determined status from the chosen Meningococci breakpoint and MUST persist only
  rows that contain an antibiotic and measurement.
- **FR-007**: An E-Test measurement MUST use the allowed scale for its antibiotic and MUST have both
  an antibiotic and a breakpoint; otherwise the isolate save MUST be rejected.
- **FR-008**: The system MUST require a laboratory number matching digits, a slash, and exactly two
  trailing digits, and MUST reject an empty or malformed value.
- **FR-009**: Unless both agar growth results are "no," oxidase MUST have a determined positive or
  negative result.
- **FR-010**: When both agar growth results are "no," oxidase, agglutination, ONPG, gamma-GT, and
  both MALDI-TOF results MUST remain "not determined"; serogroup PCR, siaA, ctrA, and cnl MAY still
  carry results.
- **FR-011**: When both agar growth results are "no" and the sending material is not native
  material, a valid save MUST change the sending material to "no growth"; native-material
  classifications MUST remain unchanged.
- **FR-012**: The system MUST reject an isolate when more than one of csb-PCR, csc-PCR, and cswy-PCR
  is positive.
- **FR-013**: The system MUST require the 16S rDNA best match and percentage when 16S rDNA is
  positive, the relevant best match and confidence when a MALDI-TOF result is determined, and at
  least one RIDOM evaluation when Real-Time PCR is positive.
- **FR-014**: The system MUST reject a stem number or laboratory number already assigned to another
  isolate and MUST associate the validation message with the conflicting field.
- **FR-015**: On any validation failure, the system MUST persist nothing, redisplay the edit form
  with validation messages, and make the tab containing an error visible.
- **FR-016**: On a valid primary save, the system MUST persist the isolate results and return the
  user to the Meningococci sendings list.
- **FR-017**: On a valid combined save-and-report action, the system MUST persist the same results
  and open report creation for the same isolate. Passing results-entry validation is the only
  readiness gate in this feature; the system has no separate analysis-complete state.
- **FR-018**: The isolate MUST carry a report status of none, preliminary, or final. Results entry
  MUST preserve that status, while the reporting workflow remains responsible for changing it.
- **FR-019**: The system MUST allow an individual PubMLST query by the isolate's prefixed stem
  number and MUST display whether a match was found. Matching MUST depend on the externally
  uploaded sequence data already being available online in PubMLST.
- **FR-020**: A successful PubMLST query MUST fill and display as read-only: source database,
  PubMLST id, sequence type, clonal complex, PorA VR1/VR2, FetA VR, porB, fHbp, NHBA, nadA, penA,
  GyrA, parC, parE, rpoB, rplF, Bexsero reactivity, and Trumenba reactivity, and MUST provide a link
  to the matched external entry.
- **FR-021**: On a valid isolate save with a non-zero PubMLST id, the system MUST add the matched
  PubMLST record if absent or update the corresponding stored record with the displayed typing data.
- **FR-022**: PubMLST data MUST remain optional; an absent match MUST NOT prevent valid isolate
  results from being saved or taken to report creation.
- **FR-023**: All isolate entry actions MUST require the standard laboratory-user role, and edit
  submissions MUST include anti-forgery protection.
- **FR-024**: An edit request without an isolate id MUST return a bad-request response, and an id
  for a non-existent isolate MUST return a not-found response.
- **FR-025**: Coordination, scheduling, and assignment of which person performs each analysis MUST
  remain outside this feature; no in-app analysis task planning is provided.

### Key Entities *(include if feature involves data)*

- **Meningococci isolate**: The analysis record belonging one-to-one to a Meningococci sending. It
  carries general identification, culture, serogrouping, molecular typing, E-Test, identifier,
  report-date, report-status, and remark values. It shares common isolate fields and workflow with
  the parallel Haemophilus domain while retaining Meningococci-specific analysis fields.
- **Meningococci sending**: The received specimen record that owns the isolate and supplies the
  read-only material, sampling, invasive, and patient context. Saving a no-growth isolate may change
  a non-native sending's material classification to "no growth."
- **Epsilometer test (E-Test)**: An antibiotic susceptibility measurement associated with the
  isolate, containing an applicable clinical breakpoint, a measurement from the antibiotic's scale,
  and a derived susceptibility result.
- **Meningococci clinical breakpoint**: Authoritative, date-versioned reference data used to offer
  antibiotics and interpret E-Test measurements for this pathogen.
- **Neisseria PubMLST isolate**: Optional typing data matched by the isolate's prefixed stem number,
  including its PubMLST source and id, sequence type, clonal complex, typing alleles, and vaccine
  reactivity values.
- **Report status**: The isolate's downstream reporting state: none, preliminary, or final. It is
  preserved during analysis entry and changed by the separate reporting workflow.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: In 100% of valid edit attempts, an authorized laboratory user can save Meningococci
  analysis results in one form and subsequently reopen the isolate with those values retained.
- **SC-002**: In 100% of invalid edit attempts covered by the stated growth, dependent-result,
  identifier, and E-Test rules, no changes are persisted and a field-level or summary validation
  message is displayed.
- **SC-003**: Every saved E-Test row has an antibiotic, an allowed-scale measurement, a breakpoint,
  and a susceptibility result consistent with that breakpoint.
- **SC-004**: In 100% of valid combined save-and-report attempts, the isolate edits are persisted
  and report creation opens for the same isolate without a separate analysis-completeness step.
- **SC-005**: Every successful individual PubMLST match displays the complete returned typing set
  read-only, provides access to the matched external entry, and is retained after a valid save.
- **SC-006**: A missing PubMLST match never prevents otherwise valid analysis data from being saved
  or taken to report creation.
- **SC-007**: No user can create, assign, or schedule an analysis task within this workflow; all
  such coordination continues outside the system.

## Assumptions

- **Status-quo interpretation**: Requirements describe existing observable behavior and are not
  proposals for changed validation, workflow, or data collection.
- **Isolate lifecycle**: Patient-and-sending intake (A2-M) creates the isolate with its sending;
  this feature edits it and provides no separate create or delete operation.
- **Reporting boundary**: This feature records report inputs and offers navigation to report
  creation. Interpretation, template selection, preliminary/final generation, and report-status
  transitions belong to A5-M and A4-M.
- **Readiness meaning**: "Ready for reporting" means the isolate edit passes the implemented
  validation and saves successfully. It does not mean that every available analysis has a result.
- **PubMLST boundary**: Individual lookup and persistence visible on the edit form are included.
  Laboratory staff upload sequence data directly to PubMLST outside this application, and matching
  becomes possible only after PubMLST makes the data available online. Date-range batch matching
  and remote matching policy are specified under D1.
- **Shared legacy abstraction**: Common isolate fields and edit behavior are shared with the
  Haemophilus isolate feature, while pathogen-specific fields remain in parallel domain models as
  recorded in the constitution's legacy constraints.
- **Role terminology**: "Standard laboratory user" means the application's default user role,
  which protects all actions in this isolate-entry controller.
