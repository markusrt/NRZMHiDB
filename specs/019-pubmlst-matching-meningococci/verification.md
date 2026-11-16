# Verification: PubMLST matching (Meningococci)

**Spec**: 019 · **Date**: 2026-11-08 · **Build**: `7bb4b55` · **Host**: ScreenshotTests
SpecVerificationHost (`http://localhost:9899`, catalog `HaemophilusWeb_Screenshots`) ·
**Accounts used**: `Standard1`, `Labor1`, `Admin1`, `Rki1`, anonymous · **Scope**: full; the matching submission, the
result page and the remote databases on the unit-test path (external service, never called).

## Summary

| Verdict | Count |
| --- | --- |
| Match | 18 |
| Spec deviation | 2 |
| Defect confirmed | 10 |
| New defect | 2 |
| Not verifiable | 3 |

By primary path: Browser 9 · File 0 · DB 2 · Unit test (existing) 2 · Unit test (added) 8 · Unit test (scratch) 8 ·
Code 3 · Not verifiable 3 (adds up to the row count). Also test-asserted: 1 row with a browser verdict. Test run:
`vstest.console.exe … /TestCaseFilter:"(FullyQualifiedName~HaemophilusWeb.Services.PubMlstMatcherTests|…NeisseriaPubMlstServiceTests|…IrisPubMlstServiceTests|…Controllers.PubMlstControllerTests|…Controllers.MeningoIsolateControllerTests|…Utils.AppSettingsExtensionsTests)&TestCategory!=Integration"`
→ 40 passed, 0 failed (16 of them added); full suite with `TestCategory!=Integration` → 1195 passed, 0 failed. The
filter keeps the four explicit live-service tests (`[Category("Integration")]`) out of every run, so no test contacted
PubMLST.

**External services**: PubMLST and IRIS were not contacted. In the browser every request to the two PubMLST addresses
was stopped by a page route before it left the browser: 6 × `POST /MeningoIsolate/PubMlstMatch` (held and aborted, to
read the button state and the posted fields) and 5 × `POST /PubMlst/NeisseriaIsolates/` (3 answered with synthetic
data or HTTP 404/500, 2 aborted). The skill's context route block caught nothing (the page routes came first); no
browser request went to `pubmlst.org`. Geonames requests: 0. In the scratch run (S11), the process configuration
routed all HTTP traffic to a listener on 127.0.0.1 that answered 502; it received one `CONNECT rest.pubmlst.org:443`
from the unchanged batch wiring, which therefore never left the machine (a guard request to a reserved `.invalid` host
proved the routing first). The four `Iris*` keys exist in `Web.config` and `Web.template.config` (count 4 each); no
value was read or shown, and every test uses synthetic credentials.

"Unit test (scratch)" means: the real `PubMlstMatcher`, `NeisseriaPubMlstService`, `IrisPubMlstService`,
`PubMlstController` and `MeningoIsolateController` run with stubbed remote answers or substitutes and own in-memory sets,
in a console program compiled in the session scratchpad against `HaemophilusWebTests\bin\Debug` and not committed,
because a committed test would cement the defects. Source, configuration and output:
`verification/019-scratch-tests.cs`, `verification/Scratch019.exe.config`, `verification/019-scratch-tests-output.txt`
(git-ignored, 18 checks, all as described; referred to as S1–S12).

Edge cases are numbered in the order of the spec's list before this verification: EC1 Exact reference search, EC2 What
is sent to PubMLST, EC3 Values copied, not linked, EC4 Periods and bounds, EC5 Default period is the current year, EC6
Heading versus selection, EC7 Deleted sendings are included, EC8 No ordering of the result, EC9 Previous matches are
never removed, EC10 Different database order, EC11 Local record identified by PubMLST id only, EC12 Fixed field set on
update, EC13 Partial results on failure, EC14 Unhandled remote failures abort the run, EC15 Duplicate locus, EC16 Long
synchronous run, EC17 Isolate saved in full, EC18 Misleading log entry, EC19 Stem number on the individual lookup, EC20
Client-provided values on save, EC21 Authorization, EC22 Architecture. FR-015 (documentation scope) states no behavior
and has no row.

Records created: none. Form changes on `/MeningoIsolate/Edit/5`, `/6` and `/7` (MZ001/26–MZ003/26) were never saved;
no seeded record was changed.

## Results

| ID | Spec says | App/test shows | Verdict | Path | Evidence |
| --- | --- | --- | --- | --- | --- |
| FR-001 · US1.1 | Menu "PubMLST" with the single entry "Matching (Meningokokken)" for Standard User and Administrator, not for Public Health. | `Standard1`, `Labor1`, `Admin1`: menu "PubMLST" with only "Matching (Meningokokken)" → `/MeningoIsolate/PubMlstMatch`; `Rki1`: no such menu or link. | Match | Browser | `verification/019-FR-001-admin-menu.png`; run results |
| FR-002 · US4.1 · US4.2 · SC-006 | Matching page and lookup admit only Standard User; others refused; anonymous sent to sign in. | `GET /MeningoIsolate/PubMlstMatch`: `Standard1`, `Labor1` HTTP 200; `Admin1`, `Rki1` and anonymous 302 to `/Account/Login?ReturnUrl=%2FMeningoIsolate%2FPubMlstMatch`. The lookup address was not requested (route block); its POST-only role rule is asserted by a test. | Match | Browser | crafted GETs in own contexts; also test-asserted: `Controllers/MeningoIsolateControllerTests.Controller_RequiresStandardRole`, `Controllers/PubMlstControllerTests.NeisseriaIsolates_IsPostForStandardRole` (added) |
| EC21 · DEC-001 | Administrator sees the menu but is refused. [SUSPECTED-DEFECT] | `Admin1` sees "PubMLST" → "Matching (Meningokokken)"; the page redirects it to sign-in. Observation only; DEC-001 stays open. | Defect confirmed | Browser | `verification/019-FR-001-admin-menu.png` |
| FR-003 · US1.2 · EC6 | Page "PubMLST Matching", heading "Entnahmedatum", "Start-Datum"/"Ende-Datum" = 1 January/31 December of the current year, hint, button "Abfrage der PubMLST Daten". | Title "PubMLST Matching", heading "Entnahmedatum", "Start-Datum" 01.01.2026, "Ende-Datum" 31.12.2026, hint "Hinweis: Die Abfrage nimmt einige Zeit in Anspruch. Die Seite lädt automatisch neu sobald alle Daten von PubMLST geladen sind.", button "Abfrage der PubMLST Daten". | Match | Browser | `verification/019-US1-2-matching-form.png` |
| FR-003 (token) · US4.3 | The submission verifies the anti-forgery token. | The form posts `__RequestVerificationToken`, `From`, `To`, `primary-submit` (held in the browser); the POST action carries the token check. A POST without token was not sent (route block). | Match | Unit test (added) | `Controllers/MeningoIsolateControllerTests.PubMlstMatchPost_ValidatesAntiForgeryToken`; posted field names from the held request |
| US1.3 · UX-007 | Button changes to "Abfrage läuft ..."; no progress, no cancel. | After the click the button reads "Abfrage läuft ..." and is also disabled (not in the spec); no progress indication or cancel control. The waiting itself was not run (submission aborted). | Spec deviation | Browser | `verification/019-US1-3-running.png` |
| FR-004 · US1.4 · EC4 (bounds) | Sampling date in the period, or receiving date without sampling date; bounds inclusive; deleted sendings not excluded; start after end selects nothing. | Tests: start and end day included, day after end excluded, receiving date used only without sampling date (a sampling date before the period excludes the isolate even when received within it), start after end selects nothing without remote call. SQL: all 26 Meningococci sendings store dates at 00:00, so the end day is included. | Match | Unit test (added) | `Services/PubMlstMatcherTests.Match_SelectsBySamplingDateOrWithoutItByReceivingDate_BoundsInclusive`, `Match_StartDateAfterEndDate_SelectsNothing`; existing `Match_FromToMatchingXDaysRecord_ReturnsListWithXEntries`; `sqlcmd` result in session |
| EC4 (empty dates) | An empty or unreadable date is not reported; the run proceeds with the earliest possible date. | The form has no check: `From=` and `From=abc` (end 31.12.2026) and `To=` were posted without a message (held in the browser). Binding gives 0001-01-01 with an invalid model state that the action ignores (S12); an empty start date then selects an isolate sampled in 1999. On the system the default "Start-Datum" left empty would match every isolate up to the end date against PubMLST. | New defect | Unit test (scratch) | `verification/019-EC4-empty-start-date.png`; S12 → UX-016 |
| EC7 · DATA-016 | Isolates of deleted sendings are selected. [SUSPECTED-DEFECT] | An isolate whose sending is soft-deleted was selected, matched and linked (S1). On the system a 2020 run would include MZ003/20 (soft-deleted fixture, stem number 3); not run. | Defect confirmed | Unit test (scratch) | S1; `sqlcmd` result in session |
| FR-005 · US1.5 · EC1 | Prefixed stem number searched, public database first, IRIS only if not found; only exactly one record counts. | Two records → not found, details not fetched; zero records → not found; primary found → secondary not asked; primary not found → secondary asked. The unchanged batch wiring asks the public database first (S11). Whether PubMLST matches the name exactly is the remote side and was not observed. | Match | Unit test (added) | `Services/NeisseriaPubMlstServiceTests.GetIsolateByReference_MoreThanOneRecord_ReturnsNullWithoutFetchingDetails`; existing `GetIsolateByReference_InexistingIsolate_ReturnsNull`, `PubMlstMatcherTests.Match_WithResultOnPrimaryService_DoesNotCallSecondary`, `Match_OnSecondaryService_FindsMatchAndCreatesPubMlstIsolate`; S11 |
| EC10 · DATA-016 | Batch asks public then IRIS, the lookup IRIS then public. [SUSPECTED-DEFECT] | `PubMlstController()` holds `IrisPubMlstService, NeisseriaPubMlstService` (S10); the lookup's first action is the IRIS session request (S11b); the batch action's first request is the public search (S11). | Defect confirmed | Unit test (scratch) | S10, S11, S11b |
| FR-006 · US1.7 · SC-002 | Without stem number no remote request; listed as not found with "DE -". | Isolate without stem number: no service call, row with stem number "DE -" and no PubMLST id. The page wording "kein Treffer" was not observable (see FR-010). | Match | Unit test (existing) | `Services/PubMlstMatcherTests.Match_WithEmptyStemNumber_DoesNotCallPubMlstService` |
| FR-007 · Copied PubMLST values | Details and allele designations fetched; 17 values copied; empty for a missing locus, no value for missing ST, cc, reactivity. | All 17 values from a simulated record, `NadA` "" for the missing locus `NadA_peptide`; missing `fields`/`schemes`/phenotypic values → null. The details and allele addresses of the found record are read (`…/isolates/93683`, `…/allele_ids?return_all=1`). | Match | Unit test (existing) | `Services/NeisseriaPubMlstServiceTests.GetIsolateById_ExistingIsolate_AllFieldsAreSet`, `GetIsolateByReference_FieldsMissing_OtherValuesAreSet`, `GetIsolateByReference_SchemesPropertyNull_OtherValuesAreSet`; added `GetIsolateByReference_SendsOnlyReferenceAndReadsFoundRecord` |
| FR-008 · US3.1 · US3.2 · US3.3 · SC-004 · TEST-001 | New PubMLST id → new local record; known id → 17 values overwritten (also with empty values), id and database kept. | Known id: the existing record (same object, same local id) gets all remote values including "" and null, keeps id and database; no second record; another isolate linked to it shows the new values. New id: record created (existing tests). | Match | Unit test (added) | `Services/PubMlstMatcherTests.Match_KnownPubMlstId_OverwritesTypingValuesOfExistingRecord`, `Match_SecondRun_SearchesMatchedIsolateAgainWithoutSecondRecord`; existing `Match_WithCorrespondingPupMlstRecord_FindsMatchAndCreatesPubMlstIsolate` |
| EC11 · DATA-016 | Local record found by PubMLST id only; an IRIS record with the id of a public one overwrites it and keeps the other database name. [SUSPECTED-DEFECT] | IRIS record 18377 overwrote the stored public record 18377 ("SV-iris"), kept "pubmlst_neisseria_isolates", the result row showed that name, and both isolates share the record (S2). | Defect confirmed | Unit test (scratch) | S2 |
| SC-003 · DATA-036 | At most one local record per PubMLST id. | 40 of the 41 local PubMLST records carry PubMLST id 0 (empty records from saves without match, 005); the batch path itself keeps one record per real id (FR-008 row). | Defect confirmed | DB | `sqlcmd` result in session |
| FR-009 · US1.6 · US1.8 · EC9 | Each found isolate linked and saved before the next; isolates not found keep their link and values. | Order observed: search DE1, save, search DE2, save; both isolates linked to their records. An isolate not found keeps its linked record and values, and nothing is saved. | Match | Unit test (added) | `Services/PubMlstMatcherTests.Match_SavesEachFoundIsolateBeforeSearchingTheNext`, `Match_IsolateNotFound_KeepsExistingLinkAndValues` |
| FR-010 · US1.9 · US1.10 · SC-001 · EC8 | Result page "PubMLST Matching abgeschlossen" with buttons, summary, table, marks, links; database order. | The page is rendered only by the matching submission, which would contact PubMLST, so it was not shown. The row data behind it (one row per selected isolate, PubMLST id only when found, "DE -", laboratory number, isolate id, database) is asserted by the existing matcher tests. | Not verifiable | Not verifiable | — |
| FR-011 · US2.1 | Lookup: first record found wins, "not found" when none, no database write. | First service without record → second asked and its record returned; first found → second not asked; none → HTTP 404. The lookup controller has no database dependency. The order IRIS → public is the EC10 row. | Match | Unit test (added) | `Controllers/PubMlstControllerTests.NeisseriaIsolates_FirstServiceWithoutRecord_ReturnsRecordOfSecondService`, `NeisseriaIsolates_FirstServiceFindsRecord_DoesNotAskSecond`, `NeisseriaIsolates_NoServiceFindsRecord_ReturnsNotFound`; existing `NeisseriaIsolates_ExistingIsolate_FieldsAreSet` |
| US2.2 | Found: "Isolat wurde geladen", typing fields, PubMLST id and database filled, link enabled; nothing stored. | Synthetic IRIS record 99019 answered in the browser on MZ001/26: "Isolat wurde geladen", fields filled ("SV-ST", "SV-cc", "SV-1", `NadA` empty), hidden id 99019 and database "pubmlst_neisseria_iris", link "PubMLST Eintrag anzeigen" enabled → `https://pubmlst.org/bigsdb?page=info&db=pubmlst_neisseria_iris&id=99019`. | Match | Browser | `verification/019-US2-2-lookup-found.png` (answer simulated) |
| US2.3 · UX-017 | Not found or failure: "Isolat wurde nicht gefunden", typing fields emptied, PubMLST id 0. | As stated, and the database is emptied too; but the link keeps its state: after the found answer on MZ001/26 a 404 left "PubMLST Eintrag anzeigen" enabled with id 99019; on MZ003/26 (stored record 99001) an HTTP 500 left "PubMLST Eintrag" enabled with id 99001. | New defect | Browser | `verification/019-US2-3-lookup-notfound.png`, `019-US2-3-lookup-500-stale-link.png` (answers simulated) → UX-017 |
| US2.4 · EC20 | The save stores the posted values without contacting PubMLST again. | MZ003/26 is linked to record 99001 ("pubmlst_neisseria_iris", ST "SV-100", PorA VR1 "SV7-2"), stored in the 005 verification from a synthetic lookup answer that exists nowhere on PubMLST; a refetch on save could not have produced these values. | Match | DB | form values on `/MeningoIsolate/Edit/7`; 005 `verification.md` FR-021 |
| EC19 · UX-013 | Lookup uses the stem number as loaded; without stem number "DE -". | MZ001/26: stem number changed to 5999 in the form, "Abfragen" still posted `isolateReference=DE5001`; MZ002/26 (no stem number) posted `isolateReference=DE -` and showed "Isolat wurde nicht gefunden" after the abort. | Defect confirmed | Browser | held requests in run results |
| FR-012 · US3.4 · US3.5 | IRIS session at most once per run; each database at most once per isolate; every run searches again. | Two IRIS lookups: one session request, five GETs in total; not found → each database asked exactly once; a second run searches the matched isolate again and keeps one record. | Match | Unit test (added) | `Services/IrisPubMlstServiceTests.GetIsolateByReference_TwoLookups_EstablishSessionOnce`, `PubMlstMatcherTests.Match_IsolateNotFound_AsksEachDatabaseOnce`, `Match_SecondRun_SearchesMatchedIsolateAgainWithoutSecondRecord` |
| FR-013 · EC3 · US3.6 · SC-005 | Isolate form, reports, exports use only the local copy and do not contact PubMLST. | Opening `/MeningoIsolate/Edit/5`, `/6`, `/7` sent no lookup request until "Abfragen"; no browser request went to `pubmlst.org`. The exports print the stored values (011 verification: "PubMLST ID" 0 for empty records). Only `PubMlstController` and the matching action create PubMLST clients. | Match | Browser | request log in run results; 011 `verification.md` (DATA-036 rows) |
| FR-014 · EC2 | Only the prefixed stem number (plus IRIS credentials) is sent; credentials from four settings. | The search posts exactly `{"field.isolate": "DE14505"}` to `…/pubmlst_neisseria_isolates/isolates/search`; the four `Iris*` settings fill the IRIS authorization; IRIS addresses carry the consumer key and session token (existing). | Match | Unit test (added) | `Services/NeisseriaPubMlstServiceTests.GetIsolateByReference_SendsOnlyReferenceAndReadsFoundRecord`, `Utils/AppSettingsExtensionsTests.GetIrisAuthentication_ReadsFourNamedSettings`; existing `IrisPubMlstServiceTests.GetIsolateByReference_AddsAuthenticationParameters` |
| EC5 · UX-007 | Current year although the internal name suggests "last year". [POTENTIAL-REFACTORING] | `MeningoIsolateController.PubMlstMatch()`: `var lastYear = DateTime.Now.Year;` (form shows 2026). | Match | Code | `Controllers/MeningoIsolateController.cs`, `PubMlstMatch()` |
| EC12 · REF-002 | 17-field update list kept twice, marked for clean-up. [POTENTIAL-REFACTORING] | `PubMlstMatcher.CreateOrUpdatePubMlstIsolate` (17 fields, "TODO fix this duplication") and `MeningoIsolateController.MapPubMlstData` (17 fields plus `Database`). | Match | Code | files named |
| EC13 · EC14 · DATA-016 | Unhandled failures end the run; earlier isolates keep their data; details "not found" handled. [SUSPECTED-DEFECT] | "Not found" on the details → next database asked (S5). A "not found" status of the search (WebException), a connection failure (AggregateException), an IRIS answer to rejected credentials (ArgumentNullException) each end the run; the isolate found before stays saved (S3, S4, S6; S11 with the unchanged wiring). Without IRIS settings the lookup fails before any request (S11b; not in the spec). A failing lookup (HTTP 500 simulated) shows "Isolat wurde nicht gefunden". | Defect confirmed | Unit test (scratch) | S3–S6, S11, S11b; `verification/019-US2-3-lookup-500-stale-link.png` |
| EC15 · DATA-016 | A locus listed twice ends the run. [SUSPECTED-DEFECT] | `PorA_VR1` twice in the allele designations → ArgumentException ends the run (S7). Whether PubMLST sends such records was not observed. | Defect confirmed | Unit test (scratch) | S7 |
| EC16 | Long synchronous run, up to three requests per database and isolate, platform time limit. | Needs a run against PubMLST with many isolates; not done. | Not verifiable | Not verifiable | — |
| EC17 · DATA-016 | Each matched isolate is written back in full; concurrent edits can be lost. [SUSPECTED-DEFECT] | Needs a real run with Entity Framework and a second user saving during the run. The matcher marks the whole isolate as modified (`_db.MarkAsModified(isolate)`), which the EF wrapper turns into `EntityState.Modified`. | Not verifiable | Not verifiable | `Services/PubMlstMatcher.cs`, `Models/ApplicationDbContextWrapper.cs` |
| EC18 · DATA-016 | "Failed to match isolate by reference …" logged as error also for successes. [SUSPECTED-DEFECT] | Successful match of DE1: log entry of level Error, "Failed to match isolate by reference DE1" (S8). | Defect confirmed | Unit test (scratch) | S8 |
| US4.4 · EC21 (token) · SEC-005 | The lookup accepts requests without token. | The isolate form posts only `isolateReference=DE5001` (held in the browser); the lookup action has no token check (S9). Acceptance on the server was not requested (route block). | Defect confirmed | Unit test (scratch) | S9; held requests |
| EC22 · REF-003 · TEST-001 | Matching service tested except update, result page, matching page; one IRIS live test on request; services created in the controller. [POTENTIAL-REFACTORING] | Four explicit live-service tests (three `NeisseriaPubMlstServiceTests`, one `IrisPubMlstServiceTests`), not one; the update of an existing record is now tested (FR-008 row); the action creates `PubMlstMatcher`, both clients and the credentials itself, `PubMlstController()` creates both clients. | Spec deviation | Code | `HaemophilusWebTests/Services/*PubMlstServiceTests.cs`; `Controllers/MeningoIsolateController.cs`, `Controllers/PubMlstController.cs` |

## Unit tests added

16 test cases, all in existing fixtures except one new file:

- `HaemophilusWebTests/Services/PubMlstMatcherTests.cs`: `Match_SelectsBySamplingDateOrWithoutItByReceivingDate_BoundsInclusive`
  (FR-004), `Match_StartDateAfterEndDate_SelectsNothing` (EC4), `Match_KnownPubMlstId_OverwritesTypingValuesOfExistingRecord`
  (FR-008), `Match_IsolateNotFound_KeepsExistingLinkAndValues` (FR-009), `Match_SavesEachFoundIsolateBeforeSearchingTheNext`
  (FR-009), `Match_IsolateNotFound_AsksEachDatabaseOnce` (FR-012), `Match_SecondRun_SearchesMatchedIsolateAgainWithoutSecondRecord`
  (FR-012, SC-004); own in-memory sets in an NSubstitute context
- `HaemophilusWebTests/Services/NeisseriaPubMlstServiceTests.cs`: `GetIsolateByReference_MoreThanOneRecord_ReturnsNullWithoutFetchingDetails`
  (FR-005), `GetIsolateByReference_SendsOnlyReferenceAndReadsFoundRecord` (FR-014, FR-007)
- `HaemophilusWebTests/Services/IrisPubMlstServiceTests.cs`: `GetIsolateByReference_TwoLookups_EstablishSessionOnce` (FR-012)
- `HaemophilusWebTests/Controllers/PubMlstControllerTests.cs`: `NeisseriaIsolates_FirstServiceWithoutRecord_ReturnsRecordOfSecondService`,
  `NeisseriaIsolates_FirstServiceFindsRecord_DoesNotAskSecond`, `NeisseriaIsolates_NoServiceFindsRecord_ReturnsNotFound` (FR-011),
  `NeisseriaIsolates_IsPostForStandardRole` (FR-002)
- `HaemophilusWebTests/Controllers/MeningoIsolateControllerTests.cs`: `PubMlstMatchPost_ValidatesAntiForgeryToken` (FR-003, US4.3)
- `HaemophilusWebTests/Utils/AppSettingsExtensionsTests.cs` (new, added to `HaemophilusWebTests.csproj`):
  `GetIrisAuthentication_ReadsFourNamedSettings` (FR-014; synthetic values)

The database order of batch and lookup (DATA-016) and the absent token check of the lookup (SEC-005) got no committed
test, because it would cement the defects; they are in the scratch checks.

## Spec corrections applied

- US1.3: the button is also disabled while the run waits (was: changes to "Abfrage läuft ..." only).
- US2.3: the database is emptied too, and the link to the PubMLST entry keeps its previous target and enabled state
  (new `[SUSPECTED-DEFECT]`, UX-017).
- Edge case "Periods and bounds": the form has no check; an empty or unreadable start date matches every earlier
  isolate, an empty end date none; stored dates carry no time of day (was: "derived from the unchecked form input";
  new `[SUSPECTED-DEFECT]`, UX-016).
- Edge case "Unhandled remote failures abort the run": without IRIS settings the lookup fails before any request and
  never asks the public database (added).
- Edge case "Architecture": four live-service tests (was: one IRIS test); the update of an existing local record is now
  tested (was: untested).
- Cross-reference notes for confirmed defects from other specs: "Stem number on the individual lookup" (005 US3.3,
  UX-013), "Client-provided values on save" (005 US3.5, DATA-036), "Authorization" (021, SEC-005).
- Assumptions: sending dates without time of day are now observed; new bullet on what the verification ran.

## Backlog updates

- DEC-001: observation for the PubMLST matching recorded (no decision).
- SEC-005: reproduced at unit level for the PubMLST lookup (form posts no token, action has no check).
- DATA-016: reproduced at unit level (deleted sendings, database order, cross-database overwrite, aborts with partial
  saves, duplicate locus, error log for successes; lookup without IRIS settings); concurrent-edit overwrite not
  reproduced.
- DATA-036: source extended with 019; confirmed on system (40 of 41 local records with PubMLST id 0).
- UX-007: confirmed on system (current-year default, button disabled, no progress or cancel; result order not
  observable).
- UX-013: source extended with 019; confirmed on system (`isolateReference=DE -` for MZ002/26).
- TEST-001: PubMLST update of an existing local record covered by tests added in the 019 verification; result page and
  matching page stay untested.
- NEW UX-016: PubMLST matching runs with empty or unreadable dates without notice.
- NEW UX-017: failed PubMLST lookup leaves the link to the previous PubMLST entry enabled.

## Not verifiable here

- FR-010, US1.9, US1.10, SC-001, EC8: the result page appears only after the matching submission, which contacts
  PubMLST. A period that selects only isolates without stem number (e.g. 20.08.2022 to 20.08.2022, which currently selects
  only MZ022/26, a record of an earlier verification) would render it without any remote request, but the submission is blocked for this verification; with the owner's consent it could
  be run once.
- EC16: run time and request time limit need a real run against PubMLST with many isolates.
- EC17: the concurrent-edit overwrite needs a real run with Entity Framework and a second user.
- EC1 (remote side): whether PubMLST matches the isolate name exactly needs the external service.

## Open questions for the owner

- May a later verification send the matching submission once for a period that contains only isolates without stem
  number (no remote request by construction), to verify the result page (FR-010)?
- UX-016: should the matching refuse empty or unreadable dates, and should it limit the period (an empty start date
  matches the whole history against PubMLST in one request)? Not a `DEC-*` item; recorded as a bug.
