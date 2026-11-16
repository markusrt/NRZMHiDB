# Verification: Postal-code lookup (Geonames)

**Spec**: 022 · **Date**: 2026-11-08 · **Build**: `27f3211` · **Host**: ScreenshotTests
SpecVerificationHost (`http://localhost:9899`, catalog `HaemophilusWeb_Screenshots`) ·
**Accounts used**: `Standard1`, `Rki1`, `Admin1` · **Scope**: full; failure paths of the external service on the
unit-test path and with synthetic answers injected in the browser.

## Summary

| Verdict | Count |
| --- | --- |
| Match | 18 |
| Spec deviation | 3 |
| Defect confirmed | 6 |
| New defect | 1 |
| Not verifiable | 0 |

By primary path: Browser 19 · File 0 · DB 1 · Unit test (existing) 1 · Unit test (added) 1 · Unit test (scratch) 3 ·
Code 3 · Not verifiable 0 (adds up to the row count). Also test-asserted: 5 rows with a browser verdict. Test run:
`vstest.console.exe … /TestCaseFilter:"FullyQualifiedName~HaemophilusWeb.Services.GeonamesServiceTest|FullyQualifiedName~HaemophilusWeb.Controllers.GeonamesControllerTests"`
→ 23 passed, 0 failed (10 of them added); full suite → 1179 passed, 0 failed.

**Real Geonames requests**: 17, all sent by the forms as `Standard1` (the application's configured account; its value
was neither read nor shown; the key exists once in `Web.config` and `Web.template.config`, checked by count):
"91301", "06773", "06773 Gräfenhainichen" (chosen suggestion), "00000" (unknown) on the Haemophilus submission form,
each as one suggestion and one lookup request; "9", "91", "913" typed key by key (suggestions only); "A-1010" (foreign,
odd) and "97080" on the Meningococci submission form; "13353" on the sender form. The refused requests of `Rki1` and
`Admin1` did not reach the service. Every other answer used below was injected in the browser with a page route
(`page.route` … `fulfill`), so the server and the service were not involved; the evidence says so. Blocked external
URLs (route block of the skill): none.

"Unit test (scratch)" means: the real `GeonamesService` with a stubbed HTTP function, run in a console program
compiled in the session scratchpad against `HaemophilusWebTests\bin\Debug` and not committed, because a committed test
would cement the defects DATA-041/DATA-023 or the unencoded parameters of UX-003. Source and output:
`verification/022-scratch-tests.cs`, `verification/022-scratch-tests-output.txt` (git-ignored, 10 checks, all as
described).

Edge cases are numbered in the order of the spec's list before this verification: EC1 Foreign residence, EC2 Values
written as data, EC3 Existing values on edit, EC4 Longer input, EC5 One request per keystroke, EC6 Service
unavailable or failing, EC7 Service error answers, EC8 Place names in suggestions, EC9 Unencoded parameters, EC10
Unencrypted connection, EC11 Missing setting, EC12 Duplicated statement, EC13 Fields matched by name ending, EC14
Access, EC15 Architecture. FR-013 (documentation scope) states no behavior and has no row.

Records created: sender `SV-022-Alpha` (id 8, "13353 Berlin"). Unsaved form input only otherwise; no seeded record was
changed.

## Results

| ID | Spec says | App/test shows | Verdict | Path | Evidence |
| --- | --- | --- | --- | --- | --- |
| FR-001 | Sender create/edit, both submission forms (create/edit) and both patient pages offer suggestions and lookup on "Postleitzahl". | `/Sender/Create`, `/Sender/Edit/8`, `/PatientSending/Create`, `/PatientSending/Edit/1`, `/MeningoPatientSending/Create`, `/MeningoPatientSending/Edit/1`, `/Patient/Edit/1`, `/MeningoPatient/Edit/1`: field with `list="postalCodeSearchresults"`, the datalist, and two `input` handlers on the field. | Match | Browser | run results in session; screenshots `022-US1-*.png` |
| FR-002 · US2.1 · US2.2 · US2.3 | Every non-empty change asks for postal codes starting with the content and offers `<postal code> <place>`; service number/order, no limit (assumed). After choosing a suggestion the lookup takes the chosen place. | "9", "91", "913" each sent a suggestion request; "913" gave 29 suggestions ("91301 Forchheim", "91320 Ebermannstadt", …), "06773" 12, unsorted. Choosing "06773 Gräfenhainichen" wrote "06773" back in the same step and took "Gräfenhainichen"; the suggestion request for "06773 Gräfenhainichen" returned `{"postalCodes":[]}`, so the list was empty afterwards (not in the spec). | Spec deviation | Browser | `022-US2-1-typed-913-suggestions.png`, `022-US2-3-suggestion-chosen.png`; also test-asserted: `Services/GeonamesServiceTest.QueryPostalCodePrefix_AsksSearchInGermanyAndPassesAnswerThrough` |
| FR-003 · US2.4 · US2.5 | Change to ≠ 5 characters empties place and county, sets "n.n."; emptying in one step changes nothing. | Choosing a suggestion: "Wohnort"/"Landkreis" empty and state `Unknown` before any answer. "913" after "Unbekannt": fields emptied, "n.n.". Emptying the field with one change: 0 requests, "Unbekannt"/"n.n." kept. | Match | Browser | run results; `022-US2-1-typed-913-suggestions.png` |
| FR-004 · US1.1 · US1.2 | From 5 characters: split at the first space, write back the postal code, ask for the places of the postal code in Germany. | Five-character inputs sent `postalCode=91301&placeName=`; "06773 Gräfenhainichen" sent `postalCode=06773&placeName=Gräfenhainichen` and the field showed "06773" at once. Four characters ("913") sent no lookup. | Match | Browser | request log; also test-asserted: `Services/GeonamesServiceTest.QueryPostalCode_AsksLookupForPostalCodeInGermany` |
| FR-005 · US1.3 · US1.4 · SC-002 | Exact place-name match, otherwise none; without place name the first place. | "06773" without choice took "Gossa" (first of the lookup answer), while the first suggestion was "Gräfenhainichen"; with the chosen suggestion "Gräfenhainichen" was taken. The difference between the two orders was not in the spec. | Spec deviation | Browser | `022-US1-3-06773-first-place.png`, `022-US2-3-suggestion-chosen.png`; also test-asserted: `QueryPostalCode_MultipleMatchesWithoutPlaceName_ReturnsFirstEntryWithItsFields`, `QueryPostalCodeAndPlaceName_WithoutExactlyEqualName_ReturnsEmptyArray` (4 cases), existing `QueryPostalCodeAndPlaceName_MultipleMatches_ReturnsBestMatch` |
| FR-006 · US1.5 · US1.6 · US1.9 · SC-001 · SC-003 | Place taken: postal code, place, county as delivered, state by short code; none: "Unbekannt"/"n.n."; absent fields skipped. | "91301": "Forchheim", "Landkreis Forchheim", "Bayern" (`BY`); "06773": "Gossa", "Anhalt-Bitterfeld", "Sachsen-Anhalt" (`ST`); "97080" (Meningococci): "Würzburg", "Würzburg", "Bayern"; "00000": "Unbekannt", "Unbekannt", "n.n.". Sender "13353": only "Ort" = "Berlin" (the form has no fields ending in County/State). | Match | Browser | `022-US1-1-91301-haemophilus.png`, `022-US1-5-97080-meningo.png`, `022-US1-6-00000-unknown.png`, `022-US1-9-sender-13353.png`; also test-asserted: `QueryPostalCode_MultipleMatchesWithoutPlaceName_ReturnsFirstEntryWithItsFields` |
| FR-007 · US1.7 · US3.4 · SC-004 | No message to the user; console "Postal code query result: …"; "Land" never changed. | No alert or validation message in any case (only the Beta banner). Console: "Postal code query result:{…}" without a space. "Land" = "Österreich" stayed after "A-1010" and after "97080" (which set "Bayern"). | Spec deviation | Browser | console log in run results; `022-EC1-foreign-A-1010-meningo.png`, `022-US1-5-97080-meningo.png` |
| US1.8 · EC2 | Filled fields stay editable and are saved like typed values. | "Ort" not read-only; sender `SV-022-Alpha` saved with "13353"/"Berlin" (SQL: `Senders` id 8). The export county matching of EC2 belongs to 011/025 and was not repeated. | Match | DB | `sqlcmd` result in session |
| FR-008 · EC6 (failures) · DATA-041 | "Not found" yields an empty result; other failures, including answers not in the expected format, end with the generic server error. | A JSON status answer (account/quota message) is not a failure: the lookup returns it with `"postalcodes":[]`, and the form writes "Unbekannt"/"n.n." as for an unknown postal code, without notice (browser, answer injected). Only a network failure (`AggregateException`) or a non-JSON answer (`JsonReaderException`) propagates (HTTP 500). The live service answers an unknown code with an empty list, not with "not found". | Defect confirmed | Unit test (scratch) | `verification/022-scratch-tests-output.txt` (checks 1–2, 5–8); browser: quota answer injected → "Unbekannt"/"n.n."; existing `QueryPostalCode_UnexpectedResult_ReturnsEmptyArray` asserts the empty list for `{"foo":"bar"}` |
| EC6 (stale values) | Failing request: fields keep what US2.4 left in them. | After "91301" (Forchheim), a change to "97080" with a synthetic HTTP 500 for both requests left "Forchheim", "Landkreis Forchheim", "Bayern" next to "97080"; no message. With "970801" the fields were emptied first. | New defect | Browser | `022-EC6-server-error-five-chars.png` (answers injected in the browser) → DATA-042 |
| FR-009 · EC11 | The account name comes from `GeonamesAccount`; without the setting an empty account name is sent. | Key present once in `Web.config` and `Web.template.config` (count only). Without the setting (test configuration) both service addresses carry `&username=&`. | Match | Unit test (added) | `Services/GeonamesServiceTest.QueryPostalCode_WithoutAccountSetting_SendsEmptyAccountName` |
| FR-010 · US3.1 · US3.2 · US3.3 · US3.5 · SC-005 | "Land": 250 countries, German names, ordered by name, three-letter code, "Deutschland" preselected; no country on the Haemophilus form, default `DEU`; file from 2020-01-09. | Meningococci create/edit: label "Land", 250 options ("AFG:Afghanistan", "EGY:Ägypten", "ALA:Alandinseln", …, "CYP:Zypern"), culture-aware order, `DEU` selected. Haemophilus forms: no country field. SQL: all 34 Haemophilus patients `DEU`. `git log` of `countries.json`: one commit, 2020-01-09. | Match | Browser | run results; also test-asserted: `Controllers/GeonamesControllerTests.LoadCountries_ReadsCountryFileAndOrdersByName`, `PatientWithoutCountry_DefaultsToDeu` |
| FR-011 · US4.1 | The EpiScanGIS export asks for coordinates with postal code and "Wohnort", under the place rule. | Existing tests from the 023 verification assert the call with "91301"/"Forchheim", the place matching "Wohnort" with the real lookup component, and the dropped case without coordinate. | Match | Unit test (existing) | `Controllers/EpiscanGisExportControllerTests.Download_QueriesCoordinateWithPatientPostalCodeAndCity`, `DownloadWithGeonamesService_UsesPlaceMatchingCity`, `DownloadWithoutCoordinateForOneCase_ReturnsOneLineLess` |
| US4.2 · SC-006 · DATA-020 | A "Wohnort" spelled differently or "Unbekannt" excludes the case without notice. [SUSPECTED-DEFECT] | Reproduced at unit level in the 023 verification ("Graefenhainichen" and "Unbekannt" left out); not repeated here. The place rule behind it is asserted by `QueryPostalCodeAndPlaceName_WithoutExactlyEqualName_ReturnsEmptyArray("Unbekannt")`. | Defect confirmed | Unit test (scratch) | 023 `verification/023-scratch-tests-output.txt`; DATA-020 |
| FR-012 (roles) | Only the Standard User role is admitted to both requests and the forms. | `Rki1` and `Admin1`: `POST /Geonames/PostalCode/` and `/Geonames/PostalCodeStartsWith/` → 302 to `/Account/Login?ReturnUrl=…`; `/PatientSending/Create` → 302. | Match | Browser | crafted requests in own contexts (route block installed) |
| FR-012 (token) · EC14 · SEC-005 | No anti-forgery token is verified. | The forms post only `postalCode`/`placeName` and `postalCodePrefix`, without token, and get HTTP 200 with the answer. | Defect confirmed | Browser | request log (17 requests) |
| EC1 · DATA-022 | A foreign postal code overwrites "Wohnort"/"Landkreis" and resets "Ausland". [SUSPECTED-DEFECT] | Meningococci form with "Ausland", "Österreich", "SV-022-Wien": "A-1010" → "Unbekannt", "Unbekannt", "n.n.", "Land" still "Österreich". The rejected save (003 state/country rule) was not attempted. | Defect confirmed | Browser | `022-EC1-foreign-A-1010-meningo.png` |
| EC3 | Opening an edit page does not run the lookup. | Five edit pages opened: 0 requests; stored values (e.g. "97070", "Würzburg", empty county, "n.n.") unchanged. | Match | Browser | request counter in run results |
| EC4 | Five or more characters without a space are sent as postal code; typically "Unbekannt"/"n.n.". | "A-1010" sent as `postalCode=A-1010`; result "Unbekannt"/"n.n.". | Match | Browser | `022-EC1-foreign-A-1010-meningo.png` |
| EC5 · UX-003 | One request per keystroke, no delay; the last answer to arrive decides (not exercised). | "9", "91", "913": three requests; five-character changes two each. With a delayed answer injected for "11111" and a fast one for "22222", the late answer replaced "22222" by "11111" and its place. | Match | Browser | `022-EC5-out-of-order-answers.png` (answers injected) |
| EC7 · DATA-023 | The suggestion script expects `postalCodes`; the fallback and status answers cause a script error and remove the suggestions. [SUSPECTED-DEFECT] | Injected `{"postalcodes":[]}` and a status message: "Cannot read properties of undefined (reading 'length')", list emptied. The server passes a status answer through unchanged and returns `{"postalcodes":[]}` after "not found" (scratch checks 3–4). Live empty results (`{"postalCodes":[]}`) cause no error. | Defect confirmed | Browser | `022-DATA-023-fallback-script-error.png`; `022-scratch-tests-output.txt` |
| EC8 · SEC-008 | Place names are inserted without escaping. [SUSPECTED-DEFECT] | Injected names: "Sankt Peter's Ort" → "12371 Sankt Peter"; "SV' data-sv022='1" added an attribute; `X' /><b id='sv022'>…` added a `<b>` element to the page. | Defect confirmed | Browser | `022-SEC-008-unescaped-suggestions.png` (answer injected) |
| EC9 · UX-003 | Typed content is passed unencoded. [POTENTIAL-REFACTORING] | "97080&country=AT" → `…&postalcode=97080&country=AT`; "970#x" → `…&postalcode_startsWith=970#x`. The browser's form post encodes space and umlaut itself. | Match | Unit test (scratch) | `022-scratch-tests-output.txt` (checks 9–10) |
| EC10 · SEC-009 | The service is called over plain HTTP. [POTENTIAL-REFACTORING] | `GeonamesService.BaseUrl = "http://api.geonames.org"`; the host's outbound traffic is not observable from the browser. | Match | Code | `HaemophilusWeb/Services/GeonamesService.cs`, `BaseUrl` |
| EC12 · EC6 (log text) | "Unbekannt" set twice; "not found" logged as "$Failed to call Geonames …". [POTENTIAL-REFACTORING] | `_QueryPostalCodeViaGeonames.cshtml`: `PopulateCityInput("Unbekannt")` twice; `GeonamesService.QueryByPostalCode`/`QueryByPostalCodePrefix`: `Log.Error(e, "$Failed …")`. | Match | Code | files named |
| EC13 | Fields matched by id ending; each occurs once on the current forms. | Submission forms: `Patient_PostalCode`, `Patient_City`, `Patient_County`, `Patient_State` (and `Patient_Country`), once each; sender form: `PostalCode`, `City` only. | Match | Browser | run results |
| EC14 · REF-009 · SEC-011 | Country list reachable as an address for any signed-in account, answers with a type name (not exercised). | `Rki1`, `Admin1`: `GET /Geonames/LoadCountries` → HTTP 200, `text/html`, ``System.Collections.Generic.List`1[HaemophilusWeb.Models.Country]``. | Match | Browser | crafted requests |
| EC15 · REF-003 · TEST-002 | Service created by concrete type; patient page group instantiates the lookup page group; misleading test name. | `GeonamesController()` news `GeonamesService`; `MeningoPatientController` calls `geonamesController.LoadCountries()`; `QueryPostalCode_MultipleMatches_ReturnsAllEntries` asserts one entry. | Match | Code | `Controllers/GeonamesController.cs`, `Controllers/MeningoPatientController.cs`, `Services/GeonamesServiceTest.cs` |

## Unit tests added

- `HaemophilusWebTests/Services/GeonamesServiceTest.cs`:
  `QueryPostalCode_MultipleMatchesWithoutPlaceName_ReturnsFirstEntryWithItsFields` (FR-005, FR-006),
  `QueryPostalCodeAndPlaceName_WithoutExactlyEqualName_ReturnsEmptyArray` (4 cases: "gossa", "Goss", "Gossa ",
  "Unbekannt"; FR-005), `QueryPostalCode_AsksLookupForPostalCodeInGermany` (FR-004),
  `QueryPostalCodePrefix_AsksSearchInGermanyAndPassesAnswerThrough` (FR-002),
  `QueryPostalCode_WithoutAccountSetting_SendsEmptyAccountName` (FR-009, EC11)
- `HaemophilusWebTests/Controllers/GeonamesControllerTests.cs`: `LoadCountries_ReadsCountryFileAndOrdersByName`
  (FR-010), `PatientWithoutCountry_DefaultsToDeu` (FR-010)

10 test cases in 7 methods. No test was written for DATA-041, DATA-042, DATA-023 or the unencoded parameters.

## Spec corrections applied

- FR-008 and EC6: was "a 'not found' answer yields an empty result; an answer not in the expected format ends with the
  generic server error"; now: unknown codes come as an empty list, any JSON answer without places (status messages)
  yields an empty result ("Unbekannt"/"n.n.", DATA-041), only network failures and non-JSON answers end in HTTP 500,
  and a failed five-character change keeps the previous place (new `[SUSPECTED-DEFECT]`, DATA-042).
- US2.3: added that the suggestion list is empty after choosing a suggestion.
- US2.2 and Assumptions: number and order of suggestions observed instead of assumed.
- US1.3: added that the first place of the lookup can differ from the first suggestion ("Gossa" vs "Gräfenhainichen").
- US1.7: console text quoted as "Postal code query result:…" (no space).
- EC5, EC7, EC8, EC14: "not exercised" replaced by the observed behavior; EC14 names SEC-005 and the address.
- EC15: coverage note updated for the added tests.
- Scope note and Assumptions: the verification and its evidence kinds are named.

## Backlog updates

- SEC-005: confirmed on system 2026-11-08 for both postal-code lookups.
- SEC-008: confirmed on system 2026-11-08 for the suggestion list (synthetic answer).
- DATA-022: confirmed on system 2026-11-08.
- DATA-023: confirmed on system 2026-11-08 (synthetic answers; server side at unit level).
- DATA-041: extended to the interactive lookup; reproduced at unit level 2026-11-08 (022 verification).
- NEW DATA-042: failed postal-code lookup leaves the previous place next to a new postal code.
- UX-003: observations added (request counts, exceptions, unencoded parameters).
- REF-009: country-list address observed on system.

## Not verifiable here

- None. The live service's behavior on quota exhaustion, time-outs and outages could not be produced without
  harming the owner's account; those paths rest on the scratch reproduction and injected browser answers.

## Open questions for the owner

- DATA-041/DATA-042: should a failing or quota-exhausted lookup be shown to the user instead of writing "Unbekannt"
  or leaving the previous place? (Possible `DEC-*` candidate, since "Unbekannt" is written into patient data.)
- The live service sends 29 suggestions for three digits and is asked on every keystroke (17 requests for this
  verification alone); is the account's quota a known constraint in production?
