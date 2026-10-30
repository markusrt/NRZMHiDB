# Feature Specification: Data exports (generic)

**Feature Branch**: `speckit-init`

**Created**: 2026-10-22

**Status**: Draft (status-quo inventory)

**Input**: User description: "Feature title: \"Data exports (generic)\" (backlog ref B1, Group B — Data exports & epidemiology). Document the current, as-implemented behavior of the sending/isolate data export features as they exist today, covering the Haemophilus exports (RKI, Laboratory, PubMLST) and the Meningococci exports (Laboratory, IRIS, LGA/State Authority, RKI). The separate Einsender (sender) export is out of scope here — it is covered by B3. Read HaemophilusWeb/Controllers/PatientSendingController.cs and MeningoPatientSendingController.cs (export actions, SendingsMatchingExportQuery, ExportToExcel, export definitions), and the export definition classes. Capture what each export includes, its date-range/adjustment options, sampling-location filters, and which roles can run it. Document observable behavior only — do not propose changes."

> **Scope note**: This is an *as-is* specification (backlog ref B1, decision D3: one generic spec). It records the
> current, observable behavior of the seven submission/isolate spreadsheet exports offered from the Export menu:
> Haemophilus RKI, Laboratory, and PubMLST; Meningococci Laboratory, IRIS, LGA (state health authority), and RKI.
> Because all seven are implemented on one shared mechanism (date-range query -> selection filter -> column
> definition -> spreadsheet download), the mechanism is documented once and the concrete targets are enumerated.
> It does **not** propose changes. Out of scope: the sender ("Einsender") exports for both pathogens (B3); the
> detailed duplicate-submission cleaning and patient-data removal rules behind the "Unbereinigte Daten" option (B2;
> only the option's existence and high-level effect are recorded here); the token-protected GIS export, which is
> not offered in the Export menu; and the clinical interpretation rules whose results some exports consume
> (specs 006/007). Submission and isolate capture are covered by specs 002/003 and 004/005.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Export laboratory data for a date range (Priority: P1)

A Standard User opens the Haemophilus or Meningococci "Labor Export", accepts or changes the pre-filled date range, and downloads a spreadsheet with one row per non-deleted submission in that range, covering submission, patient, and isolate laboratory data for every sampling location.

**Why this priority**: The laboratory exports are the broadest internal data extracts and are the reference for what the other, narrower exports select from.

**Independent Test**: With submissions of every sampling location (invasive and non-invasive), some with and some without a sampling date, and some soft-deleted, run each laboratory export for a range and compare the rows and columns with the stored data.

**Acceptance Scenarios**:

1. **Given** a Standard User selects "Labor Export" under Haemophilus or Meningokokken, **When** the page opens without a date range, **Then** a form titled "Entnahmedatum" shows start and end date fields pre-filled with 1 January and 31 December of the previous calendar year.
2. **Given** a start and end date are submitted, **When** the export runs, **Then** a spreadsheet named `Labor-Export_<start yyyyMMdd>-<end yyyyMMdd>.xlsx` is downloaded.
3. **Given** a submission has a sampling date, **When** selection runs, **Then** it is included exactly when its sampling date lies within the range (both ends inclusive); **Given** it has no sampling date, **Then** its receiving date is used instead.
4. **Given** submissions of all sampling locations exist in the range, **When** a laboratory export runs, **Then** all of them are included regardless of sampling location or interpretation result; soft-deleted submissions are never included.
5. **Given** the Haemophilus laboratory export runs, **When** the spreadsheet is produced, **Then** each row contains patient number, laboratory and stem numbers, sender, dates, sender laboratory number, DEMIS ID, sampling location, material, invasiveness, sender's serotype and species, the patient's initials, birth date, age at sampling, gender, postal code, city, county, state, clinical information, Hib vaccination and its date, both remarks, the isolate's growth, biochemical, agglutination, PCR, sequencing, identification, and MLST results, MIC values and evaluations for Ampicillin, Amoxicillin/Clavulanate, Meropenem, Cefotaxime, Imipenem, and Ciprofloxacin, the evaluation, report date, and the RKI match reference, file number, and status.
6. **Given** the Meningococci laboratory export runs, **When** the spreadsheet is produced, **Then** each row contains the corresponding submission and patient data plus risk factors, the interpretation rule, derived serogroup, a "kein Nachweis" marker when no meningococci were found, the isolate's growth, biochemical, capsule, agglutination, PCR, PorA/FetA, identification, and remark data, MIC values and evaluations for Benzylpenicillin, Ciprofloxacin, Rifampicin, Cefotaxime, and Azithromycin, and the matched PubMLST typing values.

---

### User Story 2 - Export surveillance data for the RKI (Priority: P1)

A Standard User or a Public Health (`RKI`) user opens the Haemophilus or Meningococci "RKI Export", chooses a date range, and downloads a reduced data set of invasive cases with county and state numbers. For Haemophilus, the user can additionally choose between adjusted and unadjusted data.

**Why this priority**: The RKI exports are the only exports open to the Public Health role and are the feed for national surveillance.

**Independent Test**: As each role, open both RKI exports, run them for a range containing invasive and non-invasive submissions, duplicate patients, positive and negative Meningococci interpretations, and patients with known and unknown counties, and inspect the rows and columns.

**Acceptance Scenarios**:

1. **Given** the Haemophilus RKI export page opens, **When** it is shown, **Then** it offers the previous-year date range and a "Unbereinigte Daten" Yes/No choice defaulting to "Nein", with the hint "Unbereinigt: Inklusive doppelter Einsendungen."
2. **Given** the Haemophilus RKI export runs, **When** submissions are selected, **Then** only submissions with sampling location blood, liquor, or other-invasive are included; non-invasive submissions are excluded regardless of the adjustment choice.
3. **Given** "Unbereinigte Daten" is "Nein", **When** the Haemophilus RKI spreadsheet is produced, **Then** duplicate submissions of the same patient are cleaned or marked (a `duplicate_group` column appears when duplicate groups remain) and the patient initials, date of birth, and postal code columns are removed; **Given** "Ja", **Then** no duplicate cleaning or marking happens but the same three patient columns are still removed (detailed rules: B2).
4. **Given** the Haemophilus RKI spreadsheet is produced, **Then** it contains the RKI column names `klhi_nr`, `eing`, `ent`, `mat`, `geb_monat`, `geb_jahr`, `geschlecht`, `hib_impf`, `styp`, `b_lac`, `kreis_nr`, `bundesland`, `einsender`, `landkreis`, `bundeslandName`, `ampicillinMHK`, `ampicillinBewertung`, `amoxicillinClavulansaeureMHK`, `bewertungAmoxicillinClavulansaeure`, `demis_id`, followed by the real-time PCR result, device, and result columns.
5. **Given** the Meningococci RKI export page opens, **When** it is shown, **Then** it offers only the date range; no adjustment choice is shown and no duplicate cleaning or patient-column removal is applied.
6. **Given** the Meningococci RKI export runs, **When** submissions are selected, **Then** only submissions at an invasive sampling location whose isolate interpretation does not conclude "no meningococci" are included.
7. **Given** the Meningococci RKI spreadsheet is produced, **Then** each row contains the NRZM patient number, receiving and sampling dates, capsule null locus, agglutination, serogroup and capsule PCR results, real-time PCR data, PorA VR1/VR2 and FetA VR (only when the respective PCR is positive, "negativ" when negative), MIC values for Penicillin, Ciprofloxacin, Cefotaxime, and Rifampicin, birth month and year, gender initial, county and state numbers, PubMLST typing values, and the DEMIS ID.
8. **Given** either RKI export is downloaded, **Then** its file name is `RKI-Export_<start yyyyMMdd>-<end yyyyMMdd>.xlsx`.

---

### User Story 3 - Export Haemophilus data for PubMLST (Priority: P2)

A Standard User opens "PubMLST Export" under Haemophilus, chooses a date range and adjustment option, and downloads a spreadsheet laid out in the PubMLST submission column format with English values.

**Why this priority**: It feeds the international typing database and applies the strictest Haemophilus selection.

**Independent Test**: Run the export for a range containing invasive and non-invasive submissions, patients whose county is "Ausland", and isolates with every evaluation; inspect the selection and the value encodings.

**Acceptance Scenarios**:

1. **Given** the PubMLST export page opens, **When** it is shown, **Then** it offers the previous-year date range and the "Unbereinigte Daten" choice defaulting to "Nein", with the hint "Unbereinigt: Auch Daten aus dem Ausland, alle Entnahmeorte, doppelte Einsendungen enthalten."
2. **Given** the export runs with either adjustment choice, **When** submissions are selected, **Then** only blood, liquor, and other-invasive submissions are included, patients whose county text equals "Ausland" are excluded, and isolates evaluated as H. parainfluenzae, no growth, no Haemophilus species, Haemophilus sp. but not H. influenzae, or not H. influenzae are excluded.
3. **Given** "Nein" is chosen, **When** the spreadsheet is produced, **Then** duplicate submissions are cleaned or marked and the patient initials, date of birth, and postal code columns are removed; **Given** "Ja", **Then** only the three patient columns are removed (detailed rules: B2).
4. **Given** the spreadsheet is produced, **Then** columns follow the PubMLST layout (`isolate`, `aliases`, `references`, `country`, `region`, `year`, `date_received`, `date_sampled`, `non_culture`, `serotype`, `serotype_by_serology`, `serotype_by_PCR`, `genotype`, `biotype`, `ribotype`, `age_yr`, `age_mth`, `sex`, `source`, `disease`, `epidemiology`, `MLEE_lineage`, `MLEE_ET`, `beta_lactamase`, `AMX_MIC`, `AMX_SIR`, `AMC_MIC`, `AMC_SIR`, `CTX_MIC`, `CTX_SIR`, `CRO_MIC`, `CRO_SIR`, `ENA_accession`, `comments`, the seven MLST locus columns, `ftsI`), with placeholder columns left empty.
5. **Given** values are exported, **Then** country is always "Germany", region is the English state name, dates are `yyyy-MM-dd` (missing: "n.a."), year is the sampling year (empty without sampling date), serotypes use `a`-`f`, `NT`, or `ND`, sex is `f`/`m`/empty, source is `blood`, `CSF`, or the free-text other location, beta-lactamase is `positive`/`negative`/empty, SIR values are the first letter of the evaluation, and `age_mth` is filled only for patients younger than 12 months.
6. **Given** the export is downloaded, **Then** its file name is `PubMLST-Export_<start yyyyMMdd>-<end yyyyMMdd>.xlsx`.

---

### User Story 4 - Export Meningococci data for IRIS and the state health authority (Priority: P3)

A Standard User opens "IRIS Export" or "LGA Export" under Meningokokken, chooses a date range, and downloads the respective reduced spreadsheet of invasive cases with meningococci detected.

**Why this priority**: These are narrower recipient-specific extracts built on the same selection as the Meningococci RKI export.

**Independent Test**: Run both exports for a range containing invasive and non-invasive submissions and positive/negative interpretations; verify the selection and the columns.

**Acceptance Scenarios**:

1. **Given** either page opens, **When** it is shown, **Then** it offers only the previous-year date range; no adjustment choice is shown.
2. **Given** either export runs, **When** submissions are selected, **Then** the same selection as the Meningococci RKI export applies (invasive sampling location and meningococci not ruled out); no overseas exclusion is applied.
3. **Given** the IRIS export runs, **Then** each row contains laboratory and stem numbers, state, receiving and sampling dates, material, invasiveness, age at sampling in years and (under 12 months only) in months, gender, clinical information, sampling location, agglutination, MIC values for Benzylpenicillin, Cefotaxime, Rifampicin, and Ciprofloxacin, PorA/FetA variable regions, serogroup PCR, real-time PCR and device, and capsule PCR/allele results; the file is named `Iris-Export_<start>-<end>.xlsx`.
4. **Given** the LGA export runs, **Then** each row contains the NRZM patient number, gender initial, birth month as `MM / yyyy`, receiving and sampling dates, derived serogroup, PorA VR1/VR2 and FetA VR, species (first non-empty of MALDI-TOF Vitek best match, 16S rRNA best match, or the real-time PCR result when that PCR is positive), and county and state numbers; the file is named `LGA-Export_<start>-<end>.xlsx`.

---

### User Story 5 - Reach exports according to role (Priority: P4)

Users see and can run only the exports their role permits; the navigation offers a role-specific subset.

**Why this priority**: Access control determines who can extract identifiable patient data.

**Independent Test**: Log in with a Standard User, a Public Health, and an Administrator-only account; compare offered menu entries with the outcome of calling each export address directly.

**Acceptance Scenarios**:

1. **Given** a Standard User, **When** the navigation is shown, **Then** the Export menu lists, under Haemophilus, RKI, Labor, PubMLST, and Einsender exports and, under Meningokokken, Labor, IRIS, LGA, RKI, and Einsender exports, and all seven exports in scope can be run.
2. **Given** a Public Health user, **When** the navigation is shown, **Then** only "RKI Export (Haemophilus)" and "RKI Export (Meningokokken)" are offered as top-level entries, and both can be run.
3. **Given** a Public Health user addresses the LGA export directly, **When** it runs, **Then** it is permitted, although it is not offered in that user's navigation.
4. **Given** a Public Health user addresses the Laboratory, PubMLST, or IRIS export directly, **Then** access is refused.
5. **Given** an account holding only the Administrator role, **When** the navigation is shown, **Then** the Export menu is offered, but running any of the seven exports is refused because none admits the Administrator role.

### Edge Cases

- A request without a start date always shows the form pre-filled with the previous calendar year, even if an end date was supplied; an unparseable start date behaves the same way and shows no error message.
- A start date later than the end date, or a missing end date, is not rejected; the download contains only the header row.
- The end date is compared as a calendar date at the start of the day; the range includes the end day for date-only values.
- Rows are ordered by stem number ascending.
- Unknown birth date: RKI exports show birth month and year as `0`; age-based columns (Laboratory, IRIS, PubMLST) show age `0` years and, where present, `0` months. A missing sampling date is shown as "keine Angabe" in day-month-year columns. Haemophilus RKI shows stem number `-1` when no stem number exists.
- County and state numbers are found by comparing the patient's county text with the stored county list, ignoring case, text after a comma, and the fill words "Städteregion", "Kreisfreie Stadt", "Region", "Landkreis", "Kreis". Without a match both numbers are empty; the Meningococci exports also log a warning, Haemophilus does not. The county list and the exact normalization rules are specified in spec 025.
- When several stored county versions match, the one with the earliest validity date is used; the sampling or receiving date of the submission is not considered. [POTENTIAL-REFACTORING]
- Enumerated values are exported as their German display labels (for example "n.d." for not determined); only a zero value without any display label becomes an empty cell. Gender initials are derived from the German labels (`m`, `w`, `d`, `k`), with `?` for an unknown gender in the RKI exports and an empty cell in the LGA export.
- The Haemophilus RKI export applies no evaluation filter, so non-H. influenzae evaluations at invasive locations are included; the PubMLST export excludes them.
- The PubMLST hint promises that unadjusted data includes overseas data and all sampling locations, but overseas-county patients and non-invasive submissions are excluded in both modes. In adjusted mode an additional removal of "other non-invasive" submissions has no effect because those are already excluded. Only duplicate handling differs between the two modes. [SUSPECTED-DEFECT]
- The overseas exclusion in the PubMLST export compares the patient's free-text county with "Ausland", not the patient's state; a patient whose state is "Ausland" but whose county text differs is still exported. [SUSPECTED-DEFECT]
- In the PubMLST export a patient whose state is "Ausland" gets the region value `blood`. [SUSPECTED-DEFECT]
- The PubMLST evaluation exclusion does not list H. haemolyticus, so such isolates are exported although the export's own value mapping marks this evaluation as "actually not to be exported". [SUSPECTED-DEFECT]
- PubMLST `AMX_MIC` is always empty while `AMX_SIR` carries the Ampicillin evaluation; this is confirmed by an automated test and therefore appears intentional.
- Unknown birth date yields `age_yr` 0 and `age_mth` 0 in the PubMLST and IRIS exports, which cannot be told apart from infants younger than one month. [SUSPECTED-DEFECT]
- The PubMLST export reuses the selection that is internally named for IRIS; the Haemophilus side has no IRIS export, and the source carries a note to clarify and merge the two. [POTENTIAL-REFACTORING]
- The Meningococci RKI page receives the "unadjusted" setting as part of its input model but neither shows nor uses it. [POTENTIAL-REFACTORING]
- The LGA export is authorized for the Public Health role but not offered in its navigation. [SUSPECTED-DEFECT]
- The Export menu is shown to Administrator-only accounts, but every export refuses that role (accounts are registered with exactly one role). [SUSPECTED-DEFECT]
- The export forms embed an anti-forgery token, but the exports accept any request method and do not verify the token. [POTENTIAL-REFACTORING]
- If writing the spreadsheet fails, the failure is only traced and the user still receives the (empty or partial) temporary file as a normal download, with no error shown. Failures while assembling rows (before writing) surface as an error page. [SUSPECTED-DEFECT]
- Each export writes a temporary file on the server that is not deleted afterwards, so exported patient data remains in the server's temporary directory (relevant to Constitution Principle IV). [POTENTIAL-REFACTORING]
- The single worksheet is not given an explicit name. [POTENTIAL-REFACTORING]
- The automated test intended to cover the PubMLST empty-query behavior actually calls the Laboratory export, so PubMLST's form defaults are not covered by tests. [POTENTIAL-REFACTORING]

## Requirements *(mandatory)*

### Functional Requirements

#### Generic export mechanism

- **FR-001**: Each export MUST be reachable at its own address and MUST, when no start date is supplied, show a form under the heading "Entnahmedatum" with start date ("Start-Datum") and end date ("Ende-Datum") fields in `DD.MM.YYYY` format, pre-filled with 1 January and 31 December of the previous calendar year, and an "Exportieren nach Excel" button.
- **FR-002**: When a start date is supplied, the system MUST select all non-deleted submissions of the export's pathogen whose sampling date lies within start and end date inclusive or, if no sampling date is recorded, whose receiving date lies within that range.
- **FR-003**: The system MUST then apply the export target's selection filter (see Export Catalogue), order the remaining submissions by stem number ascending, and produce one spreadsheet row per submission.
- **FR-004**: Columns MUST be produced from the export target's fixed column definition in a fixed order; header names are either explicit names or the field's display name; enumerated values MUST be written as their display labels (a zero value that has no display label is written as an empty cell), missing values as empty cells, and numeric columns as numbers.
- **FR-005**: For exports with an adjustment option, the system MUST apply the post-processing chosen by "Unbereinigte Daten" to the assembled table before writing (see FR-012).
- **FR-006**: The system MUST return a single-worksheet spreadsheet (`.xlsx`) with a header row, named `<Prefix>-Export_<start yyyyMMdd>-<end yyyyMMdd>.xlsx`, where the prefix is `RKI`, `Labor`, `PubMLST`, `Iris`, or `LGA`.
- **FR-007**: Exports MUST NOT validate that the start date precedes the end date and MUST NOT persist any record of an export run.

#### Targets

- **FR-008**: Haemophilus and Meningococci Laboratory exports MUST include all sampling locations without any interpretation or geography filter and MUST include identifying patient data (initials, birth date, postal code, city, county).
- **FR-009**: The Haemophilus RKI export MUST include only blood, liquor, and other-invasive submissions.
- **FR-010**: The Haemophilus PubMLST export MUST include only blood, liquor, and other-invasive submissions, MUST exclude patients whose county text equals "Ausland", and MUST exclude the evaluations H. parainfluenzae, no growth, no Haemophilus species, Haemophilus sp. but not H. influenzae, and not H. influenzae.
- **FR-011**: The Meningococci RKI, IRIS, and LGA exports MUST include only submissions at a sampling location classified as invasive (blood, blood and liquor, joint aspirate, liquor, petechiae, pleural aspirate, serum, other-invasive) whose isolate interpretation does not conclude that no meningococci were found.
- **FR-012**: The Haemophilus RKI and PubMLST exports MUST offer "Unbereinigte Daten" (Yes/No, default No). With No, duplicate submissions MUST be cleaned or marked and the initials, date-of-birth, and postal-code columns removed; with Yes, only those three columns MUST be removed. Detailed rules are specified in B2.
- **FR-013**: Meningococci exports and the Laboratory exports MUST NOT offer an adjustment option and MUST NOT apply duplicate cleaning or patient-column removal.
- **FR-014**: Exports carrying county and state numbers (Haemophilus RKI, Meningococci RKI, LGA) MUST derive the county number from the stored county list matched on the patient's county text, and the state number from the first two digits of that county number; both MUST be empty when no county matches.
- **FR-015**: Each export MUST contain the column content listed in its user story and in the Export Catalogue.

#### Access

- **FR-016**: The Haemophilus RKI, Meningococci RKI, and LGA exports MUST be permitted for the Standard User and Public Health roles; the Haemophilus Laboratory, Haemophilus PubMLST, Meningococci Laboratory, and IRIS exports MUST be permitted for the Standard User role only. No export MUST admit the Administrator role on its own.
- **FR-017**: The navigation MUST offer the full Export menu to Standard User and Administrator roles and only the two RKI exports to the Public Health role.
- **FR-018**: This feature MUST document current behavior only and MUST NOT introduce new selection, anonymization, or access rules.

### Export Catalogue

| Export (menu label) | Pathogen | File prefix | Roles permitted | Adjustment option | Selection beyond date range | Patient identifiers in output |
| --- | --- | --- | --- | --- | --- | --- |
| RKI Export | Haemophilus | `RKI` | Standard User, Public Health | Yes ("Unbereinigte Daten") | Blood, liquor, other-invasive | None (initials, birth date, postal code removed); birth month/year, sex, county kept |
| Labor Export | Haemophilus | `Labor` | Standard User | No | None (all sampling locations) | Patient number, initials, birth date, postal code, city, county, state |
| PubMLST Export | Haemophilus | `PubMLST` | Standard User | Yes ("Unbereinigte Daten") | Blood, liquor, other-invasive; county not "Ausland"; non-H. influenzae evaluations excluded (except H. haemolyticus) | None (initials, birth date, postal code removed); age, sex, region kept |
| Labor Export | Meningococci | `Labor` | Standard User | No | None (all sampling locations) | Patient number, initials, birth date, postal code, city, county, state |
| IRIS Export | Meningococci | `Iris` | Standard User | No | Invasive location and meningococci not ruled out | State, age, gender |
| LGA Export | Meningococci | `LGA` | Standard User, Public Health (not in its menu) | No | Invasive location and meningococci not ruled out | Patient number, gender initial, birth month/year, county/state numbers |
| RKI Export | Meningococci | `RKI` | Standard User, Public Health | No (not shown, ignored) | Invasive location and meningococci not ruled out | Patient number, birth month/year, gender initial, county/state numbers |

### Roles and Boundaries

| Actor or role | Current observable involvement |
| --- | --- |
| Standard User (`Standardbenutzer`) | Sees the full Export menu and can run all seven exports. |
| Public Health (`RKI`) | Sees only the two RKI exports; can also run the LGA export by direct address; is refused the other four. |
| Administrator | Sees the Export menu but is refused every export unless the account also holds the Standard User role. |
| Unauthenticated visitor | Is required to log in; no export is publicly reachable. |
| External recipients (RKI, PubMLST, IRIS, state health authority) | Receive the downloaded files outside the system; no transmission happens within the system. |

### Key Entities *(include if feature involves data)*

- **Export query**: Start date and end date, plus for the Haemophilus RKI and PubMLST exports the "Unbereinigte Daten" Yes/No choice.
- **Submission (sending)**: The row subject. Supplies sampling and receiving dates, sampling location, material, invasiveness, sender, sender laboratory number, DEMIS ID, and soft-delete state; linked to one patient and one isolate.
- **Patient**: Supplies initials, birth date, gender, postal code, city, free-text county, state, clinical information, risk factors (Meningococci), and Hib vaccination (Haemophilus).
- **Isolate**: Supplies stem and laboratory numbers, laboratory results, susceptibility tests, evaluation (Haemophilus) or interpretation results (Meningococci), report date, remark, and for Meningococci the matched PubMLST typing record.
- **County reference list**: Stored county names, county numbers, and validity dates used to derive county and state numbers.
- **Export definition**: The fixed, ordered column set and value encoding of one export target.
- **Export file**: The downloaded single-sheet spreadsheet; not retained or logged as a business record by the system.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: For each of the seven exports, 100% of non-deleted submissions in a test range that meet the documented selection rules appear exactly once, and 0 submissions outside them appear.
- **SC-002**: In 100% of test runs, a submission with a sampling date is selected by that date and a submission without one by its receiving date, with both range ends inclusive.
- **SC-003**: Every downloaded file name matches the documented `<Prefix>-Export_<start>-<end>.xlsx` pattern for its export.
- **SC-004**: In 100% of Haemophilus RKI and PubMLST files, initials, date of birth, and postal code columns are absent in both adjustment modes; in adjusted mode, duplicate handling is applied as specified in B2.
- **SC-005**: For each role, the observed menu entries and allowed/refused outcomes match the Roles and Boundaries table in 100% of the 7 x 3 role/export combinations.
- **SC-006**: Opening any export without a date range pre-fills the full previous calendar year in 100% of cases.

## Assumptions

- Current application code, views, and automated tests are the source of truth for this inventory; no production data or deployment configuration was inspected.
- Sampling and receiving dates are stored as date-only values, so the end date of the range is effectively inclusive. If time-of-day were stored, submissions later than midnight on the end date would be excluded.
- A refused export request for an authenticated user is assumed to lead back to the login page (standard behavior of the authentication setup); the exact page shown was not exercised.
- Whether an Administrator account can also hold the Standard User role was not established beyond the observation that registration assigns exactly one role; the Administrator refusal therefore applies to single-role accounts.
- The overseas-county comparison is assumed to follow the database's default case-insensitive text comparison, and patients without a county are assumed to remain included.
- Meningococci invasiveness and "meningococci not ruled out" follow the interpretation rules documented in spec 007; Haemophilus evaluations are those documented in specs 004/006.
- How an empty worksheet name is handled by spreadsheet applications was not verified; existing automated tests open the generated files successfully.
- The detailed duplicate-detection, cleaning, and marking rules are deliberately left to B2; the sender exports are left to B3.
