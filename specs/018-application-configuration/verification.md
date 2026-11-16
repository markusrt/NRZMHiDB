# Verification: Application configuration

**Spec**: 018 · **Date**: 2026-11-08 · **Build**: `17bb7a1` · **Host**: ScreenshotTests
SpecVerificationHost (`http://localhost:9899`, catalog `HaemophilusWeb_Screenshots`, `host.ready` written 13:49:26 and
unchanged at the end) · **Accounts used**: `Standard1`, `Admin1`, `Rki1`, `Labor1`, no account · **Scope**: full

## Summary

| Verdict | Count |
| --- | --- |
| Match | 19 |
| Spec deviation | 0 |
| Defect confirmed | 5 |
| New defect | 0 |
| Not verifiable | 1 |

By primary path: Browser 15 · File 1 · DB 4 · Unit test (existing) 0 · Unit test (added) 2 · Unit test (scratch) 0 ·
Code 2 · Not verifiable 1 (25 rows). Also test-asserted: 12 rows with a browser, file or DB verdict. Test run:
`vstest.console.exe … /TestCaseFilter:"(…Controllers.ConfigurationControllerTests|…Services.ConfigurationServiceTests|…Validators.ConfigurationViewModelValidatorTests|…Automapper.IsolateViewModelMappingActionTests|…Automapper.MeningoIsolateViewModelMappingActionTests|…Controllers.ReportControllerTests|…Controllers.MeningoReportControllerTests)&TestCategory!=Integration"`
→ 108 passed, 0 failed; full suite (`TestCategory!=Integration`) → 1284 passed, 0 failed.

Scenarios are `US<story>.<n>`; edge cases are numbered as the list stood before this verification (EC1 "One page, one
save for all settings" … EC17 "Architecture"). FR-018 is a scope statement and has no row. Assumption A6 (changes to
the application settings file need a restart) and A7 (consumers named only) claim nothing to verify.

No external service was contacted. Each report page visit in the browser posted `/Report/QueryHealthOffice/` on load
(also the Meningococci page); the route block aborted all of them (5 blocked requests in total, all
`POST http://localhost:9899/Report/QueryHealthOffice/`). Report data and signer lists were otherwise read from the
page source with `page.request.get`, which runs no page script. The three generated documents had their
`ReportGenerated` confirmation aborted by a page route; `ReportStatus`/`ReportDate` of isolate 1 (KL001/20) and of
MZ004/20 (seeded `SV-Seed-Positive`) stayed 0/NULL. No Geonames request was made. All page routes were removed at the
end.

Temporary changes to seeded data, all through the page and all reverted: the five settings were changed for the
checks below and saved back to their seeded values ("Dr. Jane Doe", "Dr. John Doe"; empty announcement without
dates; "PD Dr. rer. nat. Vorname Nachname"; "PD Dr. med. Vorname Nachname"; the three placeholder contact blocks). A
read-only query of `ConfigurationEntries` before the first change and after the last restore returned identical
rows.

## Results

| ID | Spec says | App/test shows | Verdict | Path | Evidence |
| --- | --- | --- | --- | --- | --- |
| FR-001 · US1.1 | "Konfiguration" is the first Administration entry for Standard User and Administrator, not offered to `RKI` | `Standard1`, `Labor1` and `Admin1`: "Konfiguration" first, then a divider; `Rki1` has no "Administration" menu | Match | Browser | navigation of each account |
| FR-002 · US4.2 · US4.3 · SC-005 | Only Standard User admitted; Public Health refused; no account sent to sign in | `Rki1` and no account: `/Configuration`, `/Configuration/Index`, `/Configuration/Edit` and the save post answer 302 to `/Account/Login?ReturnUrl=…`; `Labor1` (all roles) gets the page | Match | Browser | `verification/018-FR-002-anonymous.png` |
| EC16 · DEC-001 | Administrator sees the entry but is refused [SUSPECTED-DEFECT] | `Admin1` sees "Konfiguration"; all four addresses (GET and save post) answer 302 to the sign-in page | Defect confirmed | Browser | `verification/018-FR-002-Admin1.png` |
| FR-003 · US1.2 · US2.1 · US3.1 · US4.1 · EC15 | Page with seven fields, hints, sections, "Speichern"/"Abbrechen"; "Abbrechen" to home, nothing stored, no prompt | Title "Konfiguration" (`h1`), labels "Unterzeichner", "Mitteilung", "Gültig von", "Gültig bis", "Gesamtleitung", "Ärztliche Leitung", "Kontaktdaten", the three hints verbatim, sections "Mitteilung im Befund" and "Leitung und Kontakte"; date fields with date picker, values shown as "08.11.2026"; "Abbrechen" links to `/`, a changed "Gesamtleitung" was not stored, no `beforeunload` handler, no dialog on "Abbrechen" or on a menu link | Match | Browser | `verification/018-FR-003-page.png`; also test-asserted: `Controllers/ConfigurationControllerTests.EditGet_ShowsStoredValues` |
| FR-004 · US1.3 · US1.6 · EC4 · SC-001 | One signer per non-blank line, trimmed, in order, duplicates kept | "␣␣SV-018-Signer A␣␣", empty line, blank line, "SV-018-Signer B", "SV-018-Signer A" stored as `["SV-018-Signer A","SV-018-Signer B","SV-018-Signer A"]`, shown one per line | Match | Browser | `verification/018-FR-004-saved.png`; SQL; also test-asserted: `ConfigurationControllerTests.EditPost_StoresSignersPerNonBlankLineTrimmedInOrderWithDuplicates`, `Services/ConfigurationServiceTests.SetReportSigners_TrimsAndDropsBlankEntries` |
| US1.5 · EC5 · DATA-027 | Empty list accepted without warning; afterwards every report attempt fails | Empty field saved with "Konfiguration gespeichert.", stored `[]`; both report pages offer only "Auswählen..." and answer "Befund erstellen" (template chosen) with "Fehler Bitte wählen Sie eine Befundvorlage und einen Unterzeichner aus." | Defect confirmed | Browser | `verification/018-US1-5-Report.png`, `verification/018-US1-5-MeningoReport.png` |
| FR-005 · US3.3 | Blocks split at empty or blank lines, trimmed at their ends, inner breaks kept, empty blocks dropped, order kept; shown separated by one empty line | Input with a leading empty line, a blank separator line and three empty lines stored as `["SV-018 Kontakt A  \nTelefon: 0000/00-00000","SV-018 Kontakt B\nZeile 2","SV-018 Kontakt C"]` (trailing spaces inside a block kept); page shows the blocks separated by one empty line | Match | DB | SQL; `verification/018-US3-saved.png`; also test-asserted: `ConfigurationControllerTests.EditPost_StoresContactBlocksSplitAtBlankLines` |
| FR-006 · US3.2 | Director names trimmed; empty accepted without message | "␣␣␣SV-018 Gesamtleitung␣␣␣" and "␣␣SV-018 Aerztliche Leitung␣" stored trimmed | Match | Browser | `verification/018-US3-saved.png`; also test-asserted: `ConfigurationControllerTests.EditPost_StoresDirectorNamesTrimmedAndAcceptsEmpty` (empty value) |
| FR-007 · US2.4 | Text trimmed, line breaks unified, stored with both dates | Text with leading blank line and trailing spaces stored as `"SV-018 Mitteilung Zeile 1\nSV-018 Mitteilung Zeile 2"` with `Start`/`End` `2026-11-08T00:00:00` | Match | DB | SQL; also test-asserted: `ConfigurationControllerTests.EditPost_StoresAnnouncementTrimmedWithUnifiedLineBreaksAndDates` |
| FR-008 · US2.2 · US2.3 · EC1 · EC8 · SC-002 | Text needs both dates; end not before start, also without text; browser blocks end before start; nothing stored on failure | Text without dates: the browser submits, the page returns with "Bitte ein Startdatum für die Mitteilung angeben." and "Bitte ein Enddatum für die Mitteilung angeben." and keeps the entered values; only "Gültig bis" given: only the start message. End before start: the browser shows "Das Enddatum darf nicht vor dem Startdatum liegen." and sends no request; the same post sent with the page's token (with and without text) returns the page with that message. Signers and "Gesamtleitung" changed in the same failed saves were not stored (SQL) | Match | Browser | `verification/018-FR-008-missing-dates.png`, `verification/018-FR-008-end-before-start-browser.png`; also test-asserted: `Validators/ConfigurationViewModelValidatorTests.Validate_WithAMaximalInvalidModel_ReturnsAllInvalidFields` (3 cases), `ConfigurationControllerTests.EditPost_InvalidModel_ShowsPageAgainAndStoresNothing`, `ConfigurationServiceTests.AnnouncementIsActiveOn_WithoutDates_IsActive` (EC8) |
| FR-009 · US1.3 | Page shown again with stored values and "Konfiguration gespeichert.", which fades out | Save answers 302 to `/Configuration/Edit`; the page shows the stored values and "Konfiguration gespeichert.", hidden after about five seconds; a reload shows no message | Match | Browser | `verification/018-FR-004-saved.png`; also test-asserted: `ConfigurationControllerTests.EditPost_Success_StoresFiveSettingsAndRedirectsWithMessage` |
| FR-010 · US4.4 | Save without anti-forgery token rejected, nothing stored | `Standard1` and `Labor1` posts without token: HTTP 500 with the framework message 'The required anti-forgery form field "__RequestVerificationToken" is not present.'; values "SV-018-NoToken"/"SV-018-Refused" not stored | Match | Browser | SQL; also test-asserted: `ConfigurationControllerTests.EditPost_RequiresAntiForgeryToken` |
| FR-011 · EC12 · EC13 | Five settings in the database, one shared set, effective without restart | `ConfigurationEntries` holds exactly the five keys `ReportSigners`, `Announcement`, `LabDirector`, `MedicalDirector`, `Contacts` (JSON values); every save was reflected on both report pages at the next request, `host.ready` unchanged (no restart) | Match | DB | SQL; report page source |
| FR-012 · US1.4 · EC6 | Report pages offer "Auswählen..." and the stored signers in stored order | Both report pages offered "Auswählen...", "SV-018-Signer A", "SV-018-Signer B", "SV-018-Signer A"; the chosen signer is printed in the document body | Match | Browser | report page source; `verification/SV-018-Hi.docx`; also test-asserted: `Controllers/ReportControllerTests.Isolate_ConfiguredReportSigners_AddsSignersToViewBag`, `Controllers/MeningoReportControllerTests.Isolate_UsesMeningococciTemplateFolderAndConfiguredSigners` |
| FR-013 · US2.5 · US2.6 · EC7 · EC9 · SC-003 | Announcement carried only on days within the period (both inclusive) and when not blank | Report data of both pathogens on 2026-11-08: period 08.11.–08.11. and 07.11.–09.11. carry the text (`HasAnnouncement` true); 01.11.–07.11. and 09.11.–30.11. carry `""`; blank text with dates is stored as `""` with both dates and carries nothing. Documents: the text appears in the body of both generated reports and is absent from a report generated while inactive. A page left open across the period end (EC7) was not exercised | Match | Browser | report page source; `verification/SV-018-Hi.docx`, `verification/SV-018-Nm.docx`, `verification/SV-018-Hi-no-announcement.docx`; also test-asserted: `ConfigurationServiceTests.AnnouncementIsActiveOn_PeriodBoundariesAreInclusive` (5 cases), `ConfigurationControllerTests.EditPost_StoresDatesWithoutText` |
| FR-014 · US3.4 · US3.5 · SC-004 | Report data carry both director names and the contact blocks joined by one empty line; templates print them in the headers | Report data of both pathogens: `LabDirector`, `MedicalDirector` and `Contacts` = blocks joined by `\n\n`. "BLNAS v10.docx" (KL001/20) and "Untersuchungsbefund v2.docx" (MZ004/20) headers: "Gesamtleitung: SV-018 Gesamtleitung", "ärztliche Leitung: SV-018 Aerztliche Leitung" and the three blocks with their inner line breaks, right after the save | Match | File | `verification/SV-018-Hi.docx`, `verification/SV-018-Nm.docx`; also test-asserted: `Automapper/IsolateViewModelMappingActionTests.ProcessModelToViewModel_AppliesReportConfiguration`, `Automapper/MeningoIsolateViewModelMappingActionTests.ProcessModelToViewModel_AppliesReportConfiguration` (3 cases each) |
| FR-015 · EC10 | Missing settings created with placeholder defaults at start | The freshly seeded catalog holds the five settings with the placeholders of the migration seed: two example signers, two placeholder directors, three contact blocks with "Telefon: 0931/31-XXXXX", announcement `{"Text":"","Start":null,"End":null}` | Match | DB | SQL before the first change |
| SC-006 | Existing settings keep their values at a later start | Needs an application restart after a change; a host restart reseeds the catalog, and the seed method works on the real database context, so no unit test can run it | Not verifiable | Not verifiable | — |
| FR-016 · EC3 · REF-014 | Save overwrites without history, audit or concurrency check | Page open in two tabs: tab B saved new directors, tab A then saved and silently replaced both, including "Ärztliche Leitung" changed only in tab B; no warning. `ConfigurationEntries` has only `ConfigurationEntryId`, `Key`, `Value`; no history or audit table; no log entry for saves | Defect confirmed | Browser | `verification/018-FR-016-last-save-wins.png`; `sys.tables`; `Logs/2026-11-08.txt` |
| EC2 · REF-014 | Settings written one by one, no combined undo | `ConfigurationService.Set` calls `db.SaveChanges()` per key; `ConfigurationController.Edit` (POST) calls five setters in sequence | Defect confirmed | Code | `Services/ConfigurationService.cs`, `Controllers/ConfigurationController.cs` |
| EC11 | Unreadable stored value treated as empty without error | Every getter returns its empty fallback for a value that is not JSON | Match | Unit test (added) | `ConfigurationServiceTests.Get_UnreadableStoredValue_ReturnsEmptySettings` |
| EC14 | Settings also read when an isolate is opened for editing, not displayed | `/Isolate/Edit/1` and `/MeningoIsolate/Edit/4` contain none of the saved "SV-018" values; the report-data mapping that reads them is the one the isolate forms use | Match | Browser | page source; also test-asserted: `IsolateViewModelMappingActionTests.ProcessModelToViewModel_AppliesReportConfiguration` |
| EC17 · REF-003 | Services created directly, static test hook | `ConfigurationController` and `ReportControllerBase.AddReportSignersToViewBag` create `new ConfigurationService(db)`; `IsolateViewModelMappingActionBase` creates its own database context unless `DbForTest` is set ("TODO Ugly quick fix") | Defect confirmed | Code | files named |
| TEST-001 (configuration page) | The configuration page has no automated tests | Ten tests added for the page controller; six for the report data of both pathogens; seven for the service | Match | Unit test (added) | see "Unit tests added" |
| FR-017 · "Settings not editable in the application" | All other settings only in the settings file, not shown or editable | The page shows only the seven fields; the deployed settings file holds exactly the 15 keys of the table (counted by name, values not read); the "Beta" banner "Achtung: Änderungen werden nicht zentral gespeichert! …" is shown on every page | Match | Browser | `verification/018-FR-003-page.png`; key count |

## Unit tests added

23 test cases in 15 methods.

- `HaemophilusWebTests/Controllers/ConfigurationControllerTests.cs` (new file, added to `HaemophilusWebTests.csproj`):
  `Index_RedirectsToEdit` (US4.5), `EditGet_ShowsStoredValues` (FR-003), `EditPost_StoresSignersPerNonBlankLineTrimmedInOrderWithDuplicates`
  (FR-004), `EditPost_StoresContactBlocksSplitAtBlankLines` (FR-005), `EditPost_StoresDirectorNamesTrimmedAndAcceptsEmpty`
  (FR-006), `EditPost_StoresAnnouncementTrimmedWithUnifiedLineBreaksAndDates` (FR-007), `EditPost_StoresDatesWithoutText`
  (EC9), `EditPost_InvalidModel_ShowsPageAgainAndStoresNothing` (FR-008), `EditPost_Success_StoresFiveSettingsAndRedirectsWithMessage`
  (FR-009), `EditPost_RequiresAntiForgeryToken` (FR-010).
- `HaemophilusWebTests/Services/ConfigurationServiceTests.cs`: `AnnouncementIsActiveOn_PeriodBoundariesAreInclusive`
  (5 cases, FR-013), `AnnouncementIsActiveOn_WithoutDates_IsActive` (EC8), `Get_UnreadableStoredValue_ReturnsEmptySettings`
  (EC11).
- `HaemophilusWebTests/Automapper/IsolateViewModelMappingActionTests.cs`: `ProcessModelToViewModel_AppliesReportConfiguration`
  (3 cases, FR-013, FR-014).
- `HaemophilusWebTests/Automapper/MeningoIsolateViewModelMappingActionTests.cs`: `ProcessModelToViewModel_AppliesReportConfiguration`
  (3 cases, FR-013, FR-014).

No test asserts the empty signer list (DATA-027), the role rule (DEC-001) or the five separate writes (REF-014).

## Spec corrections applied

- US1.5: saving an empty signer list shows the usual "Konfiguration gespeichert." and no warning (was: "no message is
  shown"); the report page's message is quoted.
- EC16 (Authorization): confirmation of DEC-001 and the redirect of refused accounts to the sign-in page recorded.
- EC17 (Architecture): coverage note updated to the tests added in this verification (was: "the configuration page
  itself has none").
- Assumptions A1, A3, A4, A5 updated to the observed behavior (running system verified; anti-forgery refusal with
  HTTP 500, sign-in redirect and date fields observed; placeholder defaults observed on a fresh catalog, a later start
  not exercised; values printed in body and headers of two named templates).

## Backlog updates

- DEC-001: observed for the configuration page (no decision).
- DATA-027: confirmed on system 2026-11-08 (018 verification).
- REF-014: confirmed on system 2026-11-08 (last save wins across two tabs, no history or audit, five writes).
- REF-003: configuration service and mapping database context named (018 verification).
- TEST-001: configuration page covered by tests added in the 018 verification.

## Not verifiable here

- SC-006 (and the second half of FR-015): that a later application start keeps changed settings needs an application
  restart after a change without reseeding; the host reseeds on every start, and the seed method needs a real
  database context. A host option that restarts IIS Express without reseeding, or a test against a LocalDB catalog,
  would make it verifiable.

## Open questions for the owner

- None beyond the open decision DEC-001.
