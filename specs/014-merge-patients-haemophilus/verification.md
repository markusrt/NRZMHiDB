# Verification: Merge patients (Haemophilus)

**Spec**: 014 · **Date**: 2026-11-08 · **Build**: `db63ae8` · **Host**: ScreenshotTests
SpecVerificationHost (`http://localhost:9899`, catalog `HaemophilusWeb_Screenshots`, `host.ready` written
2026-11-08 13:49:26, unchanged during the run) · **Accounts used**: `Standard1`, `Admin1`, `Rki1` · **Scope**: full

## Summary

| Verdict | Count |
| --- | --- |
| Match | 17 |
| Spec deviation | 2 |
| Defect confirmed | 6 |
| New defect | 1 |
| Not verifiable | 0 |

By primary path: Browser 18 · File 1 · DB 5 · Unit test (existing) 0 · Unit test (added) 0 · Unit test (scratch) 1 ·
Code 1 · Not verifiable 0 (adds up to 26 rows). Also test-asserted: 7 rows with a browser, file or DB verdict. Test
run: `vstest.console.exe … /TestCaseFilter:"(FullyQualifiedName~HaemophilusWeb.Validators.MergePatientRequestValidatorTests|FullyQualifiedName~HaemophilusWeb.Controllers.PatientSendingControllerTests.MergePatient)&TestCategory!=Integration"`
→ 11 passed, 0 failed; full suite (`TestCategory!=Integration`) → 1218 passed, 0 failed.

Edge cases are numbered as the list stood before this verification (EC1 = first open shows the message, EC2 = patient
numbers per database, EC3 = where the patient number is shown, EC4 = soft-deleted submissions, EC5 = limited summary,
EC6 = number mix-up in the confirmation data, EC7 = re-read at confirmation, EC8 = one address, any method, no token,
EC9 = no record of the merge, EC10 = Meningococci targets, EC11 = shared base and copies, EC12 = export grouping).

Test data (all created through intake by `Standard1`, sender 1): patients "Q.A." to "Q.I." with the sender
laboratory numbers `SV-014-A1`, `SV-014-A2` (second submission via "Bestehenden Patienten verwenden"), `SV-014-B1`
(stem number 99014 entered on the isolate form), `SV-014-C1`, `SV-014-C2` (added while the confirmation page was
open), `SV-014-D1`, `SV-014-E1`, `SV-014-F1`, `SV-014-G1`, `SV-014-G2` (soft-deleted), `SV-014-H1`, `SV-014-I1`
(soft-deleted). Merges run: B into A (keep Patient 1), C into D (keep Patient 2), F into E (single GET), G into H
(failed). Patient numbers of this host run: A 8, B 9, C 10, D 11, E 12, F 13, G 14, H 15, I 16. No seeded record was
changed; the seeded patients 1 and 4 were only shown on the confirmation page (patient 1 ends in HTTP 500 before it).

## Results

| ID | Spec says | App/test shows | Verdict | Path | Evidence |
| --- | --- | --- | --- | --- | --- |
| FR-001 · US1.1 | Administration → "Haemophilus" offers "Patienten Zusammenfügen" to Standard User and Administrator, not to Public Health | `Standard1` and `Admin1`: entry below the header "Haemophilus" (`/PatientSending/MergePatient`), second entry below "Meningokokken"; `Rki1`: no entry. Page title "Patienten Zusammenfügen (Haemophilus)", heading "Patientennummern", fields "Patienten-Nr. 1"/"Patienten-Nr. 2", button "Zusammenfügen vorbereiten" | Match | Browser | `verification/EC1-first-open.png` |
| FR-002 · SC-006 | Only Standard User; Administrator-only and Public Health refused | `Admin1` and `Rki1`: `/PatientSending/MergePatient`, also with all parameters, → HTTP 302 to `/Account/Login?ReturnUrl=…`; no merge. Administrator refusal is the DEC-001 observation, already recorded for this address by the 002 verification | Match | Browser | `verification/FR-002-admin-refused.png` |
| FR-003 | Two whole-number fields | Two text inputs with number validation (`data-val-number`), default `0` | Match | Browser | `verification/EC1-first-open.png` |
| FR-004 · US2.1 | Equal numbers rejected with "Bitte geben Sie zum Zusammenfügen zwei verschiedenen Patientennummern ein" at "Patienten-Nr. 1"; no lookup | 8/8: form again, message at "Patienten-Nr. 1" only, no confirmation | Match | Browser | `verification/FR-004-equal.png`; also test-asserted: `Validators/MergePatientRequestValidatorTests.EqualPatientNumbers_FailOnPatientOneId`, `Controllers/PatientSendingControllerTests.MergePatient_InvalidRequest_ReturnsRequestViewWithoutSaving` |
| FR-005 · US2.2 · US2.3 · SC-003 | Number without active submission → "Unbekannter Patient" at its field; both fields when both unknown; no confirmation | 8/999: message at "Patienten-Nr. 2"; 998/999: at both; 8/16 (patient I, only a soft-deleted submission): at "Patienten-Nr. 2"; no record changed | Match | Browser | `verification/FR-005-unknown2.png`, `FR-005-unknown-both.png`, `FR-005-softdeleted-only.png`; also test-asserted: `Controllers/PatientSendingControllerTests.MergePatient_PatientWithoutActiveSendings_IsReportedAsUnknownAndNothingChanges` |
| US2.4 | Non-numeric entry rejected by the input conversion, form shown again | The browser blocks the submit with "The Feld Patienten-Nr. 1 muss eine Zahl enthalten."; a POST bypassing it ("abc") returns the form with "Patienten-Nr. 1 ist kein gültiger Wert für abc." (framework message) | Spec deviation | Browser | `verification/US2-4-nonnumeric-client.png` |
| FR-006 · US1.2 · EC5 | Summary `Patienten-Nr.: <n>,  Initialen: …, Geschlecht: …, Postleitzahl: …`; one line per active submission `Labornummer: …, Labnr. Einsender: …, Eingangsdatum: …, Stammnummer: …` | Formats as stated; gender as display name ("männlich"), laboratory number with prefix ("KL007/26"), stem number with prefix ("H99014"), "H -" without stem number. Birth date, residence, clinical data and Hib vaccination are not shown (EC5) | Spec deviation | Browser | `verification/US1-2-confirmation.png`; also test-asserted: `Controllers/PatientSendingControllerTests.MergePatient_WithoutConfirmation_ShowsSummariesAndActiveSendingsWithoutSaving` |
| FR-007 · US1.3 | "Zusammenfügen zu" with "Patient 1" (preselected)/"Patient 2", red "Zusammenfügen und doppelten Patienten löschen", "Abbrechen" | As stated (`btn btn-danger`); nothing is saved before the confirmation | Match | Browser | `verification/US1-2-confirmation.png` |
| FR-008 · FR-009 · US1.4 · SC-001 | Keep Patient 1: active submissions of Patient 2 move, Patient 2 deleted, one save | Merge B (9) into A (8): `SV-014-B1` now has patient 8, patient 9 is gone; A has 3 active submissions (2 + 1 shown) | Match | DB | before/after query of `Patients`, `Sendings`, `Isolates`; `verification/US1-4-success.png`; also test-asserted: `Controllers/PatientSendingControllerTests.MergePatient_Confirmed_ReassignsActiveSendingsAndRemovesOtherPatient(PatientOne,42,43)` |
| US1.5 | Keep Patient 2: submissions of Patient 1 move, Patient 1 deleted | Merge C (10) into D (11) with "Patient 2": `SV-014-C1` and `SV-014-C2` belong to 11, patient 10 is gone | Match | DB | `verification/US1-5-success-keep2.png`; also test-asserted: `…ReassignsActiveSendingsAndRemovesOtherPatient(PatientTwo,43,42)` |
| FR-010 · US1.6 · SC-002 | Kept patient unchanged, nothing copied, submission and isolate data unchanged except the patient | Row diff before/after the B-into-A merge: only `Sendings.PatientId` of `SV-014-B1` changed and patient 9 was removed; patient 8 and every isolate column (stem number 99014, report status) identical | Match | DB | before/after diff; also test-asserted: `…ReassignsActiveSendingsAndRemovesOtherPatient` |
| US1.7 · EC12 | Moved submission shows the kept patient's data everywhere, own data unchanged | Edit page of `SV-014-B1`: "Q.A.", 01.02.1991, 99991; isolate page "Patienten-Nr. 8", stem 99014; list row "Q.A. · H99014 · KL007/26"; laboratory export 2026: KL007/26 with "Patienten-Nr." 8, "Q.A.", 01.02.1991 | Match | File | `verification/US1-7-labor-export-2026.xlsx`, `US1-7-list.png`; column also test-asserted: `Tools/HaemophilusSendingLaboratoryExportTests` ("Patienten-Nr.") |
| FR-011 · EC4 · SC-005 | Soft-deleted submissions of the patient to delete are not moved; the save fails as a whole, generic error page | Merge G (14, `SV-014-G2` soft-deleted) into H (15): HTTP 500, "The DELETE statement conflicted with the REFERENCE constraint "FK_dbo.Isolates_dbo.Sendings_SendingId"…"; data unchanged; log line "Unhandled error: An error occurred while updating the entries." | Defect confirmed | Browser | `verification/FR-011-confirmation.png`, `FR-011-error.png`; SQL after the failure; DATA-009 |
| FR-012 · US3.2 | "Erfolgreich", both pre-merge summaries, "Alle Einsendungen wurden diesem Patienten zugeordnet." / "Der Patient wurde gelöscht.", no submission list | As stated for both choices | Match | Browser | `verification/US1-4-success.png`, `US1-5-success-keep2.png` |
| FR-013 · US3.1 · US3.3 · EC10 · SC-004 | "Abbrechen" opens the Meningococci merge page, "Zurück zur Übersicht" the Meningococci list; cancelling changes nothing | "Abbrechen" → `/MeningoPatientSending/MergePatient` ("Patienten Zusammenfügen (Meningokokken)"); "Zurück zur Übersicht" → `/MeningoPatientSending`; the cancelled 4/9 confirmation changed no row (inside the before/after diff) | Defect confirmed | Browser | `verification/US3-1-cancel-meningo.png`; DATA-010 |
| FR-014 · EC9 · SC-007 | No audit record, log entry, former-patient reference or undo | `HaemophilusWeb/Logs/2026-11-08.txt` unchanged after two successful merges (last line "Application start" 13:49:19 until the failed merge); no audit/history table in the catalog; `Sendings` has no former-patient column | Defect confirmed | DB | log file and `sys.tables`; DATA-009, REF-016 |
| FR-015 · EC8 | Any request method, token not checked; one request merges without confirmation | `GET /PatientSending/MergePatient?PatientOneId=12&PatientTwoId=13&MainPatient=PatientOne&Confirmation=true` without token → "Erfolgreich"; `SV-014-F1` moved to 12, patient 13 deleted | Defect confirmed | Browser | `verification/FR-015-get-merge.png`; SEC-005 |
| US3.4 | Resubmitting the confirmation reports the deleted patient as "Unbekannter Patient", no change | Replay of the identical B-into-A POST: form "Patientennummern" with "Unbekannter Patient" at "Patienten-Nr. 2"; no change | Match | Browser | replayed request (HTTP 200) |
| EC1 | Menu open treats both numbers as `0` and shows the "two different numbers" message | Fields `0`/`0`, message at "Patienten-Nr. 1" on first open | Defect confirmed | Browser | `verification/EC1-first-open.png`; DATA-009 |
| EC2 | Patient numbers per database; a Meningococci number addresses a Haemophilus patient of the same number | 4/9: the confirmation shows the Haemophilus patient 4 ("S.Q."); the catalog's Meningococci numbers 1–4 all exist in Haemophilus, so the "unknown" branch was shown with numbers 998/999 | Match | Browser | confirmation text (4/9) |
| EC3 · UX-008 | Patient number read-only on the isolate page and in the laboratory export; no column or search in the list | Isolate page "Patienten-Nr. 9" (later 8); list columns "Initialen", "Geburtsdatum", "Stammnummer", "Eingangsdatum", "Entnahmeort", "Invasiv", "Labor-Nr", "Befund"; export column "Patienten-Nr." | Match | Browser | `verification/US1-7-list.png`, `US1-7-labor-export-2026.xlsx` |
| EC6 | Confirmation data carries Patient 1's number in Patient 2's slot; masked by re-display | System: hidden `PatientTwoId` of the confirmation form = submitted 9 (masked). Scratch run: request 42/43 → confirmation model `PatientTwoId` 42 | Defect confirmed | Unit test (scratch) | `verification/MergeConfirmationSlotRepro.cs` (compiled with `csc` against `HaemophilusWebTests\bin\Debug`, exit 0); DATA-009 |
| EC7 | Submissions are re-read at confirmation time | `SV-014-C2` added to C while the C/D confirmation was open (not listed) was moved to D with `SV-014-C1` | Match | DB | `Sendings` query after the merge |
| EC11 · REF-001 · REF-002 | Flow once on the shared base; lookup and reassign-and-delete as two copies; two near-identical view sets | `PatientSendingController.MergePatients`/`SendingsByPatient` and `MeningoPatientSendingController.MergePatients`/`SendingsByPatient` differ only in the patient key; the three Haemophilus and Meningococci views differ only in `ViewBag.Title` (`fc`) | Match | Code | `Controllers/PatientSendingController.cs`, `Controllers/MeningoPatientSendingController.cs`, `Views/*/MergePatient*.cshtml` |
| Assumption "no identity check" · DATA-040 | The system does not verify that the two patients are the same person | B ("Q.B.", female, other birth date and postal code) was merged into A ("Q.A.", male) without notice | Match | Browser | `verification/US1-2-confirmation.png` |
| New EC "Patient without gender" | (not in the spec) | Prepare 1/8 (seeded patient 1 has no gender) → HTTP 500 "Der Objektverweis wurde nicht auf eine Objektinstanz festgelegt." (framework message) in `EnumUtils.GetEnumDescription`; nothing changed | New defect | Browser | `verification/New-EC-gender-missing-500.png`; UX-018 |

## Unit tests added

- `HaemophilusWebTests/Validators/MergePatientRequestValidatorTests.cs` (new, added to `HaemophilusWebTests.csproj`):
  `EqualPatientNumbers_FailOnPatientOneId` (2 cases: 8/8, 0/0; FR-004, US2.1), `DifferentPatientNumbers_AreValid`
  (1 case; FR-004, FR-005).
- `HaemophilusWebTests/Controllers/PatientSendingControllerTests.cs`: `MergePatient_InvalidRequest_ReturnsRequestViewWithoutSaving`
  (1 case; FR-004), `MergePatient_PatientWithoutActiveSendings_IsReportedAsUnknownAndNothingChanges` (4 cases; FR-005,
  FR-008, US2.2, US2.3), `MergePatient_WithoutConfirmation_ShowsSummariesAndActiveSendingsWithoutSaving` (1 case;
  FR-006, FR-007, US1.2, US1.3), `MergePatient_Confirmed_ReassignsActiveSendingsAndRemovesOtherPatient` (2 cases;
  FR-008 to FR-010, FR-012, US1.4 to US1.6), with the helper `CreateMergeDb` on the fixture's own `IntakeDbStub`.
- 11 test cases in 6 methods. The confirmation test deliberately does not assert `PatientTwoId` (EC6), and no test
  covers the soft-deleted failure, the missing audit or the missing gender (defects).

## Spec corrections applied

- US1.2, FR-006: display format added (was: placeholders only; now: gender display name, "KL" laboratory-number and
  "H" stem-number prefixes, "H -" without stem number).
- US2.4: messages added (was: "rejected as an invalid number by the input conversion"; now: browser message "The Feld
  Patienten-Nr. 1 muss eine Zahl enthalten." and server message "Patienten-Nr. 1 ist kein gültiger Wert für abc.").
- EC1, EC4, EC6, EC9 and Assumptions: "not exercised"/"not verified" notes replaced by the observations (HTTP 500 with
  the reference-conflict message, masked number, no log line for successful merges); the Assumption "no automated
  test covers the merge function" replaced by the tests added here.
- New edge case "Patient without gender" with `[SUSPECTED-DEFECT]`.

## Backlog updates

- DATA-009: confirmed on system 2026-11-08 (soft-deleted failure, first-open message, no log or history); number
  mix-up masked on the system and reproduced at unit level.
- DATA-010: confirmed on system 2026-11-08.
- SEC-005: confirmed on system 2026-11-08 for the Haemophilus merge (GET without token merges at once).
- REF-016: confirmed on system 2026-11-08 for the Haemophilus merge.
- UX-008: confirmed on system 2026-11-08 (list columns named).
- REF-002: merge views differ only in the title; `MergePatients` copies differ only in the patient key.
- DATA-040: note that the merge accepts patients with different identity values without notice.
- TEST-001: Haemophilus patient merge covered by the tests added here; defects untested on purpose.
- NEW UX-018: patient merge fails with HTTP 500 when a patient has no gender.

## Not verifiable here

- None. The deletion of a soft-deleted submission without isolate record (EC4, second half) cannot be produced
  through the UI (intake always creates the isolate) and is left as a schema-derived statement inside the EC4 row.

## Open questions for the owner

- UX-018: patients without gender exist in the seed (Haemophilus patients 1 and 2) and, presumably, in imported
  production data. Should the merge summary show "keine Angabe" or should such records first be completed? (Not a
  `DEC-*` candidate unless the owner wants to decide how missing genders are handled in general.)
- External calls: none blocked (no report page was opened); 0 Geonames requests (postal codes were set without
  keystrokes).
