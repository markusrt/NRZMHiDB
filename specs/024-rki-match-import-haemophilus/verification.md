# Verification: RKI match import (Haemophilus)

**Spec**: 024 · **Date**: 2026-11-08 · **Build**: `a9aa988` · **Host**: ScreenshotTests
SpecVerificationHost (`http://localhost:9899`, catalog `HaemophilusWeb_Screenshots`) ·
**Accounts used**: `Standard1`, `Rki1`, `Admin1`, anonymous · **Scope**: full

The import makes no outbound call: the controller writes the upload to a temporary file, the importer reads it with
the CSV library and updates the database (`RkiMatchController.Index`, `RkiMatchImporter.ImportMatchesToContext`). All
match files were synthetic, built in the layout of `RkiMatchImporterTests`, and referenced only own submissions of
sender #001 with "Labnr. Einsender" `SV-024-…`, sampled and received in 2012 (a year without other records; checked
with SQL) or received on 31.12.2008 for the 2009 rule, with stem numbers 92401–92412. Uploaded files and the
database snapshots after each upload (`db-after-u*.txt`) are kept in `verification/`.

The host reached its four-hour limit during the verification and was restarted by the main session at 13:49, which
reseeded the catalog. Uploads u01–u14 and the role, export and merge checks ran before the restart (submissions
`SV-024-A` … `SV-024-K`); the quoted-value and unquoted-semicolon files (u15, u16) and the file with an empty status,
the pre-2009 stem number with a newer laboratory number, and one reference for two submissions (u17) ran afterwards
against re-created submissions `SV-024-Q1`, `SV-024-R1`, `SV-024-D2`, `SV-024-S1`.

## Summary

| Verdict | Count |
| --- | --- |
| Match | 27 |
| Spec deviation | 8 |
| Defect confirmed | 4 |
| New defect | 0 |
| Not verifiable | 2 |

By primary path: Browser 14 · File 4 · DB 17 · Unit test (existing) 1 · Unit test (added) 1 · Unit test (scratch) 0 ·
Code 2 · Not verifiable 2 (41 rows). Also test-asserted: 14 rows with a browser, file or DB verdict. Test run:
`vstest.console.exe … /TestCaseFilter:"(FullyQualifiedName~HaemophilusWeb.Tools.RkiMatchImporterTests|FullyQualifiedName~HaemophilusWeb.Controllers.RkiMatchControllerTests|FullyQualifiedName~HaemophilusWeb.Tools.HaemophilusSendingLaboratoryExportTests)&TestCategory!=Integration"`
→ 36 passed, 0 failed; full suite (`TestCategory!=Integration`) → 1207 passed, 0 failed.

Edge cases are numbered as the list stood before this verification: EC1 "Row for a submission without isolate", EC2
"Stem number that exists only as a Meningococci isolate", EC3 "Row with `klhi_nr` of a submission received before
2009 …", EC4 "Same `InterneRef` …", EC5 "Status word with surrounding spaces", EC6 "Header spelled differently", EC7
"Semicolon inside the file number", EC8 "Global separator setting", EC9 "Privacy", EC10 "Architecture".

## Results

| ID | Spec says | App/test shows | Verdict | Path | Evidence |
| --- | --- | --- | --- | --- | --- |
| FR-001 · US1.1 · US1.2 · US1.3 · SC-001 | Page "RKI Matching Daten" by address only, one file field "CSV Datei", one button "Upload", no import list, not linked; refused without sign-in | `/RkiMatch` (HTTP 200): heading "RKI Matching Daten", label "CSV Datei", file field and "Upload" only; no link to `/RkiMatch` in the navigation of `Standard1`, `Admin1` or `Rki1`; anonymous GET and POST → 302 to `/Account/Login?ReturnUrl=%2FRkiMatch` | Match | Browser | `verification/FR-001-page-standard1.png`, `FR-001-page-Admin1.png`, `FR-001-page-Rki1.png` |
| US1.1 (page title) | Normal page frame (title not stated) | The page sets no title: browser tab "Haemophilus Web -" with nothing after the dash | Spec deviation | Browser | `verification/FR-001-page-standard1.png`; UX-006 extended |
| FR-001 · US1.4 · SEC-004 · SEC-005 | Every signed-in role may upload; no anti-forgery check | `Rki1` and `Admin1` open the page (HTTP 200); `Rki1` uploaded a header-only file → "File Uploaded Successfully!!"; the form has no token field and every upload was processed | Defect confirmed | Browser | `verification/US1-4-upload-Rki1.png` |
| FR-010 · US1.5 · UX-006 | Same page with "File Uploaded Successfully!!" under the button, file field empty, also when no row matched | u01 (13 rows): line under "Upload", URL `/RkiMatch`, file field empty; the same line for u05, u12 (header only) and u13 (unknown headers, nothing stored) | Match | Browser | `verification/US1-5-upload-u01-success.png`, `US2-7-header-only.png`, `US2-7-other-headers.png` |
| US1.6 | Zero-length file: nothing imported, success line | u11 (0 bytes) → "File Uploaded Successfully!!", records unchanged, no temporary file written | Match | Browser | `verification/US1-6-empty-file.png`; also test-asserted: `Controllers/RkiMatchControllerTests.Index_FileOfZeroLength_ShowsSuccessLineOnSamePage` |
| US1.7 · DATA-021 | No file chosen → "File upload failed: Object reference not set to an instance of an object." | The browser posts an empty part (`filename=""`); page shows "File upload failed: Der Objektverweis wurde nicht auf eine Objektinstanz festgelegt." (framework message in the server's language) | Defect confirmed | Browser | `verification/US1-7-no-file.png`; spec text corrected (language) |
| US1.8 | Failure line with the technical message "in English" | Status helper and CSV library messages are English ("'xyz' is not a valid value for 'RkiStatus'; …", "Input string was not in a correct format."), framework messages German (US1.7, US6.4) | Spec deviation | Browser | `verification/FR-009-unknown-status-matched.png`, `FR-003-nonnumeric.png`; also test-asserted: `Controllers/RkiMatchControllerTests.Index_FailingUpload_ShowsFailureLineWithExceptionMessage` |
| US1.9 · SEC-012 | Name/extension not checked; temporary file left behind | `.csv` files accepted; every one of the 18 non-empty uploads left a `tmp*.tmp` copy (same size as the upload) in the temp directory of the account running IIS Express; the zero-length upload and the oversized request left none | Defect confirmed | File | `tmp537E.tmp` (540 bytes, u01) … `tmp2845.tmp` (178 bytes, u17), listed in the temp directory |
| FR-002 · US2.1 | UTF-8, semicolons, header line; quoted values accepted (derived) | "möglich" read correctly; u15 `"SV-024-AZ;J2"` stored as `SV-024-AZ;J2` for `SV-024-Q1` | Match | DB | `verification/db-after-u17.txt`, `US2-1-quoted.png` |
| EC7 | An unquoted semicolon shifts the following columns | u16 `…;SV-024-AZ;H2;definitiv;92408;;` → "File upload failed: Input string was not in a correct format." (the status word lands in `klhi_nr`); nothing stored | Spec deviation | Browser | `verification/EC-unquoted-semicolon.png` |
| New EC "File from a Windows program" | Not stated | u08 with CR LF updated `SV-024-B`; u09 with UTF-8 byte order mark created the record of `SV-024-K` | Spec deviation | DB | `verification/db-after-u08.txt`, `db-after-u09.txt`; also test-asserted: `Tools/RkiMatchImporterTests.ImportMatches_FileWithoutByteOrderMarkAndWithCrLf_IsRead` (the other tests write a byte order mark) |
| FR-002 · US2.2 · EC6 | Six columns by name; case of header names left open | u10 with `INTERNEREF;…;KLHI_NR;…` updated `SV-024-B` to "definitiv"; u13 with other names stored nothing | Spec deviation | DB | `verification/db-after-u10.txt`, `EC-header-case.png`; also test-asserted: `Tools/RkiMatchImporterTests.ImportMatches_HeaderInUpperCase_IsRecognized` |
| FR-003 · US2.3 · US2.4 | Whole numbers, negatives accepted; non-numeric cell fails the import | Stem numbers `-1` and `-92499` read without error; u06 with `12x` → "File upload failed: Input string was not in a correct format.", the valid first row (`SV-024-B`) not stored | Match | Browser | `verification/FR-003-nonnumeric.png`, `db-after-u06.txt`; also test-asserted: `Tools/RkiMatchImporterTests.ImportMatches_NonNumericStemNumber_ThrowsFormatException` |
| FR-004 · US2.5 | German words case-insensitive, English names, 0–3, prefixes, empty = "kein Match"; "the test file covers … the empty cell" | "Probable" → 2 (`SV-024-H`), "3" → 3 (`SV-024-I`), "mög" → 1 (`SV-024-J`), empty → 0 (`SV-024-R1`, u17); the empty-status row of the test table has no stem number and is skipped, so no test covered it | Spec deviation | DB | `verification/db-after-u01.txt`, `db-after-u17.txt`; also test-asserted: `Tools/RkiMatchImporterTests.ImportMatches_EmptyStatusInAppliedRow_StoresNone`, `Utils/EnumSerializerTests.DeserializeEnumStrict_ValidDisplayString_ReturnsCorrectValue` |
| FR-004 · US2.6 · EC5 | Any unknown status word fails the whole import | u04: unknown word in a matched row → failure line, the valid row for `SV-024-G` not stored; u05: unknown word in a row matching nothing → success, `SV-024-G` stored "definitiv"; u07 " möglich" in a matched row → failure | Spec deviation | Browser | `verification/FR-009-unknown-status-matched.png`, `US2-6-unknown-status-unmatched.png`, `EC-leading-space.png`; also test-asserted: `Tools/RkiMatchImporterTests.ImportMatches_UnknownStatusInAppliedRow_ThrowsWithStatusMessage`, `…_UnknownStatusInRowWithoutSubmission_IsIgnored` |
| US2.7 | No data rows or no recognized header → nothing stored, success | u12 and u13 → success line, snapshot unchanged | Match | Browser | `verification/US2-7-header-only.png`, `US2-7-other-headers.png`, `db-after-u13.txt` |
| FR-005 · US3.2 · US3.3 | Lookup by stem number, then by `LaufendeNummer`/`Jahr` | `SV-024-A` found by 92401; `SV-024-C` (no stem number) found by `-1;34;2026` | Match | DB | `verification/db-after-u01.txt`; also test-asserted: `Tools/RkiMatchImporterTests.ImportMatches_MatchViaYearlySequentialIsolateNumber_MatchesCorrespondingEntry` |
| US3.1 · DATA-021 | Row without `klhi_nr` skipped even with laboratory number | `;35;2026` row (laboratory number of `SV-024-E`) created no record | Defect confirmed | DB | `verification/db-after-u01.txt` |
| FR-006 · US3.4 · SC-004 · EC3 | Submissions received before 2009 untouched; stem path first | `SV-024-D` (31.12.2008) got no record; u17 row with stem of `SV-024-D2` (31.12.2008) and the laboratory number 1/2026 of `SV-024-Q1` was skipped, `SV-024-Q1` kept its values | Match | DB | `verification/db-after-u01.txt`, `db-after-u17.txt`; also test-asserted: `Tools/RkiMatchImporterTests.ImportMatches_SendingIsBefore2009_DoesNotMatch` |
| FR-006 · US3.5 | Row without `InterneRef` skipped | `;SV-024-AZ-E1;möglich;92405` created no record for `SV-024-E` | Match | DB | `verification/db-after-u01.txt` |
| US3.6 | Ambiguous lookup fails the import | Not producible through the UI (stem numbers are unique); a test with two submissions of stem number 2515 gets `InvalidOperationException` | Match | Unit test (added) | `Tools/RkiMatchImporterTests.ImportMatches_StemNumberFoundTwice_ThrowsInvalidOperationException` |
| US3.7 · REF-013 | Soft-deleted submission found and updated | `SV-024-F`, soft-deleted before u01, received "wahrscheinlich" / 90240601 | Match | DB | `verification/db-after-u01.txt` |
| FR-008 · US3.8 · US4.3 · SC-003 | Rows in file order; lower rank ignored | u01 "definitiv" then "wahrscheinlich" for 92401 → "definitiv"/90240101 kept; u03 "kein Match" for 92401 → unchanged | Match | DB | `verification/db-after-u04.txt`; also test-asserted: `Tools/RkiMatchImporterTests.ImportMatches_SendingIsFound_MatchesCorrespondingEntry`, `…_ExistingDefiniteRecordAndLowerRow_LeavesRecordUnchanged` |
| FR-007 · FR-008 · US4.1 | New record written whatever the state, also "kein Match" | `SV-024-B`: record 90240201 / `SV-024-AZ-B1` / 0 | Match | DB | `verification/db-after-u01.txt` |
| FR-008 · US4.2 | Equal rank replaces reference, file number and state | u03 "möglich" for `SV-024-C`: 90240301/`…-C1` → 90240302/`…-C2` | Match | DB | `verification/db-after-u04.txt`; also test-asserted: `Tools/RkiMatchImporterTests.ImportMatches_ExistingRkiMatch_DatabaseSendingIdIsLeftIntact` |
| US4.4 · REF-013 | State only raised; no page shows or edits the record | Lowering ignored (row above); submission edit page, isolate page and list of `SV-024-A` contain neither reference nor file number | Match | DB | `verification/db-after-u04.txt` |
| US4.5 · EC4 | One record per submission; same reference allowed | u01 created 7 records for 7 submissions; u17 stored 90249999 for both `SV-024-R1` and `SV-024-S1` | Match | DB | `verification/db-after-u01.txt`, `db-after-u17.txt` |
| US4.6 · US4.7 | Saved together; table keyed by submission, reference int, file number text, state int | `RkiMatchRecords`: `SendingId` int (key), `RkiReferenceId` int, `RkiReferenceNumber` nvarchar, `RkiStatus` int; no other column | Match | DB | `sys.columns` query |
| FR-009 · US6.1 · SC-005 | All or nothing | u04, u06 and u07 failed and kept none of their valid rows (`SV-024-G`, `SV-024-B` unchanged); u16 stored nothing | Match | Browser | `verification/db-after-u04.txt`, `db-after-u06.txt`, `db-after-u07.txt`; also test-asserted: `Tools/RkiMatchImporterTests.ImportMatches_RunsInsideOneTransaction` |
| FR-012 · US6.2 · US6.3 · SC-007 | Neither failures nor imports are logged or recorded | `HaemophilusWeb/Logs/2026-11-08.txt` stayed at 21 lines through all uploads; the record table has no date or account column | Match | File | application log line count before and after |
| US6.4 | Request above the default size → generic error page, not the failure line | 5.7 MB file → HTTP 500 "Maximale Anforderungslänge überschritten."; not logged | Match | Browser | `verification/US6-4-request-too-large.png` |
| US6.5 | Second upload of the same file changes nothing, success again | u01 uploaded twice: snapshots identical, success line | Match | DB | `verification/db-after-u01.txt` = `db-after-u02.txt`, `US6-5-second-upload.png` |
| FR-011 · US5.1 · US5.2 · SC-006 | Last three columns after "Bemerkung (Isolat)"; reference "as number" | Laboratory export 2012: "RKI InterneRef", "RKI Aktenzeichen", "RKI Status" last; values and German words as stored; the reference is a text cell (`t="str"`) holding digits | Spec deviation | File | `verification/US5-laboratory-export-2012.xlsx`; also test-asserted: `Tools/HaemophilusSendingLaboratoryExportTests.DataTable_ContainsRkiMatchRecord` |
| US5.3 | Without record: two empty cells and "kein Match" | `SV-024-E` (H92405): empty, empty, "kein Match" | Match | File | `verification/US5-laboratory-export-2012.xlsx`; also test-asserted: `Tools/HaemophilusSendingLaboratoryExportTests.DataTable_WithoutRkiMatchRecord_ShowsEmptyReferenceAndKeinMatch` |
| US5.4 | No other page or export shows the record | Submission and isolate pages and list without the values; Haemophilus RKI export 2012 contains none of them; only the laboratory export reads `RkiMatchRecord` | Match | Browser | `verification/US5-4-rki-export-2012.xlsx` |
| US5.5 | Patient merge keeps the record with its submission | Merge of the patients of `SV-024-H` into `SV-024-I`: submission moved to the kept patient, record 90240801 / 2 unchanged | Match | DB | merge "Erfolgreich", SQL before and after |
| EC1 · EC2 | Submission without isolate never matched; Meningococci stem numbers not found | EC1: the UI cannot create a submission without isolate. EC2: the seeded Meningococci stem numbers 1–3 also exist as Haemophilus stem numbers, so a row cannot tell the two apart; a Meningococci isolate with an unused stem number would make it verifiable | Not verifiable | Not verifiable | — |
| EC8 | Process-wide separator switched to ";" and left so | `RkiMatchImporter.ImportMatchesToContext` sets `CsvConfig.ItemSeperatorString` and `CsvSerializer.UseEncoding`; no other use of `CsvConfig`/`CsvSerializer` in `HaemophilusWeb` | Match | Code | `HaemophilusWeb/Tools/RkiMatchImporter.cs` |
| EC10 | Importer creates its own access; stale comment | `RkiMatchController` field `new ApplicationDbContextWrapper(new ApplicationDbContext())`; class comment names `klhi_nr` as `Sending.SendingId` | Match | Code | `HaemophilusWeb/Controllers/RkiMatchController.cs`, `HaemophilusWeb/Tools/RkiMatchImporter.cs` |
| Assumptions (real RKI file) | Layout known from importer and test table only | No real RKI file available; synthetic files in that layout behave as described, conformance of real files unknown | Not verifiable | Not verifiable | — |
| SC-002 | Ten-row test table yields the five listed records | Existing tests assert the values for 2515, 2466, 2488, 2494 and the laboratory number 010/17 | Match | Unit test (existing) | `Tools/RkiMatchImporterTests.ImportMatches_SendingIsFound_MatchesCorrespondingEntry`, `…_MatchViaYearlySequentialIsolateNumber_MatchesCorrespondingEntry` |

## Unit tests added

12 test cases (12 methods):

- `HaemophilusWebTests/Tools/RkiMatchImporterTests.cs`: `ImportMatches_UnknownStatusInAppliedRow_ThrowsWithStatusMessage`
  (FR-004, US2.6), `ImportMatches_UnknownStatusInRowWithoutSubmission_IsIgnored` (FR-004, US2.6),
  `ImportMatches_EmptyStatusInAppliedRow_StoresNone` (US2.5), `ImportMatches_NonNumericStemNumber_ThrowsFormatException`
  (FR-003, US2.4), `ImportMatches_HeaderInUpperCase_IsRecognized` (FR-002, EC6),
  `ImportMatches_FileWithoutByteOrderMarkAndWithCrLf_IsRead` (FR-002, US2.1),
  `ImportMatches_ExistingDefiniteRecordAndLowerRow_LeavesRecordUnchanged` (FR-008, SC-003),
  `ImportMatches_StemNumberFoundTwice_ThrowsInvalidOperationException` (US3.6), `ImportMatches_RunsInsideOneTransaction`
  (FR-009)
- `HaemophilusWebTests/Controllers/RkiMatchControllerTests.cs` (new file, added to `HaemophilusWebTests.csproj`):
  `Index_FileOfZeroLength_ShowsSuccessLineOnSamePage` (FR-010, US1.6),
  `Index_FailingUpload_ShowsFailureLineWithExceptionMessage` (FR-010, US1.8)
- `HaemophilusWebTests/Tools/HaemophilusSendingLaboratoryExportTests.cs`:
  `DataTable_WithoutRkiMatchRecord_ShowsEmptyReferenceAndKeinMatch` (FR-011, US5.3)

No test was written for the upload without a file (DATA-021), lab-number-only rows (DATA-021), the temporary file
(SEC-012) or soft-deleted submissions (REF-013).

## Spec corrections applied

- Scope note and Assumptions: the spec now says it was verified with synthetic files (was: no running system).
- US1.1: the page sets no title; the tab reads "Haemophilus Web -" (was: not stated).
- US1.7: the object-reference message follows the server's language, on the host "Der Objektverweis wurde nicht auf
  eine Objektinstanz festgelegt." (was: English text, derived).
- US1.8: library and helper messages English, framework messages in the server's language (was: English).
- US1.9: one `tmp*.tmp` per non-empty upload observed; zero-length files are not written (was: derived).
- US2.1 and new EC "File from a Windows program": byte order mark, CR LF and quoted values accepted (was: quoted values
  derived; byte order mark and CR LF not stated).
- US2.2 and EC6: header names recognized regardless of case (was: open).
- US2.4: message "Input string was not in a correct format.", reading fails before any row is applied (was: derived).
- US2.5: the empty-status row of the test table is skipped, so the test table does not cover the empty cell (was:
  covered).
- US2.6, FR-004, matching summary "State": the status word is interpreted only for applied rows; an unknown word in a
  skipped row does not stop the import (was: any unknown word fails the import).
- US5.2: the reference is written as digits in a text cell (was: as number).
- EC7: an unquoted semicolon that pushes text into a numeric column fails the import; quoting allowed (was: shifts
  the columns, not exercised).
- Coverage notes of US3.6, US3.7, US5.3, US5.5, US6.4, EC3, EC5 and EC10 now say what was observed or is asserted by
  the added tests.

## Backlog updates

- SEC-004: confirmed on system 2026-11-08 (`Rki1`/`Admin1` open `/RkiMatch`, `Rki1` upload processed).
- SEC-005: confirmed on system 2026-11-08 for the RKI match import (no token in the form).
- SEC-012: confirmed on system 2026-11-08 for the RKI match upload (18 leftover `tmp*.tmp` files).
- DATA-021: confirmed on system 2026-11-08 (lab-number-only row skipped; German object-reference message).
- UX-006: observed on system 2026-11-08 (success for zero matches, failures not logged, mixed-language messages, no
  page title).
- REF-013: observed on system 2026-11-08 (lenient status forms, soft-deleted submission matched, no lowering).
- TEST-001: RKI match import covered by tests added in the 024 verification (2026-11-08), with the parts left untested
  on purpose.

## Not verifiable here

- EC1, EC2: see the results table; a Meningococci isolate with a stem number unused by Haemophilus would make EC2
  verifiable.
- Conformance with a real RKI match file: no such file is available, and real RKI data may not be used; a
  pseudonymized sample file from the RKI would make the column layout verifiable.

## Open questions for the owner

- The upload accepts a file in which a row is skipped silently for many reasons (no stem number, unknown stem number,
  pre-2009 submission, no `InterneRef`, lower rank) and reports success without counts (UX-006); whether an import
  summary is wanted is a change-spec question, not decided here.
- A status word that the RKI might send with surrounding spaces or in a non-UTF-8 file fails the import or is
  misread; whether the real files are UTF-8 without padding is unknown (needs a real sample).
- Seed extension proposal: the records of this verification were lost with the host restart; if later specs need RKI
  match records (e.g. for the laboratory export), a seeded submission with an RKI match record would make them
  independent of the upload.
