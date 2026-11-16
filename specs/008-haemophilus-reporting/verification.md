# Verification: Haemophilus reporting (preliminary + final)

**Spec**: 008 · **Date**: 2026-11-08 · **Build**: `0dabc6d` · **Host**: ScreenshotTests
SpecVerificationHost (`http://localhost:9899`, catalog `HaemophilusWeb_Screenshots`) ·
**Accounts used**: Standard1, Rki1, Admin1 · **Scope**: full

## Summary

| Verdict | Count |
| --- | --- |
| Match | 19 |
| Spec deviation | 3 |
| Defect confirmed | 6 |
| New defect | 0 |
| Not verifiable | 2 |

By primary path: Browser 19 · File 3 · DB 5 · Unit test (existing) 0 · Unit test (added) 1 · Code 0 ·
Not verifiable 2 (30 rows). Also test-asserted: 5 rows with a browser or DB verdict. Test run:
`vstest.console.exe … /TestCaseFilter:"FullyQualifiedName~HaemophilusWeb.Controllers.ReportControllerTests"` → 19
passed, 0 failed; full suite → 1104 passed, 0 failed.

Edge cases are numbered EC1–EC10 as the list stood before this verification (EC1 = missing/unknown identifier, EC10 =
health-office lookup is ancillary). Scenarios are `US<story>.<n>`.

Test data: submissions `SV-008-A` (KL009/26, isolate 12, with DEMIS-ID `SV-008-DEMIS-A`), `SV-008-B` (KL010/26,
isolate 13) and `SV-008-C` (KL011/26, isolate 14), created through intake with sender 1 and synthetic patients
"V.A.", "V.B.", "V.C." (postal code 97080). Isolate 12: agglutination a, bexA and Serotyp-PCR n.d., ompP2 and fucK
positive, MLST "SV8", Ampicillin E-Test 0,5 (final interpretation discrepant, preliminary not). Isolate 13:
agglutination negativ, bexA negativ (preliminary interpretation discrepant, final not). The host serves 21 Haemophilus
templates from `ReportTemplates/` (top level only; `includes/` and `Meningo/` are not offered); the configured
preliminary marker is "Teilbefund - ", which only "Fax - Teilbefund - Serotyp v4.docx" carries. Signers in the seed:
"Dr. Jane Doe", "Dr. John Doe".

## Results

| ID | Spec says | App/test shows | Verdict | Path | Evidence |
| --- | --- | --- | --- | --- | --- |
| FR-001 · US1.1 | Report creation is entered after the save-and-create-report action | "Änderungen speichern und Befund erstellen" on isolate 12 → `/Report/Isolate/12` "Befund erstellen". Not in the spec: every submissions-list row also offers "Befund erstellen" (002 FR-014), and the report page links back with "Isolat bearbeiten" | Spec deviation | Browser | `verification/FR-005-report-page.png`, `verification/FR-020-list-status.png` |
| FR-002 · SC-006 | Report routes need only authentication; the edit-to-report path is Standard User only | `Rki1` and `Admin1`: `/Report/Isolate/14` HTTP 200 with summary; `/Isolate/Edit/14` and `/PatientSending` redirect to `/Account/Login?ReturnUrl=…`. `Admin1` sees the laboratory navigation, `Rki1` only the RKI exports | Match | Browser | `verification/FR-002-Rki1-report-page.png`, `verification/FR-002-Admin1-report-page.png` |
| FR-003 · EC1 | Missing identifier → bad request, unknown → not found | `/Report/Isolate` → 400, `/Report/Isolate/abc` → 400, `/Report/Isolate/99999` → 404 | Match | Browser | also test-asserted: `Controllers/ReportControllerTests.Isolate_WithoutId_ReturnsBadRequest`, `Isolate_UnknownId_ReturnsNotFound` (added) |
| FR-004 | Report view holds isolate, patient, submission, interpretation, susceptibility, typing, contact, comment, announcement, QR data | Page data of isolate 12: laboratory number, patient, dates, both interpretations and disclaimer, `ETests` (Ampicillin 0,5 sensibel), `Typings` (ompP2, fucK, MLST), `Contacts`, `LabDirector`, `MedicalDirector`, `Announcement`, `Comment`, `DemisIdQrImageUrl` (PNG data URL), sender address | Match | Browser | page script `documentData` |
| FR-005 · US1.2 | Isolate summary, all Haemophilus templates, all configured signers | Summary "Befund (Auszug)" (Labornummer, Entnahmeort, Patient, Invasiv, Beurteilung, Agglutination, Serotyp-PCR, bexA, ß-Laktamase, E-Test rows, "Teilbefund", "Befund"); "Befundsvorlage" lists the 21 top-level templates after "Auswählen..."; "Unterzeichner" lists "Dr. Jane Doe", "Dr. John Doe" | Match | Browser | `verification/FR-005-report-page.png`; also test-asserted: `ReportControllerTests.Isolate_ReportTemplatesAvailable_AddsFileInfosToViewBag`, `Isolate_ConfiguredReportSigners_AddsSignersToViewBag` (added) |
| FR-006 · US1.2 · EC10 (display) | Lookup by the patient's postal code; returned details shown; success enables the edit link | Page posts `postalCode=97080` to `/Report/QueryHealthOffice/` on load (blocked). With a synthetic answer injected in the browser: address with `<br>` line breaks, telephone `tel:` link, fax `mailto:0093112346@fax`, email link, "Gesundheitsamt bearbeiten" enabled (`/HealthOffices/Edit?postalCode=97080`). Blocked or empty (`null`) answer: box empty, button inactive, no message, generation unaffected | Match | Browser | `verification/FR-006-synthetic-health-office.png` |
| FR-006 (lookup and match) | Details come from the RKI tool, matched against stored health offices | RKI call blocked by the verification rules; no health office stored in the catalog | Not verifiable | — | Needs an RKI tool stub on the server side |
| FR-007 · US3.1 · SC-004 | Template and signer required; same error if either is missing | Nothing, template only, signer only: "Fehler Bitte wählen Sie eine Befundvorlage und einen Unterzeichner aus.", no dialog, no download, no request. Not in the spec: each click adds another copy, and the messages stay after both selections are made | Spec deviation | Browser | `verification/FR-007-missing-selection.png`, `verification/FR-007-repeated-error.png` |
| FR-008 · US2.1 · EC4 | Preliminary exactly when the template path contains the marker | Option values are unencoded paths (`/ReportTemplates/Fax - Teilbefund - Serotyp v4.docx`); this template posts `preliminary=true`, "BLNAS v10.docx" posts `preliminary=false` | Match | Browser | request log |
| FR-009 · US3.2 | Final checks "Befund", preliminary checks "Teilbefund" | Isolate 12: BLNAS opens "Diskrepante Ergebnisse", Teilbefund template downloads directly. Isolate 13: Teilbefund template opens the dialog, BLNAS does not | Match | Browser | `verification/FR-010-discrepancy-dialog.png` |
| FR-010 · US3.3 · US3.4 | Explicit continue required; declining creates nothing | Dialog text "Diskrepante Ergebnisse, bitte Datenbankeinträge kontrollieren. Soll dennoch ein Befundvorschlag ausgegeben werden?", buttons "Nein"/"Ja". "Nein": dialog closes, no download, no request, status unchanged. "Ja": download and `ReportGenerated` | Match | Browser | `verification/FR-010-discrepancy-dialog.png` |
| FR-011 · US4.1 · US4.2 | Signer added, typing values by attribute name, QR image usable | After generation the page data holds `Signer` and `Typing-<Attribute>` keys (e.g. `Typing-bexA` "negativ"); the documents of isolate 12 contain the QR image `image_generated_1.png` at 952500 × 952500 EMU (100 × 100 px). The templates print typings through the `Typings` list, not the flattened keys (REF-004) | Match | Browser | `verification/03-final1-BLNAS-KL009_26.docx` |
| FR-012 | Line breaks kept; Word document offered for download | `Contacts` lines appear as `<w:br/>` breaks in the header of the downloaded `.docx`; files open as Word documents (zip with `word/document.xml`) | Match | File | `verification/03-final1-BLNAS-KL009_26.docx` |
| FR-013 · FR-014 · US4.3 · EC9 | Filename and report date are the generation date | Filename "KL 009_26-2026-11-08.docx", "Würzburg, den 08.11.2026", "Befund vom 08.11.2026". Both dates are already in the page when it loads (`documentData.Date`, filename literal in the script), so they are the page-load date, not the click date. Preliminary and final documents of one day get the same name | Spec deviation | File | `verification/01-prelim-Teilbefund-KL009_26.docx`, `verification/03-final1-BLNAS-KL009_26.docx` |
| FR-015 · US1.4 · EC6 · Assumptions | Notification after rendering and download, no confirmation | Download "KL 009_26-2026-11-08.docx" and `POST /Report/ReportGenerated/` (`id=12&preliminary=false`) in the same step; the page shows no message afterwards | Match | Browser | `verification/FR-015-after-final-generation.png` |
| FR-016 · US1.5 · EC8 · SC-001 | Final sets Final and replaces the date | Isolate 12: Preliminary → Final with `2026-11-08 11:15:00.233`; second final → `11:15:21.010` | Match | DB | also test-asserted: `ReportControllerTests.ReportGenerated_ValidIsolateId_AssignsReportDate`, `ReportGenerated_MultipleReports_UpdatesReportDate`, `ReportGenerated_FinalAfterPreliminary_SetsFinalAndReportDate` (added) |
| FR-017 · US2.2 · SC-002 | Preliminary on a non-final isolate sets Preliminary without date | Isolate 12: None → 1/NULL, again → 1/NULL; isolate 13 ("Ja" after the dialog) → 1/NULL | Match | DB | also test-asserted: `ReportGenerated_ValidIsolateIdAndPreliminaryReport_DoesNotAssignReportDate`, `ReportGenerated_PreliminaryAfterPreliminary_KeepsPreliminaryWithoutReportDate` (added) |
| FR-018 · US2.3 · SC-003 | Preliminary after Final keeps status and date | Isolate 12 stays 2 with `11:15:21.010` | Match | DB | also test-asserted: `ReportGenerated_PreliminaryReportAfterFinal_DoesNotResetStateNorReportDate` |
| FR-019 · EC7 | Unknown or missing identifier acknowledged without change | `ReportGenerated(4711/null, false/true)` returns `true` and leaves the isolate at None without date | Match | Unit test (added) | `ReportControllerTests.ReportGenerated_UnknownOrMissingIsolateId_AcknowledgesAndChangesNoIsolate` (4 cases) |
| FR-020 | List distinguishes None, Preliminary, Final | "Befund" column: red `glyphicon-remove-sign` "Kein Befund erstellt" (KL011/26), yellow `glyphicon-time` "Teilbefund" (KL010/26), green `glyphicon-ok-sign` "Endbefund" (KL009/26) | Match | Browser | `verification/FR-020-list-status.png` |
| FR-021 · US4.4 | Signer only document data, no signing record | Signer appears under "Mit freundlichen Grüßen" in the documents ("Dr. Jane Doe", "Dr. John Doe"); the notification carries no signer; no table or column for signers or signatures exists | Match | DB | schema query; `verification/01-prelim-Teilbefund-KL009_26.docx` |
| FR-022 · SC-007 | No generated document persisted | Only `Isolates.ReportStatus`/`ReportDate` relate to reports; the server receives only `id` and `preliminary` | Match | DB | schema query, request log |
| FR-023 | Preserve current behavior, no new rules | Constraint on future changes; nothing observable | Not verifiable | — | — |
| US2.4 · Assumptions | Template decides which fields appear; preliminary interpretation used | Preliminary document: "Untersuchungsbefund zu KL009/26 - Teilbefund", typing section "folgt", the preliminary interpretation, "Untersuchungen zur Antibiotika-Resistenz: folgt", fax cover sheet; the final document lists typings and E-Test | Match | File | `verification/01-prelim-Teilbefund-KL009_26.docx`, `verification/03-final1-BLNAS-KL009_26.docx` |
| EC3 · SEC-004 · DEC-011 | No role check on report routes [SUSPECTED-DEFECT] | `Rki1` opened isolate 14's report page and generated a "Fax - Teilbefund - Serotyp v4.docx" document; isolate 14 changed to Teilbefund (1/NULL) | Defect confirmed | Browser | `verification/FR-002-Rki1-report-page.png`, DB query |
| SEC-005 (report-status marking) | Anti-forgery token not verified on report-status marking | The page's own request is `id=12&preliminary=true` without token field or header; stored | Defect confirmed | Browser | request log, DB query |
| DATA-034 | Stale isolate form writes back report status and date | Form of isolate 13 opened at "Teilbefund"; final report generated from the report page; saving the form set isolate 13 back to 1/NULL | Defect confirmed | Browser | DB query; `verification/07-final-KL010_26-before-stale-save.docx` |
| DATA-024 · EC2 · EC5 | Template failures show no message; empty-list label never shown | Template download aborted in the browser: no download, no message, no `ReportGenerated`, browser error "InvalidStateError: Failed to read the 'responseText' property from 'XMLHttpRequest' …". Empty template list: test-asserted only (`Isolate_NoReportTemplatesAvailable_AddsFileInfosToViewBag`) | Defect confirmed | Browser | `verification/EC5-template-load-failure.png` |
| SEC-008 | Health-office values rendered as markup | Synthetic answer with `<b>Teststrasse 1</b>` in the address: rendered bold | Defect confirmed | Browser | `verification/FR-006-synthetic-health-office.png` |
| DATA-015 | Missing telephone breaks the display; missing fax/email show "null" | Synthetic answers: without telephone only the address appears, button inactive, "Cannot read properties of null (reading 'replace')"; without fax and email both show "null" (fax linked to `mailto:-@fax`) | Defect confirmed | Browser | `verification/DATA-015-no-phone.png` |

## Unit tests added

- `HaemophilusWebTests/Controllers/ReportControllerTests.cs`: `Isolate_WithoutId_ReturnsBadRequest` (FR-003),
  `Isolate_UnknownId_ReturnsNotFound` (FR-003), `Isolate_ConfiguredReportSigners_AddsSignersToViewBag` (FR-005),
  `ReportGenerated_UnknownOrMissingIsolateId_AcknowledgesAndChangesNoIsolate` (FR-019, 4 cases),
  `ReportGenerated_FinalAfterPreliminary_SetsFinalAndReportDate` (FR-016),
  `ReportGenerated_PreliminaryAfterPreliminary_KeepsPreliminaryWithoutReportDate` (FR-017). 9 test cases (6 methods).

## Spec corrections applied

- FR-001: adds the "Befund erstellen" list action (002 FR-014) and the "Isolat bearbeiten" link (was: only the
  save-and-create-report action).
- FR-007: quotes the message and records that repeated attempts stack it and that it stays (was: "the same
  missing-selection error").
- FR-013, FR-014, EC9: filename date and report date are the server date at page load (was: generation date); same-day
  preliminary and final documents share the filename.
- FR-006, FR-015, EC5 and a new EC "An isolate form opened before a report is generated …": cross-references and
  `[SUSPECTED-DEFECT]` notes for SEC-008, DATA-015, SEC-005, DATA-024 and DATA-034.

## Backlog updates

- SEC-004: confirmed on system 2026-11-08 (`Rki1`, `Admin1`).
- DEC-011: observation recorded, no decision taken.
- SEC-005: confirmed for the report-status marking; source extended by 008 FR-015.
- SEC-008: confirmed with a synthetic lookup answer; source extended by 008 FR-006.
- DATA-015: confirmed with synthetic lookup answers; source extended by 008 FR-006.
- DATA-024: confirmed for the template-retrieval failure; source extended by 008 Edge Cases.
- DATA-034: confirmed again for the Haemophilus report page; source extended by 008 Edge Cases.

## Not verifiable here

- FR-006 (lookup and match): the RKI tool call is blocked; the server-side match against stored health offices and the
  storing of new offices need a server-side RKI stub or a recorded answer.
- FR-023: a constraint on future changes, nothing to observe.

## Open questions for the owner

- The report date in the document and the filename date are fixed when the report page is loaded. Is a report page
  that stays open overnight a realistic case worth a change (date at click time)?
- Preliminary and final documents of the same day download under the same name ("KL 009_26-2026-11-08.docx"); the
  browser numbers the second copy. Should the filename name the report type?
- For the 016 verification: an empty lookup answer (`null`) leaves the health-office box empty without a message and
  raises the browser error "Cannot read properties of null (reading 'error')"; the page's `error` branch never applies
  because the server never answers in that shape. 016 states this only as an assumption.
- Own records left behind: submissions `SV-008-A` to `SV-008-C` (isolates 12 Final, 13 Preliminary, 14 Preliminary
  set by `Rki1`).
