# Verification: Duplicate-patient anonymization in exports

**Spec**: 012 · **Date**: 2026-11-08 · **Build**: `82f4f63` · **Host**: ScreenshotTests
SpecVerificationHost (`http://localhost:9899`, catalog `HaemophilusWeb_Screenshots`) ·
**Accounts used**: `Standard1` · **Scope**: full (unit-test path per `AGENTS.md`, plus file checks of the locally
generated Haemophilus RKI, PubMLST and laboratory exports and the Meningococci RKI, IRIS, LGA and laboratory exports)

## Summary

| Verdict | Count |
| --- | --- |
| Match | 25 |
| Spec deviation | 0 |
| Defect confirmed | 4 |
| New defect | 1 |
| Not verifiable | 0 |

By primary path: Browser 3 · File 17 · DB 1 · Unit test (existing) 2 · Unit test (added) 4 · Code 3 · Not verifiable 0
(adds up to the row count). Also test-asserted: 9 rows with a browser, file or DB verdict. Test run:
`vstest.console.exe … /TestCaseFilter:` `DuplicatePatientResolverTests`, `HaemophilusSendingRkiExportTests`,
`HaemophilusPubMlstExportTests`, `CreateExcelFileTests` → 124 passed, 0 failed (42 of them added); full suite → 1094
passed, 1 failed on the first run (flaky existing test, see Backlog updates), 1095 passed, 0 failed on the rerun.

Edge cases are numbered in the order of the spec's Edge Cases list before this verification: EC1 exact text matching,
EC2 distinct persons without birth date and postal code, EC3 key without separator, EC4 six-month rule, EC5 stem order
versus receiving order, EC6 sampling versus receiving date in the comment, EC7 dropped submission does not reset the
reference, EC8 PubMLST stem numbers as text, EC9 strain texts differ per export, EC10 empty-strain step, EC11
positional group labels, EC12 indirect identifiers remain, EC13 PubMLST non-invasive removal, EC14 Meningococci RKI
ignores the option, EC15 controller wiring, EC16 test coverage, EC17 error inside duplicate handling, EC18 empty
selection.

Own records created through the UI (seeded sender 1, all sampled and received in 2025, "Blut", isolate defaults
"NTHi" and beta-lactamase "n.d.", no E-test; stem numbers entered in the isolate form):

| Record | Stem number | Received | Patient (record) |
| --- | --- | --- | --- |
| `SV-012-A` | H9998 | 10.02.2025 | "X.Y.", 02.03.2004, 97000, weiblich (5) |
| `SV-012-B` | H9999 | 20.02.2025 | same values (7, created with 03.03.2004, birth date edited afterwards) |
| `SV-012-C` | H10000 | 15.05.2025 | same values (6, "Neuen Patienten anlegen") |
| `SV-012-D` | H10001 | 01.04.2025 | same values (8, created with 04.03.2004, birth date edited afterwards) |
| `SV-012-E` | H10002 | 03.03.2025 | "Q.Q.", weiblich, no birth date, no postal code (9) |
| `SV-012-F` | H10003 | 10.03.2025 | same values (10, "Neuen Patienten anlegen") |
| `SV-012-G` | none | 05.06.2025 | "N.I.", 05.05.2005, 97001, männlich (11), "Anderer nicht invasiv" |

B and D could not be attached to an "X.Y." patient through intake once two such records existed (third-patient block,
002 FR-011), so they were created with other birth dates and edited; that path is the new defect DATA-040. No seeded
record was changed. Exports for 2025 contain only these records; the Meningococci exports for 2020 contain the seeded
MZ004/20.

## Results

| ID | Spec says | App/test shows | Verdict | Path | Evidence |
| --- | --- | --- | --- | --- | --- |
| FR-001 · US1.1 | Haemophilus RKI and PubMLST offer "Unbereinigte Daten" "Nein"/"Ja", preselected "Nein"; a request without the value counts as "Nein". | Both pages show the label "Unbereinigte Daten" with "Nein" checked and "Ja". A direct request without `Unadjusted` returned the same adjusted files as the "Nein" form runs (RKI 3 rows with `duplicate_group`, PubMLST 2 rows). | Match | Browser | `verification/012-PatientSending-RkiExport.png`, `012-PatientSending-PubMlstExport.png`, `012-rki-default.xlsx`, `012-pubmlst-default.xlsx` |
| FR-002 · US1.1 · US2.1 · US2.2 · US2.3 | "Nein": cleaning, marking, identifier removal; "Ja": identifier removal only. | RKI "Nein": 3 of 6 rows, 24 columns (23 + `duplicate_group`); RKI "Ja": all 6 rows, 23 columns, no group column. PubMLST "Nein": 2 rows; "Ja": all 6 rows; 42 columns in both, no group column. No file has an identifier column. | Match | File | `012-rki-nein.xlsx`, `012-rki-ja.xlsx`, `012-pubmlst-nein.xlsx`, `012-pubmlst-ja.xlsx`; also test-asserted: `Tools/DuplicatePatientResolverTests.WhenCleanupFinished_PatientColumnsAreRemoved`, `RemovePatientData_KeepsAllRowsAndAddsNoGroupColumn` (added) |
| FR-003 · US1.2 · EC1 | Same patient exactly when initials, birth date, postal code and gender match as text, regardless of patient records. | A (patient record 5) and C (record 6) share "Group 1" in the adjusted RKI file. Initials differing only in case form two patients. | Match | File | `012-rki-nein.xlsx`; also test-asserted: `Tools/DuplicatePatientResolverTests.WhenInitialsDifferOnlyInCase_ThenEntriesBelongToDifferentPatients` (added) |
| EC2 · DATA-002 | Distinct persons with same initials and gender and neither birth date nor postal code are one patient; one identical strain within the window is dropped. `[SUSPECTED-DEFECT]` | E and F belong to two separate "Q.Q." patient records; F (received 7 days after E) is missing from the adjusted RKI and PubMLST files and present in both unadjusted files. | Defect confirmed | File | `012-rki-nein.xlsx`, `012-pubmlst-nein.xlsx`, `012-rki-ja.xlsx` |
| EC3 | The four values are joined without separator. `[POTENTIAL-REFACTORING]` | `Tools/DuplicatePatientResolver.GroupDuplicatePatients` groups by `$"{Initials}{DateOfBirth}{PostalCode}{Sex}"`. | Match | Code | `HaemophilusWeb/Tools/DuplicatePatientResolver.cs` (part of DATA-002) |
| FR-004 · US1.7 | Matching is repeated before each step; no group column when no patient keeps two rows. | Adjusted PubMLST file: after dropping, "X.Y." keeps only H10000 and "Q.Q." only H10002, and the file has no `duplicate_group` column although both patients had several rows before. | Match | File | `012-pubmlst-nein.xlsx`; also test-asserted: `Tools/DuplicatePatientResolverTests.WhenIdenticalStrainReceivedTenDaysLater_ThenLaterEntryIsDroppedAndNoGroupIsMarked` (added) |
| FR-005 | A submission with empty beta-lactamase, ampicillin evaluation and serotype is dropped, also for single-submission patients. | The existing test removes two empty-strain rows of three different patients and keeps the third row. | Match | Unit test (existing) | `Tools/DuplicatePatientResolverTests.WhenEmptyRecords_ThenEntriesAreRemoved` |
| EC10 | The empty-strain step never takes effect because the serotype/evaluation column is always filled. `[POTENTIAL-REFACTORING]` | For each of the 14 evaluations the RKI `styp` and the PubMLST `serotype` value is non-empty. | Match | Unit test (added) | `Tools/HaemophilusSendingRkiExportTests.DataTable_SerotypeIsNeverEmpty`, `Tools/HaemophilusPubMlstExportTests.DataTable_SerotypeIsNeverEmpty` |
| FR-006 · US1.3 | An identical strain received the same day or less than one month after the earlier one is dropped, the earlier kept. | B (10 days after A) is missing from the adjusted RKI file, A is kept. | Match | File | `012-rki-nein.xlsx`; also test-asserted: `Tools/DuplicatePatientResolverTests.WhenDuplicatePatients_ThenIdenticalOlderEntriesStrainsAreRemoved`, `WhenIdenticalStrainReceivedTenDaysLater_…` (added) |
| FR-006 · US1.4 · EC4 · DEC-009 · DATA-001 | Only the month component counts: identical strains received 1 to 11 months later are kept, although the documented intent is "less than six months". `[SUSPECTED-DEFECT]` | C (3 months and 5 days after the kept A, identical strain) is kept in the adjusted RKI file. The whole-year case was not exercised. DEC-009 observed only; no test written for the disputed distances. | Defect confirmed | File | `012-rki-nein.xlsx` |
| FR-006 · US1.5 | A difference in one strain characteristic keeps both submissions. | Differences in beta-lactamase, ampicillin evaluation, serotype or sampling location each keep both rows, which form "Group 1". | Match | Unit test (added) | `Tools/DuplicatePatientResolverTests.WhenOneStrainCharacteristicDiffers_ThenBothEntriesAreKeptAndGrouped` (4 cases); serotype also in the existing `WhenDuplicatePatients_ThenIdenticalOlderEntriesStrainsAreRemoved` |
| EC5 | A higher stem number received earlier is dropped when the month component is 0 to 5. | RKI: D (H10001, received 1 month and 14 days before the reference C) is dropped. PubMLST: A and B (received about three months before H10000, which is processed first there) are dropped. | Match | File | `012-rki-nein.xlsx`, `012-pubmlst-nein.xlsx` |
| EC6 | The comment speaks of the sampling date, the comparison uses the receiving date. `[POTENTIAL-REFACTORING]` | Summary of `CleanDuplicateIdenticalStrainsNotTooFarApart` says "date sampled"; the code reads `ReceivedDate`. The test data had equal sampling-to-receiving offsets, so the files do not distinguish the two. | Match | Code | `HaemophilusWeb/Tools/DuplicatePatientResolver.cs` |
| FR-007 · EC7 | A kept identical strain becomes the new reference; a dropped one does not. | RKI: D is compared with C (kept, the later reference) and dropped; compared with A it would have been kept (1 month and 22 days later). The second half (a dropped submission is not a reference) cannot be separated from the disputed window and stays code-derived. | Match | File | `012-rki-nein.xlsx`; also test-asserted: `Tools/DuplicatePatientResolverTests.WhenIdenticalStrainIsKept_ThenItBecomesTheReferenceForLaterEntries` (added) |
| EC8 · DATA-003 | PubMLST compares stem numbers as text, RKI as numbers. `[SUSPECTED-DEFECT]` | Same four "X.Y." submissions: adjusted RKI keeps 9998 and 10000, adjusted PubMLST keeps only H10000 (taken as the earliest by text order). | Defect confirmed | File | `012-rki-nein.xlsx`, `012-pubmlst-nein.xlsx`; RKI numeric order also test-asserted: `Tools/HaemophilusSendingRkiExportTests.CleanOrMarkDuplicates_WithRkiLayout_KeepsLowerNumericStemNumberAndMarksGroup` (added) |
| EC9 | The compared strain texts differ per export (German labels in RKI, codes in PubMLST). | RKI rows: `mat` "Blut", `styp` "NTHi", `b_lac` "n.d."; PubMLST rows: `source` "blood", `serotype` "NT", `beta_lactamase` empty. | Match | File | `012-rki-ja.xlsx`, `012-pubmlst-ja.xlsx` |
| FR-008 · US1.6 · EC11 | `duplicate_group` appended last, `Group <n>` from 1 per patient in order of the first row, empty for single patients; labels restart per run. | Adjusted RKI: last column `duplicate_group`, A and C "Group 1", E empty; a second run gives the same labels. | Match | File | `012-rki-nein.xlsx`, `012-rki-nein-rerun.xlsx`; also test-asserted: `Tools/DuplicatePatientResolverTests.WhenTwoPatientsEachHavingTwoEntries_ThenDuplicateGroupsAreMarked`, `WhenOnlyOnePatientHasSeveralEntries_ThenOtherRowsHaveAnEmptyGroupCell` (added) |
| FR-009 · US1.8 · SC-001 | `initials`, `date_of_birth`, `postal_code` removed in both modes. | None of the eleven Haemophilus RKI and PubMLST files contains these columns. | Match | File | all `012-rki-*.xlsx` and `012-pubmlst-*.xlsx` of Haemophilus; also test-asserted: `Tools/HaemophilusSendingRkiExportTests.DataTable_ContainsOnlyPayloadColumnsAfterCleanup` |
| FR-010 · EC12 | All other columns stay, including the listed indirect attributes. | RKI keeps birth month and year, gender, county name and number, state name and number, sender, DEMIS ID, sampling and receiving dates; PubMLST keeps 42 columns including age in years and months, sex, region, year, both dates. | Match | Unit test (added) | `Tools/HaemophilusSendingRkiExportTests.DataTable_KeepsIndirectPatientColumnsAfterCleanup`, `Tools/HaemophilusPubMlstExportTests.DataTable_RemovePatientData_KeepsIndirectPatientColumns`; file headers agree |
| US2.4 · US2.5 | Hints "Unbereinigt: Inklusive doppelter Einsendungen." (RKI) and "Unbereinigt: Auch Daten aus dem Ausland, alle Entnahmeorte, doppelte Einsendungen enthalten." (PubMLST). | Both texts shown verbatim under the option. | Match | Browser | `012-PatientSending-RkiExport.png`, `012-PatientSending-PubMlstExport.png` |
| EC13 · DATA-004 | The adjusted-mode removal of "other non-invasive" submissions has no effect. `[SUSPECTED-DEFECT]` | G ("Anderer nicht invasiv") is in the laboratory export and missing from the PubMLST file in both modes. | Defect confirmed | File | `012-lab-hi-noninvasive.xlsx`, `012-pubmlst-ja-noninvasive.xlsx`, `012-pubmlst-nein-noninvasive.xlsx` |
| FR-011 · US3.1 | Laboratory exports keep all submissions and the identifiers. | Haemophilus: all six duplicates (and G) with "Initialen", "Geburtsdatum", "Postleitzahl". Meningococci: the three active 2020 submissions with the same columns. | Match | File | `012-lab-hi.xlsx`, `012-lab-hi-noninvasive.xlsx`, `012-lab-mz.xlsx` |
| FR-011 · US3.2 · EC14 · DEC-010 | Meningococci RKI: no option on the page, a sent value has no effect, no cleaning or removal. | Page has no "Unbereinigte Daten" field; the 2020 file with and without `Unadjusted=Yes` has the same row and 36 columns, no group column, no initials/birth date/postal code ("PatNr NRZM", "Geburtsmonat", "Geburtsjahr", "Geschlecht" present). DEC-010 observed only. | Match | File | `012-MeningoPatientSending-RkiExport.png`, `012-rki-mz-default.xlsx`, `012-rki-mz-ja.xlsx` |
| FR-011 · US3.3 | Meningococci IRIS and LGA apply none of the steps. | Neither file has a group column or identifier columns. Only one Meningococci submission matches their selection, so the absence of dropping is shown by the unchanged column sets, not by duplicate rows. | Match | File | `012-iris-mz.xlsx`, `012-lga-mz.xlsx` |
| EC15 · REF-003 | The resolver is created and the mode chosen inside both export actions. `[POTENTIAL-REFACTORING]` | `PatientSendingController.PubMlstExport` and `RkiExportWithPotentialAdjustments` each create `new DuplicatePatientResolver(...)` and switch between `RemovePatientData` and `CleanOrMarkDuplicates`. | Match | Code | `HaemophilusWeb/Controllers/PatientSendingController.cs` |
| EC16 · TEST-002 | One test uses the wall clock; the window tests pass under both readings; no test for the RKI layout or the export actions. `[POTENTIAL-REFACTORING]` | `CreateRecord` uses `DateTime.Now`; the window tests use 0 days and 6 months. The verification added a test with the RKI layout; the export actions stay untested (spec text corrected). | Match | Unit test (existing) | `Tools/DuplicatePatientResolverTests.CreateRecord`, `WhenDuplicatePatients_ThenIdenticalEntriesMoreThenSixMonthApartStrainsAreKept` |
| EC17 | An error inside duplicate handling surfaces as an error and no file is downloaded. | An exception thrown by the post-processing step propagates out of `CreateExcelDocument` and no file is written. | Match | Unit test (added) | `Tools/CreateExcelFileTests.CreateExcelDocument_WhenPostProcessingThrows_ExceptionPropagatesAndNoFileIsWritten` |
| EC18 | Empty selection: header only, identifier columns removed, no group column. | 2019 ("Nein"): RKI header with 23 columns, PubMLST header with 42 columns, no rows, no group column. | Match | File | `012-rki-empty-2019.xlsx`, `012-pubmlst-empty-2019.xlsx`; also test-asserted: `Tools/DuplicatePatientResolverTests.WhenSelectionIsEmpty_ThenOnlyTheReducedHeaderRemains` (added) |
| FR-012 · SC-005 | Exports change no record and store no record of the run. | Row counts of all 20 tables and checksums of `Sendings`, `Patients`, `Isolates` were identical before and after three export runs. | Match | DB | read-only `sqlcmd` snapshots (scratchpad) |
| New EC "Several patient records with the same matching values" · 002 FR-015 · DATA-040 | Not in the spec before (observed while preparing the data). | Editing the birth date of submissions B and D stored two more "X.Y." patients with the values of the two existing ones; no duplicate prompt or error. | New defect | Browser | DB rows of patients 5–8 (read-only query) |

## Unit tests added

42 test cases in 13 methods.

- `HaemophilusWebTests/Tools/DuplicatePatientResolverTests.cs` (10 cases, 7 methods, fixed dates):
  `WhenIdenticalStrainReceivedTenDaysLater_ThenLaterEntryIsDroppedAndNoGroupIsMarked` (FR-004, FR-006, US1.3, US1.7),
  `WhenOneStrainCharacteristicDiffers_ThenBothEntriesAreKeptAndGrouped` (4 cases; FR-006, US1.5, US1.6),
  `WhenIdenticalStrainIsKept_ThenItBecomesTheReferenceForLaterEntries` (FR-007),
  `WhenInitialsDifferOnlyInCase_ThenEntriesBelongToDifferentPatients` (FR-003, EC1),
  `WhenOnlyOnePatientHasSeveralEntries_ThenOtherRowsHaveAnEmptyGroupCell` (FR-008, US1.6),
  `RemovePatientData_KeepsAllRowsAndAddsNoGroupColumn` (FR-002, FR-009, US2.1–US2.3),
  `WhenSelectionIsEmpty_ThenOnlyTheReducedHeaderRemains` (EC18)
- `HaemophilusWebTests/Tools/HaemophilusSendingRkiExportTests.cs` (16 cases, 3 methods):
  `DataTable_KeepsIndirectPatientColumnsAfterCleanup` (FR-010), `DataTable_SerotypeIsNeverEmpty` (14 cases; EC10),
  `CleanOrMarkDuplicates_WithRkiLayout_KeepsLowerNumericStemNumberAndMarksGroup` (FR-006, EC8 numeric part, US1.6)
- `HaemophilusWebTests/Tools/HaemophilusPubMlstExportTests.cs` (15 cases, 2 methods):
  `DataTable_RemovePatientData_KeepsIndirectPatientColumns` (FR-009, FR-010), `DataTable_SerotypeIsNeverEmpty`
  (14 cases; EC10)
- `HaemophilusWebTests/Tools/CreateExcelFileTests.cs` (1 case; the file existed empty and was not compiled, now added
  to `HaemophilusWebTests.csproj`): `CreateExcelDocument_WhenPostProcessingThrows_ExceptionPropagatesAndNoFileIsWritten`
  (EC17)

No test was written for DATA-001 (disputed window, DEC-009), DATA-002, DATA-003 (text order) or DATA-004.

## Spec corrections applied

- EC4: coverage note (was: tests only cover 0 days and exactly six months; now: tests cover only distances both
  readings treat alike, 0 and 10 days, 6 and 7 months; the disputed distances are untested).
- EC16: coverage note (was: no test exercises duplicate cleaning with the RKI layout; now: one test does, the export
  actions' mode choice stays untested).
- New edge case "Several patient records with the same matching values" with `[SUSPECTED-DEFECT]` and cross-reference
  to 002 FR-015 / DATA-040.

## Backlog updates

- DATA-001: confirmed on system 2026-11-08 (3 months and 5 days kept; whole-year case not exercised).
- DATA-002: confirmed on system 2026-11-08 (two "Q.Q." records without birth date and postal code merged).
- DATA-003: confirmed on system 2026-11-08 (PubMLST keeps H10000 where RKI keeps 9998 and 10000).
- DATA-004: confirmed on system 2026-11-08 for the no-op non-invasive removal.
- DEC-009, DEC-010: observations recorded, no decision taken.
- TEST-002: tests added for the undisputed parts; flaky existing test
  `Tools/HaemophilusPubMlstExportTests.DataTable_ContainsAntibioticMeasurements("CTX", Cefotaxime, Susceptible, …)`
  recorded (random E-test from `Setup` with the same antibiotic is found first).
- NEW DATA-040: editing a submission stores duplicate patient identities without a check (source 002 FR-015, 012).

## Not verifiable here

- None. The whole-year distance of FR-006 (dropped again at 12 months plus less than one month) was not exercised; it
  belongs to the disputed window (DEC-009) and needs no further evidence for DATA-001.

## Open questions for the owner

- DATA-040 belongs to spec 002, which this verification did not edit (one spec per run); add the observation to 002
  FR-015 when 002 is next touched?
- Seed extension for later specs: two invasive Meningococci submissions with a positive finding for one patient would
  let the Meningococci RKI/IRIS/LGA checks show the absence of duplicate handling by rows, not only by columns.
