# Feature Specification: Sender export

**Feature Branch**: `speckit-init`

**Created**: 2026-10-22

**Status**: Draft (status-quo inventory)

**Input**: User description: "Feature title: \"Sender export\" (backlog ref B3, Group B — Data exports & epidemiology). Document the current, as-implemented behavior of the sender (Einsender) export feature as it exists today, which generates an Excel file of senders for a selected date range from a column/field definition. Read HaemophilusWeb/Controllers/SenderControllerBase.cs (Export, CreateExportDefinition, SendersMatchingExportQuery), HaemophilusWeb/Tools/ExportDefinition.cs, HaemophilusWeb/Tools/CreateExcelFile.cs, and the Sender and MeningoSender Export views. Capture the date-range query, the exported columns, which roles can run it, and that it applies to both the Haemophilus (Sender) and Meningococci (MeningoSender) databases via the shared SenderControllerBase. Document observable behavior only — do not propose changes."

> **Scope note**: This is an *as-is* specification (backlog ref B3). It records the current, observable behavior of
> the two "Einsender Export" entries of the Export menu — one under Haemophilus, one under Meningokokken. Both are
> served by one shared sender implementation and differ only in which pathogen's submissions they evaluate and in the
> pathogen name used in the page title and file name, so one spec covers both. The export lists senders (not
> submissions) that had at least one submission in a chosen date range, with their contact data and per-sender
> submission count, stem numbers, and laboratory numbers. It does **not** propose changes. It reuses the generic
> spreadsheet mechanism specified in spec 011 (B1) — date-range form, column definition, single-sheet download — with
> a sender-specific column set; shared mechanism properties are referenced, not re-derived. Out of scope: sender
> maintenance (create, edit, delete, restore, list; spec 001, A1), the submission/isolate exports (spec 011, B1),
> duplicate-patient handling and anonymization (spec 012, B2; not applied here), and how stem and laboratory numbers
> are assigned (specs 004/005).

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Export the senders active in a period (Priority: P1)

A Standard User opens "Einsender Export" under Haemophilus or Meningokokken, accepts or changes the pre-filled date range, and downloads a spreadsheet with one row per sender that had at least one non-deleted submission of that pathogen in the range, showing the sender's contact data plus how many submissions it sent and which stem and laboratory numbers they received.

**Why this priority**: This is the whole purpose of the feature: a period-based list of active senders (for example for annual reporting, mailings, or acknowledgements) with traceability to the isolates they sent.

**Independent Test**: Create several senders; give them Haemophilus and Meningococci submissions inside and outside a test range, some with and some without a sampling date, some soft-deleted, and soft-delete one sender that has submissions in the range. Run both sender exports for the range and compare rows and cell contents with the stored data.

**Acceptance Scenarios**:

1. **Given** a Standard User selects "Einsender Export" under Haemophilus (or Meningokokken), **When** the page opens without a date range, **Then** a page titled "Export der Einsender (Haemophilus)" (or "Export der Einsender (Meningokokken)") shows the heading "Entnahmedatum der Einsendung(en)", the fields "Start-Datum" and "Ende-Datum" pre-filled with 1 January and 31 December of the previous calendar year, and an "Exportieren nach Excel" button.
2. **Given** a start and end date are submitted, **When** the export runs, **Then** a spreadsheet named `Einsender-Haemophilus-Export_<start yyyyMMdd>-<end yyyyMMdd>.xlsx` (or `Einsender-Meningokokken-Export_<start yyyyMMdd>-<end yyyyMMdd>.xlsx`) is downloaded.
3. **Given** a submission of the export's pathogen has a sampling date, **When** selection runs, **Then** it counts as "in the range" exactly when its sampling date lies within start and end date (both inclusive); **Given** it has no sampling date, **Then** its receiving date is used instead.
4. **Given** a sender has at least one non-deleted submission of the export's pathogen in the range and the sender itself is not deleted, **When** the export runs, **Then** the sender appears exactly once, regardless of how many submissions it has.
5. **Given** a sender has no non-deleted submission of the export's pathogen in the range, **or** the sender is soft-deleted, **When** the export runs, **Then** the sender does not appear.
6. **Given** a sender row is produced, **Then** its columns are, in this order: "Einsendernummer", "Name", "Abteilung", "Straße", "Postleitzahl", "Ort", "Telefon #1", "Telefon #2", "Fax", "E-Mail", "Bemerkung", "Anzahl Einsendungen", "Stammnummern", "Labornummern".
7. **Given** a sender row is produced, **Then** "Anzahl Einsendungen" is the number of that sender's submissions of the export's pathogen in the range, "Stammnummern" lists their stem numbers without pathogen prefix, comma-separated without spaces, with `-` for a submission whose isolate has no stem number, and "Labornummern" lists their laboratory numbers with the pathogen prefix (`KL` for Haemophilus, `MZ` for Meningococci), comma-separated without spaces, in the same submission order as "Stammnummern".

---

### User Story 2 - Export per pathogen from one shared sender directory (Priority: P2)

The sender directory is shared by both pathogens (spec 001). The user runs the sender export separately for Haemophilus and for Meningococci; each export lists the same sender records but selects and counts only that pathogen's submissions.

**Why this priority**: The per-pathogen scoping is what distinguishes the two menu entries; a sender active in both workflows must be counted correctly in each.

**Independent Test**: Give one sender two Haemophilus submissions and one Meningococci submission in the range and a second sender only a Meningococci submission; run both exports.

**Acceptance Scenarios**:

1. **Given** a sender has submissions in the range in both pathogens, **When** each export runs, **Then** the sender appears in both files with identical contact columns, and each file's count, stem numbers, and laboratory numbers refer only to that file's pathogen.
2. **Given** a sender has submissions in the range in only one pathogen, **When** the other pathogen's export runs, **Then** the sender does not appear in it.
3. **Given** a sender's contact data was changed after the submissions were recorded, **When** the export runs, **Then** the current contact data is exported; no historic sender data is kept or shown.

---

### User Story 3 - Reach the sender export according to role (Priority: P3)

Only users with the Standard User role can run the sender exports; the navigation offers them to Standard User and Administrator accounts.

**Why this priority**: The export contains the contact data of all active senders and the identifiers of their isolates; access control determines who can extract it.

**Independent Test**: Log in with a Standard User, a Public Health, and an Administrator-only account; compare the offered menu entries with the outcome of calling both sender export addresses directly.

**Acceptance Scenarios**:

1. **Given** a Standard User, **When** the navigation is shown, **Then** the Export menu lists "Einsender Export" under both Haemophilus and Meningokokken, and both exports can be run.
2. **Given** a Public Health (`RKI`) user, **When** the navigation is shown, **Then** no sender export is offered, and addressing either sender export directly is refused.
3. **Given** an account holding only the Administrator role, **When** the navigation is shown, **Then** both "Einsender Export" entries are offered, but running either export is refused.
4. **Given** an unauthenticated visitor addresses a sender export, **Then** the visitor is required to log in.

### Edge Cases

- A request without a start date always shows the form pre-filled with the previous calendar year, even if an end date was supplied; an unparseable start date behaves the same way and shows no error message (same as spec 011).
- A start date later than the end date, or a missing end date, is not rejected; the download contains only the header row. The same holds when no sender had a submission in the range.
- The end date is compared as a calendar date at the start of the day; for date-only values the end day is included (same assumption as spec 011).
- The page heading reads "Entnahmedatum der Einsendung(en)" (sampling date), although submissions without a sampling date are matched by their receiving date.
- Sender rows are not explicitly sorted; their order is whatever the database returns. Within a row, stem and laboratory numbers are listed in the order the submissions are returned, which is likewise not explicitly defined, but the two lists are positionally aligned. [POTENTIAL-REFACTORING]
- A sender is selected only through non-deleted submissions, but "Anzahl Einsendungen", "Stammnummern", and "Labornummern" are computed over all of that sender's submissions in the range, including soft-deleted ones. A selected sender's count and number lists can therefore include deleted submissions; a sender whose only submissions in the range are deleted is not listed at all. [SUSPECTED-DEFECT]
- The range matching is performed twice, once to select senders and once to compute the per-sender values, with diverging deletion handling (the cause of the previous item). [POTENTIAL-REFACTORING]
- "Anzahl Einsendungen" is written as a text cell rather than a number, because the generic spreadsheet writer treats only certain numeric types as numbers and the count is not one of them; "Einsendernummer" is written as a number. [SUSPECTED-DEFECT]
- Stem numbers are exported without the pathogen prefix (`H` / `DE`) while laboratory numbers carry their prefix (`KL` / `MZ`); a Meningococci laboratory number entered with a leading `-` is shown as `NR` followed by the number.
- A submission without an isolate record would make the export fail with an error page instead of producing a file (every submission is normally created together with its isolate; see Assumptions).
- The sender selection loads the patient of every matching submission although no patient data is used or exported. [POTENTIAL-REFACTORING]
- The export contains no patient data; it does contain sender contact data (names, departments, addresses, phone, fax, e-mail, free-text remark) and isolate identifiers. No duplicate cleaning or anonymization (spec 012) is applied.
- The sender's soft-delete flag is not exported (deleted senders never appear).
- The export form embeds an anti-forgery token, but the export accepts any request method and does not verify the token (same as spec 011). [POTENTIAL-REFACTORING]
- If writing the spreadsheet fails, the failure is only traced and the user still receives the (empty or partial) temporary file as a normal download, with no error shown; failures while assembling the rows surface as an error page (same as spec 011). [SUSPECTED-DEFECT]
- Each export writes a temporary file on the server that is not deleted afterwards, so exported sender contact data remains in the server's temporary directory (relevant to Constitution Principle IV; same as spec 011). [POTENTIAL-REFACTORING]
- The single worksheet is not given an explicit name (same as spec 011). [POTENTIAL-REFACTORING]
- Both sender exports are reachable for Administrator-only accounts through the menu but refused by authorization (accounts are registered with exactly one role; same pattern as spec 011). [SUSPECTED-DEFECT]
- The sender export (as well as the per-sender aggregation) lives in the shared sender controller rather than in a service, and the two pathogen variants exist as parallel controllers over the same shared sender directory (Constitution, "Known Architectural Constraints": parallel domains and god-controllers). [POTENTIAL-REFACTORING]
- No automated test covers the sender export; the existing sender controller tests cover only the delete warning.

## Requirements *(mandatory)*

### Functional Requirements

#### Form and query

- **FR-001**: The system MUST offer a sender export per pathogen (Haemophilus, Meningococci), each reachable at its own address and from the respective section of the Export menu as "Einsender Export".
- **FR-002**: When no start date is supplied, the export MUST show a form titled "Export der Einsender (Haemophilus)" or "Export der Einsender (Meningokokken)" with the heading "Entnahmedatum der Einsendung(en)", "Start-Datum" and "Ende-Datum" fields in `DD.MM.YYYY` format pre-filled with 1 January and 31 December of the previous calendar year, and an "Exportieren nach Excel" button.
- **FR-003**: When a start date is supplied, the system MUST consider a submission of the export's pathogen "in the range" when its sampling date lies within start and end date inclusive or, if it has no sampling date, when its receiving date lies within that range.
- **FR-004**: The system MUST NOT validate that the start date precedes the end date and MUST NOT persist any record of an export run.

#### Selection and content

- **FR-005**: The export MUST contain exactly one row per sender that is not soft-deleted and has at least one non-deleted submission of the export's pathogen in the range; no other senders MUST appear.
- **FR-006**: Each row MUST contain, in order: sender number ("Einsendernummer"), "Name", "Abteilung", "Straße", "Postleitzahl", "Ort", "Telefon #1", "Telefon #2", "Fax", "E-Mail", "Bemerkung", taken from the sender's current record, with empty cells for missing values.
- **FR-007**: Each row MUST then contain "Anzahl Einsendungen" (number of the sender's submissions of the export's pathogen in the range), "Stammnummern" (their isolates' stem numbers without prefix, `-` when absent), and "Labornummern" (their isolates' laboratory numbers with pathogen prefix), the two lists comma-separated without spaces and in the same submission order.
- **FR-008**: The per-sender values of FR-007 MUST, as currently implemented, be computed over all of the sender's submissions in the range including soft-deleted ones (see Edge Cases).
- **FR-009**: The export MUST NOT include patient data and MUST NOT apply duplicate cleaning or anonymization.

#### Output

- **FR-010**: The system MUST return a single-worksheet spreadsheet (`.xlsx`) with a header row, named `Einsender-<Pathogen>-Export_<start yyyyMMdd>-<end yyyyMMdd>.xlsx`, where `<Pathogen>` is `Haemophilus` or `Meningokokken`, produced by the generic export mechanism of spec 011.

#### Access

- **FR-011**: Both sender exports MUST be permitted for the Standard User (`Standardbenutzer`) role only; Public Health and Administrator-only accounts MUST be refused, and unauthenticated visitors MUST be asked to log in.
- **FR-012**: The navigation MUST offer both sender exports to Standard User and Administrator roles and none to the Public Health role.
- **FR-013**: This feature MUST document current behavior only and MUST NOT introduce new selection, content, or access rules.

### Column Catalogue

| # | Header | Content | Cell type |
| --- | --- | --- | --- |
| 1 | Einsendernummer | Sender number | Number |
| 2 | Name | Sender name | Text |
| 3 | Abteilung | Department | Text |
| 4 | Straße | Street and house number | Text |
| 5 | Postleitzahl | Postal code | Text |
| 6 | Ort | City | Text |
| 7 | Telefon #1 | Primary phone | Text |
| 8 | Telefon #2 | Secondary phone | Text |
| 9 | Fax | Fax | Text |
| 10 | E-Mail | E-mail address | Text |
| 11 | Bemerkung | Free-text remark | Text |
| 12 | Anzahl Einsendungen | Number of the sender's submissions of this pathogen in the range (including soft-deleted ones) | Text (see Edge Cases) |
| 13 | Stammnummern | Stem numbers without prefix, comma-separated; `-` for none | Text |
| 14 | Labornummern | Laboratory numbers with `KL`/`MZ` prefix (`NR` for Meningococci numbers entered with leading `-`), comma-separated | Text |

### Roles and Boundaries

| Actor or role | Current observable involvement |
| --- | --- |
| Standard User (`Standardbenutzer`) | Sees both "Einsender Export" entries and can run both exports. |
| Public Health (`RKI`) | Sees no sender export; is refused both when addressing them directly. |
| Administrator | Sees both entries in the Export menu but is refused unless the account also holds the Standard User role. |
| Unauthenticated visitor | Is required to log in; no sender export is publicly reachable. |

### Key Entities *(include if feature involves data)*

- **Export query**: Start date and end date; no further options.
- **Sender**: The row subject, from the directory shared by both pathogens (spec 001). Supplies sender number, name, department, street, postal code, city, two phone numbers, fax, e-mail, remark, and the soft-delete state used for exclusion.
- **Submission (sending)**: Of the export's pathogen only. Supplies sampling and receiving dates for range matching, the sender reference, and the soft-delete state (used for sender selection but not for the per-sender values).
- **Isolate**: Linked to each submission; supplies the stem number and the prefixed laboratory number.
- **Export file**: The downloaded single-sheet spreadsheet; not retained or logged as a business record by the system.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: For both sender exports, 100% of non-deleted senders with at least one non-deleted submission of the pathogen in a test range appear exactly once, and 0 other senders appear.
- **SC-002**: In 100% of test runs, a submission with a sampling date is matched by that date and a submission without one by its receiving date, with both range ends inclusive.
- **SC-003**: For every exported sender, the count equals the number of entries in its stem-number list and in its laboratory-number list, and equals the number of its submissions of that pathogen in the range (including soft-deleted ones, as currently implemented).
- **SC-004**: Every downloaded file name matches `Einsender-Haemophilus-Export_<start>-<end>.xlsx` or `Einsender-Meningokokken-Export_<start>-<end>.xlsx`, and every file has exactly the 14 columns of the Column Catalogue in that order.
- **SC-005**: For each of the 3 roles, the observed menu entries and allowed/refused outcomes for both sender exports match the Roles and Boundaries table in 100% of the 6 role/export combinations.
- **SC-006**: Opening either sender export without a date range pre-fills the full previous calendar year in 100% of cases.

## Assumptions

- Current application code and views are the source of truth for this inventory; no production data or deployment configuration was inspected, and no automated test of the sender export exists to confirm behavior.
- Sampling and receiving dates are stored as date-only values, so the end date of the range is effectively inclusive (same assumption as spec 011).
- Sender rows and the order of numbers within a row follow the database's default return order, which in practice is assumed to be ascending sender number and ascending submission identifier respectively; this is not guaranteed by the implementation.
- Every submission has an isolate record, because isolates are created together with submissions (specs 002–005); the failure behavior for a submission without one was derived from the code, not exercised.
- A refused request for an authenticated user is assumed to lead back to the login page (standard behavior of the authentication setup; same assumption as spec 011).
- Whether an Administrator account can also hold the Standard User role was not established beyond the observation that registration assigns exactly one role (same as spec 011).
- Inclusion of soft-deleted submissions in the per-sender values is recorded as observed behavior; whether it is intended is left to review.
- Generic mechanism properties (date parsing, form defaults, error handling, temporary files, worksheet naming) were verified to use the same shared code path as the exports in spec 011 and are therefore stated consistently with it.
