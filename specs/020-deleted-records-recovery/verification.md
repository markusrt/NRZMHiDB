# Verification: Deleted-records recovery

**Spec**: 020 · **Date**: 2026-11-08 · **Build**: `b7f58e4` · **Host**: ScreenshotTests
SpecVerificationHost (`http://localhost:9899`, catalog `HaemophilusWeb_Screenshots`, `host.ready` written 13:49:26 and
unchanged at the end) · **Accounts used**: `Standard1`, `Admin1`, `Rki1`, no account · **Scope**: full

## Summary

| Verdict | Count |
| --- | --- |
| Match | 15 |
| Spec deviation | 3 |
| Defect confirmed | 10 |
| New defect | 0 |
| Not verifiable | 0 |

By primary path: Browser 18 · File 1 · DB 4 · Unit test (existing) 0 · Unit test (added) 1 · Unit test (scratch) 1 ·
Code 3 · Not verifiable 0 (28 rows). Also test-asserted: 9 rows with a browser, file or DB verdict. Test run:
`vstest.console.exe … /TestCaseFilter:"(…Controllers.SenderControllerTests|…Controllers.SendingControllerTests|…Controllers.PatientSendingControllerTests|…Controllers.MeningoPatientSendingControllerTests)&TestCategory!=Integration"`
→ 87 passed, 0 failed; full suite (`TestCategory!=Integration`) → 1288 passed, 0 failed.

Scenarios are `US<story>.<n>`; edge cases are numbered as the list stood before this verification: EC1 "Soft delete is
the only deletion", EC2 "What deletion changes for senders", EC3 "What deletion changes for submissions", EC4 "Numbers
are not reused", EC5 "Inverted selection", EC6 "Page size", EC7 "Restore of an active record", EC8 "Field rules
skipped", EC9 "No audit trail", EC10 "Relation to earlier specs", EC11 "Authorization", EC12 "Architecture". FR-013 is
a scope statement and has no row; EC10 only points to specs 002 and 003 and is covered by the rows it names.

Spec 020 is cross-cutting. Rows already proven by earlier verifications cite their spec and row; everything 020 claims
beyond them was checked again on the system with own records. No external service was contacted: the route block was
installed before the first navigation (cookies cleared) and in the three role contexts, and blocked nothing (0
requests); no page with an outbound lookup was opened in the browser (report pages were read with `page.request.get`,
which runs no page script). No Geonames request was made (postal codes were set without typing).

Data preparation and own records (sampling dates in 2016, a year no other record used): senders `SV-020-Alpha` …
`SV-020-India` (nine, created in reverse order; all deleted through the delete page, then Alpha restored through
"Wiederherstellen", India through `/MeningoSender/Undelete`, Golf through the image request of US5.4; the other six stay
deleted); Haemophilus submissions `SV-020-H1` (KL017/26, sender Alpha, active), `SV-020-H2` (KL018/26, deleted through
"Löschen bestätigen", stays deleted), `SV-020-H3` (KL019/26, stem number 92020, deleted and restored by address);
Meningococci submissions `SV-020-M1` (MZ014/26), `SV-020-M2` (MZ015/26; soft-deleted with a delete request carrying the
edit page's anti-forgery token, because "Eintrag löschen" ends in HTTP 500 (DEC-007); restored by address), `SV-020-M3`
(MZ016/26). Patient "Q.Z." was created on the generated `/Patient/Create` page and deleted permanently through
`/Patient/Delete/<id>`. No seeded record was changed; the fixtures of `SV-Seed-DEC-002` were only read.

## Results

| ID | Spec says | App/test shows | Verdict | Path | Evidence |
| --- | --- | --- | --- | --- | --- |
| FR-001 · US1.1 · US2.1 · US4.1 | "Administration" offers "Gelöschte Einsender" in its general part and, under "Haemophilus", "Gelöschte Einsendungen (Haemophilus)" and "Patienten Zusammenfügen"; under "Meningokokken" only "Patienten Zusammenfügen"; no menu for Public Health | `Standard1` and `Admin1`: "Konfiguration", "Gelöschte Einsender" (`/Sender/Deleted`), "Antibiotika Grenzwerte", "Gesundheitsämter", header "Haemophilus" with "Gelöschte Einsendungen (Haemophilus)" (`/PatientSending/Deleted`) and "Patienten Zusammenfügen", header "Meningokokken" with only "Patienten Zusammenfügen"; `Rki1` has no "Administration" menu | Match | Browser | browser run (menu entries); also 001 Assumption (navigation), 015 FR-001 |
| FR-002 · US5.1 · US5.2 · SC-005 | Delete, deleted-list and restore functions admit only Standard User; unauthenticated requests go to sign-in | `Admin1`, `Rki1` and no account: `/Sender/Deleted`, `/Sender/Undelete/<id>`, `/Sender/Delete/<id>`, `/PatientSending/Deleted`, `/PatientSending/Undelete/<id>`, `/PatientSending/Delete/<id>`, `/MeningoPatientSending/Deleted`, `/MeningoPatientSending/Undelete/<id>`, `/MeningoSender/Undelete/<id>` → 302 `/Account/Login?ReturnUrl=…`; the addressed sender, Haemophilus and Meningococci submissions stayed deleted (DB) | Match | Browser | HTTP status via request; DB; also 001 FR-016, 002 FR-020, 003 FR-022 |
| DEC-001 · EC11 | Administrator sees both entries but is refused `[SUSPECTED-DEFECT]` | As FR-001/FR-002: `Admin1` sees "Gelöschte Einsender" and "Gelöschte Einsendungen (Haemophilus)" and every address redirects it to sign-in (observation only, no decision) | Defect confirmed | Browser | browser run |
| FR-003 · US5.3 · SC-006 (data kept) | Deleting only sets the deleted mark and keeps record and related data; the confirmation verifies the anti-forgery token | Sender: "Eintrag löschen" → "Löschen bestätigen" (warning lists "Einsendung KL017/26", "Einsendung MZ014/26") → "Eintrag löschen" → `/Sender`, `Deleted = 1`. Haemophilus: "Eintrag löschen" → page "Löschen bestätigen" ("Sind Sie sicher, dass sie die folgende Einsendung löschen wollen?") → `/PatientSending`; `SV-020-H2` kept with patient "Q.B.", sender, sampling date and isolate (KL018/26). POST without token to `/Sender/Delete/13` and `/PatientSending/Delete/20` → HTTP 500 "The required anti-forgery form field "__RequestVerificationToken" is not present.", nothing deleted | Match | DB | `verification/FR-003-sender-delete-page.png`, `FR-003-submission-delete-page.png`; SQL before/after; also test-asserted: `Controllers/SenderControllerTests.DeleteConfirmed_InvalidSender_SetsDeletedFlagAndSavesWithoutValidation`, `Controllers/PatientSendingControllerTests.DeleteConfirmed_InvalidSending_SetsDeletedFlagAndSavesWithoutValidation` |
| FR-004 · US1.2 · US1.3 · US1.6 · SC-001 | "Gelöschte Einsender" lists exactly the deleted senders with "Name", "Postleitzahl", "Ort" and a green "Wiederherstellen"; sorted by name, eight per page, search and paging, no page size choice, state remembered; no counts, date or links; empty table when none is deleted | Before any deletion: "Keine Daten in der Tabelle vorhanden", "0 bis 0 von 0 Einträgen". With nine deleted: rows `SV-020-Alpha` … `SV-020-Hotel` on page 1 (name ascending, created in reverse order), "1 bis 8 von 9 Einträgen", India on page 2, no active sender listed; buttons `btn btn-success` → `/Sender/Undelete/<id>`, no other link; no length menu, page length 8, order `[0,"asc"]`; page 2 and the search "Golf" were restored on the next visit (`localStorage` key `DataTables_senders_/Sender/Deleted`) | Match | Browser | `verification/US1-6-empty-deleted-senders.png`, `FR-004-deleted-senders-page1.png`; also test-asserted: `Controllers/SenderControllerTests.IndexAndDeleted_SplitSendersByDeletedFlag` |
| EC2 | A deleted sender leaves "Einsender", the sender choice of new submissions and both sender exports, but stays attached and selectable on its submissions | With Alpha deleted: missing from `/Sender` and from both create forms; the edit pages of KL017/26 and MZ014/26 offer and select "#013: SV-020-Alpha"; both 2016 sender exports have no data row | Match | Browser | browser run; `verification/020-sender-h-2016-deleted.xlsx`, `020-sender-m-2016-deleted.xlsx`; also 001 FR-010, 002 FR-004, 013 FR-005 |
| FR-005 · US2.2 · US2.3 · US2.5 · SC-002 · SC-004 · EC5 · DATA-006 | "Gelöschte Einsendungen (Haemophilus)" lists every active and no deleted submission; columns "Initialen", "Geburtsdatum", "Stammnummer", "Labornummer", "Eingangsdatum"; sorted by initials, eight per page, all rows loaded at once; the deleted submission is found nowhere `[SUSPECTED-DEFECT]` | Page "Gelöschte Einsendungen": 17 rows = the 17 active Haemophilus submissions (SQL), the deleted `SV-020-H2` missing; columns as stated with prefixes ("H92020", "H -", "KL017/26"); order `[0,"asc"]`, page length 8, no length menu, client-side table with all rows in the page; overview search "Q.B." → "Keine Einträge vorhanden.", "sv-020" → only KL017/26; Meningococci overview "sv-020" → only MZ014/26 while MZ015/26 was deleted | Defect confirmed | Browser | `verification/DATA-006-deleted-submissions-page.png`, `US2-5-overview-search-without-deleted.png`; also 002 Assumption ("Deleted" action) |
| FR-006 (sender) · US1.4 · US1.5 · SC-003 | "Wiederherstellen" clears the mark at once, without confirmation, field rules or message, and shows "Einsender"; the sender is back in the list, in the sender choice of both pathogens and in both sender exports | Click on "Wiederherstellen" of Alpha: GET `/Sender/Undelete/13` → `/Sender` ("Einsender"), no dialog, no message apart from the Beta banner; Alpha listed, offered by both create forms; "1 bis 8 von 8 Einträgen" left on the deleted list; 2016 exports: Haemophilus row Alpha, 1, "KL017/26", Meningococci row Alpha, 1, "MZ014/26" | Match | Browser | `verification/US1-4-restored-sender-list.png`, `020-sender-h-2016-restored.xlsx`, `020-sender-m-2016-restored.xlsx`; also test-asserted: `Controllers/SenderControllerTests.Undelete_InvalidDeletedSender_ClearsDeletedFlagAndSavesWithoutValidation` |
| FR-006 (active record) · US2.4 · EC7 | Restoring an active record saves it unchanged, without refusal; for submissions the overview is shown | "Wiederherstellen" of the active KL017/26 on "Gelöschte Einsendungen" → `/PatientSending`, no message; `BINARY_CHECKSUM` of the submission and its patient unchanged; `/Sender/Undelete/13` (active) → 302 `/Sender`, values unchanged | Match | DB | SQL checksums; also test-asserted (added): `Controllers/PatientSendingControllerTests.Undelete_ActiveSending_StaysActiveAndRedirectsToList`, `Controllers/SenderControllerTests.Undelete_ActiveSender_StaysActiveAndRedirectsToList` |
| US5.4 · DATA-008 · SEC-005 | Restore is a plain link without token or confirmation; any request by a signed-in Standard User restores `[SUSPECTED-DEFECT]` | Plain GETs restored KL019/26, MZ015/26, `SV-020-Alpha` and `SV-020-India` without a confirmation step; an image element added to the start page with `src="/Sender/Undelete/7"` restored `SV-020-Golf` (DB `Deleted = 0`) | Defect confirmed | Browser | DB; also 002 and 003 New EC "Missing or unknown submission number" (restore) |
| FR-007 · US3.1 · US3.2 | Restore by direct address works for both pathogens and shows the pathogen's overview with the submission | `/PatientSending/Undelete/22` → `/PatientSending`, KL019/26 (stem "H92020") listed again; `/MeningoPatientSending/Undelete/19` → `/MeningoPatientSending`, MZ015/26 listed again | Match | Browser | `verification/US3-1-restored-by-address.png`; also test-asserted: `Controllers/PatientSendingControllerTests.Undelete_InvalidDeletedSending_ClearsDeletedFlagAndSavesWithoutValidation`, `Controllers/MeningoPatientSendingControllerTests.Undelete_InvalidDeletedSending_ClearsDeletedFlagAndSavesWithoutValidation` |
| FR-007 (missing or unknown number) · US3.5 · UX-010 | Missing or unknown submission number ends on the error page; senders answer "bad request" and "not found" `[SUSPECTED-DEFECT]` | `/PatientSending/Undelete`, `/PatientSending/Undelete/99999`, `/MeningoPatientSending/Undelete`, `/MeningoPatientSending/Undelete/99999` → HTTP 500 "Der Objektverweis wurde nicht auf eine Objektinstanz festgelegt." (framework message); `/Sender/Undelete` → 400, `/Sender/Undelete/99999` → 404 (IIS pages "400.0 - Bad Request", "404.0 - Not Found") | Defect confirmed | Browser | HTTP status via request; sender part also test-asserted: `Controllers/SenderControllerTests.SingleSenderActions_WithoutId_ReturnBadRequest`, `SingleSenderActions_WithUnknownId_ReturnNotFound` |
| FR-008 · US3.3 · US3.4 · DATA-007 | Saving the edit page of a deleted sender or submission restores it `[SUSPECTED-DEFECT]` | Shown on 2026-10-30 for senders (001 EC6), Haemophilus (002 New EC "Editing a deleted submission restores it") and Meningococci submissions (003, same row); not repeated | Defect confirmed | Browser | 001, 002 and 003 `verification.md` |
| FR-009 (submissions) · US4.2 · US4.3 · US4.4 · DEC-007 | Meningococci "deleted submissions" address and delete page end on the error page; only a crafted confirmation soft-deletes `[SUSPECTED-DEFECT]` | Edit page link "Eintrag löschen" → `/MeningoPatientSending/Delete/18`: HTTP 500 "The view 'Delete' or its master was not found …"; `/MeningoPatientSending/Deleted` HTTP 500 "The view 'Deleted' …"; MZ014/26 not deleted; POST `/MeningoPatientSending/Delete/19` with the edit page's token → 302 `/MeningoPatientSending`, `Deleted = 1` (data preparation) | Defect confirmed | Browser | HTTP status via request; DB; also 003 FR-018, 015 EC3 |
| FR-009 (sender pages) · US4.5 · REF-008 | Meningococci sender pages other than the export end on the error page; a restore by direct address "would work" | `/MeningoSender` and `/MeningoSender/Deleted` → HTTP 500 (view not found); `/MeningoSender/Undelete/5` → 302 `/MeningoSender` → HTTP 500, but `SV-020-India` is active again (DB `Deleted = 0`) | Spec deviation | Browser | HTTP status via request; DB; also test-asserted (added): `Controllers/SenderControllerTests.MeningoUndelete_DeletedSender_ClearsDeletedFlagOfSharedSender`; 001 EC7 |
| FR-010 (excluded) · EC3 | Deleted submissions are left out of both overviews and their search, the seven exports, the EpiScanGIS export, the sender selection of the sender exports, the sender delete warning and both merge lookups | Overviews and search: see FR-005 row; others by reference: 011 FR-002 (soft-deleted HI and KL003/20 excluded), 023 FR-003, 013 FR-005 (SV-013-Delta not listed), 001 FR-009 (warning lists only active), 014 FR-005, 015 FR-004 ("Unbekannter Patient" for numbers with only deleted submissions) | Match | Browser | `verification/US2-5-overview-search-without-deleted.png`; also test-asserted: `Controllers/PatientSendingControllerTests.DataTableAjax_ListsOnlyNotDeletedSendingsWithSenderAndPatientColumns`, `Controllers/MeningoPatientSendingControllerTests.DataTableAjax_ListsOnlyNotDeletedSendingsWithSenderAndPatientColumns`, `Controllers/EpiscanGisExportControllerTests.DownloadWithOneSoftDeletedSending_ReturnsOneLineLess` |
| FR-010 (duplicate check) · EC3 | The patient duplicate check at intake compares against all stored patients, also those of deleted submissions (derived) | Intake with "Q.B.", 02.02.2001, 99008 (patient whose only submission KL018/26 is deleted) → "Ein Patient mit den selben Initialen, Geburtsdatum und Postleitzahl existiert bereits. …" with "Konfliktlösung" "Neuen Patienten anlegen" / "Bestehenden Patienten verwenden"; not saved | Match | Browser | `verification/FR-010-duplicate-check-deleted-submission-patient.png` |
| FR-010 (pages reachable) · EC3 | Edit, isolate and report pages of a deleted submission stay reachable by direct address (derived) | Deleted KL018/26: `/PatientSending/Edit/21` 200 "Einsendung bearbeiten", `/Isolate/Edit/21` 200 "Isolat bearbeiten", `/Report/Isolate/21` 200 "Befund erstellen"; seeded deleted MZ003/20: `/MeningoReport/Isolate/3` 200 (read with `page.request.get`, no page script) | Match | Browser | HTTP status via request; also 004 and 005 New EC "Isolate of a deleted submission" |
| FR-010 (sender export values) · DEC-002 | "Anzahl Einsendungen", "Stammnummern" and "Labornummern" of the sender exports include deleted submissions | By reference: 013 FR-008 · EC6 (SV-Seed-DEC-002 counts 2 with KL003/20 and MZ003/20 deleted; observation only, no decision) | Defect confirmed | File | 013 `verification.md` |
| FR-010 (PubMLST matching) · DATA-016 | The PubMLST batch matching includes deleted submissions | By reference: 019 EC7 (reproduced at unit level; a run on the system would call PubMLST) | Defect confirmed | Unit test (scratch) | 019 `verification.md` |
| FR-010 (merge failure) · DATA-009 | A deleted submission on the patient being merged away makes the merge fail with the error page and nothing changed | By reference: 014 FR-011 · EC4 and 015 FR-008 · EC2 (HTTP 500 with the reference conflicts on the isolate tables, data unchanged) | Defect confirmed | Browser | 014 and 015 `verification.md` |
| FR-011 · EC4 · SC-006 (numbers) | Stem and laboratory numbers of deleted submissions stay assigned; new submissions continue counting from the highest number including deleted ones | With KL018/26 deleted as highest number, the next intake got KL019/26; with MZ015/26 deleted, the next got MZ016/26. Stem numbers are not counted at intake (none assigned); stem number 92020 of the deleted KL019/26, entered on KL017/26's isolate page → "Diese Stammnummer ist bereits vergeben" | Spec deviation | DB | SQL of `Isolates`/`MeningoIsolates`; `verification/FR-011-stem-number-of-deleted-taken.png`; also test-asserted (added): `Controllers/SendingControllerTests.AssignStemNumber_IsolateOfDeletedSendingInCurrentYear_AssignsFollowingLaboratoryNumber` |
| FR-012 · EC9 · REF-016 | No record of who deleted or restored, or when `[POTENTIAL-REFACTORING]` | `Senders`, `Sendings`, `MeningoSendings` have only the `Deleted` column of that kind; no audit or history table (`sys.tables`: only `AspNetUserLogins`, `__MigrationHistory` match); `HaemophilusWeb/Logs/2026-11-08.txt` got only "Unhandled error" lines during the run, none for the deletions, restores or the patient deletion | Defect confirmed | DB | `INFORMATION_SCHEMA.COLUMNS`, `sys.tables`, log file |
| EC1 · REF-022 | Soft delete is the only deletion for senders and submissions; the generated submission delete pages delete nothing; the generated patient delete page removes a patient permanently (derived) | `/Sending/Delete/20` and `/MeningoSending/Delete/18` → 404 "Die Ressource kann nicht gefunden werden."; `/Patient/Delete/18` and `/MeningoPatient/Delete/16` 200 with "Delete", "Are you sure you want to delete this?", "Male", "Unknown", "NotStated"; "Delete" for patient "Q.B." (deleted submission with isolate) → HTTP 500 "The DELETE statement conflicted with the REFERENCE constraint "FK_dbo.Isolates_dbo.Sendings_SendingId"…", nothing deleted; patient "Q.Z." created on `/Patient/Create` ("Neuen Patienten anlegen") and deleted → gone (DB, then 404); `/Sending` and `/Sending/Details/21` 200; `/MeningoSending` HTTP 500 (view does not compile: "… 'MeningoSamplingLocation' enthält keine Definition für 'Other'") | Spec deviation | Browser | `verification/EC1-patient-delete-page.png`; HTTP status via request; DB; log line 15:38:34 |
| EC6 · REF-010 (page) | The page loads every active submission at once and looks up each isolate individually `[POTENTIAL-REFACTORING]` | `PatientSendingControllerBase.Deleted` includes only the patient; `Views/PatientSending/Deleted.cshtml` reads `item.Sending.Isolate` per row; the browser table is client-side with all 17 rows | Match | Code | `HaemophilusWeb/Controllers/PatientSendingControllerBase.cs` (`Deleted`), `HaemophilusWeb/Views/PatientSending/Deleted.cshtml` |
| EC8 · REF-010 (switch) | Delete and restore skip the sender field rules; for submissions the switch hits another data connection without effect `[POTENTIAL-REFACTORING]` | `PatientSendingControllerBase.EditUnvalidated` calls `db.PerformWithoutSaveValidation` on its own context, while `sendingController.Edit` saves through the context of the separately created `SendingController` (`new ApplicationDbContextWrapper(new ApplicationDbContext())`); sender: `SenderControllerBase.EditUnvalidated` on the same context | Match | Code | `HaemophilusWeb/Controllers/PatientSendingControllerBase.cs`, `PatientSendingController.cs`, `SenderControllerBase.cs`; sender part also test-asserted: `Controllers/SenderControllerTests.DeleteConfirmed_InvalidSender_SetsDeletedFlagAndSavesWithoutValidation` |
| EC12 · REF-003 | Shared base controllers; the Meningococci variants inherit the functions without pages; restore goes through a separately created sending controller `[POTENTIAL-REFACTORING]` | `SenderControllerBase` and `PatientSendingControllerBase` hold delete, deleted list and restore for both pathogens; `MeningoSenderController` and `MeningoPatientSendingController` add no views for them (HTTP 500 above); `PatientSendingController()` creates `new SendingController()` | Match | Code | `HaemophilusWeb/Controllers/SenderControllerBase.cs`, `PatientSendingControllerBase.cs`, `PatientSendingController.cs` |
| TEST-001 (deleted-records pages and restore) | Deleted-records pages and restoring are not covered by tests | Covered by the tests added in the 001, 002 and 003 verifications and the four added here (restore of an active sender and submission, Meningococci sender restore, laboratory number after a deleted submission); DATA-006, UX-010 and DATA-008 stay untested on purpose | Match | Unit test (added) | see "Unit tests added" |

## Unit tests added

4 test cases in 4 methods, all with `// specs/020 …` comments:

- `HaemophilusWebTests/Controllers/SenderControllerTests.cs`: `Undelete_ActiveSender_StaysActiveAndRedirectsToList`
  (FR-006, EC7), `MeningoUndelete_DeletedSender_ClearsDeletedFlagOfSharedSender` (US4.5; asserts only the cleared flag
  and the unvalidated save, not the redirect that ends in HTTP 500)
- `HaemophilusWebTests/Controllers/PatientSendingControllerTests.cs`: `Undelete_ActiveSending_StaysActiveAndRedirectsToList`
  (US2.4, FR-006)
- `HaemophilusWebTests/Controllers/SendingControllerTests.cs`:
  `AssignStemNumber_IsolateOfDeletedSendingInCurrentYear_AssignsFollowingLaboratoryNumber` (FR-011, SC-006)

No tests for DATA-006, DATA-007, DATA-008, UX-010, DEC-001, DEC-007 or SEC-005 (defects and open decisions).

## Spec corrections applied

- US4.5, FR-009, Assumptions: the Meningococci sender restore address clears the deleted mark of the shared sender and
  then ends on the error page (was: "would work and restore the shared sender (derived)").
- EC4 "Numbers are not reused": only laboratory numbers are counted at intake; stem numbers are entered on the isolate
  page and one held by a deleted submission is refused with "Diese Stammnummer ist bereits vergeben" (was: new
  submissions continue counting stem numbers too).
- EC1 "Soft delete is the only deletion": observed behavior of the generated pages added (submission delete addresses
  "not found"; patient delete permanent without submissions, HTTP 500 with a submission's isolate; Meningococci list
  does not compile), tagged `[POTENTIAL-REFACTORING]` (REF-022).
- EC12 "Architecture": coverage note updated for the tests added in the 001, 002, 003 and 020 verifications.
- Assumptions: note which derived points the verification exercised; US3.5 and US5.4 name their backlog IDs.

## Backlog updates

- DEC-001: observation for the deleted-records pages recorded (no decision).
- DEC-007: observation recorded (no decision).
- DATA-006: confirmed on system 2026-11-08 (020 verification).
- DATA-008: confirmed on system 2026-11-08 (020 verification), including the restore triggered by an image element.
- UX-010: source 020 US3.5 added; confirmed on system 2026-11-08 for `/Undelete` of both pathogens.
- SEC-005: confirmed on system 2026-11-08 for the restore links.
- REF-008: Meningococci sender restore confirmed on system (mark cleared, then HTTP 500).
- REF-010: observed in code and browser.
- REF-016: confirmed on system 2026-11-08 for sender and submission delete and restore and the generated patient
  deletion.
- TEST-001: deleted-records pages and restore covered, except DATA-006, UX-010 and DATA-008.
- NEW REF-022: generated patient and submission pages reachable by direct address.

## Not verifiable here

- None. DATA-016 (PubMLST matching includes deleted submissions) rests on the unit-level reproduction of the 019
  verification, because a run on the system would call PubMLST.

## Open questions for the owner

- REF-022: the generated patient pages let a Standard User create a patient without submission and delete patients
  permanently by direct address, without trace. Remove the scaffolding, or is any of it still used (e.g. for
  clean-ups)? Possibly a `DEC-*` candidate, because it touches data retention.
- The deleted submissions left by verifications (here `SV-020-H2`) can only be found with SQL; DATA-006 remains the
  open bugfix.
