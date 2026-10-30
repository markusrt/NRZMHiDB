# Verification: Haemophilus patient + sending intake (PatientSending)

**Spec**: 002 · **Date**: 2026-10-30 · **Build**: `f475cea` · **Host**: ScreenshotTests
SpecVerificationHost (`http://localhost:9899`, catalog `HaemophilusWeb_Screenshots`) ·
**Accounts used**: `Standard1` (all laboratory flows), `Labor1`, `Admin1`, `Rki1` (role checks) ·
**Scope**: full

## Summary

| Verdict | Count |
| --- | --- |
| Match | 16 |
| Spec deviation | 3 |
| Defect confirmed | 5 |
| New defect | 4 |
| Not verifiable | 0 |

By primary path: Browser 24 · File 0 · DB 2 · Unit test (existing) 1 · Unit test (added) 1 · Not verifiable 0
(adds up to the row count). Also test-asserted: 12 rows with a browser or DB verdict. Test run:
`vstest.console.exe … /Tests:PatientSendingControllerTests,SendingValidatorTests,PatientValidatorTests,SendingControllerTests`
→ 52 passed, 0 failed (35 existing, 17 added); full suite → 638 passed, 0 failed.

Edge cases are numbered in the order of the spec's Edge Cases list before this verification: EC1 missing required
patient fields, EC2 missing required sending fields, EC3 receiving before sampling, EC4 "other" sampling location
without free text, EC5 duplicate patient (one match), EC6 duplicate patient (two or more matches), EC7 delete/restore
bypasses validation, EC8 sender availability, EC9 laboratory number reassignment. Edge cases added by this
verification are named in the rows.

## Results

| ID | Spec says | App/test shows | Verdict | Path | Evidence |
| --- | --- | --- | --- | --- | --- |
| FR-001 · US1 | One combined form captures sending and patient data | "Einsendung erfassen" holds the sending fields ("Einsendernummer" … "Serotyp Einsender"), the patient fields ("Initialen" … "Datum Hib-Impfung") and "Bemerkung"; one "Neu anlegen" saves both | Match | Browser | `verification/FR-002-create-defaults.png` |
| FR-002 · US1.1 | Defaults: receiving today, sampling today − 7, species "H. influenzae", Hib "not stated" | "Eingangsdatum" 30.10.2026, "Entnahmedatum" 23.10.2026, "Spezies Einsender" "H. influenzae", "Hib-Impfung" "keine Angabe"; also preselected: "Entnahmeort" "Blut", "Material" "Isolat", "Bundesland" "n.n." | Match | Browser | `verification/FR-002-create-defaults.png`; also test-asserted (added): `Controllers/PatientSendingControllerTests.Create_NewPatientSending_PrefillsDefaults` |
| FR-003 · US1.2 · SC-001 · Assumption (identifiers) | Save links sender and patient and assigns a stem number and a laboratory number | Two submissions for `SV-001-Alpha` sampled 2020 got KL001/26 and KL002/26 (year of saving), linked to sender 2 and new patients; "Stammnummer" stays empty ("H -" in the list, DB `StemNumber` NULL); automatic stem numbers are disabled for every submission type | Spec deviation | Browser | `verification/US1-2-saved-first.png`; DB; also test-asserted (existing): `Controllers/SendingControllerTests.AssignStemNumber_NoIsolate_AssignsCorrectLaboratoryNumber`; (added): `Controllers/PatientSendingControllerTests.Create_ValidModel_LinksSenderAndPatientAndAssignsLaboratoryNumberButNoStemNumber` |
| FR-004 · EC8 | Sender choice: active senders plus the one attached to the submission | With `SV-002-Beta` soft-deleted, the create form offers #001–#003; the edit page of its submission KL003/26 also offers "#004: SV-002-Beta" (selected); restored afterwards | Match | Browser | `verification/FR-004-deleted-sender-kept-on-edit.png`; also test-asserted (existing): `Controllers/SendingControllerTests.Create_AddsPossiblePatientsAndNonDeletedSenders`; (added): `Controllers/PatientSendingControllerTests.Edit_SendingOfDeletedSender_OffersActiveSendersAndTheAttachedOne` |
| FR-005 · SC-004 | One sender directory shared with the Meningococci intake | `/MeningoPatientSending/Create` offers the same options #001–#003 with identical texts; the deleted `SV-002-Beta` is missing there too | Match | Browser | browser result (option lists) |
| FR-006 · EC2 · EC3 · EC4 | Sender, patient link, receiving date, invasive, sender lab number, species required; receiving ≥ sampling; free text for "other" | Sender, receiving date, lab number, species and the date order are enforced with the quoted messages; both "Anderer nicht invasiv" and "Anderer invasiv" require "Anderer Entnahmeort"; an empty sending fails only on lab number and species, i.e. patient link and invasive never fail (patient link set by the system, "Invasiv" derived) | Spec deviation | Unit test (existing) | `Validators/SendingValidatorTests.Validate_WithAMaximalInvalidModel_ReturnsAllInvalidFields` (4 cases); (added): `Validate_ReceivingDateOnSamplingDate_IsValid`, `Validate_AnySamplingLocation_PassesInvasiveRule` (3 cases); browser: messages via server POST |
| FR-007 · EC1 | Initials required, in dotted form; gender required | Browser rejects "Max M." and "EM" with "Die Initialen müssen in der Form 'E.M.' eingegeben werden."; empty initials and gender rejected by browser and server. The server accepts "Max M." (stored "MAX M.", KL007/26): its pattern is not anchored | New defect | Browser | server POST and DB; DATA-030; the matching part also test-asserted (existing): `Validators/PatientValidatorTests.Validate_WithAMaximalInvalidModel_ReturnsAllInvalidFields` ("Herbert", empty initials) |
| FR-008 · US1.4 · SC-005 | Invalid save persists nothing and redisplays the form with messages | Empty form: browser shows five messages before submitting; server POST returns 200 with the form and messages, nothing stored. With the sender missing, the server shows only the sender message (lab number/species messages appear once a sender is chosen) | Match | Browser | `verification/FR-008-empty-client.png`; DB; also test-asserted (existing): `Controllers/PatientSendingControllerTests.Create_InvalidModel_ProducesValidationErrors` |
| FR-009 · US2.1 · EC5 · SC-002 | One match on initials, birth date, postal code pauses the save and asks for a choice | Second Y.D./03.03.2003/97082 intake (typed "y.d.") shows "Ein Patient mit den selben Initialen, Geburtsdatum und Postleitzahl existiert bereits. …" with "Neuen Patienten anlegen" / "Bestehenden Patienten verwenden", none preselected; saving without a choice shows the prompt again; nothing stored meanwhile | Match | Browser | `verification/US2-1-duplicate-prompt.png`; also test-asserted (added): `Controllers/PatientSendingControllerTests.Create_LowerCaseInitialsOfOneExistingPatient_PausesForDuplicateResolution` |
| FR-010 · US2.2 · US2.3 | "Use existing" attaches and updates the patient; "create new" creates one | "Bestehenden Patienten verwenden": KL004/26 attached to patient 4, its "Wohnort" updated to "SV-002-Ort-geaendert"; "Neuen Patienten anlegen": KL005/26 with new patient 5 | Match | Browser | DB; also test-asserted (added): `Create_UseExistingPatient_AttachesSendingToExistingPatientAndUpdatesIt`, `Create_CreateNewPatient_CreatesSecondPatientWithSameIdentifyingDetails` |
| FR-011 · US2.4 · EC6 | Two matches: save rejected with the third-patient message | Third Y.D. intake: "Es existieren bereits zwei Patienten mit den selben Initialen, Geburtsdatum und Postleitzahl. Ein dritter Patient mit den selben Eigenschaften kann leider nicht gespeichert werden."; no choice offered, nothing stored | Match | Browser | `verification/US2-4-third-patient-rejected.png`; also test-asserted (added): `Create_TwoExistingPatientsMatch_RejectsThirdPatient` |
| FR-011 (choice posted) · SC-003 | A third such patient is never stored | Same request with `DuplicatePatientResolution=CreateNewPatient`: stored, third patient Y.D. (id 6, KL006/26); with `UseExistingPatient`: HTTP 500 "Die Sequenz enthält mehr als ein Element." | New defect | Browser | server POST and DB; DATA-031 |
| FR-012 · US1.3 · EC9 | Warning with the new laboratory number when it changed | Two create pages both showed KL001/26; the one saved second got 002/26 and the list showed "Warnung Beim Speichern der letzten Einsendung hat sich die Labornummer geändert. Neue Labornummer: 002/26" | Match | Browser | `verification/US1-3-labnumber-warning.png`; also test-asserted (added): `Create_LaboratoryNumberTakenMeanwhile_WarnsWithNewLaboratoryNumber`, `Create_LaboratoryNumberAsShown_DoesNotWarn` |
| FR-013 · US3.1 | List with eleven data columns incl. postal codes and sender lab number; paging, sorting, full-text and per-column search | Visible: "Initialen", "Geburtsdatum", "Stammnummer", "Eingangsdatum", "Entnahmeort", "Invasiv", "Labor-Nr" (link to `/Isolate/Edit/n`), actions, "Befund" (icon "Kein Befund erstellt"); postal codes and sender lab number are hidden columns, found by the search box ("sv-002-a2", "liquor") and by "Erweiterte Suche" fields "Patient PLZ", "Einsender Labornummer", "Einsender PLZ", "Initialen", "Geburtsdatum"; sort toggles per column; default "Labor-Nr" descending, 8 rows per page | Spec deviation | Browser | `verification/FR-013-list.png`, `verification/FR-013-extended-search.png`; also test-asserted (existing): `DataTableAjax_ReportGenerated_IsSetAccordingToReportState` (3 cases); (added): `DataTableAjax_ListsOnlyNotDeletedSendingsWithSenderAndPatientColumns` |
| FR-014 | Rows offer edit and "Befund erstellen" | "Bearbeiten" → `/PatientSending/Edit/n`, "Befund erstellen" → `/Report/Isolate/n` | Match | Browser | `verification/FR-013-list.png` |
| FR-015 · US3.2 | Edit saves patient and sending together and returns to the list | "Einsendung bearbeiten" for KL001/26: "Bemerkung" and "Wohnort" changed, "Änderungen speichern" → `/PatientSending`; DB shows both changes | Match | Browser | DB; also test-asserted (added): `Edit_ValidModel_SavesPatientAndSendingAndRedirectsToList` |
| FR-016 · US3.3 | Soft delete; deleted submissions leave the active list | KL002/26 (`SV-001-Alpha`) deleted via "Löschen bestätigen" → "Eintrag löschen"; list shows 7 entries without it; DB `Deleted = 1` (left deleted on purpose, see below) | Match | Browser | `verification/FR-016-list-after-delete.png`; also test-asserted (added): `DeleteConfirmed_InvalidSending_SetsDeletedFlagAndSavesWithoutValidation`, `DataTableAjax_ListsOnlyNotDeletedSendingsWithSenderAndPatientColumns` |
| FR-017 | Confirmation shows initials, birth date, stem number, laboratory number, receiving date | "Sind Sie sicher, dass sie die folgende Einsendung löschen wollen?" with "Initialen" X.B., "Geburtsdatum" 02.02.1992, "Stammnummer" "H -", "Labornummer" KL002/26, "Eingangsdatum" 17.06.2020 | Match | Browser | `verification/FR-017-delete-confirmation.png` |
| FR-018 · US3.4 · EC7 · SC-006 | Delete and restore toggle the flag without field validation | No invalid submission can be created through the UI; the controller saves inside `PerformWithoutSaveValidation` for a submission without lab number and species, both when deleting and restoring. Browser: `/PatientSending/Undelete/7` → 302 `/PatientSending`, KL006/26 active again | Match | Unit test (added) | `Controllers/PatientSendingControllerTests.DeleteConfirmed_InvalidSending_SetsDeletedFlagAndSavesWithoutValidation`, `Undelete_InvalidDeletedSending_ClearsDeletedFlagAndSavesWithoutValidation`; DB |
| FR-019 | Merge reassigns all submissions and removes the emptied patient after confirmation | "Patienten Zusammenfügen (Haemophilus)" with 5 and 6: "Bestätigung" lists both patients and their submissions; "Zusammenfügen und doppelten Patienten löschen" → "Erfolgreich … Alle Einsendungen wurden diesem Patienten zugeordnet. … Der Patient wurde gelöscht."; KL006/26 now on patient 5, patient 6 gone | Match | Browser | `verification/FR-019-merge-confirmation.png`, `verification/FR-019-merge-success.png`; DB |
| FR-020 | All intake actions need "Standardbenutzer"; RKI export also "RKI" | `Standard1`, `Labor1`: 200 everywhere; `Rki1`: only `/PatientSending/RkiExport` 200, all others redirect to sign-in; `Admin1` and anonymous: every address (list, create, edit, delete, deleted list, restore, merge, three exports, list data POST) redirects to `/Account/Login` | Match | Browser | `verification/FR-020-Admin1-home.png`, `verification/FR-020-Rki1-home.png` |
| FR-021 (binding) | Create and edit bind only the intended fields | Create POST with `Sending.Deleted=true` stored KL008/26 as deleted; no field list exists on create or edit | New defect | DB | browser POST then DB query; SEC-013 |
| FR-021 (token) · SEC-005 | Anti-forgery token not verified on create/edit | Create POST without `__RequestVerificationToken` stored KL009/26 | Defect confirmed | DB | browser POST then DB query |
| Assumption ("Deleted" action) · DATA-006 | "Gelöschte Einsendungen" lists non-deleted submissions | Page lists the 7 active submissions with "Wiederherstellen" links; the deleted KL002/26 is missing | Defect confirmed | Browser | `verification/DATA-006-deleted-page.png` |
| New EC "Editing a deleted submission restores it" · DATA-007 | (020, backlog) Saving the edit page of a deleted submission restores it | KL005/26 deleted, `/PatientSending/Edit/6` opened by address, "Bemerkung" changed, saved → active again (DB `Deleted = 0`) | Defect confirmed | Browser | DB |
| New EC "Missing or unknown submission number" (restore) · DATA-008 | (020, backlog) Restore is a plain link; unknown id ends on the error page | GET `/PatientSending/Undelete/7` restores without confirmation; `/Undelete/9999` and `/Undelete` → HTTP 500 "Der Objektverweis wurde nicht auf eine Objektinstanz festgelegt." | Defect confirmed | Browser | HTTP status via request |
| New EC "Missing or unknown submission number" (edit, delete) | Not stated | `/PatientSending/Edit` and `/Delete` without id or with id 9999 → HTTP 500 (null reference) | New defect | Browser | HTTP status via request; UX-010 |
| DEC-001 | (backlog) Administrator sees laboratory navigation but is refused | `Admin1` sees "Haemophilus", "Gelöschte Einsendungen (Haemophilus)", "Patienten Zusammenfügen" and the exports; every intake address redirects it to sign-in (observation only, no decision) | Defect confirmed | Browser | `verification/FR-020-Admin1-home.png` |

SC-004 has no row of its own; FR-005 covers it.

## Unit tests added

- `HaemophilusWebTests/Controllers/PatientSendingControllerTests.cs` (extended, 13 cases):
  `Create_NewPatientSending_PrefillsDefaults` (FR-002),
  `Create_ValidModel_LinksSenderAndPatientAndAssignsLaboratoryNumberButNoStemNumber` (FR-003),
  `Create_LowerCaseInitialsOfOneExistingPatient_PausesForDuplicateResolution` (FR-009),
  `Create_UseExistingPatient_AttachesSendingToExistingPatientAndUpdatesIt`,
  `Create_CreateNewPatient_CreatesSecondPatientWithSameIdentifyingDetails` (FR-010),
  `Create_TwoExistingPatientsMatch_RejectsThirdPatient` (FR-011, without a posted choice only),
  `Create_LaboratoryNumberTakenMeanwhile_WarnsWithNewLaboratoryNumber`, `Create_LaboratoryNumberAsShown_DoesNotWarn`
  (FR-012), `Edit_SendingOfDeletedSender_OffersActiveSendersAndTheAttachedOne` (FR-004),
  `Edit_ValidModel_SavesPatientAndSendingAndRedirectsToList` (FR-015),
  `DeleteConfirmed_InvalidSending_SetsDeletedFlagAndSavesWithoutValidation`,
  `Undelete_InvalidDeletedSending_ClearsDeletedFlagAndSavesWithoutValidation` (FR-016, FR-018),
  `DataTableAjax_ListsOnlyNotDeletedSendingsWithSenderAndPatientColumns` (FR-013, FR-016). They use a private context
  stub with non-static sets, as the 001 tests do.
- `HaemophilusWebTests/Validators/SendingValidatorTests.cs` (extended, 4 cases): `Validate_ReceivingDateOnSamplingDate_IsValid`,
  `Validate_AnySamplingLocation_PassesInvasiveRule` (3 cases) (FR-006).
- No tests for DATA-030, DATA-031, SEC-013, UX-010 (defects).

## Spec corrections applied

- FR-003, US1 intro, Independent Test, US1.2, SC-001, Key Entities (Isolate), Assumptions: intake assigns only the
  laboratory number, of the year of saving; no stem number (was: stem number and laboratory number assigned at save).
  New `[POTENTIAL-REFACTORING]` REF-018.
- FR-006, Edge Cases (missing sending fields), SC-005: messages quoted; both "Anderer …" locations need the free text;
  patient link and invasive value cannot fail (was: listed as required checks).
- FR-007, new Edge Case "Initials format on the server": browser vs. server check (`[SUSPECTED-DEFECT]` DATA-030).
- FR-011, SC-003, Edge Cases: message quoted; a posted duplicate-resolution choice skips the check (was: "never
  stored"; `[SUSPECTED-DEFECT]` DATA-031).
- FR-013, US3.1: hidden search-only columns, "Erweiterte Suche", default sort and page size (was: all columns shown,
  "per-column search").
- FR-021: no field list on create/edit (was: "binding only the intended … fields"; `[SUSPECTED-DEFECT]` SEC-013).
- New Edge Cases "Missing or unknown submission number" (UX-010, DATA-008) and "Editing a deleted submission restores
  it" (DATA-007).

## Backlog updates

- DEC-001: observation for the intake pages recorded (no decision taken).
- SEC-005: confirmed on system for the combined create save.
- DATA-006: confirmed on system.
- DATA-007: source 002 added; confirmed on system for Haemophilus submissions.
- DATA-008: source 002 added; confirmed on system.
- NEW SEC-013: combined submission form binds every patient and sending property.
- NEW DATA-030: server-side initials check accepts free text.
- NEW DATA-031: duplicate-resolution choice skips the third-patient block.
- NEW UX-010: submission pages with missing or unknown id end in HTTP 500.
- NEW REF-018: automatic stem numbers implemented but never enabled.

## Data prepared for 013 FR-015 / DEC-002

Sender `SV-001-Alpha` (sender 2) now has exactly two Haemophilus submissions sampled in 2020:

| Laboratory number | Sampling date | Receiving date | Labnr. Einsender | Patient | State |
| --- | --- | --- | --- | --- | --- |
| KL001/26 | 10.03.2020 | 12.03.2020 | SV-002-A1 | X.A. | active |
| KL002/26 | 15.06.2020 | 17.06.2020 | SV-002-A2 | X.B. | soft-deleted |

The laboratory numbers carry the year of saving (26), not 2020 (FR-003). Neither has a stem number. The data lives
only in the current catalog; a host restart reseeds it.

## Records and seed changes

- Seed records unchanged.
- Own records left behind: sender `SV-002-Beta` (soft-deleted and restored for FR-004); Haemophilus submissions
  KL001/26 and KL002/26 (above), KL003/26–KL007/26 and KL009/26 (active), KL008/26 (deleted, FR-021); patients X.A.,
  X.B., Y.D. (ids 4, 5; id 6 merged away), "MAX M.", O.P., N.T.

## Host incident

At session start the app ran against the developer database instead of the seeded catalog: a build of
`HaemophilusWebTests` at 11:03 (after `host.ready` at 10:48) ran `CustomWebConfigTransform` in `HaemophilusWeb.csproj`
and regenerated `HaemophilusWeb/Web.config` from `Web.template.config`, undoing the host's connection swap. With the
owner's approval the `DefaultConnection` string was set back to the host value; `Web.config.screenshotbak` still
holds the original, which the host restores on shutdown. The test build in this session used
`/p:BuildProjectReferences=false` and left `Web.config` unchanged.

## Not verifiable here

- None.

## Open questions for the owner

- None that block this spec. DEC-001 and DEC-002 stay open.
