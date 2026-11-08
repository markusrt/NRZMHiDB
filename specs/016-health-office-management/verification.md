# Verification: Health office management

**Spec**: 016 · **Date**: 2026-11-08 · **Build**: `40b979c` · **Host**: ScreenshotTests
SpecVerificationHost (`http://localhost:9899`, catalog `HaemophilusWeb_Screenshots`, `host.ready` written 13:49:26 and
unchanged at the end) · **Accounts used**: `Standard1`, `Admin1`, `Rki1`, signed-out session · **Scope**: full

## Summary

| Verdict | Count |
| --- | --- |
| Match | 17 |
| Spec deviation | 4 |
| Defect confirmed | 7 |
| New defect | 0 |
| Not verifiable | 3 |

By primary path: Browser 15 · File 1 · DB 1 · Unit test (existing) 2 · Unit test (added) 6 · Unit test (scratch) 2 ·
Code 1 · Not verifiable 3 (31 rows). Also test-asserted: 2 rows with a browser verdict. Test run: `vstest.console.exe …
/TestCaseFilter:"(…ReportControllerTests|…HealthOfficesControllerTests|…Models.HealthOfficeTests|…RkiToolTests|…HealthOfficeValidatorTests)&TestCategory!=Integration"`
→ 45 passed, 0 failed; full suite (`TestCategory!=Integration`) → 1243 passed, 0 failed.

Scenarios are `US<story>.<n>`; edge cases are numbered as the list stood before this verification (EC1 "Why local
edits exist" … EC17 "The lookup-and-match logic exists twice").

The RKI lookup was never sent to the server: the route block aborted the report pages' `POST /Report/QueryHealthOffice/`
and the edit link `GET /HealthOffices/Edit?postalCode=97070`. Display checks used answers injected in the browser
(page route), lookup and matching checks the real `RkiTool` with a stubbed query function and the stored sample pages
in unit tests. No Geonames request was made. Own records: health offices `SV-016-Amt A` to `SV-016-Amt I` (postal codes
99011–99019; `SV-016-Amt I` deleted, `SV-016-Amt B` with cleared telephone, fax and email, `SV-016-Amt C` corrected),
`SV-016-Rki1 Amt` (99020, created by `Rki1`) and a second record with the address of `SV-016-Amt D` (99021). Seeded
isolate 1 of both pathogens was only opened; one document was generated for Haemophilus isolate 1 with
`ReportGenerated` aborted, and its report status stayed "none" (checked with SQL).

## Results

| ID | Spec says | App/test shows | Verdict | Path | Evidence |
| --- | --- | --- | --- | --- | --- |
| FR-001 · US1.1 | Both report pages request the office for the patient's postal code in the background, without blocking | Both pages post `postalCode=97070` (only this field) to `/Report/QueryHealthOffice/` on load; with the answer delayed by 3 s, "Befundsvorlage", signer and create button are usable while the box is empty, then the box fills | Match | Browser | `verification/US1-haemo-full.png`, `verification/US1-meningo-full.png` |
| FR-002 · US1.7 · SC-003 | No office, unreachable or unreadable answer: no office, nothing stored, failure only logged | Unit: no-result page, a failing query and a page with a result list but no fields return `null` and store nothing; browser: blocked, HTTP 500 and `null` answers leave box empty and button inactive. The log line ("Failed to query RKI Tool") is not asserted (no NLog reference in the test project) | Match | Unit test (added) | `Controllers/ReportControllerTests.QueryHealthOffice_WebsiteGivesNoUsableOffice_ReturnsNullAndStoresNothing` (3 cases); also `Tools/RkiToolTests.QueryHealthOffice_InvalidPostalCode_ReturnsNull`, `…_ThrowsException_ReturnsNull` |
| FR-003 | Several offices: only the first is used | First office of the multi-result sample page returned | Match | Unit test (existing) | `Tools/RkiToolTests.QueryHealthOffice_MultipleResults_ReturnsFirst` |
| FR-004 · US1.3 · EC11 (collapse) | Address multi-line, indentation removed, decoded; telephone/fax spaces collapsed; email unchanged; postal code recorded | Sample pages give `Landratsamt Forchheim\nGesundheitsamt\n…`, decoded umlaut, collapsed `09191 86-3504`, email and postal code as given | Match | Unit test (existing) | `Tools/RkiToolTests.QueryHealthOffice_ValidPostalCode_ReturnsValidData`, `…_AddressWithUmlaut_UnescapesHtml`, `…_InvalidPhoneAndFax_RemovesExcessSpaces` |
| FR-005 · US1.2 · US1.4 · EC7 | Website always asked first; stored record with the same address (line breaks ignored) returned, nothing stored; duplicates possible | Unit: stored record with CR LF line breaks and corrected values returned, website asked once, no save. Catalog: the edit form stores CR LF. Browser: a second record with the address of `SV-016-Amt D` was created without objection | Match | Unit test (added) | `Controllers/ReportControllerTests.QueryHealthOffice_StoredRecordWithSameAddress_ReturnsStoredRecordAndStoresNothing`; SQL of `HealthOffices.Address` |
| EC6 · Assumption 2 (letter case) | Letter case follows the database rules, assumed case-insensitive, not verified | Column collation `SQL_Latin1_General_CP1_CI_AS`; the same comparison in SQL finds `SV-016-Amt D` for the address in lower case and with trailing spaces, not with an extra inner space | Spec deviation | DB | `sys.columns` collation, read-only `SELECT … WHERE REPLACE(REPLACE(Address…)) = …` |
| FR-006 · US1.5 · EC4 · REF-007 | Unknown address: website values stored without validation and returned | Unit: one record with the website values, stored through the save without validation, returned. Browser: the edit page refuses the same kind of malformed telephone ("Das Feld Telefon enthält keine gültige Telefonnummer.") | Match | Unit test (added) | `Controllers/ReportControllerTests.QueryHealthOffice_NoStoredRecordWithSameAddress_StoresWebsiteValuesWithoutValidationAndReturnsThem` |
| FR-007 · US1.6 · EC11 (fax link) | Address line by line, dial link, fax link `0<digits>@fax` (or `-`), email link; button activated | Injected answer: address with `<br>`, `tel:09191863504`, `mailto:009191863508@fax`, email link, "Gesundheitsamt bearbeiten" active (`/HealthOffices/Edit?postalCode=97070`); fax `""` gives an empty link to `mailto:-@fax`; same on the Meningococci page | Match | Browser | `verification/US1-haemo-full.png`, `verification/US1-meningo-full.png` |
| FR-007 (no office) · Assumption 5 · EC2 · UX-005 | No office: box empty, button inactive; assumed no visible error | Injected `null` answer: box empty, button inactive, no message, but the page script stops with "Cannot read properties of null (reading 'error')" on both pages; the script's `error` branch never applies. HTTP 500: empty without script error | Spec deviation | Browser | `verification/US1-haemo-nullAnswer.png`, `verification/US1-meningo-nullAnswer.png` |
| FR-008 · US1.8 | Office details not part of the document data; missing office does not block generation | Document generated with the injected office shown: none of "SV-016", "Teststra…", "09191", "Gesundheitsamt", "99011" in body, headers or footers; generation also works with the lookup blocked (008/009) | Match | File | `verification/FR-008-report-with-health-office.docx` |
| FR-009 (no office) · US2.2 | Edit opened from the report page repeats the website query; no office → "not found" | Unit: website query repeated with the postal code; no office or unreadable page → `HttpNotFound`. Browser: the active button targets `/HealthOffices/Edit?postalCode=97070` (request blocked) | Match | Unit test (added) | `Controllers/HealthOfficesControllerTests.Edit_PostalCodeWithoutOfficeOnWebsite_ReturnsNotFound` (2 cases) |
| FR-009 (match) · US2.1 | Office found → edit page of the stored record with the same address | The branch needs the website and the database context created inside the controller | Not verifiable | — | See "Not verifiable here" |
| FR-010 | "Gesundheitsämter" in Administration for Standard User and Administrator, not for Public Health | `Standard1` and `Admin1`: entry under "Administration" (`/HealthOffices`); `Rki1`: no "Administration" menu, no entry | Match | Browser | navigation read per account |
| FR-011 · US2.3 | Columns, eight per page, sorted by postal code, searchable, no address, no create/details/delete links, state remembered | Columns "Postleitzahl", "Email", "Telefon", "Fax" and "Bearbeiten"; "1 bis 8 von 9 Einträgen", page 2 holds 99019; ascending postal codes; search "99017" → one row; search and sorting restored after leaving the page (`localStorage` key `DataTables_healthOffices_/HealthOffices`); the email is shown as an email link | Match | Browser | `verification/FR-011-list-page1.png`, `verification/FR-011-state-saved.png`, `verification/FR-011-empty-list.png` |
| FR-012 · US2.4 · US2.5 · US2.6 · US3.1 · SC-004 | Fields with placeholders, "Änderungen speichern"/"Neu anlegen", "Zurück zur Liste", no delete; validation; success shows the list | Labels "Adresse", "Telefon", "Fax", "Email", "Postleitzahl", placeholders `09191-86-3504`, `09191-86-3508`, `Gesundheitsamt@lra-fo.de`, `91301`; no delete button; "Das Feld Email enthält keine gültige E-Mail-Adresse." and "'Postleitzahl' darf nicht leer sein." in the browser, "Das Feld Telefon/Fax enthält keine gültige Telefonnummer." after the round trip; nothing saved; saving and creating show "Gesundheitsämter" | Match | Browser | `verification/FR-012-create-client-validation.png`, `verification/US2-4-edit-page.png`, `verification/US2-6-edit-validation.png`; also test-asserted: `Validators/HealthOfficeValidatorTests`, `Models/HealthOfficeTests.Validate_MalformedValue_FailsOnThatMember` |
| FR-013 · US2.7 · US2.8 · SC-002 | Corrections apply to every postal code of the office; a changed address no longer matches | Unit: two postal codes of the same office → one record, the correction is returned for the second; changed address → new record from the website values, the changed record untouched | Match | Unit test (added) | `Controllers/ReportControllerTests.QueryHealthOffice_TwoPostalCodesOfSameOffice_StoreOneRecordAndShowCorrectionForBoth`, `…_StoredAddressChanged_StoresAndReturnsNewRecordFromWebsite` |
| FR-013 (postal code) · US2.9 · US3.2 · SC-005 · EC3 · DATA-014 | The record's own postal code has no effect; manual records for unknown postal codes never shown | Scratch: a stored record for 99020 is not returned when the website knows no office; a stored record with postal code 91301 and another address is ignored and a new record stored. Browser: the create page stores such records; the list offers no create link | Defect confirmed | Unit test (scratch) | `verification/Scratch016.cs` |
| FR-014 · US3.3 · US3.4 · US3.5 · EC15 | Details/delete by address only; missing id "bad request", unknown "not found"; deletion permanent; non-existent confirmation ends on the generic error page | Details: "HealthOffice", fields, "Edit", "Back to List"; delete: "Are you sure you want to delete this?", "Delete" → list, record gone (SQL); missing id → HTTP 400, unknown → HTTP 404 (IIS page as on `/Sender/Details/4711`); confirming an unknown record → HTTP 500 "Der Wert darf nicht NULL sein." (framework message, logged as unhandled error) | Spec deviation | Browser | `verification/US3-3-details.png`, `verification/US3-4-delete.png`; also test-asserted: `Controllers/HealthOfficesControllerTests.DetailsDeleteAndEdit_WithoutId_ReturnBadRequest`, `Controllers/ReportControllerTests.QueryHealthOffice_StoredRecordDeleted_StoresFreshRecordWithWebsiteValues` |
| REF-016 (FR-014 audit) | Deletion not logged, no undo | The confirmed deletion left no line in `Logs/2026-11-08.txt`; only the failed confirmation appears | Defect confirmed | Browser | application log |
| EC5 · UX-005 | List without address, findable only under the first lookup's postal code | No address column; searching "Teststadt" → "Keine Einträge vorhanden."; scratch: the stored postal code stays 91301 after a lookup for 91077 | Defect confirmed | Browser | `verification/FR-011-list-page1.png`; `verification/Scratch016.cs` |
| FR-015 · EC13 · SEC-004 · SC-006 | Only sign-in required; Public Health and Administrator not refused | `Rki1` and `Admin1`: HTTP 200 for list, create, edit, details, delete; `Rki1` created `SV-016-Rki1 Amt`; signed out: HTTP 302 to `/Account/Login?ReturnUrl=…`; scratch: no `Authorize` on the controller, its actions or the lookup | Defect confirmed | Browser | `verification/FR-015-rki1-created.png` |
| FR-016 (forms) | Create, edit and delete verify the anti-forgery token | POST without token → HTTP 500 "The required anti-forgery form field "__RequestVerificationToken" is not present." for all three | Match | Browser | crafted requests |
| EC14 · SEC-005 (lookup) | Lookup accepts only POST, no token check | Both report pages post only `postalCode` (request held in the browser); scratch: `QueryHealthOffice` has `HttpPost` and no `ValidateAntiForgeryToken` | Defect confirmed | Unit test (scratch) | `verification/Scratch016.cs`; request bodies from the page route |
| FR-017 · EC12 | One list for both pathogens | The Meningococci page posts to `/Report/QueryHealthOffice/`; the catalog has one table `HealthOffices` without a pathogen column | Match | Browser | request URL; `sys.columns` |
| EC8 · DATA-015 | Locally cleared fields break the display; cleared fields stored "as empty" | Cleared telephone, fax and email are saved without a value (SQL); with those values injected: "null" for email and fax (`mailto:-@fax`), button active; without telephone only the address, button inactive, "Cannot read properties of null (reading 'replace')" | Defect confirmed | Browser | `verification/US1-haemo-clearedFaxEmail.png`, `verification/US1-haemo-clearedPhone.png` |
| EC9 · SEC-008 | Values from the RKI website or from local edits are inserted as markup | Local edits cannot carry markup: an address with `<b>`/`<i>` is refused with HTTP 500 "Ein möglicherweise gefährlicher Request.Form-Wert wurde vom Client … entdeckt." (framework message), input lost. Rendering of RKI markup confirmed by 008/009, not repeated | Spec deviation | Browser | `verification/EC9-request-validation.png` |
| EC16 · UX-009 | Details and delete pages untranslated | Titles "Details"/"Delete", heading "HealthOffice", "Edit", "Back to List", "Delete", labels "Email" and "Fax" next to German ones | Defect confirmed | Browser | `verification/US3-3-details.png`, `verification/US3-4-delete.png` |
| EC17 · REF-002 · REF-003 | Lookup-and-match exists twice; RKI client created inside the controllers | `ReportControllerBase.QueryHealthOffice` and `HealthOfficesController.Edit(int?, string)` repeat the same address query; both hold `private readonly RkiTool rkiTool = new RkiTool()`, the health-office controller also `new ApplicationDbContext()` | Match | Code | `Controllers/ReportControllerBase.cs`, `Controllers/HealthOfficesController.cs` |
| TEST-001 (health offices) | Lookup, matching, storing and pages untested | Covered by the tests added here (see "Unit tests added"); the edit page's matching branch and the views stay untested | Match | Unit test (added) | `Controllers/ReportControllerTests`, `Controllers/HealthOfficesControllerTests`, `Models/HealthOfficeTests` |
| EC10 | Patients without or with foreign postal code are still looked up | No such patient in the catalog; the RKI answer would not be observable anyway | Not verifiable | — | See "Not verifiable here" |
| FR-018 | Documentation only, no new rules | A constraint on future changes | Not verifiable | — | — |

## Unit tests added

16 test cases in 11 methods:

- `HaemophilusWebTests/Controllers/ReportControllerTests.cs` (real `RkiTool` with a stubbed query function set by
  reflection, own health-office set): `QueryHealthOffice_NoStoredRecordWithSameAddress_StoresWebsiteValuesWithoutValidationAndReturnsThem`
  (FR-006), `QueryHealthOffice_StoredRecordWithSameAddress_ReturnsStoredRecordAndStoresNothing` (FR-005),
  `QueryHealthOffice_TwoPostalCodesOfSameOffice_StoreOneRecordAndShowCorrectionForBoth` (SC-002),
  `QueryHealthOffice_StoredAddressChanged_StoresAndReturnsNewRecordFromWebsite` (US2.8),
  `QueryHealthOffice_StoredRecordDeleted_StoresFreshRecordWithWebsiteValues` (US3.5),
  `QueryHealthOffice_WebsiteGivesNoUsableOffice_ReturnsNullAndStoresNothing` (FR-002, 3 cases)
- `HaemophilusWebTests/Controllers/HealthOfficesControllerTests.cs` (new): `Edit_PostalCodeWithoutOfficeOnWebsite_ReturnsNotFound`
  (FR-009, 2 cases), `DetailsDeleteAndEdit_WithoutId_ReturnBadRequest` (US3.3, US3.4)
- `HaemophilusWebTests/Models/HealthOfficeTests.cs` (new): `Validate_PlaceholderStyleValues_IsValid`,
  `Validate_WithoutPhoneFaxAndEmail_IsValid`, `Validate_MalformedValue_FailsOnThatMember` (3 cases) (FR-012)

Scratch reproduction (not committed to the test project): `verification/Scratch016.cs`, compiled with `csc` against
`HaemophilusWebTests\bin\Debug`, for DATA-014, the first-lookup postal code of UX-005, and the attributes behind SEC-004
and SEC-005.

## Spec corrections applied

- Edge Cases, second bullet, and Assumption 5: the empty lookup answer shows no message, but the page script stops on it
  ("Cannot read properties of null (reading 'error')"); its error branch never applies (was: assumed "no visible
  error", not observed).
- Edge Cases, address matching, and Assumption 2: letter case and trailing spaces are ignored under the verification
  catalog's collation (was: assumed, not verified).
- Edge Cases, cleared fields, and Assumption 3: cleared telephone, fax and email are saved without a value (was: "as
  empty"); the report-page behavior was observed (was: "not exercised").
- Edge Cases, unescaped markup: markup reaches the page only from the RKI website; local edits with tags end in HTTP 500
  (was: "from the external RKI website or from local edits").
- Edge Cases, deletion: the confirmation for a non-existent record ends in HTTP 500 and is logged as an unhandled error
  (was: "the generic error page"); cross-reference to REF-016 added.
- Edge Cases, last bullet, and Assumption 1: coverage notes updated for the tests added here.

## Backlog updates

- SEC-004: confirmed on system 2026-11-08 for the health-office pages (`Rki1`, `Admin1`; creation by `Rki1`).
- SEC-005: reproduced at unit level 2026-11-08 for the health-office lookup; the three forms do check the token.
- SEC-008: observation added (local edits cannot carry markup; request validation answers HTTP 500).
- DATA-014: reproduced at unit level 2026-11-08.
- DATA-015: confirmed on system 2026-11-08 (cleared fields saved without a value, display as in 008/009).
- UX-005: confirmed on system 2026-11-08; first-lookup postal code reproduced at unit level; the `null` answer's
  script error added.
- UX-009: confirmed on system 2026-11-08 for the health-office pages.
- REF-007: observation added (save without validation in the lookup; telephone/fax checked only by the server).
- REF-016: source extended by 016 FR-014 (health-office deletion and edits); confirmed on system 2026-11-08.
- TEST-001: covered by tests added in the 016 verification (2026-11-08) for the health-office lookup, matching,
  storing and the controller answers; the edit page's matching branch and the views stay untested.

## Not verifiable here

- FR-009 (match) · US2.1: opening the edit page from the report page asks the RKI website (blocked by the verification
  rules), and the controller creates its database context itself, so a unit test cannot supply stored records. Would
  need an injectable context or a server-side RKI stub.
- EC10: no patient without postal code or with a foreign one exists in the catalog, and the RKI answer for such a value
  is not observable without calling the website. The page's request body would show the value sent; a seeded patient
  with an empty postal code would make it observable.
- FR-018: a constraint on future changes, nothing to observe.

## Open questions for the owner

- The report page cannot tell "the RKI website knows no office" from a failure: both leave the box empty without a
  message (the empty answer even stops the page script). Should the page say that no health office was found?
- Health-office deletion and corrections keep no history (REF-016). Is a history of local corrections wanted, given
  that one correction applies to every postal code of an office?
- Own records left behind (until the next host start): `SV-016-Amt A` to `SV-016-Amt H` (99011–99019 without 99014), `SV-016-Rki1 Amt`
  (99020) and the duplicate of `SV-016-Amt D` (99021).
