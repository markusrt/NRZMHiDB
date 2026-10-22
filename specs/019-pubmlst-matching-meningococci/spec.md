# Feature Specification: PubMLST matching (Meningococci)

**Feature Branch**: `speckit-init`

**Created**: 2026-10-22

**Status**: Draft (status-quo inventory)

**Input**: User description: "Feature title: \"PubMLST matching (Meningococci)\" (backlog ref D1, Group D — Integrations). Document the current, as-implemented behavior of PubMLST matching for Meningococci isolates as it exists today, both as a batch over a date range and for individual entries, where remote PubMLST records are fetched once and copied into the local database. Read HaemophilusWeb/Services/PubMlstMatcher.cs, the MeningoIsolate PubMlstMatch action, and PubMlstService. Capture how isolates are matched by reference/stem number, what fields are copied, create-or-update behavior, and that data is only fetched from remote once. Document observable behavior only — do not propose changes."

> **Scope note**: This is an *as-is* specification (backlog ref D1). It records the current, observable behavior of
> matching Meningococci isolates with the external PubMLST databases: the batch matching over a date range
> (PubMLST menu → "Matching (Meningokokken)") with its result page, the remote lookup used by both the batch and the
> individual lookup on the isolate form, how an isolate is found remotely by its prefixed stem number, which typing
> values are copied, how the local copy of a PubMLST record is created or updated, who may run the matching, and how
> often remote data is actually fetched. It does **not** propose changes and does not reproduce any credential
> values. Out of scope and referenced instead: the PubMLST tab of the isolate form, its read-only typing fields and
> how an individual lookup result is saved with the isolate (`specs/005-meningococci-isolate-entry`), the result
> interpretation, which does not use PubMLST data (`specs/007-meningococci-result-interpretation`), Meningococci
> reports (`specs/009-meningococci-reporting`), the exports that print the stored PubMLST typing values
> (`specs/011-data-exports-generic`), and the IRIS credential settings (`specs/018-application-configuration`). The
> Haemophilus "PubMLST Export" is a file export, not a matching, and is covered by spec 011. Uploading sequence data
> to PubMLST happens outside the application.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Match all Meningococci isolates of a period with PubMLST (Priority: P1)

After laboratory staff have uploaded sequence data to PubMLST (outside the application) and PubMLST has published
it, a Standard User opens PubMLST → "Matching (Meningokokken)", chooses a period, and starts the matching. The
application looks up every Meningococci isolate of that period in PubMLST by its prefixed stem number, copies the
typing values of every isolate found into the local database, links them to the isolate, and lists the outcome per
isolate.

**Why this priority**: This is the only way to bring PubMLST typing data into many isolates at once; the stored
values are then printed by the Meningococci Laboratory and RKI exports (spec 011) and shown on the isolate form
(spec 005).

**Independent Test**: With a Standard User account, choose a period containing isolates with stem numbers that
exist in PubMLST, one isolate without a stem number, and one with a stem number unknown to PubMLST; start the
matching and verify the counts, the per-isolate rows, and that the found isolates show the PubMLST values when
their isolate form is opened.

**Acceptance Scenarios**:

1. **Given** a Standard User or an Administrator account, **When** the navigation is shown, **Then** it offers the
   menu "PubMLST" with the single entry "Matching (Meningokokken)"; a Public Health (`RKI`) account is not offered
   the menu.
2. **Given** a Standard User selects the entry, **When** the page "PubMLST Matching" is shown, **Then** it shows the
   heading "Entnahmedatum", the date fields "Start-Datum" and "Ende-Datum" prefilled with 1 January and 31 December
   of the **current** calendar year, the hint "Hinweis: Die Abfrage nimmt einige Zeit in Anspruch. Die Seite lädt
   automatisch neu sobald alle Daten von PubMLST geladen sind.", and the button "Abfrage der PubMLST Daten".
3. **Given** a period, **When** the user starts the matching, **Then** the button changes to "Abfrage läuft ..." and
   the page waits until every isolate of the period has been processed; there is no progress indication and no way
   to cancel.
4. **Given** the matching runs, **Then** it selects every Meningococci isolate whose sending has a sampling date
   within the period, or — when the sending has no sampling date — whose receiving date lies within the period
   (both bounds inclusive), the same date rule as the Meningococci exports (spec 011).
5. **Given** a selected isolate with a stem number, **When** it is processed, **Then** its prefixed stem number
   (for example `DE1234`) is searched first in the public PubMLST Neisseria isolate database and, only if no
   record is found there, in the access-protected IRIS PubMLST database; the first database that returns exactly
   one record wins.
6. **Given** a record is found, **Then** its typing values are copied into a local PubMLST record (created when no
   local record with the same PubMLST id exists, otherwise updated, see User Story 3), the isolate is linked to
   that local record, and the isolate is saved immediately, before the next isolate is processed.
7. **Given** a selected isolate without a stem number, **When** it is processed, **Then** no remote request is made
   and the isolate is listed as not found with the stem number "DE -".
8. **Given** no record or more than one record is found in both databases, **Then** the isolate is listed as not
   found and its existing PubMLST link and data, if any, remain unchanged.
9. **Given** all isolates are processed, **When** the page "PubMLST Matching abgeschlossen" is shown, **Then** it
   offers "Zurück zur Übersicht" (Meningococci sending overview) and "Abfrage wiederholen" (the matching form), the
   summary "Die Abfrage hat *N* Datensätze mit PubMLST abggeglichen. Davon wurden *M* anhand der Stammnummer
   gefunden." (wording as shown, *N* = selected isolates, *M* = isolates found), and a table with the columns
   "PubMLST ID", "Datenbank", "Stammnummer", and "Labornummer".
10. **Given** a found isolate in the result table, **Then** the row shows a green mark and the PubMLST id as a link to
    the record's public PubMLST information page (in the database that returned it), the database name
    (`pubmlst_neisseria_isolates` or `pubmlst_neisseria_iris`), the prefixed stem number, and the laboratory number
    as a button that opens the isolate form; a not-found isolate shows a red mark and "kein Treffer" instead of the
    id and an empty database.

---

### User Story 2 - Look up a single isolate in PubMLST (Priority: P2)

On the PubMLST tab of a Meningococci isolate form, a Standard User selects "Abfragen" to fetch the PubMLST typing
values for this one isolate; the values become part of the isolate when it is saved (spec 005).

**Why this priority**: It covers single isolates outside a batch run (for example when data was published after the
last batch), but the persistence itself belongs to isolate entry and is specified in spec 005.

**Independent Test**: Open the isolate form of an isolate whose stem number exists only in the IRIS database, select
"Abfragen", and verify that the IRIS record is shown and that nothing is stored until the isolate is saved.

**Acceptance Scenarios**:

1. **Given** the PubMLST tab, **When** "Abfragen" is selected, **Then** the stem number with prefix as it was stored
   when the form was opened is sent to the lookup, which searches first the IRIS PubMLST database and then the
   public PubMLST Neisseria isolate database (the opposite order of the batch matching) and returns the first record
   found.
2. **Given** a record is found, **Then** the form shows "Isolat wurde geladen", fills the read-only typing fields, the
   PubMLST id, and the database, and enables the link to the PubMLST entry (spec 005); nothing is written to the
   database by the lookup itself.
3. **Given** no record is found or the lookup fails, **Then** the form shows "Isolat wurde nicht gefunden", empties
   the typing fields, and resets the PubMLST id to 0.
4. **Given** a looked-up record, **When** the isolate is saved validly, **Then** the values shown in the form are
   stored with the create-or-update rule described in spec 005 (FR-021); the save does not contact PubMLST again.

---

### User Story 3 - Keep one local copy per PubMLST record (Priority: P2)

Each PubMLST record that was ever matched is kept as one local record identified by its PubMLST id. Reports,
exports, and the isolate form read only this local copy; PubMLST is contacted only when a matching or an individual
lookup is run.

**Why this priority**: Defines what "fetched once" means today and which values the rest of the application uses.

**Independent Test**: Match an isolate in a batch run, change nothing remotely, run the batch again, and verify that
no second local record is created; verify that opening the isolate form or running an export does not contact
PubMLST.

**Acceptance Scenarios**:

1. **Given** a found remote record whose PubMLST id is not yet stored locally, **When** a batch match processes it,
   **Then** a new local record is created with the PubMLST id, the database name, and all copied values (see
   "Copied PubMLST values").
2. **Given** a found remote record whose PubMLST id is already stored locally, **When** a batch match processes it,
   **Then** the existing local record is reused and its seventeen typing values are overwritten with the remote
   values, including values that are now empty; the stored database name and PubMLST id are not changed.
3. **Given** several isolates are linked to the same local record, **When** that record is updated, **Then** all of
   them show the updated values.
4. **Given** a batch run, **Then** each selected isolate with a stem number is searched once per database at most,
   and the second database is not asked once the first has returned a record; the IRIS session is established once
   per run and reused for all isolates of that run.
5. **Given** an isolate that is already linked to a local record, **When** it falls into the period of a later batch
   run, **Then** it is searched remotely again and its local record is refreshed; there is no "already matched"
   skip, and remote data is therefore fetched again on every run, not only once.
6. **Given** remote data changes after a match, **When** no new matching or lookup is run, **Then** the application
   keeps showing and exporting the previously copied values.

---

### User Story 4 - Be refused (Priority: P3)

A user without the required role tries to run the matching or the individual lookup.

**Why this priority**: Defines the boundary of a feature that writes typing data into many isolates at once and
contacts an external service with laboratory credentials.

**Independent Test**: Request the matching page and the lookup with an Administrator-only and with a Public Health
account and verify both are refused.

**Acceptance Scenarios**:

1. **Given** an account holding only the Administrator role, or a Public Health account, **When** the matching page
   is requested or submitted, or the individual lookup is called, **Then** the request is refused, because all three
   admit only the Standard User role (`Standardbenutzer`).
2. **Given** a request without a signed-in account, **When** it is handled, **Then** the user is sent to sign in.
3. **Given** a matching submission without the page's anti-forgery token, **When** it is handled, **Then** it is
   rejected and nothing is matched.
4. **Given** an individual lookup request without an anti-forgery token, **When** it is handled, **Then** it is
   accepted, because the lookup does not check the token; it does not write to the database.

### Edge Cases

- **Exact reference search**: The remote search asks for isolates whose isolate name equals the prefixed stem
  number (derived). Exactly one result counts as a match; zero results and more than one result both count as "not
  found" without telling the user which of the two applies; both cases are only written to the application log.
- **What is sent to PubMLST**: Only the prefixed stem number (and, for the IRIS database, the laboratory's
  credentials) is sent; no patient data leaves the application through this feature (cf. Constitution Principle IV).
- **Values copied, not linked**: The application keeps its own copy of the typing values; PubMLST is not contacted
  when the isolate form, reports, or exports are opened. The copy is refreshed only by a later batch run that
  covers the isolate or by an individual lookup followed by a save.
- **Periods and bounds**: The period is compared with the stored dates including the time of day, with the end date
  taken as the very start of that day; since sending dates are entered without time, the end day is effectively
  included (derived). No check prevents a start date after the end date; such a period selects nothing and shows
  "0" in the summary. An empty or unreadable date is not reported back to the user; the run proceeds with the
  earliest possible date for that bound (derived from the unchecked form input).
- **Default period is the current year**: Unlike the exports, which default to the previous year (spec 011), the
  matching form defaults to the current calendar year, although the internal name of the default suggests "last
  year". [POTENTIAL-REFACTORING]
- **Heading versus selection**: The form is headed "Entnahmedatum" (sampling date), but sendings without a sampling
  date are selected by their receiving date.
- **Deleted sendings are included**: The batch selects isolates regardless of whether their sending is marked as
  deleted, unlike the Meningococci exports, which exclude deleted sendings (spec 011). [SUSPECTED-DEFECT]
- **No ordering of the result**: The result table lists the isolates in the order the database returns them; no
  sorting by stem or laboratory number is applied.
- **Previous matches are never removed**: When an already linked isolate is not found in a later run (for example
  because its stem number changed or the remote record was removed), the result shows "kein Treffer", but the
  isolate keeps its previous link and values.
- **Different database order for batch and individual lookup**: The batch asks the public database first and IRIS
  second; the individual lookup asks IRIS first and the public database second. An isolate present in both databases
  can therefore be linked to different records depending on which way it was matched. [SUSPECTED-DEFECT]
- **Local record identified by PubMLST id only**: An existing local record is found by its numeric PubMLST id alone,
  without its database. A record from the IRIS database whose id equals that of an already stored record from the
  public database (or vice versa) would overwrite that record's typing values while keeping the other database name,
  and both isolates would then share it. The individual save path (spec 005) updates the database name in this case;
  the batch path does not. [SUSPECTED-DEFECT]
- **Fixed field set on update**: The batch update overwrites exactly the seventeen typing values listed under "Copied
  PubMLST values"; the same field list is maintained a second time for the individual save path (spec 005), with the
  database name as an additional field there. The source itself marks this duplication for clean-up.
  [POTENTIAL-REFACTORING]
- **Partial results on failure**: Each found isolate is saved as soon as it is processed. If the run stops
  part-way (for example because a remote request fails in a way that is not handled, see next item, or the request
  takes longer than the platform allows), the isolates processed until then keep their new PubMLST data, the user
  sees an error page instead of the result page, and no list of processed isolates is shown.
- **Unhandled remote failures abort the run**: A "not found" status while fetching the details of a found record is
  treated as "not found" for that database, and the next database is asked. Any other failure while fetching the
  details, and any failure during the search itself (for example a "not found" status from the search, a connection
  error, or an IRIS response without a result count after rejected credentials), are not caught and end the whole
  batch run, or make an individual lookup show "Isolat wurde nicht gefunden" (derived from code,
  not exercised). Because the batch asks IRIS only for isolates not found in the public database, invalid IRIS
  credentials affect the batch only from the first such isolate on, while every individual lookup asks IRIS first.
  [SUSPECTED-DEFECT]
- **Duplicate locus in a remote response**: If a remote record listed the same locus more than once, building the
  copied values would fail and, as above, end the run (derived from code; whether PubMLST returns such records was
  not verified). [SUSPECTED-DEFECT]
- **Long synchronous run**: For each isolate up to three remote requests per database are made one after another
  within a single page request; no request time limit is configured beyond the platform default (derived). Large
  periods, such as the default full year, can therefore take a long time and may exceed that limit.
- **Isolate saved in full**: Each matched isolate is written back as a whole with the values loaded at the start of
  the run, not only its PubMLST link. A change saved by another user on the same isolate form while the run is still
  in progress could thereby be overwritten without notice (derived from code, not exercised). [SUSPECTED-DEFECT]
- **Misleading log entry**: For every isolate with a stem number, an error entry "Failed to match isolate by
  reference …" is written to the application log, including isolates that were matched successfully.
  [SUSPECTED-DEFECT]
- **Stem number on the individual lookup**: The individual lookup uses the stem number stored when the form was
  opened; a stem number entered or changed in the same form takes effect only after saving and reopening. An isolate
  without a stem number sends the placeholder "DE -", which finds nothing (derived).
- **Client-provided values on save**: The values stored by the individual path are those posted back by the form;
  the server does not fetch them again on save, so the stored copy is only as reliable as the form submission
  (derived; see spec 005). [POTENTIAL-REFACTORING]
- **Authorization**: The matching page and the individual lookup admit only the Standard User role. Navigation shows
  the "PubMLST" menu to Standard User and Administrator accounts, so an Administrator-only account sees "Matching
  (Meningokokken)" but is refused, as in specs 011, 014, 017, and 018. [SUSPECTED-DEFECT] Public Health accounts are
  neither offered the menu nor admitted. The matching submission verifies the anti-forgery token; the individual
  lookup does not, but it only reads from PubMLST.
- **Architecture** (Constitution "Known Architectural Constraints (Legacy Debt)", Principles II and III): The batch
  logic lives in a dedicated matching service with automated tests (period selection, isolates without stem number,
  first/second database, creation of a new local record); the update of an existing local record, the result page,
  and the matching page itself have no automated test. The remote services are covered by tests with simulated
  responses, plus one IRIS test against the live service that runs only on explicit request. The matching page
  creates the matching service, both remote services, and their credentials directly instead of receiving them,
  reflecting the "weak dependency injection" constraint. The feature exists only for Meningococci; there is no
  Haemophilus counterpart. [POTENTIAL-REFACTORING]

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The navigation MUST offer the menu "PubMLST" with the entry "Matching (Meningokokken)" to Standard User
  and Administrator accounts and MUST NOT offer it to Public Health accounts.
- **FR-002**: The matching page, its submission, and the individual lookup MUST admit only accounts holding the
  Standard User role and MUST refuse Administrator-only and Public Health accounts (current behavior, see Edge
  Cases); unauthenticated requests MUST be sent to sign in.
- **FR-003**: The matching page MUST offer "Start-Datum" and "Ende-Datum", prefilled with 1 January and 31 December
  of the current year, the waiting hint, and the button "Abfrage der PubMLST Daten"; the submission MUST verify the
  anti-forgery token.
- **FR-004**: The batch MUST select every Meningococci isolate whose sending's sampling date lies within the period,
  or, when the sampling date is missing, whose receiving date lies within the period, without excluding deleted
  sendings and without validating the period.
- **FR-005**: For each selected isolate with a stem number, the batch MUST search the prefixed stem number first in
  the public PubMLST Neisseria isolate database and then, only if not found, in the IRIS PubMLST database, and MUST
  accept a database's answer only when exactly one record is returned.
- **FR-006**: For each selected isolate without a stem number, the batch MUST NOT contact PubMLST and MUST list the
  isolate as not found.
- **FR-007**: For each found record, the system MUST fetch the record's details and allele designations and MUST
  copy the values listed under "Copied PubMLST values", using an empty value for a missing locus and no value for a
  missing sequence type, clonal complex, or reactivity.
- **FR-008**: When no local record with the found PubMLST id exists, the batch MUST create one with the PubMLST id,
  database name, and all copied values; otherwise it MUST overwrite the seventeen typing values of the existing
  record and MUST keep its PubMLST id and database name.
- **FR-009**: The batch MUST link each found isolate to the created or updated local record and MUST save it before
  processing the next isolate; isolates not found MUST keep any existing link unchanged.
- **FR-010**: After all isolates are processed, the system MUST show the page "PubMLST Matching abgeschlossen" with
  the number of selected isolates, the number found, and one row per selected isolate with PubMLST id (linked to the
  PubMLST information page) or "kein Treffer", database name, prefixed stem number, and the laboratory number
  linking to the isolate form.
- **FR-011**: The individual lookup MUST search the given prefixed stem number first in the IRIS database and then in
  the public database, MUST return the first record found with the values of FR-007, MUST answer "not found" when
  neither returns exactly one record, and MUST NOT write to the database; storing is part of the isolate save (spec
  005).
- **FR-012**: Within one batch run, the system MUST establish the IRIS session at most once and MUST search each
  isolate at most once per database; across runs, it MUST search every selected isolate again (current behavior, no
  skip of already matched isolates).
- **FR-013**: All other features (isolate form, exports, reports) MUST use only the locally stored copy and MUST NOT
  contact PubMLST.
- **FR-014**: Only the prefixed stem number and, for the IRIS database, the configured IRIS credentials MUST be sent
  to PubMLST; the IRIS credentials MUST be read from the application settings file (spec 018).
- **FR-015**: This feature MUST document the current matching behavior only and MUST NOT introduce new matching rules,
  databases, fields, or permissions.

### Copied PubMLST values

Remote names are the PubMLST locus and field names used for the lookup; they are given only to identify the source of
each value. The PubMLST id and the database name are stored in addition.

| Local value (label on the isolate form) | Remote source | When missing remotely |
| --- | --- | --- |
| PorA-VR1 | Locus `PorA_VR1` | Empty |
| PorA-VR2 | Locus `PorA_VR2` | Empty |
| FetA-VR | Locus `FetA_VR` | Empty |
| 'porB | Locus `'porB` | Empty |
| fHbp | Locus `'fHbp` | Empty |
| NHBA | Locus `NHBA_peptide` | Empty |
| nadA | Locus `NadA_peptide` | Empty |
| penA | Locus `penA` | Empty |
| GyrA | Locus `gyrA` | Empty |
| parC | Locus `NEIS1525` | Empty |
| parE | Locus `NEIS1600` | Empty |
| rpoB | Locus `rpoB` | Empty |
| rplF | Locus `'rplF` | Empty |
| ST | `ST` of the record's first typing scheme | No value |
| cc | `clonal_complex` of the record's first typing scheme | No value |
| Bexsero reactivity | Phenotypic field `Bexsero_reactivity` | No value |
| Trumenba reactivity | Phenotypic field `Trumenba_reactivity` | No value |

### Remote databases

| Database name (shown in "Datenbank") | Access | Batch order | Individual lookup order |
| --- | --- | --- | --- |
| `pubmlst_neisseria_isolates` (public PubMLST Neisseria isolates) | Public, no credentials | First | Second |
| `pubmlst_neisseria_iris` (IRIS) | Signed requests with the `IrisAccessToken`, `IrisAccessTokenSecret`, `IrisConsumerKey`, and `IrisConsumerSecret` settings; values not reproduced | Second | First |

Local records created before the database name was recorded carry the public database name.

### Roles and Boundaries

| Actor or role | Current observable involvement |
| --- | --- |
| Standard User (`Standardbenutzer`) | Sees the "PubMLST" menu; runs the batch matching and the individual lookup; saves individual lookup results with the isolate (spec 005). |
| Administrator | Sees the "PubMLST" menu but is refused by the matching page and the lookup unless the account also holds the Standard User role. |
| Public Health (`RKI`) | Is neither offered the menu nor admitted. |
| Laboratory staff (outside the application) | Upload sequence data to PubMLST; matching finds an isolate only after PubMLST has published it. |
| PubMLST (external service) | Answers the reference searches and returns record details and allele designations; it receives no patient data. |
| Operator with server access | Maintains the IRIS credentials in the application settings file (spec 018). |

### Key Entities *(include if feature involves data)*

- **Meningococci isolate**: Selected by the dates of its sending; identified remotely by its stem number with the
  prefix "DE"; optionally linked to one local PubMLST record.
- **Local PubMLST record**: A copy of one remote PubMLST record: PubMLST id, database name, and seventeen typing
  values. It is looked up by PubMLST id and may be linked to several isolates. It has no date of retrieval and no
  history.
- **Matching period**: Start and end date of the batch run.
- **Match result row**: Per selected isolate: PubMLST id and database (when found), prefixed stem number, and
  laboratory number with a link to the isolate form. It is shown once and not stored.
- **IRIS credentials**: Four named settings of the application settings file used to sign requests to the IRIS
  database.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: For a period with N isolates, the result page lists exactly N rows and its found count equals the number
  of rows with a PubMLST id, in 100% of runs that complete.
- **SC-002**: In 100% of tested isolates without a stem number, no remote request is made and the row shows
  "kein Treffer".
- **SC-003**: After a completed run, every found isolate is linked to a local record whose seventeen typing values
  equal the remote values, and at most one local record exists per PubMLST id, in 100% of tests.
- **SC-004**: Running the same period twice without remote changes creates no additional local records and leaves
  all values unchanged in 100% of tests.
- **SC-005**: Opening isolate forms and producing exports issues zero requests to PubMLST in 100% of tests.
- **SC-006**: Across the three roles, observed access to the matching page and the individual lookup matches the
  Roles and Boundaries table in 100% of attempts.

## Assumptions

- Current application code, views, schema migrations, the application settings file (names only), role constants,
  and the automated tests are the source of truth. No running system was exercised and the external PubMLST service
  was not called; remote behavior (exact-match search, response layout) is derived from the requests the code sends
  and the simulated responses used in the tests.
- Accounts are assumed to hold exactly one role, as in specs 011, 014, 017, and 018, so an Administrator account is
  refused by the matching page and the lookup.
- "Fetched only once" from the backlog description is interpreted against the code: within one run each isolate is
  searched at most once per database and the IRIS session is reused, and all other features read only the local
  copy; but every batch run and every individual lookup fetches again, so the local copy is authoritative only until
  the next matching that covers the isolate.
- The refusal of a submission without anti-forgery token, the redirect of unauthenticated users to sign in, the
  handling of empty date fields, and the request time limit are derived from the standard behavior of the framework
  features used; not observed in a browser.
- The consequences of unhandled remote failures (aborted run, error page, partial saves) and of concurrent edits are
  derived from code paths; not exercised.
- Sending dates are assumed to be stored without time of day, as they are entered as dates only; therefore the end
  day of the period is effectively included.
- Whether the IRIS and public databases can assign the same numeric PubMLST id to different records was not verified;
  the related edge case describes what the code would do.
