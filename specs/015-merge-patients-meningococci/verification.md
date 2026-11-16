# Verification: Merge patients (Meningococci)

**Spec**: 015 · **Date**: 2026-11-08 · **Build**: `13c8e5e` · **Host**: ScreenshotTests
SpecVerificationHost (`http://localhost:9899`, catalog `HaemophilusWeb_Screenshots`, `host.ready` written
2026-11-08 13:49:26, unchanged during the run) · **Accounts used**: `Standard1`, `Admin1`, `Rki1` · **Scope**: full

## Summary

| Verdict | Count |
| --- | --- |
| Match | 20 |
| Spec deviation | 1 |
| Defect confirmed | 8 |
| New defect | 0 |
| Not verifiable | 0 |

By primary path: Browser 20 · File 1 · DB 5 · Unit test (existing) 1 · Unit test (added) 0 · Unit test (scratch) 1 ·
Code 1 · Not verifiable 0 (adds up to 29 rows). Also test-asserted: 6 rows with a browser, file or DB verdict. Test
run: `vstest.console.exe … /TestCaseFilter:"(FullyQualifiedName~HaemophilusWeb.Controllers.MeningoPatientSendingControllerTests.MergePatient|FullyQualifiedName~HaemophilusWeb.Utils.ReportFormatterTests.ToLaboratoryNumberWithPrefix|FullyQualifiedName~EpiscanGisExportControllerTests.DownloadWithTwoOfSamePatient)&TestCategory!=Integration"`
→ 15 passed, 0 failed; full suite (`TestCategory!=Integration`) → 1227 passed, 0 failed.

Edge cases are numbered as the list stood before this verification (EC1 = first open shows the message, EC2 =
soft-deleted submissions, EC3 = missing deletion page, EC4 = Patient 1 number in Patient 2's slot, EC5 = no
anti-forgery check, any method, EC6 = no audit trail or undo, EC7 = cancel/back targets, EC8 = patient number
visibility, EC9 = cross-pathogen numbers, EC10 = limited summary, EC11 = stale confirmation, EC12 = sampling before
birth, EC13 = derived age, EC14 = EpiScanGIS grouping, EC15 = export duplicate rules, EC16 = parallel implementation).

Test data (all created through Meningococci intake by `Standard1`, sender 1, receiving date 08.11.2026): patients
"R.A." (male, born 01.01.2015) to "R.I." with the sender laboratory numbers `SV-015-A1`, `SV-015-A2` (second
submission via "Bestehenden Patienten verwenden"; city, "Meningitis" and "Asplenie" then set on the edit page),
`SV-015-B1` ("R.B.", female, born 03.03.1990, sampled 01.06.2010, "Sepsis", "Anderer" risk factor `SV-015-RisikoB`,
stem number 99015 entered on the isolate form), `SV-015-C1`, `SV-015-C2` (added while the confirmation page was open),
`SV-015-D1`, `SV-015-E1`, `SV-015-F1`, `SV-015-G1`, `SV-015-G2` (soft-deleted), `SV-015-H1`, `SV-015-I1`
(soft-deleted). The two soft deletes were made with a delete request carrying the anti-forgery token, because the
Meningococci pages offer no working delete (EC3, DEC-007). Merges run: B into A (keep Patient 1), C into D (keep
Patient 2), F into E (single GET), G into H (failed). Patient numbers of this host run: A 5, B 6, C 7, D 8, E 9, F 10,
G 11, H 12, I 13. No seeded record was changed; the seeded Meningococci patients 1 and 3 were only entered on the
request page (patient 1 ends in HTTP 500 before the confirmation, patient 3 is "Unbekannter Patient").

## Results

| ID | Spec says | App/test shows | Verdict | Path | Evidence |
| --- | --- | --- | --- | --- | --- |
| FR-001 · US1.1 | Administration → "Meningokokken" offers "Patienten Zusammenfügen" (only entry, no "Gelöschte Einsendungen") to Standard User and Administrator, not to Public Health | `Standard1` and `Admin1`: below the header "Meningokokken" only "Patienten Zusammenfügen" (`/MeningoPatientSending/MergePatient`); `Rki1`: no merge entry. Page title "Patienten Zusammenfügen (Meningokokken)", heading "Patientennummern", fields "Patienten-Nr. 1"/"Patienten-Nr. 2", button "Zusammenfügen vorbereiten" | Match | Browser | `verification/EC1-first-open.png` |
| FR-002 · SC-006 | Only Standard User; Administrator-only and Public Health refused | `Admin1` and `Rki1`: `/MeningoPatientSending/MergePatient`, also with both numbers and `Confirmation=true`, → HTTP 302 to `/Account/Login?ReturnUrl=…`; patients 5 and 8 still exist afterwards. Administrator refusal recorded under DEC-001 (observation only) | Match | Browser | `verification/FR-002-Admin1-refused.png`, `FR-002-Rki1-refused.png` |
| FR-003 · US2.1 | Equal numbers: form again, "Bitte geben Sie zum Zusammenfügen zwei verschiedenen Patientennummern ein" at "Patienten-Nr. 1" | 5/5: request form, message at "Patienten-Nr. 1" only | Match | Browser | `verification/US2-1-equal.png`; also test-asserted: `Controllers/MeningoPatientSendingControllerTests.MergePatient_InvalidRequest_ReturnsRequestViewWithoutSaving`, `Validators/MergePatientRequestValidatorTests.EqualPatientNumbers_FailOnPatientOneId` |
| FR-003 (spec 014 US2.4) | Non-numeric entry rejected as in spec 014 | Browser blocks with "The Feld Patienten-Nr. 1 muss eine Zahl enthalten."; a POST with "abc" returns the form with "Patienten-Nr. 1 ist kein gültiger Wert für abc." (framework message) | Match | Browser | request form after the attempts |
| FR-004 · US2.2 · SC-003 | Only active Meningococci submissions count; no Haemophilus data; "Unbekannter Patient" otherwise | 5/999: message at "Patienten-Nr. 2"; 998/999: at both; 5/13 (patient I, only soft-deleted): at "Patienten-Nr. 2"; 3/5 (seeded patient 3, only soft-deleted): at "Patienten-Nr. 1"; 5/15 (15 is an active Haemophilus patient, no Meningococci patient): at "Patienten-Nr. 2"; no record changed | Match | Browser | `verification/US2-2-unknown2.png`, `FR-004-softdeleted-only.png`, `FR-004-haemophilus-number.png`; also test-asserted: `Controllers/MeningoPatientSendingControllerTests.MergePatient_PatientWithoutActiveMeningoSendings_IsReportedAsUnknownAndNothingChanges` |
| FR-005 · US1.2 | Summary and submission lines in the formats of spec 014, "MZ"/"NR" laboratory-number and "DE" stem-number prefixes | `Patienten-Nr.: 5,  Initialen: R.A., Geschlecht: männlich, Postleitzahl: 97001`; `Labornummer: MZ002/26, Labnr. Einsender: SV-015-B1, Eingangsdatum: 08.11.2026, Stammnummer: DE99015`; without stem number "DE -" (not stated in the spec). "NR" cannot be produced through intake | Spec deviation | Browser | `verification/US1-2-confirmation.png`; also test-asserted: `Controllers/MeningoPatientSendingControllerTests.MergePatient_WithoutConfirmation_ShowsSummariesAndActiveSendingsWithMeningococciPrefixesWithoutSaving`, `Utils/ReportFormatterTests.ToLaboratoryNumberWithPrefix_ValidInput_AddsPrefix("-01/2019",Meningococci,"NR01/2019")` |
| FR-003 · US1.3 | Shared confirmation layout: "Zusammenfügen zu" with "Patient 1" (preselected)/"Patient 2", red "Zusammenfügen und doppelten Patienten löschen", "Abbrechen" | As stated (`btn btn-danger`, "Patient 1" active); heading "Bestätigung"; nothing saved before the confirmation | Match | Browser | `verification/US1-2-confirmation.png` |
| FR-006 · US1.3 · SC-001 | Keep Patient 1: active submissions of Patient 2 move, Patient 2 deleted, one save | Merge B (6) into A (5): `SV-015-B1` now has patient 5, patient 6 is gone; A has 3 active submissions (2 + 1 shown) | Match | DB | before/after query of `MeningoPatients`, `MeningoSendings`, `MeningoIsolates`; `verification/US1-3-success.png`; also test-asserted: `Controllers/MeningoPatientSendingControllerTests.MergePatient_Confirmed_ReassignsActiveMeningoSendingsAndRemovesOtherPatient(PatientOne,42,43)` |
| FR-006 (keep Patient 2) · US1.3 | Keep Patient 2: submissions of Patient 1 move, Patient 1 deleted | Merge C (7) into D (8) with "Patient 2": `SV-015-C1` and `SV-015-C2` belong to 8, patient 7 is gone | Match | DB | `verification/US1-3-success-keep2.png`; also test-asserted: `…ReassignsActiveMeningoSendingsAndRemovesOtherPatient(PatientTwo,43,42)` |
| FR-007 · US1.4 · SC-002 | Kept patient unchanged (incl. clinical information, risk factors), nothing copied, submission and isolate data unchanged except the patient | Row diff before/after the B-into-A merge: only `MeningoSendings.MeningoPatientId` of `SV-015-B1` changed and patient 6 (with "Sepsis", `SV-015-RisikoB`, `SV-015-OrtB`) was removed; patient 5 and every isolate column (stem number 99015, report status) identical | Match | DB | before/after diff; also test-asserted: `…ReassignsActiveMeningoSendingsAndRemovesOtherPatient` |
| US1.5 · EC15 | Moved submission shows the kept patient's data and number, own data unchanged; no duplicate rules in Meningococci exports | Edit page of `SV-015-B1`: "R.A.", 01.01.2015, 97001, `SV-015-OrtA`, "Meningitis", "Asplenia" checked, own sampling date 01.06.2010 and sender laboratory number; isolate page "Patienten-Nr. 5", stem "DE99015"; list filtered by "Patienten-Nr." 5 shows MZ001/26, MZ002/26, MZ010/26 all "R.A."; Laboratory export 2010 (only `SV-015-B1` falls into 2010): one row with "Patienten-Nr." 5, MZ002/26, DE99015, "R.A.", 01.01.2015, "männlich", 97001, `SV-015-OrtA`, "Meningitis", "Asplenie" | Match | File | `verification/US1-5-labor-export-2010.xlsx`, `US1-5-isolate.png`, `US1-5-list.png` |
| FR-008 · EC2 · SC-005 | Soft-deleted submissions are not moved; the save fails as a whole (reachable only via requests outside the pages) | Merge G (11, `SV-015-G2` soft-deleted) into H (12, keep Patient 2): confirmation listed only `SV-015-G1`; confirm → HTTP 500 "The DELETE statement conflicted with the REFERENCE constraint "FK_dbo.MeningoIsolates_dbo.MeningoSendings_MeningoSendingId"…"; data unchanged; log line "Unhandled error: An error occurred while updating the entries." Every intake submission of this run has an isolate record (Assumption) | Defect confirmed | Browser | `verification/FR-008-confirmation.png`, `FR-008-error.png`; SQL after the failure; DATA-009 |
| EC3 · DEC-007 | "Eintrag löschen" leads to a deletion page that does not exist for Meningococci | `/MeningoPatientSending/Delete/15` (the button's target) → HTTP 500 "The view 'Delete' or its master was not found…"; a POST with token soft-deletes (HTTP 302 to the list) | Defect confirmed | Browser | response of the GET; SQL `Deleted = 1` for `SV-015-G2`, `SV-015-I1`; DEC-007 (observation only) |
| FR-009 · EC12 · EC13 | Sampling-before-birth rule not re-checked; next edit fails with "Das Entnahmedatum muss nach dem Geburtsdatum des Patienten liegen"; age derived from the kept patient | `SV-015-B1` (sampled 01.06.2010) moved to A (born 01.01.2015) without notice; saving its edit page unchanged → message at "Entnahmedatum", not saved; "Patientenalter bei Entnahme" "-4" on the isolate page and in the Laboratory export | Defect confirmed | Browser | `verification/FR-009-edit-fails.png`, `US1-5-isolate.png`, `US1-5-labor-export-2010.xlsx`; DATA-009 |
| FR-010 · US3.1 · US3.3 · EC7 · SC-004 | "Abbrechen" → Meningococci merge page; "Zurück zur Übersicht" → Meningococci list; cancelling changes nothing | "Abbrechen" (`btn btn-secondary`) → `/MeningoPatientSending/MergePatient`; "Zurück zur Übersicht" → `/MeningoPatientSending` ("Einsendungen"); the cancelled 5/6 confirmation changed no row (inside the before/after diff) | Match | Browser | `verification/US3-1-cancel.png`, `US1-3-success.png` |
| FR-003 · US3.2 | "Erfolgreich", both pre-merge summaries, "Alle Einsendungen wurden diesem Patienten zugeordnet." / "Der Patient wurde gelöscht." | As stated for both choices | Match | Browser | `verification/US1-3-success.png`, `US1-3-success-keep2.png` |
| FR-003 (spec 014 US3.4) | Resubmitting the confirmation reports the deleted patient as "Unbekannter Patient" | Replay of the identical B-into-A POST: HTTP 200, request form with "Unbekannter Patient" at "Patienten-Nr. 2"; no change | Match | Browser | replayed request |
| FR-011 · EC6 · SC-007 | No audit record, log entry, former-patient reference or undo | `HaemophilusWeb/Logs/2026-11-08.txt`: no line for the three successful merges (only the failed merge and the two HTTP 500 cases as "Unhandled error"); no audit or history table in the catalog; `MeningoSendings` has no former-patient column | Defect confirmed | DB | log file and `sys.tables`; DATA-009, REF-016 |
| EC1 | Menu open treats both numbers as `0` and shows the "two different numbers" message | Fields `0`/`0`, message at "Patienten-Nr. 1" on first open | Defect confirmed | Browser | `verification/EC1-first-open.png`; DATA-009 |
| EC4 | Confirmation data carries Patient 1's number in Patient 2's slot; masked by re-display | System: hidden `PatientTwoId` of the 5/6 confirmation = 6 (masked). Scratch run with `MeningoPatientSendingController`: request 42/43 → confirmation model `PatientTwoId` 42 | Defect confirmed | Unit test (scratch) | `verification/MergeConfirmationSlotReproMeningo.cs` (compiled with `csc` against `HaemophilusWebTests\bin\Debug`, exit 0; Geonames wired with a stub function); DATA-009 |
| EC5 | Any request method, token not checked; one request merges without confirmation | `GET /MeningoPatientSending/MergePatient?PatientOneId=9&PatientTwoId=10&MainPatient=PatientOne&Confirmation=true` without token → HTTP 200 "Erfolgreich"; `SV-015-F1` moved to 9, patient 10 deleted | Defect confirmed | Browser | `verification/FR-003-get-merge.png`; SEC-005 |
| EC8 | List: hidden patient-number column, "Patienten-Nr." extended search, full-text search; isolate page and Laboratory export show the number | List columns "Initialen", "Geburtsdatum", "Stammnummer", "Eingangsdatum", "Entnahmeort", "Invasiv", "Labor-Nr", "Befund"; search field "Patienten-Nr." filters to patient 5's three rows; isolate page "Patienten-Nr. 5"; export column "Patienten-Nr.". Full-text inclusion of the number from code (`QueryRecords`, `FullTextSearch`) | Match | Browser | `verification/US1-5-list.png`, `US1-5-isolate.png`, `US1-5-labor-export-2010.xlsx` |
| EC9 | Patient numbers per database; a Haemophilus number is "Unbekannter Patient" or addresses an unrelated Meningococci patient | Haemophilus patient 15 (active) entered on the Meningococci page: "Unbekannter Patient"; numbers 1–13 exist in both databases (e.g. 5 is Meningococci "R.A."), so they address the Meningococci record of that number | Match | Browser | `verification/FR-004-haemophilus-number.png` |
| EC10 | Summary shows only number, initials, gender, postal code; other attributes of the deleted record are discarded | Confirmation shows only those four values; birth date, residence, clinical information and risk factors of B were not shown and were removed with patient 6 | Match | Browser | `verification/US1-2-confirmation.png`; before/after diff |
| EC11 | Submissions are re-read at confirmation time | `SV-015-C2` added to C while the C/D confirmation was open (not listed) was moved to D with `SV-015-C1` | Match | DB | `MeningoSendings` query after the merge |
| EC14 | EpiScanGIS writes at most one line per Meningococci patient number | Not run on the system (token-protected, Geonames per case); the existing test asserts one line for two cases of one patient number | Match | Unit test (existing) | `Controllers/EpiscanGisExportControllerTests.DownloadWithTwoOfSamePatientMissingSerogroup_AddsPatientOnlyOnceAndChoosesTheOneWithMostData` |
| EC16 · REF-001 · REF-002 | Lookup and reassign-and-delete are copies of the Haemophilus ones; the three view pairs differ only in the title | `MeningoPatientSendingController.SendingsByPatient`/`MergePatients` differ from the Haemophilus pair only in `MeningoPatientId`; `git diff --no-index` of the three view pairs shows only the `ViewBag.Title` line | Match | Code | `Controllers/MeningoPatientSendingController.cs`, `Controllers/PatientSendingController.cs`, `Views/{PatientSending,MeningoPatientSending}/MergePatient*.cshtml` |
| Assumption "no identity check" (spec 014) · DATA-040 | The system does not verify that the two patients are the same person | B ("R.B.", female, born 1990, other postal code) was merged into A ("R.A.", male, born 2015) without notice | Match | Browser | `verification/US1-2-confirmation.png` |
| New EC "Patient without gender" · UX-018 | (not in the spec) | Prepare 1/5 (seeded Meningococci patient 1 has no gender; all four seeded Meningococci patients lack one) → HTTP 500 "Der Objektverweis wurde nicht auf eine Objektinstanz festgelegt." (framework message, `EnumUtils.cs` line 169); nothing changed | Defect confirmed | Browser | `verification/New-EC-gender-missing-500.png`; UX-018 |

## Unit tests added

- `HaemophilusWebTests/Controllers/MeningoPatientSendingControllerTests.cs`:
  `MergePatient_InvalidRequest_ReturnsRequestViewWithoutSaving` (1 case; FR-003, US2.1),
  `MergePatient_PatientWithoutActiveMeningoSendings_IsReportedAsUnknownAndNothingChanges` (5 cases, one with a
  Haemophilus-only number; FR-003, FR-004, US2.2), `MergePatient_WithoutConfirmation_ShowsSummariesAndActiveSendingsWithMeningococciPrefixesWithoutSaving`
  (1 case; FR-005, US1.2), `MergePatient_Confirmed_ReassignsActiveMeningoSendingsAndRemovesOtherPatient` (2 cases;
  FR-006, FR-007, US1.3, US1.4), with the helper `CreateMergeDb` on the fixture's own `IntakeDbStub`.
- 9 test cases in 4 methods. The confirmation test deliberately does not assert `PatientTwoId` (EC4); no test covers
  the soft-deleted failure, the missing audit, the sampling date not re-checked or the missing gender (defects).
- These tests assert rows whose primary path is the browser or the database, so the summary counts them under "also
  test-asserted", not under "Unit test (added)".

## Spec corrections applied

- US1.2: submission-line example and "DE -" for a missing stem number added (was: prefixes only).
- EC1, EC2, EC3, EC4, EC5, EC6, EC12: "not exercised" notes replaced by the observations (message on first open, HTTP
  500 with the `FK_dbo.MeningoIsolates_dbo.MeningoSendings_MeningoSendingId` conflict, HTTP 500 of "Eintrag löschen",
  masked number with unit-level reproduction, GET merge, no log line for successful merges, edit save rejected); EC12
  tagged `[SUSPECTED-DEFECT]` (DATA-009 already lists it).
- EC13: negative age ("-4") for a sampling date before the kept patient's birth date added.
- New edge case "Patient without gender" with `[SUSPECTED-DEFECT]` (UX-018).
- Assumptions: "no automated test covers the merge function" replaced by the tests added here; the soft-deleted failure
  and the missing Meningococci delete page now cite the observations.

## Backlog updates

- DATA-009: confirmed on system 2026-11-08 for the Meningococci merge (soft-deleted failure, first-open message, no
  log, sampling date not re-checked with negative age); number mix-up reproduced at unit level with the Meningococci
  controller.
- SEC-005: confirmed on system 2026-11-08 for the Meningococci merge (GET without token merges at once).
- REF-016: confirmed on system 2026-11-08 for the Meningococci merge.
- UX-018: source extended with "015 Edge Cases"; confirmed on system 2026-11-08 with a seeded Meningococci patient.
- DATA-010: note that the Meningococci pages are not affected.
- DEC-001, DEC-007: observations only (Administrator refused; "Eintrag löschen" HTTP 500, soft delete by request).
- DATA-040: the Meningococci merge also accepts different identities and discards clinical information and risk
  factors.
- REF-002: the three Meningococci/Haemophilus view pairs differ only in `ViewBag.Title`.
- TEST-001: Meningococci patient merge covered by the tests added here; defects untested on purpose.

## Not verifiable here

- None. The "NR" laboratory-number prefix cannot be produced through intake and is asserted by
  `Utils/ReportFormatterTests` (FR-005 row); the EpiScanGIS grouping (EC14) is asserted by an existing unit test,
  because the export is token-protected and calls Geonames per case.

## Open questions for the owner

- UX-018: all four seeded Meningococci patients lack a gender, so imported production records are presumably affected
  as well. Same question as in 014: should the merge summary show "keine Angabe", or should such records be completed
  first?
- FR-009 / DATA-009: a merge can leave a submission that cannot be saved any more and shows a negative age. Should a
  future change block such merges, warn, or leave it to the user? (Not a `DEC-*` candidate on its own; it belongs to
  the DATA-009 change spec.)
- External calls: none blocked (no report page was opened); 0 Geonames requests (postal codes were set without
  keystrokes).
