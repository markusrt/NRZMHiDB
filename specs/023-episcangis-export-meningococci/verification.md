# Verification: EpiScanGIS export (Meningococci)

**Spec**: 023 · **Date**: 2026-11-08 · **Build**: `121be4f` · **Host**: ScreenshotTests
SpecVerificationHost (`http://localhost:9899`, catalog `HaemophilusWeb_Screenshots`) ·
**Accounts used**: no account, `Standard1`, `Admin1`, `Rki1`, `Labor1` · **Scope**: full, with the export run itself
on the unit-test path: the export is admitted only by the secret `RegistrationToken`, whose value was neither read nor
used, so the browser checks cover the refusal path and the navigation only.

## Summary

| Verdict | Count |
| --- | --- |
| Match | 19 |
| Spec deviation | 0 |
| Defect confirmed | 7 |
| New defect | 2 |
| Not verifiable | 2 |

By primary path: Browser 3 · File 1 · DB 0 · Unit test (existing) 3 · Unit test (added) 10 · Unit test (scratch) 11 ·
Code 0 · Not verifiable 2 (adds up to the row count). Also test-asserted: 2 rows with a browser verdict. Test run:
`vstest.console.exe … /TestCaseFilter:"FullyQualifiedName~HaemophilusWeb.Controllers.EpiscanGisExportControllerTests"`
→ 34 passed, 0 failed (26 of them added); full suite → 1169 passed, 0 failed.

"Unit test (scratch)" means: the real `EpiscanGisExportController` (and, where named, the real `GeonamesService` with a
stubbed HTTP function) run with in-memory data in a separate test assembly compiled in the session scratchpad and not
committed, because a committed test would cement a defect and the run on the system needs the token. Its source and
output are kept in `verification/023-scratch-tests.cs` and `verification/023-scratch-tests-output.txt` (git-ignored).
Rows on that path are reproduced at unit level, not on the running system; the backlog entries say so.

Edge cases are numbered in the order of the spec's Edge Cases list before this verification: EC1 missing
`RegistrationToken` setting, EC2 submission without isolate, EC3 serogroup values, EC4 empty postal code, EC5 receiving
date before 2019-10-01 with a later sampling date, EC6 several runs per day, EC7 negative age, EC8 privacy, EC9
architecture and test coverage. EC6 (two runs deliver the same name) follows from FR-010 and has no row; EC8 is covered
by the rows on FR-002 and FR-008; EC9 is corrected as a coverage note. FR-013 (documentation scope) states no behavior
and has no row.

No records were created or changed; the catalog was only read through the refusal requests. No export year was needed.

## Results

| ID | Spec says | App/test shows | Verdict | Path | Evidence |
| --- | --- | --- | --- | --- | --- |
| FR-001 · US1.2 · SC-001 | Without token or with any other token the answer is "not found". | Anonymous: `/EpiscanGisExport`, `?token=`, `?token=SV-023-wrong` and `/EpiscanGisExport/Index?token=SV-023-wrong` → HTTP 404, no download. Test: null, empty, "token", "TOKEN", "Token " (valid value "Token" in the test configuration) → `HttpNotFoundResult`. | Match | Browser | `verification/023-anon-noToken.png`, `023-anon-emptyToken.png`, `023-anon-wrongToken.png`, `023-anon-wrongTokenIndex.png`; also test-asserted: `Controllers/EpiscanGisExportControllerTests.DownloadWithMissingOrDifferentToken_Returns404` (5 cases), `DownloadWithInvalidToken_Returns404` |
| FR-001 · US1.1 · US1.6 | The exact `RegistrationToken` value delivers the case file; no sign-in page. | The action compares with the `RegistrationToken` setting and returns a `FileContentResult` with one line per qualifying case. The registration side of US1.6 belongs to spec 021. | Match | Unit test (existing) | `EpiscanGisExportControllerTests.Download_ReturnsFileContent` |
| US1.2 · SEC-015 (new) | "Not found", as for an address that does not exist; nothing hints that the address is valid. | The refusal shows the web server's page "HTTP Error 404.0 - Not Found" (title "IIS 10.0 Detailed Error - 404.0 - Not Found"); `/SV023DoesNotExist` and `/EpiscanGisExport/SV023Nothing` show the framework's page "Die Ressource kann nicht gefunden werden." Both HTTP 404, but distinguishable. | New defect | Browser | `verification/023-anon-wrongToken.png` vs `023-anon-missingController.png`, `023-anon-missingAction.png` |
| FR-002 · US1.3 · US1.4 · US1.5 | Token decides, not the role; retrieval only; no anti-forgery check; no navigation entry. | `Standard1`, `Admin1`, `Rki1`, `Labor1`: without token and with `SV-023-wrong` → HTTP 404 each. POST with and without wrong token → 404. `Labor1` (all roles): no menu link contains "EpiScanGIS" (all 26 navigation entries listed, none for this export). Test: `HttpGet` and `AllowAnonymous` on the action, no `HttpPost`, `Authorize` or `ValidateAntiForgeryToken`. | Match | Browser | `verification/023-US1-3-<account>-wrongToken.png` (4), `023-US1-5-menu-Labor1.png`; also test-asserted: `Index_AdmitsAnonymousGetWithoutRoleOrAntiForgeryCheck` |
| FR-003 · US2.1 · US2.2 | Meningococci submissions only; soft-deleted left out; "Blut" and "Liquor" included. | A soft-deleted submission is missing from the file; all submissions with "Blut" or with "Liquor" are exported. Only `MeningoSendings` is read. | Match | Unit test (added) | `DownloadWithOneSoftDeletedSending_ReturnsOneLineLess`, `DownloadWithBloodOrLiquor_ExportsEverySending` (2) |
| US2.3 · DEC-008 · DATA-020 | Only "Blut"/"Liquor"; "Blut und Liquor" and the other invasive locations left out. `[SUSPECTED-DEFECT]` | Blood, Liquor exported; BloodAndLiquor, JointAspiration, Petechien, PleuralAspiration, Serum, OtherInvasive left out. Observation only; DEC-008 stays open. | Defect confirmed | Unit test (scratch) | scratch S1 |
| FR-004 · US2.4 · EC5 | Sampling date decides when present, otherwise receiving date; no end date. | Sampled 15.10.2019 included, 15.09.2019 excluded; without sampling date received 15.10.2019 included, 15.09.2019 excluded; received before, sampled after the start included and vice versa excluded; sampled 01.01.2030 included. | Match | Unit test (added) | `Download_SelectsByTimeWindow` (7) |
| US2.4 (boundary) · DEC-008 · DATA-020 | Sampled exactly 2019-10-01 left out, received 2019-10-01 without sampling date included. `[SUSPECTED-DEFECT]` | As stated. | Defect confirmed | Unit test (scratch) | scratch S2 |
| FR-005 · US2.5 · EC3 | Fresh interpretation; no serogroup, `NG`, `cnl` left out. | Asserted by the existing tests. | Match | Unit test (existing) | `DownloadWithTwoEntriesMissingSerogroup_ReturnsTwoLineLess`, `DownloadWithEntriesInterpretedAsNgAndCnl_ReturnsTwoLinesLess` |
| FR-006 · US2.6 · US4.4 · US5.1 · US5.3 · SC-005 | Lookup with "Postleitzahl" and "Wohnort"; a case without chosen place is left out, the run completes. | Lookup received ("91301", "Forchheim"); a case whose lookup answers nothing is missing, the other lines are delivered. | Match | Unit test (added) | `Download_QueriesCoordinateWithPatientPostalCodeAndCity`, `DownloadWithoutCoordinateForOneCase_ReturnsOneLineLess` |
| US5.2 | Place whose name equals "Wohnort"; its coordinates become columns 9 and 10. | With the real lookup component and a stubbed answer for 06773, "Gräfenhainichen" yields `;51.728924;12.456513`. | Match | Unit test (added) | `DownloadWithGeonamesService_UsesPlaceMatchingCity`; also `Services/GeonamesServiceTest.QueryCoordinate_MultipleMatches_ReturnsBestMatch` |
| US5.3 · DATA-020 | A "Wohnort" spelled differently or "Unbekannt" leaves the case out without notice. `[SUSPECTED-DEFECT]` (022) | "Graefenhainichen" (service: "Gräfenhainichen") and "Unbekannt" left out; 8 of 10 lines. | Defect confirmed | Unit test (scratch) | scratch S7 |
| US5.1 · US5.4 · REF-012 | One lookup per case before the reduction; sequential, plain HTTP. `[POTENTIAL-REFACTORING]` | 10 lookups for 10 cases yielding 9 lines (two cases of one patient). `GeonamesService.BaseUrl` is `http://api.geonames.org`; lookups run inside the `foreach` of `Index`. | Match | Unit test (scratch) | scratch S11; `Services/GeonamesService.cs` |
| US5.5 · EC4 | Foreign or empty postal code: outcome depends on the service. | Depends on the answer of the external service, which may not be called. | Not verifiable | Not verifiable | — |
| FR-007 · US4.1 · US4.2 · US4.3 · SC-003 | One line per patient number, longest line wins, with equal length the case read first. | Existing test: the line with full variants wins; added test: two equal-length lines (serogroup B read first, C second) keep B. Scratch S12: a complete line beats a line without sampling date. | Match | Unit test (added) | `DownloadWithTwoEqualLengthCasesOfSamePatient_KeepsTheCaseReadFirst`; also `DownloadWithTwoOfSamePatientMissingSerogroup_AddsPatientOnlyOnceAndChoosesTheOneWithMostData` |
| FR-008 · US3.2 · US3.7 · SC-004 | Ten columns in the documented order, no header, no quoting; no identifying data. | Literal line `1;10.01.2022;02.01.2022;C;22;14;5-5;10;49.71754;11.05877`. | Match | Unit test (existing) | `Download_FormatsCsvCorrectly` |
| US3.4 · DATA-020 | Values unescaped; a semicolon or line break shifts or splits the columns. `[POTENTIAL-REFACTORING]` | PorA VR1 "22;SV" adds a column, a line break in FetA VR adds a line (11 lines for 10 cases). | Defect confirmed | Unit test (scratch) | scratch S9 |
| FR-009 · US3.3 · US3.5 · US3.6 | `dd.MM.yyyy`; missing sampling date empty; age from receiving date without sampling date; decimal point. | Under `de-DE`: `1;10.01.2022;;C;22;14;5-5;10;49.71754;11.05877`. | Match | Unit test (added) | `DownloadWithoutSamplingDate_WritesEmptyColumnAndAgeAtReceiving` |
| US3.5 · DATA-005 | Unknown birth date → age `0`. `[SUSPECTED-DEFECT]` | Age column `0`. | Defect confirmed | Unit test (scratch) | scratch S3 |
| FR-010 · US3.1 · US3.8 · US2.7 | `text/csv`, `EpiscanGis_Export_<yyyyMMdd>.csv`, empty file when nothing qualifies, ascending patient number. | As stated; patient numbers stored in descending order come out ascending. | Match | Unit test (added) | `Download_ReturnsCsvNamedAfterCurrentDate`, `DownloadWithoutQualifyingCase_ReturnsEmptyFile`, `Download_WritesLinesInAscendingPatientNumberOrder` |
| FR-011 · US6.1 · SC-006 | The export logs nothing about requests or outcomes. | `HaemophilusWeb/Logs/2026-11-08.txt` unchanged (1758 bytes, last entry 12:20:15) after 14 refused requests and two requests to unknown addresses. The controller has no logger, so a successful run cannot log either. | Match | File | log file size and last entry before and after |
| US6.2 | A "not found" answer leaves out only that case; the run continues. | With the real lookup component, HTTP 404 (`WebException`) for one postal code: that case missing, the others delivered. The log message of spec 022 was not captured. | Match | Unit test (added) | `DownloadWithGeonamesNotFoundForOneCase_LeavesOutOnlyThatCase` |
| US6.3 · DATA-020 · SEC-012 | Network failure, time-out or unexpected answer ends the run; an empty temp file remains. `[SUSPECTED-DEFECT]` | A network failure (`AggregateException`/`HttpRequestException`) on the third lookup and an HTML answer (`JsonReaderException`) each end the run with an exception and leave one empty `tmp*.tmp`. The "Unhandled error" log entry was not exercised. | Defect confirmed | Unit test (scratch) | scratch S4, S6 |
| US6.3 · FR-011 · DATA-041 (new) | Any answer in an unexpected form ends the run. | A JSON answer without place list (a synthetic Geonames status message "daily limit … exceeded") for every lookup: run completes, empty file for 10 qualifying cases, no exception and no log entry (the lookup logs only `WebException`). | New defect | Unit test (scratch) | scratch S5; also `Services/GeonamesServiceTest.QueryPostalCode_UnexpectedResult_ReturnsEmptyArray` |
| FR-012 · US6.4 · US6.5 · SC-006 | No record changed; no file kept after a successful run. | With the temp folder redirected to an empty directory, a successful run leaves it empty; `SaveChanges` is never called. | Match | Unit test (added) | `Download_SavesNothingAndLeavesNoTemporaryFile`; scratch S4b |
| EC1 · SEC-003 | Without the setting a request without token is admitted. | Setting null: no token → file with 10 lines; `SV-023-wrong` → not found. On the host the key exists once in `Web.config`, `Web.template.config`, `Web.Debug.config` and `Web.Release.config` (counted with `grep -c`, values not read). | Defect confirmed | Unit test (scratch) | scratch S8 |
| EC2 | A selected submission without isolate ends the run with the generic server error. | `NullReferenceException`, one empty `tmp*.tmp` left. | Match | Unit test (scratch) | scratch S10 |
| EC7 | Sampling date before birth date gives a negative age. | Random test data with such dates gave age `-2`. | Match | Unit test (scratch) | scratch S12 |
| SEC-007 | The token travels in the address and appears in web-server logs. | The host keeps no IIS Express request log (`Documents\IISExpress\Logs` absent); the anonymous GET is shown in the FR-002 row. | Not verifiable | Not verifiable | — |
| TEST-001 (EpiScanGIS part) | Date window, deleted submissions, missing coordinates and failures untested. | Covered by the added tests except the defect parts ("Blut und Liquor", boundary day, abort cases). | Match | Unit test (added) | see "Unit tests added" |

## Unit tests added

`HaemophilusWebTests/Controllers/EpiscanGisExportControllerTests.cs` (existing fixture; 15 methods, 26 test cases):

- `DownloadWithMissingOrDifferentToken_Returns404` (5 cases; FR-001, US1.2)
- `Index_AdmitsAnonymousGetWithoutRoleOrAntiForgeryCheck` (FR-002, US1.3–US1.5)
- `DownloadWithOneSoftDeletedSending_ReturnsOneLineLess` (FR-003, US2.2)
- `DownloadWithBloodOrLiquor_ExportsEverySending` (2 cases; FR-003, US2.3 undisputed part)
- `Download_SelectsByTimeWindow` (7 cases; FR-004, US2.4 away from the boundary day, EC5)
- `DownloadWithoutCoordinateForOneCase_ReturnsOneLineLess` (FR-006, US5.3, SC-005)
- `Download_QueriesCoordinateWithPatientPostalCodeAndCity` (FR-006, US5.1)
- `DownloadWithGeonamesService_UsesPlaceMatchingCity` (US5.2)
- `DownloadWithGeonamesNotFoundForOneCase_LeavesOutOnlyThatCase` (US6.2, FR-011)
- `DownloadWithTwoEqualLengthCasesOfSamePatient_KeepsTheCaseReadFirst` (FR-007, US4.2)
- `DownloadWithoutSamplingDate_WritesEmptyColumnAndAgeAtReceiving` (FR-009, US3.3, US3.5, US3.6)
- `Download_ReturnsCsvNamedAfterCurrentDate` (FR-010, US3.1)
- `DownloadWithoutQualifyingCase_ReturnsEmptyFile` (FR-010, US3.8)
- `Download_WritesLinesInAscendingPatientNumberOrder` (FR-010, US2.7)
- `Download_SavesNothingAndLeavesNoTemporaryFile` (FR-012, US6.4, US6.5)

No test was added for the defect rows (location subset, boundary day, age `0`, abort cases, silent drop on an error
answer, missing setting, unescaped values); they were reproduced in the uncommitted scratch assembly only.

## Spec corrections applied

- US1.2: was "as for an address that does not exist; nothing hints that the address is valid"; now names the two
  different 404 pages, tagged `[SUSPECTED-DEFECT]` (SEC-015).
- US6.3, FR-011, selection summary, roles table: was "unreachable, time-out or unexpected answer aborts the run"; now a
  network failure, time-out or non-JSON answer aborts, while a JSON answer without places (e.g. a quota message) leaves
  the case out silently without log entry, tagged `[SUSPECTED-DEFECT]` (DATA-041).
- US2.4, US3.4, EC2, EC7, Assumptions: "derived from code; not exercised" markers replaced by what was reproduced with
  stubbed data (not on the running system).
- EC9 (architecture): the test-coverage statement lists the tests added in this verification and the parts left
  untested on purpose.

## Backlog updates

- NEW SEC-015: refused token distinguishable from an unknown address (observed on system).
- NEW DATA-041: Geonames JSON error answer drops EpiScanGIS cases silently (unit level).
- DATA-020, DATA-005, SEC-003: reproduced at unit level 2026-11-08 (023 verification), with details.
- DEC-008: observation recorded, no decision taken.
- SEC-012: EpiScanGIS deletes its temp file after a successful run, leaves an empty one after an abort.
- REF-012: per-case lookups and open-ended window observed at unit level.
- SEC-007: not verifiable here (no web-server request log on the host).
- TEST-001: EpiScanGIS part covered by the added tests, defect parts untested on purpose.

## Not verifiable here

- US5.5 · EC4 (foreign or empty postal code): the outcome depends on the answer of the external Geonames service, which
  may not be called; a recorded service answer for such postal codes would make it testable with the stubbed HTTP
  function.
- SEC-007 (token in web-server logs): the host writes no IIS Express request log; needs the production server's log
  configuration.
- Everything of an actual export run on the system (real data, real coordinates, real file download): needs the valid
  token, which the verification does not use by rule. Rows on the export logic were moved to the unit-test path.
- The log entries of US6.2 ("$Failed to call Geonames …") and US6.3 ("Unhandled error: …"): the test project does not
  capture NLog output.

## Open questions for the owner

- SEC-015: should the refusal look exactly like an unknown address (same 404 page), or is a 404 status enough?
- DATA-041: should an error answer of Geonames (quota, account) abort the run or be logged, so that the receiver does
  not get an incomplete file that looks normal? The same imprecision exists in spec 022 ("Service unavailable or
  failing": a JSON answer without list yields an empty result, not a server error); 022 was not edited here and its
  verification should check it.
- DEC-008 candidates stay as they are (location subset, boundary day); no decision taken.
