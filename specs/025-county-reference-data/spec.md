# Feature Specification: County reference data management

**Feature Branch**: `speckit-init`

**Created**: 2026-10-30

**Status**: Draft (status-quo inventory)

**Input**: User description: "Feature title: \"County reference data management\" (backlog ref C6, Group C — Reference data & administration). Document the current, as-implemented behavior of maintaining the county (Landkreis) reference list used to derive county and state numbers in exports, as it exists today: the generated list, details, create, edit, and delete pages, the file upload import, the validity date, and how exports match a patient's county text against the list. Read HaemophilusWeb/Controllers/CountyController.cs, HaemophilusWeb/Tools/CountyImporter.cs, the County model, HaemophilusWeb/Views/County/*, HaemophilusWebTests/Tools/CountyImporterTests.cs, and the county/state-number derivation used by the RKI and LGA exports. Capture CRUD, validation, the upload format, matching and normalization rules, which version is chosen when several match, and the access model. Document observable behavior only — do not propose changes."

> **Scope note**: This is an *as-is* specification (backlog ref C6). It records the current, observable behavior of
> the county (Landkreis) reference list: the hidden, generated list, details, create, edit, and delete pages, the
> hidden upload page "GV100AD acs-Datei hochladen" that reads the official DeStatis municipality directory
> ("Gemeindeverzeichnis", format GV100AD) and adds its county records, the meaning of the validity date
> ("ValidSince"), the validation (or absence of it), the access model, and how the three exports that carry county
> and state numbers (Haemophilus RKI export, Meningococci RKI export, Meningococci LGA export) match a patient's
> free-text county against the list, which normalization steps they apply, and which record is taken when several
> match. It does **not** propose changes. Out of scope and referenced instead: the export pages, their selection
> rules, and column sets (`specs/011-data-exports-generic`), which already state the matching rule, the
> earliest-validity choice, and the warning behavior; the sign-in and role model (`specs/021-auth-authorization-roles`,
> access matrix row "Counties (generated pages, file upload)"); the postal-code lookup that fills the patient's county
> text (`specs/022-postal-code-lookup-geonames`); the patient forms themselves (`specs/002-patient-sending-intake`,
> `specs/003-meningo-patient-sending`); the "Ausland" comparison of the PubMLST export, which compares the county
> text with a fixed word and does not use the county list (spec 011).

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Derive county and state numbers in exports (Priority: P1)

When a Standard User or a Public Health user runs the Haemophilus RKI export, the Meningococci RKI export, or the
Meningococci LGA export (spec 011), each exported row carries a county number and a state number. Both are derived by
comparing the patient's free-text "Landkreis" with the names in the stored county list; the county number of the
matching record is exported, and the state number is its first two characters.

**Why this priority**: Deriving these two numbers is the only purpose of the county list; the RKI and the state health
authority receive the numbers, not the free text, as the geographic key of a case.

**Independent Test**: Store a county "Flensburg, Stadt" with number "01001"; create submissions whose patients have the
county texts "Flensburg", "flensburg", "Kreisfreie Stadt Flensburg", "Flensburg, Stadt", "Landkreis Flensburg", "Hof",
and no text; run the three exports and compare the county and state number columns; for the Meningococci exports also
check the application log.

**Acceptance Scenarios**:

1. **Given** a stored county record with name "Flensburg, Stadt" and number "01001", **When** a patient's county text
   is "Flensburg", "flensburg", "Kreisfreie Stadt Flensburg", "Flensburg, Stadt", or "Landkreis Flensburg", **Then**
   the row carries county number "01001" and state number "01" (Haemophilus RKI columns `kreis_nr` and `bundesland`;
   Meningococci RKI and LGA columns "Landkreis" and "Bundesland").
2. **Given** the comparison, **When** the two names are normalized, **Then** on both sides the text from the first
   comma onward is dropped, the fill words "Städteregion", "Kreisfreie Stadt", "Region", "Landkreis", and "Kreis" are
   removed in this order with surrounding blanks trimmed, and the remainders are compared ignoring upper and lower
   case (as stated in spec 011; the detailed rules are in the Normalization table below).
3. **Given** a patient whose county text matches no stored record (for example "Hof" against "Hofstetten", or
   "Unbekannt" written by the postal-code lookup, spec 022), **When** the row is exported, **Then** both number columns
   are empty; the Meningococci RKI and LGA exports write a warning to the application log naming the county text and the
   submission identifier, the Haemophilus RKI export does not (spec 011); the export completes normally.
4. **Given** a patient without county text, **When** the row is exported, **Then** both number columns are empty and
   no warning is written.
5. **Given** several stored records match the same text (for example the same county from several uploaded editions,
   or a record created by hand next to an uploaded one), **When** the row is exported, **Then** the record with the
   earliest "ValidSince" is used; the submission's sampling or receiving date is not considered (spec 011,
   [POTENTIAL-REFACTORING] recorded there); among records with the same "ValidSince" the storage order decides
   (derived; see Assumptions).
6. **Given** the matched record, **When** the state number is derived, **Then** it is the first two characters of the
   stored county number, whatever they are; a county number with fewer than two characters yields what there is, and
   an empty county number yields an empty state number.
7. **Given** the export runs, **When** the patient's "Bundesland" selection differs from the state implied by the
   matched county, **Then** the numbers still follow the matched county; the patient's "Bundesland" is not consulted
   for either number. The Haemophilus RKI export additionally exports the free county text (`landkreis`) and the state
   name (`bundeslandName`) as separate columns (spec 011).
8. **Given** the county list is read once at the start of each export run, **When** a record is created, edited, or
   deleted, **Then** the change applies to the next export run; no export, submission, or patient stores a reference to
   a county record.

---

### User Story 2 - Load an edition of the official county list (Priority: P1)

A Standard User enters the address of the hidden upload page, chooses a GV100AD file ("Gemeindeverzeichnis",
quarterly edition published by DeStatis), and uploads it. Every county record of the file is added to the stored list
with its official name, its five-digit regional key, and the edition's reference date as "ValidSince".

**Why this priority**: The upload is the intended way to fill the list; without it every county would have to be
typed by hand. The county pages and the importer were added together with the "RKI Export Modul" (change log
2015-02-13; see Assumptions).

**Independent Test**: With a Standard User account, open the upload page by direct address, upload the three-county
test file of the automated importer tests, and verify that exactly three records ("Flensburg, Stadt" 01001 2014-10-31,
"Kiel, Landeshauptstadt" 01002 2014-12-31, "Lübeck, Hansestadt" 01003 2014-01-31) were added; upload the same file
again and verify that three more records appear.

**Acceptance Scenarios**:

1. **Given** a Standard User requests the upload page by direct address, **When** it is shown, **Then** it has the
   heading "GV100AD acs-Datei hochladen", a file chooser labelled "ACS-Datei:", the button "Hochladen", and — because
   opening the page is handled like an upload without a file — already shows the message "Bitte geben sie eine Datei
   an." below the button.
2. **Given** a GV100AD file is chosen, **When** "Hochladen" is selected, **Then** every line beginning with the record
   type "40" (county) becomes one new county record: the name from character positions 23 to 72 with surrounding
   blanks removed (for example "Flensburg, Stadt", "Kiel, Landeshauptstadt"), the county number from positions 11 to
   15 (for example "01001"), and "ValidSince" from positions 3 to 10 read as year, month, day (for example "20141031"
   → 31 October 2014, at midnight). Afterwards the list page is shown.
3. **Given** the file also contains lines of other record types (state "10", municipal association "50", municipality
   "60"), **When** it is imported, **Then** those lines are ignored; only the county lines are stored. Lines are
   processed in file order.
4. **Given** the file is encoded in the Windows Western encoding used by DeStatis, **When** it is imported, **Then**
   names with umlauts are stored correctly ("Lübeck, Hansestadt"); the file is always read with that encoding.
5. **Given** the same file or a newer edition is uploaded, **When** the import runs, **Then** every county line is
   added as a new record again; existing records are neither compared, updated, nor replaced, so the list then holds
   each county once per uploaded edition (and several times for a repeated upload).
6. **Given** a county line that is shorter than 72 characters or whose validity field is not eight digits, **When**
   the import runs, **Then** nothing of the file is stored (all records of the file are stored together or not at all)
   and the upload page is shown again with "Fehler:" followed by the technical error text of the failure.
7. **Given** no file is chosen, or the chosen file is empty, **When** "Hochladen" is selected, **Then** the upload page
   is shown again with "Bitte geben sie eine Datei an." and nothing is stored.
8. **Given** the upload succeeds, **When** the list page is shown, **Then** no count of imported records, no message,
   and no log entry record the upload, and the uploaded file remains in the server's temporary folder.

---

### User Story 3 - Review the stored county list and a single record (Priority: P2)

A Standard User enters the address of the hidden list page to see which county records are stored, with which numbers
and validity dates, and opens the details page of a record.

**Why this priority**: The list is the only place where the records that drive the exported numbers can be inspected,
for example to understand why an export carries an unexpected or empty county number.

**Independent Test**: With a Standard User account, open the list page by direct address and verify its columns and
links; open a details page; repeat with an Administrator-only and a Public Health account and verify that both are
refused.

**Acceptance Scenarios**:

1. **Given** a Standard User requests the list page by direct address, **When** it is shown, **Then** the page "Index"
   lists all stored records in one table with the columns "CountyNumber", "Name", and "ValidSince", offers the links
   "Edit", "Details", and "Delete" for each row and "Create New" above the table, and provides no paging, search,
   sorting, or filtering; rows appear in storage order (derived; see Assumptions).
2. **Given** the list, **When** "ValidSince" is shown, **Then** it appears as a date with time of day in the German
   format (for example "31.12.2014 00:00:00").
3. **Given** "Details" is selected, **When** the page is shown, **Then** it shows the headings "Details" and "County",
   the three values, and the links "Edit" and "Back to List".
4. **Given** the details, edit, or delete page is requested without a record identifier, **When** the request is
   handled, **Then** a "bad request" response is shown; an unknown identifier gives a "not found" response.
5. **Given** an account holding only the Administrator role, or a Public Health account, **When** any county page or the
   upload is requested, **Then** the request is refused (redirected to the sign-in page), because all county pages admit
   only the Standard User role (`Standardbenutzer`).
6. **Given** any role, **When** the navigation or the home page is shown, **Then** no entry leads to the county pages
   or the upload page (spec 021); all county pages are reachable only by direct address.

---

### User Story 4 - Add, correct, or delete a single county record (Priority: P3)

A Standard User creates one county record by hand, corrects a record (for example a misspelt name or a wrong number),
or deletes a record. The change takes effect for the next export run.

**Why this priority**: Single-record maintenance is a fallback next to the upload; deletion is the only way to remove
duplicate or outdated editions, one record at a time.

**Independent Test**: Create a record with an arbitrary number and name; verify that only the validity date is
checked; edit it; delete it; run an export in between and verify the effect on the exported numbers.

**Acceptance Scenarios**:

1. **Given** the list, **When** "Create New" is selected, **Then** the page "Create" (heading "County") offers the text
   fields "CountyNumber", "Name", and "ValidSince", the button "Create", and the link "Back to List".
2. **Given** a value in "ValidSince" that is a date (German date format, optionally with time), **When** "Create" is
   selected, **Then** the record is stored and the list is shown; "CountyNumber" and "Name" are stored as entered,
   including any text, any length, or nothing at all.
3. **Given** "ValidSince" is empty or not a date, **When** the user saves, **Then** the form is shown again with a
   validation message (also before sending, in the browser) and nothing is stored. No other rule is checked: the
   county number is not required to be five digits, the name is not required, and duplicates of number or name are
   accepted.
4. **Given** "Edit" is selected for a record, **When** the page "Edit" is shown, **Then** it offers the same three
   fields filled with the stored values and the button "Save"; saving overwrites all three values in place, keeps no
   previous version, writes no audit record, and shows the list; the same validation as on the create page applies.
5. **Given** the edit form of a record is open and the record is deleted meanwhile, **When** "Save" is selected,
   **Then** the generic error page is shown.
6. **Given** "Delete" is selected for a record, **When** the page "Delete" is shown, **Then** it asks "Are you sure you
   want to delete this?" above the three values and offers the button "Delete" and the link "Back to List".
7. **Given** the deletion is confirmed, **When** it is executed, **Then** the record is removed permanently (no
   soft delete, no recovery page as in spec 020, no audit record) and the list is shown; confirming the deletion of a
   record that no longer exists shows the generic error page.
8. **Given** a record is edited or deleted, **When** patients and submissions are inspected, **Then** none of them has
   changed: the patient's county is free text, and no stored record refers to a county record; only future exports are
   affected.
9. **Given** the create, edit, or delete form is submitted, **When** the request is handled, **Then** the anti-forgery
   token is verified; the upload does not verify a token and accepts any request method (spec 021).

### Edge Cases

- **Normalization is symmetric and literal**: The same steps are applied to the stored name and to the patient's text.
  The comma rule drops everything from the first comma onward but only if the comma is not the first character. Fill
  words are removed wherever they occur in the text, not only as whole words, and only in the spelling listed (for
  example "Region" is also removed from inside "Regionalverband", and "Kreis" from "Rhein-Kreis"; "landkreis" in lower
  case is not removed). Because both sides are treated alike, a text that carries the same prefix as the stored name
  still matches; a text without it may not. After the fill words only upper and lower case are ignored: umlauts,
  hyphens, brackets, and inner blanks must be identical.
- **Different counties can normalize to the same text**: Where the official list contains a city "X, Stadt" (or
  "X, Hansestadt" and the like) and a surrounding district "Landkreis X", both names normalize to "X", and the patient
  text "Landkreis X" delivered by the postal-code lookup (spec 022) matches both; the record with the earliest
  "ValidSince" (and, on a tie, the first in storage order) is exported, which can be the city's number for a patient of
  the district. Derived from the rules; not exercised. [SUSPECTED-DEFECT]
- **A record with an empty name matches patients without county text**: A record saved on the create or edit page
  with an empty "Name" is stored without a name; the comparison treats "no name" against "no county text" as equal.
  Every patient without county text then receives that record's county and state number in all three exports, and the
  Meningococci exports write no warning for them. Derived from the matching rule and its automated test for two empty
  names; not exercised. The upload never produces an empty name of this kind (it stores an empty text, which does not
  match). [SUSPECTED-DEFECT]
- **"Earliest validity wins" together with "uploads only add"**: Because every upload adds records and the earliest
  "ValidSince" is chosen, uploading a newer edition changes the exported number only for names that did not exist
  before. A county whose number changed (territorial reform) keeps being exported with the old number as long as the
  old record is present and the name is unchanged; the newer record is never chosen. Removing the old edition requires
  deleting its records one at a time on the delete page. (The date-based choice itself is tagged in spec 011.)
  [POTENTIAL-REFACTORING]
- **No validation of the county number**: Any text is accepted as county number, and the exports take its first two
  characters as the state number; a mistyped number is exported as typed. The validity date is the only checked field.
  Constitution Principle I ("reference values MUST be treated as authoritative domain data") is not reflected in
  checks. [POTENTIAL-REFACTORING]
- **Treatment as reference data** (Constitution Principle I): The county list is maintained as ordinary runtime data:
  any Standard User can create, overwrite, and permanently delete records and can add a whole edition by upload, with
  no review step, version history, audit record, log entry, or recovery; a wrong upload cannot be undone except record
  by record. [POTENTIAL-REFACTORING]
- **Upload page served by the upload function**: The page is shown by the same function that performs the upload, so
  the page can be reached with any request method and already displays "Bitte geben sie eine Datei an." when first
  opened; the form carries no anti-forgery token and none is verified (spec 021 records the latter).
- **Upload errors show technical text**: A failed import shows "Fehler:" followed by the untranslated technical message
  of the failure (for example an index-out-of-range or date-format message); the failure is not logged, and the
  uploaded copy remains in the server's temporary folder. [POTENTIAL-REFACTORING]
- **File format assumptions**: The importer relies on fixed character positions of the GV100AD layout and on the
  record type "40"; a file in another layout (for example a CSV export of the directory) either imports nothing (no
  line starts with "40") and shows the list unchanged, or fails with the technical message when a line happens to start
  with "40" but does not fit the positions. The page title and label say "acs-Datei"/"ACS-Datei" although the DeStatis
  files and the automated test use the extension ".asc".
- **Untranslated generated pages**: The list, details, create, edit, and delete pages are standard generated pages
  with English titles ("Index", "Create", "Edit", "Details", "Delete"), the entity name "County" as heading, the field
  names "CountyNumber", "Name", "ValidSince" as labels, and English links and buttons, unlike the German upload page
  and the German pages of the rest of the application (spec 021 lists them among the generated pages).
- **Validity date is "valid since" only**: A record has no end of validity and never expires; "ValidSince" serves only
  as the sort key that decides which of several matching records is exported (earliest first). It is stored with a
  time of day, which is midnight for uploaded records and whatever the user entered for records created by hand.
- **Authorization**: All county pages and the upload admit only the Standard User role. No menu entry exists, so the
  Administrator-menu observation of specs 011, 014, and 017 does not arise here. Public Health accounts run the two RKI
  exports that depend on the list (spec 011) but can neither view nor maintain the list.
- **Layering and duplication**: The county pages read and write the database directly from the page controller and
  render the stored record itself; the matching rule lives in the county record, and the look-up with the "empty
  county" fallback and the first-two-characters rule is copied into each of the three exports (the warning code twice,
  in the two Meningococci exports). The importer and the matching rule have automated tests (three-county test file;
  name variations "Hamburg"/"Hamburg, Freie und Hansestadt", "Städteregion Aachen"/"Aachen", "Berlin"/"Kreisfreie
  Stadt Berlin", "Region Hannover"/"Hannover"), and the export tests cover the derived numbers; the county pages and
  the upload have none. This reflects the constitution's "Known Architectural Constraints (Legacy Debt)" and
  Principles II and III. [POTENTIAL-REFACTORING]

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: All county pages (list, details, create, edit, delete) and the upload MUST admit only accounts holding
  the Standard User role and MUST refuse Administrator-only and Public Health accounts (redirect to the sign-in page).
- **FR-002**: No navigation entry or home-page tile MUST lead to any county page; they MUST be reachable by direct
  address only (current behavior, spec 021).
- **FR-003**: The list page MUST show all stored county records in one table with county number, name, and validity
  date, with "Edit", "Details", and "Delete" per row and "Create New" above, without paging, search, or sorting.
- **FR-004**: The details page MUST show county number, name, and validity date of one record with the links "Edit"
  and "Back to List"; a missing identifier MUST give a "bad request" response and an unknown one a "not found"
  response, on the details, edit, and delete pages alike.
- **FR-005**: The create and edit pages MUST offer the fields "CountyNumber", "Name", and "ValidSince"; saving MUST
  require "ValidSince" to be a date and MUST apply no other rule (no format, length, presence, or uniqueness check on
  number or name).
- **FR-006**: Saving an edit MUST overwrite the three values in place, without previous version, warning, or audit
  record; saving for a record that no longer exists MUST end on the generic error page.
- **FR-007**: Confirming the delete page MUST remove the record permanently, without soft delete, recovery, or audit
  record; confirming for a record that no longer exists MUST end on the generic error page.
- **FR-008**: The create, edit, and delete forms MUST verify the anti-forgery token; the upload MUST NOT verify one and
  MUST accept any request method (current behavior).
- **FR-009**: The upload page MUST be titled "GV100AD acs-Datei hochladen", offer a file chooser "ACS-Datei:" and the
  button "Hochladen", and MUST show "Bitte geben sie eine Datei an." when opened, when no file is chosen, or when the
  chosen file is empty.
- **FR-010**: The upload MUST read the file in the Windows Western encoding, MUST take every line starting with "40"
  as one county record (name positions 23–72 trimmed, county number positions 11–15, validity positions 3–10 as
  year-month-day), MUST ignore all other lines, and MUST add the records to the list without comparing, updating, or
  replacing existing records.
- **FR-011**: The upload MUST store all county records of a file together or none; on failure it MUST show the upload
  page with "Fehler:" followed by the technical message; on success it MUST show the list without a summary or log
  entry.
- **FR-012**: The Haemophilus RKI, Meningococci RKI, and Meningococci LGA exports MUST derive the county number of a
  row from the stored record whose normalized name equals the patient's normalized county text, and the state number
  from the first two characters of that county number; both MUST be empty when no record matches (spec 011 FR-014).
- **FR-013**: Normalization MUST, on both names, drop the text from the first comma onward (comma not in first
  position), remove the fill words "Städteregion", "Kreisfreie Stadt", "Region", "Landkreis", "Kreis" in this order
  wherever they occur in this spelling, trim blanks, and compare the remainders ignoring upper and lower case; two
  absent names MUST count as equal, an absent name and a present one as unequal.
- **FR-014**: When several records match, the exports MUST use the record with the earliest validity date, regardless
  of the submission's dates; the patient's "Bundesland" MUST NOT be consulted.
- **FR-015**: The Meningococci RKI and LGA exports MUST write a warning to the application log for every row whose
  non-empty county text matches no record; the Haemophilus RKI export MUST NOT (current behavior, spec 011).
- **FR-016**: A county record MUST have no end of validity and MUST NOT be referenced by patients, submissions, or
  exports; changes MUST take effect at the next export run.
- **FR-017**: This feature MUST document the current county behavior only and MUST NOT introduce new validation,
  de-duplication, or audit rules.

### Upload File Layout (GV100AD county record)

| Field | Character positions (1-based) | Rule | Example |
| --- | --- | --- | --- |
| Record type | 1–2 | Line is imported only if it is "40" | `40` |
| Validity ("ValidSince") | 3–10 | Eight digits year, month, day; stored at midnight | `20141031` → 31.10.2014 |
| County number | 11–15 | Stored as text (state 2 digits, administrative region 1, county 2) | `01001` |
| Name | 23–72 | Stored as text, surrounding blanks removed, suffixes kept | `Flensburg, Stadt` |
| All other positions | 16–22, 73 onward | Ignored | — |

### Normalization and Matching

| Step | Applied to | Rule | Example |
| --- | --- | --- | --- |
| 1. Absent values | both | Both absent → equal; one absent → unequal | — / — → match |
| 2. Comma suffix | both | Drop from the first comma onward, unless the comma is the first character | "Hamburg, Freie und Hansestadt" → "Hamburg" |
| 3. Fill words | both | Remove "Städteregion", "Kreisfreie Stadt", "Region", "Landkreis", "Kreis" in this order, wherever they occur, exact spelling; trim blanks after each | "Kreisfreie Stadt Berlin" → "Berlin"; "Landkreis Forchheim" → "Forchheim" |
| 4. Compare | both | Equal ignoring upper and lower case; nothing else is folded | "flensburg" = "Flensburg" |
| 5. Several matches | list | Earliest "ValidSince" wins; tie → storage order | — |
| 6. State number | result | First two characters of the county number | "01001" → "01" |

### Roles and Boundaries

| Actor or role | Current observable involvement |
| --- | --- |
| Standard User (`Standardbenutzer`) | Reaches all county pages and the upload by direct address; lists, creates, edits, deletes, and uploads records; runs the exports that use them. |
| Administrator | Refused by every county page and the upload unless the account also holds the Standard User role; sees no entry. |
| Public Health (`RKI`) | Refused by every county page and the upload; runs the Haemophilus and Meningococci RKI exports whose county and state numbers depend on the list (spec 011). |
| Unauthenticated visitor | Redirected to the sign-in page. |
| DeStatis (external) | Publishes the GV100AD file that the upload reads; no connection from the system, the user obtains the file. |

### Key Entities *(include if feature involves data)*

- **County record (Landkreis)**: One row of the reference list with county number (free text, the five-digit regional
  key when uploaded), name (free text, the official name including suffixes such as ", Stadt" when uploaded), and
  validity date "ValidSince" (date with time). No end of validity, no edition or source marker, no uniqueness; the same
  county appears once per uploaded edition. Not linked to any patient, submission, or export.
- **Patient county text ("Landkreis")**: Free text on Haemophilus and Meningococci patients, filled by the postal-code
  lookup (spec 022) or typed; it is matched against the county records at export time only.
- **County and state number columns**: Export values (`kreis_nr`/`bundesland` in the Haemophilus RKI export;
  "Landkreis"/"Bundesland" in the Meningococci RKI and LGA exports) derived per row at export time (spec 011).
- **GV100AD file**: The DeStatis municipality directory edition uploaded by the user; only its county lines are used.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: For the three-county test file, 100% of uploads add exactly three records with the expected names,
  numbers, and validity dates, and a repeated upload adds exactly three more.
- **SC-002**: For patient texts that differ from the stored name only by case, a comma suffix, or the five fill words,
  100% of exported rows carry the stored county number and its first two characters as state number.
- **SC-003**: For patient texts that match no record, 100% of exported rows carry empty county and state numbers; the
  Meningococci exports log exactly one warning per such row with non-empty text and the Haemophilus RKI export logs
  none.
- **SC-004**: When two records with different validity dates match the same text, 100% of exported rows carry the
  number of the record with the earlier date.
- **SC-005**: In 100% of tested saves with an empty or non-date "ValidSince", nothing is stored and a validation message
  is shown; in 100% of saves with any other content of number and name, the record is stored as entered.
- **SC-006**: Across the three roles, observed access to the county pages and the upload matches the Roles and
  Boundaries table in 100% of attempts, and no navigation entry to them is shown to any role.

## Assumptions

- Current application code, views, the schema migration of 2015-02-11 that created the county table, the version
  history (commits 2015-02-11 "Added CountyImporter", "Added county controller with default views and databse
  migrations", "Added upload form and fixed importer encoding"; 2015-02-13 "Fixed county importer to only import the
  name part"; 2015-02-23 "Implemented similarity comparison for county names"; 2016-04-30 "Added user roles and role
  based access restrictions"), and the automated tests (importer, name matching, three export definitions) are the
  source of truth. No running system was exercised and no real GV100AD file was available; the file layout is known
  from the importer, its test data, and the DeStatis layout it cites.
- The change log ("Über" page data) contains no entry that names counties, the upload, or the GV100AD file; the entry
  "RKI Export Modul" (feature, 2015-02-13) coincides with the county commits and is assumed to be the release that
  introduced them.
- The storage order used for the list page and for ties between records with the same validity date is assumed to be
  the order of creation (ascending record identifier), the usual order for a table read without sorting; not verified.
- The display of "ValidSince" with time of day, the German date format on input and output, and the validation messages
  for an empty or non-date value follow the application's German culture setting and the standard handling of a
  mandatory date field; the exact message texts were not observed in a browser.
- Storing an empty "Name" or "CountyNumber" as "no value" (rather than empty text) follows the standard binding of empty
  form fields; the consequence for patients without county text (Edge Cases) is derived from the matching rule's test
  for two absent names and was not exercised.
- The "different counties normalize to the same text" case is derived from the normalization rules and the naming
  conventions of the GV100AD ("X, Stadt" next to "Landkreis X"); which concrete pairs exist in a given edition was not
  checked.
- The all-or-nothing behavior of the upload follows from the records being stored together after the whole file was
  read; the left-over temporary file follows from the absence of a delete step; neither was exercised.
- Refusal of Administrator-only and Public Health accounts means redirection to the sign-in page, as established in
  spec 021; accounts hold exactly one role.
- The GV100AD layout described (record type "40", positions of date, key, and name) is taken from the importer and its
  test data and is assumed to hold for the quarterly editions the laboratory uploads; other DeStatis layouts were not
  examined.
