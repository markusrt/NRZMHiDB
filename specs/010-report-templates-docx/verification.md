# Verification: Report templates (.docx)

**Spec**: 010 · **Date**: 2026-11-08 · **Build**: `214b7fd` · **Host**: ScreenshotTests
SpecVerificationHost (`http://localhost:9899`, catalog `HaemophilusWeb_Screenshots`) ·
**Accounts used**: Standard1, Labor1, Admin1, anonymous · **Scope**: full

## Summary

| Verdict | Count |
| --- | --- |
| Match | 20 |
| Spec deviation | 6 |
| Defect confirmed | 1 |
| New defect | 1 |
| Not verifiable | 1 |

By primary path: Browser 15 · File 9 · DB 0 · Unit test (existing) 1 · Unit test (added) 1 · Code 2 ·
Not verifiable 1 (29 rows). Also test-asserted: 2 rows with a browser verdict. Test run: `vstest.console.exe …
/TestCaseFilter:"FullyQualifiedName~HaemophilusWeb.Utils.FileListerTests|FullyQualifiedName~HaemophilusWeb.Controllers.ReportControllerTests|FullyQualifiedName~HaemophilusWeb.Controllers.MeningoReportControllerTests"`
→ 40 passed, 0 failed; full suite → 1122 passed, 0 failed.

Edge cases are numbered EC1–EC14 as the list stood before this verification (EC1 = top-level-only discovery, EC14 =
two near-identical page scripts). Scenarios are `US<story>.<n>`.

Test data: Haemophilus submissions `SV-010-A` (KL012/26, isolate 15, DEMIS-ID `SV-010-DEMIS-A`; growth, β-lactamase
negativ, agglutination negativ, ompP2 and fucK positiv, bexA negativ, MLST "SV10", E-Tests Ampicillin 0,5, Cefotaxim
0,012 and Azithromycin 0,5 with the "ohne Grenzwerte" breakpoint) and `SV-010-B` (KL013/26, isolate 16, no DEMIS-ID,
isolate defaults), and the Meningococci submission `SV-010-M` (MZ013/26, isolate 17, DEMIS-ID `SV-010-DEMIS-M`), all
with sender 1 and synthetic patients "V.X.", "V.Y.", "V.Z." without birth date or postal code. The seeded Haemophilus
isolate 1 served the failure paths; its report status stayed 0/NULL (the one probe that produced a document had its
`ReportGenerated` request aborted in the browser). Temporary changes, all reverted in the session: three probe
entries in `HaemophilusWeb/ReportTemplates/` (`SV-010-Probe v1.docx` with non-zip content, `~$SV-010-Lock.docx`, a
folder `SV-010-Ordner.docx`; no stored template was touched, `git status` of the folder is clean), and the
announcement in "Konfiguration" (set to two lines from 01.11.2026 to 30.11.2026, then reset to empty text without
dates, as seeded). Templates were inspected read-only as zips (document, headers, footers, field instructions).

## Results

| ID | Spec says | App/test shows | Verdict | Path | Evidence |
| --- | --- | --- | --- | --- | --- |
| FR-001 | Haemophilus templates at the top of `ReportTemplates/`, Meningococci in `ReportTemplates/Meningo/`, fragments in `includes/` | Option values `/ReportTemplates/<name>.docx` on the Haemophilus page and `/ReportTemplates/Meningo/<name>.docx` on the Meningococci page; the folder holds 21 + 5 templates and 9 fragments in `includes/` | Match | Browser | `verification/US1-H-template-list.png`, `verification/US1-M-template-list.png`; also test-asserted: `Controllers/MeningoReportControllerTests.Isolate_UsesMeningococciTemplateFolderAndConfiguredSigners` |
| FR-002 · US1.1 · US1.2 · US1.4 · EC1 · SC-001 · SC-002 | Direct `.docx` entries listed on every request, no cache; no fragments, no other pathogen | "Befundsvorlage" lists exactly the 21 Haemophilus and the 5 Meningococci files (names as in the inventory table); no `includes/` or cross-pathogen entry. Three probe entries added to the folder appeared on the next page load (25 entries after "Auswählen..."), and after their removal the list was back to 21 | Match | Browser | `verification/US1-4-probe-files-listed.png`; also test-asserted: `Utils/FileListerTests.Files_SubFolderWithMatchingFiles_ListsTopLevelEntriesOnly`, `Controllers/ReportControllerTests.Isolate_TemplateAddedBetweenRequests_IsListedOnNextRequest` (added), `Isolate_ReportTemplatesAvailable_AddsFileInfosToViewBag` |
| FR-003 · US1.5 | Missing location yields an empty list without error | Asserted for the lister and the controller; the page part (placeholder only, required-selection error) follows from 008 FR-007 | Match | Unit test (existing) | `Utils/FileListerTests.Files_UnknownDirectory_ReturnsEmptyList`, `Controllers/ReportControllerTests.Isolate_NoReportTemplatesAvailable_AddsFileInfosToViewBag`, `MeningoReportControllerTests.Isolate_UsesMeningococciTemplateFolderAndConfiguredSigners` |
| FR-004 | Discovery runs even when the page is rejected | `Isolate(null)` returns HTTP 400 with the two templates already in the view data | Match | Unit test (added) | `Controllers/ReportControllerTests.Isolate_WithoutId_StillDiscoversTemplates` |
| FR-005 · US1.3 · US4.2 | File name with extension as label, app-relative location as value, "Auswählen..." first, no default | Label "Befundsvorlage"; first entry "Auswählen..." with empty value, selected index 0, no entry marked selected; labels such as "BLNAS v10.docx", values such as `/ReportTemplates/BLNAS v10.docx` (unencoded); the version is part of the label | Match | Browser | `verification/US1-H-template-list.png` |
| FR-006 | No pre-selection or filtering by isolate, status or role | Identical 22-entry lists for isolate 1 (no report), 15 (Endbefund) and 16 as `Standard1`, and for isolate 15 as `Labor1` | Match | Browser | run log |
| FR-007 · US3.1 · US3.2 · US3.3 · SC-003 · EC5 | Marker "Teilbefund - " from the setting, matched on the whole location string | The page script holds `templateFile.indexOf("Teilbefund - ")` on the option value (folder plus file name); exactly "Fax - Teilbefund - Serotyp v4.docx" and "Fax - Teilbefund - v2.docx" match. Requests: `preliminary=true` for both, `preliminary=false` for "BLNAS v10.docx", "Fax - BLNAS v10.docx", "Kein Wachstum v1.docx", "Nicht Invasiv v1.docx", "Untersuchungsbefund v2.docx" | Match | Browser | request log |
| FR-008 · US2.1 | Fresh retrieval with a random parameter; output caching off for `.docx` | Every generation requested the template as `…docx?nocache=0.<random>`; the response carried no caching headers; the configuration holds the `.docx` caching profile `DontCache` | Match | Browser | request log; `Web.config` `system.webServer/caching` |
| FR-009 | Data extended by `Signer`, `Typing-<Attribute>`, `DemisIdQrImage` | After generation the page data of isolate 15 holds `Signer`, `Typing-ompP2`, `Typing-fucK`, `Typing-bexA`, `Typing-MLST` and `DemisIdQrImage`; isolate 16 (no typings, no DEMIS-ID) only `Signer` | Match | Browser | run log |
| FR-010 · US2.2 · US2.3 · US2.4 · SC-004 | Values replaced, lists repeated, conditional sections, line breaks kept | Nine generated reports contain no leftover tag. KL012/26 "BLNAS v10.docx": four typing rows (ompP2, fucK, bexA, MLST) and three E-Test rows for four and three list entries; MZ013/26 "Untersuchungsbefund v2.docx": the three report lines; QR section with "Meldungs-ID" only for isolates with DEMIS-ID; the comment/announcement section appears only with an active announcement and keeps its line break as `<w:br/>` | Match | File | `verification/G1-KL012-BLNAS.docx`, `G4-KL013-Kein-Wachstum.docx`, `G6-MZ013-Untersuchungsbefund.docx`, `G8-KL013-Kein-Wachstum-Hinweis.docx`, `G9-MZ013-Nativmaterial-Hinweis.docx` |
| FR-011 · US2.5 · EC9 | 100 x 100, not centered | QR image inline at 952500 × 952500 EMU (100 × 100 px) in all five reports with DEMIS-ID. The filling step does not center it, but the template paragraph holding `{%DemisIdQrImage}` is centered in all 26 templates, so the image is centered in the report | Spec deviation | File | `verification/G1-KL012-BLNAS.docx`, `G6-MZ013-Untersuchungsbefund.docx` |
| FR-012 · SC-005 | Filled document only in the browser, not stored | Per generation only `GET` of the template and `POST …/ReportGenerated/` with `id` and `preliminary`; the catalog has no table or binary column for documents | Match | Browser | request log; schema query |
| FR-013 · US2.6 · EC6 · SC-007 · DATA-024 | Failures offer no document, send no notification, show no message [SUSPECTED-DEFECT] | Non-zip probe file: page error "Can't find end of central directory : is this a zip file ?", no console log from the page code. Folder entry: HTTP 403, "Ajax error … 403 Forbidden". Template with an unclosed loop (supplied in the browser): console log "Unclosed loop" / "Multi error". Each time: no download, no message, no `ReportGenerated`; isolate 1 stayed 0/NULL | Defect confirmed | Browser | `verification/EC6-corrupt-template-no-message.png`, `verification/EC6-render-error-no-message.png`; DB query |
| FR-014 · US4.4 · SC-006 | No in-app function to upload, edit, rename or delete templates | Navigation of `Standard1`, `Labor1` (all roles) and `Admin1` offers no template entry; the template folder is referenced only by the report controller base | Match | Browser | run log |
| FR-015 · US4.1 | Shipped only when registered; 26 registered, 9 fragments not | `HaemophilusWeb.csproj` has 26 `Content` items under `ReportTemplates/` and none under `includes/`; the existing package outputs `obj/Debug/Package/PackageTmp` and `obj/Release/Package/PackageTmp` contain the 26 templates and no `includes/` folder | Match | File | project file; package folders |
| US4.3 | Template releases announced in the change log | "Über" lists "Update Befundvorlagen gemäß DAkkS Reviewanmerkungen" (24.02.2026), "Update Kopfzeile Befundvorlagen" (10.08.2024), "Update Befundvorlagen" (11.03.2023, 11.01.2022) and older entries | Match | Browser | `verification/US4-3-change-log.png` |
| EC2 · UX-004 | Lock/temp `.docx` files offered [POTENTIAL-REFACTORING] | `~$SV-010-Lock.docx` was offered at once. Not in the spec: a folder `SV-010-Ordner.docx` was offered too (discovery lists file-system entries); choosing it ended in HTTP 403 and no message | Spec deviation | Browser | `verification/US1-4-probe-files-listed.png` |
| EC3 · Assumptions (order) | No explicit sort; order of the file system | Entries in case-insensitive alphabetical order on the NTFS host ("Sonstige …" before "SV-010-…", `~$…` last) | Match | Browser | `verification/US1-4-probe-files-listed.png` |
| EC4 · DATA-024 | "Keine Vorlagen hinterlegt" never shown [SUSPECTED-DEFECT] | An empty template folder cannot be produced without moving the shipped templates away from the running host; the empty list itself is test-asserted (FR-003) | Not verifiable | Not verifiable | — |
| EC7 · Assumptions (missing values) | Missing/null values render `undefined` (documented default, not verified) | Synthetic template supplied in the browser for isolate 1: an absent key, the null `Comment` and the absent `Typing-bexA` rendered "undefined", the empty `Announcement` rendered nothing. The nine reports from stored templates contain no "undefined" (comment only inside its section; missing birth date prepared as "keine Angabe") | Match | File | `verification/EC7-null-probe-isolate1.docx` |
| EC8 · REF-004 | `Typing-<Attribute>` values unused [POTENTIAL-REFACTORING] | No placeholder `Typing-…` in any of the 26 templates; typings come from the repeated `Typings` section | Match | File | template inspection |
| EC10 · REF-004 | Includes with absolute developer paths, not shipped [POTENTIAL-REFACTORING] | All 26 templates carry `INCLUDETEXT` fields to `D:\Development\NRZMHiDB\HaemophilusWeb\ReportTemplates\…` (the current checkout lives elsewhere, so they do not resolve even here); the generated reports keep 5–11 such fields; fragments not shipped (FR-015) | Match | File | template inspection; `verification/G2-KL012-Fax-BLNAS.docx` |
| EC11 · REF-004 | Haemophilus fax variants embed the non-fax template [POTENTIAL-REFACTORING] | True for the 14 fax variants with a base template. Not in the spec: `Fax - Teilbefund - Serotyp v4` has no base and embeds only fragments; the Meningococci fax variants embed only the fragments (fax cover sheet, fax footnote, footer, contact header, signature) | Spec deviation | File | template inspection |
| EC12 · SEC-010 | Libraries from public CDNs on every page, two without integrity | `/Sender` loads docxtemplater 3.42.2 and FileSaver 2.0.0 from cdnjs with `integrity`, `pizzip.js` and `pizzip-utils.js` 3.0.6 from unpkg without | Match | Browser | request log |
| EC13 · REF-004 | Deprecated load-then-set-data interface [POTENTIAL-REFACTORING] | `Views/Report/Isolate.cshtml` and `Views/MeningoReport/Isolate.cshtml`, `DoCreateReport`: `new window.docxtemplater().loadZip(zip)`, `attachModule`, `setData`, `setOptions`, `render`; no console warning during nine generations | Match | Code | page scripts |
| EC14 · REF-002 | Two copies differing in discrepancy check, filename prefix, edit link [POTENTIAL-REFACTORING] | Diff of the two scripts: also the `ReportGenerated` address (`~/Report/…` vs `~/MeningoReport/…`) and the isolate id property differ; the Meningococci discrepancy check is `if (false)//… TODO` | Spec deviation | Code | `Views/Report/Isolate.cshtml`, `Views/MeningoReport/Isolate.cshtml` |
| Template Data Consumed | Susceptibility in "resistance-profile templates (both sets)", typing in "most templates", conditional comment and announcement | Placeholder inventory: susceptibility loop in 14 Haemophilus templates and the 2 Meningococci "Untersuchungsbefund" templates (there inside `{#HasETests}`); typing loop in 21 of 26; one outer section `{#HasCommentOrAnnouncement}` with the announcement text and a nested `{#HasComment}`; QR section in all 26 | Spec deviation | File | template inspection |
| Assumptions (direct access) | Whether template files need sign-in was not established | Anonymous requests: Haemophilus and Meningococci templates and `includes/Signature.docx` HTTP 200; `/ReportTemplates/` HTTP 403; the report page redirects to the sign-in page | Spec deviation | Browser | run log |
| New EC "For a susceptibility result without breakpoints" · UX-015 | — | "BLNAS v10.docx" for KL012/26 prints the Azithromycin row as "0,5 µg/ml, n.d.", "≤--- µg/ml", ">--- µg/ml", "---" | New defect | File | `verification/G1-KL012-BLNAS.docx` |

## Unit tests added

- `HaemophilusWebTests/Utils/FileListerTests.cs`: `Files_SubFolderWithMatchingFiles_ListsTopLevelEntriesOnly`
  (FR-002, EC1)
- `HaemophilusWebTests/Controllers/ReportControllerTests.cs`:
  `Isolate_TemplateAddedBetweenRequests_IsListedOnNextRequest` (FR-002, US1.4),
  `Isolate_WithoutId_StillDiscoversTemplates` (FR-004)

Three test cases in three methods.

## Spec corrections applied

- FR-011, US2.5, image edge case: the filling step does not center the image; it keeps the template paragraph's
  alignment, centered in all 26 templates (was: "not centered").
- EC2: folders named `*.docx` are offered too; choosing one ends in HTTP 403 without a message.
- EC6: unreadable files are raised without console output; only placeholder errors are logged (was: every processing
  failure logged); backlog DATA-024 named.
- EC7 and its assumption: `undefined` reproduced with a synthetic template; no stored template showed it (was: not
  verified).
- New edge case: "≤--- µg/ml" for results without breakpoints [SUSPECTED-DEFECT] (UX-015).
- EC11: `Fax - Teilbefund - Serotyp v4` and the Meningococci fax variants embed only the shared fragments.
- EC14: the copies also differ in the notification address and isolate id.
- "Template Data Consumed": susceptibility, typing and comment/announcement rows name the templates and the nesting.
- Assumptions: template files are served without sign-in (was: not established).

## Backlog updates

- DATA-024: confirmed on system 2026-11-08 for processing failures (non-zip file, folder entry, template error);
  empty-list label not reproducible.
- UX-004: confirmed (lock file, no sort) and extended by folder entries.
- REF-002: differences of the two report scripts listed.
- REF-004: confirmed (absolute include paths kept in generated reports, fax embedding incl. Meningococci, no
  `Typing-` reference, deprecated calls).
- SEC-010: confirmed on system.
- NEW UX-015: Haemophilus reports print "≤--- µg/ml" for susceptibility results without breakpoints.

## Not verifiable here

- EC4 (DATA-024, "Keine Vorlagen hinterlegt"): needs an empty or missing template folder for a pathogen. Verifiable on
  a host started with a copy of the application whose `ReportTemplates/Meningo/` is empty.

## Open questions for the owner

- The template files (and, wherever the `includes/` folder is deployed, the fragments) can be downloaded without
  signing in. They hold letterhead, contact placeholders and fixed text, no patient data. Is this acceptable, or
  should the template folder require sign-in?
- UX-015: should Haemophilus reports list susceptibility results without breakpoints at all (the Meningococci report
  removes Azithromycin, 009 FR-006), or print them with another wording? Candidate for a `DEC-*` entry.

Blocked external calls: `POST http://localhost:9899/Report/QueryHealthOffice/` 23 times (every load of a Haemophilus
or Meningococci report page, in the main and one role context); no other outbound call was triggered.
