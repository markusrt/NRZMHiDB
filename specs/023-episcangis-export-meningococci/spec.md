# Feature Specification: EpiScanGIS export (Meningococci)

**Feature Branch**: `speckit-init`

**Created**: 2026-10-30

**Status**: Draft (status-quo inventory)

**Input**: User description: "Feature title: \"EpiScanGIS export (Meningococci)\" (backlog ref B4, Group B — Data exports & epidemiology). Document the current, as-implemented behavior of the token-protected, automated EpiScanGIS export of Meningococci cases as it exists today. Read HaemophilusWeb/Controllers/EpiscanGisExportController.cs, its use of the Geonames service for coordinates, and HaemophilusWebTests/Controllers/EpiscanGisExportControllerTests.cs. Capture how the export is addressed and authorized (token in the request, no sign-in), which cases are selected and how one line per patient is built, the output format, columns, and value encodings, how coordinates are looked up and when a case is left out, and what is logged. Document observable behavior only — do not propose changes."

> **Scope note**: This is an *as-is* specification (backlog ref B4). It records the current, observable behavior of
> the EpiScanGIS export of invasive Meningococci cases: how the export is addressed and admitted (a secret token in the
> address, no sign-in), which submissions are selected, how the cases of one patient are reduced to one line, the
> file that is delivered (name, format, columns, value encodings), how the geographic coordinates of each case are
> obtained from the external Geonames service and when a case is left out, what happens when something fails, and what
> is logged. It does **not** propose changes. Out of scope and referenced instead: the sign-in and role rules and the
> shared `RegistrationToken` secret (spec 021, User Story 7, FR-012, access matrix); the postal-code lookup itself and
> its `GeonamesAccount` setting (spec 022, User Stories 1 and 4); the derivation of the serogroup from the isolate
> (spec 007); the merge of Meningococci patients, which decides what "one patient" is (spec 015); soft-deleted
> submissions (spec 020); the seven Export-menu exports and their selection rules (spec 011); the settings table
> (spec 018). The values of the `RegistrationToken` and `GeonamesAccount` settings are named but never reproduced.
> The external service was not called and no running system was exercised; everything is derived from code, version
> history, the change log, and automated tests.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Download the export automatically with a token (Priority: P1)

An external tool (the EpiScanGIS import) fetches the current Meningococci case file from the application at any time
by requesting the export address with the agreed secret token; nobody signs in and nobody clicks anything.

**Why this priority**: The export exists only for this automated hand-over; since 2022-10-16 it has no place in the
navigation (change log entry "Automatischer EpiScanGIS Export", Meningococci; the version history notes that the menu
entry was removed because the export is called by a tool only).

**Independent Test**: Request the export address without a session (a) with the value of the `RegistrationToken`
setting as `token`, (b) with any other value, (c) without a token, (d) with a request method other than a plain
retrieval; repeat (a) and (b) with a signed-in account of each role. Verify what is answered in each case.

**Acceptance Scenarios**:

1. **Given** no session, **When** the export address is requested with a `token` parameter whose value equals the
   value of the `RegistrationToken` setting exactly (same characters, same case), **Then** a file download of the
   Meningococci case file (User Stories 2–5) is answered; no sign-in page and no page of the application is shown.
2. **Given** no session, **When** the export address is requested with a token that differs from the setting in any
   way, or without a token, **Then** the answer is "not found", as for an address that does not exist; nothing hints
   that the address is valid (spec 021, SC-006).
3. **Given** any signed-in account (Standard User, Administrator, Public Health, or without role), **Then** the same
   two rules apply: the token decides, the account and its role do not (spec 021, access matrix row "EpiScanGIS
   export (token in address)").
4. **Given** the export address is requested with a method other than a plain retrieval (for example a form post),
   **Then** the answer is "not found" (derived from framework behavior; spec 021, User Story 7.1, tagged there).
5. **Given** the export, **Then** it is not offered in the Export menu or anywhere else in the navigation (spec 011,
   scope note; version history: menu entry "EpiScanGIS Export" existed from 2022-01-31 to 2022-10-16), and no
   anti-forgery token is verified, because the request carries no form.
6. **Given** the same `RegistrationToken` setting, **Then** its value both opens the registration form and the export,
   and it travels as part of the address (spec 021, User Story 7.2, tagged there; spec 018 names the setting).

---

### User Story 2 - Select the cases to be exported (Priority: P1)

The file contains the invasive Meningococci cases since autumn 2019 whose serogroup is known.

**Why this priority**: The selection determines which patients reach the epidemiological map at all; it is fixed in
the application and cannot be influenced by the caller.

**Independent Test**: With Meningococci submissions of every sampling location, with and without sampling date, on
both sides of 2019-10-01, soft-deleted and active, and with isolates whose interpretation yields a serogroup, `NG`,
`cnl`, or none, run the export and compare the patient numbers in the file with the stored data.

**Acceptance Scenarios**:

1. **Given** the export runs, **Then** it considers Meningococci submissions only; Haemophilus submissions are never
   part of it. The caller cannot pass a date range, a filter, or any other parameter besides the token.
2. **Given** a Meningococci submission, **Then** it is considered only when it is not soft-deleted (spec 020, "What
   deletion changes for submissions").
3. **Given** a submission, **Then** it is considered only when its sampling location ("Entnahmeort") is exactly "Blut"
   or exactly "Liquor". The combined location "Blut und Liquor" and the other locations classified as invasive
   ("Gelenkspunktat", "Petechien", "Pleurapunktat", "Serum", "Anderer invasiv") are left out, although the RKI, IRIS,
   and LGA exports include them (spec 011, FR-011). This is a difference from the selection documented in spec 011,
   not a repetition of it. [SUSPECTED-DEFECT]
4. **Given** a submission with a sampling date ("Entnahmedatum"), **Then** it is considered only when the sampling date
   is later than 2019-10-01; **given** a submission without sampling date, **Then** it is considered only when its
   receiving date ("Eingangsdatum") is 2019-10-01 or later. A case sampled exactly on 2019-10-01 is therefore left
   out while a case received on that day without sampling date is included (derived from code; not covered by a
   test). There is no end date: every later case is included on every run. [SUSPECTED-DEFECT]
5. **Given** a considered submission, **When** the export runs, **Then** the clinical interpretation of its isolate is
   computed afresh with the current rules (spec 007) and its normalized serogroup is taken; the case is left out when
   no serogroup results or when the serogroup is `NG` (non-groupable) or `cnl` (capsule null locus). Cases whose
   interpretation concludes "no meningococci" or whose typing is not yet complete therefore do not appear, and a case
   can appear in a later run once its data is completed.
6. **Given** a considered submission with a serogroup, **Then** it is kept only when coordinates are found for its
   patient (User Story 5); otherwise it is left out without notice.
7. **Given** the kept cases, **Then** they are reduced to one line per patient number (User Story 4) and written in
   ascending order of patient number (derived from the order in which the cases are read; the container that collects
   the lines preserves that order in practice but does not guarantee it).

---

### User Story 3 - Read one line of the file (Priority: P1)

Each line of the file describes one patient's case in ten fixed columns that the EpiScanGIS import understands.

**Why this priority**: The column layout is the contract with the receiving system (version history 2022-01-15:
"Adapt export to match import format"); the automated test fixes it literally.

**Independent Test**: Create a case with known patient number, receiving date 2022-01-10, sampling date 2022-01-02,
birth date 2012-01-01, serogroup C, PorA VR1 "22", VR2 "14", FetA VR "5-5", and a patient whose postal code resolves to
49.71754 / 11.05877; run the export and compare the line with
`1;10.01.2022;02.01.2022;C;22;14;5-5;10;49.71754;11.05877`.

**Acceptance Scenarios**:

1. **Given** the file, **Then** it is a plain-text file announced as CSV, named `EpiscanGis_Export_<yyyyMMdd>.csv`
   with the server's current date (for example `EpiscanGis_Export_20261030.csv`), one case per line, columns
   separated by semicolons, no header line, no quoting, and no trailing separator.
2. **Given** a line, **Then** its ten columns are, in this order: (1) the patient number; (2) the receiving date;
   (3) the sampling date; (4) the serogroup; (5) the PorA VR1 variant; (6) the PorA VR2 variant; (7) the FetA VR
   variant; (8) the patient's age at sampling in whole years; (9) the latitude; (10) the longitude (see "Columns and
   value encodings").
3. **Given** the dates, **Then** they are written as day.month.year with two-digit day and month and four-digit year
   ("10.01.2022"); a missing sampling date yields an empty column 3 (two adjacent semicolons), unlike the Export-menu
   exports, which write "keine Angabe" (spec 011).
4. **Given** the variants, **Then** PorA VR1, PorA VR2, and FetA VR are written exactly as stored in the isolate
   ("PorA VR1", "PorA VR2", "FetA VR" on the isolate page, spec 005); a variant that was not recorded yields an empty
   column. The values are not escaped: a stored value containing a semicolon or a line break would shift or split the
   columns (derived from code; not exercised). [POTENTIAL-REFACTORING]
5. **Given** the age, **Then** it is the number of full years between the patient's birth date and the sampling date,
   or the receiving date when no sampling date is stored; **given** no birth date, **Then** the age is written as `0`,
   which cannot be told apart from an infant (same rule and same finding as the age columns in spec 011, Edge Cases).
   [SUSPECTED-DEFECT]
6. **Given** the coordinates, **Then** latitude and longitude are written as decimal numbers with a point as decimal
   separator, with the precision delivered by the external service, and are never empty, because cases without
   coordinates are left out (User Story 5).
7. **Given** the file, **Then** it contains no initials, birth date, gender, postal code, place name, county, sender,
   or laboratory number; the patient number is the same internal number that the RKI and LGA exports write
   (spec 011) and that the patient and submission pages show as "Patient".
8. **Given** no case passes the selection, **Then** an empty file with the same name is delivered.

---

### User Story 4 - One line per patient (Priority: P2)

A patient with several exported cases appears once, with the line that carries the most data.

**Why this priority**: EpiScanGIS counts persons; the version history (2022-01-30: "Remove empty serogroup and duplicate
patients") and a dedicated test fix this rule.

**Independent Test**: Give two submissions the same patient number, one with serogroup A and an empty PorA VR1, the
other with serogroup B and full variants; run the export and verify that exactly one line starts with that patient
number and that it carries `B;22;14;5-5`.

**Acceptance Scenarios**:

1. **Given** several kept cases with the same patient number, **Then** the file contains exactly one line for that
   patient number.
2. **Given** the candidates, **Then** the line that is kept is the one with the most characters; a candidate with the
   same number of characters as the current line does not replace it, so with equal lengths the case read first wins
   (within one patient the order in which cases are read is not fixed). Length stands in for "most data": a case
   without sampling date, or with fewer or shorter variant values, yields a shorter line and loses, whatever its date;
   two complete lines of equal length are decided by reading order. [POTENTIAL-REFACTORING]
3. **Given** a patient merge (spec 015), **Then** the moved submissions share the kept patient's number from then on
   and the export emits one line for the merged person (spec 015, "EpiScanGIS export grouping").
4. **Given** a patient whose cases all fail the selection or lack coordinates, **Then** the patient has no line.

---

### User Story 5 - Locate the case on the map (Priority: P2)

Each exported case carries the coordinates of the patient's place of residence, resolved from the patient's postal
code and place name.

**Why this priority**: Coordinates are what makes the file usable for a geographic information system; without them a
case is left out entirely.

**Independent Test**: With a substituted lookup that answers a fixed coordinate for every request, verify that every
selected case carries that coordinate; with a lookup that answers nothing for one case, verify that this case is
absent; verify that the lookup receives the patient's "Postleitzahl" and "Wohnort".

**Acceptance Scenarios**:

1. **Given** a case that passed the selection of User Story 2 (scenarios 2–5), **When** the export runs, **Then** the
   external Geonames service is asked once for that case with the patient's "Postleitzahl" and "Wohnort" (spec 022,
   User Story 4, scenario 1); the question is asked per case, before the reduction to one line per patient, so a
   patient with several cases is looked up once per case. [POTENTIAL-REFACTORING]
2. **Given** the answer, **Then** the place is chosen under the same rule as the forms (spec 022, FR-005): the place
   whose name equals "Wohnort" exactly, or — when "Wohnort" is empty — the first place returned for the postal code;
   its latitude and longitude become columns 9 and 10.
3. **Given** no place is chosen (unknown postal code, empty postal code, a "Wohnort" spelled differently from the
   service's place name, the placeholder "Unbekannt", or a "not found" answer of the service), **Then** the case is
   left out of the file without notice (spec 022, User Story 4, scenario 2, tagged there).
4. **Given** the lookups, **Then** they are made one after the other for every selected case on every run, over plain
   HTTP, using the quota of the configured Geonames account (spec 022, Edge Cases, tagged there). Because the time
   window has no end and the start date is fixed (User Story 2.4), the number of lookups and the duration of one run
   grow with the database. [POTENTIAL-REFACTORING]
5. **Given** a patient living abroad, **Then** the lookup still searches German postal codes only (spec 022); the case
   is exported only if the foreign postal code happens to match a German one whose place name equals "Wohnort" —
   then with German coordinates (derived from code; not exercised).

---

### User Story 6 - Failures and logging (Priority: P3)

An operator wants to know what the application records about the export and what happens when a step fails.

**Why this priority**: The export is unattended; nobody sees an error page, and the only traces are in the log.

**Independent Test**: Run the export with a wrong token, with a correct token, with a lookup that answers "not found"
for one case, and with a lookup that fails with a network error; inspect the answer and the application log after each
run.

**Acceptance Scenarios**:

1. **Given** a request with a wrong, missing, or correct token, **Then** the export itself writes nothing to the
   application log: neither the attempt, nor the token outcome, nor the number of cases exported or left out, nor the
   requester. Rejected attempts are therefore not visible in the application log (Constitution Principle IV and the
   logging constraint under "Technology & Platform Constraints"); only the web server's own request log, if kept,
   records the requests, including the token in the address (derived; not exercised). [POTENTIAL-REFACTORING]
2. **Given** the external service answers "not found" for a case, **Then** the lookup component logs an error whose
   message starts with a stray "$" ("$Failed to call Geonames PostalCodeLookupJSON REST API.", spec 022, tagged there)
   and the case is left out; the export continues and the file is delivered.
3. **Given** the external service cannot be reached, times out, or answers in an unexpected form for any one case,
   **Then** the whole run ends with the generic server error: no file is delivered, the error is logged by the general
   error logging ("Unhandled error: …"), and the cases already processed are discarded. An empty temporary file
   created on the server for the run remains behind in that case (derived from code; not exercised).
   [SUSPECTED-DEFECT]
4. **Given** a successful run, **Then** the lines are first written to a temporary file in the server's temporary
   folder, read back, delivered, and the temporary file is deleted; nothing of the run is kept on the server.
   [POTENTIAL-REFACTORING]
5. **Given** a successful run, **Then** nothing in the application records that it happened; the patient, submission,
   and isolate records are not changed or marked.

### Edge Cases

- **Missing `RegistrationToken` setting**: A request without token would be admitted (spec 021, Edge Cases, tagged
  there); the setting exists in the main configuration file, its template, and both build transforms.
- **Submission without isolate**: Every Meningococci submission owns an isolate created on save (spec 003); should a
  selected submission have none, the interpretation step would fail and the run would end with the generic server
  error as in User Story 6.3 (derived from code; not exercised).
- **Serogroup values**: Column 4 carries the normalized serogroup of spec 007 (for example `B`, `C`, `W`, `Y`, or a
  combined value such as `W/Y` as the rules deliver it); `NG` and `cnl` never appear because such cases are left out.
- **Patient with empty postal code**: The service is asked with an empty postal code; the answer, and thus whether the
  case appears, depends on the service (derived; not exercised).
- **Receiving date earlier than 2019-10-01 with a later sampling date**: Included, because the sampling date decides
  whenever it exists; conversely a sampling date before or on 2019-10-01 excludes the case even when it was received
  later.
- **Several runs per day**: Each run produces the file anew from the current data; two runs on the same day deliver
  files with the same name.
- **Age for a sampling date before the birth date**: Not possible through the forms (spec 003 rejects it); for stored
  data of that kind the age would be negative (derived from the shared age rule; not exercised).
- **Privacy** (Constitution Principle IV): The file contains pseudonymous health data (internal patient number, case
  dates, age, serogroup, typing, and the coordinates of the postal-code place) and is handed to whoever presents the
  token, without sign-in, role, or record of the download; whether the transport is encrypted depends on the hosting,
  not on the application. The patient's postal codes travel unencrypted to the external provider (spec 022, tagged
  there).
- **Architecture** (Constitution "Known Architectural Constraints (Legacy Debt)", Principles II and III): Selection,
  line building, reduction per patient, and file handling all live in the page group of the export; the database
  access and the lookup service are created by the page group itself when no substitutes are supplied, which the
  automated tests use. The tests cover the token check, the line format (literal example line), the exclusion of a
  non-invasive sampling location and of cases without serogroup, and the one-line-per-patient rule with "most data";
  they do not cover the date window, the exclusion of "Blut und Liquor" and the other invasive locations, soft-deleted
  submissions, cases without coordinates, or the failure cases of User Story 6. The test for the invalid token names
  the expected answer; the test for a non-invasive location uses "Ohrabstrich". [POTENTIAL-REFACTORING]

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The export MUST be reachable at its own address with a `token` parameter, without sign-in, and MUST
  deliver the case file when the token equals the value of the `RegistrationToken` setting exactly; it MUST answer
  "not found" for any other or missing token (spec 021, FR-012). The setting's value MUST NOT appear in this
  specification.
- **FR-002**: The export MUST accept the plain retrieval method only, MUST NOT check roles, MUST NOT verify an
  anti-forgery token, and MUST NOT be offered in the navigation (current behavior; spec 011, spec 021).
- **FR-003**: The export MUST consider Meningococci submissions only, MUST exclude soft-deleted submissions, and MUST
  include only submissions whose sampling location is exactly "Blut" or exactly "Liquor".
- **FR-004**: The export MUST include a submission with sampling date only when the sampling date is later than
  2019-10-01, and a submission without sampling date only when the receiving date is 2019-10-01 or later; it MUST
  apply no end date and MUST accept no date range from the caller.
- **FR-005**: The export MUST compute the isolate interpretation with the current rules (spec 007) for each considered
  submission and MUST leave out cases without normalized serogroup or with serogroup `NG` or `cnl`.
- **FR-006**: The export MUST obtain coordinates per case from the external Geonames service with the patient's
  "Postleitzahl" and "Wohnort" under the place-selection rule of spec 022 (FR-005, FR-011) and MUST leave out cases
  without a chosen place, without notice.
- **FR-007**: The export MUST write at most one line per patient number, keeping the candidate line with the most
  characters and, with equal length, the one read first.
- **FR-008**: Each line MUST consist of ten semicolon-separated columns in the order patient number, receiving date,
  sampling date, serogroup, PorA VR1, PorA VR2, FetA VR, age at sampling in years, latitude, longitude, without
  header, quoting, or escaping.
- **FR-009**: Dates MUST be written as `dd.MM.yyyy`; a missing sampling date MUST yield an empty column; the age MUST
  be the full years between birth date and sampling date (or receiving date without sampling date) and MUST be `0`
  without birth date; coordinates MUST use a point as decimal separator.
- **FR-010**: The file MUST be delivered as `text/csv` under the name `EpiscanGis_Export_<yyyyMMdd>.csv` with the
  server's current date, in ascending order of patient number, and MUST be empty when no case qualifies.
- **FR-011**: The export MUST NOT log the request, its outcome, or the number of cases; a "not found" answer of the
  external service MUST be logged by the lookup component and MUST leave out only that case; any other lookup failure
  MUST end the run with the generic server error, logged by the general error logging (current behavior).
- **FR-012**: The export MUST NOT change or mark any patient, submission, or isolate record and MUST NOT keep the file
  on the server after delivery (current behavior).
- **FR-013**: This feature MUST document the current behavior only and MUST NOT introduce new parameters, columns,
  selection rules, or logging.

### Columns and value encodings

| # | Column | Source | Encoding | Example |
| --- | --- | --- | --- | --- |
| 1 | Patient number | Meningococci patient ("Patient" on the submission page) | Integer | `1` |
| 2 | Receiving date | Submission "Eingangsdatum" | `dd.MM.yyyy`; always present | `10.01.2022` |
| 3 | Sampling date | Submission "Entnahmedatum" | `dd.MM.yyyy`; empty when missing | `02.01.2022` |
| 4 | Serogroup | Normalized serogroup of the current interpretation (spec 007) | Group value; never `NG`, `cnl`, or empty | `C` |
| 5 | PorA VR1 | Isolate "PorA VR1" | Text as stored; empty when not recorded | `22` |
| 6 | PorA VR2 | Isolate "PorA VR2" | Text as stored; empty when not recorded | `14` |
| 7 | FetA VR | Isolate "FetA VR" | Text as stored; empty when not recorded | `5-5` |
| 8 | Age at sampling | Patient "Geburtsdatum" against sampling date (or receiving date) | Whole years; `0` when birth date unknown | `10` |
| 9 | Latitude | Chosen place of the Geonames lookup (spec 022) | Decimal with point, service precision | `49.71754` |
| 10 | Longitude | Chosen place of the Geonames lookup (spec 022) | Decimal with point, service precision | `11.05877` |

### Selection summary

| Step | Rule | Case left out when |
| --- | --- | --- |
| Pathogen | Meningococci submissions only | — |
| Deletion | Not soft-deleted (spec 020) | Soft-deleted |
| Sampling location | Exactly "Blut" or "Liquor" | Any other location, including "Blut und Liquor" and the other invasive locations |
| Time window | Sampling date > 2019-10-01, or no sampling date and receiving date ≥ 2019-10-01 | Earlier cases; sampling date exactly 2019-10-01 |
| Serogroup | Normalized serogroup present and not `NG`/`cnl` | No serogroup, `NG`, `cnl` (incl. "no meningococci" conclusions, incomplete typing) |
| Coordinates | Place chosen by the Geonames lookup | No chosen place (unknown postal code, "Wohnort" mismatch, "Unbekannt", "not found") |
| Per patient | Longest line per patient number | Shorter lines of the same patient |

### Roles and Boundaries

| Actor or role | Current observable involvement |
| --- | --- |
| EpiScanGIS import tool (external, automated) | Requests the export address with the token and receives the case file; the only intended caller. |
| Anonymous visitor | Receives the file with the correct token, "not found" otherwise (spec 021). |
| Standard User, Administrator, Public Health, account without role | No menu entry; treated exactly like an anonymous visitor at the export address — the token decides, not the role (spec 021). |
| Geonames service (external) | Supplies the coordinates per case; a "not found" answer leaves the case out, any other failure aborts the run (spec 022). |
| Operator | Maintains the `RegistrationToken` and `GeonamesAccount` settings (spec 018) and the receiving tool's configuration; has no view of runs inside the application. |

### Key Entities *(include if feature involves data)*

- **Meningococci submission (case)**: Supplies receiving date, sampling date, sampling location, and soft-delete
  state; linked to one Meningococci patient and one isolate (spec 003).
- **Meningococci patient**: Supplies the patient number, birth date, "Postleitzahl", and "Wohnort"; the unit of the
  one-line-per-patient rule and of the merge (spec 015).
- **Meningococci isolate and its interpretation**: Supplies PorA VR1, PorA VR2, FetA VR, and — through the current
  interpretation rules — the normalized serogroup (specs 005, 007).
- **Coordinate**: Latitude and longitude of the place chosen for the patient's postal code and place name by the
  Geonames lookup (spec 022); not stored, obtained anew on every run.
- **Export line**: The ten-column text line for one patient; exists only in the delivered file.
- **`RegistrationToken` setting**: The secret that admits the request (spec 018, spec 021); its value is not
  reproduced.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: In 100% of requests, the file is delivered with the exact token value and "not found" is answered for
  any other or missing token, regardless of whether and as whom the requester is signed in.
- **SC-002**: In 100% of runs, every exported patient number belongs to a non-deleted Meningococci submission with
  sampling location "Blut" or "Liquor" inside the time window, with a serogroup other than `NG`/`cnl`, and with a
  chosen place; no other submission contributes a line.
- **SC-003**: In 100% of runs, each patient number occurs at most once, and for a patient with several qualifying
  cases the kept line is the longest candidate.
- **SC-004**: 100% of lines have exactly ten semicolon-separated columns matching the "Columns and value encodings"
  table; the reference case yields `1;10.01.2022;02.01.2022;C;22;14;5-5;10;49.71754;11.05877`.
- **SC-005**: 100% of cases whose patient postal code and "Wohnort" yield no chosen place are absent from the file,
  and the run still completes (spec 022, SC-006).
- **SC-006**: After any number of runs, 0 records in the application are changed, 0 files remain on the server after a
  successful run, and 0 application-log entries describe a successful or rejected request.

## Assumptions

- Current application code, the change log ("Über" page data: "EpiScanGIS Export", bug, GitHub issue 6, 2022-01-30;
  "Automatischer EpiScanGIS Export", feature, 2022-10-16, both Meningococci), version history, configuration structure
  (values not read into this spec), and the automated tests are the source of truth. The external service was not
  called and no running system was exercised.
- The receiving system (EpiScanGIS) and its import format are known only through the file this application produces
  and the version-history note "Adapt export to match import format"; what the receiver does with the file, and its
  schedule, are outside the application and this spec.
- "Not found" for a non-retrieval request method and the exact behavior on a missing `RegistrationToken` setting are
  derived from framework behavior and code (spec 021).
- Ascending patient-number order of the lines is derived from the reading order and the practical behavior of the
  collecting container; it is not a documented guarantee of that container.
- Every selected submission is assumed to own an isolate (spec 003); the consequence of a missing isolate is derived,
  not exercised.
- The behavior for an empty postal code, a foreign postal code, a network failure or unexpected answer of the external
  service (whole-run abort, left-over temporary file), a negative age, and unescaped semicolons in variant values is
  derived from code and the shared helpers; none of it is covered by tests or was exercised.
- Decimal precision of the coordinates is assumed to be whatever the service delivers (the test uses five decimals);
  the application neither rounds nor pads.
- Labels are quoted as shown to users on the Meningococci pages; column names in this spec are descriptive, since the
  file itself carries no header.
