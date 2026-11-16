# Verification: Meningococci reporting (preliminary + final)

**Spec**: 009 · **Date**: 2026-11-08 · **Build**: `b86e578` · **Host**: ScreenshotTests
SpecVerificationHost (`http://localhost:9899`, catalog `HaemophilusWeb_Screenshots`) ·
**Accounts used**: Standard1, Rki1, Admin1 · **Scope**: full

## Summary

| Verdict | Count |
| --- | --- |
| Match | 22 |
| Spec deviation | 3 |
| Defect confirmed | 6 |
| New defect | 0 |
| Not verifiable | 2 |

By primary path: Browser 21 · File 2 · DB 5 · Unit test (existing) 0 · Unit test (added) 2 · Code 1 ·
Not verifiable 2 (33 rows). Also test-asserted: 7 rows with a browser or DB verdict. Test run:
`vstest.console.exe … /TestCaseFilter:"FullyQualifiedName~HaemophilusWeb.Controllers.MeningoReportControllerTests|FullyQualifiedName~HaemophilusWeb.Controllers.ReportControllerTests"`
→ 34 passed, 0 failed; full suite → 1119 passed, 0 failed.

Edge cases are numbered EC1–EC11 as the list stood before this verification (EC1 = missing/unknown identifier, EC11 =
report routes check no role). Scenarios are `US<story>.<n>`.

Test data: Meningococci submissions `SV-009-A` (MZ010/26, isolate 14, DEMIS-ID `SV-009-DEMIS-A`), `SV-009-B` (MZ011/26,
isolate 15) and `SV-009-C` (MZ012/26, isolate 16), created through intake with sender 1, "Blut", "Vitaler Stamm" and
synthetic patients "V.A.", "V.B.", "V.C." (postal code 97080). Isolate 14: agglutination and serogroup PCR B,
Penicillin 0,064, Cefotaxim 0,008, Azithromycin 1 (breakpoint "ohne Grenzwerte"); no rule matches, so the report line is
"Diskrepante Ergebnisse, bitte Datenbankeinträge kontrollieren.". Isolate 15: values of rule StemInterpretation_20
(poly agglutination, serogroup PCR cnl, PorA 7-2/4, FetA 1-5), Penicillin 0,064, Azithromycin 1. The host serves five
Meningococci templates from `ReportTemplates/Meningo/`; the configured preliminary marker is "Teilbefund - ", which only
"Fax - Teilbefund - v2.docx" carries. Signers: "Dr. Jane Doe", "Dr. John Doe". The seeded MZ004/20 was not used.

## Results

| ID | Spec says | App/test shows | Verdict | Path | Evidence |
| --- | --- | --- | --- | --- | --- |
| FR-001 · US1.1 | Report creation from the sending list or after save-and-create-report | "Änderungen speichern und Befund erstellen" on isolate 14 → `/MeningoReport/Isolate/14` "Befund erstellen"; every list row offers "Befund erstellen" (`/MeningoReport/Isolate/n`); the report page links back with "Isolat bearbeiten" | Match | Browser | `verification/US1-1-report-page.png`, `verification/FR-021-list-status.png` |
| FR-002 · SC-007 | Report routes need only authentication; edit and list paths are Standard User only | `Rki1` and `Admin1`: `/MeningoReport/Isolate/16` HTTP 200 with summary; `/MeningoIsolate/Edit/16` and `/MeningoPatientSending` → 302 to `/Account/Login?ReturnUrl=…`. `Admin1` sees the laboratory navigation, `Rki1` only "RKI Export (Haemophilus)"/"RKI Export (Meningokokken)" | Match | Browser | `verification/FR-002-Rki1-report-page.png`, `verification/FR-002-Admin1-report-page.png` |
| FR-003 · EC1 | Missing identifier → bad request, unknown → not found | `/MeningoReport/Isolate` → 400, `/MeningoReport/Isolate/abc` → 400, `/MeningoReport/Isolate/99999` → 404 | Match | Browser | also test-asserted (added): `Controllers/MeningoReportControllerTests.Isolate_WithoutId_ReturnsBadRequest`, `Isolate_UnknownId_ReturnsNotFound` |
| FR-004 | Report data from isolate, patient, submission, report lines, typings, susceptibility, configuration, QR | Page data of isolate 15: laboratory number, patient, dates, `Report` lines, `Typings` (Identifikation, Agglutination, Serogenogruppe, PorA, FetA), `ETests`, `Comment`, `LabDirector`, `MedicalDirector`, `Contacts`, sender address; isolate 14 adds `DemisIdQrImageUrl` (PNG data URL). `Interpretation`/`InterpretationPreliminary` stay empty | Match | Browser | page script `documentData`; also test-asserted (existing): `Automapper/MeningoIsolateViewModelMappingActionTests.ProcessModelToViewModel_CopiesReportTypingsAndCommentFromInterpretation`, `ProcessModelToViewModel_PopulatesDemisIdImageUrl`, `ProcessModelToViewModel_PopulatesSenderData` |
| FR-005 · US1.2 | Summary shows laboratory number, sampling location, patient, invasive, agglutination, serogroup PCR, susceptibility, report lines | "Befund (Auszug)": "Labornummer" MZ011/26, "Entnahmeort" Blut, "Patient" V.B. / 97080, "Invasiv" Ja, "Agglutination" poly, "Serogruppen-PCR" cnl, "Penicillin" sensibel, "Befund" with both report lines | Match | Browser | `verification/FR-005-report-page-rule20.png` |
| FR-006 · US3.2 · SC-006 | Azithromycin removed from page and document data | Isolate 14: summary lists "Cefotaxim" and "Penicillin" only; the page data hold five of the six susceptibility rows (Azithromycin row missing although measured with breakpoint); the documents have no Azithromycin | Match | Browser | `verification/US1-1-report-page.png`; also test-asserted (added): `MeningoReportControllerTests.Isolate_MeasuredAzithromycin_IsRemovedWhileOtherMeasuredResultsRemain` |
| FR-007 · US1.2 | Templates of the Meningococci set and configured signers | "Befundsvorlage": "Auswählen...", "Fax - Nativmaterial v2.docx", "Fax - Teilbefund - v2.docx", "Fax - Untersuchungsbefund v2.docx", "Nativmaterial v2.docx", "Untersuchungsbefund v2.docx" (values `/ReportTemplates/Meningo/…`); "Unterzeichner": "Dr. Jane Doe", "Dr. John Doe" | Match | Browser | also test-asserted (added): `MeningoReportControllerTests.Isolate_UsesMeningococciTemplateFolderAndConfiguredSigners` |
| FR-008 · US1.2 · EC9 (display) | Lookup by the patient's postal code; returned details shown; success enables the edit link | Page posts `postalCode=97080` to `/Report/QueryHealthOffice/` (the Haemophilus address) on load (blocked). Synthetic answer injected in the browser: address with `<br>` line breaks, `tel:` link, fax `mailto:0093112346@fax`, email link, "Gesundheitsamt bearbeiten" enabled (`/HealthOffices/Edit?postalCode=97080`, not clicked). Blocked answer: box empty, button inactive, no message, generation unaffected | Match | Browser | `verification/FR-008-synthetic-health-office.png` |
| FR-008 (lookup and match) | Details come from the RKI tool, matched against stored health offices | RKI call blocked by the verification rules | Not verifiable | — | Needs a server-side RKI stub or a recorded answer |
| FR-009 · US3.1 · SC-004 | Template and signer required; same error if either is missing | Nothing, template only, signer only: "Fehler Bitte wählen Sie eine Befundvorlage und einen Unterzeichner aus.", no download, no request. Not in the spec: each click adds another copy, and the messages stay after both selections are made and a document is generated | Spec deviation | Browser | `verification/FR-009-missing-selection.png` |
| FR-010 · US2.1 · EC2 · EC3 · Assumptions (marker) | Preliminary exactly when the template path contains the marker; one stored template carries it | "Fax - Teilbefund - v2.docx" posts `preliminary=true`; "Untersuchungsbefund v2.docx" and "Fax - Untersuchungsbefund v2.docx" post `preliminary=false`; no other template contains "Teilbefund - " | Match | Browser | request log |
| FR-011 · US3.3 · SC-004 | No discrepancy confirmation; discrepant text follows the same path | Isolate 14 (report line "Diskrepante Ergebnisse, bitte Datenbankeinträge kontrollieren."), final template: no dialog, download and `ReportGenerated` at once; the final document prints the discrepancy line | Match | Browser | `verification/FR-011-discrepant-no-dialog.png`, `verification/06-final-discrepant-MZ010_26.docx` |
| FR-012 · US4.1 · US4.2 | Signer added, typing values by attribute name, QR image usable | After generation the page data hold `Signer` and `Typing-Identifikation`, `Typing-Agglutination`, `Typing-Serogenogruppe`, `Typing-PorA - Sequenztyp`, `Typing-FetA - Sequenztyp`; the document of isolate 14 contains `image_generated_1.png` at 952500 × 952500 EMU (100 × 100 px) | Match | Browser | `verification/06-final-discrepant-MZ010_26.docx` |
| FR-013 · US1.3 · US2.4 · Assumptions | Same data object for both types; template decides content; line breaks kept; Word document offered | Preliminary "Fax - Teilbefund - v2.docx": fax cover sheet, "Untersuchungsbefund zu MZ011/26 - Teilbefund", typings and report lines, no susceptibility table. Final "Untersuchungsbefund v2.docx": "… - Endbefund", typings, susceptibility table (Penicillin 0,064 µg/ml, sensibel), report lines, comment. `Contacts` lines appear as `<w:br/>` breaks in the header | Match | File | `verification/01-prelim-Teilbefund-MZ011_26.docx`, `verification/03-final1-Untersuchungsbefund-MZ011_26.docx` |
| FR-014 · FR-015 · US4.3 · EC8 | Filename and report date are the generation date | Filename "MZ 011_26-2026-11-08.docx"; "Würzburg, den 08.11.2026", "Befund vom 08.11.2026". Both are in the page when it loads (`documentData.Date`, filename literal in the script), so they are the page-load date. Preliminary and final documents of one day get the same name | Spec deviation | File | `verification/01-prelim-Teilbefund-MZ011_26.docx`, `verification/03-final1-Untersuchungsbefund-MZ011_26.docx` |
| FR-016 · US1.4 · EC5 · Assumptions | Notification after rendering and download, no confirmation | Download and `POST /MeningoReport/ReportGenerated/` (`id=15&preliminary=false`) in the same step; no message afterwards | Match | Browser | `verification/FR-016-after-final-generation.png` |
| FR-017 · US1.5 · EC7 · SC-001 | Final sets Final and replaces the date | Isolate 15: Preliminary → 2 with `2026-11-08 11:30:10.477`; second final → `11:30:28.957`. Isolate 14: None → 2 with `11:30:50.743` | Match | DB | also test-asserted (added): `MeningoReportControllerTests.ReportGenerated_StartingState_EndsInDocumentedStateAndReportDate` (None/Preliminary/Final → Final) |
| FR-018 · US2.2 · SC-002 | Preliminary on a non-final isolate sets Preliminary without date | Isolate 15: None → 1/NULL, again → 1/NULL | Match | DB | also test-asserted (added): same method (None/Preliminary → Preliminary) |
| FR-019 · US2.3 · SC-003 | Preliminary after Final keeps status and date | Isolate 15 stays 2 with `11:30:28.957` after two preliminary documents | Match | DB | also test-asserted (added): same method (Final → Final, date kept) |
| FR-020 · EC6 | Unknown or missing identifier acknowledged without change | `ReportGenerated(4711/null, false/true)` returns `true` and leaves the isolate unchanged | Match | Unit test (added) | `MeningoReportControllerTests.ReportGenerated_UnknownOrMissingIsolateId_AcknowledgesAndChangesNoIsolate` (4 cases); crafted posts with `id=99999` and without id answered HTTP 200 `true`, no isolate changed |
| FR-021 · US4.5 | List distinguishes None, Preliminary, Final | "Befund" column: red `glyphicon-remove-sign` "Kein Befund erstellt" (MZ009/26), yellow `glyphicon-time` "Teilbefund" (MZ012/26), green `glyphicon-ok-sign` "Endbefund" (MZ010/26, MZ011/26) | Match | Browser | `verification/FR-021-list-status.png` |
| FR-022 · US4.4 · SC-008 | Signer only document data, no signing record | Signer appears under "Mit freundlichen Grüßen" in the documents; the notification carries no signer; no table or column for signers or signatures exists | Match | DB | schema query; `verification/01-prelim-Teilbefund-MZ011_26.docx` |
| FR-023 · SC-008 | No generated document persisted | Only `MeningoIsolates.ReportStatus`/`ReportDate` relate to reports; the server receives only `id` and `preliminary` | Match | DB | schema query, request log |
| FR-024 · US4.6 | Status hidden, report date editable as `DD.MM.YYYY` | Isolate form of MZ011/26: `ReportStatus` hidden ("Final"), "Befund am" text field with date picker, value "08.11.2026", editable | Match | Browser | browser run |
| FR-025 | Preserve current behavior, no new rules | Constraint on future changes; nothing observable | Not verifiable | — | — |
| EC10 | Unresolvable sender leaves the sender address empty, report creation continues | View model with `SenderName`, `SenderStreet`, `SenderCity` null; report page returned | Match | Unit test (added) | `MeningoReportControllerTests.Isolate_SenderNotFound_LeavesSenderFieldsEmptyAndShowsReportPage` (the UI cannot create such a submission) |
| EC11 · SEC-004 · DEC-011 | No role check on report routes [SUSPECTED-DEFECT] | `Rki1` opened MZ012/26's report page and generated "Fax - Teilbefund - v2.docx"; isolate 16 changed to Teilbefund (1/NULL) | Defect confirmed | Browser | `verification/FR-002-Rki1-report-page.png`, `verification/07-Rki1-prelim-MZ012_26.docx`, DB query |
| SEC-005 (report-status marking) | Anti-forgery token not verified on report-status marking | The page's own request is `id=15&preliminary=true` without token field or header; stored | Defect confirmed | Browser | request log, DB query |
| DATA-034 | Stale isolate form writes back report status and date | Form of isolate 16 opened at "Teilbefund"; final report generated from the report page (2 with `11:32:54.817`); saving the form set isolate 16 back to 1/NULL | Defect confirmed | Browser | DB query; `verification/08-final-MZ012_26-before-stale-save.docx` |
| DATA-024 · EC4 · US3.4 | Template failures show no message; empty-list label never shown | Template download aborted in the browser: no download, no message, no `ReportGenerated`, browser error "InvalidStateError: Failed to read the 'responseText' property from 'XMLHttpRequest' …". Empty template list: test-asserted only (`MeningoReportControllerTests.Isolate_UsesMeningococciTemplateFolderAndConfiguredSigners`, empty folder) | Defect confirmed | Browser | `verification/EC4-template-load-failure.png` |
| SEC-008 | Health-office values rendered as markup | Synthetic answer with `<b>Teststrasse 1</b>` in the address: rendered bold | Defect confirmed | Browser | `verification/FR-008-synthetic-health-office.png` |
| DATA-015 | Missing telephone breaks the display; missing fax/email show "null" | Synthetic answers: without telephone only the address appears, button inactive, "Cannot read properties of null (reading 'replace')"; without fax and email both show "null" (fax linked to `mailto:-@fax`) | Defect confirmed | Browser | `verification/DATA-015-no-phone.png` |
| REF-021 (FR-011 leftovers) | Not in the spec | `Views/MeningoReport/Isolate.cshtml`: discrepancy dialog `#discrepantResultsModal` and `#ForceCreateReport` handler present, opening check `if (false)//… TODO`; summary branch `Model.Interpretation` ("TODO remove soon") never taken; unused `@using DocumentFormat.OpenXml.EMMA`; lookup posted to `~/Report/QueryHealthOffice/`; filename prefix hard-coded ("TODO Replace MZ and KL with Database Prefix"). In the browser the dialog never appeared (FR-011 row) | Spec deviation | Code | `Views/MeningoReport/Isolate.cshtml` (`#TryCreateReport` click handler, summary `dl`) |

## Unit tests added

- `HaemophilusWebTests/Controllers/MeningoReportControllerTests.cs` (new, added to `HaemophilusWebTests.csproj`):
  `Isolate_WithoutId_ReturnsBadRequest` (FR-003), `Isolate_UnknownId_ReturnsNotFound` (FR-003),
  `Isolate_MeasuredAzithromycin_IsRemovedWhileOtherMeasuredResultsRemain` (FR-006),
  `Isolate_UsesMeningococciTemplateFolderAndConfiguredSigners` (FR-007),
  `Isolate_SenderNotFound_LeavesSenderFieldsEmptyAndShowsReportPage` (EC10),
  `ReportGenerated_StartingState_EndsInDocumentedStateAndReportDate` (FR-017–FR-019, Report State Transitions, 6 cases),
  `ReportGenerated_UnknownOrMissingIsolateId_AcknowledgesAndChangesNoIsolate` (FR-020, 4 cases). 15 test cases
  (7 methods). The fixture uses its own data sets and restores `IsolateViewModelMappingActionBase.DbForTest` after each
  test.

## Spec corrections applied

- FR-009: quotes the message and records that repeated attempts stack it and that it stays (was: "the same
  missing-selection error").
- FR-014, FR-015, US4.3 and EC8: filename date and report date are the server date at page load (was: generation date);
  same-day preliminary and final documents share the filename.
- FR-011: records the disabled discrepancy dialog and the unused interpretation branch, `[POTENTIAL-REFACTORING]`
  (REF-021).
- FR-008: names the shared lookup address `/Report/QueryHealthOffice/` and adds the SEC-008/DATA-015 note
  (`[SUSPECTED-DEFECT]`).
- FR-016, EC4 and a new EC "An isolate form opened before a report is generated …": `[SUSPECTED-DEFECT]` notes for
  SEC-005, DATA-024 and DATA-034.

## Backlog updates

- SEC-004: confirmed on system 2026-11-08 for Meningococci (`Rki1`, `Admin1`).
- DEC-011: observation recorded for the Meningococci report page, no decision taken.
- SEC-005: confirmed for the Meningococci report-status marking; source extended by 009 FR-016.
- SEC-008, DATA-015: confirmed with synthetic lookup answers; sources extended by 009 FR-008.
- DATA-024: confirmed for the Meningococci template-retrieval failure; source extended by 009 Edge Cases.
- DATA-034: confirmed again with the Meningococci report page; source extended by 009 Edge Cases.
- NEW REF-021: Meningococci report page leftovers (disabled discrepancy dialog, unused interpretation branch, unused
  namespace, lookup on the Haemophilus address, hard-coded filename prefix).

## Not verifiable here

- FR-008 (lookup and match): the RKI tool call is blocked; the server-side match against stored health offices and the
  storing of new offices need a server-side RKI stub or a recorded answer.
- FR-025: a constraint on future changes, nothing to observe.

## Open questions for the owner

- `DEC-*` candidate: a final Meningococci document of a discrepant isolate prints "Diskrepante Ergebnisse, bitte
  Datenbankeinträge kontrollieren." without any confirmation, while the Haemophilus page asks first (008 FR-009/FR-010).
  The Meningococci check is switched off in the page script with a TODO. Should Meningococci reporting ask for
  confirmation as well (it would check the report lines, since the Meningococci model has no interpretation text)?
- As in 008: the document date and the filename date are fixed at page load, and same-day preliminary and final
  documents share the filename "MZ 011_26-2026-11-08.docx".
- Own records left behind: submissions `SV-009-A` (MZ010/26, isolate 14, Endbefund), `SV-009-B` (MZ011/26, isolate 15,
  Endbefund) and `SV-009-C` (MZ012/26, isolate 16, Teilbefund without date after the stale-form save; its preliminary
  report was generated by `Rki1`). Saving the isolate forms created empty PubMLST records (DATA-036, from 005).
