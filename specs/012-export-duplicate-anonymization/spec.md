# Feature Specification: Duplicate-patient anonymization in exports

**Feature Branch**: `speckit-init`

**Created**: 2026-10-22

**Status**: Draft (status-quo inventory)

**Input**: User description: "Feature title: \"Duplicate-patient anonymization in exports\" (backlog ref B2, Group B — Data exports & epidemiology). Document the current, as-implemented behavior of duplicate-patient handling and data anonymization applied during exports, as it exists today. Read HaemophilusWeb/Tools/DuplicatePatientResolver.cs and its use in the RKI/PubMLST/IRIS export paths (RemovePatientData vs CleanOrMarkDuplicates, driven by the \"Unadjusted\" option). Capture when patient data is removed vs cleaned/marked, and the privacy intent. Document observable behavior only — do not propose changes."

> **Scope note**: This is an *as-is* specification (backlog ref B2). It records the current, observable rules by which
> the Haemophilus RKI export and the Haemophilus PubMLST export detect submissions of the same patient, drop
> repeated submissions, mark the remaining ones as duplicate groups, and remove direct patient identifiers before the
> spreadsheet is written, depending on the "Unbereinigte Daten" (unadjusted data) option. It does **not** propose
> changes. The generic export mechanism (date range, selection filters, column layouts, file names, roles) is
> specified in spec 011 (B1) and is referenced, not repeated, here. The Haemophilus PubMLST export uses the selection
> that the code names "IRIS"; there is no Haemophilus IRIS export, and the Meningococci IRIS, LGA, RKI, and both
> Laboratory exports apply none of the rules described here; they are in scope only to record that absence. Out of
> scope: patient merging in the database (a data-maintenance function, not an export rule), sender exports (B3), the
> token-protected GIS export, and the selection filters themselves (B1).

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Export adjusted surveillance data without direct patient identifiers (Priority: P1)

A Standard User or Public Health (`RKI`) user runs the Haemophilus RKI export, or a Standard User runs the Haemophilus PubMLST export, leaving "Unbereinigte Daten" at its default "Nein". Before the file is written, repeated submissions of the same patient with the same strain characteristics received shortly after one another are dropped, the remaining submissions of the same patient are labelled with a common duplicate-group label, and the patient's initials, date of birth, and postal code are removed from the file.

**Why this priority**: This is the default path for the two exports that leave the institution for national surveillance and the international typing database; it carries the privacy intent (direct identifiers never leave the system in these files) and the epidemiological intent (one case is not counted several times).

**Independent Test**: For one patient (same initials, date of birth, postal code, and gender) create several invasive submissions in the export range: two with identical beta-lactamase result, ampicillin evaluation, serotype/evaluation, and sampling location received a few days apart, one with a different serotype, and one identical one received several months later; add a second, unrelated patient with one submission. Run both exports with "Nein" and inspect rows and columns.

**Acceptance Scenarios**:

1. **Given** "Unbereinigte Daten" is "Nein" (the preselected value, also used when the option is not supplied at all), **When** the Haemophilus RKI or PubMLST export runs, **Then** duplicate cleaning and marking is applied and afterwards the initials, date-of-birth, and postal-code columns are removed.
2. **Given** several submissions in the export share the same patient initials, date of birth, postal code, and gender (exact text match, see FR-003), **When** cleaning runs, **Then** these submissions are treated as belonging to one patient, regardless of whether they are stored under one or several patient records.
3. **Given** two submissions of one patient have the same strain characteristics (beta-lactamase result, ampicillin evaluation, serotype/evaluation, and sampling location as exported), **And** the later one by stem number was received on the same day or less than one calendar month after the earlier one, **When** cleaning runs, **Then** the later submission is dropped from the file and the earlier one is kept.
4. **Given** two submissions of one patient have the same strain characteristics, **And** the later one was received one or more (but fewer than twelve) months after the earlier kept one, **When** cleaning runs, **Then** both are kept (see the tagged observation on the six-month rule under Edge Cases).
5. **Given** two submissions of one patient differ in at least one strain characteristic, **When** cleaning runs, **Then** both are kept.
6. **Given** after dropping, two or more submissions of the same patient remain, **When** marking runs, **Then** a final column `duplicate_group` is added and every submission of that patient carries the same label `Group <n>`, with `n` counted from 1 in order of the patient's first submission by stem number; submissions of patients with a single remaining submission have an empty cell in that column.
7. **Given** after dropping, no patient has more than one remaining submission, **When** the file is produced, **Then** no `duplicate_group` column is present.
8. **Given** cleaning and marking are finished, **When** the file is written, **Then** it contains no initials, date-of-birth, or postal-code column; all other columns of the export (spec 011) remain, including the indirect patient attributes listed in FR-010.

---

### User Story 2 - Export unadjusted data, still without direct patient identifiers (Priority: P2)

The same users choose "Unbereinigte Daten" = "Ja" to obtain all selected submissions, including repeated submissions of the same patient, while the direct patient identifiers are still removed.

**Why this priority**: It is the deliberate opt-out from duplicate cleaning; it shows that identifier removal does not depend on the option.

**Independent Test**: Run the same data set as in User Story 1 with "Ja" and compare row count and columns with the "Nein" run.

**Acceptance Scenarios**:

1. **Given** "Unbereinigte Daten" is "Ja", **When** the Haemophilus RKI or PubMLST export runs, **Then** every submission selected by the export's filters (spec 011) appears as one row; no submission is dropped as a duplicate.
2. **Given** "Ja", **When** the file is produced, **Then** no `duplicate_group` column is added, so the file itself does not indicate which rows belong to the same patient.
3. **Given** "Ja", **When** the file is produced, **Then** the initials, date-of-birth, and postal-code columns are removed exactly as with "Nein".
4. **Given** the Haemophilus RKI export page, **When** it is shown, **Then** the hint reads "Unbereinigt: Inklusive doppelter Einsendungen.", which matches the observed difference between the two modes.
5. **Given** the Haemophilus PubMLST export page, **When** it is shown, **Then** the hint reads "Unbereinigt: Auch Daten aus dem Ausland, alle Entnahmeorte, doppelte Einsendungen enthalten."; of these three promises only the inclusion of duplicate submissions is observable (consistent with spec 011).

---

### User Story 3 - Know which exports are not anonymized or de-duplicated (Priority: P3)

A reviewer (data-protection officer, maintainer, or recipient liaison) needs to know which exports apply neither duplicate handling nor identifier removal, so that file handling outside the system can be judged correctly.

**Why this priority**: The rules of this feature are limited to two exports; the boundary is privacy-relevant (Constitution Principle IV) but does not change any file content.

**Independent Test**: Run every export of spec 011 for the data set of User Story 1 and check for duplicate rows, a `duplicate_group` column, and identifier columns.

**Acceptance Scenarios**:

1. **Given** the Haemophilus or Meningococci Laboratory export runs, **When** the file is produced, **Then** all submissions are present and initials, birth date, and postal code are included (internal laboratory extract).
2. **Given** the Meningococci RKI export runs, **When** the file is produced, **Then** no duplicate cleaning, marking, or identifier-column removal is applied; the page shows no adjustment option and an "unadjusted" value sent with the request has no effect.
3. **Given** the Meningococci IRIS or LGA export runs, **When** the file is produced, **Then** no duplicate cleaning, marking, or identifier-column removal is applied; their reduced column sets are defined in spec 011.

### Edge Cases

- Patient matching is an exact, case- and whitespace-sensitive text comparison of initials, date of birth, postal code, and the gender value as exported. The same person recorded as "e.m." and "E.M.", or with a differently formatted postal code, is not recognised as a duplicate; the stored patient record identity is not used.
- Distinct persons with the same initials and gender and with neither date of birth nor postal code recorded are treated as one patient; if their submissions have identical strain characteristics within the dropping window, one of them is dropped from the adjusted export. [SUSPECTED-DEFECT]
- The four matching values are joined into one key without a separator, so in rare combinations two different value sets could form the same key (for example when the date of birth is empty). [POTENTIAL-REFACTORING]
- The intended rule, as described next to the code, is that identical strains of a patient received less than six months apart are reduced to one. The actual comparison uses only the month component of the calendar difference, measured from the later to the earlier submission: identical strains received on the same day or within less than one month are dropped; identical strains received one to eleven months later are kept; identical strains received 12, 24, ... months plus less than one month later are dropped again. The automated tests only cover a zero-day and an exactly six-month difference, which both pass under either reading. [SUSPECTED-DEFECT]
- If a submission with a higher stem number was received *earlier* than the matching submission with a lower stem number, the difference becomes positive and the higher-stem-number submission is dropped when the month component is between 0 and 5 (that is, up to almost six months, and again at whole-year distances). Which submission survives therefore depends on stem-number order, not on receiving order.
- The description next to the code speaks of the *sampling* date, but the comparison uses the *receiving* date. [POTENTIAL-REFACTORING]
- A dropped submission does not reset the comparison date: each further identical-strain submission is compared with the last *kept* submission of that strain for the patient.
- In the PubMLST export the stem number is compared as text with the prefix "H" (for example "H1000" sorts before "H999"), so with stem numbers of different length the "earlier" submission is chosen by text order; in the RKI export it is compared as a number. [SUSPECTED-DEFECT]
- The strain characteristics compared are the exported texts, which differ between the two exports (German labels such as "negativ"/"Hib"/sampling-location label in RKI; "negative"/"b"/"blood" or the free-text other location in PubMLST). For "other invasive" submissions both exports use the free-text other sampling location literally, so differently spelled free texts count as different strains.
- A first cleaning step is meant to drop a submission whose strain characteristics are all empty (the "no growth, please resubmit" case). It applies to every patient, including patients with a single submission (the automated test confirms this for three different patients). In both real exports it never takes effect, because the serotype/evaluation column is always filled (RKI: evaluation label such as "kein Wachstum" or "NTHi"; PubMLST: mapped code), so a no-growth submission at an invasive location is kept in the RKI export and becomes part of the patient's duplicate group. [POTENTIAL-REFACTORING]
- Duplicate-group labels are positional and restart at "Group 1" in every export run; the same patient can carry different labels in different files, and rows of one group are not moved next to each other.
- Removing the three identifier columns leaves indirect identifiers in the file (see FR-010), including sampling and receiving dates and, for RKI, the sender and the DEMIS ID; the export is therefore pseudonymized by removal of direct identifiers, not anonymized in a strict sense.
- In the PubMLST adjusted mode an additional removal of "other non-invasive" submissions takes place before the file is built; it has no effect because the selection already excludes them, so in practice duplicate handling is the only difference between the PubMLST modes (recorded in spec 011 together with the misleading PubMLST hint). [SUSPECTED-DEFECT]
- The Meningococci RKI export receives the "unadjusted" setting but ignores it; no Meningococci export applies duplicate handling or identifier-column removal, so the parallel Meningococci domain (Constitution, Known Architectural Constraints) has no counterpart to this feature. [POTENTIAL-REFACTORING] (recorded in spec 011)
- The duplicate handling is created and selected inside the export controller actions rather than injected, and the choice between the two behaviors is duplicated in both Haemophilus export actions (Constitution, Known Architectural Constraints: god-controllers with weak dependency injection). [POTENTIAL-REFACTORING]
- One automated test derives its dates from the current wall-clock time and another test file for the RKI layout verifies only that identifier removal leaves 23 of 26 columns; no test exercises duplicate cleaning with the RKI column layout or the export actions end to end (Constitution Principle II). [POTENTIAL-REFACTORING]
- Duplicate handling runs on the assembled table before the spreadsheet is written; an error inside it surfaces as an error page and no file is downloaded (spec 011 describes the separate behavior when writing the file fails).
- An empty selection produces a file with the header row only; the identifier columns are removed from the header as well and no `duplicate_group` column is added.

## Requirements *(mandatory)*

### Functional Requirements

#### Mode selection

- **FR-001**: The Haemophilus RKI and Haemophilus PubMLST exports MUST offer the "Unbereinigte Daten" choice ("Nein"/"Ja"), preselected with "Nein"; a request without this value MUST be treated as "Nein".
- **FR-002**: With "Nein" the system MUST apply, in this order: (a) empty-strain removal (FR-005), (b) repeated-submission dropping (FR-006), (c) duplicate-group marking (FR-008), (d) identifier removal (FR-009). With "Ja" the system MUST apply only identifier removal (FR-009). All steps MUST run on the already selected and ordered export rows (spec 011) before the file is written.

#### Patient matching

- **FR-003**: Submissions MUST be treated as belonging to the same patient exactly when their exported initials, date of birth, postal code, and gender value are textually identical (case-sensitive; an empty value matches only an empty value).
- **FR-004**: Patient matching MUST be repeated from the remaining rows before each of the steps FR-005, FR-006, and FR-008.

#### Cleaning

- **FR-005**: Within each patient (including patients with one submission), a submission whose beta-lactamase, ampicillin-evaluation, and serotype/evaluation values are all empty MUST be dropped. (In both current exports the serotype/evaluation value is never empty; see Edge Cases.)
- **FR-006**: Within each patient, submissions MUST be processed in ascending stem-number order; a submission MUST be dropped when an earlier kept submission of the same patient has identical beta-lactamase, ampicillin-evaluation, serotype/evaluation, and sampling-location values and the calendar difference from the later to the earlier receiving date has a month component from 0 to 5 (counting the remaining months after whole years, with a negative value meaning the earlier submission was received first).
- **FR-007**: When a submission with identical strain characteristics is kept, it MUST replace the earlier one as the reference for later comparisons of that strain for the patient; a dropped submission MUST NOT become the reference.

#### Marking

- **FR-008**: After dropping, for every patient with two or more remaining submissions the system MUST write the label `Group <n>` into a column `duplicate_group` appended after all other columns, with `n` starting at 1 and increasing per patient in order of the patient's first remaining row; the column MUST be added only if at least one such patient exists, and rows of other patients MUST stay empty in it.

#### Identifier removal (privacy)

- **FR-009**: In both modes the system MUST remove the initials (`initials`), date-of-birth (`date_of_birth`), and postal-code (`postal_code`) columns from the Haemophilus RKI and PubMLST files; these values MUST be used only for patient matching and MUST NOT appear in the downloaded file.
- **FR-010**: Identifier removal MUST leave all other export columns unchanged, in particular: for RKI the birth month and year, gender, county name and number, state name and number, sender, DEMIS ID, and sampling and receiving dates; for PubMLST age in years and (under 12 months) months, sex, region, sampling year, and sampling and receiving dates.

#### Boundaries

- **FR-011**: The Haemophilus and Meningococci Laboratory exports and the Meningococci RKI, IRIS, and LGA exports MUST NOT apply any of the steps FR-005 to FR-009.
- **FR-012**: Dropping and marking MUST affect only the produced file; no submission, patient, or isolate record MUST be changed, merged, or deleted, and no record of the export run MUST be stored.
- **FR-013**: This feature MUST document current behavior only and MUST NOT introduce new matching, cleaning, or anonymization rules.

### Mode Overview

| Export | Option offered | "Nein" (adjusted, default) | "Ja" (unadjusted) | Initials, birth date, postal code in file |
| --- | --- | --- | --- | --- |
| Haemophilus RKI | Yes | Empty-strain removal, repeated-submission dropping, `duplicate_group` marking, identifier removal | Identifier removal only | Removed in both modes |
| Haemophilus PubMLST (internally "IRIS" selection) | Yes | Same as RKI (plus a no-effect removal of non-invasive submissions) | Identifier removal only | Removed in both modes |
| Haemophilus Laboratory | No | None | None | Included |
| Meningococci RKI | Not shown; value ignored | None | None | Not in the column set (NRZM patient number, birth month/year, gender initial are; spec 011) |
| Meningococci IRIS / LGA | No | None | None | Not in the column sets (spec 011) |
| Meningococci Laboratory | No | None | None | Included |

### Key Entities *(include if feature involves data)*

- **Export option "Unbereinigte Daten"**: Yes/No choice of the Haemophilus RKI and PubMLST exports; "Nein" (default) means adjusted, "Ja" means unadjusted.
- **Patient match key**: The combination of exported initials, date of birth, postal code, and gender used to decide that rows belong to the same patient; exists only during file production.
- **Strain characteristics**: The exported beta-lactamase result, ampicillin evaluation, serotype/evaluation, and (for dropping) sampling location of a row.
- **Duplicate group**: A set of remaining rows of one patient, labelled `Group <n>` in the `duplicate_group` column; local to one file.
- **Direct identifiers**: Initials, date of birth, and postal code; removed from both files.
- **Indirect patient attributes**: The remaining patient-related columns (FR-010) that stay in the files.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: In 100% of Haemophilus RKI and PubMLST files, in both modes, no initials, date-of-birth, or postal-code column is present.
- **SC-002**: For a test data set with known duplicates, the adjusted file contains exactly the rows predicted by FR-005 to FR-007, and the unadjusted file contains exactly the rows of the selection (spec 011), with 0 deviations.
- **SC-003**: In 100% of adjusted files, every patient with two or more remaining rows carries one common `Group <n>` label, labels are numbered consecutively from 1, and the column is absent when no such patient exists.
- **SC-004**: In 100% of runs of the five exports listed in FR-011, row count and identifier columns are identical to a run without any duplicate handling.
- **SC-005**: After any export run, the number and content of stored submissions, patients, and isolates is unchanged.

## Assumptions

- Current application code, views, and automated tests are the source of truth; no production data, recipient agreements, or data-protection documentation were inspected. The privacy intent (direct identifiers never leave the system in RKI/PubMLST files) is inferred from identifier removal being unconditional and from the identifiers being used only for matching.
- The description of the intended six-month rule is taken from the explanatory comments next to the code; the actual effect described in FR-006 follows from the date-difference calculation of the date library in use (month component of a years-months-days difference). It has not been executed against real data.
- Stem numbers are assumed to be assigned roughly in receiving order, so that "earlier by stem number" usually means "received earlier"; the out-of-order case is described separately under Edge Cases.
- Initials and gender are mandatory for Haemophilus patients (patient validation), so every match key contains at least these two values; date of birth and postal code are optional.
- Because direct identifiers are removed in the unadjusted mode without adding any group label, it is assumed that recipients of unadjusted files are not expected to recognise same-patient rows themselves.
- The selection filters, column sets, file names, and access rules of all exports are as specified in spec 011.
