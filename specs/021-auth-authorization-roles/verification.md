# Verification: Authentication & authorization roles

**Spec**: 021 · **Date**: 2026-11-08 · **Build**: `ef341c1` · **Host**: ScreenshotTests
SpecVerificationHost (`http://localhost:9899`, catalog `HaemophilusWeb_Screenshots`, `host.ready` written
2026-11-08 13:49:26, no restart during the run) · **Accounts used**: `Standard1`, `Admin1`, `Rki1`, `Labor1`,
anonymous (one fresh browser context per account) · **Scope**: full; cells of the access matrix already shown by the
verifications of 001–005, 008, 009, 011, 013–020, 022–024 are cited by spec and row and re-checked only where cheap

## Summary

| Verdict | Count |
| --- | --- |
| Match | 27 |
| Spec deviation | 1 |
| Defect confirmed | 11 |
| New defect | 0 |
| Not verifiable | 4 |

By primary path: Browser 28 · File 0 · DB 0 · Unit test (existing) 2 · Unit test (added) 5 · Unit test (scratch) 3 ·
Code 1 · Not verifiable 4 (43 rows). Also test-asserted: 10 rows with a browser verdict. Test run:
`vstest.console.exe … /TestCaseFilter:"(FullyQualifiedName~…AccountControllerTests|…AuthorizationMatrixTests|…AccountViewModelsTests|…EpiscanGisExportControllerTests)&TestCategory!=Integration"`
→ 77 passed, 0 failed; full suite (`TestCategory!=Integration`) → 1325 passed, 0 failed. Scratch reproduction
`verification/Sec021Scratch.cs` → SEC-001, SEC-002, SEC-003 reproduced.

Edge cases are numbered as the list stood before this verification: EC1 "Refusal looks like sign-in", EC2 "Roles
travel with the sign-in", EC3 "Accounts without role", EC4 "Account holding Standard User and Administrator", EC5
"Role seeding", EC6 "Registration without token setting", EC7 "Role page save", EC8 "Anti-forgery tokens —
consolidated", EC9 "Helper functions reachable by address", EC10 "Generated pages", EC11 "No audit of access", EC12
"Least privilege", EC13 "Consistency with earlier specs", EC14 "Architecture". EC12 and EC13 summarize other rows and
have none of their own; EC14's coverage note was corrected without a row.

Registration was exercised on the system only without token and with the wrong token `SV-021-wrong`; the configured
`RegistrationToken` was never read, printed or sent (its key exists once in `Web.config` and once in the template,
counted). No account was created and no seeded password was changed; the password change succeeds only in unit
tests. Data created: roles "SV-021-Rolle" (through the form) and "SV-021-Rolle-ohne-Token" (POST without token) as
`Admin1`. Blocked external requests: none (no page with an automatic external call was opened; report pages were not
loaded). Geonames requests: 0.

## Results

| ID | Spec says | App/test shows | Verdict | Path | Evidence |
| --- | --- | --- | --- | --- | --- |
| FR-001 · US1.1 · US7.3 · SC-001 | Every page needs a sign-in except sign-in, registration, external-login pages and the EpiScanGIS export; redirect with return address; static files open | Anonymous: `/`, `/Home/Index`, `/Home/About`, `/Home/Change/…`, `/PatientSending/Edit/1`, `/Sender`, `/MeningoPatientSending`, `/HealthOffices`, `/RkiMatch`, `/Role`, `/Role/Create`, `/Account/Manage`, `/Geonames/LoadCountries`, `/Configuration/Edit`, `/County`, `/Patient`, `/Sending`, `/MeningoPatient`, `/MeningoSending` → 302 to `/Account/Login?ReturnUrl=<address>`; `/Account/Login`, `/Account/Register` (also `?token=SV-021-wrong`), `/Account/ExternalLoginFailure` → 200; `/Account/ExternalLoginCallback` → 302 `/Account/Login`; `/EpiscanGisExport` → 404 (023); `/ReportTemplates/BLNAR%20v10.docx`, `/Content/Site.css`, `/Images/changes/20220330.png`, `/favicon.ico` → 200 | Match | Browser | run log; `verification/US1-2-login-page.png`; also test-asserted (added): `Controllers/AuthorizationMatrixTests.GlobalFilters_RequireSignInForEveryAction`, `…OnlySignInRegistrationExternalLoginAndEpiscanGisExportAdmitAnonymousRequests` |
| FR-002 · US1.2 · US1.3 · US1.4 | Sign-in page "Anmeldung", fields, no registration/recovery/external links; required messages; "Ungültiger Benutzername oder Passwort." without saying which; local return address only | Title "Haemophilus Web - Anmeldung", "Bitte melden Sie sich an:", "Benutzername", "Passwort", "Eingeloggt bleiben?", "Anmelden"; anonymous navigation "Haemophilus", "Anmelden" only. Empty submit: "Das Feld "Benutzername" ist erforderlich.", "Das Feld "Passwort" ist erforderlich." (browser and server). Wrong password and unknown user: the same "Ungültiger Benutzername oder Passwort.". `ReturnUrl=/Sender` → `/Sender`; `ReturnUrl=http://example.invalid/x` → 302 to `/`. POST without token → HTTP 500 (anti-forgery) | Match | Browser | `verification/US1-3-required.png`, `US1-3-wrong-password.png`; also test-asserted: `Controllers/AccountControllerTests.LoginUserNotFound_AddsModelError`, `…RedirectsToLocalPage`, `…RedirectsToIndexForRemoteUrls` |
| US1.5 | "Eingeloggt bleiben?" survives the browser; otherwise session; 14 days, renewed while in use | `.AspNet.ApplicationCookie` (HttpOnly, SameSite Lax): without the option a session cookie (no expiry); with it an expiry 14.000 days after sign-in. Renewal while in use not observable in one session | Match | Browser | run log (cookie list) |
| US1.6 · SC-005 · EC11 · SEC-006 | No lockout, delay, counting or logging of failed sign-ins `[SUSPECTED-DEFECT]` | Six wrong passwords for `Rki1`: each answered in about 0.85 s with the same message; the seventh attempt with `123456` signed in. `AccessFailedCount` 0 and `LockoutEnabled` 0 for all four accounts afterwards; `Logs/2026-11-08.txt` got no line for sign-ins, failures, sign-outs or role creations (only unhandled errors) | Defect confirmed | Browser | run log; SQL `AspNetUsers` (read-only); log file scan |
| FR-003 · US1.7 | "Über" and "Abmelden (`<user>`)" for every signed-in user; sign-out with token ends the sign-in and shows the sign-in page | `Standard1`, `Admin1`, `Rki1`, `Labor1`: "Über" (`/Home/About`) and "Abmelden (`<account>`)"; the link ends on `/Account/Login?ReturnUrl=%2F` with the anonymous navigation. POST `/Account/LogOff` without token → HTTP 500 (anti-forgery), still signed in | Match | Browser | `verification/US1-7-after-logoff.png`; also test-asserted: `Controllers/AccountControllerTests.LogOff_CallsSignOutAndRedirectsToIndex` |
| US1.8 | Plain HTTP from a host other than `localhost` is redirected permanently to HTTPS | The host serves `localhost`, which the rule exempts | Not verifiable | Not verifiable | Needs a request to the app under another host name; rule "Redirect to HTTPS" present in `Web.template.config` |
| FR-005 · US2.1 · US2.2 · US2.6 · SC-003 | Standard User and Administrator: full laboratory navigation, three tiles; no difference between them | `Standard1`, `Admin1` and `Labor1`: identical lists — "Einsender", "Haemophilus", "Meningokokken", "Export" (headers "Haemophilus"/"Meningokokken"; "RKI Export", "Labor Export", "PubMLST Export", "Einsender Export"; "Labor Export", "IRIS Export", "LGA Export", "RKI Export", "Einsender Export"), "PubMLST" → "Matching (Meningokokken)", "Administration" ("Konfiguration", "Gelöschte Einsender", "Antibiotika Grenzwerte", "Gesundheitsämter"; "Gelöschte Einsendungen (Haemophilus)", "Patienten Zusammenfügen"; "Patienten Zusammenfügen"); tiles "Einsender", "Haemophilus Einsendungen", "Meningokokken Einsendungen" with six buttons | Match | Browser | `verification/US2-home-Standard1.png`, `US2-home-Admin1.png`, `US2-home-Labor1.png` |
| US2.3 | Public Health: only the two RKI export entries; one "Export" tile with "RKI Export" (Haemophilus) | `Rki1`: "RKI Export (Haemophilus)", "RKI Export (Meningokokken)", "Über", "Abmelden (Rki1)"; tile "Export" with "RKI Export" → `/PatientSending/RkiExport` | Match | Browser | `verification/US2-home-Rki1.png` |
| US2.4 · EC3 | Account without role: no entries, empty home page; reaches everything open to signed-in accounts | No role-less account exists, and one can be created only by registration (secret token) or a crafted registration | Not verifiable | Not verifiable | Would need a seeded account without role; the access part follows from the matrix test (`AuthorizationMatrixTests.AccountAndHomePagesNeedOnlyASignIn`) |
| US2.5 · US6.5 · FR-009 | No entry for user/role administration, own password, RKI match import, counties or registration; no account administration at all | None of the four accounts' navigation or home page links to `/Role`, `/Account/Manage`, `/Account/Register`, `/RkiMatch` or `/County`; "Manage Account" offers only the password change; no page lists accounts | Match | Browser | run log (link lists); `verification/US8-1-manage.png` |
| US3.1 · US3.3 | Standard User: every menu entry and tile opens | `Standard1`: all 25 navigation and tile addresses HTTP 200 (`/MeningoIsolate/PubMlstMatch` only as GET) | Match | Browser | run log; also test-asserted (added): `Controllers/AuthorizationMatrixTests.LaboratoryFunctionsAdmitStandardUserAndPublicHealthOnlyForTheRkiExports` |
| US3.2 · US5.3 · FR-010 | Role pages admit only Administrator, are not linked, store a role under the entered name | `Standard1`, `Rki1`: `/Role`, `/Role/Create` → 302 to sign-in; `Admin1`, `Labor1`: 200. "Rollen" lists "Administrator", "RKI", "Standardbenutzer" with the link "Neue Rolle anlegen"; "Neuen Rolle anlegen" ("Bitte legen Sie eine neue Rolle an:", "Name", "Neu anlegen", "Zurück zur Liste") stored "SV-021-Rolle" and returned to the list | Match | Browser | `verification/US5-3-roles-Admin1.png`, `US5-3-role-create.png`; also test-asserted (added): `Controllers/AuthorizationMatrixTests.RolePagesAdmitOnlyTheAdministratorRole` |
| US4.1 | Public Health runs both RKI exports | `Rki1`: both RKI export pages HTTP 200; export run shown in 011 (FR-016 · FR-017 row) | Match | Browser | run log; 011 `verification.md` |
| US4.2 · DEC-006 | LGA export admitted by address, not in the navigation `[SUSPECTED-DEFECT]` | `Rki1`: `/MeningoPatientSending/StateAuthorityExport` HTTP 200; no entry (direct run in 011 EC18). Observation only | Defect confirmed | Browser | run log; 011 EC18 |
| US4.3 | Public Health refused by laboratory exports, submissions, isolates, merges, deleted records, configuration, breakpoints, matching | `Rki1`: `/County`, `/County/Create`, `/County/Upload`, `/Patient`, `/Patient/Create`, `/Sending`, `/MeningoPatient`, `/MeningoSending`, `/MeningoSender`, `/Role` and the helper addresses → 302 to sign-in; the other cells by reference: 001 FR-016, 002 FR-020, 003 FR-022, 004 FR-016, 005 FR-023, 011 FR-016 · FR-017, 013 FR-011 · FR-012, 014 FR-002, 015 FR-002, 017 FR-002, 018 FR-002, 019 FR-002, 020 FR-002, 022 FR-012 | Match | Browser | run log; also test-asserted (added): `Controllers/AuthorizationMatrixTests.PublicHealthIsNamedOnlyByTheRkiAndLgaExports` |
| US4.4 · DEC-011 · SEC-004 | Report pages of any isolate admitted to Public Health, with document generation and status marking `[SUSPECTED-DEFECT]` | Shown in 008 (FR-002, EC3) and 009 (FR-002, EC11): `Rki1` opened the report pages and generated documents that set the report status; not repeated (the pages call the RKI lookup on load) | Defect confirmed | Browser | 008 and 009 `verification.md` |
| US4.5 · SEC-004 | All health-office pages admitted to Public Health `[SUSPECTED-DEFECT]` | `Rki1` and `Admin1`: `/HealthOffices`, `/HealthOffices/Create` HTTP 200; create by `Rki1` shown in 016 (FR-015 row) | Defect confirmed | Browser | run log; 016 `verification.md` |
| US4.6 · US7.5 · SEC-004 | RKI match import admitted to Public Health, unlinked, no token, writes match states `[SUSPECTED-DEFECT]` | `Rki1` and `Admin1`: `/RkiMatch` HTTP 200, linked nowhere; upload by `Rki1` processed and form without token shown in 024 (FR-001 rows) | Defect confirmed | Browser | run log; 024 `verification.md` |
| US5.1 · DEC-001 | Administrator-only account refused by every laboratory menu entry and tile `[SUSPECTED-DEFECT]` | `Admin1`: 22 of the 25 navigation and tile addresses (all except home, "Über" and "Gesundheitsämter") → 302 to sign-in, also county and generated pages and helpers. Observation only | Defect confirmed | Browser | `verification/US2-home-Admin1.png`; run log |
| US5.2 · US5.4 · US7.4 | Any signed-in account admitted to home, "Über", change entries, own password page, report pages, health offices, RKI match import, country list | `Admin1` and `Rki1`: `/`, `/Home/About`, `/Home/Change/2026-07-08`, `/Account/Manage`, `/HealthOffices`, `/RkiMatch`, `/Geonames/LoadCountries` HTTP 200; report pages in 008/009 FR-002 | Match | Browser | run log; also test-asserted (added): `Controllers/AuthorizationMatrixTests.AccountAndHomePagesNeedOnlyASignIn` |
| FR-004 · US5.5 | Exactly three access-relevant roles; further roles grant nothing | Every role check of every action names only "Standardbenutzer", "Administrator" or "RKI"; the catalog holds those three plus the two SV-021 roles, which no account can receive without registration | Match | Unit test (added) | `Controllers/AuthorizationMatrixTests.EveryRoleCheckNamesOnlyTheThreeFixedRoles`; SQL `AspNetRoles` |
| FR-006 · SC-002 | Each function admits exactly the roles of the access matrix | Today's probes for `Standard1`, `Admin1`, `Rki1`, `Labor1` and anonymous agree with every row (cells above); remaining cells by the references in this table | Match | Browser | run log; also test-asserted (added): `Controllers/AuthorizationMatrixTests` (7 methods, disputed cells left out) |
| FR-007 · US6.1 | Without or with a wrong token the sign-in page is shown at the registration address, no message; nothing links there | `/Account/Register` and `?token=SV-021-wrong`: page "Anmeldung" at the registration address, form posts to `/Account/Login`, no message; no page links to the address | Match | Browser | `verification/US6-1-register-wrong-token.png`; also test-asserted (added): `Controllers/AccountControllerTests.Register_WithoutConfiguredToken_ShowsLoginView` |
| US6.2 | Correct token: form "Neuen Benutzer registrieren" with a role choice of all stored roles | The configured token opens the form with every stored role as choice (synthetic token in the test). Labels not observed: the system form needs the secret | Match | Unit test (added) | `Controllers/AccountControllerTests.Register_WithConfiguredToken_ShowsFormWithAllStoredRoles` |
| FR-008 · US6.3 | User name, password, role required; 6–100 characters; confirmation must match; no other rule; unique names of letters, digits, "@", "_", "." | Required members and length/compare rules as stated; "abcdef" accepted, "abcde" refused; "sv.021@x_y" accepted, "SV 021", "SV-021" and a taken name refused | Match | Unit test (added) | `Models/AccountViewModelsTests.RegisterViewModel_Validation`, `…_PasswordMaximumLength`; `Controllers/AccountControllerTests.UserManagerDefaults_DecideWhichAccountsCanBeCreated` |
| FR-007 · US6.4 · SC-004 | Valid input: account created, exactly the chosen role, browser signed in as the new account, home page | One `AddToRoleAsync` with the new account's id and the chosen role, non-persistent sign-in with the new identity, redirect to Home/Index; a rejected account shows the form again without role or sign-in | Match | Unit test (added) | `Controllers/AccountControllerTests.Register_Valid_CreatesAccountAssignsChosenRoleSignsInAsNewAccountAndRedirectsHome`, `…Register_CreateFails_ShowsFormAgainWithoutRoleAssignmentOrSignIn` |
| US6.6 · SEC-001 | Registration POST accepted without the token `[SUSPECTED-DEFECT]` | The POST action takes only the form model; called without any prior token request it creates the account, assigns the role and redirects home | Defect confirmed | Unit test (scratch) | `verification/Sec021Scratch.cs` |
| US6.7 · SEC-002 | Unknown role: account created, then error page, account left without role `[SUSPECTED-DEFECT]` | With a role assignment that throws, the account was created first, the exception is unhandled, no delete | Defect confirmed | Unit test (scratch) | `verification/Sec021Scratch.cs` |
| EC6 · SEC-003 | Missing setting opens registration and the EpiScanGIS export `[SUSPECTED-DEFECT]` | Setting absent: a request without token gets the registration form; the export part reproduced in 023 | Defect confirmed | Unit test (scratch) | `verification/Sec021Scratch.cs`; 023 `verification.md` |
| FR-012 · US7.1 · SC-006 | EpiScanGIS export without sign-in with the token, "not found" otherwise | Anonymous `/EpiscanGisExport` → 404; with the token and with wrong tokens test-asserted (023 FR-002 row) | Match | Unit test (existing) | `Controllers/EpiscanGisExportControllerTests.Download_ReturnsFileContent`, `…DownloadWithMissingOrDifferentToken_Returns404` |
| US7.2 · SEC-007 | One secret opens registration and the export and travels in the address `[SUSPECTED-DEFECT]` | Both open with the value of `RegistrationToken`, passed as query value `token`; server logs not checkable | Defect confirmed | Unit test (existing) | `Controllers/EpiscanGisExportControllerTests` (setup); also `AccountControllerTests.Register_WithConfiguredToken_ShowsFormWithAllStoredRoles` |
| US8.1 · FR-011 | Own password page by address for any signed-in account, English "Manage Account" | `Standard1`: "Manage Account", "You're logged in as Standard1.", "Change Password Form", "Current password", "New password", "Confirm new password", "Change password", "Use another service to log in.", "There are no external authentication services configured. …"; `Admin1`, `Rki1`: HTTP 200 | Match | Browser | `verification/US8-1-manage.png`; also test-asserted (added): `Controllers/AuthorizationMatrixTests.AccountAndHomePagesNeedOnlyASignIn` |
| US8.2 | Correct current password changes it ("Ihr Passwort wurde geändert."); otherwise errors in English | Success path and message in tests; on the system (no change made): "Incorrect password."; "The New password must be at least 6 characters long." and "The new password and confirmation password do not match."; POST without token → HTTP 500 | Match | Unit test (added) | `Controllers/AccountControllerTests.Manage_WithCorrectCurrentPassword_ChangesPasswordAndRedirectsWithSuccessMessage`, `…Manage_WithWrongCurrentPassword_ShowsPageAgainWithError`, `Models/AccountViewModelsTests.ManageUserViewModel_Validation`; `verification/US8-2-wrong-current-password.png`, `US8-2-short-password.png` |
| US8.3 | Other sign-ins stay valid after a password change | Needs a password change on the system, which only an own account may undergo; none can be created without the token | Not verifiable | Not verifiable | Would need a seeded disposable account |
| US8.4 | External-login functions have no effect | "Manage Account" states that no external services are configured; anonymous `/Account/ExternalLoginCallback` → 302 `/Account/Login`; `/Account/ExternalLoginFailure` "Login Failure." / "Unsuccessful login with service."; signed in, `/Account/LinkLoginCallback` → 302 `/Account/Manage?Message=Error` | Match | Browser | run log |
| FR-013 · EC1 · UX-002 | Refusal leads to the sign-in page with return address; signing in again leads back; background requests get "not authorized" | `Rki1` → `/Sender`: `/Account/Login?ReturnUrl=%2FSender`, page shows `Rki1`'s navigation above the form; signing in again: same page. Background requests (`X-Requested-With`) to `/Sender` and `/PatientSending/DataTableAjax`: HTTP 200, empty body, header `X-Responded-JSON` `{"status":401,"headers":{"location":"…/Account/Login?ReturnUrl=…"}}`; without the header a POST gets 302 | Spec deviation | Browser | `verification/EC1-refusal-Rki1-Sender.png`; run log |
| EC2 | Roles are read at sign-in; a database change takes effect only after the next sign-in | Needs a role change in the catalog; SQL access is read-only and no page changes roles | Not verifiable | Not verifiable | Would need a fixture account whose role the host changes during a session |
| EC4 | An account holding Standard User and Administrator is admitted to all Standard User functions and the role pages | `Labor1` (all three roles, seeded outside the application): every laboratory address and `/Role`, `/Role/Create` HTTP 200 | Match | Browser | run log |
| EC5 · REF-015 | Role seeding code exists but is never called `[POTENTIAL-REFACTORING]` | `ApplicationDbContext.InitializeDatabase` (calls `SeedRoles`) has no caller in the application | Match | Code | `HaemophilusWeb/Models/IdentityModels.cs`, `InitializeDatabase`/`SeedRoles` |
| EC7 · REF-015 | Role save without anti-forgery check and rules; empty or used name ends on the error page `[POTENTIAL-REFACTORING]` | POST of only `Name` without token stored "SV-021-Rolle-ohne-Token" (302 to `/Role`); empty name and "RKI" through the form → HTTP 500 "Validation failed for one or more entities. See 'EntityValidationErrors' property for more details." | Match | Browser | `verification/EC7-role-empty-name.png`; SQL `AspNetRoles` |
| EC8 · SEC-005 | Token verified on sign-in, registration, sign-out, password change and the CRUD forms; not on the listed actions `[SUSPECTED-DEFECT]` | Sign-in, sign-out, password change without token → HTTP 500; role save without token stored (new for SEC-005); the other cells by reference: 002 and 003 (submission saves), 008 and 009 (status marking), 011, 013 (exports), 014, 015 (merges), 016 (lookup), 020 (restore), 022 (postal-code lookups), 024 (import) | Defect confirmed | Browser | run log; earlier `verification.md` files |
| EC9 · SEC-011 | Helper functions reachable by address for their controller's role, acting without form or token (derived) `[POTENTIAL-REFACTORING]` | `/Patient/CreatePatient`, `/Sending/AssignStemAndLaboratoryNumber/1`, `/MeningoSending/CreateSendingAndAssignStemAndLaboratoryNumber` → 302 to sign-in for `Admin1`, `Rki1`; not called as Standard User (each call changes data); country list in 022 EC14 | Match | Browser | run log |
| EC10 · REF-022 | Generated patient, sending and county pages only by address, Standard User only | `Standard1`: `/Patient`, `/Patient/Create`, `/Sending`, `/MeningoPatient`, `/County`, `/County/Create` HTTP 200; `/MeningoSending` HTTP 500 (view does not compile, REF-022), `/MeningoSender` HTTP 500 (no view, REF-008); `Admin1`, `Rki1`: all 302 to sign-in | Match | Browser | run log |

## Unit tests added

- `HaemophilusWebTests/Controllers/AccountControllerTests.cs`: `Register_WithoutConfiguredToken_ShowsLoginView` (3
  cases, US6.1), `Register_WithConfiguredToken_ShowsFormWithAllStoredRoles` (US6.2),
  `Register_Valid_CreatesAccountAssignsChosenRoleSignsInAsNewAccountAndRedirectsHome` (US6.4, SC-004),
  `Register_CreateFails_ShowsFormAgainWithoutRoleAssignmentOrSignIn` (US6.3),
  `UserManagerDefaults_DecideWhichAccountsCanBeCreated` (6 cases, FR-008),
  `Manage_WithCorrectCurrentPassword_ChangesPasswordAndRedirectsWithSuccessMessage` (US8.2),
  `Manage_WithWrongCurrentPassword_ShowsPageAgainWithError` (US8.2) — 14 cases in 7 methods.
- `HaemophilusWebTests/Controllers/AuthorizationMatrixTests.cs` (new): `GlobalFilters_RequireSignInForEveryAction`,
  `OnlySignInRegistrationExternalLoginAndEpiscanGisExportAdmitAnonymousRequests`,
  `EveryRoleCheckNamesOnlyTheThreeFixedRoles`, `RolePagesAdmitOnlyTheAdministratorRole`,
  `AccountAndHomePagesNeedOnlyASignIn` (5 cases), `LaboratoryFunctionsAdmitStandardUserAndPublicHealthOnlyForTheRkiExports`,
  `PublicHealthIsNamedOnlyByTheRkiAndLgaExports` — 11 cases in 7 methods. The Administrator cells (DEC-001), the LGA
  export for Public Health (DEC-006) and the functions without role check (SEC-004, SEC-011) are deliberately not
  asserted.
- `HaemophilusWebTests/Models/AccountViewModelsTests.cs` (new): `RegisterViewModel_Validation` (6 cases),
  `RegisterViewModel_PasswordMaximumLength` (2), `ManageUserViewModel_Validation` (4) — 12 cases in 3 methods.

In total 37 test cases in 17 methods. Not committed: `verification/Sec021Scratch.cs` (scratch reproduction of SEC-001,
SEC-002 and the registration part of SEC-003; compiled with `csc` against `HaemophilusWebTests\bin\Debug`).

## Spec corrections applied

- Edge case "Refusal looks like sign-in": refused background requests get HTTP 200 with an empty body and an
  `X-Responded-JSON` header carrying status 401 (was: the "not authorized" status, derived); the sign-in page then shows
  the navigation of the signed-in account.
- US1.5: the cookie lifetime is now stated as observed (14 days with "Eingeloggt bleiben?", session cookie without);
  the renewal while in use is marked as not observed (was: derived).
- US7.3: static files observed without sign-in (was: derived from configuration, not exercised).
- US6.3: the account rules cite unit tests (was: derived, not exercised); US6.6 and US6.7 reproduced at unit level with
  their backlog IDs (was: derived, not exercised); edge cases "Registration without token setting" (unit level in 023
  and 021) and "Role page save" (observed errors) updated likewise.
- Edge case "Architecture": the coverage note names the tests added for registration, password change, account rules
  and role checks; the navigation by role stays uncovered.
- Assumptions: no longer "No running system was exercised"; the framework-default and derived items say what this
  verification observed or reproduced.

## Backlog updates

- DEC-001: observed across all menus (observation only, no decision).
- DEC-006: observed again (observation only).
- SEC-001, SEC-002: reproduced at unit level 2026-11-08 (021 verification).
- SEC-003: reproduced at unit level for the registration form.
- SEC-004: observed again for `/RkiMatch` and the health-office pages.
- SEC-005: confirmed on system for the role page save.
- SEC-006: confirmed on system (no lockout, counter, delay or log line).
- SEC-007: reproduced at unit level (one setting for both, token in the query).
- SEC-011: helper addresses refuse `Admin1` and `Rki1`.
- UX-002: confirmed on system, including the answer to background requests.
- REF-015: confirmed (no caller of the seeding code; role save errors).
- TEST-001: registration, role checks and password change covered by tests added in the 021 verification; navigation
  by role stays untested.

## Not verifiable here

- US1.8: the HTTPS redirect exempts `localhost`; needs a request under another host name.
- US2.4 · EC3: no account without role; needs a seeded role-less account.
- US8.3: needs a password change of a disposable account; none can be created without the token.
- EC2: needs a role change in the catalog during a session; no page changes roles and SQL access is read-only.

## Open questions for the owner

- Seed extension: a disposable account without role and one more Standard User account (e.g. `NoRole1`,
  `Pwd1`) in the host would let US2.4, US8.3 and the system part of the password change be verified without the
  registration token.
- Report templates and the other static files are delivered without sign-in (FR-001, observed). The templates contain
  no patient data; should they stay public (Constitution Principle IV, "every non-public route")? No backlog entry was
  created.
- The roles "SV-021-Rolle" and "SV-021-Rolle-ohne-Token" stay in the catalog until the next host start; they appear only
  in the role choice of the registration form.
