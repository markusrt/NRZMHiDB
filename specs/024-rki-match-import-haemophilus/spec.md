# Feature Specification: RKI match import (Haemophilus)

**Feature Branch**: `speckit-init`

**Created**: 2026-10-30

**Status**: Draft (status-quo inventory)

**Input**: User description: "Feature title: \"RKI match import (Haemophilus)\" (backlog ref D2, Group D — Integrations). Document the current, as-implemented behavior of importing RKI match data for Haemophilus submissions from an uploaded file as it exists today. Read HaemophilusWeb/Controllers/RkiMatchController.cs, HaemophilusWeb/Tools/RkiMatchImporter.cs, HaemophilusWeb/Models/RkiMatchRecord.cs, HaemophilusWeb/Views/RkiMatch/Index.cshtml, the migration HaemophilusWeb/Migrations/201802182157080_RkiMatch.cs, HaemophilusWebTests/Tools/RkiMatchImporterTests.cs, and where the imported data is consumed (Models/Sending.cs, HaemophilusWeb/Tools/HaemophilusSendingLaboratoryExport.cs). Capture the upload page, the expected file format and columns, how rows are matched to submissions, what is stored or updated, the outcome messages, error handling, and the access model. Document observable behavior only — do not propose changes."

> **Scope note**: This is an *as-is* specification (backlog ref D2). It records the current, observable behavior of
> the import of RKI match data for Haemophilus submissions: the hidden upload page "RKI Matching Daten", the file it
> expects (a semicolon-separated text file with a header line and six named columns), how each row is matched to a
> Haemophilus submission (by stem number, then by laboratory number), which rows are skipped, what is stored or
> updated on the submission (RKI reference, RKI file number, RKI match status, with the "best status wins" rule), the
> two outcome messages, the all-or-nothing failure behavior, the access model, and the only place where the imported
> data becomes visible again (the Haemophilus laboratory export). It does **not** propose changes. Out of scope and
> referenced instead: sign-in, roles, and anti-forgery rules (spec 021, User Stories 4.6 and 7.5, access matrix row
> "RKI match import \"RKI Matching Daten\""); the Haemophilus laboratory export as a whole (spec 011, User Story 1
> scenario 5); the assignment and uniqueness of stem and laboratory numbers (spec 004); soft-deleted submissions
> (spec 020). The feature has no counterpart for Meningococci. No running system was exercised; everything is derived
> from code, version history, the change log, and the automated tests. No secret values are reproduced.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Upload the RKI match file (Priority: P1)

A laboratory employee receives from the RKI a table that tells, for each Haemophilus isolate the RKI could relate to
a notified case, the RKI's internal reference, its file number, and how certain the match is. The employee opens the
hidden page "RKI Matching Daten" by its address, chooses the file, presses "Upload", and reads one line of feedback.

**Why this priority**: The upload is the only way RKI match information enters the application; nothing else creates
or edits these records (change log: "Import-Funktion für RKI Match Datensätze", feature, 2018-02-20; "Überarbeitung
der RKI Match Funktion (Duplikate, Matching mit Labornummer).", feature, 2018-03-18; both without pathogen tag).

**Independent Test**: Sign in with an account of each role (Standard User, Administrator, Public Health, without
role) and request the page by direct address; upload (a) a well-formed file, (b) an empty file, (c) no file, (d) a file
with an unknown status word; observe the page, the feedback line, and the stored match data.

**Acceptance Scenarios**:

1. **Given** any signed-in account, **When** the page is requested by direct address, **Then** the page "RKI Matching
   Daten" opens inside the normal page frame with the role's navigation; it offers one file field labelled "CSV Datei"
   and one button "Upload"; nothing else is shown and no previous import is listed.
2. **Given** the page, **Then** it is not linked from any menu or page (navigation reviewed: the menus "Export",
   "PubMLST", and "Administration" and the Public Health menu contain no entry for it; spec 021, User Story 7.5).
3. **Given** no session, **When** the page or the upload is requested, **Then** it is refused (redirected to the
   sign-in page) by the general sign-in rule (spec 021, User Story 1).
4. **Given** a signed-in account of any role, including Public Health and Administrator-only, **When** a file is
   uploaded, **Then** the import runs: the page names no role and verifies no anti-forgery token (spec 021, User
   Stories 4.6 and 7.5, tagged there; not re-tagged here).
5. **Given** a non-empty file, **When** "Upload" is pressed and the import completes, **Then** the same page is shown
   again with the line "File Uploaded Successfully!!" under the button; the file field is empty again. The message is
   shown regardless of how many rows matched a submission — also when no row matched. [POTENTIAL-REFACTORING]
6. **Given** a file of zero length, **When** it is uploaded, **Then** nothing is imported and "File Uploaded
   Successfully!!" is shown nevertheless.
7. **Given** no file is chosen, **When** "Upload" is pressed, **Then** the page shows "File upload failed: Object
   reference not set to an instance of an object." (derived from code and framework behavior: the missing file arrives
   as "no file" and the length check fails on it; not exercised). [SUSPECTED-DEFECT]
8. **Given** the import fails for any reason (User Story 6), **Then** the page shows "File upload failed: `<message>`"
   where `<message>` is the technical message of the failure, in English, and nothing of the file is stored.
9. **Given** the uploaded file, **Then** its name and extension are not checked; the automated tests use the extension
   `.asc`. The file is first written to a temporary file in the server's temporary folder and read from there; that
   temporary file is not deleted afterwards (derived from code; not exercised). [SUSPECTED-DEFECT]

---

### User Story 2 - Read the file (Priority: P1)

The uploaded file is a plain-text table with a header line that names six columns; the application reads the four
numeric columns and two text columns it knows and ignores the rest.

**Why this priority**: The column names are the contract between the RKI's table and the application; a file with
other headers imports nothing and still reports success.

**Independent Test**: Upload the test table of the automated tests (header
`InterneRef;Aktenzeichen;StatusName;klhi_nr;LaufendeNummer;Jahr` followed by ten rows) against submissions with stem
numbers 2515, 2466, 2488, 2494, and one with laboratory number 010/17; verify the stored values listed in User
Story 4.

**Acceptance Scenarios**:

1. **Given** the file, **Then** it is read as UTF-8 text, one row per line, columns separated by semicolons, with the
   first line as header; columns are identified by their header name, not by position (derived from the reading
   library; the test file carries the headers in the order shown). Double-quoted values are accepted as the library
   defines them (derived; not exercised).
2. **Given** the header, **Then** the recognized columns are `InterneRef`, `Aktenzeichen`, `StatusName`, `klhi_nr`,
   `LaufendeNummer`, and `Jahr` (see "File columns"); columns with other names are ignored, and a recognized column
   that is absent is treated as empty in every row.
3. **Given** a row, **Then** `InterneRef`, `klhi_nr`, `LaufendeNummer`, and `Jahr` are read as whole numbers; an empty
   cell means "not given"; a negative number is accepted as a number (the test file uses `-12` and `-234` as stem
   numbers that match nothing).
4. **Given** a row whose numeric cell contains text that is not a whole number, **Then** reading fails and the whole
   import ends with "File upload failed: `<message>`" (derived from the reading library; not covered by a test).
5. **Given** the column `StatusName`, **Then** its value is interpreted as one of the four RKI match states (see
   "RKI match states"): the German words "kein Match", "möglich", "wahrscheinlich", "definitiv" regardless of case;
   also accepted by the shared helper are the internal English names (None, Possible, Probable, Definite), the
   numbers 0–3, and any text that is the beginning of one of the German words (for example "mög"); an empty cell
   means "kein Match" (derived from the shared helper; the test file covers the four German words and the empty
   cell). [POTENTIAL-REFACTORING]
6. **Given** a `StatusName` that none of these rules accept, **Then** the import fails with the helper's message,
   shown as "File upload failed: 'xyz' is not a valid value for 'RkiStatus'; valid values are 'None (0), Possible (1),
   Probable (2), Definite (3)'" (derived from code; not covered by a test). Because the failure ends the whole import,
   one unknown word in one row prevents every other row from being stored (User Story 6).
7. **Given** a file with no data rows, or a file whose header matches no recognized column, **Then** nothing is
   stored and "File Uploaded Successfully!!" is shown.

---

### User Story 3 - Match a row to a Haemophilus submission (Priority: P1)

Each row names the isolate by its stem number ("Stammnummer", column `klhi_nr`) and may additionally give the two
parts of the laboratory number ("Labornummer", columns `LaufendeNummer` and `Jahr`); the application finds the
Haemophilus submission that owns that isolate.

**Why this priority**: Matching decides which submission receives the RKI data; the laboratory-number fallback was
added on 2018-03-18 (version history: "It is possible to reference an isolate via yearly sequential number as well").

**Independent Test**: Upload rows (a) with the stem number of an existing isolate, (b) with a stem number that exists
nowhere plus the yearly number and year of an existing isolate, (c) with an empty stem number but a valid yearly
number and year, (d) with a stem number of a submission received in 2008, (e) with a stem number but without
`InterneRef`; verify which submissions receive a match record.

**Acceptance Scenarios**:

1. **Given** a row without `klhi_nr`, **Then** the row is skipped entirely, even when `LaufendeNummer` and `Jahr` are
   filled (the laboratory-number path is only tried for rows that carry a stem number; the test for this path uses
   the placeholder stem number `-12`). [SUSPECTED-DEFECT]
2. **Given** a row with `klhi_nr`, **Then** the Haemophilus submission whose isolate has exactly this stem number
   (the number without the display prefix "H") is looked up; Meningococci submissions are never considered.
3. **Given** no submission with that stem number, and both `LaufendeNummer` and `Jahr` filled, **Then** the submission
   whose isolate has this yearly sequential number and this four-digit year is looked up (the pair that forms the
   laboratory number, for example 10 and 2017 for "010/17"; spec 004). When either of the two is empty, the row is
   skipped.
4. **Given** a submission found by either path, **Then** it is used only when its receiving date ("Eingangsdatum") is
   2009-01-01 or later; submissions received before 2009 are left untouched (tests with 2008-12-31 and 2005-01-13).
5. **Given** a submission found and old enough, **Then** the row is used only when `InterneRef` is filled; a row
   without `InterneRef` is skipped (test: last row of the test file).
6. **Given** the lookup by stem number or by laboratory number finds more than one submission, **Then** the import
   fails ("Sequence contains more than one element") and nothing is stored. Stem and laboratory numbers are kept
   unique by the isolate form (spec 004), so this is not expected for active data (derived from code; not exercised).
7. **Given** a soft-deleted submission (spec 020) with the given stem or laboratory number, **Then** it is found and
   updated like an active one; the deletion mark is not consulted (derived from code; not covered by a test).
   [POTENTIAL-REFACTORING]
8. **Given** a stem number that occurs in several rows, **Then** every row is processed in file order against the same
   submission (User Story 4).

---

### User Story 4 - Store or update the match state of a submission (Priority: P1)

For each matched row the submission receives, or updates, exactly one RKI match record with reference, file number,
and state; a better or equal state replaces an existing one, a worse one is ignored.

**Why this priority**: This is the data the feature exists for; the "best status wins" rule was introduced on
2018-03-18 for duplicate rows (version history: "For duplicate entries the best status should be used").

**Independent Test**: Run the test file against a submission with stem number 2515 (rows "definitiv" then
"wahrscheinlich"), against 2466 that already carries "möglich" with reference 123 and file number "abc", and against
2494 (one row "wahrscheinlich"); verify the stored reference, file number, and state of each.

**Acceptance Scenarios**:

1. **Given** a matched submission without RKI match record, **When** a row is applied, **Then** a record is created
   that belongs to this submission alone (one record per submission at most) and carries `InterneRef` as "InterneRef",
   `Aktenzeichen` as "RKI Aktenzeichen", and the interpreted `StatusName` as "RKI Status". A new record is always
   written, whatever the state — also "kein Match" (test: stem number 2488 receives "kein Match" with reference
   103766885 and file number "S51-021804901").
2. **Given** a matched submission that already carries a record, **When** the row's state is higher than or equal to
   the stored state, **Then** reference, file number, and state are all replaced by the row's values (test: stored
   "möglich"/123/"abc" becomes "möglich"/10376685/"S51-021804901" for an incoming "möglich").
3. **Given** a matched submission that already carries a record, **When** the row's state is lower than the stored
   state, **Then** the row is ignored and the stored values stay (test: "definitiv" stays although a later row says
   "wahrscheinlich" for the same stem number 2515).
4. **Given** the order "kein Match" < "möglich" < "wahrscheinlich" < "definitiv", **Then** the import can only keep or
   raise a submission's state and never lower or remove it; a later RKI correction downwards (for example from
   "definitiv" to "kein Match") is not taken over, and no page of the application shows or edits the stored record
   (derived from code; the only reader is the export of User Story 5). [POTENTIAL-REFACTORING]
5. **Given** several submissions matched by one file, **Then** each receives its own record (test: three submissions,
   three records).
6. **Given** the file is applied, **Then** all records of one upload are saved together at the end; the patient,
   submission, and isolate data themselves are not changed, and no other field of the submission is touched.
7. **Given** a record, **Then** it is stored in the table created by the migration `RkiMatch` of 2018-02-18 with the
   submission number as key, the RKI reference as whole number, the RKI file number as free text, and the state as a
   number (Constitution Principle V: a reviewed migration with a working reverse step).

---

### User Story 5 - See the imported data (Priority: P2)

The only place where the imported match data becomes visible is the Haemophilus laboratory export.

**Why this priority**: Without the export the records would be write-only; spec 011 already names the three columns.

**Independent Test**: Run "Labor Export" (Haemophilus) over a period containing a submission with a stored record and
one without; compare the three RKI columns.

**Acceptance Scenarios**:

1. **Given** the Haemophilus laboratory export (spec 011, User Story 1 scenario 5: "the RKI match reference, file
   number, and status"), **Then** its last three columns are "RKI InterneRef", "RKI Aktenzeichen", and "RKI Status",
   after "Bemerkung (Isolat)".
2. **Given** a submission with a record, **Then** the columns carry the stored reference as number, the file number as
   text, and the state as its German word ("kein Match", "möglich", "wahrscheinlich", "definitiv"; test: "möglich").
3. **Given** a submission without a record, **Then** "RKI InterneRef" and "RKI Aktenzeichen" are empty and "RKI
   Status" is "kein Match" — the same word as a stored state "kein Match"; the export does not distinguish "never
   matched by the RKI" from "matched with result: no match" (derived from code; not covered by a test).
4. **Given** the Haemophilus RKI export, the PubMLST export, the sender exports, the submission pages, the isolate
   page, and the report (specs 004, 008, 011, 013), **Then** none of them shows or exports the RKI match record.
5. **Given** a Haemophilus patient merge (spec 014), **Then** the record stays with its submission, which moves to the
   kept patient; the merge does not touch it (derived from the record's ownership; not covered by a test).

---

### User Story 6 - Failures, all-or-nothing, and traces (Priority: P3)

A laboratory employee wants to know what happens when an upload fails and what the application records about an
import.

**Why this priority**: Importer runs are data-changing bulk operations; the constitution asks that they be explicit,
auditable, and recoverable (Principle I).

**Independent Test**: Upload a file whose fifth row carries an unknown status word while the first four rows match
existing submissions; verify that none of the four received a record and that the page shows the failure line.
Inspect the application log afterwards.

**Acceptance Scenarios**:

1. **Given** any failure while reading or applying the file (unknown status word, non-numeric cell, ambiguous lookup,
   database error), **Then** the whole import is rolled back: no record created or changed by this upload is kept, the
   page shows "File upload failed: `<message>`", and the user may correct the file and upload again.
2. **Given** a failure, **Then** the failure is not written to the application log: the page catches every error
   itself and shows its message, so the general error logging ("Unhandled error: …", spec 021) is not reached
   (derived from code). [POTENTIAL-REFACTORING]
3. **Given** a successful import, **Then** nothing records that it happened: no log entry, no import date on the
   record, no count of matched or skipped rows, no name of the uploading account, and no copy of the file inside the
   application (the temporary server file of User Story 1.9 aside). The only trace is the changed records themselves,
   which carry no timestamp (Constitution Principle I, "importer runs MUST be explicit, auditable"; Technology &
   Platform Constraints on logging). [POTENTIAL-REFACTORING]
4. **Given** a file larger than the platform's default request size (about 4 MB; no explicit limit is configured),
   **Then** the request is rejected before the page runs, with the generic error page rather than the "File upload
   failed" line (derived from configuration and framework behavior; not exercised).
5. **Given** two uploads of the same file, **Then** the second changes nothing (every row meets an equal state and
   rewrites the same values) and reports success again.

### Edge Cases

- **Row for a submission without isolate**: Every Haemophilus submission owns an isolate created on save (spec 002); a
  submission without isolate can neither be found by stem number nor by laboratory number and is simply never matched.
- **Stem number that exists only as a Meningococci isolate**: Not found; Meningococci data ("DE" numbers) is outside
  this feature.
- **Row with `klhi_nr` of a submission received before 2009 and a laboratory number of a newer one**: The stem-number
  path finds the old submission first, so the laboratory-number path is not tried and the row is skipped because of
  the 2009 rule (derived from code; not exercised).
- **Same `InterneRef` in rows for different stem numbers**: Each submission receives its own record with the same
  reference; the reference is not required to be unique.
- **Status word with surrounding spaces**: Not trimmed by the helper; " möglich" with a leading space is not the
  beginning of any German word and fails the import (derived from the shared helper; not exercised).
- **Header spelled differently** (for example `KLHI_NR` or `Klhi_Nr`): Whether the column is still recognized depends
  on the reading library's name matching and is not fixed by a test; the test file uses the exact spellings listed
  under "File columns".
- **Semicolon inside the file number**: `Aktenzeichen` is free text; an unquoted semicolon shifts the following columns
  of that row (derived from the separator rule; not exercised).
- **Global separator setting**: Reading the file switches the shared text-table separator of the application to the
  semicolon and leaves it so; today no other function reads or writes such tables, so nothing else is affected
  (derived from code). [POTENTIAL-REFACTORING]
- **Privacy** (Constitution Principle IV): The file carries no patient identifiers, only RKI references, RKI file
  numbers, status words, and isolate numbers; the stored record links an RKI case reference to a submission that
  holds identifiable patient data. The page is open to every signed-in account including Public Health (spec 021,
  tagged there) and leaves the uploaded file in the server's temporary folder (User Story 1.9).
- **Architecture** (Constitution "Known Architectural Constraints (Legacy Debt)", Principles II and III): The page
  group creates its own database access and hands it to the importer; the importer carries reading, matching, and
  updating in one unit, which the tests exercise with a substituted in-memory database. The code comment of the
  importer still describes `klhi_nr` as the submission number, which it was until 2018-03-18; since then it is the stem
  number. The tests cover the four German status words and the empty cell, the stem-number and laboratory-number
  paths, the 2009 rule, the "best status wins" rule with an existing record, and several submissions per file; they
  do not cover the page itself (no test exists for the page group), unknown status words, non-numeric cells, rows with
  laboratory number but without stem number, soft-deleted submissions, or the all-or-nothing behavior (the test
  database applies changes without a transaction). [POTENTIAL-REFACTORING]

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The application MUST offer the page "RKI Matching Daten" by direct address only, with one file field
  "CSV Datei" and one button "Upload", to every signed-in account regardless of role, MUST refuse it without sign-in,
  MUST NOT verify an anti-forgery token on the upload, and MUST NOT link it from the navigation (current behavior;
  spec 021).
- **FR-002**: The application MUST read the uploaded file as UTF-8 text with semicolon-separated columns and a header
  line, MUST recognize the columns `InterneRef`, `Aktenzeichen`, `StatusName`, `klhi_nr`, `LaufendeNummer`, and
  `Jahr` by name, and MUST ignore other columns; the file name and extension MUST NOT be checked.
- **FR-003**: The application MUST read `InterneRef`, `klhi_nr`, `LaufendeNummer`, and `Jahr` as optional whole
  numbers and MUST end the import with a failure when a numeric cell cannot be read as a whole number.
- **FR-004**: The application MUST interpret `StatusName` as one of the four states "kein Match", "möglich",
  "wahrscheinlich", "definitiv" (case-insensitive; empty means "kein Match"; the shared helper's further accepted
  forms are documented in User Story 2.5) and MUST end the import with a failure for any other value.
- **FR-005**: The application MUST skip every row without `klhi_nr`, MUST look up the Haemophilus submission whose
  isolate has the stem number `klhi_nr`, and, when none exists and both `LaufendeNummer` and `Jahr` are given, MUST
  look up the submission whose isolate has this yearly sequential number and year.
- **FR-006**: The application MUST apply a row only to a submission whose receiving date is 2009-01-01 or later and
  only when `InterneRef` is given; otherwise the row MUST be skipped without notice.
- **FR-007**: The application MUST keep at most one RKI match record per submission, holding the RKI reference
  (`InterneRef`), the RKI file number (`Aktenzeichen`), and the state.
- **FR-008**: For a submission without record, the application MUST create the record with the row's values whatever
  the state; for a submission with record, it MUST replace reference, file number, and state when the row's state is
  equal to or higher than the stored state in the order "kein Match" < "möglich" < "wahrscheinlich" < "definitiv",
  and MUST leave the record unchanged when the row's state is lower.
- **FR-009**: The application MUST apply all rows of one upload together and MUST keep none of them when any row
  fails ("all or nothing").
- **FR-010**: After the upload the application MUST show the same page with "File Uploaded Successfully!!" when no
  failure occurred (including for an empty file or a file in which no row matched), and "File upload failed:
  `<message>`" with the failure's technical message otherwise.
- **FR-011**: The Haemophilus laboratory export MUST carry the stored record in the columns "RKI InterneRef", "RKI
  Aktenzeichen", and "RKI Status" (German state word), with empty reference and file number and the word "kein Match"
  for submissions without record (spec 011); no other page or export MUST show the record (current behavior).
- **FR-012**: The application MUST NOT log, count, or otherwise record an import or its failure beyond the changed
  records themselves (current behavior).
- **FR-013**: This feature MUST document the current behavior only and MUST NOT introduce new columns, matching rules,
  messages, access rules, or logging.

### File columns

| Column header | Meaning | Type | Required for a row to be used |
| --- | --- | --- | --- |
| `InterneRef` | RKI internal reference of the matched case; stored as "InterneRef" | Whole number | Yes (row skipped when empty) |
| `Aktenzeichen` | RKI file number; stored as "RKI Aktenzeichen" | Free text | No (stored as given, may be empty) |
| `StatusName` | Certainty of the match; stored as "RKI Status" | One of the state words (see below); empty = "kein Match" | Must be interpretable; otherwise the import fails |
| `klhi_nr` | Stem number ("Stammnummer") of the Haemophilus isolate, without the display prefix "H" | Whole number | Yes (row skipped when empty) |
| `LaufendeNummer` | Yearly sequential part of the laboratory number ("Labornummer", for example 10 for "010/17") | Whole number | Only for the fallback, together with `Jahr` |
| `Jahr` | Four-digit year part of the laboratory number (for example 2017 for "010/17") | Whole number | Only for the fallback, together with `LaufendeNummer` |

### RKI match states

| Word in the file (case-insensitive) | Stored state | Rank | Shown in the laboratory export as |
| --- | --- | --- | --- |
| "kein Match" or empty cell | None | 0 | "kein Match" (also shown for submissions without record) |
| "möglich" | Possible | 1 | "möglich" |
| "wahrscheinlich" | Probable | 2 | "wahrscheinlich" |
| "definitiv" | Definite | 3 | "definitiv" |

A row replaces a stored record only when its rank is equal to or higher than the stored rank; a new record starts
below rank 0, so the first applied row always writes.

### Matching summary

| Step | Rule | Row skipped or import failed when |
| --- | --- | --- |
| Stem number present | `klhi_nr` filled | Skipped: `klhi_nr` empty (even with laboratory number) |
| Lookup by stem number | Haemophilus isolate with exactly this stem number | — (continue with next step when none) |
| Lookup by laboratory number | Only when no stem-number match and both `LaufendeNummer` and `Jahr` filled | Skipped: none found, or one of the two empty |
| Ambiguity | Exactly one submission per lookup | Import failed: more than one submission found |
| Receiving date | 2009-01-01 or later | Skipped: received before 2009 |
| Reference | `InterneRef` filled | Skipped: empty |
| State | Interpretable `StatusName` | Import failed: unknown word |
| Rank | Row rank ≥ stored rank (or no record yet) | Skipped: row rank lower than stored |

### Roles and Boundaries

| Actor or role | Current observable involvement |
| --- | --- |
| Laboratory employee (Standard User) | Opens the page by address, uploads the RKI table, reads the one-line feedback; sees the result only in the Haemophilus laboratory export. |
| Administrator (without Standard User role) | Admitted to the page and the upload like any signed-in account (spec 021, User Story 4.4); cannot run the laboratory export. |
| Public Health (`RKI`) | Admitted to the page and the upload by direct address although its navigation shows only the two RKI exports (spec 021, User Story 4.6, tagged there). |
| Account without role | Admitted like any signed-in account (spec 021). |
| Anonymous visitor | Refused (redirected to the sign-in page). |
| RKI (external) | Produces the table outside the application; the application only reads the six columns described here. |
| Operator with database access | The only party able to lower, correct, or remove a stored record. |

### Key Entities *(include if feature involves data)*

- **RKI match record**: Belongs to exactly one Haemophilus submission (the submission number is its key); holds the
  RKI reference ("InterneRef", whole number), the RKI file number ("RKI Aktenzeichen", text), and the RKI match
  state ("RKI Status", one of four ranked values). Created on 2018-02-18 by migration; written only by the import,
  read only by the Haemophilus laboratory export.
- **Haemophilus submission**: Supplies the receiving date for the 2009 rule and owns the record; linked to one
  isolate (spec 002) and one patient.
- **Haemophilus isolate**: Supplies the stem number ("Stammnummer") and the laboratory number parts (yearly sequential
  number and year, "Labornummer"; spec 004) by which rows are matched.
- **Import row**: One line of the uploaded table with the six recognized columns; exists only during the upload and
  is not stored.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: In 100% of uploads by a signed-in account of any role, the page accepts the file and answers with one
  of the two feedback lines; 100% of requests without sign-in are refused.
- **SC-002**: For the ten-row test table and submissions with stem numbers 2515, 2466, 2488, 2494 and laboratory
  number 010/17, exactly five records result with the states "definitiv", "möglich", "kein Match", "wahrscheinlich",
  and "möglich" and the references 11357917, 10376685, 103766885, 10340481, and 103766775 respectively; the rows
  without stem number, without reference, or with unknown stem numbers produce no record.
- **SC-003**: In 100% of cases, a stored record is replaced only by a row of equal or higher rank; a lower-ranked row
  changes 0 fields.
- **SC-004**: In 100% of cases, a submission received before 2009-01-01 receives no record from any upload.
- **SC-005**: In 100% of uploads that fail on any row, 0 records of that upload are kept and the page shows the
  failure line with the technical message.
- **SC-006**: In 100% of Haemophilus laboratory exports, every submission with a record shows its reference, file
  number, and German state word in the three RKI columns, and every submission without record shows two empty cells
  and "kein Match".
- **SC-007**: After any number of imports, 0 log entries and 0 stored import metadata (date, account, counts)
  describe them.

## Assumptions

- Current application code, version history (commits of 2018-02-19 "RKI Match import tool, view and controller",
  2018-03-12 "Small fixes to RKI match export", 2018-03-18 "New requirements for RKI match import"), the change log
  ("Über" page data: "Import-Funktion für RKI Match Datensätze", feature, 2018-02-20; "Überarbeitung der RKI Match
  Funktion (Duplikate, Matching mit Labornummer).", feature, 2018-03-18), and the automated tests are the source of
  truth. No running system was exercised and no real RKI file was available; the file layout is known from the
  importer and its test table only.
- Header-based column recognition, acceptance of quoted values, the failure on non-numeric cells, and the
  case-sensitivity of header names are derived from the behavior of the reading library and are not fixed by tests.
- The further accepted status forms (English names, numbers 0–3, word prefixes) and the exact failure message for an
  unknown status word are derived from the shared enumeration helper; the tests cover only the four German words and
  the empty cell.
- The behavior when no file is chosen (technical "object reference" message), when the request exceeds the platform's
  default size, when a lookup is ambiguous, and when a soft-deleted submission is matched is derived from code and
  framework behavior; none of it is exercised or tested.
- The left-over temporary file is derived from the absence of a delete step; the server's temporary folder and its
  housekeeping are outside the application.
- The import is assumed to be the only writer of RKI match records and the Haemophilus laboratory export the only
  reader; a search over the application's pages and exports found no other use.
- Spec 021 is taken as authoritative for sign-in, role, and anti-forgery behavior and is referenced, not re-derived;
  spec 011 is taken as authoritative for the export as a whole. No difference from either was found; this spec only
  adds the "kein Match" default and the column positions for the export.
- Labels are quoted as shown to users; column headers are quoted as they appear in the test table.
