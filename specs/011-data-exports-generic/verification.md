# Verification: Data exports (generic)

**Spec**: 011 · **Date**: 2026-11-08 · **Build**: `c180142` · **Host**: ScreenshotTests
SpecVerificationHost (`http://localhost:9899`, catalog `HaemophilusWeb_Screenshots`) ·
**Accounts used**: `Standard1`, `Rki1`, `Admin1`, no account · **Scope**: full (column definitions on the unit-test
path per `AGENTS.md`; forms, date ranges, file names, row selection and roles on the browser/file path)

## Summary

| Verdict | Count |
| --- | --- |
| Match | 17 |
| Spec deviation | 6 |
| Defect confirmed | 13 |
| New defect | 0 |
| Not verifiable | 1 |

By primary path: Browser 7 · File 24 · DB 0 · Unit test (existing) 1 · Unit test (added) 1 · Code 3 · Not verifiable 1
(adds up to the row count). Also test-asserted: 7 rows with a browser or file verdict. Test run:
`vstest.console.exe … /TestCaseFilter:` the seven export test fixtures in `Tools/`, `ExportDefinitionTests` and
`Controllers/PatientSendingControllerTests` → 220 passed, 0 failed (17 of them added); full suite → 1139 passed,
0 failed.

Edge cases are numbered in the order of the spec's Edge Cases list before this verification: EC1 missing or unreadable
start date, EC2 start after end / missing end, EC3 end date as calendar date, EC4 row order, EC5 unknown birth date and
placeholders, EC6 county matching and warning, EC7 several county versions, EC8 enumerated values and gender initials,
EC9 Haemophilus RKI without evaluation filter, EC10 PubMLST hint, EC11 PubMLST overseas by county text, EC12 region
`blood`, EC13 H. haemolyticus exported, EC14 `AMX_MIC` empty, EC15 age 0 for unknown birth date, EC16 PubMLST reuses
the IRIS query, EC17 Meningococci RKI ignores "unadjusted", EC18 LGA for Public Health, EC19 Export menu for
Administrator, EC20 anti-forgery token, EC21 write failure, EC22 temp files, EC23 worksheet name, EC24 PubMLST
empty-query test.

Own records created through the UI (seeded sender 1; laboratory numbers of 2026 because they are assigned in the year
of saving; "Bemerkung" and "Labnr. Einsender" `SV-011-<key>`):

| Key | Database | Sampled / received | Location | Patient, other details | Stem number |
| --- | --- | --- | --- | --- | --- |
| HA | Haemophilus | 01.01.2024 / 03.01.2024 | Blut | "A.A.", 15.06.2023, weiblich, county "Landkreis SV-011-Ort", Bayern; "Hib", ß-Laktamase positiv, agglutination b | H20001 |
| HB | Haemophilus | none / 31.12.2024 | Liquor | "B.B.", county "Ausland", state Bayern | H20002 |
| HC | Haemophilus | 01.06.2024 / 03.06.2024 | Anderer nicht invasiv "SV-011-Rachen" | "C.C." | H20003 |
| HJ | Haemophilus | 31.12.2023 / 02.01.2024 | Blut | "J.J." | H20008 |
| HE | Haemophilus | 05.05.2024 / 07.05.2024 | Blut | "E.E."; "H. parainfluenzae" | H20004 |
| HF | Haemophilus | 06.06.2024 / 08.06.2024 | Anderer invasiv "SV-011-Gelenk" | "F.F.", divers, state "Ausland", county "Paris" | H20005 |
| HG | Haemophilus | 07.07.2024 / 09.07.2024 | Blut | "G.G."; "H. haemolyticus" | H19999 |
| HH | Haemophilus | 08.08.2024 / 10.08.2024 | Blut | "H.H.", no birth date, state "n.n." | none |
| HI | Haemophilus | 09.09.2024 / 11.09.2024 | Blut | "I.I."; soft-deleted through "Eintrag löschen" | H20006 |
| HK | Haemophilus | 31.12.2024 / 02.01.2025 | Blut | "K.K.", keine Angabe; ß-Laktamase negativ | H20007 |
| MA | Meningococci | 01.01.2024 / 03.01.2024 | Blut | "M.A.", 10.10.2023, weiblich, county "Kreis SV-011-Ort"; inputs of rule 08 (as MZ004/20) | DE30002 |
| MB | Meningococci | 15.03.2024 / 17.03.2024 | Rachenabstrich | "M.B." | DE30004 |
| MC | Meningococci | none / 31.12.2024 | Liquor | "M.C.", no birth date, keine Angabe, county "SV-011-Unbekannt"; as MA but FetA PCR negativ (no rule) | DE30001 |
| MD | Meningococci | 31.12.2023 / 02.01.2024 | Blut | "M.D." | none |
| ME | Meningococci | 01.07.2024 / 03.07.2024 | Serum | "M.E.", divers; growth and oxidase negativ only (no rule) | DE30003 |
| MF | Meningococci | 02.02.2024 / 04.02.2024 | Blut | "M.F.", state "Ausland", country FRA, county "Ausland"; as ME | DE30005 |

Counties "SV-011-Ort" with "09111" (valid since 01.01.2000) and "08222" (valid since 01.01.2015) were created on
`/County/Create` for the run and deleted afterwards. The 2020 range holds the seeded MZ001/20–MZ004/20 (MZ003/20
soft-deleted) and KL001/20–KL003/20 (KL003/20 soft-deleted); the November 2026 range holds the records of the 007
verification. No seeded record was changed. Haemophilus intake requires a gender, so a Haemophilus patient without
gender cannot be created through the UI; the seeded MZ004/20 patient has none.

## Results

| ID | Spec says | App/test shows | Verdict | Path | Evidence |
| --- | --- | --- | --- | --- | --- |
| FR-001 · US1.1 · US2.1 · US2.5 · US3.1 · US4.1 · SC-006 | Each export has its own address; without start date a form "Entnahmedatum" with "Start-Datum"/"Ende-Datum" pre-filled with the previous year and "Exportieren nach Excel"; Haemophilus RKI and PubMLST add "Unbereinigte Daten" ("Nein" preselected) with their hints; the Meningococci pages show only the dates. | All seven pages (HTTP 200) show "Entnahmedatum", "Start-Datum" 01.01.2025, "Ende-Datum" 31.12.2025 and "Exportieren nach Excel"; RKI and PubMLST (Haemophilus) show "Unbereinigte Daten" "Nein" (checked) / "Ja" with "Unbereinigt: Inklusive doppelter Einsendungen." and "Unbereinigt: Auch Daten aus dem Ausland, alle Entnahmeorte, doppelte Einsendungen enthalten."; the other five pages have no option. Page headings: "RKI Export (Haemophilus)", "Export der Labordaten (Haemophilus)", "Export der PubMLST Daten (Haemophilus)", "Export der Labordaten (Meningokokken)", "IRIS Export", "Export LGA (Meningokokken)", "RKI Export (Meningokokken)". | Match | Browser | `verification/011-PatientSending-RkiExport.png` and the six other `011-<Controller>-<Action>.png`; also test-asserted: `Controllers/PatientSendingControllerTests.LaboratoryExport_EmptyQuery_ForwardToViewWithQueryForLastYear`, `RkiExport_EmptyQuery_ForwardToViewWithQueryForLastYear` |
| EC1 | Without start date the form is shown pre-filled even with an end date; an unparseable start date behaves the same way and shows no error message. | `?To=2024-12-31`: pre-filled form, no message. `?From=abc&To=2024-12-31` and "abc" typed into the IRIS form: pre-filled form (end date replaced by 31.12.2025) with "Start-Datum ist kein gültiger Wert für abc."; an emptied start field submitted from the form: "Eingabe erforderlich". No download in either case. | Spec deviation | Browser | `verification/011-EC1-unparseable-start.png`, `011-EC1-ui-unparseable-start.png` |
| FR-002 · US1.3 · SC-002 · EC3 | Non-deleted submissions whose sampling date lies in the range (both ends inclusive), without sampling date by receiving date. | 2024 files: HA (sampled on the start day) and HK (sampled on the end day, received 2025) included; HJ (sampled 31.12.2023, received 02.01.2024) and MD excluded; HB and MC (no sampling date, received 31.12.2024) included; soft-deleted HI and KL003/20 excluded. | Match | File | `verification/011-h-labor-2024.xlsx`, `011-h-rki-2024-nein.xlsx`, `011-m-labor-2024b.xlsx`, `011-h-labor-2020.xlsx` |
| FR-003 · EC4 | Rows ordered by stem number ascending. | Ascending (H19999, H20001 … H20007; DE30001 … DE30005), but submissions without stem number come first: HH (`-1` in the RKI file, "H - " in the laboratory file); in the November 2026 Meningococci laboratory file nine "DE -" rows precede DE5001 in an order that is neither laboratory number nor patient number. | Spec deviation | File | `verification/011-h-rki-2024-nein.xlsx`, `011-m-labor-202611.xlsx` |
| FR-004 · EC8 | Fixed columns; enumerations as display labels, a zero value without label empty, missing values empty, numbers as numbers; gender initials `m`/`w`/`d`/`k`, `?` without gender in the RKI exports, empty in LGA. | Labels "n.d.", "keine Angabe", "NSH Real-Time-PCR (BD-MAX)" (zero value with label); "Real-Time-PCR Auswertung (RIDOM)" empty (zero without label); `klhi_nr`, `geb_monat`, `geb_jahr`, `einsender`, "Patienten-Nr." written as numeric cells; `geschlecht` m/w/d/k; seeded MZ004/20 without gender: "?" (Meningococci RKI), empty (IRIS, LGA). | Match | File | `verification/011-h-rki-2024-nein.xlsx`, `011-m-rki-2020.xlsx`, `011-m-lga-2020.xlsx`; also test-asserted: `Tools/ExportDefinitionTests.ToDataTable_EnumsAreConvertedToStrings`, `Tools/HaemophilusSendingRkiExportTests.DataTable_ContainsNullForUndefinedEnumValues`, `DataTable_ExportsGenderInitial` (added), `Tools/MeningoSendingRkiExportTests.DataTable_WithoutBirthDateAndGender_ExportsZeroAndQuestionMark` (added) |
| FR-005 · FR-012 · US2.3 · US3.3 · SC-004 | "Unbereinigte Daten": "Nein" cleans or marks duplicates and removes initials, date of birth, postal code; "Ja" removes only the three columns. | Both modes of both exports: no identifier column (RKI 23, PubMLST 42 columns); no duplicates in the 2024 data, so equal rows and no `duplicate_group`. The duplicate handling itself was confirmed in the 012 verification (`duplicate_group` in the adjusted RKI file). | Match | File | `verification/011-h-rki-2024-nein.xlsx`, `011-h-rki-2024-ja.xlsx`, `011-h-pubmlst-2024-nein.xlsx`, `011-h-pubmlst-2024-ja.xlsx`; 012 `verification.md` FR-002; also test-asserted: `Tools/HaemophilusSendingRkiExportTests.DataTable_ContainsOnlyPayloadColumnsAfterCleanup`, `Tools/DuplicatePatientResolverTests.WhenCleanupFinished_PatientColumnsAreRemoved` |
| FR-006 · US1.2 · US2.8 · US3.6 · US4.3 · US4.4 · SC-003 | Single-sheet `.xlsx` named `<Prefix>-Export_<start>-<end>.xlsx` with `RKI`, `Labor`, `PubMLST`, `Iris`, `LGA`. | Suggested names `RKI-Export_20240101-20241231.xlsx` (both RKI exports), `Labor-Export_…` (both laboratory exports), `PubMLST-Export_…`, `Iris-Export_…`, `LGA-Export_…`; content type `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet`; one worksheet per file. | Match | File | all `verification/011-*.xlsx`; also test-asserted: `Controllers/PatientSendingControllerTests.LaboratoryExport_ValidQuery_CreatesExcel` (one worksheet) |
| FR-007 · EC2 | Start after end and a missing end date are not rejected; header row only. | 31.12.2024–01.01.2024 (form and direct address, both pathogens): header only, `Labor-Export_20241231-20240101.xlsx`. Missing end date (LGA form, direct address): header only, file name `…_20240101-00010101.xlsx` (not in the spec). | Spec deviation | File | `verification/011-h-labor-reversed.xlsx`, `011-m-lga-noend.xlsx` |
| FR-008 · US1.4 | Laboratory exports: all sampling locations, no interpretation or geography filter, identifying patient data. | Haemophilus 2024: 8 rows incl. HC (non-invasive), HE, HG, HB ("Ausland"), HF; columns "Initialen", "Geburtsdatum", "Postleitzahl", "Wohnort", "Landkreis", "Bundesland". Meningococci 2024: MB (Rachenabstrich, "kein Nachweis") included. | Match | File | `verification/011-h-labor-2024.xlsx`, `011-m-labor-2024.xlsx` |
| FR-009 · US2.2 · EC9 | Haemophilus RKI: only blood, liquor, other-invasive; no evaluation filter. | Both modes: 7 rows; HC missing; HE ("H. parainfluenzae") and HG ("H. haemolyticus") included. | Match | File | `verification/011-h-rki-2024-nein.xlsx`, `011-h-rki-2024-ja.xlsx` |
| FR-010 · US3.2 | PubMLST: invasive locations only, county text "Ausland" excluded, five non-H. influenzae evaluations excluded. | Both modes: 5 rows; HC, HB (county "Ausland") and HE ("H. parainfluenzae") missing; patients without county included. | Match | File | `verification/011-h-pubmlst-2024-nein.xlsx`, `011-h-pubmlst-2024-ja.xlsx` |
| FR-011 · US2.6 · US4.2 | Meningococci RKI, IRIS, LGA: invasive location and meningococci not ruled out; same selection; no overseas exclusion. | All three files have the same rows in every range: 2020 only MZ004/20; 2024 MA, MC, ME (Serum), MF ("Ausland"), not MB (non-invasive) and not the 2024 isolates while they matched rule 27 ("kein Nachweis", first export); November 2026 nine rows without MZ008/26, MZ012/26, MZ013/26 ("kein Nachweis") and MZ009/26 (non-invasive). | Match | File | `verification/011-m-rki-2020.xlsx`, `011-m-iris-2020.xlsx`, `011-m-lga-2020.xlsx`, `011-m-rki-2024.xlsx`, `011-m-rki-2024c.xlsx`, `011-m-iris-2024c.xlsx`, `011-m-lga-2024c.xlsx`, `011-m-rki-202611.xlsx` |
| DATA-039 (007 FR-011, Edge Cases) | Unmatched Meningococci interpretation counts as "meningococci found". | MC, ME, MF (empty "Regel") are in the 2024 RKI, IRIS and LGA files; in November 2026 the six submissions without rule and the rule 32 submission MZ007/26 (DEC-005) are in all three. | Defect confirmed | File | `verification/011-m-labor-2024b.xlsx`, `011-m-rki-2024c.xlsx`, `011-m-labor-202611.xlsx`, `011-m-lga-202611.xlsx` |
| FR-013 · US2.5 · DEC-010 | Meningococci and laboratory exports offer no adjustment option and apply no duplicate cleaning or column removal. | No option on the five pages; no Meningococci or laboratory file has `duplicate_group`; the laboratory files keep the identifier columns. The 012 verification showed equal Meningococci RKI files with and without `Unadjusted=Yes`. | Match | Browser | `verification/011-MeningoPatientSending-RkiExport.png`, `011-m-labor-2024b.xlsx` |
| EC17 | The Meningococci RKI page receives "unadjusted" but neither shows nor uses it. `[POTENTIAL-REFACTORING]` | `MeningoPatientSendingController.RkiExportWithPotentialAdjustments(FromToQueryWithAdjustment query)` passes `query` only to the selection and the file name. | Match | Code | `HaemophilusWeb/Controllers/MeningoPatientSendingController.cs` (REF-005) |
| FR-014 · EC6 | County and state numbers from the county list matched on the county text (fill words, case); empty without match; Meningococci log a warning, Haemophilus does not. | "Landkreis SV-011-Ort" (Haemophilus RKI) and "Kreis SV-011-Ort" (Meningococci RKI, LGA) → "09111" / "09"; "Ausland", "Paris", "SV-011-Unbekannt" → empty. Application log: "County SV-011-Unbekannt not found for sending 20." twice per Meningococci RKI and LGA run (once per number column); no warning for the Haemophilus counties. | Match | File | `verification/011-h-rki-2024-nein.xlsx`, `011-m-rki-2024b.xlsx`, `011-m-lga-2024b.xlsx`; `HaemophilusWeb/Logs/2026-11-08.txt`; also test-asserted: `Tools/HaemophilusSendingRkiExportTests.DataTable_ContainsRkiColumns`, `DataTable_UnknownCounty_LeavesCountyAndStateNumbersEmpty` (added) |
| EC7 · DATA-025 | With several matching county versions the earliest validity wins, regardless of the submission date. | Versions "09111" (since 2000) and "08222" (since 2015): 2024 submissions get "09111" in all three files. | Defect confirmed | File | `verification/011-h-rki-2024-nein.xlsx`, `011-m-rki-2024b.xlsx`, `011-m-lga-2024b.xlsx` |
| FR-015 · US1.5 · US1.6 · US2.4 · US2.7 · US3.4 · US4.3 · US4.4 | Each export contains the column content of its user story and the catalogue. | Header rows of the seven files match the user stories (Haemophilus RKI 23, laboratory 77, PubMLST 42; Meningococci laboratory 86, RKI 36, IRIS 27, LGA 12 columns). The existing tests asserted only the column counts; added tests assert every header and the order. | Match | Unit test (added) | `Tools/HaemophilusSendingRkiExportTests`, `HaemophilusSendingLaboratoryExportTests`, `HaemophilusPubMlstExportTests`, `MeningoSendingLaboratoryExportTests`, `MeningoSendingRkiExportTests`, `MeningoSendingIrisExportTests`, `MeningoStateAuthorityExportTests`: `DataTable_ColumnHeadersAreInDocumentedOrder`; files as above |
| US3.5 | Country "Germany", region the English state name, dates `yyyy-MM-dd`, year, serotypes `a`–`f`/`NT`/`ND`, sex `f`/`m`/empty, source, beta-lactamase, SIR, `age_mth` under 12 months only. | As stated (Bavaria, 2024-01-03, b/NT, f/m, blood or free text, positive/negative, `age_mth` 6 only for HA; divers and keine Angabe → empty sex), plus two values the spec did not name: region "n.a." for the state "n.n." (HH) and serotype "Haemophilus Hemolyticus" for HG. | Spec deviation | File | `verification/011-h-pubmlst-2024-nein.xlsx`; also test-asserted: `Tools/HaemophilusPubMlstExportTests.DataTable_DatesAreFormattedCorrectly`, `DataTable_ContainsSexProperties`, `DataTable_UnknownState_RegionIsNotAvailable` (added) |
| EC5 | Unknown birth date: RKI birth month/year `0`, age columns `0`; missing sampling date "keine Angabe"; Haemophilus RKI stem `-1`. | As stated (HH, MC, HB); the LGA export leaves the birth month of MC empty (not in the spec). | Spec deviation | File | `verification/011-h-rki-2024-nein.xlsx`, `011-m-rki-2024b.xlsx`, `011-m-lga-2024b.xlsx`, `011-h-labor-2024.xlsx`; also test-asserted: `Tools/HaemophilusSendingRkiExportTests.DataTable_WithoutBirthDateSamplingDateAndStemNumber_ExportsPlaceholders`, `Tools/MeningoStateAuthorityExportTests.DataTable_WithoutBirthDate_LeavesBirthMonthEmpty` (added) |
| EC10 · DATA-004 | The PubMLST hint promises overseas data and all locations in unadjusted mode, but both are excluded in both modes. `[SUSPECTED-DEFECT]` | "Ja" and "Nein" files have the same five rows; HB (county "Ausland") and HC (non-invasive) missing from both. | Defect confirmed | File | `verification/011-h-pubmlst-2024-ja.xlsx`, `011-h-pubmlst-2024-nein.xlsx`, `011-PatientSending-PubMlstExport.png` |
| EC11 · DATA-004 | Overseas check uses the county text, so a state "Ausland" with other county text is exported. `[SUSPECTED-DEFECT]` | HF (state "Ausland", county "Paris") is in both PubMLST files. | Defect confirmed | File | `verification/011-h-pubmlst-2024-nein.xlsx` |
| EC12 · DATA-004 | State "Ausland" gets region `blood`. `[SUSPECTED-DEFECT]` | HF: region `blood`. | Defect confirmed | File | `verification/011-h-pubmlst-2024-nein.xlsx` |
| EC13 · DATA-004 | H. haemolyticus is not excluded. `[SUSPECTED-DEFECT]` | HG (H19999) in both PubMLST files, serotype "Haemophilus Hemolyticus". | Defect confirmed | File | `verification/011-h-pubmlst-2024-nein.xlsx` |
| EC14 | `AMX_MIC` always empty, `AMX_SIR` from Ampicillin. | The tests assert the empty `AMX_MIC` column and `AMX_SIR` derived from the Ampicillin evaluation. | Match | Unit test (existing) | `Tools/HaemophilusPubMlstExportTests.DataTable_ContainsEmptyColumnsAtIndex("AMX_MIC")`, `DataTable_ContainsAmxAntibioticSusceptibilityInferredFromAmpicillin` |
| EC15 · DATA-005 | Unknown birth date yields age 0 years and 0 months in PubMLST and IRIS. `[SUSPECTED-DEFECT]` | HH: `age_yr` 0, `age_mth` 0 (PubMLST); MC: 0 and 0 (IRIS). | Defect confirmed | File | `verification/011-h-pubmlst-2024-nein.xlsx`, `011-m-iris-2024b.xlsx` |
| EC16 | PubMLST reuses the IRIS-named selection with a TODO. `[POTENTIAL-REFACTORING]` | `PatientSendingController.PubMlstExport` calls `SendingsMatchingExportQuery(queryWithAdjustment, ExportType.Iris)` after "//TODO clarify and merge method with Iris-Export should actually be the same". | Match | Code | `HaemophilusWeb/Controllers/PatientSendingController.cs` (REF-005) |
| EC18 · DEC-006 · US5.3 | LGA permitted for Public Health but not in its navigation. `[SUSPECTED-DEFECT]` | `Rki1` navigation without LGA; form HTTP 200, direct run returns `LGA-Export_20200101-20201231.xlsx`. | Defect confirmed | Browser | `verification/011-US5-menu-Rki1.png` |
| EC19 · DEC-001 · US5.5 | Export menu shown to Administrator-only accounts, every export refuses them. `[SUSPECTED-DEFECT]` | `Admin1` sees the full Export menu; all seven addresses (form and run) answer HTTP 302 to the sign-in page. | Defect confirmed | Browser | `verification/011-US5-menu-Admin1.png` |
| FR-016 · FR-017 · US5.1 · US5.2 · US5.4 · SC-005 | Standard User: full menu and all seven exports; Public Health: only the two RKI exports as top-level entries, LGA by address, refused Laboratory/PubMLST/IRIS; no anonymous access. | `Standard1` menu: "Haemophilus": RKI, Labor, PubMLST, Einsender; "Meningokokken": Labor, IRIS, LGA, RKI, Einsender; all seven run. `Rki1`: top-level "RKI Export (Haemophilus)", "RKI Export (Meningokokken)" (both run); Laboratory (both), PubMLST and IRIS answer HTTP 302 to `/Account/Login?ReturnUrl=…`. No account: all seven HTTP 302 to the sign-in page. | Match | Browser | `verification/011-US5-menu-Rki1.png` |
| EC20 · SEC-005 | Forms embed a token, but the exports accept any method and do not verify it. `[POTENTIAL-REFACTORING]` | GET `/PatientSending/LaboratoryExport?From=2024-01-01&To=2024-12-31` returns the file; a POST of only the dates without token to `/MeningoPatientSending/RkiExport` returns `RKI-Export_20200101-20201231.xlsx`. | Defect confirmed | Browser | request results in the run log |
| EC21 · DATA-019 | A write failure is only traced; the user downloads the empty or partial temp file. `[SUSPECTED-DEFECT]` | Not reproducible: needs a failing spreadsheet write on the server. | Not verifiable | Not verifiable | — |
| EC22 · SEC-012 | Each export leaves its temp file on the server. `[POTENTIAL-REFACTORING]` | Every export run left a `tmp*.tmp` spreadsheet in the temp directory of the account running IIS Express, of the size of the downloaded file (e.g. 5318 bytes for `011-h-labor-2024.xlsx`). | Defect confirmed | File | listing of `%TEMP%\tmp*.tmp` (31 files of this run between 12:10 and 12:15, plus older ones of earlier verifications) |
| EC23 | The worksheet is not given an explicit name. `[POTENTIAL-REFACTORING]` | Every file names its sheet "Table1" (`xl/workbook.xml`), the default name of the table. | Spec deviation | File | all `verification/011-*.xlsx` (REF-005) |
| EC24 · TEST-002 | The PubMLST empty-query test calls the Laboratory export. `[POTENTIAL-REFACTORING]` | `PubMlstExport_EmptyQuery_ForwardToViewWithQueryForLastYear` calls `controller.LaboratoryExport(new FromToQuery())`. | Match | Code | `HaemophilusWebTests/Controllers/PatientSendingControllerTests.cs` |
| DATA-036 (005 US3.5) | — (not in the spec) | Meningococci laboratory and RKI files show "PubMLST ID" 0 with empty typing columns for every 2024 isolate (saved without lookup), "-" for the seeded 2020 isolates without linked record. | Defect confirmed | File | `verification/011-m-labor-2024b.xlsx`, `011-m-rki-2024b.xlsx`, `011-m-labor-2020.xlsx` |
| DATA-038 (005 FR-011) | — (not in the spec) | MA, MC, ME: material "Nicht angewachsen" in the laboratory and IRIS files although their isolates now have "typisches Wachstum". | Defect confirmed | File | `verification/011-m-labor-2024b.xlsx`, `011-m-iris-2024b.xlsx` |

## Unit tests added

- `HaemophilusWebTests/Tools/HaemophilusSendingRkiExportTests.cs`: `DataTable_ColumnHeadersAreInDocumentedOrder`
  (FR-004, FR-015, US2.4), `DataTable_WithoutBirthDateSamplingDateAndStemNumber_ExportsPlaceholders` (EC5),
  `DataTable_ExportsGenderInitial` (5 cases; EC8), `DataTable_UnknownCounty_LeavesCountyAndStateNumbersEmpty` (FR-014)
- `HaemophilusWebTests/Tools/HaemophilusPubMlstExportTests.cs`: `DataTable_ColumnHeadersAreInDocumentedOrder` (US3.4),
  `DataTable_UnknownState_RegionIsNotAvailable` (US3.5)
- `HaemophilusWebTests/Tools/HaemophilusSendingLaboratoryExportTests.cs`: `DataTable_ColumnHeadersAreInDocumentedOrder`
  (US1.5)
- `HaemophilusWebTests/Tools/MeningoSendingLaboratoryExportTests.cs`: `DataTable_ColumnHeadersAreInDocumentedOrder`
  (US1.6)
- `HaemophilusWebTests/Tools/MeningoSendingRkiExportTests.cs`: `DataTable_ColumnHeadersAreInDocumentedOrder` (US2.7),
  `DataTable_WithoutBirthDateAndGender_ExportsZeroAndQuestionMark` (EC5, EC8)
- `HaemophilusWebTests/Tools/MeningoSendingIrisExportTests.cs`: `DataTable_ColumnHeadersAreInDocumentedOrder` (US4.3)
- `HaemophilusWebTests/Tools/MeningoStateAuthorityExportTests.cs`: `DataTable_ColumnHeadersAreInDocumentedOrder`
  (US4.4), `DataTable_WithoutBirthDate_LeavesBirthMonthEmpty` (EC5)

13 methods, 17 test cases. The selection rules of the export actions (FR-002, FR-008 to FR-011) remain without unit
tests; they live in controller queries against the database context and were verified on the file path.

## Spec corrections applied

- EC1: an unreadable or emptied start date shows the binding message and replaces the submitted end date; only an
  address without start date shows no message (was: no error message in either case).
- EC2: a missing end date appears as `00010101` in the file name (added).
- EC4: submissions without stem number come first, in no defined order (added).
- EC5: the LGA export leaves an unknown birth month empty (added).
- US3.5: region "n.a." for the state "n.n." and serotype "Haemophilus Hemolyticus" for an exported H. haemolyticus
  isolate (added).
- EC23 and Assumptions: the worksheet appears as "Table1" (was: no name; handling not verified).
- Assumptions: a refused or anonymous export request is redirected to the sign-in page (was: assumed, not exercised).
- FR-011: `[SUSPECTED-DEFECT]` note for unmatched interpretations (007, DATA-039); Edge Cases: notes for the empty
  PubMLST record shown as "PubMLST ID" 0 (005, DATA-036) and the exported material "Nicht angewachsen" (005,
  DATA-038).

## Backlog updates

- DATA-004: confirmed on system 2026-11-08 for the hint, the county-text check, region `blood` and H. haemolyticus.
- DATA-005: confirmed on system 2026-11-08 (PubMLST, IRIS).
- DATA-025: confirmed on system 2026-11-08 (Haemophilus RKI, Meningococci RKI, LGA).
- DATA-036: confirmed in the exports 2026-11-08; source extended by 011 Edge Cases.
- DATA-038: confirmed in the exports 2026-11-08; source extended by 011 Edge Cases.
- DATA-039: confirmed on system 2026-11-08 with own 2024 submissions.
- DEC-001: observation for the exports recorded (no decision).
- DEC-006: observation recorded (no decision).
- SEC-005: confirmed for the exports (GET, POST without token).
- SEC-012: confirmed for the spreadsheet exports.
- REF-005: worksheet name "Table1" recorded.
- TEST-002: header tests added for the seven exports; PubMLST empty-query test and the selection rules still untested.

## Not verifiable here

- EC21 (DATA-019): the write-failure path needs a spreadsheet write that fails on the server (e.g. a full or
  read-only temp directory); no UI path produces it. A unit test with an injected failing writer would make it
  verifiable once the writer can be substituted.

## Open questions for the owner

- The unparseable or emptied start date discards the end date the user entered and shows the previous year again.
  Intended, or should the form keep the entered end date?
- Submissions without stem number are exported first and in no defined order. Should the exports order them (e.g. by
  laboratory number)?
- The application log of the running host (`HaemophilusWeb/Logs/`) lies inside the application folder; it now holds
  the county warnings of this run. No action taken.
