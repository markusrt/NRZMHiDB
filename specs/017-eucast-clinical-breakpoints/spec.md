# Feature Specification: EUCAST clinical breakpoints management

**Feature Branch**: `speckit-init`

**Created**: 2026-10-22

**Status**: Draft (status-quo inventory)

**Input**: User description: "Feature title: \"EUCAST clinical breakpoints management\" (backlog ref C3, Group C — Reference data & administration). Document the current, as-implemented behavior of managing EUCAST antibiotic clinical breakpoints (Antibiotika Grenzwerte) as it exists today. Read HaemophilusWeb/Controllers/EucastClinicalBreakpointsController.cs, the EucastClinicalBreakpoint model/validator, and related Views. Capture CRUD, validity dates, and how breakpoints are treated as authoritative reference data. Document observable behavior only — do not propose changes."

> **Scope note**: This is an *as-is* specification (backlog ref C3). It records the current, observable behavior of
> the clinical breakpoint list ("Antibiotika Grenzwerte" in the Administration menu, page title "Grenzwerte"): listing,
> creating, and editing breakpoint records, the hidden details and delete pages, the validation rules, the meaning of
> version and "Gültig ab" (valid from) and "Gültig für" (pathogen), the "ohne Grenzwerte" (no breakpoints) records,
> and how the records act as reference data for the rest of the application. It also summarises, at a high level, where
> breakpoints are consumed: the E-Test section of isolate entry, the report documents, and the exports. It does
> **not** propose changes. Out of scope and referenced instead: the E-Test entry form and the derivation of the
> susceptibility result (`specs/004-haemophilus-isolate-entry`, `specs/005-meningococci-isolate-entry`); report pages
> and report documents (`specs/008-haemophilus-reporting`, `specs/009-meningococci-reporting`,
> `specs/010-report-templates-docx`); export columns (`specs/011-data-exports-generic`); the one-time legacy data
> importers, which also assign breakpoints to imported measurements but are not part of the running application.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Review the breakpoint list (Priority: P1)

A Standard User opens Administration → "Antibiotika Grenzwerte" to see which antibiotics and which breakpoint versions
are available for E-Test interpretation in the Haemophilus and Meningococci databases.

**Why this priority**: The list is the entry point to all breakpoint maintenance and the only place where the
reference data that drives E-Test interpretation, reports, and exports can be inspected.

**Independent Test**: With a Standard User account, open the menu entry and verify the columns, paging, sorting,
search, and that each row offers only "Bearbeiten"; with an Administrator-only and a Public Health account, verify
that the page is refused.

**Acceptance Scenarios**:

1. **Given** a Standard User or an Administrator account, **When** the Administration menu is shown, **Then** it
   offers the entry "Antibiotika Grenzwerte"; a Public Health (`RKI`) account is not offered the entry.
2. **Given** a Standard User selects the entry, **When** the page "Grenzwerte" is shown, **Then** it lists all stored
   breakpoint records of both pathogens in one table with the columns "Antibiotikum", "Antibiotikum Details",
   "Version", "Gültig für", "Gültig ab", "Sensibel <=", "Resistent >" and a "Bearbeiten" button per row, and offers
   the button "Neuen Grenzwert anlegen" above the table.
3. **Given** the list, **When** it is displayed, **Then** it shows eight rows per page, has a search field, is
   initially sorted by "Version" descending (as text), offers no page-size choice, and remembers paging, sorting, and
   search in the browser.
4. **Given** the list, **When** values are displayed, **Then** "Antibiotikum" is shown with its German display name,
   while "Gültig für" is shown with the internal pathogen identifier ("Haemophilus", "Meningococci", "None") rather
   than the German labels used on the form, and "Gültig ab" is shown as a date with time of day.
5. **Given** the list, **When** a row is displayed, **Then** no links to the details or delete page are offered.
6. **Given** an account holding only the Administrator role, or a Public Health account, **When** the list or any
   other breakpoint page is requested, **Then** the request is refused, because the breakpoint pages admit only the
   Standard User role (`Standardbenutzer`).

---

### User Story 2 - Add a new breakpoint version or a new antibiotic (Priority: P1)

When EUCAST publishes new breakpoints, or when the laboratory starts testing an additional antibiotic, a Standard
User creates a new breakpoint record for the antibiotic and the pathogen. From then on the antibiotic and the new
record are offered in the E-Test section of that pathogen's isolate form, newest validity first.

**Why this priority**: Creating records is how breakpoint versions are kept current (the application's change history
records "Hinzufügen der EUCAST Grenzwerte" for 2015, 2016, and 2017) and how a new antibiotic becomes selectable at
all (change history 2016-05-13: only antibiotics created in the administrative area are offered).

**Independent Test**: Create a record for an antibiotic not yet in the list with "Gültig für" = Meningokokken and all
four breakpoint fields filled; open a Meningococci isolate and verify the antibiotic and the record (labelled
`<details> - v<version> vom <date>`) are offered and that an entered measurement is interpreted against it; verify the
antibiotic is not offered for Haemophilus isolates.

**Acceptance Scenarios**:

1. **Given** the list, **When** the user selects "Neuen Grenzwert anlegen", **Then** the page "Grenzwert anlegen"
   offers "Antibiotikum" (choice of all antibiotics known to the application, ordered by German name, without an
   empty choice), "Antibiotikum Details" (placeholder "Meropenem (meningitis)"), "Version" (placeholder "1.0"),
   "Gültig für" (choice "Haemophilus", "Meningokokken", "Allgemein"), "Gültig ab" (date picker), "Sensibel <="
   (placeholder "0,5"), and "Resistent >" (placeholder "0,5"), with the buttons "Neu anlegen" and "Zurück zur Liste".
2. **Given** valid input, **When** "Neu anlegen" is selected, **Then** the record is stored and the list "Grenzwerte"
   is shown.
3. **Given** "Antibiotikum Details" is empty, **When** the user saves, **Then** the form is shown again with a
   validation message and nothing is stored.
4. **Given** some but not all of "Version", "Gültig ab", "Sensibel <=", and "Resistent >" are filled, **When** the user
   saves, **Then** the form is shown again with the message "Die Felder 'Version', 'Gültig ab', 'Sensibel <=' und
   'Resistent >' müssen entweder alle gefüllt oder alle leer sein." and nothing is stored.
5. **Given** "Antibiotikum Details" is longer than 128 characters or "Version" longer than 64 characters, **When** the
   user saves, **Then** the form is shown again with a validation message and nothing is stored.
6. **Given** all four of "Version", "Gültig ab", "Sensibel <=", and "Resistent >" are left empty, **When** the user
   saves, **Then** the record is stored as an "ohne Grenzwerte" record: it is offered as `<details> - ohne Grenzwerte`
   and every measurement interpreted against it is classified "not determined" (see specs 004 and 005).
7. **Given** all four fields are filled, **When** the record is stored, **Then** it is offered in the E-Test section
   as `<details> - v<version> vom <dd. MMM. yy of Gültig ab>`; a measurement greater than "Resistent >" is classified
   resistant, a measurement at or below "Sensibel <=" susceptible, and any other measurement intermediate (see specs
   004 and 005).
8. **Given** a record whose antibiotic had no record yet for the chosen pathogen, **When** an isolate form of that
   pathogen is opened afterwards, **Then** the antibiotic becomes selectable in the E-Test section and, if it is one
   of the pathogen's configured standard antibiotics, its standard E-Test row appears.
9. **Given** a record with the same antibiotic, "Antibiotikum Details", "Gültig für", and "Version" as an existing
   record (including two "ohne Grenzwerte" records with the same antibiotic, details, and pathogen), **When** the user
   saves, **Then** no validation message is shown; the store is rejected by the database and the generic error page
   is shown instead of the form.
10. **Given** the user has changed a field on the create or edit page, **When** the user tries to leave the page
    without saving, **Then** the browser asks for confirmation.

---

### User Story 3 - Correct an existing breakpoint record (Priority: P2)

A Standard User notices a typing error in a breakpoint record and corrects it via "Bearbeiten". The change applies in
place to the existing record and therefore to every E-Test measurement that already refers to it.

**Why this priority**: Editing is the only maintenance action offered for existing records; because records are
shared by all measurements that use them, edits have effects on historical data that users must be aware of.

**Independent Test**: Choose a record already used by a saved E-Test; change "Resistent >" and "Gültig ab"; open the
report data of that isolate and verify that the new threshold and year are shown next to the unchanged stored
susceptibility result.

**Acceptance Scenarios**:

1. **Given** the list, **When** the user selects "Bearbeiten", **Then** the page "Grenzwert bearbeiten" shows the
   same fields as the create page, filled with the stored values, with the buttons "Änderungen speichern" and "Zurück
   zur Liste"; it offers no delete button.
2. **Given** valid changes, **When** "Änderungen speichern" is selected, **Then** all fields of the record are
   overwritten and the list is shown; the same validation as on the create page applies, including the database
   rejection of a duplicate combination (generic error page).
3. **Given** a record that is already used by stored E-Test measurements, **When** it is edited, **Then** no warning
   is shown, no previous version is kept, and the change is not logged.
4. **Given** an edited "Sensibel <=", "Resistent >", or "Gültig ab", **When** a report of an isolate with a stored
   measurement against this record is created later, **Then** the report shows the new threshold values and the year
   of the new "Gültig ab", while the stored susceptibility result of the measurement is not recalculated.
5. **Given** an edited "Antibiotikum", **When** isolates, reports, or exports that contain measurements against this
   record are shown later, **Then** those measurements appear under the new antibiotic.
6. **Given** an edited "Gültig für", **When** an isolate of the previous pathogen with a measurement against this
   record is opened, **Then** the record is no longer offered for that measurement's row (derived; see Assumptions).
7. **Given** the edit page is requested without a record identifier, **When** the request is handled, **Then** a "bad
   request" response is shown; an unknown identifier gives a "not found" response.

---

### User Story 4 - View or delete a breakpoint record directly (Priority: P3)

A details page and a delete page exist for each record, but they are not linked from the list (the links are
disabled in the page) and are reachable only by entering their address directly.

**Why this priority**: The pages are hidden from the normal flow, but deletion is reachable and removes data far
beyond the breakpoint record itself.

**Independent Test**: Open the details page of a record directly and verify its content; delete, in a test
environment, a record that is used by stored E-Test measurements and verify that those measurements are gone from the
affected isolates.

**Acceptance Scenarios**:

1. **Given** the details page of an existing record is opened directly, **When** it is shown, **Then** it lists
   antibiotic (internal identifier), details, version, "Gültig ab", "Sensibel <=", and "Resistent >" under the
   untranslated headings "Details" and "EucastClinicalBreakpoint" with the links "Edit" and "Back to List"; "Gültig
   für" is not shown. A missing identifier gives a "bad request" response and an unknown one a "not found" response.
2. **Given** the delete page of an existing record is opened directly, **When** it is shown, **Then** it asks "Are you
   sure you want to delete this?" with the same details and a "Delete" button; it gives no indication of how many
   measurements use the record.
3. **Given** the deletion is confirmed, **When** it is executed, **Then** the record is removed permanently together
   with every stored E-Test measurement of either pathogen that refers to it, without audit record or undo, and the
   list is shown.
4. **Given** the deletion of a record that no longer exists is confirmed, **When** it is executed, **Then** the generic
   error page is shown.

### Edge Cases

- **Validity is "valid from" only**: A record has a version and a "Gültig ab" date but no end date. Records never
  expire; all records of a pathogen and antibiotic stay selectable on the isolate form indefinitely. The form lists
  them newest "Gültig ab" first and, when a measurement is chosen and no breakpoint is selected yet, automatically
  selects the first listed (newest) one. The record is not chosen by the isolate's sampling or receipt date; "Gültig
  ab" is used only for ordering, for the label, and for the year printed on reports. (The legacy Access importer, by
  contrast, chose the record by receipt date; that is out of scope.)
- "ohne Grenzwerte" records have no "Gültig ab" and are listed after the dated records of the same antibiotic
  (derived; see Assumptions). They let an antibiotic be recorded without EUCAST values (change history 2016-05-13).
- The list holds not only EUCAST values: the change history (2022-06-26) records Azithromycin for Meningococci with
  CLSI values, and Meningococci reports omit Azithromycin measurements. Nothing in the record distinguishes the source
  of the values.
- "Gültig für" = "Allgemein" can be chosen and stored, but such a record is never offered for either pathogen,
  because the isolate forms offer only records whose pathogen matches exactly. [SUSPECTED-DEFECT]
- Validation does not check that "Sensibel <=" is less than or equal to "Resistent >", that values are positive, or
  that "Version" and "Gültig ab" are plausible. With inverted thresholds, the form would classify every measurement
  above "Resistent >" as resistant even when it is at or below "Sensibel <=", and no measurement as intermediate
  (derived from the interpretation order resistant → susceptible → intermediate). [SUSPECTED-DEFECT]
- Duplicate combinations of antibiotic, details, pathogen, and version are rejected only by the database, which ends
  on the generic error page and loses the entered values instead of showing a validation message. [SUSPECTED-DEFECT]
- **Treatment as reference data** (Constitution Principle I): Breakpoints are the reference data from which E-Test
  susceptibility results are derived, and they decide which antibiotics can be recorded at all. Currently they are
  maintained as ordinary runtime data: any Standard User can create and overwrite records in place, without a review
  step, version history, or audit record, and changes to records in use silently change what existing reports show
  (User Story 3). [POTENTIAL-REFACTORING]
- Editing thresholds or "Gültig ab" of a record in use makes reports of earlier measurements show thresholds and a
  year that differ from the ones against which the stored result was derived, so a report can show, for example, a
  "susceptible" result next to thresholds that would now classify the measurement as resistant. [SUSPECTED-DEFECT]
- Deleting a record also deletes all E-Test measurements of both pathogens that refer to it, without any warning on
  the delete page; this is a data-losing operation that is neither auditable nor recoverable within the application,
  contrary to Constitution Principle I. The delete page is reachable only by direct address. [SUSPECTED-DEFECT]
- The details and delete pages are untranslated standard pages (English labels, entity name as heading, internal
  antibiotic identifiers), unlike the German list, create, and edit pages; the list shows internal pathogen
  identifiers in "Gültig für".
- Decimal values are entered with a comma as decimal separator ("0,5"), matching the application's German culture
  setting.
- Authorization: All breakpoint pages admit only the Standard User role. Navigation shows "Antibiotika Grenzwerte" to
  Standard User and Administrator accounts, so an Administrator-only account sees the entry but is refused, as in specs
  011 and 014. [SUSPECTED-DEFECT] Public Health accounts are neither offered the entry nor admitted.
- The create, edit, and delete forms verify the anti-forgery token.
- The breakpoint pages read and write the database directly from the page controller and render the stored record
  itself rather than a page-specific model; the logic that derives the offered antibiotics from the breakpoint list is
  copied in two places, and the client-side interpretation script is duplicated in the Haemophilus and Meningococci
  isolate forms. This reflects the constitution's "Known Architectural Constraints (Legacy Debt)" (parallel
  Meningococci/Haemophilus domains; god-controllers with weak dependency injection) and Principle III. Neither the
  breakpoint validator nor the breakpoint pages have automated tests. [POTENTIAL-REFACTORING]

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The navigation MUST offer "Antibiotika Grenzwerte" in the Administration menu to Standard User and
  Administrator accounts and MUST NOT offer it to Public Health accounts.
- **FR-002**: All breakpoint pages MUST admit only accounts holding the Standard User role and MUST refuse
  Administrator-only and Public Health accounts (current behavior, see Edge Cases).
- **FR-003**: The list "Grenzwerte" MUST show all stored records of both pathogens with antibiotic, details, version,
  pathogen, valid-from date, susceptible threshold, resistant threshold, and an edit button, eight per page,
  searchable, initially sorted by version descending; it MUST offer a create button and MUST NOT link to the details
  or delete pages.
- **FR-004**: The create and edit pages MUST offer antibiotic, details, version, pathogen ("Haemophilus",
  "Meningokokken", "Allgemein"), valid-from date, susceptible threshold ("Sensibel <="), and resistant threshold
  ("Resistent >").
- **FR-005**: Saving MUST require non-empty details of at most 128 characters and a version of at most 64 characters,
  and MUST require version, valid-from date, and both thresholds to be either all filled or all empty; otherwise the
  form MUST be shown again with validation messages and nothing stored.
- **FR-006**: Saving MUST NOT check the relation between the two thresholds, their range, or the plausibility of
  version and date (current behavior).
- **FR-007**: A record with version, valid-from date, and both thresholds empty MUST be treated as "ohne Grenzwerte":
  it MUST be offered as `<details> - ohne Grenzwerte`, and measurements interpreted against it MUST be classified
  "not determined".
- **FR-008**: A record with all four values MUST be offered as `<details> - v<version> vom <valid-from date>`, and
  measurements MUST be classified resistant above the resistant threshold, susceptible at or below the susceptible
  threshold, and intermediate otherwise (interpretation as documented in specs 004 and 005).
- **FR-009**: The combination of antibiotic, details, pathogen, and version MUST be unique; a duplicate MUST be
  rejected by the database and MUST end on the generic error page (current behavior).
- **FR-010**: Saving an edit MUST overwrite the record in place, without warning about measurements that use it,
  without keeping the previous values, and without audit record.
- **FR-011**: The isolate form of each pathogen MUST offer, in its E-Test section, only antibiotics for which at least
  one record with exactly that pathogen exists, MUST show a configured standard antibiotic's row only if such a record
  exists, and MUST list the records of an antibiotic newest valid-from date first, with no end of validity.
- **FR-012**: Report data for an E-Test measurement MUST take the thresholds and the year of the valid-from date from
  the referenced record at the time the report is created, and MUST show "---" instead when the stored result is "not
  determined" (see spec 010); the stored result MUST NOT be recalculated.
- **FR-013**: Exports MUST attribute each stored measurement to the antibiotic of its referenced record (see spec
  011); breakpoint thresholds MUST NOT be exported.
- **FR-014**: The details and delete pages MUST be reachable by direct address only; confirming deletion MUST remove
  the record and every E-Test measurement that refers to it, permanently, without audit record or undo.
- **FR-015**: The create, edit, and delete forms MUST verify the anti-forgery token.
- **FR-016**: The breakpoint records MUST be one list shared by both pathogens, separated only by the "Gültig für"
  value of each record.
- **FR-017**: This feature MUST document the current breakpoint behavior only and MUST NOT introduce new validation,
  versioning, or deletion rules.

### Record Kinds

| Record kind | Version, "Gültig ab", thresholds | Label on isolate form | Interpretation of a measurement | On report |
| --- | --- | --- | --- | --- |
| With breakpoints | All filled | `<details> - v<version> vom <date>` | Resistant / susceptible / intermediate | Thresholds and year of "Gültig ab" |
| "ohne Grenzwerte" | All empty | `<details> - ohne Grenzwerte` | Not determined | "---" for thresholds and year |
| "Gültig für" = "Allgemein" | Either | Never offered | Not applicable | Not applicable |

### Effects of Changes on Existing Data

| Change to a record in use | Effect on stored measurements | Effect on later reports and exports |
| --- | --- | --- |
| New record (new version) added | None; existing measurements keep their record | None; the new record is pre-selected for new measurements |
| Thresholds or "Gültig ab" edited | Stored result unchanged | Reports show new thresholds and year |
| Antibiotic edited | Measurements now count as the new antibiotic | Reports and exports list them under the new antibiotic |
| "Gültig für" edited | Measurement keeps the record | Record no longer offered on the previous pathogen's form |
| Record deleted | All measurements referring to it are deleted | Measurements disappear from reports and exports |

### Roles and Boundaries

| Actor or role | Current observable involvement |
| --- | --- |
| Standard User (`Standardbenutzer`) | Sees the Administration entry; lists, creates, and edits records; can open details and delete pages by direct address; uses the records in the E-Test section of isolate entry. |
| Administrator | Sees the Administration entry "Antibiotika Grenzwerte" but is refused by every breakpoint page unless the account also holds the Standard User role. |
| Public Health (`RKI`) | Is neither offered the entry nor admitted to the breakpoint pages. |

### Key Entities *(include if feature involves data)*

- **Clinical breakpoint record (Grenzwert)**: One record per antibiotic, details text, pathogen ("Gültig für"), and
  version, with an optional valid-from date ("Gültig ab"), a susceptible threshold ("Sensibel <="), and a resistant
  threshold ("Resistent >"). Version, date, and both thresholds are either all present (a dated breakpoint version) or
  all absent ("ohne Grenzwerte"). It has no end of validity and is not linked to any sender, patient, or submission.
- **E-Test measurement**: A stored measurement of an isolate of either pathogen; it refers to exactly one breakpoint
  record, from which its antibiotic is taken, and stores the measured value and the susceptibility result derived on
  the isolate form (specs 004 and 005).
- **Standard antibiotics**: Per-pathogen configured antibiotics whose E-Test rows the isolate form shows by default,
  provided a breakpoint record exists for them.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: In 100% of tested saves with empty details, or with only some of version, valid-from date, and the two
  thresholds filled, zero records are stored and a validation message is shown.
- **SC-002**: After creating a record for an antibiotic and pathogen that had none, the antibiotic is offered on the
  isolate form of that pathogen and not on the other pathogen's form in 100% of tests.
- **SC-003**: For an antibiotic with several dated records, the newest valid-from record is pre-selected for a new
  measurement in 100% of tests, and all older records remain selectable.
- **SC-004**: For measurements against an "ohne Grenzwerte" record, 100% are classified "not determined" and their
  reports show "---" for thresholds and year.
- **SC-005**: After editing the thresholds of a record in use, 100% of tested reports for existing measurements show
  the new thresholds with the unchanged stored result.
- **SC-006**: Deleting a record used by N stored measurements leaves zero of those measurements in the database in
  100% of tests.
- **SC-007**: Across the three roles, observed access to the breakpoint pages matches the Roles and Boundaries table in
  100% of attempts.

## Assumptions

- Current application code, views, schema migrations, configuration, the application's change history, and role
  constants are the source of truth. No running system was exercised; there are no automated tests for the breakpoint
  pages or the breakpoint validator.
- The deletion of dependent E-Test measurements follows from the cascading relation defined when the measurement table
  was created; no later migration changes it. It was not exercised.
- When "Gültig für" is left unselected on the create page (no option is pre-selected for a new record), the record is
  assumed to be stored with "Haemophilus", following standard form handling for an absent value; not exercised.
- Records without "Gültig ab" are assumed to be listed after dated records because the database sorts empty dates last
  in descending order; not verified.
- After a change of "Gültig für", an existing measurement whose record is no longer offered is assumed to show no
  selected breakpoint on the isolate form, so saving that isolate requires choosing another breakpoint (derived from
  the form and its validation, see specs 004 and 005); not exercised.
- The "Gültig ab" display with time of day and the internal identifiers shown for "Gültig für" and on the details and
  delete pages are derived from the standard display of dates and enumerations; not observed in a browser.
- Accounts are assumed to hold exactly one role, as in specs 011 and 014, so an Administrator account is refused.
- The values of the stored breakpoint records themselves (which EUCAST or CLSI versions exist) are data, not behavior,
  and were not inspected.
