# Verification: Sender export

**Spec**: 013 · **Date**: 2026-11-08 · **Build**: `ec29065` · **Host**: ScreenshotTests
SpecVerificationHost (`http://localhost:9899`, catalog `HaemophilusWeb_Screenshots`) ·
**Accounts used**: `Standard1`, `Rki1`, `Admin1`, no account · **Scope**: full (column definition on the unit-test
path per `AGENTS.md`; form, date range, file names, row selection, row content and roles on the browser/file path)

## Summary

| Verdict | Count |
| --- | --- |
| Match | 16 |
| Spec deviation | 3 |
| Defect confirmed | 5 |
| New defect | 0 |
| Not verifiable | 2 |

By primary path: Browser 5 · File 14 · DB 0 · Unit test (existing) 0 · Unit test (added) 2 · Code 3 · Not verifiable 2
(adds up to the row count). Also test-asserted: 6 rows with a browser or file verdict. Test run:
`vstest.console.exe … /TestCaseFilter:"FullyQualifiedName~HaemophilusWeb.Controllers.SenderControllerTests"` → 15
passed, 0 failed (4 of them added); full suite → 1143 passed, 0 failed.

Edge cases are numbered in the order of the spec's Edge Cases list before this verification: EC1 missing or unreadable
start date, EC2 start after end / missing end / no sender, EC3 end date as calendar date, EC4 heading "Entnahmedatum",
EC5 row order, EC6 soft-deleted submissions counted, EC7 range matched twice, EC8 count as text, EC9 stem and
laboratory number prefixes, EC10 submission without isolate, EC11 unused patient data, EC12 no patient data, EC13
deleted flag not exported, EC14 anti-forgery token, EC15 write failure, EC16 temp files, EC17 worksheet name, EC18
Administrator menu, EC19 controller architecture, EC20 test coverage. FR-013 (documentation scope) states no behavior
and has no row.

Test year 2022 (no submission of either database had a sampling or receiving date in 2021 or 2022 before the run).
Own records created through the UI (laboratory numbers of 2026 because they are assigned in the year of saving;
"Labnr. Einsender" and "Bemerkung" `SV-013-<key>`; synthetic patients born 01.02.2000, weiblich):

| Sender (number) | Contact data | Submissions (key, sampled / received, stem number, laboratory number) |
| --- | --- | --- |
| SV-013-Alpha (4) | all eleven fields filled; "Bemerkung" changed from "SV-013-alt" to "SV-013-neu" after the submissions were saved | Haemophilus: HA 01.01.2022 / 03.01.2022, H31301, KL024/26; HB none / 31.12.2022, none, KL025/26; HC 31.12.2022 / 02.01.2023, H31302, KL026/26; HD 31.12.2021 / 02.01.2022, none, KL027/26; HE 15.06.2022 / 17.06.2022, H31303, KL028/26, soft-deleted; HH none / 01.01.2023, none, KL031/26. Meningococci: MA 10.05.2022 / 12.05.2022, DE31301, MZ020/26; MC 20.08.2022 / 22.08.2022, none, MZ022/26 |
| SV-013-Beta (5) | "Name" and "Telefon #1" only | Meningococci: MB 12.05.2022 / 14.05.2022, none, MZ021/26 |
| SV-013-Gamma (6) | soft-deleted after its submission was saved | Haemophilus: HF 10.03.2022 / 12.03.2022, KL029/26 |
| SV-013-Delta (7) | — | Haemophilus: HG 11.04.2022 / 13.04.2022, KL030/26, soft-deleted |

The 2020 runs read the seeded senders 1–3 (fixture `SV-Seed-DEC-002`: KL002/20 and MZ002/20 active, KL003/20 and
MZ003/20 soft-deleted). No seeded record was changed.

## Results

| ID | Spec says | App/test shows | Verdict | Path | Evidence |
| --- | --- | --- | --- | --- | --- |
| FR-001 · FR-002 · US1.1 · SC-006 | One export per pathogen at its own address, "Einsender Export" in the Export menu; without start date a page "Export der Einsender (Haemophilus)" / "(Meningokokken)" with "Entnahmedatum der Einsendung(en)", "Start-Datum"/"Ende-Datum" pre-filled with the previous year and "Exportieren nach Excel". | `/Sender/Export` and `/MeningoSender/Export` (HTTP 200): heading (`h1`) "Export der Einsender (Haemophilus)" / "Export der Einsender (Meningokokken)", "Entnahmedatum der Einsendung(en)", 01.01.2025 / 31.12.2025, "Exportieren nach Excel"; the `Standard1` Export menu lists "Einsender Export" under "Haemophilus" and under "Meningokokken". | Match | Browser | `verification/013-Sender-Export.png`, `013-MeningoSender-Export.png`, `013-US3-menu-Standard1.png`; also test-asserted: `Controllers/SenderControllerTests.Export_WithoutRange_DefaultsToPreviousCalendarYear` |
| EC1 | Without start date the form is shown pre-filled even with an end date; an unparseable start date behaves the same way and shows no error message. | `?To=2022-12-31`: pre-filled form, no message. `?From=abc&To=2022-12-31` and "abc" typed into the form: pre-filled form (end date replaced by 31.12.2025) with "Start-Datum ist kein gültiger Wert für abc."; no download. | Spec deviation | Browser | `verification/013-EC1-unparseable-start.png`, `013-EC1-ui-unparseable-start.png` |
| FR-003 · US1.3 · SC-002 · EC3 | A submission is in the range by its sampling date, without one by its receiving date, both ends inclusive. | 2022 Haemophilus file: HA (sampled on the start day), HC (sampled on the end day, received 2023) and HB (no sampling date, received on the end day) counted; HD (sampled 31.12.2021, received 02.01.2022) and HH (no sampling date, received 01.01.2023) not counted. | Match | File | `verification/013-h-2022.xlsx`; also test-asserted: `Controllers/SenderControllerTests.Export_SelectsActiveSendersBySamplingOrReceivingDateOfThePathogen` (added) |
| FR-004 · EC2 | Start after end and a missing end date are not rejected, header row only, as when no sender had a submission; no record of the run is kept. | 31.12.2022–01.01.2022 (form): header only, `Einsender-Haemophilus-Export_20221231-20220101.xlsx`; 2021 (form, no submissions): header only; missing end date (direct address): header only, file name `Einsender-Meningokokken-Export_20220101-00010101.xlsx` (not in the spec). The catalog has no table for export runs or other export records. | Spec deviation | File | `verification/013-h-reversed.xlsx`, `013-m-2021-empty.xlsx`, `013-m-noend.xlsx`; table list of `HaemophilusWeb_Screenshots` |
| FR-005 · US1.4 · US1.5 · US2.2 · SC-001 | One row per non-deleted sender with a non-deleted submission of the pathogen in the range; no other senders. | 2022 Haemophilus: only SV-013-Alpha, once despite four submissions; SV-013-Gamma (soft-deleted sender with HF in the range), SV-013-Beta (Meningococci only) and SV-013-Delta (only a soft-deleted submission) absent. 2022 Meningococci: Alpha and Beta. 2020: senders 1, 2 (Haemophilus) and 1, 2, 3 (Meningococci). | Match | File | `verification/013-h-2022.xlsx`, `013-m-2022.xlsx`, `013-h-2020.xlsx`, `013-m-2020.xlsx`; also test-asserted: `Controllers/SenderControllerTests.Export_SelectsActiveSendersBySamplingOrReceivingDateOfThePathogen` (added) |
| FR-006 · US1.6 · US2.3 · SC-004 | Columns "Einsendernummer", "Name", "Abteilung", "Straße", "Postleitzahl", "Ort", "Telefon #1", "Telefon #2", "Fax", "E-Mail", "Bemerkung", then the three number columns; current sender data, empty cells for missing values. | The added tests assert all 14 headers in this order for both controllers and the contact cells of a row. Files: the same 14 headers; Alpha with all eleven contact values and the edited "Bemerkung" "SV-013-neu" in both files; Beta with empty cells except "Name" and "Telefon #1". | Match | Unit test (added) | `Controllers/SenderControllerTests.Export_WithRange_WritesDocumentedFileNameAndColumns` (2 cases), `Export_RowListsContactDataAndNumbersOfThePathogenOnly`; files as above |
| FR-007 · US1.7 · US2.1 · SC-003 | Count, stem numbers without prefix (`-` when absent) and laboratory numbers with prefix, comma-separated without spaces, same order; per pathogen. | Alpha, Haemophilus: 4, "31301,-,31302,31303", "KL024/26,KL025/26,KL026/26,KL028/26"; Meningococci: 2, "31301,-", "MZ020/26,MZ022/26"; Beta: 1, "-", "MZ021/26". Count equals the length of both lists; each file lists only its pathogen. | Match | File | `verification/013-h-2022.xlsx`, `013-m-2022.xlsx`; also test-asserted: `Controllers/SenderControllerTests.Export_RowListsContactDataAndNumbersOfThePathogenOnly` (added) |
| FR-008 · EC6 · DEC-002 | Per-sender values include soft-deleted submissions; a sender whose only submissions in the range are deleted is not listed. `[SUSPECTED-DEFECT]` | Alpha's count 4 includes the soft-deleted HE (H31303, KL028/26); SV-Seed-DEC-002 (2020): 2, "2,3", "KL002/20,KL003/20" and "MZ002/20,MZ003/20" with KL003/20 and MZ003/20 soft-deleted; SV-013-Delta (only HG, soft-deleted) not listed. Observation only; DEC-002 stays open. | Defect confirmed | File | `verification/013-h-2022.xlsx`, `013-h-2020.xlsx`, `013-m-2020.xlsx` |
| EC7 | The range is matched twice with diverging deletion handling. `[POTENTIAL-REFACTORING]` | `SenderControllerBase.SendersMatchingExportQuery` filters `!s.Deleted` and the range; `CreateExportDefinition` repeats the range filter without the deletion filter. | Match | Code | `HaemophilusWeb/Controllers/SenderControllerBase.cs` (REF-005) |
| FR-009 · EC12 | No patient data; no duplicate cleaning or anonymization. | The files have only the 14 sender and number columns; the patients' initials and birth dates appear nowhere; sender contact data is exported unchanged. | Match | File | `verification/013-h-2022.xlsx`, `013-m-2022.xlsx` |
| FR-010 · US1.2 | Single-sheet `.xlsx` named `Einsender-<Pathogen>-Export_<start>-<end>.xlsx`. | `Einsender-Haemophilus-Export_20220101-20221231.xlsx`, `Einsender-Meningokokken-Export_20220101-20221231.xlsx` (and 2020); content type `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet`; one worksheet. | Match | File | all `verification/013-*.xlsx`; also test-asserted: `Controllers/SenderControllerTests.Export_WithRange_WritesDocumentedFileNameAndColumns` (added) |
| EC8 · DATA-026 | "Anzahl Einsendungen" is a text cell, "Einsendernummer" a number. `[SUSPECTED-DEFECT]` | In every file "Einsendernummer" is a numeric cell and "Anzahl Einsendungen" a text cell. | Defect confirmed | File | `verification/013-h-2022.xlsx`, `013-m-2022.xlsx` |
| EC9 | Stem numbers without `H`/`DE`, laboratory numbers with `KL`/`MZ`; a Meningococci laboratory number entered with a leading `-` shows as `NR…`. | "31301" for H31301 and DE31301, "KL…"/"MZ…" laboratory numbers. A leading `-` cannot be entered through the UI; the existing test asserts "NR01/2019" for "-01/2019". | Match | File | `verification/013-h-2022.xlsx`, `013-m-2022.xlsx`; also test-asserted: `Utils/ReportFormatterTests.ToLaboratoryNumberWithPrefix_ValidInput_AddsPrefix("-01/2019",Meningococci,"NR01/2019")` |
| EC4 | The heading says "Entnahmedatum der Einsendung(en)", although submissions without sampling date are matched by receiving date. | Heading as stated; HB (no sampling date) is counted by its receiving date. | Match | File | `verification/013-Sender-Export.png`, `013-h-2022.xlsx` |
| EC5 | Rows and numbers within a row are not explicitly ordered. `[POTENTIAL-REFACTORING]` | Rows in ascending sender number (1, 2, 3; 4, 5); numbers in ascending submission order (KL024 … KL028/26), as the Assumptions expect; nothing in the code orders them. | Match | File | `verification/013-m-2022.xlsx`, `013-m-2020.xlsx` (REF-005) |
| EC10 | A submission without isolate record makes the export fail with an error page. | Not reproducible: the UI and the seed always create the isolate together with the submission. | Not verifiable | Not verifiable | — |
| EC11 | The selection loads the patient of every matching submission but uses none of its data. `[POTENTIAL-REFACTORING]` | `SendersMatchingExportQuery` calls `.Include(s => s.Patient)` and then selects only `SenderId`. | Match | Code | `HaemophilusWeb/Controllers/SenderControllerBase.cs` (REF-005) |
| EC13 | The sender's soft-delete flag is not exported. | No "Gelöscht" column; the soft-deleted SV-013-Gamma is absent. | Match | File | `verification/013-h-2022.xlsx` |
| EC14 · SEC-005 | The form embeds a token, but the export accepts any method and does not verify it. `[POTENTIAL-REFACTORING]` | GET `/Sender/Export?From=2022-01-01&To=2022-12-31` returns the Haemophilus file (same row as the form export); a POST of only the dates without token to `/MeningoSender/Export` returns HTTP 200 with `Einsender-Meningokokken-Export_20220101-20221231.xlsx`. | Defect confirmed | Browser | `verification/013-h-2022-get.xlsx`; POST result in the run log |
| EC15 · DATA-019 | A write failure is only traced; the user downloads the empty or partial temp file. `[SUSPECTED-DEFECT]` | Not reproducible: needs a failing spreadsheet write on the server. | Not verifiable | Not verifiable | — |
| EC16 · SEC-012 | Each export leaves its temp file on the server. `[POTENTIAL-REFACTORING]` | Each of the nine export runs left a `tmp*.tmp` spreadsheet in the temp directory of the account running IIS Express, of the size of the download (2394, 2450, 2512, 2601 bytes for the four year files). | Defect confirmed | File | listing of `%TEMP%\tmp*.tmp` (12:34:08–12:34:56) |
| EC17 · REF-005 | The worksheet is not given an explicit name. `[POTENTIAL-REFACTORING]` | Every file names its sheet "Table1". | Spec deviation | File | all `verification/013-*.xlsx` |
| FR-011 · FR-012 · US3.1 · US3.2 · US3.4 · SC-005 | Standard User runs both; Public Health sees none and is refused; unauthenticated visitors must sign in. | `Standard1`: both entries, both exports run. `Rki1`: navigation shows only "RKI Export (Haemophilus)" and "RKI Export (Meningokokken)"; both sender addresses (form and run) HTTP 302 to `/Account/Login?ReturnUrl=…`. No account: the same four requests HTTP 302 to the sign-in page. | Match | Browser | `verification/013-US3-menu-Standard1.png`, `013-US3-menu-Rki1.png` |
| EC18 · DEC-001 · US3.3 | Administrator-only accounts see both entries, but both exports refuse them. `[SUSPECTED-DEFECT]` | `Admin1` sees both "Einsender Export" entries; both addresses, form and run, HTTP 302 to the sign-in page. | Defect confirmed | Browser | `verification/013-US3-menu-Admin1.png` |
| EC19 | Export and aggregation live in the shared sender controller, with parallel controllers per pathogen. `[POTENTIAL-REFACTORING]` | `SenderControllerBase<TPatient, TSending>` holds `Export`, `CreateExportDefinition` and `SendersMatchingExportQuery`; `SenderController` and `MeningoSenderController` differ only in `DatabaseType` and `QuerySendings`. | Match | Code | `HaemophilusWeb/Controllers/SenderControllerBase.cs`, `SenderController.cs`, `MeningoSenderController.cs` |
| TEST-001 (sender export) | No automated test covers the sender export (EC20). | Tests added for file name, headers, selection and row content of non-deleted submissions; the counting of soft-deleted submissions (DEC-002) and the text count cell (DATA-026) are deliberately not tested. | Match | Unit test (added) | `Controllers/SenderControllerTests` (see below) |

## Unit tests added

- `HaemophilusWebTests/Controllers/SenderControllerTests.cs`: `Export_WithRange_WritesDocumentedFileNameAndColumns`
  (2 cases; FR-006, FR-007, FR-010, US1.2, US1.6, SC-004), `Export_SelectsActiveSendersBySamplingOrReceivingDateOfThePathogen`
  (FR-003, FR-005, US1.3, US1.4, US2.2, SC-002), `Export_RowListsContactDataAndNumbersOfThePathogenOnly` (FR-006,
  FR-007, US1.7, US2.1, US2.3)

3 methods, 4 test cases. The tests run the private selection and column definition of the shared base controller
through reflection on isolated in-memory sets. They use only non-deleted submissions and do not assert the cell type
of the count, so DEC-002 and DATA-026 are not cemented.

## Spec corrections applied

- EC1: an unparseable start date shows the binding message and replaces the submitted end date; only an address
  without start date shows no message (was: no error message in either case).
- EC2: a missing end date appears as `00010101` in the file name (added).
- EC17: the worksheet appears as "Table1" (was: no name).
- EC20 and the first assumption: test coverage updated to the tests added (was: no automated test).
- Assumptions: refused and anonymous requests are redirected to the sign-in page (was: assumed); the assumed row
  order is noted as observed.

## Backlog updates

- DATA-026: confirmed on system 2026-11-08.
- DEC-001: observation for the sender exports recorded (no decision).
- DEC-002: observation recorded with the seeded fixture and own records (no decision).
- SEC-005: confirmed for both sender exports (GET, POST without token).
- SEC-012: confirmed for both sender exports.
- REF-005: worksheet name "Table1" and the observed row order recorded for the sender export.
- TEST-001: sender export covered by tests added in the 013 verification (2026-11-08), except the DEC-002 behavior.

## Not verifiable here

- EC10: a submission without isolate record cannot be created through the UI or found in the seed; a seed fixture
  would make the failure observable (a unit test would pin a crash, so none was written).
- EC15 (DATA-019): the write-failure path needs a spreadsheet write that fails on the server; no UI path produces it.

## Open questions for the owner

- The unparseable start date discards the end date the user entered (same as 011). Intended?
- No external call was triggered; the route block recorded no blocked address.
