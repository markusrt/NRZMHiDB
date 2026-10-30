# Verification: Sender management (shared across both databases)

**Spec**: 001 · **Date**: 2026-10-30 · **Build**: `001be49` · **Host**: ScreenshotTests
SpecVerificationHost (`http://localhost:9899`, catalog `HaemophilusWeb_Screenshots`) ·
**Accounts used**: `Standard1` (all laboratory flows), `Labor1`, `Admin1`, `Rki1` (role checks) ·
**Scope**: full

## Summary

| Verdict | Count |
| --- | --- |
| Match | 35 |
| Spec deviation | 7 |
| Defect confirmed | 1 |
| New defect | 0 |
| Not verifiable | 1 |

By evidence path (a row may use several paths, so the paths add up to more than the 44 rows): Browser 36 ·
Unit test (existing) 1 · Unit test (added) 14 · File 5 · DB 7 · Not verifiable 1.

Test run: `vstest.console.exe HaemophilusWebTests\bin\Debug\HaemophilusWebTests.dll
/Tests:SenderControllerTests,SenderTests` → 22 passed, 0 failed (2 existing, 20 added). Full suite, five runs:
four times 621 passed, once 620 passed and 1 failed; the failing test was not captured and did not recur. The
sender fixtures use their own non-static data sets and passed in every run.

## Results

| ID | Spec says | App/test shows | Verdict | Evidence |
| --- | --- | --- | --- | --- |
| US1.1 | "Neuen Einsender anlegen", Name + Telefon #1, save → back to list, sender listed | `SV-001-Alpha` created with all ten fields; redirect to `/Sender`; row found via the search box | Match | `verification/US1-1-create-filled.png`, `verification/US1-1-created-in-list.png`; DB row id 2 |
| US1.2 | Edit, save → persisted, back to list | "Einsender bearbeiten" prefilled; Abteilung changed, Telefon #2 cleared; redirect to `/Sender`; DB updated (Phone2 NULL) | Match | `verification/US1-2-edit.png` |
| US1.3 | Same sender selectable in both intake workflows | Haemophilus and Meningococci "Einsendung erfassen" offer the same options, e.g. "#002: SV-001-Alpha (SV-001 Abteilung Bakteriologie) - 97080 Würzburg" | Match | `verification/US1-3-haemophilus-sender-select.png` |
| US2.1 | Delete sender without submissions → flagged, off list, not offered | `SV-001-Overpost` deleted via "Eintrag löschen" (edit page) → "Löschen bestätigen" → "Eintrag löschen"; gone from list and both create forms; listed under "Gelöschte Einsender" | Match | `verification/US2-1-delete-confirm.png`, `verification/FR-011-deleted-list.png` |
| US2.2 | Warning listing submissions of both databases with reassignment links; deletion still possible | Alert "Dem Einsender "Nationales Referenzzentrum" sind noch Einsendungen zugeordnet.", links "Einsendung KL001/20" → `/PatientSending/Edit/1`, "Einsendung MZ001/20" → `/MeningoPatientSending/Edit/1`; confirming deletes the sender | Match | `verification/US2-2-delete-warning.png` |
| US2.3 | Standard user restores via "Wiederherstellen"; Administrator sees the entry but is refused | Sender 1 restored by `Standard1`, redirect to `/Sender`, back in list; `Admin1` sees "Gelöschte Einsender" and is redirected to sign-in | Match | `verification/US2-3-restored.png`, `verification/FR-016-Admin1-sender.png` |
| US2.4 | Existing submissions stay attributed to the deleted sender | With sender 1 deleted, both submission edit pages still show "#001: Nationales Referenzzentrum …" selected (option kept for the current value); DB `SenderId` unchanged | Match | `verification/US2-4-existing-sending-after-delete.png`; DB |
| US3.1 | Export without range defaults to previous calendar year | Both export pages ("Export der Einsender (Haemophilus)" / "(Meningokokken)", heading "Entnahmedatum der Einsendung(en)") prefill "Start-Datum" 01.01.2025, "Ende-Datum" 31.12.2025 | Match | `verification/US3-1-export-default.png` |
| US3.2 | Spreadsheet of senders with matching submissions, sender details + count, stem and lab numbers | Columns Einsendernummer … Bemerkung, "Anzahl Einsendungen", "Stammnummern", "Labornummern"; 2020 range: one row, sender 1, 1, 1, KL001/20 | Match | File: `Einsender-Haemophilus-Export_20200101-20201231.xlsx` |
| US3.3 | Each workflow scoped to its own submissions, named for its database | `Einsender-Haemophilus-Export_…` lists KL001/20; `Einsender-Meningokokken-Export_…` lists MZ001/20 | Match | File: both 2020 exports |
| EC missing required | No Name / Telefon #1 → rejected, redisplayed, nothing persisted | Browser blocks submit with "Das Feld "Name" ist erforderlich." / "Das Feld "Telefon #1" ist erforderlich."; direct POST redisplays the form with the same messages | Match | `verification/FR-004-empty-client.png`; test added: `Models/SenderTests.Validate_WithoutName_FailsOnName`, `…WithoutPhone1_FailsOnPhone1` |
| EC malformed contact | Malformed e-mail/phone/fax fails on save | E-mail rejected in the browser before submit; "abc"/"xyz"/"fax!" pass the browser and are rejected by the server ("Das Feld Telefon #1 enthält keine gültige Telefonnummer.", same for Telefon #2, Fax); no row persisted | Spec deviation | `verification/FR-005-malformed-client.png`; DB; test added: `Models/SenderTests.Validate_MalformedPhoneAndFax_FailsOnEachField`, `…MalformedEmail_FailsOnEmail` |
| EC missing id | Details/Edit/Delete: no id → bad request, unknown → not found | 400 / 404 for Details, Edit, Delete and also Undelete (restore) | Spec deviation | Browser (HTTP status); test added: `Controllers/SenderControllerTests.SingleSenderActions_WithoutId_ReturnBadRequest`, `…WithUnknownId_ReturnNotFound` |
| EC delete with submissions | Deletion permitted, sender stays attached | See US2.2 and US2.4 | Match | `verification/US2-2-delete-warning.png` |
| EC delete/restore bypass validation | Flag toggled even for an otherwise invalid record | No invalid sender can be created through the UI and the catalog is read-only for verification; controller saves inside `PerformWithoutSaveValidation` for an invalid sender (no Name, no Telefon #1) | Match | test added: `Controllers/SenderControllerTests.DeleteConfirmed_InvalidSender_SetsDeletedFlagAndSavesWithoutValidation`, `Undelete_InvalidDeletedSender_ClearsDeletedFlagAndSavesWithoutValidation` |
| EC Meningococci maintenance | Only export in navigation; CRUD via the Haemophilus area | Correct, and `/MeningoSender`, `/Create`, `/Edit/2`, `/Details/2`, `/Delete/2`, `/Deleted` return HTTP 500 "The view '…' or its master was not found" | Spec deviation | Browser (HTTP status) |
| FR-001 | One shared directory for both workflows | See US1.3 | Match | `verification/US1-3-haemophilus-sender-select.png` |
| FR-002 | List with Name, Abteilung, Ort, Telefon #1, E-Mail; paged, sortable, searchable; create action | Columns as stated; sorted by Name ascending, toggles to descending; "Suchen" box filters; paging controls "Zurück"/"1"/"Nächste"; rows offer only "Bearbeiten"; page size 8 is in the view code but was not exercised (3 senders) | Match | `verification/US1-list.png` |
| FR-003 | Create with ten fields, persisted, back to list | See US1.1; all ten fields stored | Match | DB |
| FR-004 | Name and Telefon #1 required | See EC missing required | Match | `verification/FR-004-empty-client.png`; test added: `Models/SenderTests.Validate_NameAndPhone1Only_IsValid` and the two above |
| FR-005 | E-mail and phone/fax formats validated on save | See EC malformed contact; well-formed values such as "+49 (931) 12345-1" accepted | Spec deviation | test added: `Models/SenderTests.Validate_WellFormedPhoneAndFax_IsValid`, `…WellFormedEmail_IsValid` |
| FR-006 | Edit persisted, back to list | See US1.2 | Match | `verification/US1-2-edit.png` |
| FR-007 | Read-only details view with seven fields | `/Sender/Details/2` shows the seven fields, but no page links to it and its headings/links are English ("Details", "Sender", "Edit", "Back to List") | Spec deviation | `verification/FR-007-details.png` |
| FR-008 | Soft delete; deleted senders not listed, not offered | See US2.1; DB `Deleted = 1` | Match | test added: `Controllers/SenderControllerTests.IndexAndDeleted_SplitSendersByDeletedFlag`, `DeleteConfirmed_…` |
| FR-009 | Delete confirmation with warning and links for both databases | See US2.2; after soft-deleting KL001/20 only MZ001/20 is listed | Match | test-asserted: `Controllers/SenderControllerTests.Delete_AddsExistingSendingsToViewBag`; test added: `Delete_SenderWithSendingsInBothDatabases_ListsOnlyNotDeletedSendingsOfBoth` |
| FR-010 | Deletion keeps existing attributions | See US2.4 | Match | DB |
| FR-011 | Deleted list (Name, Postleitzahl, Ort) with "Wiederherstellen" | "Gelöschte Einsender" with exactly these columns and a green "Wiederherstellen" per row; restore redirects to `/Sender` | Match | `verification/FR-011-deleted-list.png`; test added: `IndexAndDeleted_SplitSendersByDeletedFlag`, `Undelete_…` |
| FR-012 | Flag toggled without field validation | See EC delete/restore bypass | Match | test added (see EC) |
| FR-013 | Per-database export with sender fields, count, stem and lab numbers | See US3.2, US3.3; file names `Einsender-<Haemophilus\|Meningokokken>-Export_<yyyyMMdd>-<yyyyMMdd>.xlsx` | Match | File |
| FR-014 | Default range = previous calendar year | See US3.1 | Match | test added: `Controllers/SenderControllerTests.Export_WithoutRange_DefaultsToPreviousCalendarYear` |
| FR-015 (selection) | Sampling date if present, else receiving date; deleted submissions excluded from the selection | Submission sampled 01.01.2020, received 03.01.2020: range 01.01.–01.01.2020 lists the sender, range 02.01.–31.12.2020 does not; after soft-deleting KL001/20 the 2020 Haemophilus export has no rows (KL001/20 restored afterwards) | Match | File: `…_20200101-20200101.xlsx`, `…_20200102-20201231.xlsx`, `…_20200101-20201231.xlsx` |
| FR-015 (values) | Count and number lists include soft-deleted submissions (DEC-002) | Needs a sender with one deleted and one active submission in the range; the seed has one submission per database | Not verifiable | — |
| FR-016 | All sender actions require the standard laboratory-user role | `Standard1`, `Labor1`: 200 on all sender addresses; `Admin1`, `Rki1` and anonymous requests: redirect to `/Account/Login?ReturnUrl=…` for list, create, edit, delete, details, deleted list and both exports | Match | `verification/FR-016-Admin1-sender.png`, `verification/FR-016-Rki1-sender.png` |
| FR-017 | Create/edit bind only the editable fields | POST with `Deleted=true` to create and to edit stored `Deleted = 0` | Match | DB; test added: `Controllers/SenderControllerTests.PostActions_BindEditableFieldsOnly` |
| DATA-007 | (020, backlog) Saving the edit page of a deleted sender restores it | Edit page of deleted `SV-001-Overpost` opened by address, Bemerkung changed, saved → sender back in the active list | Defect confirmed | `verification/DATA-007-edit-restores-deleted-sender.png` |
| FR-018 | Details, edit, delete: 400 / 404 | Also applies to restore | Spec deviation | See EC missing id |
| FR-019 | Postal-code lookup fills city; warning before leaving with unsaved changes | Typing "97080" filled Ort "Würzburg" (Geonames); navigating away from a changed form raised a `beforeunload` dialog | Match | `verification/FR-019-postal-lookup.png` |
| SC-001 | Valid sender created and listed in one attempt | See US1.1 | Match | — |
| SC-002 | Selectable from both workflows without duplicate | See US1.3 | Match | — |
| SC-003 | Every deletion recoverable, no attribution lost | See US2.3, US2.4 | Match | — |
| SC-004 | "count and links" of affected submissions | Links only, no count | Spec deviation | `verification/US2-2-delete-warning.png` |
| SC-005 | Invalid input never persisted, actionable message | See EC missing required, EC malformed contact | Match | DB; tests added |
| SC-006 | Export rows exactly the senders with a matching non-deleted submission | See FR-015 (selection); values caveat under FR-015 (values) | Match | File |
| Assumption (navigation) | "Einsender" and "Gelöschte Einsender" shown to standard-user or administrator roles | `Standard1`, `Labor1`, `Admin1` see both; `Rki1` sees neither (only its two RKI exports) | Match | Browser |

## Unit tests added

- `HaemophilusWebTests/Models/SenderTests.cs` (new, 11 cases): `Validate_NameAndPhone1Only_IsValid`,
  `Validate_WithoutName_FailsOnName`, `Validate_WithoutPhone1_FailsOnPhone1` (FR-004),
  `Validate_WellFormedPhoneAndFax_IsValid` (4 cases), `Validate_MalformedPhoneAndFax_FailsOnEachField` (2 cases),
  `Validate_WellFormedEmail_IsValid`, `Validate_MalformedEmail_FailsOnEmail` (FR-005).
- `HaemophilusWebTests/Controllers/SenderControllerTests.cs` (extended, 9 cases):
  `Delete_SenderWithSendingsInBothDatabases_ListsOnlyNotDeletedSendingsOfBoth` (FR-009),
  `SingleSenderActions_WithoutId_ReturnBadRequest`, `SingleSenderActions_WithUnknownId_ReturnNotFound` (FR-018),
  `IndexAndDeleted_SplitSendersByDeletedFlag` (FR-002, FR-008, FR-011),
  `DeleteConfirmed_InvalidSender_SetsDeletedFlagAndSavesWithoutValidation`,
  `Undelete_InvalidDeletedSender_ClearsDeletedFlagAndSavesWithoutValidation` (FR-008, FR-011, FR-012),
  `PostActions_BindEditableFieldsOnly` (2 cases, FR-017), `Export_WithoutRange_DefaultsToPreviousCalendarYear` (FR-014).
  The new tests use a private context stub with non-static sets because `ApplicationDbContextMock` shares static
  data between all instances.

## Spec corrections applied

- Edge case "Malformed contact fields", FR-005: phone/fax formats are checked only on the server (was: silent on
  where; the browser checks only e-mail and the required fields).
- Edge case "Missing id", FR-018: restore included (was: details, edit, delete).
- FR-007, FR-002: details view is unlinked English scaffolding; rows offer only "Bearbeiten" and deletion starts on
  the edit page (was: details view described as a regular page).
- SC-004: links, no count (was: "count and links").
- Edge case "Meningococci sender maintenance", Assumptions: `MeningoSender` CRUD addresses end in HTTP 500 (was:
  "technically expose the same actions").
- New edge case "Editing a deleted sender restores it" (`[SUSPECTED-DEFECT]`, DATA-007).

## Backlog updates

- DATA-007: source 001 added; confirmed on system 2026-10-30 for senders.
- REF-008: extended with the HTTP 500 Meningococci CRUD addresses (confirmed on system) and the unlinked details
  scaffolding.
- DEC-001: confirmed on system for the sender pages (no decision taken).

## Not verifiable here

- FR-015 (values, DEC-002): needs a sender with a deleted and an active submission in the same range. Creating a
  second submission belongs to the 002/003 flows; better checked in the 013 verification or with a seed extension.
- FR-012 in the browser: needs a sender that fails validation; the UI cannot create one and the catalog is read-only
  for verification. Covered by unit tests instead.

## Open questions for the owner

- None that block this spec. DEC-001 and DEC-002 stay open.
