# Verification: EUCAST clinical breakpoints management

**Spec**: 017 · **Date**: 2026-11-08 · **Build**: `6605431` · **Host**: ScreenshotTests
SpecVerificationHost (`http://localhost:9899`, catalog `HaemophilusWeb_Screenshots`, `host.ready` written 13:49:26 and
unchanged at the end) · **Accounts used**: `Standard1`, `Admin1`, `Rki1` · **Scope**: full

## Summary

| Verdict | Count |
| --- | --- |
| Match | 17 |
| Spec deviation | 3 |
| Defect confirmed | 10 |
| New defect | 0 |
| Not verifiable | 0 |

By primary path: Browser 24 · File 0 · DB 3 · Unit test (existing) 1 · Unit test (added) 1 · Unit test (scratch) 0 ·
Code 1 · Not verifiable 0 (30 rows). Also test-asserted: 5 rows with a browser verdict. Test run: `vstest.console.exe …
/TestCaseFilter:"(…Validators.EucastClinicalBreakpointValidatorTests|…Models.EucastClinicalBreakpointTests|…ViewModels.EpsilometerTestReportModelTests|…IsolateViewModelMappingActionTests|…MeningoSendingLaboratoryExportTests|…HaemophilusPubMlstExportTests)&TestCategory!=Integration"`
→ 144 passed, 0 failed; full suite (`TestCategory!=Integration`) → 1261 passed, 0 failed.

Scenarios are `US<story>.<n>`; edge cases are numbered as the list stood before this verification (EC1 "Validity is
'valid from' only" … EC14 "The breakpoint pages read and write the database directly"); assumptions likewise (A1
"Current application code …" … A8 "The values of the stored breakpoint records"). FR-017 is a scope statement and has
no row; EC3 (CLSI values for Azithromycin, omitted from Meningococci reports) belongs to 009 FR-006 and was verified
there.

No external service was contacted. The Meningococci report page posts `/Report/QueryHealthOffice/` on load; on the
first five visits a page route left in the shared browser page by the 016 verification answered it in the browser with
synthetic data (found by a probe request), afterwards the page routes were cleared and the route block aborted it
(`POST http://localhost:9899/Report/QueryHealthOffice/`, twice). No Geonames request was made. Own records: sender
`SV-017-Sender` with one Meningococci submission (`SV-017-L1`, sampled 04.11.2026); breakpoints `SV-017-Ceftriaxon Men`
(Meningococci: v`SV-017-1` 01.03.2025 0,125/0,125, v`SV-017-2` 01.06.2025 0,25/0,5 — edited and deleted —, and an
"ohne Grenzwerte" record), `SV-017-Allgemein` (Levofloxacin, "Allgemein"), `SV-017-Inverted` (Chloramphenicol, 1/0,25),
`SV-017-Crafted` (Moxifloxacin, stored by a request without "Gültig für"); a Haemophilus copy of `SV-017-Ceftriaxon Men`
was created for the duplicate check and deleted. Seeded records were only read; seeded Haemophilus isolate 1 was
opened, not saved.

## Results

| ID | Spec says | App/test shows | Verdict | Path | Evidence |
| --- | --- | --- | --- | --- | --- |
| FR-001 · US1.1 | "Antibiotika Grenzwerte" in Administration for Standard User and Administrator, not Public Health | `Standard1` and `Admin1`: "Administration" → "Antibiotika Grenzwerte" (`/EucastClinicalBreakpoints`); `Rki1`: no "Administration" menu, no entry | Match | Browser | navigation read per account |
| FR-002 · US1.6 · EC12 · SC-007 · DEC-001 | Only Standard User admitted; Administrator-only sees the entry but is refused | `Admin1` and `Rki1`: list, create, edit, details, delete and the create post → HTTP 302 to `/Account/Login?ReturnUrl=…` | Defect confirmed | Browser | `verification/FR-002-admin1-refused.png` |
| FR-003 · US1.2 · US1.3 · US1.4 · US1.5 · A6 | Columns, "Bearbeiten", create button, 8 per page, search, version descending as text, no page size, state saved; German antibiotic names, internal pathogen ids, date with time; no details/delete links | Columns "Antibiotikum", "Antibiotikum Details", "Version", "Gültig für", "Gültig ab", "Sensibel <=", "Resistent >"; "Neuen Grenzwert anlegen"; "1 bis 8 von 11 Einträgen"; version column `sorting_desc` ("SV-017-1" before "14.0" before "10.0" before empty); search field, no length menu; search "SV-017" and a changed order restored after reload (`DataTables_eucastClinicalBreakpoints_/EucastClinicalBreakpoints`); "Penicillin", "Meningococci", "None", "01.01.2024 00:00:00"; no `/Details/` or `/Delete/` in the page | Match | Browser | `verification/FR-003-list.png`, `verification/FR-003-search.png` |
| FR-004 · US2.1 · US3.1 | Fields, choices, placeholders, buttons; edit page filled, no delete button | Labels as listed; 183 antibiotics in German alphabetical order, first preselected, no empty entry; placeholders "Meropenem (meningitis)", "1.0", "0,5", "0,5"; radio "Haemophilus", "Meningokokken", "Allgemein" (none preselected); "Neu anlegen", "Zurück zur Liste"; "Grenzwert bearbeiten" filled with the stored values, "Änderungen speichern", "Zurück zur Liste", no delete | Match | Browser | `verification/US2-1-create.png`, `verification/US3-1-edit.png` |
| FR-005 · US2.3 · US2.4 · US2.5 · SC-001 | Empty details, partial four values, too long details/version: form again with a message, nothing stored | Empty details and length limits are refused in the browser before sending ("'Antibiotikum Details' darf nicht leer sein.", "Das Feld "Antibiotikum Details" muss ein Zeichenfolgen- oder Arraytyp mit einer maximalen Länge von 128 sein.", same for "Version" with 64) and by the server for crafted requests; the all-or-none rule only after the round trip, its message shown four times (below each field); nothing stored (SQL) | Spec deviation | Browser | `verification/US2-3-empty-details.png`, `verification/US2-4-partial.png`, `verification/US2-5-too-long.png`; also test-asserted: `Validators/EucastClinicalBreakpointValidatorTests`, `Models/EucastClinicalBreakpointTests.Validate_DetailsAndVersionTooLong_FailsOnBothMembers` |
| A3 ("Gültig für" unselected) | Assumed stored as "Haemophilus" | Browser refuses: "'Gültig für' darf kein Nullwert sein."; a crafted request without the field is stored with `ValidFor` 0 ("Haemophilus") | Spec deviation | Browser | crafted request; SQL of `EucastClinicalBreakpoints` |
| FR-006 · EC5 · DATA-013 (thresholds) | No check of threshold relation; inverted thresholds classify everything above resistant and nothing intermediate | "SV-017-Inverted" ("Sensibel <= 1", "Resistent > 0,25") stored; on the isolate form 0,5, 1 and 2 → "resistent", 0,25 and 0,125 → "sensibel", nothing "intermediär" | Defect confirmed | Browser | `verification/DATA-013-inverted-0-5-resistant.png` |
| FR-007 · US2.6 · SC-004 · EC2 · A4 | All four empty: "ohne Grenzwerte" label, "not determined"; listed after dated records | Record stored; offered as "SV-017-Ceftriaxon Men - ohne Grenzwerte" after both dated records; 0,125 against it → "n.d." | Match | Browser | `verification/FR-007-ohne-grenzwerte-nd.png`; also test-asserted: `Models/EucastClinicalBreakpointTests.Title_WithoutVersionDateAndThresholds_IsOhneGrenzwerte`, `…NoEucastAvailable_AnyOfTheFourValuesSet_IsFalse` (4 cases) |
| FR-008 · US2.7 | Label `<details> - v<version> vom <dd. MMM. yy>`; resistant above R, susceptible at or below S, else intermediate | "SV-017-Ceftriaxon Men - vSV-017-2 vom 01. Jun. 25", "… vSV-017-1 vom 01. Mrz. 25"; against 0,25/0,5: 0,25 "sensibel", 0,5 "intermediär", 1 "resistent"; against 0,125/0,125: 0,125 "sensibel", 1 "resistent" | Match | Browser | browser values; also test-asserted: `Models/EucastClinicalBreakpointTests.Title_WithBreakpoints_ShowsDetailsVersionAndValidFromDate` |
| FR-009 · US2.9 · EC6 · DATA-013 (duplicates) | Duplicate combination rejected only by the database, generic error page | Duplicate dated record and duplicate "ohne Grenzwerte" record on create, and a duplicate on edit → HTTP 500 "Cannot insert duplicate key row … 'IX_EucastClinicalBreakpointVersionAntibioticDetails'", input lost; the same combination for Haemophilus is stored | Defect confirmed | Browser | `verification/US2-9-duplicate.png`, `verification/US2-9-duplicate-ohne.png`, `verification/US3-2-edit-duplicate.png`, `verification/US2-9-other-pathogen.png` |
| FR-010 · US3.2 · US3.3 | Edit overwrites in place, no warning, no previous values | Thresholds, "Gültig ab", antibiotic and "Gültig für" of a record in use edited one after the other, each time back on the list without a warning; the row shows only the new values | Match | Browser | `verification/US3-4-report-after-edit.png` |
| FR-011 · US2.8 · SC-002 · SC-003 · EC1 | Only antibiotics with a record of exactly that pathogen; standard rows only with a record; newest first, no end; newest preselected | Meningococci free row offers Ceftriaxon and Chloramphenicol, not Levofloxacin ("Allgemein") or Moxifloxacin; Haemophilus free row offers Moxifloxacin, not Ceftriaxon; Ceftriaxon records listed 01.06.2025, 01.03.2025, "ohne Grenzwerte"; choosing 0,25 preselects the 2025-06 record although the sampling date is 04.11.2026 | Match | Browser | browser values; also test-asserted: `Automapper/IsolateViewModelMappingActionTests.EpsilometerTestsModelToViewModel_BuildsFreeSavedAndStandardRows` |
| EC4 · DEC-003 | "Allgemein" storable, never offered | `SV-017-Allgemein` stored, listed as "None", Levofloxacin offered on neither form | Defect confirmed | Browser | `verification/FR-003-search.png` |
| FR-012 · US3.4 · EC8 · SC-005 · DATA-012 · DATA-035 | Report takes thresholds and year from the current record; stored result not recalculated | Before edit: Ceftriaxon 0,25 "sensibel", 0,25/0,5, 2025; after editing to 0,064/0,125, 01.01.2026: "sensibel", 0,064/0,125, 2026 (`documentData.ETests`); the isolate form shows "sensibel" next to "… vom 01. Jan. 26", saving it unchanged stores "sensibel" again (SQL) | Defect confirmed | Browser | `verification/US3-4-report-before-edit.png`, `verification/US3-4-report-after-edit.png`, `verification/US3-4-isolate-form-after-edit.png` |
| FR-012 (not determined) · SC-004 | "---" for thresholds and year when the result is "not determined" | Measurement 0,5 against the "ohne Grenzwerte" record: report data "n.d.", "---", "---", "---" | Match | Browser | `verification/FR-012-report-ohne-grenzwerte.png`; also test-asserted: `ViewModels/EpsilometerTestReportModelTests.CreateFromViewModel_NotDetermined_ShowsDashesForThresholdsAndYear` |
| US3.5 | Edited antibiotic: measurements appear under the new antibiotic | After changing the record to Cefuroxim, the isolate row label and the report data show "Cefuroxim" with 0,25 "sensibel" | Match | Browser | `verification/US3-5-isolate-form-antibiotic-edited.png` |
| US3.6 · A5 | Edited "Gültig für": record no longer offered; assumed no selection and save blocked | After changing the record to "Haemophilus", the Meningococci row "Cefuroxim" shows "Breakpoint auswählen..." only; saving returns the form with "'Breakpoint' darf nicht leer sein" | Match | Browser | `verification/US3-6-isolate-form-validfor-edited.png`, `verification/US3-6-save-blocked.png` |
| US3.7 · US4.1 (identifiers) | Missing identifier "bad request", unknown "not found" | Edit, Details, Delete without id → HTTP 400; with 4711 → HTTP 404 (IIS page, as `/Sender/Details/4711`) | Match | Browser | crafted requests |
| FR-013 · Effects table (antibiotic edited) | Exports attribute measurements to the record's antibiotic; thresholds not exported; exports list edited measurements under the new antibiotic | Exports take antibiotic, MIC and S/I/R from the record's antibiotic into fixed columns; a measurement of an antibiotic without a column (Amikacin in the PubMLST export) is not exported; no threshold column | Spec deviation | Unit test (existing) | `Tools/HaemophilusPubMlstExportTests.DataTable_ContainsAntibioticMeasurements` (4 cases), `…DataTable_ContainsNoAntibiotics`, `Tools/MeningoSendingLaboratoryExportTests.DataTable_ContainsMicValues`, `…DataTable_ColumnHeadersAreInDocumentedOrder` |
| FR-014 · US4.1 · US4.2 · US4.4 | Details and delete pages by address only, content, "Delete"; confirming a missing record: generic error page | Details: "Details", "EucastClinicalBreakpoint", German labels, "Ceftriaxone", no "Gültig für", "Edit", "Back to List"; Delete: "Are you sure you want to delete this?", same data, "Delete", no usage count; confirmation → list; confirming again → HTTP 500 "Der Wert darf nicht NULL sein." (framework message) | Match | Browser | `verification/US4-1-details.png`, `verification/US4-2-delete.png` |
| FR-014 · US4.3 · EC9 · SC-006 · DATA-011 | Deletion removes all measurements that refer to the record, no audit or undo | Deleting `SV-017-Ceftriaxon Men` v`SV-017-2` removed E-Test rows 1 (orphaned) and 3 (isolate `SV-017-L1`); rows 2 and 4 (seeded Penicillin breakpoint) stayed; foreign key `…EpsilometerTests_dbo.EucastClinicalBreakpoints…` is `CASCADE`; report data lost the line | Defect confirmed | DB | SQL of `EpsilometerTests` before and after; `verification/US4-3-delete-used-breakpoint.png` |
| DATA-033 (interplay) | Isolate saves orphan E-Test rows (004/005) | Re-saving `SV-017-L1` unchanged left two rows without isolate; they keep their breakpoint and are removed with it | Defect confirmed | DB | SQL of `EpsilometerTests` |
| EC10 · UX-009 | Details and delete pages untranslated; internal pathogen ids in the list | Headings "Details", "Delete", "EucastClinicalBreakpoint", links "Edit", "Back to List", "Ceftriaxone"; labels German; list shows "Haemophilus", "Meningococci", "None" | Defect confirmed | Browser | `verification/US4-1-details.png`, `verification/US4-2-delete.png` |
| FR-015 · EC13 | Create, edit, delete verify the anti-forgery token | POST without token → HTTP 500 "The required anti-forgery form field "__RequestVerificationToken" is not present." for all three | Match | Browser | crafted requests |
| FR-016 | One list for both pathogens, separated by "Gültig für" | One table with Haemophilus, Meningococci and "None" records | Match | Browser | `verification/FR-003-list.png` |
| EC7 · REF-006 · REF-016 | Reference data maintained without review, history or audit | Edits and the deletion of a record in use left no application log line (`Logs/2026-11-08.txt`; only failures as "Unhandled error"); the catalog has no audit or history table | Defect confirmed | DB | `sys.tables`; application log |
| EC11 | Decimal comma | "0,125" entered, stored 0.125, shown "0,125" | Match | Browser | SQL; `verification/FR-003-list.png` |
| US2.10 | Leaving a changed create or edit page asks for confirmation | After typing in "Antibiotikum Details", "Zurück zur Liste" raised a `beforeunload` dialog on both pages; dismissing kept the page | Match | Browser | dialog events |
| EC14 · REF-002 | Antibiotic derivation copied twice, interpretation script duplicated | `IsolateControllerBase.AvailableAntibiotics` and `IsolateViewModelMappingActionBase.AvailableAntibiotics` ("TODO Refactor copied code from IsolateControllerBase"); `ETestChange` in `Views/Isolate/Edit.cshtml` and `Views/MeningoIsolate/Edit.cshtml`; `EucastClinicalBreakpointsController` creates `new ApplicationDbContext()` | Defect confirmed | Code | files named |
| EC14 · TEST-001 (validator) | No tests for the breakpoint validator | Validator, record label and report values now covered; the pages stay untested | Match | Unit test (added) | see "Unit tests added" |

## Unit tests added

18 test cases in 3 new files (8 methods own, plus the 3 inherited cases of `AbstractValidatorTests`):

- `HaemophilusWebTests/Validators/EucastClinicalBreakpointValidatorTests.cs`: `Validate_AllFourValuesEmpty_IsValid`
  (FR-005, FR-007), `Validate_OneOfTheFourValuesMissing_FailsAllFourWithAllOrNoneMessage` (4 cases, FR-005, US2.4),
  inherited `Validate_WithAValidModel_IsValid` and `Validate_WithAMaximalInvalidModel_ReturnsAllInvalidFields` (2 cases:
  empty details; partial values without details).
- `HaemophilusWebTests/Models/EucastClinicalBreakpointTests.cs`: `Title_WithBreakpoints_ShowsDetailsVersionAndValidFromDate`
  (FR-008), `Title_WithoutVersionDateAndThresholds_IsOhneGrenzwerte` (FR-007), `NoEucastAvailable_AnyOfTheFourValuesSet_IsFalse`
  (4 cases, FR-007), `Validate_DetailsAndVersionAtTheirLimits_IsValid` and `Validate_DetailsAndVersionTooLong_FailsOnBothMembers`
  (FR-005, US2.5).
- `HaemophilusWebTests/ViewModels/EpsilometerTestReportModelTests.cs`: `CreateFromViewModel_WithBreakpoints_CarriesThresholdsYearAndStoredResult`
  and `CreateFromViewModel_NotDetermined_ShowsDashesForThresholdsAndYear` (FR-012, SC-004).

No test was written for DATA-012 (thresholds taken from the current record), DATA-013 (no threshold check, duplicates)
or DATA-011 (cascade).

## Spec corrections applied

- US2.3–US2.5, FR-005: which checks run in the browser and which on the server, with the observed messages; the
  all-or-none message appears below each of the four fields (was: "a validation message").
- FR-004, A3: the browser requires a "Gültig für" choice; storage as "Haemophilus" applies only to a request that
  bypasses the browser (was: assumed for an unselected form).
- FR-013, "Effects of Changes on Existing Data": exports have fixed antibiotic columns; a measurement of an antibiotic
  without a column is not exported (was: exports list them under the new antibiotic).
- EC8, EC9, Effects table: stale result kept on re-save (DATA-035) and deletion of orphaned rows (DATA-033) added.
- Assumptions A1, A2, A4, A5, A6 and the coverage note of EC14 updated to the observed behavior and the added tests.

## Backlog updates

- DATA-011, DATA-012, DATA-013: confirmed on system 2026-11-08 (017 verification).
- DATA-033: interplay with DATA-011 observed (orphaned rows are deleted with their breakpoint).
- DEC-001, DEC-003: observations recorded, no decision.
- UX-009: confirmed for the breakpoint pages.
- REF-002: duplicated antibiotic derivation and interpretation script named.
- REF-006, REF-016: confirmed for breakpoint edits and deletion.
- TEST-001: breakpoint validator covered by tests added in the 017 verification (2026-11-08).

## Not verifiable here

- None.

## Open questions for the owner

- DATA-012 and DATA-035 meet here: when the server re-derives the result on save (decision of 2026-11-07), saving an
  old isolate after a breakpoint edit would change its stored result. Should the change spec for DATA-035 keep stored
  results of unchanged rows, or should a breakpoint edit be blocked or versioned first (DATA-012, REF-006)?
- Seed extension proposal: none needed; the seeded breakpoints sufficed, own records were created through the UI.
