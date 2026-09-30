# Feature Specification: Meningococci reporting (preliminary + final)

**Feature Branch**: `speckit-init`

**Created**: 2026-09-30

**Status**: Draft (status-quo inventory)

**Input**: User description: "Feature title: \"Meningococci reporting (preliminary + final)\" (backlog ref A4-M, Group A - Core laboratory workflow). Document the current, as-implemented behavior of generating Meningococci laboratory reports for an isolate, covering both a preliminary report and a final report, how report status and report date are set, status transitions, roles, outcomes, and external signing. Document observable behavior only; do not propose changes."

> **Scope note**: This specification records the current observable Meningococci report-generation workflow (backlog ref A4-M) without proposing corrections or redesign. It covers entry from isolate editing and the sending list, report preview, preliminary/final classification, document download, and resulting report status and date. Clinical interpretation rules, template authoring, configuration management, health-office maintenance, report delivery, and signing are out of scope except where their current outputs are consumed by this workflow. Report signing takes place outside the system.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Generate a final Meningococci report (Priority: P1)

A laboratory user opens report creation for a Meningococci isolate, reviews the prepared result summary, selects an available report template and configured signer, and generates a downloadable final report document.

**Why this priority**: Final generation produces the completed laboratory-report document and records the isolate as finally reported.

**Independent Test**: Starting with an existing isolate in each report state, select a template not identified as preliminary and a signer, generate the document, and verify its filename, populated data, Final status, and newly assigned report date.

**Acceptance Scenarios**:

1. **Given** a Standard User saves valid isolate edits using the save-and-create-report action, **When** the save succeeds, **Then** report creation opens for that isolate.
2. **Given** an existing isolate, **When** report creation opens, **Then** the page shows its report summary, prepared report text, applicable susceptibility results, available Meningococci templates, configured signer choices, and responsible health-office details when available.
3. **Given** a non-preliminary template and signer are selected, **When** the user requests report creation and document rendering succeeds, **Then** a populated Word-compatible document is offered for download.
4. **Given** final document rendering succeeds and download is initiated, **When** generation is reported to the system, **Then** the isolate status becomes Final and its report date is set to the current date and time.
5. **Given** an isolate already has a final report date, **When** another final report is generated, **Then** status remains Final and the report date is replaced with the new generation time.

---

### User Story 2 - Generate a preliminary Meningococci report (Priority: P2)

A laboratory user selects a template identified as preliminary and generates a downloadable preliminary report before final reporting. This records a preliminary state without assigning a final report date.

**Why this priority**: Preliminary reporting supports an earlier communication point while keeping it visibly distinct from final reporting.

**Independent Test**: Generate a report with a template whose path contains the configured preliminary marker, first for a non-final isolate and then for a final isolate, and verify the download and state outcomes.

**Acceptance Scenarios**:

1. **Given** the selected template path contains the configured preliminary marker, **When** report type is determined, **Then** the document is classified as preliminary.
2. **Given** an isolate is None or Preliminary, **When** preliminary rendering succeeds and download is initiated, **Then** its status becomes or remains Preliminary and no report date is assigned.
3. **Given** an isolate is already Final, **When** a preliminary document is subsequently generated, **Then** its Final status and existing report date remain unchanged.
4. **Given** a preliminary template is populated, **When** it renders, **Then** the same prepared report-data object is supplied as for final generation and the selected template determines which values appear in the preliminary document.

---

### User Story 3 - Select report inputs and review prepared data (Priority: P3)

The report page lets the user inspect the isolate's prepared reporting data and requires both a template and signer before generation. Meningococci report creation does not present a discrepancy confirmation, even when prepared report text describes discrepant results.

**Why this priority**: These are the current prerequisites and review behavior immediately before either document outcome.

**Independent Test**: Open report creation for representative isolates, verify the displayed and omitted data, attempt generation with either selection missing, and verify that discrepant text does not trigger a confirmation step.

**Acceptance Scenarios**:

1. **Given** no template or no signer is selected, **When** the user requests generation, **Then** one error asks for both selections and no document is generated.
2. **Given** susceptibility results include Azithromycin, **When** report creation opens, **Then** that antibiotic is excluded from the report model while other applicable measured susceptibility results remain available.
3. **Given** prepared report text describes discrepant results, **When** the user has selected a template and signer and requests generation, **Then** generation proceeds without a discrepancy confirmation.
4. **Given** no Meningococci templates are stored, **When** the page opens, **Then** the template choice indicates that no templates are available and generation cannot pass the required-selection check.

---

### User Story 4 - Download for external signing and observe status (Priority: P4)

The generated document is downloaded to the user's device. The selected signer is included as document data, but no signature or signing event is applied or stored. The sending list separately shows whether no report, a preliminary report, or a final report has been recorded.

**Why this priority**: Download is the handoff from in-system report preparation to the external signing process, while report status is the durable in-system outcome visible to users.

**Independent Test**: Generate a report with typing values and optional QR data, inspect the downloaded document and persisted isolate, and verify the sending-list status indicator and absence of an in-system signed state.

**Acceptance Scenarios**:

1. **Given** report data includes typing attributes, **When** a document is generated, **Then** each typing value is available to the template under its named typing attribute.
2. **Given** report data includes a reportable QR image, **When** a document is generated, **Then** the image is available to the template at the report's fixed image size.
3. **Given** rendering succeeds, **When** download starts, **Then** the filename uses the `MZ` prefix, the laboratory number with slashes replaced by underscores, the generation date in `YYYY-MM-DD` form, and the `.docx` extension.
4. **Given** a signer is selected, **When** generation completes, **Then** the signer is included in document data but no signature, signed status, or signing event is persisted.
5. **Given** an isolate has a report status, **When** the Meningococci sending list is shown, **Then** None, Preliminary, and Final are displayed with distinct not-created, pending, and completed indicators.
6. **Given** a Standard User opens Meningococci isolate editing, **When** the reporting fields are shown, **Then** report status is carried without an editable status control and report date is available as an editable calendar date displayed in `DD.MM.YYYY` form.

### Edge Cases

- Opening report creation without an isolate identifier returns a bad-request outcome; an identifier that does not resolve to an isolate returns not found.
- Preliminary classification depends only on whether the selected template path contains the configured preliminary marker. Any selected template without that marker follows the final path.
- The current stored Meningococci templates include one template whose name contains the default preliminary marker (`Teilbefund -`); the other stored templates follow the final path.
- A template loading or rendering error stops the flow before generation is reported, so status and report date are not updated by that attempt.
- Download is initiated before generation is reported. The system records generation without confirming that the user retained, opened, printed, delivered, or externally signed the file.
- A generation notification with a missing or unknown isolate identifier returns a successful acknowledgement while changing no isolate.
- Repeated final generation replaces the prior report date. Repeated preliminary generation does not assign a report date; after Final, preliminary generation preserves the final date.
- The report data's displayed date and the filename date reflect the generation day. The persisted final report date records the later notification time and includes time of day.
- Health-office lookup is ancillary. Missing or failed health-office data does not itself block document generation.
- If an isolate's sender cannot be resolved, sender address fields are not populated, but report creation otherwise continues with the available data.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST allow a Standard User to enter Meningococci report creation from the sending list or after successfully saving isolate edits through the save-and-create-report action.
- **FR-002**: Report routes MUST require an authenticated account. They MUST apply no report-specific role restriction beyond global authentication; therefore any authenticated role can address those routes directly, although the normal sending-list and isolate-edit paths are restricted to the Standard User role and normal laboratory navigation is shown to Standard User and Administrator roles.
- **FR-003**: Report creation MUST reject a missing isolate identifier as a bad request and an unknown isolate identifier as not found.
- **FR-004**: For an existing isolate, the system MUST prepare report data from the isolate, patient, submission, interpreted report lines, typing values, applicable measured susceptibility results, configured report information, and optional reportable QR data.
- **FR-005**: The report page MUST display laboratory number, sampling location, patient, invasive classification, agglutination, serogroup PCR, applicable susceptibility results, and prepared report lines available for the isolate.
- **FR-006**: Azithromycin susceptibility results MUST be removed from the report model before the page and document data are produced.
- **FR-007**: The report page MUST offer Word-compatible templates found in the Meningococci report-template set and signer names from report configuration.
- **FR-008**: The report page MUST request responsible health-office details using the patient's postal code and display returned address, telephone, fax, and email details; successful retrieval MUST enable the health-office edit link.
- **FR-009**: The system MUST require both a report template and signer before generation and MUST show the same missing-selection error if either is absent.
- **FR-010**: The system MUST classify a template as preliminary exactly when its selected path contains the configured preliminary marker; all other selected templates MUST be classified as final.
- **FR-011**: Meningococci report generation MUST NOT require or show a discrepancy confirmation; after the two required selections are present, discrepant prepared text MUST follow the same generation path as other prepared text.
- **FR-012**: Before rendering, the system MUST add the selected signer, expose each typing value by its typing attribute name, and make an available QR image usable by the selected template.
- **FR-013**: The system MUST supply the prepared report-data object to the selected template, preserve line breaks, and offer the rendered Word-compatible document for local download. The template MUST determine which supplied values appear.
- **FR-014**: The downloaded filename MUST be `MZ <laboratory-number>-<generation-date>.docx`, with slashes in the laboratory number replaced by underscores and the date formatted `YYYY-MM-DD`.
- **FR-015**: The report data's current-date value MUST use the report's day-month-year display format at the time the report page data is prepared.
- **FR-016**: After rendering and initiating download, the system MUST asynchronously report generation. It MUST NOT wait for confirmation that the user kept, opened, printed, delivered, or signed the file.
- **FR-017**: A final generation notification for an existing isolate MUST set status to Final and report date to the current date and time, replacing any prior report date.
- **FR-018**: A preliminary generation notification for an existing isolate not already Final MUST set status to Preliminary and MUST NOT assign a report date.
- **FR-019**: A preliminary generation notification for an existing Final isolate MUST preserve both Final status and the existing report date.
- **FR-020**: A generation notification for a missing or unknown isolate MUST acknowledge the request without changing report status or date.
- **FR-021**: The sending list MUST distinguish None, Preliminary, and Final visually as no report created, preliminary/pending, and final/completed respectively.
- **FR-022**: The selected signer MUST be used only as document data. The system MUST NOT perform, verify, or persist signing; signing occurs outside the system.
- **FR-023**: The system MUST NOT persist the generated document in this workflow. Its durable report-generation outcome is report status and, for final generation, report date.
- **FR-024**: The isolate edit form MUST preserve report status as a non-visible value and MUST expose report date to a Standard User as an editable calendar-date field displayed in `DD.MM.YYYY` form.
- **FR-025**: This feature MUST document the current reporting behavior only and MUST NOT introduce new interpretation, content, approval, delivery, or signing rules.

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
| Standard User (`Standardbenutzer`) | Can use the normal sending-list and isolate-edit paths, create reports, and edit the isolate's report date. |
| Administrator | Sees normal laboratory navigation; report routes themselves do not add an Administrator-specific restriction. |
| Public Health (`RKI`) | Does not receive normal isolate-edit/reporting navigation, but direct report routes have no role check beyond global authentication. |
| Configured report signer | Is selected by the user and inserted as report data; does not sign within the system and is not recorded as an acting account. |
| External signer/process | Completes signing after download, outside this feature and outside system tracking. |

### Key Entities *(include if feature involves data)*

- **Meningococci isolate**: The report subject. It supplies laboratory-result data and stores report status plus the nullable report date.
- **Prepared report data**: The isolate, patient, submission, interpreted report lines, typing, susceptibility, configured report information, optional QR data, current display date, and selected signer supplied to a template.
- **Report template**: An available Word-compatible document definition. Its path determines preliminary versus final classification through the configured marker, while its placeholders determine which supplied values appear.
- **Report signer choice**: A configured signer name selected for inclusion in document data. It is not a stored signature or approval record.
- **Report status**: The isolate's durable reporting state: None, Preliminary, or Final.
- **Report date**: A nullable isolate date/time assigned or refreshed by final generation and exposed on the isolate edit form as a day-granular editable value.
- **Downloaded report**: The populated local document handed off for external signing. The system does not retain it in this workflow.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: In 100% of tested successful final generations, the isolate ends in Final status and its report date reflects that generation, including when a previous final date existed.
- **SC-002**: In 100% of tested preliminary generations for non-final isolates, the isolate ends in Preliminary status and no report date is assigned.
- **SC-003**: In 100% of tested preliminary generations for already-final isolates, Final status and the existing report date remain unchanged.
- **SC-004**: For both report types, generation is blocked in 100% of tests where either template or signer is missing, while discrepant prepared text requires zero additional confirmation steps.
- **SC-005**: Every successfully rendered report is offered with the documented filename pattern, selected signer, and content determined by the selected template from the prepared report data.
- **SC-006**: In 100% of inspected Meningococci report models, Azithromycin is absent while other applicable measured susceptibility results remain available.
- **SC-007**: Across all three authenticated roles, observed direct-route access matches the documented global-authentication boundary, while normal editing and report-entry actions remain restricted as documented.
- **SC-008**: Inspection of generated records finds zero persisted document files, signatures, or signing events; only report status and the applicable report date are retained by generation.

## Assumptions

- Current application behavior, report templates, configuration defaults, and automated tests of the shared report-status behavior are the source of truth for this status-quo inventory.
- "Preliminary" means the selected template path contains the configured preliminary marker. The workflow supplies the same prepared data object to preliminary and final templates; template placeholders determine the visible document content.
- "Document generated" means document rendering completed and the local-download action was initiated. The subsequent status notification is asynchronous and has no user-visible success confirmation on the report page.
- The default preliminary marker is `Teilbefund -`, and the currently stored Meningococci template bearing that marker is the fax preliminary template. Deployments may provide a different configured marker or template set without changing the workflow.
- Configured templates, signer names, directors, contacts, and announcements are maintained outside this workflow.
- Health-office lookup is supporting context rather than a prerequisite for generation.
- Report signing, approval, delivery to recipients, and retention of signed documents are external processes and are not observable or recorded here.
- Haemophilus reporting follows a separate path and is outside this specification.
