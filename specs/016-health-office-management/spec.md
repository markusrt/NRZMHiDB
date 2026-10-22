# Feature Specification: Health office management

**Feature Branch**: `speckit-init`

**Created**: 2026-10-22

**Status**: Draft (status-quo inventory)

**Input**: User description: "Feature title: \"Health office management\" (backlog ref C2, Group C — Reference data & administration). Document the current, as-implemented behavior of health office (Gesundheitsamt) management as it exists today: looking up a health office by postal code from the RKI website and storing/editing it locally, including why local edits are allowed (historical RKI data was not always current). Read HaemophilusWeb/Controllers/HealthOfficesController.cs, HaemophilusWeb/Tools/RkiTool.cs, and the ReportControllerBase QueryHealthOffice action. Capture lookup, dedupe-by-address, create/edit/delete, and how it is used in reporting. Document observable behavior only — do not propose changes."

> **Scope note**: This is an *as-is* specification (backlog ref C2). It records the current, observable behavior of
> the responsible health office (Gesundheitsamt) function: the background lookup of the responsible health office by
> the patient's postal code on the public RKI postal-code website ("RKI PLZ Tool"), the matching of the looked-up
> office to a locally stored record by its postal address, the automatic storing of offices not yet known locally,
> the local list, edit, create, details, and delete pages ("Gesundheitsämter"), and how the result is shown on the
> report pages of both pathogens. It also records why local edits exist: the RKI data on responsible health offices
> was often not current, so locally corrected telephone, fax, and email details replace the RKI values. It does
> **not** propose changes. The health-office records are one shared list for Haemophilus and Meningococci. Out of
> scope and referenced instead: report generation, report status, templates, and signers
> (`specs/008-haemophilus-reporting`, `specs/009-meningococci-reporting`, `specs/010-report-templates-docx`); the
> patient's postal code itself, which is captured at submission intake (`specs/002-patient-sending-intake`,
> `specs/003-meningo-patient-sending`); the RKI website's own content and availability, which is documented only as a
> dependency, as seen from the application.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - See the responsible health office on the report page (Priority: P1)

A Standard User opens report creation for a Haemophilus or Meningococci isolate. While the page is shown, the system
determines the health office responsible for the patient's postal code and displays it in the box "Zuständiges
Gesundheitsamt" (address, telephone, fax, email), so that the laboratory knows whom to notify.

**Why this priority**: This is the only place where health-office data is used in the laboratory workflow; every
other part of the feature exists to make this display correct.

**Independent Test**: For a patient whose postal code is known to the RKI website, open the report page and verify
that the box shows the office's address lines, telephone, fax, and email and that "Gesundheitsamt bearbeiten" becomes
active; verify that a local health-office record with that address now exists. Open the page for a second patient
with another postal code of the same office's area and verify that no second record is created.

**Acceptance Scenarios**:

1. **Given** the Haemophilus or the Meningococci report page of an isolate, **When** the page has loaded, **Then**
   the system starts, in the background, a health-office lookup with the postal code of the isolate's patient; the
   rest of the report page is usable while the lookup runs.
2. **Given** a lookup, **When** it runs, **Then** the system always queries the RKI website with the postal code
   first, regardless of whether a matching local record exists.
3. **Given** the RKI website returns one or more health offices for the postal code, **When** the result is read,
   **Then** only the first office is used, and its address (one line per address line), telephone, fax, and email
   are taken over, with repeated spaces in telephone and fax collapsed and the queried postal code recorded with it.
4. **Given** a looked-up office, **When** a stored local record has the same postal address (compared with line
   breaks ignored), **Then** the stored record is shown instead of the RKI values, so that locally corrected
   telephone, fax, and email details are displayed.
5. **Given** a looked-up office with no stored record of the same address, **When** the lookup completes, **Then**
   the RKI values are stored as a new local health-office record, without checking them against the record's
   validation rules, and are shown.
6. **Given** a health office is shown, **When** the box is displayed, **Then** the address is shown line by line,
   the telephone number is shown as a dial link (spaces and hyphens removed from the number), the fax number is
   shown as an email link to `0<fax digits>@fax` (spaces and hyphens removed, a leading `0` added), the email address
   is shown as an email link, and the button "Gesundheitsamt bearbeiten" becomes active.
7. **Given** the RKI website returns no office, cannot be reached, or returns a page that cannot be read, **When**
   the lookup completes, **Then** the box stays empty, "Gesundheitsamt bearbeiten" stays inactive, nothing is stored,
   and the failure is only written to the application log; report generation is not affected (see specs 008 and
   009).
8. **Given** a report document is generated, **When** its data is assembled, **Then** the health-office details are
   not part of the document data; they are shown on the report page only (see spec 010).

---

### User Story 2 - Correct outdated health-office details (Priority: P2)

A Standard User notices that the telephone, fax, or email of a displayed health office is outdated. From the report
page ("Gesundheitsamt bearbeiten") or from Administration → "Gesundheitsämter" the user opens the record, corrects
the details, and saves. From then on every report page whose patient's postal code is answered by the RKI website
with this office's address shows the corrected details.

**Why this priority**: The application's change history states the reason for this function: the RKI records on
responsible health offices were often not current, so an office determined by the RKI website must be correctable
afterwards. Because matching is by address, one correction applies to all postal codes in the office's area.

**Independent Test**: Look up an office via a report page, edit its telephone number, then open the report page of
a patient with a different postal code that the RKI website assigns to the same office; verify that the corrected
number is shown.

**Acceptance Scenarios**:

1. **Given** the report page with an active "Gesundheitsamt bearbeiten" button, **When** the user selects it,
   **Then** the system queries the RKI website again with the patient's postal code, finds the stored record with the
   same address, and opens the page "Gesundheitsamt bearbeiten" for that record.
2. **Given** the edit is opened from the report page but the RKI website returns no office at that moment, or no
   stored record has the returned address, **When** the page is requested, **Then** a "not found" response is shown
   instead of the edit page.
3. **Given** the Administration menu, **When** the user selects "Gesundheitsämter", **Then** the list
   "Gesundheitsämter" shows one row per stored record with the columns "Postleitzahl", "Email", "Telefon", "Fax" and a
   "Bearbeiten" button, eight rows per page, sorted by postal code ascending, with a search field; the address is not
   shown in the list, and paging, sorting, and search state are remembered in the browser.
4. **Given** the edit page, **When** it is shown, **Then** it offers "Adresse" (multi-line), "Telefon", "Fax",
   "Email", and "Postleitzahl" with example placeholders, and the buttons "Änderungen speichern" and "Zurück zur
   Liste"; it offers no delete button.
5. **Given** the user saves with a non-empty postal code and an email, telephone, and fax in valid formats, **When**
   "Änderungen speichern" is selected, **Then** all five fields of the record are overwritten and the list
   "Gesundheitsämter" is shown (the user is not returned to the report page).
6. **Given** the user saves with an empty postal code, an invalid email address, or an invalid telephone or fax
   number, **When** "Änderungen speichern" is selected, **Then** the edit page is shown again with validation
   messages and nothing is saved. This also applies to a record originally stored from the RKI website with invalid
   values: it can only be saved once those values are corrected.
7. **Given** the telephone, fax, or email of a record is corrected, **When** any later lookup returns this office's
   address, **Then** the corrected values are shown, for every postal code of the office's area.
8. **Given** the address of a record is changed, **When** a later lookup returns the RKI address, **Then** the
   changed record no longer matches; a new record is stored from the RKI values and shown, and the changed record is
   no longer used by any lookup.
9. **Given** the postal code of a record is changed, **When** later lookups run, **Then** they are unaffected,
   because the stored postal code is not used for matching.

---

### User Story 3 - Create, view, or delete health-office records directly (Priority: P3)

Pages for creating a record ("Neues Gesundheitsamt anlegen"), viewing its details, and deleting it exist, but are not
linked from any menu or list; they are reachable only by entering their address directly.

**Why this priority**: These pages are reachable but hidden; they can change the stored data, but they are not part
of the normal flow.

**Independent Test**: Open the create page directly, store a record for a postal code that the RKI website does not
know, and open a report page for a patient with that postal code; verify that the record is not shown. Delete a
record that was stored by a lookup and open a report page for one of its postal codes; verify that a fresh record is
stored from the RKI values.

**Acceptance Scenarios**:

1. **Given** the create page "Neues Gesundheitsamt anlegen" is opened directly, **When** the user enters valid
   values and selects "Neu anlegen", **Then** a new record is stored and the list "Gesundheitsämter" is shown; the
   same validation as on the edit page applies.
2. **Given** a record created on that page, **When** a report page looks up a postal code, **Then** the record is
   shown only if its address equals (ignoring line breaks) the address the RKI website returns for that postal code;
   its own postal code is never used, so a record created for a postal code unknown to the RKI website is never shown.
3. **Given** the details page of an existing record is opened directly, **When** it is shown, **Then** it lists
   address, email, telephone, fax, and postal code under the untranslated heading "HealthOffice" with the links
   "Edit" and "Back to List"; a missing record identifier gives a "bad request" response and an unknown one a "not
   found" response.
4. **Given** the delete page of an existing record is opened directly, **When** it is shown, **Then** it asks "Are you
   sure you want to delete this?" with the same details and a "Delete" button; confirming removes the record
   permanently and shows the list "Gesundheitsämter".
5. **Given** a deleted record, **When** a later lookup returns its address, **Then** a new record is stored from the
   RKI values; local corrections of the deleted record are lost.

### Edge Cases

- **Why local edits exist and how they work** (application change history, 2017-04-20 and 2017-05-31): RKI records
  were often not current, so offices determined via the RKI website can be edited afterwards, both from the report
  page and from the Administration menu; matching looked-up offices by postal address means that a corrected
  telephone, fax, or email applies automatically to all postal codes in the office's area.
- The local records do not replace the RKI website: every lookup queries the RKI website first and uses the local
  records only to override the values of the office the website returns. When the website is unreachable, has
  changed its page layout, or returns no office, no health office is shown even if a suitable local record exists,
  and "Gesundheitsamt bearbeiten" cannot be used from the report page. The website's page layout has changed in the
  past (change history 2017-05-31 and 2017-06-16), and reading it depends on the position of fields on that page.
  [POTENTIAL-REFACTORING]
- The application's change history (2017-04-20) states that own records can be created for postal codes for which
  the RKI website finds no match. Records created that way are never shown, because lookups only match by the
  address the RKI website returns; the create page is in addition not linked from the list. [SUSPECTED-DEFECT]
- Records stored from the RKI website are saved without applying the record's validation rules, because the
  website's data may be invalid (for example malformed telephone numbers, see change history 2018-06-26). Such a
  record can be displayed but cannot be saved from the edit page until its values are made valid. This is a
  deliberate exception to Constitution Principle I's rule that input is validated before it is stored.
  [POTENTIAL-REFACTORING]
- The stored postal code of an automatically created record is the postal code of the first lookup that created it;
  later lookups for other postal codes of the same office do not change it. The list "Gesundheitsämter" shows only
  postal code, email, telephone, and fax, not the address, so an office covering many postal codes is findable in the
  list only under that first postal code. [POTENTIAL-REFACTORING]
- Matching compares the RKI address and the stored address with line breaks removed; all other characters,
  including spaces between lines, must be identical. Whether letter case matters is decided by the database's
  comparison rules (see Assumptions). If the RKI website changes the wording or layout of an office's address, a new
  record is stored and earlier local corrections stop being shown.
- No rule prevents two stored records with the same address (for example two report pages looked up simultaneously,
  or a record created directly). When several records match, the system uses one of them without a defined order.
- If a locally edited record has an empty telephone number, the report page stops while displaying it: the address is
  shown, but fax and email are not, and "Gesundheitsamt bearbeiten" stays inactive. A locally cleared email is
  displayed as the text "null", and a locally cleared fax as the text "null" linked to `-@fax` (a fax missing on the
  RKI website instead appears as an empty link). Such empty fields arise because the edit page stores cleared fields
  as empty. (Derived from the report page script and standard form handling; not exercised.)
  [SUSPECTED-DEFECT]
- The report page inserts the address, telephone, fax, and email into the page as markup without escaping. These
  values come from the external RKI website or from local edits. [SUSPECTED-DEFECT]
- A patient without postal code, or with a foreign or invalid postal code, is still looked up on the RKI website with
  that value; normally no office is returned and the box stays empty.
- Telephone and fax are shortened only by collapsing repeated spaces; email is taken over as delivered. The leading
  `0` in the fax email link is added to the number as stored (for example `09191 86-3508` becomes
  `009191863508@fax`).
- The Meningococci report page uses the Haemophilus report route for the lookup; both pathogens share the same
  stored health-office records, so a correction made from one pathogen's report page applies to the other.
- The health-office pages and the lookup require only a signed-in account and apply no role check. Navigation shows
  "Gesundheitsämter" only to Standard User and Administrator accounts, but a Public Health (`RKI`) account can open
  the list, edit, create, and delete pages and run the lookup by direct address. Unlike several earlier specs (for
  example spec 014), an Administrator is not refused here. This is weaker than Constitution Principle IV's
  least-privilege rule. [SUSPECTED-DEFECT]
- The create, edit, and delete forms verify the anti-forgery token. The lookup accepts only posted requests but does
  not check an anti-forgery token, although it can store a new record (limited to values returned by the RKI
  website). [SUSPECTED-DEFECT]
- Deleting a record is permanent, is not logged, and has no undo; the confirmation for a non-existent record ends on
  the generic error page.
- Details and delete pages are untranslated standard pages (English labels "Details", "Delete", "Edit", "Back to
  List", heading "HealthOffice"), unlike the German list, create, and edit pages.
- The lookup-and-match logic exists twice, once for the report pages and once for opening the edit page from the
  report page; the RKI website client is created directly inside the controllers, so neither the matching nor the
  storing can be tested without the website, and the health-office pages display the stored entity directly. This
  reflects the constitution's "Known Architectural Constraints (Legacy Debt)" (god-controllers with weak dependency
  injection) and Principle III. Only the reading of the RKI page is covered by automated tests, using stored sample
  pages. [POTENTIAL-REFACTORING]

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The Haemophilus and the Meningococci report pages MUST, after loading, request the responsible health
  office for the postal code of the isolate's patient in the background, without blocking the rest of the page.
- **FR-002**: Every lookup MUST first query the RKI postal-code website with the given postal code; if the website
  returns no office, cannot be reached, or its answer cannot be read, the lookup MUST return no office, MUST store
  nothing, and MUST record the failure only in the application log.
- **FR-003**: When the website returns several offices, the lookup MUST use only the first one.
- **FR-004**: From the website's answer, the lookup MUST take the office's address (multi-line, surrounding spaces and
  line indentation removed, special characters decoded), telephone and fax (repeated spaces collapsed, trimmed), and
  email (unchanged), and MUST record the queried postal code with it.
- **FR-005**: The lookup MUST search the stored health-office records for one whose address equals the looked-up
  address when line breaks are ignored; if one exists, it MUST return that stored record instead of the website
  values and MUST store nothing.
- **FR-006**: If no stored record matches, the lookup MUST store the website values as a new record without applying
  the record's validation rules and MUST return them.
- **FR-007**: The report page MUST display a returned office's address line by line, the telephone as a dial link,
  the fax as an email link to `0<fax digits>@fax` (or `-` as the link target when no fax exists), and the email as an
  email link, and MUST then activate "Gesundheitsamt bearbeiten"; if no office is returned, the box MUST stay empty
  and the button inactive.
- **FR-008**: Health-office details MUST NOT be part of the generated report document's data, and a missing office
  MUST NOT block report generation.
- **FR-009**: "Gesundheitsamt bearbeiten" on the report page MUST repeat the website query for the patient's postal
  code and MUST open the edit page of the stored record matched by address; if no office or no matching record is
  found, it MUST respond with "not found".
- **FR-010**: The navigation MUST offer "Gesundheitsämter" in the Administration menu to Standard User and
  Administrator accounts and MUST NOT offer it to Public Health accounts.
- **FR-011**: The list "Gesundheitsämter" MUST show all stored records with postal code, email, telephone, fax, and an
  edit button, sorted by postal code, eight per page, searchable; it MUST NOT show the address and MUST NOT link to
  create, details, or delete.
- **FR-012**: The edit and create pages MUST offer address, telephone, fax, email, and postal code; saving MUST
  require a non-empty postal code, a valid email format, and valid telephone and fax formats; on success they MUST
  store the values and show the list.
- **FR-013**: Changing telephone, fax, or email of a record MUST take effect for every later lookup whose website
  address matches the record; changing its address MUST make it unmatchable for lookups returning the previous
  address; its postal code MUST have no effect on lookups.
- **FR-014**: The details and delete pages MUST be reachable by direct address only; confirming deletion MUST remove
  the record permanently without audit record or undo.
- **FR-015**: The health-office pages and the lookup MUST require a signed-in account and MUST apply no further role
  restriction (current behavior, see Edge Cases).
- **FR-016**: The create, edit, and delete forms MUST verify the anti-forgery token; the lookup MUST accept only
  posted requests and MUST NOT verify an anti-forgery token (current behavior, see Edge Cases).
- **FR-017**: The health-office records MUST be one list shared by Haemophilus and Meningococci.
- **FR-018**: This feature MUST document the current health-office behavior only and MUST NOT introduce new lookup,
  matching, or editing rules.

### Lookup Outcome

| Situation | Shown on report page | Stored data afterwards | "Gesundheitsamt bearbeiten" |
| --- | --- | --- | --- |
| Website returns office, stored record with same address exists | Stored (possibly corrected) values | Unchanged | Active, opens that record |
| Website returns office, no stored record with same address | Website values | New record with website values (not validated) | Active, opens the new record |
| Website returns no office | Nothing | Unchanged | Inactive |
| Website unreachable or answer unreadable | Nothing (failure logged) | Unchanged | Inactive |
| Stored record with empty telephone matched | Address only (see Edge Cases) | Unchanged | Inactive |

### Roles and Boundaries

| Actor or role | Current observable involvement |
| --- | --- |
| Standard User (`Standardbenutzer`) | Sees health-office details on report pages, the report-page edit button, and the Administration entry "Gesundheitsämter"; can edit records and, by direct address, create, view, and delete them. |
| Administrator | Sees the Administration entry "Gesundheitsämter" and, unlike other administrative functions, is not refused by the health-office pages or the lookup. |
| Public Health (`RKI`) | Is not offered the entry, but can open all health-office pages and run the lookup by direct address, because only sign-in is required. |
| RKI postal-code website (external) | Source of the responsible office for a postal code; queried on every lookup; its unavailability or layout changes leave the report box empty. |

### Key Entities *(include if feature involves data)*

- **Health office record (Gesundheitsamt)**: A locally stored office with address (multi-line), telephone, fax,
  email, and a postal code. It is created automatically by the first lookup that returns an unknown address, or
  manually; it is matched to lookups by its address only; its telephone, fax, and email override the RKI values.
  Shared by both pathogens and not linked to any patient, submission, or isolate.
- **RKI lookup result**: The first office returned by the RKI postal-code website for one postal code (address,
  telephone, fax, email). It is not stored as such; it either selects a stored record by address or becomes a new
  stored record.
- **Patient postal code**: The postal code of the isolate's patient (captured at intake); the sole input of the
  lookup.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: In 100% of tested report pages whose patient postal code is answered by the RKI website, the box
  "Zuständiges Gesundheitsamt" shows address, telephone, fax, and email, and "Gesundheitsamt bearbeiten" is active.
- **SC-002**: For two postal codes that the RKI website assigns to the same office, looking up both creates exactly
  one stored record, and a telephone correction made after the first lookup is shown for the second postal code in
  100% of tests.
- **SC-003**: In 100% of tested lookups where the RKI website returns no office or is unreachable, zero records are
  stored, the box stays empty, and report generation remains possible.
- **SC-004**: 100% of edit or create attempts with an empty postal code or an invalid email, telephone, or fax change
  zero records.
- **SC-005**: 100% of records created manually for a postal code unknown to the RKI website are not shown on report
  pages for that postal code.
- **SC-006**: Across the three roles, observed access to the health-office pages and the lookup matches the Roles and
  Boundaries table in 100% of attempts.

## Assumptions

- Current application code, views, the schema migration, the application's change history, and role constants are
  the source of truth. The RKI website was not called; its behavior is described only as the application reads it.
  Only the reading of RKI answers is covered by automated tests (stored sample pages for one result, no result,
  several results, special characters, and malformed telephone/fax); lookup, matching, storing, and the pages have no
  automated tests.
- Address matching is executed by the database; letter-case sensitivity is assumed to follow the database's default
  comparison rules (commonly case-insensitive) and was not verified.
- The report-page failure on an empty telephone number, the "null" email display, and the storing of cleared fields as
  empty are derived from the page script and standard form handling; they were not exercised in a running system.
- The leading `0` added to the fax number in the email link is assumed to be an outside-line prefix required by the
  institute's fax-to-email gateway; the code gives no reason.
- When the lookup returns no office, the report page is assumed to show nothing and to raise no visible error; the
  exact browser behavior for the empty answer was not observed.
- Accounts are assumed to hold exactly one role, as in specs 011 and 014; since the health-office functions check no
  role, this assumption does not change the outcome here.
- The patient's postal code is assumed to be the value stored on the patient at intake (specs 002 and 003); no other
  source is used.
