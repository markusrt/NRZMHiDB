# Verification: County reference data management

**Spec**: 025 · **Date**: 2026-11-08 · **Build**: `bfb97df` · **Host**: ScreenshotTests
SpecVerificationHost (`http://localhost:9899`, catalog `HaemophilusWeb_Screenshots`, `host.ready` written
08.11.2026 13:49:26, unchanged during the run) · **Accounts used**: `Standard1`, `Admin1`, `Rki1`, anonymous ·
**Scope**: full

## Summary

| Verdict | Count |
| --- | --- |
| Match | 16 |
| Spec deviation | 3 |
| Defect confirmed | 4 |
| New defect | 1 |
| Not verifiable | 1 |

By primary path: Browser 14 · File 9 · DB 0 · Unit test (existing) 0 · Unit test (added) 0 · Unit test (scratch) 0 ·
Code 1 · Not verifiable 1 (25 rows). Also test-asserted: 6 rows with a browser or file verdict. Test run:
`vstest.console.exe … /TestCaseFilter:"(FullyQualifiedName~HaemophilusWeb.Tools.CountyImporterTests|…Models.CountyTests|…Tools.HaemophilusSendingRkiExportTests|…Tools.MeningoSendingRkiExportTests|…Tools.MeningoStateAuthorityExportTests|…Controllers.AuthorizationMatrixTests)&TestCategory!=Integration"`
→ 104 passed, 0 failed; full suite (`TestCategory!=Integration`) → 1346 passed, 0 failed.

Edge cases are numbered as the list stood before this verification: EC1 "Normalization is symmetric and literal",
EC2 "Different counties can normalize to the same text", EC3 "A record with an empty name …", EC4 "Earliest validity
wins together with uploads only add", EC5 "No validation of the county number", EC6 "Treatment as reference data",
EC7 "Upload page served by the upload function", EC8 "Upload errors show technical text", EC9 "File format
assumptions", EC10 "Untranslated generated pages", EC11 "Validity date is valid since only", EC12 "Authorization",
EC13 "Layering and duplication".

### Test data

The host seeds no counties. All county records were created through the UI: a synthetic GV100AD-shaped upload file
(built in the scratchpad, cp1252, no real DeStatis data) with one state line, association and municipality lines and
the county lines "SV-025-Flensburg, Stadt" (99001, 20141031), "SV-025-Lübeck, Hansestadt" (99002, 20141231),
"SV-025-Hof, Stadt" (99464, 20140131) and "Landkreis SV-025-Hof" (99475, 20140131), uploaded twice; failing files
(short county line, validity "2015AB01"), a CSV file and a zero-length file; and hand-made records "SV-025-Frei"
(edited to "SV-025-Frei-Geaendert", number "1"), "SV-025-ServerIso", "SV-025-OhneNummer" (empty number), an
empty-name record "99999", and "SV-025-Client". All county records were deleted through the UI at the end; the county
list is empty again.

Export records live in 2018 (empty in both databases before the run, checked with SQL): Haemophilus submissions
"SV-025-1" … "SV-025-7" (sender #001, sampled 15.03.2018, "Blut", patients "Q.A." … "Q.G." with the county texts
"Landkreis SV-025-Hof", "sv-025-flensburg", "Kreisfreie Stadt SV-025-Lübeck" with state Berlin, none,
"SV-025-Lubeck", "SV-025-OhneNummer", "SV-025-Frei-Geaendert") and Meningococci submissions "SV-025-M1" ("Landkreis
SV-025-Hof") and "SV-025-M2" ("SV-025-Lubeck"), both with "typisches Wachstum" and oxidase "negativ". These
submissions remain in the catalog. No seeded record was changed. Haemophilus RKI export runs: run 1 (uploaded records
only), run 2 (plus the empty-name and the number-less record), run 3 (plus submissions 6 and 7: HTTP 500), run 4
(after deleting the empty-name and the number-less record).

## Results

| ID | Spec says | App/test shows | Verdict | Path | Evidence |
| --- | --- | --- | --- | --- | --- |
| FR-001 · US3.5 · EC12 · SC-006 | All county pages and the upload admit only the Standard User role; Administrator-only, Public Health and anonymous requests are redirected to sign-in. | `Standard1`: HTTP 200 on list, details, create, edit, delete and upload. `Admin1`, `Rki1` and anonymous: HTTP 302 to `/Account/Login?ReturnUrl=…` for `/County`, `/County/Upload`, `/County/Create`, `/County/Details/10`, `/County/Edit/10`, `/County/Delete/10` and for a POST of a file to `/County/Upload` (nothing stored). | Match | Browser | also test-asserted: `Controllers/AuthorizationMatrixTests.LaboratoryFunctionsAdmitStandardUserAndPublicHealthOnlyForTheRkiExports` (Standard User admitted, Public Health not) |
| FR-002 · US3.6 | No navigation entry or home-page tile leads to a county page. | The navigation of `Standard1`, `Admin1` and `Rki1` (all `href`s, drop-downs included) and the home page of `Standard1` contain no address with "County"; the only county link seen is "Create New" on the list page itself. | Match | Browser | — |
| FR-003 · US3.1 · US3.2 · EC10 | List "Index" with columns "CountyNumber", "Name", "ValidSince", links "Edit", "Details", "Delete", "Create New"; no paging, search or sorting; storage order; date with time. | Title "Index", headers "CountyNumber", "Name", "ValidSince"; per row "Edit \| Details \| Delete"; "Create New" → `/County/Create`; rows in ascending identifier order (uploads in file order, then hand-made records); "31.10.2014 00:00:00", "02.02.2016 13:45:00". | Match | Browser | `verification/025-US3-1-index-empty.png`, `025-FR-010-after-upload.png`, `025-FR-005-list-free-values.png` |
| FR-004 · US3.3 · US3.4 · EC10 | Details with headings "Details"/"County", three values, "Edit", "Back to List"; missing id → bad request, unknown id → not found. | Details page as stated ("CountyNumber abc Name SV-025-Frei ValidSince 01.01.2016 00:00:00"). Without id: HTTP 400 "HTTP Error 400.0 - Bad Request" on details, edit and delete; id 999999: HTTP 404 "HTTP Error 404.0 - Not Found" (web server page), while an unknown address shows HTTP 404 "Die Ressource kann nicht gefunden werden.". Spec text made precise (no verdict change). | Match | Browser | `verification/025-US3-3-details.png` |
| FR-005 · US4.1 · US4.2 · US4.3 · SC-005 · EC5 | Create offers three fields; only "ValidSince" is checked, "also before sending, in the browser"; number and name free. | Page "Create" with heading "County", fields, "Create", "Back to List". Number "abc", "1" and empty, empty name accepted (empty stored as NULL). Empty and non-date "ValidSince": the form **is sent** (one POST per click, `$.validator` loaded but no block); the server answers with "Eingabe erforderlich" and "ValidSince ist kein gültiger Wert für kein Datum." and stores nothing. An ISO date "2016-02-03" is accepted and stored as 03.02.2016. | Spec deviation | Browser | `verification/025-US4-1-create.png`, `025-FR-005-client-empty.png`, `025-FR-005-client-nondate.png` |
| FR-006 · US4.4 · US4.5 | Edit shows the stored values and "Save", overwrites in place; a deleted record ends on the generic error page. | Edit page pre-filled ("abc", "SV-025-Frei", "01.01.2016 00:00:00"), button "Save"; saving "1", "SV-025-Frei-Geaendert", "02.02.2016 13:45" overwrote the row. Saving the edit form of a record deleted in a second tab: HTTP 500 ("Store update, insert, or delete statement affected an unexpected number of rows (0). …"). | Match | Browser | `verification/025-US4-4-edit.png`, `025-FR-006-stale-edit.png` |
| FR-007 · US4.6 · US4.7 | Delete page asks "Are you sure you want to delete this?", button "Delete", "Back to List"; hard delete; confirming a vanished record ends on the generic error page. | As stated; the record disappeared from the list and from the table (SQL). A second, stale delete confirmation: HTTP 500 ("Der Wert darf nicht NULL sein. Parametername: entity", framework message). | Match | Browser | `verification/025-US4-6-delete.png`, `025-FR-007-stale-delete.png` |
| FR-008 · US4.9 · EC7 | Create, edit and delete verify the token; the upload verifies none and accepts any request method. | Create, edit and delete without token: HTTP 500 "The required anti-forgery form field "__RequestVerificationToken" is not present.". The upload form has no token field; GET, HEAD and POST reach the upload function. PUT, DELETE and PATCH on `/County/Upload` (and on `/County`) get the web server's "HTTP Error 404.0 - Not Found", so "any request method" is too broad. | Spec deviation | Browser | — |
| FR-009 · US2.1 · US2.7 | Upload page "GV100AD acs-Datei hochladen", "ACS-Datei:", "Hochladen", "Bitte geben sie eine Datei an." when opened, without a file and for an empty file. | Exactly as stated; title "Upload"; all three cases stay on `/County/Upload` with the message; nothing stored. | Match | Browser | `verification/025-FR-009-upload-get.png`, `025-FR-009-empty-file.png` |
| FR-010 · US2.2 · US2.3 · US2.4 · US2.5 · SC-001 · EC9 | Lines starting with "40" become records (name 23–72 trimmed, number 11–15, validity 3–10 at midnight), cp1252, other lines ignored, every upload adds. | Four records with the expected names (umlaut correct, `ü` = U+00FC in SQL), numbers and dates at 00:00:00; state, association and municipality lines ignored; the list was shown afterwards. The repeated upload added four more (8 in total). The CSV file imported nothing and showed the unchanged list. | Match | Browser | `verification/025-FR-010-after-upload.png`, `025-US2-5-repeat-upload.png`; also test-asserted: `Tools/CountyImporterTests.LoadCounties_ImportsOnlyCountyRecordLines`, `LoadCounties_ValidSinceHasNoTimeOfDay`, `LoadCounties_FileWithoutCountyLines_ReturnsNothing` (added), `LoadCounties_ReturnsCountiesWithCorrectNames`, `…CorrectValidSince`, `…CorrectNumber` (existing) |
| FR-011 · US2.6 · US2.8 · EC8 | All or nothing; failure shows "Fehler:" plus technical message; success shows the list without summary or log entry. | Short county line: "Fehler:Der Index und die Länge müssen sich auf eine Position in der Zeichenfolge beziehen. Parametername: length"; invalid validity: "Fehler:Die Zeichenfolge wurde nicht als gültiges DateTime erkannt."; in both cases the valid first county line was not stored (4 records before and after). Successful uploads showed the list without message; the application log has no entry for any upload. | Match | Browser | `verification/025-FR-011-short-line.png`, `025-FR-011-bad-date.png`; `HaemophilusWeb/Logs/2026-11-08.txt`; also test-asserted: `Tools/CountyImporterTests.LoadCounties_CountyLineShorterThanNameField_Throws`, `LoadCounties_ValidityNotEightDigits_ThrowsFormatException` (added) |
| FR-012 · US1.1 · US1.3 · US1.4 · US1.7 · SC-002 | The three exports derive county and state number from the matching record; empty without match or without text; the patient's "Bundesland" is not consulted. | Haemophilus RKI 2018 run 1: "sv-025-flensburg" → "99001"/"99"; "Kreisfreie Stadt SV-025-Lübeck" (state Berlin) → "99002"/"99" with `bundeslandName` "Berlin"; no text → empty, "n.n."; "SV-025-Lubeck" → empty. Meningococci RKI and LGA 2018: "SV-025-Lubeck" → empty. | Match | File | `verification/025-h-rki-2018-run1.xlsx`, `025-m-rki-2018.xlsx`, `025-m-lga-2018.xlsx`; also test-asserted: `Tools/HaemophilusSendingRkiExportTests.DataTable_ContainsRkiColumns`, `DataTable_UnknownCounty_LeavesCountyAndStateNumbersEmpty` (existing), `Tools/MeningoSendingRkiExportTests.DataTable_CountyAndStateNumberFromMatchingCounty` (added), `Tools/MeningoStateAuthorityExportTests.DataTable_ContainsSimpleProperties` (existing) |
| FR-013 · US1.2 · EC1 | Comma suffix dropped, five fill words removed literally, case ignored, nothing else folded. | Case and the comma suffix of the stored name ("sv-025-flensburg" = "SV-025-Flensburg, Stadt"), the fill word "Kreisfreie Stadt" and "Landkreis" matched; "SV-025-Lubeck" did not match "SV-025-Lübeck, Hansestadt" (umlaut not folded). | Match | File | `verification/025-h-rki-2018-run1.xlsx`; also test-asserted: `Models/CountyTests.IsEqualTo_CommaSuffixFillWordsAndCase_ReturnsTrue`, `IsEqualTo_OnlyCaseIsFolded_ReturnsFalse` (added), `IsEqualTo_NameVariations_ReturnsTrue/False` (existing) |
| FR-014 · US1.5 · SC-004 | Several matches: earliest "ValidSince"; tie: storage order; submission dates and "Bundesland" not consulted. | Tie of "SV-025-Hof, Stadt" (id 3) and "Landkreis SV-025-Hof" (id 4), both 31.01.2014: the first stored record ("99464") was exported in all three files; the patient's "Bayern" did not matter. The date-based choice with different dates was shown in the 011 verification (DATA-025 row). | Match | File | `verification/025-h-rki-2018-run1.xlsx`, `025-m-rki-2018.xlsx`, `025-m-lga-2018.xlsx` |
| FR-015 · US1.3 · SC-003 | The Meningococci exports log a warning per unmatched row with text ("exactly one" per row in SC-003); the Haemophilus export logs none. | Meningococci RKI and LGA run for "SV-025-Lubeck": "County SV-025-Lubeck not found for sending 22." **twice per export** (once per number column); the four Haemophilus runs wrote no warning for "SV-025-Lubeck" or "SV-025-OhneNummer". Same pattern as in the 011 verification. | Spec deviation | File | `HaemophilusWeb/Logs/2026-11-08.txt` (16:20:17, four lines) |
| FR-016 · US1.8 · US4.8 · EC11 | No end of validity, no reference to a county record; changes apply to the next export run. | Table `Counties` has only `CountyId`, `CountyNumber`, `Name`, `ValidSince` and no foreign key points to it (SQL). Creating the empty-name record changed run 2, deleting it restored run 4; edits and deletes left the patient county texts unchanged (SQL). | Match | File | `verification/025-h-rki-2018-run1.xlsx`, `025-h-rki-2018-run2.xlsx`, `025-h-rki-2018-run4.xlsx` |
| FR-017 | Documents current behavior only; no new validation, de-duplication or audit rules. | The app shows none beyond the date check (rows FR-005, FR-010, REF-017). | Match | Browser | — |
| US1.6 · New EC "A record without county number breaks the exports" | The state number is the first two characters; an empty county number yields an empty state number. | Number "1" → state "1" (run 4). A record created with an empty "CountyNumber" is stored as NULL; with patient "SV-025-OhneNummer" the Haemophilus RKI export (run 3) ended in HTTP 500 "Der Wert darf nicht NULL sein. Parametername: source" (`Enumerable.Take` in the state-number column); after deleting the record, run 4 delivered empty numbers. | New defect | Browser | `verification/025-NEW-null-number-export.png`, `025-h-rki-2018-run4.xlsx`; the text cases also test-asserted: `Tools/HaemophilusSendingRkiExportTests.DataTable_StateNumberIsFirstTwoCharactersOfCountyNumber` (added) |
| DATA-028 · EC2 | "X, Stadt" and "Landkreis X" normalize to the same text; a district patient can get the city's number. | "Landkreis SV-025-Hof" (Haemophilus and Meningococci patients) → "99464" (the city) / "99" in all three files, although "Landkreis SV-025-Hof" ("99475") exists. | Defect confirmed | File | `verification/025-h-rki-2018-run1.xlsx`, `025-m-rki-2018.xlsx`, `025-m-lga-2018.xlsx` |
| DATA-029 · EC3 | A record with empty name matches every patient without county text. | Record "99999" with empty name (NULL) → patient "Q.D." without county text got "99999"/"99" in run 2; empty again in run 4 after deletion. | Defect confirmed | File | `verification/025-h-rki-2018-run2.xlsx`, `025-h-rki-2018-run4.xlsx` |
| DATA-025 · EC4 | Uploads only add and the earliest validity wins, so a renumbered county keeps its old number. | The repeated upload added a second copy of every record (US2.5) and nothing replaced the older ones; the earliest-validity choice across different dates was confirmed on this host in the 011 verification today (2024 submissions got "09111" over "08222"). | Defect confirmed | File | `verification/025-US2-5-repeat-upload.png`; 011 `verification.md` (DATA-025 row) |
| REF-017 · EC5 · EC6 | County list maintained as unvalidated runtime data. | Numbers "abc", "1" and empty stored; records overwritten and hard-deleted without trace; failed uploads not logged; the lookup is copied into the exports and runs once per number column (FR-015). | Match | Browser | `verification/025-FR-005-list-free-values.png` |
| SEC-012 · EC8 · US2.8 | The uploaded file remains in the server's temporary folder. | Each non-empty upload left a `tmp*.tmp` file of the uploaded size in the temp directory of the account running IIS Express (62, 1412, 1412, 235, 402 bytes, containing "SV-025"), also after a failed import; the zero-length upload left none. | Defect confirmed | File | `%TEMP%\tmp*.tmp` listing (16:15:17–16:15:27) |
| TEST-001 · EC13 | The county pages and the upload have no automated tests. | Still true: `CountyController` creates its own `ApplicationDbContext` in a field initializer (`private readonly ApplicationDbContext db = new ApplicationDbContext();`), so no unit test can replace the store; importer, matching and derivation tests were extended instead. | Match | Code | `HaemophilusWeb/Controllers/CountyController.cs`, field `db` |
| Assumption "GV100AD layout" | The layout is assumed to hold for the quarterly editions the laboratory uploads. | Only synthetic files may be used; no real edition was uploaded. Verifiable with a current DeStatis edition outside this catalog. | Not verifiable | Not verifiable | — |

The upload makes no outbound call: `CountyController.Upload` saves the posted file with `Path.GetTempFileName()` and
`CountyImporter` reads only that local copy (`File.ReadAllLines`); the DeStatis address appears only in a code
comment. No request was blocked during the run; no Geonames request was sent (postal codes and places were set
without the lookup).

## Unit tests added

- `HaemophilusWebTests/Tools/CountyImporterTests.cs`: `LoadCounties_ImportsOnlyCountyRecordLines` (FR-010, US2.3),
  `LoadCounties_ValidSinceHasNoTimeOfDay` (FR-010, US2.2), `LoadCounties_CountyLineShorterThanNameField_Throws`
  (FR-011, US2.6), `LoadCounties_ValidityNotEightDigits_ThrowsFormatException` (FR-011, US2.6),
  `LoadCounties_FileWithoutCountyLines_ReturnsNothing` (FR-010, EC9) — 5 cases.
- `HaemophilusWebTests/Models/CountyTests.cs`: `IsEqualTo_CommaSuffixFillWordsAndCase_ReturnsTrue` (5 cases),
  `IsEqualTo_OnlyCaseIsFolded_ReturnsFalse` (4 cases) (FR-013, EC1).
- `HaemophilusWebTests/Tools/HaemophilusSendingRkiExportTests.cs`:
  `DataTable_StateNumberIsFirstTwoCharactersOfCountyNumber` (4 cases; US1.6, US1.7; the NULL number of DATA-043 is
  left out on purpose).
- `HaemophilusWebTests/Tools/MeningoSendingRkiExportTests.cs`: `DataTable_CountyAndStateNumberFromMatchingCounty`
  (3 cases; FR-012).

In total 21 test cases in 9 methods.

## Spec corrections applied

- US4.2/US4.3: was "validation message (also before sending, in the browser)"; now the form is sent, the server shows
  "Eingabe erforderlich" or "ValidSince ist kein gültiger Wert für kein Datum.", an ISO date is accepted, empty fields
  are stored as "no value".
- FR-008, US4.9, EC7: was "accepts any request method"; now GET, HEAD and POST, with PUT, DELETE and PATCH refused by
  the web server for every address.
- FR-015, US1.3, SC-003: was "a warning"/"exactly one warning per such row"; now twice per row and run (once per
  number column).
- US1.6: was "an empty county number yields an empty state number"; now only for empty text (upload), and a number-less
  hand-made record makes a matching export end in HTTP 500 [SUSPECTED-DEFECT] (DATA-043); new edge case "A record
  without county number breaks the exports".
- US3.4: names the observed web server pages for HTTP 400 and HTTP 404.
- EC2, EC3: "derived; not exercised" replaced by the observation with the backlog IDs.
- EC8: quotes the observed failure messages ("Fehler:" without a blank) and links SEC-012.
- EC13: coverage note updated for the tests added; the pages and the upload function remain untested and why.
- Assumptions: new bullet recording what the verification observed and what stays unverified.

## Backlog updates

- DATA-028: confirmed on system 2026-11-08 (025 verification).
- DATA-029: confirmed on system 2026-11-08 (025 verification).
- SEC-012: confirmed on system 2026-11-08 for the county upload.
- REF-017: extended with the observations and the warning written twice per row.
- TEST-001: source extended with 025; county pages and upload stay uncovered (no seam), the tests added are listed.
- NEW DATA-043: a county record without number breaks the exports (HTTP 500).

## Not verifiable here

- Assumption "GV100AD layout": only synthetic files may be uploaded; a real quarterly DeStatis edition would be
  needed to confirm the positions and the record type "40".

## Open questions for the owner

- DATA-043 and DATA-029 both arise from empty fields on the county create page. Should the fix require number and name
  there (a validation change, Principle I), or should the exports tolerate empty values? This decides whether the
  change spec touches the county pages, the exports, or both.
- Leftover data: the 2018 submissions "SV-025-1" … "SV-025-7" and "SV-025-M1", "SV-025-M2" remain until the next host
  start; no county record remains.
