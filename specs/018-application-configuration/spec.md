# Feature Specification: Application configuration

**Feature Branch**: `speckit-init`

**Created**: 2026-10-22

**Status**: Draft (status-quo inventory)

**Input**: User description: "Feature title: \"Application configuration\" (backlog ref C4, Group C — Reference data & administration). Document the current, as-implemented behavior of the application Configuration screen (Konfiguration) as it exists today. Read HaemophilusWeb/Controllers/ConfigurationController.cs, HaemophilusWeb/Services/ConfigurationService.cs, and related Views. Capture what settings are editable (e.g. report signers), who may edit them, and where they are used. Document observable behavior only — do not propose changes."

> **Scope note**: This is an *as-is* specification (backlog ref C4). It records the current, observable behavior of
> the configuration page ("Konfiguration" in the Administration menu): the five settings it edits (report signers,
> the report announcement with its validity period, the overall laboratory director, the medical director, and the
> contact blocks), the validation applied when saving, who may open and save the page, where the settings are kept,
> and every place in the application where they are used. It also lists, by name only, the settings that are **not**
> editable in the application and exist only in the deployed application settings file, together with the area that
> uses each of them. It does **not** propose changes and does not reproduce any setting values or credentials. Out of
> scope and referenced instead: the report pages and report generation (`specs/008-haemophilus-reporting`,
> `specs/009-meningococci-reporting`), the report templates, their placeholders, and the preliminary-template marker
> (`specs/010-report-templates-docx`), and the health-office data shown on the report pages
> (`specs/016-health-office-management`).

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Maintain the list of report signers (Priority: P1)

A Standard User opens Administration → "Konfiguration" and maintains the names that are offered as "Unterzeichner"
(signer) when a Haemophilus or Meningococci report is created, for example when a staff member joins or leaves.

**Why this priority**: A report cannot be generated without choosing a signer, and the signer choice on both report
pages comes exclusively from this list. An empty or outdated list blocks or falsifies reporting.

**Independent Test**: With a Standard User account, enter three names (one per line, with an empty line and extra
spaces in between), save, reopen the page, and verify the names; open the report page of a Haemophilus and of a
Meningococci isolate and verify that exactly these names are offered as "Unterzeichner", in the entered order.

**Acceptance Scenarios**:

1. **Given** a Standard User or an Administrator account, **When** the Administration menu is shown, **Then** it
   offers the entry "Konfiguration" as its first item; a Public Health (`RKI`) account is not offered the entry.
2. **Given** a Standard User selects the entry, **When** the page "Konfiguration" is shown, **Then** it shows a
   multi-line field "Unterzeichner" with the hint "Ein Unterzeichner pro Zeile." containing the stored signer names,
   one per line, followed by the sections "Mitteilung im Befund" and "Leitung und Kontakte", and the buttons
   "Speichern" and "Abbrechen".
3. **Given** the user enters names one per line, **When** "Speichern" is selected, **Then** each line is stored as
   one signer with surrounding spaces removed, empty and blank lines are dropped, the entered order is kept, the page
   is shown again with the stored values, and the message "Konfiguration gespeichert." appears and fades out after a
   few seconds.
4. **Given** stored signers, **When** the report page of any Haemophilus or Meningococci isolate is opened afterwards,
   **Then** the choice "Unterzeichner" offers "Auswählen..." followed by exactly the stored names in stored order,
   and the chosen name is placed into the generated document (see specs 008, 009, and 010).
5. **Given** the field "Unterzeichner" is emptied, **When** the user saves, **Then** no message is shown and an empty
   list is stored; afterwards the report pages offer only "Auswählen...", so every report attempt ends with the
   message asking for a template and a signer (see specs 008 and 009).
6. **Given** the same name is entered twice, **When** the user saves, **Then** both lines are stored and the report
   pages offer the name twice.
7. **Given** a signer name is removed or changed, **When** reports are created afterwards, **Then** only the new list
   is offered; documents generated earlier are not affected, because the application does not keep generated
   documents or the chosen signer.

---

### User Story 2 - Publish a time-limited announcement on reports (Priority: P2)

A Standard User enters an announcement text (for example a notice about changed opening hours or a new address) and a
validity period. Every Haemophilus and Meningococci report created within that period carries the text.

**Why this priority**: The announcement reaches all senders through every report without editing any template, but it
is optional and time-limited.

**Independent Test**: Enter a text with a period from yesterday to tomorrow, save, open a report page, generate a
document, and verify the text appears; change the period to end yesterday and verify the next generated document does
not contain it.

**Acceptance Scenarios**:

1. **Given** the section "Mitteilung im Befund", **When** it is shown, **Then** it offers a multi-line field
   "Mitteilung" with the hint "Wird im Befund nur innerhalb des Gültigkeitszeitraums angezeigt." and the date fields
   "Gültig von" and "Gültig bis" (date picker, format day.month.year).
2. **Given** a text but no "Gültig von" or no "Gültig bis", **When** the user saves, **Then** the page is shown again
   with "Bitte ein Startdatum für die Mitteilung angeben." and/or "Bitte ein Enddatum für die Mitteilung angeben.",
   and none of the five settings is stored.
3. **Given** "Gültig bis" lies before "Gültig von", **When** the user tries to save, **Then** the browser shows "Das
   Enddatum darf nicht vor dem Startdatum liegen." under "Gültig bis" and does not submit the form; if the form reaches
   the server anyway, the same message is shown and nothing is stored.
4. **Given** a text with a valid period, **When** the user saves, **Then** the text is stored with surrounding
   whitespace removed and line breaks unified, together with both dates.
5. **Given** a stored text and period, **When** a report page is opened on a day from "Gültig von" through "Gültig bis"
   (both days inclusive, by the server's calendar date), **Then** the report data carries the text and the documents
   show it in their comment/announcement area; on any other day the report data carries no announcement and that
   area shows only the comment, if any (see spec 010).
6. **Given** the text is emptied (dates may stay), **When** the user saves, **Then** no date is required, the empty
   text is stored, and no report carries an announcement regardless of the dates.

---

### User Story 3 - Maintain laboratory leadership and contact details (Priority: P2)

A Standard User maintains the names of the overall laboratory director ("Gesamtleitung") and the medical director
("Ärztliche Leitung") and the contact blocks that are printed in the header of every report document.

**Why this priority**: These values appear on every generated report of both pathogens; when leadership or contact
details change, this page is the only place to update them without changing the templates.

**Independent Test**: Change both director names and enter two contact blocks separated by an empty line, each with
several lines; save, generate one Haemophilus and one Meningococci report, and verify the header shows the new names
and both blocks with their internal line breaks.

**Acceptance Scenarios**:

1. **Given** the section "Leitung und Kontakte", **When** it is shown, **Then** it offers the single-line fields
   "Gesamtleitung" and "Ärztliche Leitung" and a multi-line field "Kontaktdaten" with the hint "Mehrere Kontaktblöcke
   durch eine Leerzeile trennen. Zeilenumbrüche innerhalb eines Blocks bleiben erhalten."
2. **Given** names in "Gesamtleitung" and "Ärztliche Leitung", **When** the user saves, **Then** each is stored with
   surrounding spaces removed; an empty field is stored as empty without any message.
3. **Given** text in "Kontaktdaten", **When** the user saves, **Then** the text is split into blocks at every empty
   (or blank) line, each block is stored with surrounding whitespace removed and its inner line breaks kept, empty
   blocks are dropped, and the order is kept; on reopening, the blocks are shown separated by one empty line.
4. **Given** stored values, **When** a report page of any Haemophilus or Meningococci isolate is opened, **Then** the
   report data carries both director names and all contact blocks joined by one empty line, and the report templates
   print them in their page headers, with line breaks preserved (see spec 010).
5. **Given** a value is changed, **When** a report is generated afterwards, **Then** the new value is used;
   previously generated documents are not affected.

---

### User Story 4 - Leave the page or be refused (Priority: P3)

A user opens the configuration page and leaves it without saving, or a user without the required role tries to open
it.

**Why this priority**: Defines the boundaries of the page; it is rarely exercised but determines who can change values
that appear on every report.

**Independent Test**: Change a field and select "Abbrechen"; verify the home page is shown without a warning and that
the stored values are unchanged. Request the page with an Administrator-only and with a Public Health account and
verify that it is refused.

**Acceptance Scenarios**:

1. **Given** unsaved changes, **When** the user selects "Abbrechen" or navigates away, **Then** the home page (or the
   chosen page) is shown without a confirmation prompt and nothing is stored.
2. **Given** an account holding only the Administrator role, or a Public Health account, **When** the configuration
   page is requested or submitted, **Then** the request is refused, because the page admits only the Standard User
   role (`Standardbenutzer`).
3. **Given** a request to the page without a signed-in account, **When** it is handled, **Then** the user is sent to
   sign in.
4. **Given** a save submitted without the page's anti-forgery token, **When** it is handled, **Then** it is rejected
   and nothing is stored.
5. **Given** the address of the configuration area without a page name, **When** it is requested, **Then** the
   configuration page is shown.

### Edge Cases

- **One page, one save for all settings**: All five settings are shown on a single page and saved together with one
  "Speichern"; there is no per-setting save and no way to add further settings from the page. If validation fails,
  nothing is stored and the entered values are shown again.
- **Settings are stored one by one**: On a successful save the five settings are written one after the other, each
  as its own database write. A failure between two writes would leave some settings updated and others not, without
  a combined undo. [POTENTIAL-REFACTORING]
- **No history and no audit**: Saving overwrites the previous values; neither the previous values nor the account or
  time of the change are kept. Two users saving at the same time overwrite each other's changes without notice (last
  save wins). Because the values are printed on every report, a change cannot be traced afterwards (cf. Constitution
  Principle IV). [POTENTIAL-REFACTORING]
- **No validation of names or contents**: Signer names, director names, and contact blocks have no length limit, no
  format check, and no duplicate check; only the announcement dates are validated.
- **Empty signer list blocks reporting**: An empty "Unterzeichner" field is accepted without warning, although no
  report can then be generated (User Story 1, scenario 5).
- **Signers are free text**: Signer names are not linked to user accounts, roles, or any electronic signature; the
  choice only puts the name into the document (see specs 008 and 009, which state that no signing event is stored).
- **Announcement timing**: Whether the announcement is active is decided when the report page is opened, from the
  server's current calendar date, not from the report date, sampling date, or receipt date of the isolate. A report
  page left open across the end (or start) of the period keeps the state from when it was opened (derived).
- **Announcement period is always closed in the page**: When a text is entered, both dates are required, so an
  open-ended announcement cannot be entered through the page, although stored announcements without dates would be
  treated as always active (derived from the settings logic; not reachable through the page).
- **Dates without text**: Dates can be stored without a text; they have no effect.
- **Fresh database defaults**: When the application starts against a database that lacks any of the five settings,
  it creates the missing ones with placeholder values: two example signer names, placeholder names for both
  directors, three example contact blocks with placeholder phone numbers and e-mail addresses, and an empty
  announcement. Existing settings are never overwritten by this. Until a user edits them, the placeholders are offered
  on report pages and printed on reports.
- **Unreadable stored value**: If a stored setting value cannot be read, the application silently treats the setting
  as empty (no signers, empty names, no contacts, no announcement) and shows no error; the page then shows empty
  fields and the next save overwrites the stored value.
- **Where the settings live**: The five settings are kept in the application database, one named entry per setting,
  not in the deployed application settings file. They therefore apply to both pathogens and to every user at once,
  take effect for the next report page opened without a restart, and are carried along with database backups.
- **Shared by both pathogens**: There is one set of settings for Haemophilus and Meningococci; they cannot differ
  between the two report families.
- **Also read on the isolate form**: Because the report data and the isolate form are prepared from the same isolate
  data, the announcement, director names, and contact blocks are also read whenever an isolate of either pathogen is
  opened for editing; they are not displayed there (derived).
- **No unsaved-changes warning**: Unlike the isolate, patient, sending, and breakpoint forms, the configuration page
  does not ask for confirmation when the user leaves it with unsaved changes.
- **Authorization**: The page admits only the Standard User role. Navigation shows "Konfiguration" to Standard User
  and Administrator accounts, so an Administrator-only account sees the entry but is refused, as in specs 011, 014,
  and 017. [SUSPECTED-DEFECT] Public Health accounts are neither offered the entry nor admitted. The save verifies the
  anti-forgery token.
- **Architecture** (Constitution "Known Architectural Constraints (Legacy Debt)", Principle III): Reading and writing
  the settings is located in a dedicated service rather than in the page controller, and that service and the save
  validation have automated tests; the configuration page itself has none. The page and both report pages create the
  service directly instead of receiving it, and the report data preparation obtains its own database access through a
  static test hook, reflecting the "weak dependency injection" constraint. [POTENTIAL-REFACTORING]

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The navigation MUST offer "Konfiguration" as the first entry of the Administration menu to Standard User
  and Administrator accounts and MUST NOT offer it to Public Health accounts.
- **FR-002**: The configuration page MUST admit only accounts holding the Standard User role and MUST refuse
  Administrator-only and Public Health accounts (current behavior, see Edge Cases); unauthenticated requests MUST be
  sent to sign in.
- **FR-003**: The page "Konfiguration" MUST show, filled with the stored values, the fields "Unterzeichner",
  "Mitteilung", "Gültig von", "Gültig bis", "Gesamtleitung", "Ärztliche Leitung", and "Kontaktdaten", and the buttons
  "Speichern" and "Abbrechen"; "Abbrechen" MUST lead to the home page without storing anything.
- **FR-004**: Saving MUST store the signers as one entry per non-blank line, trimmed, in entered order, including
  duplicates, and MUST accept an empty list.
- **FR-005**: Saving MUST store the contact data as blocks separated by empty or blank lines, each trimmed at its ends
  with inner line breaks kept, dropping empty blocks, in entered order.
- **FR-006**: Saving MUST store both director names trimmed, accepting empty values.
- **FR-007**: Saving MUST store the announcement text trimmed, with unified line breaks, together with both dates.
- **FR-008**: When the announcement text is not blank, saving MUST require both "Gültig von" and "Gültig bis"; in all
  cases saving MUST reject a "Gültig bis" before "Gültig von". On any validation failure, the page MUST be shown again
  with the messages listed in User Story 2, and none of the five settings MUST be stored. The browser MUST already
  prevent submitting a "Gültig bis" before "Gültig von".
- **FR-009**: After a successful save, the page MUST be shown again with the stored values and the message
  "Konfiguration gespeichert.".
- **FR-010**: The save MUST verify the anti-forgery token.
- **FR-011**: The five settings MUST be kept in the application database as one shared set for both pathogens and all
  users, and changes MUST take effect for report pages opened afterwards without a restart.
- **FR-012**: The report pages of both pathogens MUST offer as "Unterzeichner" exactly the stored signers in stored
  order, after the empty choice "Auswählen..." (see specs 008 and 009).
- **FR-013**: The report data of both pathogens MUST carry the announcement text only when the server's current date
  lies within the stored period (both days inclusive) and the text is not blank; otherwise it MUST carry no
  announcement.
- **FR-014**: The report data of both pathogens MUST carry both director names and all contact blocks joined by one
  empty line, for use by the report templates (see spec 010).
- **FR-015**: When the application starts and any of the five settings is missing, it MUST create that setting with
  its placeholder default and MUST NOT change settings that already exist.
- **FR-016**: Saving MUST overwrite the previous values without keeping a history, an audit record, or a check for
  concurrent changes (current behavior).
- **FR-017**: All other application settings (see "Settings not editable in the application") MUST NOT be shown or
  editable in the application; they are read only from the deployed application settings file.
- **FR-018**: This feature MUST document the current configuration behavior only and MUST NOT introduce new settings,
  validation, or permissions.

### Settings editable on the configuration page

| Setting (label) | Input | Stored as | Validation | Used by |
| --- | --- | --- | --- | --- |
| "Unterzeichner" | Multi-line, one name per line | Ordered list of names | None (empty list allowed) | Signer choice on Haemophilus and Meningococci report pages; chosen name printed in the document |
| "Mitteilung" | Multi-line text | Text | None on the text | Comment/announcement area of report documents, only while active |
| "Gültig von" / "Gültig bis" | Dates (day.month.year) | Dates of the announcement | Both required when a text is entered; end not before start | Decide whether the announcement is active on the day the report page is opened |
| "Gesamtleitung" | Single line | Name | None | Header of report documents |
| "Ärztliche Leitung" | Single line | Name | None | Header of report documents |
| "Kontaktdaten" | Multi-line, blocks separated by empty lines | Ordered list of blocks | None | Contact header of report documents |

### Settings not editable in the application

These settings exist only in the deployed application settings file. They are changed by editing that file on the
server; the application offers no page for them. Values are deliberately not reproduced here.

| Setting name | Used by (observable effect) |
| --- | --- |
| `PreliminaryReportMarker` | Report pages of both pathogens: a template whose name contains this text is treated as a preliminary report (see spec 010). |
| `PrimaryAntibiotics_Haemophilus`, `PrimaryAntibiotics_Meningococci` | Isolate forms and report data: the standard antibiotics whose E-Test rows are shown by default (see specs 004, 005, and 017). |
| `AntibioticsOrder` | Isolate forms and report data: order in which antibiotics are listed. |
| `GeonamesAccount` | Account name used for the external postal-code and place-name lookup service (used, among others, by sender management, see spec 001, and by the Episcan GIS export). |
| `RegistrationToken` | Token required to register a new account and to access the Episcan GIS export. |
| `IrisAccessToken`, `IrisAccessTokenSecret`, `IrisConsumerKey`, `IrisConsumerSecret` | Credentials for the external IRIS PubMLST service used by the PubMLST lookup and matching. |
| `Beta` | When set to true, every page shows a warning banner that the installation is a demonstration version whose changes are not stored centrally. |
| Framework settings (`webpages:Version`, `webpages:Enabled`, `ClientValidationEnabled`, `UnobtrusiveJavaScriptEnabled`) | Technical behavior of the web framework; no domain meaning. |

### Roles and Boundaries

| Actor or role | Current observable involvement |
| --- | --- |
| Standard User (`Standardbenutzer`) | Sees the Administration entry "Konfiguration"; views and saves all five settings; uses the signers and sees the other settings on report pages and documents. |
| Administrator | Sees the entry "Konfiguration" but is refused by the page unless the account also holds the Standard User role. |
| Public Health (`RKI`) | Is neither offered the entry nor admitted to the page. |
| Operator with server access | Changes the settings that exist only in the deployed application settings file; outside the application. |

### Key Entities *(include if feature involves data)*

- **Configuration setting**: A named, application-wide value kept in the application database. Exactly five names are
  used: report signers, announcement, laboratory director, medical director, and contacts. Each name exists at most
  once. Settings are not tied to a pathogen, user, sender, or isolate.
- **Report signer**: A free-text name in an ordered list; offered on the report pages and printed in the document;
  not linked to any user account.
- **Announcement**: A text with a start and an end date; active on every calendar day from start through end when the
  text is not blank.
- **Contact block**: A multi-line text describing one contact person; blocks are kept in an ordered list.
- **Application settings file entry**: A named setting in the deployed application settings file, read by the
  application but not editable in it (see table above).

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: After saving N non-blank signer lines, the report pages of both pathogens offer exactly those N names in
  entered order in 100% of tests.
- **SC-002**: In 100% of tested saves with an announcement text but a missing date, or with an end date before the
  start date, none of the five settings changes.
- **SC-003**: For an announcement period from day A to day B, documents generated from report pages opened on day A,
  on day B, and on any day in between contain the text, and documents from pages opened before A or after B do not, in
  100% of tests.
- **SC-004**: After changing a director name or contact block, the next generated report of each pathogen shows the
  new value in its header in 100% of tests, without restarting the application.
- **SC-005**: Across the three roles, observed access to the configuration page matches the Roles and Boundaries
  table in 100% of attempts.
- **SC-006**: On a database without configuration settings, all five settings exist with their placeholder defaults
  after one application start, and existing settings keep their values in 100% of tests.

## Assumptions

- Current application code, views, schema migrations, startup data seeding, the report templates, the application
  settings file, and role constants are the source of truth. No running system was exercised. Automated tests exist
  for the settings service and the save validation, but not for the configuration page.
- Accounts are assumed to hold exactly one role, as in specs 011, 014, and 017, so an Administrator account is
  refused by the page.
- The refusal of a save without anti-forgery token, the redirect of unauthenticated users to sign in, and the display
  of the date fields are derived from the standard behavior of the framework features used; not observed in a browser.
- Placeholder settings are assumed to be created at the first application start after deployment, because the
  database is brought to the latest schema version on startup and the defaults are added in that step; not exercised.
- The statement that templates print the announcement, director names, and contacts is based on the placeholders
  found in the current Haemophilus and Meningococci templates and their header parts; the exact rendering is
  documented in spec 010.
- Changes to the application settings file are assumed to require editing the file on the server; whether and when
  the running application picks them up (typically by restarting the application) is standard hosting behavior and
  was not verified.
- The consumers of the settings that exist only in the application settings file (postal-code lookup, account
  registration, Episcan GIS export, IRIS PubMLST services, demonstration banner) are named only to identify where each
  setting is used; their behavior is not specified here.
