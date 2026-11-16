# Verification: Meningococci patient + sending intake (MeningoPatientSending)

**Spec**: 003 · **Date**: 2026-10-30 · **Build**: `c1e6d9f` · **Host**: ScreenshotTests
SpecVerificationHost (`http://localhost:9899`, catalog `HaemophilusWeb_Screenshots`) ·
**Accounts used**: `Standard1` (all laboratory flows), `Labor1`, `Admin1`, `Rki1` (role checks) ·
**Scope**: full

## Summary

| Verdict | Count |
| --- | --- |
| Match | 17 |
| Spec deviation | 3 |
| Defect confirmed | 10 |
| New defect | 0 |
| Not verifiable | 0 |

By primary path: Browser 24 · File 1 · DB 2 · Unit test (existing) 2 · Unit test (added) 1 · Not verifiable 0
(adds up to the row count). Also test-asserted: 12 rows with a browser verdict. Test run:
`vstest.console.exe … /Tests:MeningoPatientSendingControllerTests,MeningoPatientValidatorTests,MeningoSendingValidatorTest,MeningoSendingTests`
→ 40 passed, 0 failed (18 existing, 22 added); full suite → 660 passed, 0 failed (664 total, 4 ignored).

Edge cases are numbered in the order of the spec's Edge Cases list before this verification: EC1 missing required
patient fields, EC2 birth date too early, EC3 country/federal-state consistency, EC4 missing required sending fields,
EC5 receiving before sampling, EC6 sampling before birth, EC7 "other invasive" without free text, EC8 "other
non-invasive" without free text, EC9 duplicate patient (one match), EC10 duplicate patient (two or more matches),
EC11 delete/restore bypasses validation, EC12 laboratory number reassignment. Edge cases added by this verification
are named in the rows.

## Results

| ID | Spec says | App/test shows | Verdict | Path | Evidence |
| --- | --- | --- | --- | --- | --- |
| FR-001 · US1 | One combined form captures sending and patient data | "Einsendung erfassen" holds the sending fields ("Labornummer" … "Serogruppe Einsender"), the patient fields ("Initialen" … "Anderer Risikofaktor") and "Bemerkung"; one "Neu anlegen" saves both | Match | Browser | `verification/FR-002-create-defaults.png` |
| FR-002 · US1.1 | Defaults: receiving today, sampling today − 7, location "Blut", species "N. meningitidis" | "Eingangsdatum" 30.10.2026, "Entnahmedatum" 23.10.2026, "Entnahmeort" "Blut", "Spezies Einsender" "N. meningitidis"; also preselected: "Material" "Vitaler Stamm", "Bundesland" "n.n.", "Land" Deutschland | Match | Browser | `verification/FR-002-create-defaults.png`; also test-asserted (added): `Controllers/MeningoPatientSendingControllerTests.Create_NewPatientSending_PrefillsDefaults` |
| FR-003 · US1.2 · SC-001 · Assumption (identifiers) | Save links sender and patient and assigns a stem number and a laboratory number | `SV-003-Alpha` submission sampled 10.03.2020 got MZ008/26 (year of saving), linked to sender 3 and new patient 11; "Stammnummer" stays empty ("DE -" in the list, DB `StemNumber` NULL) for all 16 new submissions; seeded ones show "DE1"–"DE3" | Spec deviation | Browser | `verification/US1-2-saved-list.png`; DB; also test-asserted (added): `Controllers/MeningoPatientSendingControllerTests.Create_ValidModel_LinksSenderAndPatientAndAssignsLaboratoryNumberButNoStemNumber`; (existing): `Models/Meningo/MeningoSendingTests.AutoAssignStemNumber_Material_IsSetAccordingToMaterial` (4 cases) |
| FR-004 | Sender choice: active senders plus the one attached to the submission | With `SV-003-Beta` soft-deleted, the create form offers #001–#003; the edit page of its submission MZ010/26 also offers "#004: SV-003-Beta - 97082" (selected); sender restored afterwards | Match | Browser | `verification/FR-004-deleted-sender-kept-on-edit.png`; also test-asserted (added): `Controllers/MeningoPatientSendingControllerTests.Edit_SendingOfDeletedSender_OffersActiveSendersAndTheAttachedOne` |
| FR-005 · SC-004 | One sender directory shared with the Haemophilus intake | `/PatientSending/Create` offers the same options #001–#003 with identical texts; the deleted `SV-003-Beta` is missing there too; senders created on `/Sender/Create` appear in the Meningococci form | Match | Browser | browser result (option lists) |
| FR-006 · EC4 · EC5 · EC7 · EC8 | Sender, patient link, receiving date, sender lab number, species required; receiving ≥ sampling; free text for both "other" locations | Server POSTs: missing sender "'Einsendernummer' darf kein Nullwert sein.", missing receiving date "'Eingangsdatum' darf kein Nullwert sein.", receiving 09.03. before sampling 10.03. "Das Eingangsdatum muss nach dem Entnahmedatum liegen", same day accepted, "Anderer invasiv" "Anderer Entnahmeort (invasiv) darf nicht leer sein.", "Anderer nicht invasiv" "Anderer Entnahmeort (nicht invasiv) darf nicht leer sein."; browser: "'Labnr. Einsender' darf nicht leer sein.", "'Spezies Einsender' darf nicht leer sein."; without sampling date the submission is stored (MZ007/26). The patient link cannot fail (set by the system) | Spec deviation | Unit test (existing) | `Validators/MeningoSendingValidatorTest.Validate_WithAMaximalInvalidModel_ReturnsAllInvalidFields` (4 cases); (added): `Validate_ReceivingDateOnSamplingDate_IsValid`, `Validate_WithoutSamplingDate_IsValid`; browser: messages via server POST |
| FR-007 · EC1 · EC2 · EC3 | Initials required, dotted form; birth date after 01.01.1800; gender required; state/country consistency | Browser rejects "Max M." and "EM", accepts "?.?."; empty initials and gender rejected by browser ("'Initialen' darf nicht leer sein.", "'Geschlecht' darf nicht leer sein."). Server: "EM" rejected, but "Max M." stored as "MAX M." (MZ001/26) — pattern not anchored; 01.01.1800 rejected ("Der Wert von 'Geburtsdatum' muss grösser sein als '01.01.1800 00:00:00'."), 02.01.1800 and no birth date stored; "Ausland"+Deutschland and Bayern+Österreich rejected with the two country messages, "Ausland"+Österreich stored | Defect confirmed | Browser | server POST and DB; DATA-030; the matching part also test-asserted (existing): `Validators/MeningoPatientValidatorTests.Validate_WithAMaximalInvalidModel_ReturnsAllInvalidFields` (4 cases); (added): `Validate_WithoutBirthDate_IsValid`, `Validate_BirthDateAroundFirstJanuary1800_IsValidOnlyAfterIt` (2 cases), `Validate_QuestionMarkInitials_IsValid` |
| FR-008 · EC6 | Sampling before birth rejected with the quoted message | Sampling 01.01.1999, birth 01.01.2000: "Das Entnahmedatum muss nach dem Geburtsdatum des Patienten liegen" on "Entnahmedatum", nothing stored | Match | Browser | server POST; also test-asserted (added): `Controllers/MeningoPatientSendingControllerTests.Create_SamplingDateBeforeBirthDate_RejectsAndStoresNothing`, `Create_SamplingDateOrBirthDateMissing_DoesNotCheckDateOrder` (2 cases) |
| FR-009 · US1.4 · SC-005 | Invalid save persists nothing and redisplays the form with messages | Empty form: browser shows five messages before submitting; server POST with all fields empty returns 200 with the form and the messages of the non-nullable fields ("Einsendernummer", "Eingangsdatum", "Entnahmeort", "Material", "Bundesland"); nothing stored | Match | Browser | `verification/FR-009-empty-client.png`; DB |
| FR-010 · US2.1 · EC9 · SC-002 | One match on initials, birth date, postal code pauses the save and asks for a choice | Second X.A./01.02.2001/97080 intake (typed "x.a.") shows "Ein Patient mit den selben Initialen, Geburtsdatum und Postleitzahl existiert bereits. …" with "Neuen Patienten anlegen" / "Bestehenden Patienten verwenden" under "Konfliktlösung", none preselected; saving without a choice shows the prompt again; nothing stored meanwhile | Match | Browser | `verification/US2-1-duplicate-prompt.png`; also test-asserted (added): `Controllers/MeningoPatientSendingControllerTests.Create_LowerCaseInitialsOfOneExistingPatient_PausesForDuplicateResolution` |
| FR-011 · US2.2 · US2.3 | "Use existing" attaches and updates the patient; "create new" creates one | "Bestehenden Patienten verwenden": MZ011/26 attached to patient 11, its "Wohnort" overwritten with "SV-003-Ort-geaendert"; "Neuen Patienten anlegen": MZ012/26 with new patient 14 | Match | Browser | DB; also test-asserted (added): `Create_UseExistingPatient_AttachesSendingToExistingPatientAndUpdatesIt`, `Create_CreateNewPatient_CreatesSecondPatientWithSameIdentifyingDetails` |
| FR-012 · US2.4 · EC10 | Two matches: save rejected with a third-patient message | Third X.A. intake: "Es existieren bereits zwei Patienten mit den selben Initialen, Geburtsdatum und Postleitzahl. Ein dritter Patient mit den selben Eigenschaften kann leider nicht gespeichert werden."; no choice offered, nothing stored | Match | Browser | `verification/US2-4-third-patient-rejected.png`; also test-asserted (added): `Create_TwoExistingPatientsMatch_RejectsThirdPatient` |
| FR-012 (choice posted) · SC-003 · DATA-031 | A third such patient is never stored | Same request with `DuplicatePatientResolution=CreateNewPatient`: stored, third patient X.A. (id 15, MZ013/26); with `UseExistingPatient`: HTTP 500 "Die Sequenz enthält mehr als ein Element." | Defect confirmed | Browser | server POST and DB |
| FR-013 · US1.3 · EC12 | Warning with the new laboratory number when it changed | Create page showed MZ009/26; a parallel save took it; saving the page gave MZ010/26 and the list showed "Warnung Beim Speichern der letzten Einsendung hat sich die Labornummer geändert. Neue Labornummer: 010/26" (without "MZ") | Match | Browser | `verification/US1-3-labnumber-warning.png`; also test-asserted (added): `Create_LaboratoryNumberTakenMeanwhile_WarnsWithNewLaboratoryNumber`, `Create_LaboratoryNumberAsShown_DoesNotWarn` |
| FR-014 | "Invasiv" derived from the sampling location | List and laboratory export: "Blut", "Liquor" → "Ja"; "Rachenabstrich", "Anderer nicht invasiv" (shown as its free text "SV-003-Abstrich") → "Nein"; the form has no "Invasiv" field | Match | Unit test (existing) | `Models/Meningo/MeningoSendingTests.Invasive_IsCalculated_AccordingToSamplingLocation` (4 cases); (added): `Controllers/MeningoPatientSendingControllerTests.DataTableAjax_ListsOnlyNotDeletedSendingsWithSenderAndPatientColumns`; browser list |
| FR-015 · US3.1 | List with eleven data columns incl. postal codes and sender lab number; paging, sorting, full-text and per-column search | Visible: "Initialen", "Geburtsdatum", "Stammnummer", "Eingangsdatum", "Entnahmeort", "Invasiv", "Labor-Nr" (link to `/MeningoIsolate/Edit/n`), actions, "Befund" (icon "Kein Befund erstellt"); patient postal code, sender postal code, sender lab number and patient number are hidden columns, found by the search box ("sv-003-a2", "97084") and by "Erweiterte Suche" fields "Initialen", "Geburtsdatum", "Patient PLZ", "Einsender Labornummer", "Einsender PLZ", "Patienten-Nr."; sort toggles per column; default "Labor-Nr" descending, 8 rows per page | Spec deviation | Browser | `verification/US1-2-saved-list.png`, `verification/FR-015-extended-search.png`; also test-asserted (added): `DataTableAjax_ListsOnlyNotDeletedSendingsWithSenderAndPatientColumns` |
| FR-016 | Rows offer edit and "Befund erstellen" (Meningococci report) | "Bearbeiten" → `/MeningoPatientSending/Edit/n`, "Befund erstellen" → `/MeningoReport/Isolate/n` | Match | Browser | `verification/US1-2-saved-list.png` |
| FR-017 · US3.2 | Edit saves patient and sending together and returns to the list | "Einsendung bearbeiten" for MZ016/26: "Bemerkung", "Wohnort" and "Entnahmeort" ("Anderer nicht invasiv" with free text) changed, "Änderungen speichern" → `/MeningoPatientSending`; DB shows all changes; stored clinical-information and risk-factor flags shown checked | Match | Browser | DB; also test-asserted (added): `Edit_ValidModel_SavesPatientAndSendingAndRedirectsToList` |
| FR-018 · US3.3 · DEC-007 | "Eintrag löschen" ends on the error page; a confirmation request outside the pages soft-deletes | "Eintrag löschen" → `/MeningoPatientSending/Delete/17`: HTTP 500 "The view 'Delete' or its master was not found …"; POST `/Delete/17` with token → 302 `/MeningoPatientSending`, T.N. gone from the list (DB `Deleted = 1`); without token → HTTP 500 | Defect confirmed | Browser | `verification/US3-3-delete-error.png`; DB; also test-asserted (added): `DeleteConfirmed_InvalidSending_SetsDeletedFlagAndSavesWithoutValidation` |
| FR-019 · US3.4 · EC11 · SC-006 | Delete and restore toggle the flag without field validation; restore only by direct address | No invalid submission can be created through the UI; the controller saves inside `PerformWithoutSaveValidation` for a submission without lab number and species, both when deleting and restoring. Browser: `/MeningoPatientSending/Undelete/17` → 302 `/MeningoPatientSending`, T.N. active again | Match | Unit test (added) | `Controllers/MeningoPatientSendingControllerTests.DeleteConfirmed_InvalidSending_SetsDeletedFlagAndSavesWithoutValidation`, `Undelete_InvalidDeletedSending_ClearsDeletedFlagAndSavesWithoutValidation`; DB |
| FR-020 | Merge reassigns all submissions and removes the emptied patient after confirmation | "Patienten Zusammenfügen (Meningokokken)" with 14 and 15: "Bestätigung" lists both patients and their submissions; "Zusammenfügen und doppelten Patienten löschen" → "Erfolgreich … Alle Einsendungen wurden diesem Patienten zugeordnet. … Der Patient wurde gelöscht."; MZ013/26 now on patient 14, patient 15 gone | Match | Browser | `verification/FR-020-merge-confirmation.png`, `verification/FR-020-merge-success.png`; DB |
| FR-021 | Four exports; RKI, IRIS and LGA restricted to invasive submissions with meningococci found | Range 01.01.2020–31.12.2020: "Labor-Export" lists all 18 active submissions (invasive and non-invasive, all "Meningokokken" "kein Nachweis"); RKI, IRIS and LGA exports contain the header row only | Match | File | `verification/FR-021-*.xlsx` |
| FR-022 | All intake actions need "Standardbenutzer"; RKI and LGA exports also "RKI" | `Standard1`, `Labor1`: 200 everywhere (delete page and deleted list 500, see FR-018); `Rki1`: only `/MeningoPatientSending/RkiExport` and `/StateAuthorityExport` 200, all others redirect to sign-in; `Admin1` and anonymous: every address (list, create, edit, delete, deleted list, restore, merge, four exports, list data POST) redirects to `/Account/Login` | Match | Browser | `verification/FR-022-Rki1-home.png`, `verification/FR-022-Admin1-home.png` |
| FR-023 (binding) · SEC-013 | Create and edit bind only the intended fields | Create POST with `Sending.Deleted=true` stored MZ015/26 as deleted; no field list exists on create or edit | Defect confirmed | DB | browser POST then DB query |
| FR-023 (token) · SEC-005 | Anti-forgery token not verified on create/edit | Create POST without `__RequestVerificationToken` stored MZ014/26; edit POST without token stored the changed "Bemerkung" of MZ014/26 | Defect confirmed | DB | browser POST then DB query |
| Assumption ("Deleted" action) · DEC-007 | No Meningococci deleted-submissions page; the inherited action ends on the error page | `/MeningoPatientSending/Deleted` → HTTP 500 "The view 'Deleted' or its master was not found …"; the navigation has no Meningococci "Gelöschte Einsendungen" entry | Defect confirmed | Browser | HTTP status via request |
| New EC "Editing a deleted submission restores it" · DATA-007 | (020, backlog) Saving the edit page of a deleted submission restores it | MZ015/26 (deleted by the over-posting request), `/MeningoPatientSending/Edit/18` opened by address, "Bemerkung" changed, saved → active again (DB `Deleted = 0`) | Defect confirmed | Browser | DB |
| New EC "Missing or unknown submission number" (restore) · DATA-008 | (020, backlog) Restore is a plain link; unknown id ends on the error page | GET `/MeningoPatientSending/Undelete/17` restores without confirmation; `/Undelete/9999` and `/Undelete` → HTTP 500 "Der Objektverweis wurde nicht auf eine Objektinstanz festgelegt." | Defect confirmed | Browser | HTTP status via request |
| New EC "Missing or unknown submission number" (edit, delete) · UX-010 | (002, backlog) Missing or unknown id ends in HTTP 500 | `/MeningoPatientSending/Edit` and `/Delete` without id or with id 9999 → HTTP 500 (null reference) | Defect confirmed | Browser | HTTP status via request |
| DEC-001 | (backlog) Administrator sees laboratory navigation but is refused | `Admin1` sees "Meningokokken", "Labor Export", "IRIS Export", "LGA Export", "RKI Export", "Einsender Export", "Matching (Meningokokken)", "Patienten Zusammenfügen"; every intake address redirects it to sign-in (observation only, no decision) | Defect confirmed | Browser | `verification/FR-022-Admin1-home.png` |

SC-004 has no row of its own; FR-005 covers it. REF-018 is covered by the FR-003 row.

## Unit tests added

- `HaemophilusWebTests/Controllers/MeningoPatientSendingControllerTests.cs` (new, 16 cases):
  `Create_NewPatientSending_PrefillsDefaults` (FR-002),
  `Create_ValidModel_LinksSenderAndPatientAndAssignsLaboratoryNumberButNoStemNumber` (FR-003),
  `Create_SamplingDateBeforeBirthDate_RejectsAndStoresNothing`, `Create_SamplingDateOrBirthDateMissing_DoesNotCheckDateOrder`
  (2 cases) (FR-008), `Create_LowerCaseInitialsOfOneExistingPatient_PausesForDuplicateResolution` (FR-010),
  `Create_UseExistingPatient_AttachesSendingToExistingPatientAndUpdatesIt`,
  `Create_CreateNewPatient_CreatesSecondPatientWithSameIdentifyingDetails` (FR-011),
  `Create_TwoExistingPatientsMatch_RejectsThirdPatient` (FR-012, without a posted choice only),
  `Create_LaboratoryNumberTakenMeanwhile_WarnsWithNewLaboratoryNumber`, `Create_LaboratoryNumberAsShown_DoesNotWarn`
  (FR-013), `Edit_SendingOfDeletedSender_OffersActiveSendersAndTheAttachedOne` (FR-004, FR-005),
  `Edit_ValidModel_SavesPatientAndSendingAndRedirectsToList` (FR-017),
  `DeleteConfirmed_InvalidSending_SetsDeletedFlagAndSavesWithoutValidation`,
  `Undelete_InvalidDeletedSending_ClearsDeletedFlagAndSavesWithoutValidation` (FR-018, FR-019),
  `DataTableAjax_ListsOnlyNotDeletedSendingsWithSenderAndPatientColumns` (FR-014, FR-015, FR-018). Own context stub
  with non-static sets, as in the 002 tests; the country list is stubbed, so no file or network access.
- `HaemophilusWebTests/Validators/MeningoPatientValidatorTests.cs` (extended, 4 cases): `Validate_WithoutBirthDate_IsValid`,
  `Validate_BirthDateAroundFirstJanuary1800_IsValidOnlyAfterIt` (2 cases), `Validate_QuestionMarkInitials_IsValid`
  (FR-007).
- `HaemophilusWebTests/Validators/MeningoSendingValidatorTest.cs` (extended, 2 cases):
  `Validate_ReceivingDateOnSamplingDate_IsValid`, `Validate_WithoutSamplingDate_IsValid` (FR-006).
- No tests for DATA-030, DATA-031, SEC-005, SEC-013, DATA-007, DATA-008, UX-010, DEC-007 (defects).

## Spec corrections applied

- FR-003, US1 intro, Independent Test, US1.2, SC-001, Key Entities (MeningoSending, MeningoIsolate), Assumptions:
  intake assigns only the laboratory number, of the year of saving; no stem number (was: stem number and laboratory
  number assigned at save). `[POTENTIAL-REFACTORING]` REF-018.
- FR-006, Edge Cases (missing sending fields): messages quoted with the real field labels; the patient link cannot
  fail; the sampling date is optional; server redisplay of an all-empty request described (was: patient link listed
  as a required check).
- FR-007, Edge Cases (missing patient fields, birth date too early), new Edge Case "Initials format on the server",
  SC-005, Key Entities (MeningoPatient): the birth date is optional, only a given one must be after 01.01.1800; "?"
  counts as a letter; browser vs. server check (`[SUSPECTED-DEFECT]` DATA-030; was: "requiring a birth date after
  1 January 1800").
- FR-012, SC-003, Edge Cases: message quoted; a posted duplicate-resolution choice skips the check (was: "never
  stored"; `[SUSPECTED-DEFECT]` DATA-031).
- FR-015, US3.1: hidden search-only columns including the patient number, "Erweiterte Suche" with "Patienten-Nr.",
  default sort and page size, "DE" stem-number prefix (was: all columns shown, "per-column search").
- FR-018, US3.3, Assumptions ("Deleted" action): "Eintrag löschen" and the deleted list end in HTTP 500 (view not
  found); the delete request outside the pages needs the anti-forgery token (was: "generic error page", "derived
  from code; not exercised").
- FR-023: no field list on create/edit (was: "binding only the intended … fields"; `[SUSPECTED-DEFECT]` SEC-013).
- New Edge Cases "Duplicate-resolution choice skips the third-patient check" (DATA-031), "Missing or unknown
  submission number" (UX-010, DATA-008) and "Editing a deleted submission restores it" (DATA-007).

## Backlog updates

- DEC-001: observation for the Meningococci intake pages recorded (no decision taken).
- DEC-007: observation recorded (no decision taken).
- SEC-005: confirmed on system for the combined Meningococci create and edit saves.
- SEC-013: source 003 FR-023 added; confirmed on system for Meningococci submissions.
- DATA-007: source 003 added; confirmed on system for Meningococci submissions.
- DATA-008: source 003 added; confirmed on system for `/MeningoPatientSending/Undelete`.
- DATA-030: source 003 added; the Meningococci rule (with "?") confirmed on system.
- DATA-031: source 003 added; confirmed on system.
- UX-010: source 003 added; confirmed on system.
- REF-018: source 003 added; confirmed; both specs now corrected.

## Records and seed changes

- Seed records unchanged (the soft-deleted fixture MZ003/20 stays deleted).
- Own records left behind: senders `SV-003-Alpha` (3) and `SV-003-Beta` (4; soft-deleted and restored for FR-004);
  Meningococci submissions MZ001/26–MZ016/26, all active (MZ014/26 was deleted and restored, MZ015/26 was stored
  deleted and restored by DATA-007); patients "MAX M.", "?.?.", Z.Z. (three), R.E., N.S., X.A. (ids 11, 14; id 15
  merged away), X.B., X.C., T.N., O.P., K.I.

## Not verifiable here

- None.

## Open questions for the owner

- None that block this spec. DEC-001 and DEC-007 stay open.
