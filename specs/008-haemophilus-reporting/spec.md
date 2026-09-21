# Feature Specification: Haemophilus reporting (preliminary + final)

**Feature Branch**: `008-haemophilus-reporting`

**Created**: 2026-09-21

**Status**: Draft (status-quo inventory)

**Input**: User description: "Feature title: \"Haemophilus reporting (preliminary + final)\" (backlog ref A4-H, Group A - Core laboratory workflow). Document the current, as-implemented behavior of generating Haemophilus laboratory reports for an isolate, covering preliminary and final reports, report status and report date, roles, outcomes, external signing, and client-side document generation. Document observable behavior only; do not propose changes."

> **Scope note**: This specification records the current observable Haemophilus report-generation
> workflow (backlog ref A4-H) without proposing corrections or redesign. It covers entry from
> isolate editing, report preview, preliminary/final selection, document download, and the resulting
> report status and date. Clinical interpretation rules, report-template authoring, configuration
> management, health-office maintenance, Meningococci reporting, and signing are out of scope except
> where their current outputs are consumed by this workflow. Report signing takes place outside the
> system.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Generate a final Haemophilus report (Priority: P1)

A laboratory user saves a Haemophilus isolate and opens report creation. The user reviews a summary
of the isolate, chooses an available report template and configured signer, and generates a dated,
downloadable final report document containing the isolate's prepared report data.

**Why this priority**: The final document is the completed reporting outcome and is the path that
records the isolate as finally reported.

**Independent Test**: Starting with an existing isolate whose report status is either none,
preliminary, or final, select a non-preliminary template and signer, generate the document, and
verify its download name, report data, final status, and newly assigned report date.

**Acceptance Scenarios**:

1. **Given** a Standard User has saved valid isolate edits using the save-and-create-report action,
   **When** the save succeeds, **Then** the report-creation page opens for that isolate.
2. **Given** the report-creation page is open, **When** it loads, **Then** it shows an isolate
   summary, final and preliminary interpretations, applicable susceptibility results, available
   report templates, configured signer choices, and the responsible health-office details when
   those details can be obtained.
3. **Given** a non-preliminary template and a signer are selected and the selected interpretation
   is not discrepant, **When** the user requests report creation, **Then** a Word-compatible report
   document is populated and offered for download.
4. **Given** the final document has been rendered and its download initiated, **When** generation
   is reported to the system, **Then** the isolate status becomes Final and the report date is set
   to the current date and time.
5. **Given** an isolate already has a final report date, **When** another final report is generated,
   **Then** the status remains Final and the report date is replaced with the new generation time.

---

### User Story 2 - Generate a preliminary Haemophilus report (Priority: P2)

A laboratory user can select a template identified as preliminary to produce quick report
information before final analysis is reported. This path uses the preliminary interpretation and
records a preliminary state without assigning a final report date.

**Why this priority**: Preliminary reporting supports an earlier communication point while
preserving a clear distinction from the final report outcome.

**Independent Test**: Generate a report with a template whose name contains the configured
preliminary marker, first for an unreported isolate and then for a final isolate, and verify the
document path and state transitions.

**Acceptance Scenarios**:

1. **Given** the selected template name contains the configured preliminary marker, **When** report
   creation evaluates the report type, **Then** it treats the document as preliminary and uses the
   preliminary interpretation when checking whether the results are discrepant.
2. **Given** an isolate is not Final, **When** a preliminary document is rendered and its download
   is initiated, **Then** the isolate status becomes Preliminary and no report date is assigned.
3. **Given** an isolate is already Final, **When** a preliminary document is subsequently generated,
   **Then** its status remains Final and its existing report date is unchanged.
4. **Given** a preliminary template is populated, **When** the document is rendered, **Then** the
   selected template controls which supplied isolate fields appear and the preliminary
   interpretation is available in place of final analysis interpretation.

---

### User Story 3 - Resolve report-generation prerequisites and warnings (Priority: P3)

Before producing either report type, the user must select both a template and a signer. If the
interpretation relevant to the selected report type is marked discrepant, the user must explicitly
choose whether to continue with a report proposal.

**Why this priority**: These checks determine whether document generation proceeds and prevent an
unnoticed discrepant interpretation from being included.

**Independent Test**: Attempt generation with each missing selection and with discrepant final and
preliminary interpretations, then verify that no document is generated until the required choices
and explicit confirmation are supplied.

**Acceptance Scenarios**:

1. **Given** no template or no signer is selected, **When** the user requests report creation,
   **Then** an error asks for both selections and no document is generated.
2. **Given** the interpretation corresponding to the selected report type contains the discrepancy
   marker, **When** the user requests report creation, **Then** the system asks whether a report
   proposal should nevertheless be produced.
3. **Given** the discrepancy prompt is shown, **When** the user declines, **Then** the prompt closes
   and no document is generated.
4. **Given** the discrepancy prompt is shown, **When** the user explicitly continues, **Then**
   generation proceeds with the current report data and selected signer.

---

### User Story 4 - Download a populated report for external signing (Priority: P4)

The generated document is downloaded to the user's device. The selected signer appears as report
data, but the system neither applies nor records a signature; signing is completed outside the
system.

**Why this priority**: Download is the handoff from in-system report preparation to the external
signing process.

**Independent Test**: Generate a report using an isolate with typing values and an optional QR
image, then inspect the downloaded document and confirm that no signed state is stored.

**Acceptance Scenarios**:

1. **Given** report data includes typing attributes, **When** a document is generated, **Then** each
   typing value is available to the template under its named typing attribute.
2. **Given** report data includes a reportable QR image, **When** a document is generated, **Then**
   the image is available to the template at the fixed report-image size.
3. **Given** the document renders successfully, **When** download starts, **Then** its filename uses
  the `KL` prefix followed by a space, the isolate laboratory number with slashes replaced by underscores, the current date in
   `YYYY-MM-DD` form, and the `.docx` extension.
4. **Given** a signer was selected, **When** generation completes, **Then** the signer is included in
   the document data but is not persisted as a report signature or signing event.

### Edge Cases

- Opening report creation without an isolate identifier returns a bad-request outcome; using an
  identifier that does not resolve to an isolate returns a not-found outcome.
- If no report templates exist, the template list communicates that none are stored and report
  generation cannot pass the required-selection check.
- Preliminary classification depends only on the selected template path containing the configured
  preliminary marker. A selected template without that marker follows the final path.
- A document-template loading or rendering error stops the flow before the report-generation
  notification, so this path does not update status or report date.
- The download request is initiated before the report-generation notification. The system records
  generation without confirming that the user retained, opened, printed, or externally signed the
  downloaded file.
- A report-generation notification for an unknown or omitted isolate identifier returns a
  successful acknowledgement while changing no isolate.
- Repeated final generation replaces the prior report date; repeated preliminary generation does
  not assign or change a report date unless the isolate was already Final, in which case the
  existing date is preserved.
- The date supplied inside report data and the date used in the filename reflect the generation
  day; the persisted final report date records the later notification time and includes time of day.
- Responsible health-office lookup is ancillary to report generation. A missing or failed lookup
  does not itself block template selection or document generation.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST allow a Standard User to enter report creation after successfully
  saving Haemophilus isolate edits through the save-and-create-report action.
- **FR-002**: Report routes MUST require an authenticated account. The current report controller
  MUST apply no report-specific role restriction beyond that authentication requirement; therefore
  any authenticated role can address those routes directly, although the normal edit-to-report
  entry path is restricted to the Standard User role and normal laboratory navigation is shown to
  Standard User and Administrator roles.
- **FR-003**: Report creation MUST reject a missing isolate identifier as a bad request and an
  unknown isolate identifier as not found.
- **FR-004**: For an existing isolate, the system MUST prepare a report view containing isolate,
  patient, submission, interpretation, susceptibility, typing, configured contact, comment,
  announcement, and optional QR-image data available on that isolate's report model.
- **FR-005**: The report page MUST display an isolate summary and MUST offer all available
  Haemophilus Word-compatible templates plus all configured report signers.
- **FR-006**: The report page MUST request responsible health-office details using the patient's
  postal code and display returned address, telephone, fax, and email details; successful retrieval
  MUST enable the health-office edit link.
- **FR-007**: The system MUST require both a report template and a signer before generation and MUST
  show the same missing-selection error if either is absent.
- **FR-008**: The system MUST classify a selected template as preliminary exactly when its path
  contains the configured preliminary marker; every other selected template MUST be final.
- **FR-009**: For a final template, generation MUST inspect the final interpretation for the
  discrepancy marker. For a preliminary template, it MUST inspect the preliminary interpretation.
- **FR-010**: If the selected interpretation contains the discrepancy marker, the system MUST
  require an explicit continue decision before creating a report proposal; declining MUST create
  no document and MUST cause no report state transition.
- **FR-011**: Before rendering, the system MUST add the selected signer, expose each typing value by
  its typing attribute name, and make an available QR image usable by the report template.
- **FR-012**: The system MUST preserve line breaks while populating the selected template and MUST
  offer the resulting Word-compatible document for local download.
- **FR-013**: The downloaded filename MUST be `KL <laboratory-number>-<generation-date>.docx`, with
  slashes in the laboratory number replaced by underscores and the date formatted `YYYY-MM-DD`.
- **FR-014**: The report document's displayed report date MUST be the current generation date in the
  report's day-month-year format.
- **FR-015**: After rendering and initiating the download, the system MUST notify the report record
  of generation. It MUST NOT wait for confirmation that the user kept, opened, printed, or signed
  the file.
- **FR-016**: A final generation notification for an existing isolate MUST set status to Final and
  report date to the current date and time, replacing any prior report date.
- **FR-017**: A preliminary generation notification for an existing isolate not already Final MUST
  set status to Preliminary and MUST NOT assign a report date.
- **FR-018**: A preliminary generation notification for an existing Final isolate MUST preserve
  both Final status and the existing report date.
- **FR-019**: A generation notification for a missing or unknown isolate MUST acknowledge the
  request without changing report status or date.
- **FR-020**: Report-list status MUST distinguish None, Preliminary, and Final visually as not
  generated, pending/preliminary, and completed/final respectively.
- **FR-021**: The selected signer MUST be used only as document data. The system MUST NOT perform,
  verify, or persist report signing; signing occurs outside the system.
- **FR-022**: The system MUST NOT persist a generated document as part of this workflow; its durable
  in-system outcome is the isolate's report status and, for final generation, report date.
- **FR-023**: This feature MUST preserve the current preliminary and final reporting behavior as
  documented and MUST NOT introduce new clinical interpretation, document content, approval, or
  signing rules.

### Report State Transitions

| Starting state | Generated report type | Resulting state | Report date outcome |
| --- | --- | --- | --- |
| None | Preliminary | Preliminary | Remains unset |
| Preliminary | Preliminary | Preliminary | Remains unset |
| Final | Preliminary | Final | Existing value is preserved |
| None | Final | Final | Set to current date and time |
| Preliminary | Final | Final | Set to current date and time |
| Final | Final | Final | Replaced with current date and time |
| Any state or no isolate | Notification has unknown/missing identifier | Existing data unchanged | Existing data unchanged |

### Roles and Boundaries

| Actor or role | Current observable involvement |
| --- | --- |
| Standard User (`Standardbenutzer`) | Can edit a Haemophilus isolate and use the normal save-and-create-report entry path. |
| Administrator | Sees normal Haemophilus navigation; report routes themselves do not add an Administrator-specific restriction. |
| Public Health (`RKI`) | Does not receive the normal Haemophilus editing/reporting navigation, but the report routes have no role check beyond global authentication. |
| Configured report signer | Is selected by the user and inserted as report data; does not sign within the system and is not recorded as an acting account. |
| External signer/process | Completes signing after download, outside this feature and outside system tracking. |

### Key Entities *(include if feature involves data)*

- **Haemophilus isolate**: The report subject. It supplies the laboratory result data and stores
  report status plus the nullable final report date.
- **Report data**: A generated snapshot of isolate, patient, submission, interpretation,
  susceptibility, typing, configured contact, and optional QR-image values used to populate a
  document. Its report date is the current generation date.
- **Report template**: An available Word-compatible document definition. Its path determines
  preliminary versus final classification through the configured preliminary marker, and its
  placeholders determine which supplied report data appears.
- **Report signer choice**: A configured signer name selected for inclusion in document data. It is
  not a stored signature or approval record.
- **Report status**: The isolate's durable reporting state: None, Preliminary, or Final.
- **Report date**: A nullable isolate timestamp assigned or refreshed only by final generation.
- **Downloaded report**: The populated local document handed off for external signing. The system
  does not retain it in this workflow.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: In 100% of tested successful final generations, the isolate ends in Final status and
  its report date reflects that generation, including when a previous final date existed.
- **SC-002**: In 100% of tested preliminary generations for non-final isolates, the isolate ends in
  Preliminary status and no report date is assigned.
- **SC-003**: In 100% of tested preliminary generations for already-final isolates, Final status and
  the existing report date remain unchanged.
- **SC-004**: For both report types, generation is blocked in 100% of tests where either template or
  signer is missing, and discrepant output never proceeds without an explicit continue decision.
- **SC-005**: Every successfully rendered report is offered with the documented filename pattern,
  current report date, selected signer, and template-selected report content.
- **SC-006**: Across all three authenticated roles, observed direct-route access matches the
  documented global-authentication boundary, while the normal edit-to-report path remains available
  only to Standard Users.
- **SC-007**: Inspection of generated records finds zero persisted document files, signatures, or
  signing events; only report status and applicable final report date are retained by this workflow.

## Assumptions

- The current application behavior, report view, configuration, and automated controller tests are
  the source of truth for this status-quo inventory.
- "Preliminary" means the template path contains the configured preliminary marker and the
  preliminary interpretation is selected for discrepancy checking. The template itself determines
  which supplied fields are omitted; the workflow does not remove final-analysis fields from the
  report data before template population.
- "Document generated" means document rendering completed and the local-download action was
  initiated. The subsequent status notification is asynchronous and has no user-visible success
  confirmation in the report page.
- Configured templates and signer names already exist and are maintained outside this workflow.
- The responsible health-office lookup is supporting context, not a prerequisite for generation.
- Report signing, approval, delivery to recipients, and retention of signed documents are external
  processes and are not observable or recorded here.
- Meningococci reporting follows a separate path and is outside this specification.
