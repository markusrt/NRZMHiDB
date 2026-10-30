# Feature Specification: Authentication & authorization roles

**Feature Branch**: `speckit-init`

**Created**: 2026-10-22

**Status**: Draft (status-quo inventory)

**Input**: User description: "Feature title: \"Authentication & authorization roles\" (backlog ref E1, Group E — Cross-cutting). Document the current, as-implemented behavior of authentication and role-based authorization as it exists today, covering the roles User, Administrator, and PublicHealth and what each can access (e.g. PublicHealth is limited to RKI exports). Read HaemophilusWeb/Startup.cs, the auth configuration, DefaultRoles, the [Authorize] attributes on controllers, and the role checks in Views/Shared/_Layout.cshtml. Capture sign-in, roles, and access boundaries. Document observable behavior only — do not propose changes."

> **Scope note**: This is an *as-is* specification (backlog ref E1). It records the current, observable behavior of
> signing in and out, registering an account with the `RegistrationToken` setting, changing the own password, the
> three roles Standard User (`Standardbenutzer`), Administrator, and Public Health (`RKI`), how each role is assigned
> and carried, which navigation each role is offered, and which functions each role is actually admitted to — as a
> complete access matrix over all pages of the application, including the functions that need no sign-in or no role.
> It consolidates the role and anti-forgery findings of specs 008, 009, 011, 013, 014, 015, 016, 017, 018, 019, and
> 020 by reference. It does **not** propose changes. Out of scope and referenced instead: what each function does
> (specs 001–020); the content of the exports (specs 011, 012, 013); the content of the configuration settings
> (spec 018). Secret values of settings (for example `RegistrationToken`) are named but never reproduced.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Sign in and sign out (Priority: P1)

A laboratory employee opens the application, is asked to sign in with user name and password, works, and signs out
again.

**Why this priority**: Every page except a handful of explicitly open ones requires a signed-in account; sign-in is
the gate to all patient and laboratory data (Constitution Principle IV).

**Independent Test**: Without a session, request the home page and a deep page (for example a submission's edit
page); verify that the sign-in page is shown, sign in, and verify that the originally requested page opens. Sign in
with a wrong password several times and verify the message and that the account is not blocked. Sign out and verify
that the sign-in page is shown again.

**Acceptance Scenarios**:

1. **Given** no signed-in session, **When** any page other than the sign-in page, the registration page, the
   inactive external-login pages, or the EpiScanGIS export is requested, **Then** the browser is sent to the sign-in
   page "Anmeldung" ("Bitte melden Sie sich an:"), carrying the requested address as return address.
2. **Given** the sign-in page, **Then** it offers "Benutzername", "Passwort", the checkbox "Eingeloggt bleiben?", and
   the button "Anmelden"; it offers no link to registration, to password recovery, or to external sign-in providers.
   The navigation of an anonymous visitor shows only "Haemophilus" (home) and "Anmelden".
3. **Given** a missing user name or password, **When** "Anmelden" is selected, **Then** the page is shown again with a
   required-field message; **given** a wrong combination, **Then** it is shown again with "Ungültiger Benutzername oder
   Passwort." — the message does not reveal which of the two was wrong.
4. **Given** valid credentials, **When** "Anmelden" is selected, **Then** the user is signed in and sent to the return
   address if it points into the application, otherwise to the home page.
5. **Given** "Eingeloggt bleiben?" was ticked, **Then** the sign-in survives closing the browser; otherwise it lasts
   for the browser session. In both cases the sign-in expires after a period of inactivity set by the framework
   default (14 days, renewed while in use; derived, not configured by the application).
6. **Given** repeated wrong passwords for one user name, **Then** the account is never locked, no delay is added, and
   no failed attempt is counted or logged. [SUSPECTED-DEFECT]
7. **Given** a signed-in user, **Then** the navigation shows, at the right, "Über" (the change log) and "Abmelden
   (<user name>)"; **When** "Abmelden" is selected, **Then** the sign-in ends (the request carries an anti-forgery
   token) and, because the home page needs a sign-in, the sign-in page is shown.
8. **Given** the application is reached over plain HTTP from a host other than `localhost`, **Then** it is redirected
   permanently to HTTPS before sign-in (server rewrite rule).

---

### User Story 2 - See the navigation of the own role (Priority: P1)

After signing in, each user is offered the menus and home-page tiles of the role the account holds.

**Why this priority**: The navigation is the only place where roles become visible to users; it defines what each
role is expected to use.

**Independent Test**: Sign in once with a Standard User, an Administrator-only, a Public Health, and an account
without role; compare navigation and home page with the table in FR-005.

**Acceptance Scenarios**:

1. **Given** an account holding the Standard User or the Administrator role, **When** any page is shown, **Then** the
   navigation offers "Einsender", "Haemophilus", "Meningokokken", the menu "Export" (Haemophilus: "RKI Export", "Labor
   Export", "PubMLST Export", "Einsender Export"; Meningokokken: "Labor Export", "IRIS Export", "LGA Export", "RKI
   Export", "Einsender Export"), the menu "PubMLST" ("Matching (Meningokokken)"), and the menu "Administration"
   ("Konfiguration", "Gelöschte Einsender", "Antibiotika Grenzwerte", "Gesundheitsämter"; Haemophilus: "Gelöschte
   Einsendungen (Haemophilus)", "Patienten Zusammenfügen"; Meningokokken: "Patienten Zusammenfügen").
2. **Given** the same accounts, **When** the home page is shown, **Then** it offers the tiles "Einsender",
   "Haemophilus Einsendungen", and "Meningokokken Einsendungen" with their list and create buttons.
3. **Given** a Public Health account that holds neither of the two roles above, **When** any page is shown, **Then**
   the navigation offers only "RKI Export (Haemophilus)" and "RKI Export (Meningokokken)"; the home page offers a
   single tile "Export" with the button "RKI Export" (Haemophilus only).
4. **Given** an account holding none of the three roles, **Then** the navigation offers no entries besides "Über" and
   "Abmelden", and the home page is empty below its title.
5. **Given** any role, **Then** the navigation offers no entry for user or role administration, for the account's own
   password change, for the RKI match import, for counties, or for registration.
6. **Given** the navigation decision, **Then** it treats Standard User and Administrator alike; no menu entry is shown
   for one of them only.

---

### User Story 3 - Work as a Standard User (Priority: P1)

A laboratory employee with the Standard User role uses every laboratory function: senders, submissions of both
pathogens, isolates, reports, exports, PubMLST, configuration, breakpoints, health offices, deleted records, and
merges.

**Why this priority**: Nearly every function of the application admits only this role; it is the working role of
the laboratory.

**Independent Test**: With a Standard User account, open every entry of every menu and verify that none is refused.

**Acceptance Scenarios**:

1. **Given** a Standard User, **When** any menu entry or home-page button is selected, **Then** the page opens; no
   offered function refuses the role.
2. **Given** a Standard User, **When** the role pages ("Rollen") are requested by direct address, **Then** they are
   refused, because they admit only the Administrator role.
3. **Given** a Standard User, **Then** the role is the only one admitted to the laboratory functions listed in the
   access matrix (FR-007) as "Standard User only"; it is also admitted to everything open to any signed-in account.

---

### User Story 4 - Work as a Public Health user (Priority: P1)

A user of the public-health authority (role `RKI`) signs in to download the two RKI exports.

**Why this priority**: This is the only external user group; its boundary decides which patient data leaves the
laboratory.

**Independent Test**: With a Public Health account, run both RKI exports from the navigation, then request by direct
address the LGA export, the other exports, a submission list, a report page of an isolate, the health-office pages,
and the RKI match import; compare with the access matrix.

**Acceptance Scenarios**:

1. **Given** a Public Health user, **When** "RKI Export (Haemophilus)" or "RKI Export (Meningokokken)" is selected,
   **Then** the export page opens and the export can be run (spec 011).
2. **Given** a Public Health user, **When** the LGA export of the Meningococci is requested by direct address,
   **Then** it is admitted and runs, although it is not offered in this role's navigation (spec 011).
   [SUSPECTED-DEFECT]
3. **Given** a Public Health user, **When** a Laboratory, PubMLST, IRIS, or sender export, a submission list or edit
   page, an isolate page, a merge, a deleted-records page, the configuration, the breakpoints, or the PubMLST matching
   is requested by direct address, **Then** it is refused.
4. **Given** a Public Health user, **When** the report page of any Haemophilus or Meningococci isolate is requested by
   direct address with the isolate's internal number, **Then** it is admitted: the page shows the patient designation,
   laboratory number, sampling location, laboratory results, and interpretation, can generate the report document
   (which contains patient and sender data), and marks the isolate's report status as preliminary or final when a
   report is generated (specs 008, 009). [SUSPECTED-DEFECT]
5. **Given** a Public Health user, **When** the health-office pages are requested by direct address, **Then** all of
   them, including create, edit, and delete, are admitted (spec 016). [SUSPECTED-DEFECT]
6. **Given** a Public Health user, **When** the RKI match import page is requested by direct address and a file is
   uploaded, **Then** it is admitted and imports RKI match states into Haemophilus submissions (see User Story 7).
   [SUSPECTED-DEFECT]

---

### User Story 5 - Use an Administrator account (Priority: P2)

A user whose account holds only the Administrator role signs in, sees the full laboratory navigation, and tries to
use it.

**Why this priority**: The name suggests the most powerful role; in fact it is admitted to very few functions, which
earlier specs repeatedly observed and which is consolidated here.

**Independent Test**: With an Administrator-only account, select every menu entry and verify the outcome; request the
role pages by direct address.

**Acceptance Scenarios**:

1. **Given** an Administrator-only account, **When** "Einsender", "Haemophilus", "Meningokokken", any "Export" entry,
   "Matching (Meningokokken)", "Konfiguration", "Gelöschte Einsender", "Antibiotika Grenzwerte", either "Gelöschte
   Einsendungen"/"Patienten Zusammenfügen" entry, or a home-page tile button is selected, **Then** the request is
   refused, because each of these functions admits only the Standard User role (specs 001–005, 011, 013, 014, 015,
   017, 018, 019, 020). [SUSPECTED-DEFECT]
2. **Given** an Administrator-only account, **When** "Gesundheitsämter" is selected, **Then** the pages open and can
   be used, because they require only a sign-in (spec 016).
3. **Given** an Administrator-only account, **When** "Rollen" is requested by direct address, **Then** the list of
   stored roles is shown with the button "Neue Rolle anlegen"; the page "Neuen Rolle anlegen" stores a role under the
   entered name. These two pages are the only pages that admit the Administrator role specifically; no menu links to
   them.
4. **Given** an Administrator-only account, **Then** it is additionally admitted to everything open to any signed-in
   account (report pages, health offices, RKI match import, own password page; see the access matrix).
5. **Given** a new role created on "Neuen Rolle anlegen", **Then** it grants access to nothing, because every access
   check names one of the three fixed roles; it only appears in the role choice of the registration page.

---

### User Story 6 - Register a new account (Priority: P2)

An operator who knows the value of the `RegistrationToken` setting opens the registration page, creates an account
for a colleague, and chooses its role.

**Why this priority**: Registration is the only way an account and its role come into existence within the
application; it decides which role each account holds.

**Independent Test**: Request the registration address with a wrong token and with the correct token; register an
account with each of the three roles; sign in with each and verify the navigation of User Story 2. Inspect the stored
role assignments and verify that each account holds exactly one role.

**Acceptance Scenarios**:

1. **Given** the registration address is requested without the token or with a wrong token, **Then** the sign-in
   page is shown (at the registration address); no message is given. No page links to the registration address.
2. **Given** the correct token, **When** the page "Neuen Benutzer registrieren" is shown, **Then** it offers
   "Benutzername", "Passwort", "Passwort bestätigen", "Rolle" (a choice "Auswählen..." of all roles stored in the
   database), and the button "Neuen Benutzer anlegen".
3. **Given** the form, **Then** user name, password, and role are required; the password must be 6 to 100 characters
   long ("Das Passwort muss mindestens 6 Zeichen lang sein."), and the confirmation must match ("Die beiden Passwörter
   stimmen nicht überein."). No other password rule (complexity, history, expiry) applies. A user name that is
   already taken or contains characters other than letters, digits, "@", "_", and "." is rejected with the framework's
   default message (derived from framework defaults; not exercised).
4. **Given** valid input, **When** "Neuen Benutzer anlegen" is selected, **Then** the account is created, the chosen
   role is assigned to it, the browser is signed in **as the new account** (ending the session of whoever filled in
   the form), and the home page is shown.
5. **Given** registration is the only function that assigns roles, **Then** every account created in the application
   holds exactly one role; no page adds a second role, removes or changes a role, lists accounts, resets a password,
   or disables or deletes an account. The stored data would allow several roles per account; such accounts can only
   arise outside the application. This confirms the assumption of specs 011, 013, 014, 017, 018, 019, and 020 for
   accounts created in the application.
6. **Given** a registration submitted directly, without first opening the form with the correct token, **Then** it is
   accepted, because only the display of the form checks the token, not the submission; the submission checks only
   the anti-forgery token, which any visitor obtains from the sign-in page (derived from code; not exercised).
   [SUSPECTED-DEFECT]
7. **Given** a submitted role name that is not a stored role (possible only by a crafted submission), **Then** the
   account is created first and the role assignment then fails with the generic error page, leaving an account
   without role (derived from code; not exercised). [SUSPECTED-DEFECT]

---

### User Story 7 - Functions outside the role model (Priority: P2)

Some functions need no sign-in, or a sign-in but no particular role.

**Why this priority**: These are the places where the boundary of the three roles does not apply; they are the
exceptions to Constitution Principle IV's "every non-public route MUST be protected by … appropriate authorization".

**Independent Test**: Request each function in the "No sign-in" and "Any signed-in account" rows of the access matrix
without a session and with a Public Health account.

**Acceptance Scenarios**:

1. **Given** no session, **When** the EpiScanGIS export address is requested with the value of the
   `RegistrationToken` setting as token, **Then** a CSV file of Meningococci cases (one line per patient: patient
   number, receiving and sampling date, serogroup, PorA and FetA variants, age, and coordinates derived from the
   patient's postal code and city) is downloaded; with a missing or wrong token the answer is "not found". The
   export accepts only the plain request method and is used for an automated download (change log: "Automatischer
   EpiScanGIS Export"). [POTENTIAL-REFACTORING]
2. **Given** the same setting, **Then** one secret value both opens the registration form and the EpiScanGIS export,
   and it travels as part of the address. [SUSPECTED-DEFECT]
3. **Given** no session, **When** a file under the report templates, scripts, styles, or images is requested, **Then**
   it is delivered, because static files are not covered by the sign-in check (derived from configuration; not
   exercised; spec 010 left this open).
4. **Given** any signed-in account (including Public Health, Administrator-only, and accounts without role), **When**
   the following are requested, **Then** they are admitted: home page, "Über" and its change entries, the own password
   page, the Haemophilus and Meningococci report pages with report-status marking and the health-office lookup (specs
   008, 009, 016), all health-office pages (spec 016), the RKI match import page "RKI Matching Daten", and the country
   list helper of the postal-code lookup.
5. **Given** the RKI match import page, **Then** it is not linked from any page, offers a file field "CSV Datei" and
   the button "Upload", does not verify an anti-forgery token, and writes RKI match states into Haemophilus
   submissions; it reports "File Uploaded Successfully!!" or "File upload failed: <message>" (derived from code; not
   specified in any other spec). [SUSPECTED-DEFECT]

---

### User Story 8 - Change the own password (Priority: P3)

A signed-in user changes the own password.

**Why this priority**: It is the only self-service account function; it is not linked in the navigation.

**Independent Test**: Signed in, open the own password page by direct address, change the password, sign out, and
sign in with the new password.

**Acceptance Scenarios**:

1. **Given** any signed-in account, **When** the own password page is requested by direct address, **Then** an
   English page "Manage Account" is shown with "You're logged in as <user name>", a "Change Password Form" ("Current
   password", "New password", "Confirm new password", "Change password"), and a section "Use another service to log
   in." stating that no external authentication services are configured.
2. **Given** a correct current password and a new password of at least 6 characters with matching confirmation,
   **When** the form is submitted (with anti-forgery token), **Then** the password is changed and "Ihr Passwort wurde
   geändert." is shown; otherwise the errors are shown (English framework or form messages).
3. **Given** a password change, **Then** other existing sign-ins of the same account remain valid (derived; no
   re-validation of sign-ins is configured).
4. **Given** the external-login functions (sign in with, link, or remove an external provider), **Then** they have no
   effect, because no external provider is configured; their pages exist from the project template.

### Edge Cases

- **Refusal looks like sign-in**: When a signed-in user requests a function the role is not admitted to, the request
  is answered as "not authorized", which the sign-in handling turns into a redirect to the sign-in page with the
  requested address as return address; no "access denied" message is shown. Signing in again with the same account
  leads back to the same refusal. Background requests of list pages receive the "not authorized" status instead
  (derived from framework behavior; not exercised). Earlier specs call this outcome "refused".
  [POTENTIAL-REFACTORING]
- **Roles travel with the sign-in**: The roles of an account are read at sign-in and kept with the sign-in; a role
  changed in the database takes effect only after the next sign-in (derived from framework behavior).
- **Accounts without role**: An account can end up without role only by the failed registration of User Story 6.7 or
  by changes outside the application. It can sign in and reach everything open to any signed-in account.
- **Account holding Standard User and Administrator**: Possible only outside the application; such an account would
  be admitted to every Standard User function and to the role pages (derived).
- **Role seeding**: The application contains code that would create the three roles in an empty database, but it is
  never called; the roles must already exist (created on "Neuen Rolle anlegen" by an Administrator, which requires an
  Administrator account and thus the role, or in the database directly). [POTENTIAL-REFACTORING]
- **Registration without token setting**: If the `RegistrationToken` setting were missing, a request without token
  would match it and open the registration form, and the EpiScanGIS export would be delivered without token (derived
  from code; the setting exists in the main configuration file and its template). [SUSPECTED-DEFECT]
- **Role page save**: Saving "Neuen Rolle anlegen" does not verify the anti-forgery token the form carries and checks
  no rule; an empty or already used name ends on the generic error page (derived from the storage rules; not
  exercised). [POTENTIAL-REFACTORING]
- **Anti-forgery tokens — consolidated**: The token is verified on sign-in, registration, sign-out, password change,
  and on the create/edit/delete forms of senders, patients, sendings, isolates, breakpoints, health offices,
  counties, configuration, and PubMLST matching (specs 001, 004, 005, 016–020). It is **not** verified on:
  the combined Haemophilus and Meningococci submission create and edit (the form carries a token but the save does
  not check it; recorded in spec 002 FR-021 and spec 003 FR-023) [SUSPECTED-DEFECT]; all seven
  exports and both sender exports, which also accept any request method (specs 011, 013); both patient merges (specs
  014, 015); restoring senders and submissions (spec 020); the health-office lookup and report-status marking (specs
  008, 009, 016); the individual PubMLST lookup (spec 019) and the postal-code lookups (spec 022); the submission list
  data requests; the county file upload, which accepts any request method; the RKI match import; and the role page
  save. [POTENTIAL-REFACTORING]
- **Helper functions reachable by address**: Some internal helper functions of the controllers are reachable by direct
  address for the role of their controller: creating a patient record, creating a sending, and assigning stem and
  laboratory numbers to a sending (Standard User only), and the country list (any signed-in account). Requested this
  way they act without form, anti-forgery token, or duplicate check (derived from code; not exercised).
  [POTENTIAL-REFACTORING]
- **Generated pages**: The generated, untranslated patient and sending pages of both pathogens and the county pages
  are reachable only by direct address and admit only the Standard User role (spec 020 for the patient delete page).
- **No audit of access**: Sign-ins, failed sign-ins, sign-outs, registrations, role creations, and password changes
  are neither logged nor recorded; the change log lists "Auditierung von Zugriffen" as struck through. This is weaker
  than the Technology & Platform Constraints of the constitution ("failures affecting … authentication MUST be
  logged"). [POTENTIAL-REFACTORING]
- **Least privilege** (Constitution Principle IV): The Public Health boundary is enforced only on functions that name
  their roles; the report pages, health offices, and RKI match import name none and are therefore open to Public
  Health, contrary to "PublicHealth is limited to RKI exports". The navigation alone hides them.
- **Consistency with earlier specs**: spec 001 (User Story 2 scenario 3: restore by a standard user), specs 002
  FR-021 and 003 FR-023 (combined submission save does not verify the anti-forgery token), and specs 008 and 009
  (report pages open to any signed-in account) were aligned with this spec during review. The findings of specs 011,
  013, 014–020 on roles agree with this spec.
- **Architecture** (Constitution "Known Architectural Constraints (Legacy Debt)", Principles II and III): Access
  rules are declared per page group or per function, partly in shared base page groups used by both pathogens
  (submissions, senders, isolates, patients, sendings) and partly in the derived ones; the navigation repeats the role
  decision separately, which is how menu and access diverge for the Administrator. Automated tests cover sign-in
  success, failure message, local/remote return address, sign-out, and the EpiScanGIS token check, but not
  registration, role checks, or the navigation by role. [POTENTIAL-REFACTORING]

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Every page MUST require a signed-in account, except the sign-in page, the registration page, the
  external-login pages, and the EpiScanGIS export; unauthenticated requests MUST be sent to the sign-in page with the
  requested address as return address. Static files MUST be delivered without sign-in (current behavior).
- **FR-002**: Sign-in MUST use a user name and a password stored by the application, MUST show "Ungültiger
  Benutzername oder Passwort." for a wrong combination, MUST return only to return addresses inside the application,
  MUST offer a persistent sign-in ("Eingeloggt bleiben?"), and MUST NOT lock accounts, delay, count, or log failed
  attempts (current behavior).
- **FR-003**: Sign-out MUST be offered to every signed-in user as "Abmelden (<user name>)", MUST verify the
  anti-forgery token, and MUST end the sign-in.
- **FR-004**: The application MUST know exactly three access-relevant roles: Standard User (`Standardbenutzer`),
  Administrator (`Administrator`), and Public Health (`RKI`). Further stored roles MUST NOT grant access to anything.
- **FR-005**: The navigation and home page MUST be chosen by role as follows: Standard User or Administrator → full
  laboratory navigation and three home-page tiles; otherwise Public Health → the two RKI export entries and one
  "RKI Export" (Haemophilus) tile; otherwise → no entries. "Über" and "Abmelden" MUST be shown to every signed-in user.
- **FR-006**: Each function MUST admit exactly the roles shown in the access matrix below.
- **FR-007**: Registration MUST open its form only for a request carrying the value of the `RegistrationToken` setting
  and otherwise show the sign-in page; the submission MUST NOT re-check that token (current behavior). A successful
  registration MUST create the account, assign exactly the one chosen role, sign the browser in as the new account,
  and show the home page.
- **FR-008**: Passwords MUST be 6 to 100 characters long, with no further rule; user names MUST be unique.
- **FR-009**: The application MUST NOT offer any function to list accounts, change or remove an account's role, add a
  second role, reset a password, or disable or delete an account (current behavior).
- **FR-010**: The role pages ("Rollen", "Neuen Rolle anlegen") MUST admit only the Administrator role, MUST NOT be
  linked from the navigation, and MUST store a role under the entered name without anti-forgery check.
- **FR-011**: The own password page MUST be reachable by direct address for any signed-in account, MUST require the
  current password, and MUST verify the anti-forgery token.
- **FR-012**: The EpiScanGIS export MUST be delivered without sign-in when the token equals the value of the
  `RegistrationToken` setting and MUST answer "not found" otherwise.
- **FR-013**: A request refused for lack of role MUST lead to the sign-in page (current behavior).
- **FR-014**: This feature MUST document the current behavior only and MUST NOT introduce new roles, pages, or access
  rules.

### Access matrix

Legend: **✓** admitted · **–** refused (sent to the sign-in page) · **Menu** = offered in navigation or home page to
that role. "Signed-in" means any signed-in account, including accounts without role.

| Function (page group) | No sign-in | Standard User | Administrator only | Public Health | Menu | Anti-forgery verified | Spec |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Sign-in, registration (with token), external-login pages | ✓ | ✓ | ✓ | ✓ | "Anmelden" (anonymous only) | Yes (sign-in, registration) | this spec |
| Sign-out, own password page | – | ✓ | ✓ | ✓ | "Abmelden"; password page none | Yes | this spec |
| Home page, "Über" (change log) | – | ✓ | ✓ | ✓ | All signed-in | — | this spec |
| EpiScanGIS export (token in address) | ✓ with token | ✓ with token | ✓ with token | ✓ with token | None | No (plain download) | this spec |
| Static files (report templates, scripts, styles) | ✓ (derived) | ✓ | ✓ | ✓ | — | — | 010 |
| Role pages "Rollen", "Neuen Rolle anlegen" | – | – | ✓ | – | None | No | this spec |
| Senders incl. "Gelöschte Einsender", restore, delete | – | ✓ | – (menu shown) | – | User, Admin | Forms yes; restore no | 001, 020 |
| Sender exports (both pathogens) | – | ✓ | – (menu shown) | – | User, Admin | No; any method | 013 |
| Haemophilus/Meningococci submissions: list, create, edit, delete, "Gelöschte Einsendungen", restore | – | ✓ | – (menu shown) | – | User, Admin | Delete yes; create/edit, restore, list data no | 002, 003, 020 |
| Patient merges (both pathogens) | – | ✓ | – (menu shown) | – | User, Admin | No; any method | 014, 015 |
| Haemophilus RKI export, Meningococci RKI export | – | ✓ | – (menu shown) | ✓ | User, Admin, Public Health | No; any method | 011, 012 |
| Meningococci LGA export | – | ✓ | – (menu shown) | ✓ | User, Admin (not Public Health) | No; any method | 011 |
| Laboratory exports, Haemophilus PubMLST export, IRIS export | – | ✓ | – (menu shown) | – | User, Admin | No; any method | 011, 012 |
| Isolate entry (both pathogens) | – | ✓ | – | – | Reached from submissions | Yes | 004, 005 |
| PubMLST matching (Meningococci), individual PubMLST lookup | – | ✓ | – (menu shown) | – | User, Admin | Matching yes; lookup no | 019 |
| Postal-code lookups (sender and patient forms) | – | ✓ | – | – | Used inside forms | No | 022 |
| Configuration "Konfiguration" | – | ✓ | – (menu shown) | – | User, Admin | Yes | 018 |
| EUCAST breakpoints "Antibiotika Grenzwerte" | – | ✓ | – (menu shown) | – | User, Admin | Yes | 017 |
| Counties (generated pages, file upload) | – | ✓ | – | – | None | Forms yes; upload no, any method | this spec |
| Generated patient and sending pages, helper functions | – | ✓ | – | – | None | Forms yes; helpers no | 020, this spec |
| Health offices "Gesundheitsämter" (all pages) | – | ✓ | ✓ | ✓ | User, Admin | Forms yes | 016 |
| Report pages (both pathogens), report-status marking, health-office lookup | – | ✓ | ✓ | ✓ | Reached from submissions/isolates | No | 008, 009, 016 |
| RKI match import "RKI Matching Daten" | – | ✓ | ✓ | ✓ | None | No | this spec |
| Country list helper | – | ✓ | ✓ | ✓ | None | — | this spec |

### Roles and Boundaries

| Actor or role | Current observable involvement |
| --- | --- |
| Anonymous visitor | Can sign in, open the registration form with the token and submit a registration (also without the token, User Story 6.6), download the EpiScanGIS export with the token, and fetch static files. |
| Standard User (`Standardbenutzer`) | Admitted to every laboratory function and everything open to signed-in accounts; refused only by the role pages. |
| Administrator | Sees the full laboratory navigation, but is admitted only to the role pages and to the functions open to any signed-in account (health offices, report pages, RKI match import, own password page); refused by every other function unless the account also holds the Standard User role, which the application never assigns. |
| Public Health (`RKI`) | Navigation limited to the two RKI exports; additionally admitted to the LGA export and to everything open to any signed-in account, including report pages with patient data, health-office editing, and the RKI match import. |
| Account without role | Sees no navigation entries; admitted to everything open to any signed-in account. |
| Operator with database access | The only party able to give an account a second role, change or remove a role, or remove an account. |

### Key Entities *(include if feature involves data)*

- **Account (user)**: User name (unique), password (stored by the application in protected form), and role
  assignments. Created only through registration; carries further fields of the sign-in framework (e-mail, phone,
  lockout, two-factor) that the application never uses.
- **Role**: A name. Three names are access-relevant (`Standardbenutzer`, `Administrator`, `RKI`); others can be
  created but have no effect.
- **Role assignment**: Links an account to a role; the storage allows many per account, the application creates
  exactly one, at registration.
- **Sign-in**: Kept by the browser for the session or, with "Eingeloggt bleiben?", persistently; carries the account's
  roles as read at sign-in.
- **`RegistrationToken` setting**: A secret value (not reproduced) that opens the registration form and the
  EpiScanGIS export (spec 018).

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% of pages other than those listed in FR-001 send an unauthenticated visitor to the sign-in page in
  every test.
- **SC-002**: For each of the four account kinds (Standard User, Administrator-only, Public Health, without role),
  observed admission to every row of the access matrix matches the matrix in 100% of attempts.
- **SC-003**: For each of the four account kinds, the offered navigation entries and home-page tiles match FR-005 in
  100% of tests.
- **SC-004**: 100% of accounts created through registration hold exactly one role, the one chosen on the form.
- **SC-005**: After any number of wrong passwords, a correct password still signs the account in, in 100% of tests.
- **SC-006**: The EpiScanGIS export is delivered with the correct token and answered "not found" with any other token
  in 100% of tests.

## Assumptions

- Current application code, views, navigation, configuration files (values not read into this spec), version
  history, role constants, and the automated tests are the source of truth. No running system was exercised.
- The production database is assumed to contain the three roles, because the application does not create them and
  the navigation and access checks rely on them.
- The redirect of refused signed-in users to the sign-in page, the "not authorized" answer of background requests,
  the sign-in lifetime (14 days, renewed while in use), cookie protection, the user-name character rule, and the
  English framework messages are derived from the defaults of the sign-in framework in use, because the application
  configures only the sign-in page address; not observed in a browser.
- That static files are delivered without sign-in is derived from the absence of any access rule for them in the
  configuration; not exercised.
- The direct registration submission without token (User Story 6.6), the account left without role (6.7), the
  missing-setting cases, and the helper functions reachable by address are derived from code; not exercised.
- "Exactly one role" is established for accounts created in the application; whether accounts with several roles
  were created directly in the production database cannot be determined from code.
- Labels are quoted as shown to users; the English labels of the password and external-login pages come from the
  project template and were never translated.
