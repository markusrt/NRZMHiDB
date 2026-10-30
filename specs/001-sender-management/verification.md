# Verification: Sender management (shared across both databases)

**Spec**: 001 · **Date**: 2026-10-30 · **Build**: `001be49` · **Host**: ScreenshotTests
SpecVerificationHost (`http://localhost:9899`, catalog `HaemophilusWeb_Screenshots`) ·
**Accounts used**: `Standard1` (all laboratory flows), `Labor1`, `Admin1`, `Rki1` (role checks) ·
**Scope**: full

## Summary

| Verdict | Count |
| --- | --- |
| Match | 17 |
| Spec deviation | 5 |
| Defect confirmed | 1 |
| New defect | 0 |
| Not verifiable | 1 |

By primary path: Browser 19 · File 2 · DB 1 · Unit test (existing) 0 · Unit test (added) 1 · Not verifiable 1
(adds up to the row count). Also test-asserted: 8 rows with a browser or DB verdict. Test run:
`vstest.console.exe … /Tests:SenderControllerTests,SenderTests` → 22 passed, 0 failed (2 existing, 20 added);
full suite, five runs → four times 621 passed, 0 failed; once 620 passed, 1 failed (test not captured, did not
recur; the sender fixtures passed in every run).

Edge cases are numbered in the order of the spec's Edge Cases list: EC1 missing required fields, EC2 malformed
contact fields, EC3 missing id, EC4 deleting a sender with active submissions, EC5 delete/restore bypasses field
validation, EC6 editing a deleted sender restores it, EC7 Meningococci sender maintenance.

## Results

| ID | Spec says | App/test shows | Verdict | Path | Evidence |
| --- | --- | --- | --- | --- | --- |
| FR-001 · US1.3 · SC-002 | One shared directory; a sender is selectable in both intake workflows | Haemophilus and Meningococci "Einsendung erfassen" offer the same options, e.g. "#002: SV-001-Alpha (SV-001 Abteilung Bakteriologie) - 97080 Würzburg" | Match | Browser | `verification/US1-3-haemophilus-sender-select.png` |
| FR-002 | List with Name, Abteilung, Ort, Telefon #1, E-Mail; paged, sortable, searchable; create action | Columns as stated; sorted by Name ascending, toggles to descending; "Suchen" box filters; paging "Zurück"/"1"/"Nächste"; rows offer only "Bearbeiten"; page size 8 is in the view code but was not exercised (3 senders) | Match | Browser | `verification/US1-list.png` |
| FR-003 · US1.1 · SC-001 | "Neuen Einsender anlegen" with ten fields; saved, back to the list, sender listed | `SV-001-Alpha` created with all ten fields; redirect to `/Sender`; row found via the search box | Match | Browser | `verification/US1-1-create-filled.png`, `verification/US1-1-created-in-list.png`; DB row id 2 with all fields |
| FR-004 · EC1 | Name and Telefon #1 required; rejected, redisplayed, nothing persisted | Browser blocks submit with "Das Feld "Name" ist erforderlich." / "Das Feld "Telefon #1" ist erforderlich."; a direct POST redisplays the form with the same messages; no row stored | Match | Browser | `verification/FR-004-empty-client.png`; also test-asserted (added): `Models/SenderTests.Validate_NameAndPhone1Only_IsValid`, `Validate_WithoutName_FailsOnName`, `Validate_WithoutPhone1_FailsOnPhone1` |
| FR-005 · EC2 | Malformed e-mail, phone or fax rejected on save | E-mail rejected in the browser before submit; "abc"/"xyz"/"fax!" pass the browser and are rejected by the server ("Das Feld Telefon #1 enthält keine gültige Telefonnummer.", same for Telefon #2 and Fax); well-formed values such as "+49 (931) 12345-1" accepted; no row stored | Spec deviation | Browser | `verification/FR-005-malformed-client.png`; DB; also test-asserted (added): `Models/SenderTests.Validate_WellFormedPhoneAndFax_IsValid`, `Validate_MalformedPhoneAndFax_FailsOnEachField`, `Validate_WellFormedEmail_IsValid`, `Validate_MalformedEmail_FailsOnEmail` |
| FR-006 · US1.2 | Edit, save → persisted, back to the list | "Einsender bearbeiten" prefilled; Abteilung changed, Telefon #2 cleared; redirect to `/Sender`; DB updated (Phone2 NULL) | Match | Browser | `verification/US1-2-edit.png` |
| FR-007 | Read-only details view with seven fields | `/Sender/Details/2` shows the seven fields, but no page links to it and its headings and links are English ("Details", "Sender", "Edit", "Back to List") | Spec deviation | Browser | `verification/FR-007-details.png` |
| FR-008 · US2.1 | Soft delete; deleted senders not listed and not offered | `SV-001-Overpost` deleted via "Eintrag löschen" (edit page) → "Löschen bestätigen" → "Eintrag löschen"; gone from the list and from both create forms; DB `Deleted = 1` | Match | Browser | `verification/US2-1-delete-confirm.png`; also test-asserted (added): `Controllers/SenderControllerTests.IndexAndDeleted_SplitSendersByDeletedFlag`, `DeleteConfirmed_InvalidSender_SetsDeletedFlagAndSavesWithoutValidation` |
| FR-009 · US2.2 · EC4 | Warning listing active submissions of both databases with reassignment links; deletion still possible | Alert "Dem Einsender "Nationales Referenzzentrum" sind noch Einsendungen zugeordnet.", links "Einsendung KL001/20" → `/PatientSending/Edit/1` and "Einsendung MZ001/20" → `/MeningoPatientSending/Edit/1`; after soft-deleting KL001/20 only MZ001/20 is listed; confirming deletes the sender | Match | Browser | `verification/US2-2-delete-warning.png`; also test-asserted (existing): `Controllers/SenderControllerTests.Delete_AddsExistingSendingsToViewBag`; (added): `Delete_SenderWithSendingsInBothDatabases_ListsOnlyNotDeletedSendingsOfBoth` |
| FR-010 · US2.4 · SC-003 | Deletion keeps existing attributions | With sender 1 deleted, both submission edit pages still show "#001: Nationales Referenzzentrum …" selected (option kept for the current value); DB `SenderId` unchanged | Match | Browser | `verification/US2-4-existing-sending-after-delete.png`; DB |
| FR-011 · US2.3 | "Gelöschte Einsender" (Name, Postleitzahl, Ort) with "Wiederherstellen"; restored by a standard user | Page lists exactly these columns and a green "Wiederherstellen" per row; sender 1 restored by `Standard1`, redirect to `/Sender`, back in the list; `Admin1` sees the menu entry but is redirected to sign-in | Match | Browser | `verification/FR-011-deleted-list.png`, `verification/US2-3-restored.png`; also test-asserted (added): `IndexAndDeleted_SplitSendersByDeletedFlag`, `Undelete_InvalidDeletedSender_ClearsDeletedFlagAndSavesWithoutValidation` |
| FR-012 · EC5 | Delete and restore saved without field validation | No invalid sender can be created through the UI; the controller saves inside `PerformWithoutSaveValidation` for a sender without Name and Telefon #1, both when deleting and restoring | Match | Unit test (added) | `Controllers/SenderControllerTests.DeleteConfirmed_InvalidSender_SetsDeletedFlagAndSavesWithoutValidation`, `Undelete_InvalidDeletedSender_ClearsDeletedFlagAndSavesWithoutValidation` |
| FR-013 · US3.2 · US3.3 | Per-database spreadsheet with sender fields, count, stem and laboratory numbers, named for the database | Columns Einsendernummer … Bemerkung, "Anzahl Einsendungen", "Stammnummern", "Labornummern"; 2020: Haemophilus file lists sender 1 with 1, 1, KL001/20, Meningococci file with 1, 1, MZ001/20; names `Einsender-<Haemophilus\|Meningokokken>-Export_<yyyyMMdd>-<yyyyMMdd>.xlsx` | Match | File | `verification/Einsender-Haemophilus-Export-20200101-20200101.xlsx`, `verification/Einsender-Meningokokken-Export-20200101-20201231.xlsx` |
| FR-014 · US3.1 | Export without range defaults to the previous calendar year | Both export pages ("Export der Einsender (Haemophilus)" / "(Meningokokken)", heading "Entnahmedatum der Einsendung(en)") prefill "Start-Datum" 01.01.2025, "Ende-Datum" 31.12.2025 | Match | Browser | `verification/US3-1-export-default.png`; also test-asserted (added): `Controllers/SenderControllerTests.Export_WithoutRange_DefaultsToPreviousCalendarYear` |
| FR-015 (selection) · SC-006 | Sampling date if present, else receiving date; deleted submissions excluded from the selection | Submission sampled 01.01.2020, received 03.01.2020: range 01.01.–01.01.2020 lists the sender, range 02.01.–31.12.2020 does not; after soft-deleting KL001/20 the 2020 Haemophilus export has no rows (KL001/20 restored afterwards) | Match | File | `verification/Einsender-Haemophilus-Export-20200101-20200101.xlsx`, `…-20200102-20201231.xlsx`, `…-20200101-20201231.xlsx` (the last one is the run after the soft delete) |
| FR-015 (values) | Count and number lists include soft-deleted submissions (DEC-002) | Needs a sender with one deleted and one active submission in the range; the seed has one submission per database | Not verifiable | Not verifiable | — |
| FR-016 | All sender actions require the standard laboratory-user role | `Standard1`, `Labor1`: 200 on all sender addresses; `Admin1`, `Rki1` and anonymous requests: redirect to `/Account/Login?ReturnUrl=…` for list, create, edit, delete, details, deleted list and both exports | Match | Browser | `verification/FR-016-Admin1-sender.png`, `verification/FR-016-Rki1-sender.png` |
| FR-017 | Create and edit bind only the editable fields | POST with `Deleted=true` to create and to edit stored `Deleted = 0` | Match | DB | Browser POST then DB query; also test-asserted (added): `Controllers/SenderControllerTests.PostActions_BindEditableFieldsOnly` |
| FR-018 · EC3 | Details, edit, delete: missing id → bad request, unknown id → not found | 400 / 404 for details, edit, delete and also restore | Spec deviation | Browser | HTTP status via request; also test-asserted (added): `Controllers/SenderControllerTests.SingleSenderActions_WithoutId_ReturnBadRequest`, `SingleSenderActions_WithUnknownId_ReturnNotFound` |
| FR-019 | Postal-code lookup fills the city; warning before leaving with unsaved changes | Typing "97080" filled Ort "Würzburg" (Geonames); leaving a changed form raised a `beforeunload` dialog | Match | Browser | `verification/FR-019-postal-lookup.png` |
| EC6 · DATA-007 | (020, backlog) Saving the edit page of a deleted sender restores it | Edit page of the deleted `SV-001-Overpost` opened by address, Bemerkung changed, saved → sender back in the active list | Defect confirmed | Browser | `verification/DATA-007-edit-restores-deleted-sender.png` |
| EC7 | Only the export is linked for Meningococci; maintenance via the Haemophilus area | Correct, and `/MeningoSender`, `/Create`, `/Edit/2`, `/Details/2`, `/Delete/2`, `/Deleted` return HTTP 500 "The view '…' or its master was not found" | Spec deviation | Browser | HTTP status via request |
| SC-004 | "count and links" of the affected submissions | Links only, no count | Spec deviation | Browser | `verification/US2-2-delete-warning.png` |
| Assumption (navigation) · DEC-001 | "Einsender" and "Gelöschte Einsender" shown to standard-user or administrator roles | `Standard1`, `Labor1`, `Admin1` see both; `Rki1` sees neither (only its two RKI exports); `Admin1` is refused by every sender address | Match | Browser | `verification/FR-016-Admin1-sender.png` |

SC-005 (invalid input never persisted) has no row of its own; FR-004 and FR-005 cover it.

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

- FR-005, EC2: phone/fax formats are checked only on the server (was: silent on where; the browser checks only
  e-mail and the required fields).
- FR-018, EC3: restore included (was: details, edit, delete).
- FR-007, FR-002: details view is unlinked English scaffolding; rows offer only "Bearbeiten" and deletion starts on
  the edit page (was: details view described as a regular page).
- SC-004: links, no count (was: "count and links").
- EC7, Assumptions: `MeningoSender` CRUD addresses end in HTTP 500 (was: "technically expose the same actions").
- New EC6 "Editing a deleted sender restores it" (`[SUSPECTED-DEFECT]`, DATA-007).

## Backlog updates

- DATA-007: source 001 added; confirmed on system 2026-10-30 for senders.
- REF-008: extended with the HTTP 500 Meningococci CRUD addresses (confirmed on system) and the unlinked details
  scaffolding.
- DEC-001: observation for the sender pages recorded (no decision taken).

## Seed data changed and restored

- Sender 1 ("Nationales Referenzzentrum") deleted and restored through the UI (FR-009, FR-010, FR-011).
- Haemophilus submission KL001/20 soft-deleted and restored by its restore address (FR-015 selection).
- Own records left behind: `SV-001-Alpha` (active), `SV-001-Overpost` (active again after the DATA-007 check).

## Not verifiable here

- FR-015 (values, DEC-002): needs a sender with a deleted and an active submission in the same range. Creating a
  second submission belongs to the 002/003 flows; better checked in the 013 verification or with a seed extension.

## Open questions for the owner

- None that block this spec. DEC-001 and DEC-002 stay open.

> Report converted on 2026-10-30 to the revised format of `/spec-verify` (commit `b02d4c1`): one row per FR,
> primary evidence path per row. Verdicts are unchanged.
