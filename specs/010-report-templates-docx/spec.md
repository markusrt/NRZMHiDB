# Feature Specification: Report templates (.docx)

**Feature Branch**: `speckit-init`

**Created**: 2026-10-22

**Status**: Draft (status-quo inventory)

**Input**: User description: "Feature title: \"Report templates (.docx)\" (backlog ref A6, Group A — Core laboratory workflow). Document the current, as-implemented behavior of how report templates are selected and rendered for reports, as it exists today. Read HaemophilusWeb/Controllers/ReportControllerBase.cs (template listing via FileLister and ReportTemplates path) and the ReportTemplates/ directory structure. Capture how templates are discovered and chosen at report time. Document observable behavior only — do not propose changes."

> **Scope note**: This specification records the current observable report-template mechanism (backlog ref A6) shared by Haemophilus and Meningococci reporting, without proposing corrections or redesign. Per backlog decision D5 it is a standalone spec, because the template mechanism may later move to a different system, and it covers both pathogens' template sets in one spec because discovery and rendering are shared. In scope: where templates are stored and how they are organized; how the available templates are discovered for each pathogen; how a template is chosen at report time; how preliminary versus final is derived from the chosen template; how the chosen template is fetched and filled with report data in the browser; which data the templates consume; how templates are authored, versioned, and shipped. Out of scope and referenced instead: report status/date transitions, signer selection, discrepancy confirmation, filenames, and health-office lookup (see `specs/008-haemophilus-reporting` and `specs/009-meningococci-reporting`); the clinical interpretation that produces report text (see `specs/006-haemophilus-result-interpretation` and `specs/007-meningococci-result-interpretation`); configuration maintenance of directors, contacts, and signers.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Choose a report template for an isolate (Priority: P1)

A laboratory user opening report creation for an isolate is offered every report template currently stored for that pathogen and picks the one that matches the case (for example, a resistance profile, "no growth", "not invasive", a fax variant, or a preliminary report). The system does not pre-select or suggest a template; the choice is made entirely by the user.

**Why this priority**: Template choice determines both the content of the generated report and whether it counts as preliminary or final, so it is the central decision of the template mechanism.

**Independent Test**: Open report creation for a Haemophilus isolate and for a Meningococci isolate and compare the offered template names with the files stored in the respective template location.

**Acceptance Scenarios**:

1. **Given** a Haemophilus isolate, **When** report creation opens, **Then** the template choice lists exactly the Word documents stored directly in the Haemophilus template location (currently 21), each shown by its file name including the `.docx` extension, and does not list the Meningococci templates or the authoring fragments stored in sub-folders.
2. **Given** a Meningococci isolate, **When** report creation opens, **Then** the template choice lists exactly the Word documents stored directly in the Meningococci template location (currently 5).
3. **Given** report creation has opened, **When** the user looks at the template choice, **Then** no template is pre-selected; the choice shows a "select" placeholder (`Auswählen...`) until the user picks one.
4. **Given** a template file has been added to or removed from the deployed template location, **When** report creation is opened again, **Then** the offered list reflects the change without any other configuration step.
5. **Given** the template location for a pathogen is missing or contains no Word documents, **When** report creation opens, **Then** only the placeholder entry is offered and generation cannot pass the required-selection check.

---

### User Story 2 - Render the chosen template into a report document (Priority: P2)

After choosing a template (and a signer, see specs 008/009), the user requests report creation. The browser retrieves the chosen template, fills its placeholders with the isolate's prepared report data, and offers the filled document for download.

**Why this priority**: Rendering turns the chosen template into the actual report document handed off for external signing.

**Independent Test**: Choose a template that contains each kind of placeholder (simple value, repeated section, conditional section, image), generate the document, and verify each placeholder was replaced with the corresponding prepared value.

**Acceptance Scenarios**:

1. **Given** a template is chosen, **When** report creation is requested, **Then** the template file is freshly retrieved from the server (never from a browser or server cache) and filled in the browser; the filled document is not sent back to or stored on the server.
2. **Given** a template contains value placeholders, **When** it is filled, **Then** each placeholder is replaced by the prepared report-data value of the same name, and line breaks inside values are preserved as line breaks in the document.
3. **Given** a template contains a repeated section for a list (for example susceptibility results, typing results, or Meningococci report lines), **When** it is filled, **Then** the section is emitted once per list entry.
4. **Given** a template contains a conditional section (for example comment/announcement or QR image), **When** it is filled, **Then** the section appears only when the corresponding value is present or true.
5. **Given** the prepared data contains a reportable QR image and the template contains the image placeholder, **When** it is filled, **Then** the image is inserted at a fixed size of 100 x 100 and not centered.
6. **Given** the template cannot be retrieved or a placeholder cannot be processed, **When** report creation is requested, **Then** no document is offered and the report status is not updated; no in-page error message is shown to the user.

---

### User Story 3 - Preliminary versus final is derived from the chosen template (Priority: P3)

Whether a generated report is treated as preliminary or final is not chosen separately: it follows from the chosen template's location string containing the configured preliminary marker (default `Teilbefund - `).

**Why this priority**: This coupling makes template naming a functional, not just cosmetic, decision; its downstream status effects are documented in specs 008/009.

**Independent Test**: Choose the Haemophilus `Fax - Teilbefund - Serotyp v4.docx` and the Meningococci `Fax - Teilbefund - v2.docx` templates and verify they are classified as preliminary; choose any other stored template and verify it is classified as final.

**Acceptance Scenarios**:

1. **Given** the chosen template's location string contains the configured marker, **When** report creation is requested, **Then** the report is classified as preliminary.
2. **Given** the chosen template's location string does not contain the marker, **When** report creation is requested, **Then** the report is classified as final.
3. **Given** the currently stored template sets, **When** they are classified with the default marker, **Then** exactly one Haemophilus template and exactly one Meningococci template are preliminary, and all others are final.

---

### User Story 4 - Maintain the template sets (Priority: P4)

Template creation and editing is a developer task. A developer edits the Word documents using the placeholder syntax of the browser-side filling component, versions them through the file name, and ships them as part of an application release; the application has no in-app template upload, editing, or administration.

**Why this priority**: This describes how the template sets come to exist and change, which matters for any later move of the mechanism to another system, but it is not part of the per-report workflow.

**Independent Test**: Inspect the repository, the project's content list, and a published build to confirm which template files are shipped, and confirm that no screen allows template upload or editing.

**Acceptance Scenarios**:

1. **Given** a developer adds a new template file, **When** the application is built for publishing, **Then** the file is shipped only if it is also registered as content of the web project; registered files are shipped into the published template location.
2. **Given** a template is revised, **When** it is released, **Then** the version is expressed in the file name (for example `v10`, `v3`, `v2`), and that name, including the version, is what users see in the template choice.
3. **Given** templates were changed in a release, **When** users view the application's change log, **Then** such releases are announced there with entries such as "Update Befundvorlagen".
4. **Given** a user with any role, **When** they use the application, **Then** no function exists to upload, replace, edit, rename, or delete report templates.

### Edge Cases

- Discovery looks only at the top level of each pathogen's template location. The Meningococci set lives in a sub-folder of the Haemophilus location and is therefore never offered for Haemophilus; the shared authoring fragments in the `includes` sub-folder are never offered for either pathogen.
- Discovery accepts any entry whose name ends in `.docx`, including temporary or lock files a word processor might leave in the folder, and offers them as templates if present. [POTENTIAL-REFACTORING]
- The offered order is the order in which the file system enumerates the folder; no explicit sort is applied. [POTENTIAL-REFACTORING]
- The page code contains a "no templates stored" label (`Keine Vorlagen hinterlegt`), but it is only applied to individual list entries that are absent, which never occurs; an empty template location therefore shows only the `Auswählen...` placeholder and never this label. The statement in specs 008/009 that the list "communicates that none are stored" does not match this observed behavior. [SUSPECTED-DEFECT]
- The preliminary marker is matched against the full template location string (application path, folder, and file name), not the file name alone. A deployment path or folder name containing the marker would classify every template of that set as preliminary. [POTENTIAL-REFACTORING]
- Template retrieval and processing failures are not shown to the user: a retrieval failure is raised without a message, and a processing failure is only written to the browser's developer console before being raised. The user sees that no download occurred but receives no explanation. [SUSPECTED-DEFECT]
- The complete prepared report data for the isolate is supplied to every template; the template alone decides which values appear. Values not referenced by a template are silently ignored. No handling for absent values is configured, so the filling component's default applies; according to the component's documentation this renders the literal text `undefined` for a simple placeholder whose value is missing or null (not verified against generated documents).
- The browser-side preparation adds one flattened value per typing attribute (`Typing-<Attribute>`), but no currently stored template references these values; all templates use the repeated typing section instead. [POTENTIAL-REFACTORING]
- The image size is fixed at 100 x 100 for every image placeholder in every template, independent of the template.
- Template files embed the shared fragments (header, footer, signature block, first page, fax cover sheet, fax footnote) as linked includes that point to absolute paths on a developer workstation (`D:\Development\NRZMHiDB\HaemophilusWeb\ReportTemplates\...`). These include fragments are not registered as web-project content and are not shipped. Rendering uses the include content stored in the template at authoring time; if a user's word processor refreshes the linked fields in a downloaded report, the links cannot resolve on other machines. [POTENTIAL-REFACTORING]
- Fax variants of Haemophilus templates embed the corresponding non-fax template as an include plus the fax cover sheet and fax footnote, so a change to a base template reaches its fax variant only after the developer refreshes the fax variant's includes. [POTENTIAL-REFACTORING]
- The browser-side filling components are loaded from public content-delivery networks on every application page, not only on report pages. Report rendering therefore depends on those external sources being reachable from the user's browser; two of the four component files are loaded without an integrity check. [POTENTIAL-REFACTORING]
- The filling component is invoked through its older programming interface (load-then-set-data style), which the component's maintainers document as deprecated. [POTENTIAL-REFACTORING]
- The template-selection, classification, and rendering logic exists as two near-identical copies, one per pathogen report page; the copies differ only in pathogen-specific details (whether the discrepancy check is active, the filename prefix, and the isolate-edit link). [POTENTIAL-REFACTORING]

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST store report templates as Word-compatible (`.docx`) files in the application's template location: Haemophilus templates at the top level of `ReportTemplates/`, Meningococci templates in `ReportTemplates/Meningo/`, and shared authoring fragments in `ReportTemplates/includes/`.
- **FR-002**: Each time a report-creation page is opened, the system MUST discover the available templates by listing the entries with the `.docx` extension located directly (non-recursively) in the pathogen's template location. It MUST NOT cache this list between page requests.
- **FR-003**: If the pathogen's template location does not exist, discovery MUST yield an empty list without an error.
- **FR-004**: The system MUST discover templates even when the report-creation page is then rejected for a missing or unknown isolate; discovery does not depend on the isolate.
- **FR-005**: The report-creation page MUST offer each discovered template as one choice, labelled with its file name (including extension) and carrying the template's application-relative web location as its value, preceded by the placeholder `Auswählen...`, with no default selection.
- **FR-006**: The system MUST NOT pre-select, suggest, filter, or restrict templates based on isolate data, interpretation results, report status, or user role; template choice MUST be a free manual selection among all discovered templates of the pathogen.
- **FR-007**: The system MUST make the configured preliminary marker (setting `PreliminaryReportMarker`, default `Teilbefund - `) available to the report-creation page and MUST classify the chosen template as preliminary exactly when its web location string contains the marker; all other chosen templates MUST be classified as final.
- **FR-008**: On report-creation request, the browser MUST retrieve the chosen template from its web location with a random cache-busting parameter, and the web server's output caching MUST be disabled for `.docx` files.
- **FR-009**: The system MUST fill the retrieved template in the browser with the isolate's complete prepared report data, extended in the browser by the selected signer (`Signer`), one value per typing attribute (`Typing-<Attribute>`), and, when a reportable QR image is present, the decoded image (`DemisIdQrImage`).
- **FR-010**: Template filling MUST support value placeholders, repeated sections over lists, conditional sections, and image placeholders, and MUST preserve line breaks contained in values.
- **FR-011**: Image placeholders MUST be rendered at a fixed size of 100 x 100, not centered.
- **FR-012**: The filled document MUST be produced only in the browser and offered for local download; the template mechanism MUST NOT upload, persist, or archive the filled document. (Filename and status update after download: see specs 008/009.)
- **FR-013**: If template retrieval or filling fails, the system MUST NOT offer a document and MUST NOT send the report-generation notification; it currently shows no user-facing error message for this case.
- **FR-014**: The system MUST provide no in-application function to upload, edit, replace, rename, or delete templates; template changes MUST arrive only through an application release.
- **FR-015**: A template file MUST be shipped with a published release only when it is registered as content of the web project; currently all 26 pathogen templates are registered and the 9 `includes` fragments are not.
- **FR-016**: This feature MUST document the current template mechanism only and MUST NOT introduce new template-selection, rendering, or authoring rules.

### Current Template Inventory

| Set (location) | Templates offered to users | Preliminary (default marker) |
| --- | --- | --- |
| Haemophilus (`ReportTemplates/`) | 21: `BLNAR v10`, `BLNAS v10`, `BLPACR v10`, `BLPACS v10`, `Kein Wachstum v1`, `Nicht Invasiv v1`, `Sonstige - Ampi-S Cipro-R v3`, `Sonstige - Ampi-S Imi-R v3`, `Sonstige - Kein Haemophilus v3`, `Sonstige - lowBLNAR CTX-R Imi-R v3`, each of these ten also as a `Fax - ...` variant, plus `Fax - Teilbefund - Serotyp v4` (all `.docx`) | `Fax - Teilbefund - Serotyp v4` only |
| Meningococci (`ReportTemplates/Meningo/`) | 5: `Nativmaterial v2`, `Untersuchungsbefund v2`, `Fax - Nativmaterial v2`, `Fax - Untersuchungsbefund v2`, `Fax - Teilbefund - v2` (all `.docx`) | `Fax - Teilbefund - v2` only |
| Shared fragments (`ReportTemplates/includes/`) | 0 (never offered): `Fax - Deckblatt`, `Fax - Fußnote`, `Fußzeile`, `Kopfzeile - Kontakt`, `Kopfzeile - Kurz`, `Seite 1`, `Seite 1 - Keine Resistenz`, `Seite 1 - Kurz`, `Signature` | n/a |

The Haemophilus set encodes the reportable result in the template itself (one template per resistance profile or special outcome, chosen by the user), whereas the Meningococci set is small because the case-specific wording is produced by the interpretation and supplied as report lines that a single generic template repeats.

### Template Data Consumed

| Data group | Used by | Observed placeholders (examples) |
| --- | --- | --- |
| Identification and dates | Both sets | Laboratory number (with and without prefix), sender laboratory number, patient, patient birth date, sampling date and location, receiving date, current date |
| Sender address | Both sets | Sender name, department, street, city |
| Result text | Haemophilus | Final interpretation, preliminary interpretation (preliminary template only), interpretation disclaimer, evaluation, agglutination, beta-lactamase text |
| Result text | Meningococci | Repeated report-line section |
| Susceptibility | Resistance-profile templates (both sets) | Repeated section with antibiotic, measurement, result, susceptible/resistant breakpoints, breakpoint year |
| Typing | Most templates (both sets) | Repeated section with attribute and value |
| Comment and announcement | Both sets | Conditional sections for comment and announcement |
| Report configuration | Both sets (headers, footers, signature) | Laboratory director, medical director, contacts, selected signer |
| QR image | All pathogen templates | Conditional image section |

### Roles and Boundaries

| Actor or role | Current observable involvement |
| --- | --- |
| Laboratory user on the report-creation page | Chooses one template per report from the offered list; cannot add, change, or remove templates. Access to the report pages themselves is described in specs 008/009. |
| Developer | Authors and versions templates in a word processor using the filling component's placeholder syntax, maintains shared fragments, registers files as project content, and ships them in a release. |
| Deployment/operations | Receives templates only as part of the published application; may override the preliminary marker through application settings. |

### Key Entities *(include if feature involves data)*

- **Report template**: A Word-compatible document file in a pathogen's template location. Its file name is its user-visible label and carries its version; its web location string determines preliminary versus final; its placeholders determine which prepared data appear.
- **Template location**: A per-pathogen folder whose direct `.docx` entries form that pathogen's template set; fixed per pathogen in the application (Haemophilus default location, Meningococci sub-folder).
- **Template fragment (include)**: A shared Word document (header, footer, signature, first page, fax cover, fax footnote) embedded into templates at authoring time; never offered, not shipped.
- **Preliminary marker**: A configurable text (default `Teilbefund - `) whose presence in a template's location string classifies the template as preliminary.
- **Prepared report data**: The isolate's complete report view data plus browser-added signer, flattened typing values, and decoded QR image, supplied unchanged to whichever template is chosen.
- **Filled report document**: The browser-generated result of filling a template, offered for download and not retained by the system.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: For each pathogen, the offered template list matches the set of `.docx` entries directly in that pathogen's template location in 100% of checks, including immediately after a file is added or removed.
- **SC-002**: 0 shared fragments and 0 templates of the other pathogen appear in either template list.
- **SC-003**: With the default marker, exactly 1 of 21 Haemophilus templates and exactly 1 of 5 Meningococci templates are classified as preliminary, and 100% of the others as final.
- **SC-004**: In 100% of successful generations, every placeholder of the chosen template that refers to a present prepared value shows that value, repeated sections appear once per list entry, and line breaks in values are preserved.
- **SC-005**: Inspection after any number of generations finds 0 filled report documents stored by the system.
- **SC-006**: 0 application functions exist that allow any role to upload, edit, replace, or delete a report template.
- **SC-007**: In 100% of tests where the template cannot be retrieved or filled, no document is offered and the isolate's report status and date are unchanged.

## Assumptions

- Current application code, the stored template files, the web project's content list, the published build output, the default settings, and the automated tests of template discovery are the source of truth for this status-quo inventory.
- Technology names are kept out of the requirements for readability; for traceability, the browser-side filling component is currently docxtemplater 3.42.2 with a locally bundled image module, the zip and download helpers are PizZip 3.0.6 and FileSaver 2.0.0, and all are loaded by the shared layout page. Discovery is implemented by a shared file-listing helper used by the shared report controller base.
- The rendering of missing or null values as `undefined` is taken from the filling component's documented default behavior and was not reproduced against a generated document; reviewers should confirm whether any current template placeholder can receive a missing value.
- The statement that the deprecated interface is used is based on the component's public documentation for the 3.x line; it does not currently cause observable failures.
- The observation that templates rely on the include content stored at authoring time (rather than resolving includes at render time) is inferred from the fragments not being shipped and from the filling component working only on the template file itself; this is consistent with reports being generated successfully in production.
- Whether the template files' web location is covered by the application's authentication for direct access was not established from the reviewed code (templates are retrieved as static web content); the files contain placeholders and fixed letterhead text, not patient data.
- The file-system enumeration order on the production server is assumed to be effectively alphabetical (typical for the server's file system), which is why users currently see a stable order despite the absence of an explicit sort.
- The planned reduction of the Haemophilus template count noted in the backlog is a future intention and is not reflected in current behavior.
- Constitution v1.1.0 "Known Architectural Constraints (Legacy Debt)" shapes this feature: the mechanism is shared through one report controller base, but the selection and rendering page logic is duplicated per pathogen (parallel Meningococci and Haemophilus domains), and discovery is wired manually inside the controller rather than through an injected service (god-controllers with weak dependency injection).
