# Feature Specification: Deleted-records recovery

**Feature Branch**: `speckit-init`

**Created**: 2026-10-22

**Status**: Draft (status-quo inventory)

**Input**: User description: "Feature title: \"Deleted-records recovery\" (backlog ref C5, Group C — Reference data & administration). Document the current, as-implemented behavior of viewing and recovering soft-deleted records as it exists today: deleted senders and deleted sendings (Haemophilus). Read the Deleted actions in HaemophilusWeb/Controllers/SenderController.cs and PatientSendingController.cs (NotDeletedSendings filtering) and related Views. Capture how deletion is soft vs hard, how items are listed, and how/whether they are restored. Document observable behavior only — do not propose changes."

> **Scope note**: This is an *as-is* specification (backlog ref C5). It records the current, observable behavior of
> the two Administration pages for deleted records — "Gelöschte Einsender" and "Gelöschte Einsendungen
> (Haemophilus)" — together with the soft-delete and restore steps they depend on: how a sender or a submission is
> marked as deleted, what the two pages list, how "Wiederherstellen" (restore) works, which other paths clear the
> deleted mark, whether a Meningococci counterpart exists, and who may use these functions. It also summarises, by
> reference, where soft-deleted records are still included or excluded elsewhere. It does **not** propose changes.
> Out of scope and referenced instead: the sender directory and the sender delete warning (`specs/001-sender-management`),
> Haemophilus and Meningococci submission intake, editing, and the delete confirmation pages
> (`specs/002-patient-sending-intake`, `specs/003-meningo-patient-sending`), exports (`specs/011-data-exports-generic`,
> `specs/013-sender-export`), patient merges (`specs/014-merge-patients-haemophilus`,
> `specs/015-merge-patients-meningococci`), and PubMLST matching (`specs/019-pubmlst-matching-meningococci`). Permanent
> deletion of other records (patients during a merge, health offices, counties, EUCAST breakpoints) is not soft
> deletion, offers no recovery, and is covered by the respective specs (014, 015, 016, 017) where specified.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Review deleted senders and restore one (Priority: P1)

A Standard User opens Administration → "Gelöschte Einsender", finds a sender that was removed from the active
sender directory, and selects "Wiederherstellen". The sender is active again and can be chosen for new submissions.

**Why this priority**: It is the only recovery page that lists exactly the deleted records it names, and senders
are shared by both pathogens, so a wrongly deleted sender blocks new submissions of both.

**Independent Test**: With a Standard User account, soft-delete a sender (spec 001), open "Gelöschte Einsender",
verify that it is listed there and not in "Einsender", select "Wiederherstellen", and verify that the "Einsender"
list is shown and contains the sender again.

**Acceptance Scenarios**:

1. **Given** a Standard User or an Administrator account, **When** the navigation is shown, **Then** the
   "Administration" menu offers "Gelöschte Einsender" (in its general part, together with "Konfiguration",
   "Antibiotika Grenzwerte", and "Gesundheitsämter"); a Public Health (`RKI`) account is not offered the menu.
2. **Given** a Standard User opens "Gelöschte Einsender", **When** the page is shown, **Then** it lists every sender
   marked as deleted — and only those — with the columns "Name", "Postleitzahl", and "Ort" and a green button
   "Wiederherstellen" per row.
3. **Given** the list, **Then** it is sorted by name ascending, shows eight rows per page with a search box and
   paging, offers no choice of page size, and remembers sorting, search, and page in the browser for the next visit.
   It shows no submission counts, no deletion date, and no link to the sender's details or edit page.
4. **Given** a listed sender, **When** "Wiederherstellen" is selected, **Then** the deleted mark is cleared
   immediately, without a confirmation step and without checking the sender's field rules, and the active sender
   list "Einsender" is shown; no success message is displayed.
5. **Given** a restored sender, **Then** it again appears in "Einsender", in the sender choice of new and edited
   submissions of both pathogens, and — when it has non-deleted submissions in the range — in both sender exports
   (spec 013); its submissions were attached to it throughout (spec 001).
6. **Given** no sender is marked as deleted, **When** the page is opened, **Then** an empty table is shown.

---

### User Story 2 - Open "Gelöschte Einsendungen (Haemophilus)" (Priority: P1)

A Standard User who soft-deleted a Haemophilus submission by mistake opens Administration → "Gelöschte
Einsendungen (Haemophilus)" expecting to find it there and restore it. The page, however, lists the **active**
(not deleted) Haemophilus submissions, so the deleted submission cannot be found on it.

**Why this priority**: This is the page the backlog entry and specs 002 and 003 describe as the recovery path for
submissions; its actual behavior determines whether a deleted submission can be recovered through the user
interface at all.

**Independent Test**: Soft-delete one Haemophilus submission (spec 002), open "Gelöschte Einsendungen
(Haemophilus)", and verify that this submission is missing while every active Haemophilus submission is listed.

**Acceptance Scenarios**:

1. **Given** a Standard User or an Administrator account, **When** the "Administration" menu is opened, **Then** it
   offers, under the heading "Haemophilus", the entries "Gelöschte Einsendungen (Haemophilus)" and "Patienten
   Zusammenfügen"; under the heading "Meningokokken" it offers only "Patienten Zusammenfügen".
2. **Given** a Standard User selects "Gelöschte Einsendungen (Haemophilus)", **When** the page "Gelöschte
   Einsendungen" is shown, **Then** it lists every Haemophilus submission that is **not** marked as deleted, and no
   submission that is marked as deleted, with the columns "Initialen", "Geburtsdatum", "Stammnummer" (with prefix),
   "Labornummer" (with prefix), and "Eingangsdatum" and a green button "Wiederherstellen" per row.
   [SUSPECTED-DEFECT]
3. **Given** the list, **Then** it is sorted by initials ascending, shows eight rows per page with a search box and
   paging, offers no choice of page size, and remembers sorting, search, and page in the browser; all listed
   submissions are loaded into the page at once.
4. **Given** a listed (active) submission, **When** "Wiederherstellen" is selected, **Then** the submission is saved
   unchanged (it was not deleted), no confirmation or message is shown, and the Haemophilus submission overview is
   displayed.
5. **Given** a soft-deleted Haemophilus submission, **Then** neither this page nor the Haemophilus submission overview
   (including its search) lists it; no page of the application lists deleted submissions, so the user interface
   offers no way to find it again.

---

### User Story 3 - Restore a deleted submission by other means (Priority: P2)

Because the recovery page does not list deleted submissions, a deleted submission is restored today only when a
user who knows its internal submission number calls the restore address directly, or opens and saves its edit
page by direct address.

**Why this priority**: These are the only remaining ways a soft-deleted submission becomes active again; they
explain how the "recoverable" promise of specs 002 and 003 is met today.

**Independent Test**: Soft-delete a Haemophilus submission, note its internal submission number, request the
restore address with that number, and verify that the submission appears in the overview again; repeat by opening
its edit page by direct address and saving it.

**Acceptance Scenarios**:

1. **Given** a soft-deleted Haemophilus submission and its internal submission number, **When** a Standard User
   requests the restore address of the Haemophilus submissions with that number, **Then** the deleted mark is
   cleared, the submission is saved, and the Haemophilus submission overview is shown with the submission included
   again.
2. **Given** a soft-deleted Meningococci submission and its internal number, **When** a Standard User requests the
   restore address of the Meningococci submissions, **Then** the deleted mark is cleared in the same way and the
   Meningococci submission overview is shown.
3. **Given** a soft-deleted submission of either pathogen, **When** its edit page is opened by direct address and
   saved validly, **Then** the submission is active again, because the edit form does not carry the deleted mark and
   saving writes every submission field. [SUSPECTED-DEFECT]
4. **Given** a soft-deleted sender, **When** its edit page is opened by direct address and saved validly, **Then**
   the sender is active again for the same reason; the sender form protects the deleted mark from being set, but not
   from being cleared. [SUSPECTED-DEFECT]
5. **Given** the restore address is requested with a missing or unknown submission number, **Then** the generic
   error page is shown; for senders, a missing number gives a "bad request" and an unknown number a "not found"
   answer. [SUSPECTED-DEFECT]

---

### User Story 4 - Learn that Meningococci has no deleted-submissions page (Priority: P2)

A user looks for the Meningococci counterpart of "Gelöschte Einsendungen" and for a way to soft-delete a
Meningococci submission.

**Why this priority**: Answers the backlog's open question and shows that the two pathogens differ in deletion,
contrary to spec 003, User Story 3.

**Independent Test**: As a Standard User, open a Meningococci submission's edit page, select "Eintrag löschen", and
request the Meningococci "deleted submissions" address directly; verify that both end on the error page and that
navigation has no Meningococci entry.

**Acceptance Scenarios**:

1. **Given** the navigation, **Then** there is no "Gelöschte Einsendungen" entry for Meningococci.
2. **Given** the Meningococci "deleted submissions" address is requested directly, **When** it is handled, **Then**
   the generic error page is shown, because no page exists for it; the underlying request would, like the
   Haemophilus one, select non-deleted submissions. [SUSPECTED-DEFECT]
3. **Given** a Meningococci submission's edit page, **When** "Eintrag löschen" is selected, **Then** the generic
   error page is shown, because no delete confirmation page exists for Meningococci, and the submission is not
   deleted. [SUSPECTED-DEFECT]
4. **Given** a confirmation request sent outside the normal pages with a valid anti-forgery token, **Then** a
   Meningococci submission would be soft-deleted and the Meningococci overview shown (derived from code; not
   exercised); this is the only code path that soft-deletes a Meningococci submission.
5. **Given** the Meningococci sender pages (other than "Einsender Export"), **When** the sender list, deleted-sender
   list, delete, or edit address of the Meningococci sender area is requested directly, **Then** the error page is
   shown, because only the export page exists there; a restore request by direct address would work and restore the
   shared sender (derived). Sender maintenance is done through the Haemophilus "Einsender" pages (spec 001).

---

### User Story 5 - Be refused (Priority: P3)

A user without the required role tries to open a deleted-records page or to restore a record.

**Why this priority**: Defines the boundary of functions that change which senders and submissions are active.

**Independent Test**: Request both deleted-records pages and both restore addresses with an Administrator-only and
with a Public Health account and verify that all are refused.

**Acceptance Scenarios**:

1. **Given** an account holding only the Administrator role, or a Public Health account, **When** either
   deleted-records page, a restore address, or a delete page is requested, **Then** the request is refused, because
   all of them admit only the Standard User role (`Standardbenutzer`).
2. **Given** a request without a signed-in account, **When** it is handled, **Then** the user is sent to sign in.
3. **Given** a delete confirmation of a sender or a submission without the page's anti-forgery token, **When** it is
   handled, **Then** it is rejected and nothing is deleted.
4. **Given** a restore request, **Then** it is a plain link without anti-forgery token and without confirmation, so
   any request to the restore address by a signed-in Standard User (for example one triggered from another page)
   restores the record. [SUSPECTED-DEFECT]

### Edge Cases

- **Soft delete is the only deletion for senders and submissions**: Deleting a sender or a submission only sets a
  deleted mark; the record, its patient, its isolate, and all laboratory data remain stored. No page offers a
  permanent deletion of a sender or a submission. The generated, untranslated submission pages ("Delete", "Are you
  sure you want to delete this?") that exist next to the real pages have no matching function and do not delete
  anything. By contrast, the generated, untranslated patient delete page of either pathogen, reachable only by
  direct address, removes a patient permanently, without recovery (derived from code; not specified elsewhere; its
  interaction with the patient's submissions corresponds to the merge, specs 014 and 015).
- **What deletion changes for senders**: A deleted sender disappears from "Einsender", from the sender choice of new
  submissions, and from both sender exports (spec 013), but remains attached to its submissions and is still shown
  in the sender choice of those submissions (spec 001, spec 002 FR-004).
- **What deletion changes for submissions** (summary with references):
  - Excluded: the Haemophilus and Meningococci submission overviews and their search, all seven exports
    (spec 011), the token-protected EpiScanGIS export (spec 023), the selection of
    senders for the sender exports (spec 013), the list of submissions on the sender delete warning (spec 001), and
    the patient lookup and reassignment of both merges (specs 014, 015).
  - Still included: the "Anzahl Einsendungen", "Stammnummern", and "Labornummern" values of the sender exports
    (spec 013), the PubMLST batch matching (spec 019), and the patient duplicate check at intake, which compares
    against all stored patients (spec 002; derived). The edit, isolate, and report pages of a deleted submission
    remain reachable by direct address, because they do not check the deleted mark (derived from code); the PubMLST
    matching result links to the isolate forms of deleted Meningococci submissions it processed (spec 019).
  - Merge failure: a soft-deleted submission that remains attached to a patient being merged away makes the merge
    fail with the error page and nothing changed (specs 014, 015).
- **Numbers are not reused**: The stem and laboratory numbers of soft-deleted submissions stay assigned; new
  submissions continue counting from the highest number including those of deleted submissions (derived from code).
- **Inverted selection on "Gelöschte Einsendungen"**: The page selects submissions that are *not* deleted. The
  source history shows that before a 2018 restructuring, which merged the Haemophilus and Meningococci controllers,
  the page selected deleted submissions; the restructuring replaced the selection with the shared "not deleted"
  selection (derived from version history). Spec 002 records this as a "naming deviation"; its effect is that no
  page lists deleted submissions. [SUSPECTED-DEFECT]
- **Page size**: Because the page loads every active Haemophilus submission at once and looks up each submission's
  isolate individually, opening it takes longer as the database grows (derived; not measured).
  [POTENTIAL-REFACTORING]
- **Restore of an active record**: "Wiederherstellen" on an active submission (User Story 2) or a restore request
  for an active sender saves the record without change; it is not refused.
- **Field rules skipped on delete and restore**: Deleting and restoring a sender saves it without the sender field
  rules (required name and first phone number, phone and e-mail formats), so incomplete legacy senders can be
  deleted and restored (spec 001). For submissions, the switch that skips these rules is applied to a different
  data connection than the one that saves the submission; because submissions carry no such save-time field rules,
  this makes no observable difference (derived from code). [POTENTIAL-REFACTORING]
- **No audit trail**: Neither a sender nor a submission records who deleted or restored it, or when; the deleted
  mark is the only trace, and restoring removes it (cf. Constitution Principle I, "Data-losing operations … MUST be
  explicit, auditable, and recoverable"). [POTENTIAL-REFACTORING]
- **Relation to earlier specs**: Specs 002 and 003 (User Story 3 of both, spec 002 FR-016, spec 003 FR-018, SC-006
  of both) describe the soft delete and restore of submissions from the intake perspective and refer to this spec
  for the recovery path: restoring is possible only by direct address or by saving the edit page, and Meningococci
  submissions cannot be soft-deleted through the normal pages, which agrees with the finding of spec 015.
- **Authorization**: All delete, deleted-list, and restore functions admit only the Standard User role. The
  "Administration" menu with both deleted-records entries is shown to Standard User and Administrator accounts, so an
  Administrator-only account sees the entries but is refused, as in specs 011, 014, 017, 018, and 019.
  [SUSPECTED-DEFECT] Public Health accounts are neither offered the menu nor admitted. Delete confirmations verify the
  anti-forgery token; restore requests do not (User Story 5).
- **Architecture** (Constitution "Known Architectural Constraints (Legacy Debt)", Principles II and III): Sender and
  submission deletion, listing, and restore live in shared base controllers used by both pathogens; the Meningococci
  sender and submission controllers inherit the delete, deleted-list, and restore functions, but the pages for them
  exist only for Haemophilus, which is why the Meningococci variants end on the error page. Restoring a submission
  goes through a separately created sending controller with its own data connection, reflecting the "weak dependency
  injection" constraint. Automated tests cover the sender delete warning, the "non-deleted only" sender choice, and the
  "non-deleted only" selection of the generated, unlinked submission list, but not the deleted-records pages or
  restoring. [POTENTIAL-REFACTORING]

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The navigation MUST offer, in the "Administration" menu, "Gelöschte Einsender" and, under
  "Haemophilus", "Gelöschte Einsendungen (Haemophilus)" to Standard User and Administrator accounts, MUST NOT offer a
  Meningococci "Gelöschte Einsendungen" entry, and MUST NOT offer the menu to Public Health accounts.
- **FR-002**: All delete, deleted-list, and restore functions for senders and submissions MUST admit only accounts
  holding the Standard User role and MUST refuse Administrator-only and Public Health accounts (current behavior,
  see Edge Cases); unauthenticated requests MUST be sent to sign in.
- **FR-003**: Deleting a sender or a submission MUST only set its deleted mark and MUST keep the record and all
  related data; the delete confirmation MUST verify the anti-forgery token.
- **FR-004**: "Gelöschte Einsender" MUST list exactly the senders marked as deleted with name, postal code, city, and
  a "Wiederherstellen" button, sorted by name, eight per page.
- **FR-005**: "Gelöschte Einsendungen (Haemophilus)" MUST, as currently implemented, list every Haemophilus
  submission that is not marked as deleted, and no deleted submission, with initials, birth date, stem number,
  laboratory number, receiving date, and a "Wiederherstellen" button, sorted by initials, eight per page.
- **FR-006**: Restoring MUST clear the deleted mark of the addressed sender or submission without confirmation,
  without anti-forgery token, and without applying field rules, and MUST then show the active sender list or the
  submission overview of the record's pathogen; restoring an active record MUST leave it unchanged.
- **FR-007**: Restoring a submission MUST be possible by direct address for both pathogens; a missing or unknown
  submission number MUST end on the generic error page, and for senders a missing number MUST give "bad request" and
  an unknown number "not found".
- **FR-008**: Saving the edit page of a deleted sender or submission MUST clear its deleted mark (current behavior).
- **FR-009**: The Meningococci deleted-submissions address, the Meningococci submission delete page, and the
  Meningococci sender pages other than the export MUST end on the generic error page, because no pages exist for
  them (current behavior).
- **FR-010**: Soft-deleted submissions MUST be excluded and included elsewhere exactly as listed under "What deletion
  changes for submissions" (references to specs 001, 002, 011, 013, 014, 015, 019).
- **FR-011**: Stem and laboratory numbers of soft-deleted submissions MUST stay assigned and MUST NOT be reused.
- **FR-012**: The system MUST NOT record who deleted or restored a record or when (current behavior).
- **FR-013**: This feature MUST document the current behavior only and MUST NOT introduce new pages, selections,
  permissions, or deletion types.

### Deletion and recovery overview

| Record | How it is deleted | Listed as deleted | How it is restored today |
| --- | --- | --- | --- |
| Sender (shared by both pathogens) | "Eintrag löschen" on the sender edit page → confirmation (spec 001); soft | Yes, "Gelöschte Einsender" | "Wiederherstellen" on that page; restore address; saving the sender edit page |
| Haemophilus submission | "Eintrag löschen" on the submission edit page → "Löschen bestätigen" (spec 002); soft | No — the page lists active submissions | Restore address with the internal number; saving the edit page by direct address |
| Meningococci submission | Not possible through the pages (missing confirmation page); only by a request outside the pages; soft | No page | Restore address with the internal number; saving the edit page by direct address |
| Patient (either pathogen) | Removed permanently by a merge (specs 014, 015) or by the generated patient delete page reachable by direct address | — | Not recoverable in the application |

### Roles and Boundaries

| Actor or role | Current observable involvement |
| --- | --- |
| Standard User (`Standardbenutzer`) | Sees both deleted-records entries; deletes senders and Haemophilus submissions; restores senders from "Gelöschte Einsender" and any record by direct address. |
| Administrator | Sees both entries but is refused by every delete, deleted-list, and restore function unless the account also holds the Standard User role. |
| Public Health (`RKI`) | Is neither offered the entries nor admitted. |
| Operator with database access | The only party able to list deleted submissions today (outside the application). |

### Key Entities *(include if feature involves data)*

- **Sender**: Shared by both pathogens; carries a deleted mark ("Gelöscht") and no record of when or by whom it was
  set. Remains referenced by its submissions while deleted.
- **Submission (sending)**: Haemophilus or Meningococci; carries a deleted mark ("Gelöscht") and no deletion date or
  user. Keeps its patient, isolate, stem and laboratory numbers, and laboratory results while deleted.
- **Deleted-records list**: A page showing records with a "Wiederherstellen" button; one exists for senders and one
  for Haemophilus submissions (which lists active submissions); none exists for Meningococci submissions.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: "Gelöschte Einsender" lists 100% of senders marked as deleted and 0 active senders in every test.
- **SC-002**: "Gelöschte Einsendungen (Haemophilus)" lists 100% of active Haemophilus submissions and 0 soft-deleted
  submissions in every test (current behavior).
- **SC-003**: After "Wiederherstellen" on a deleted sender, the sender appears in "Einsender" and in the sender choice
  of new submissions in 100% of tests, without a confirmation step.
- **SC-004**: A soft-deleted submission of either pathogen becomes visible in its overview again after a restore
  request by direct address or after saving its edit page in 100% of tests, and through no other user-interface path.
- **SC-005**: Across the three roles, observed access to the deleted-records pages and restore functions matches the
  Roles and Boundaries table in 100% of attempts.
- **SC-006**: In 100% of tests, deleting a sender or a submission leaves the record and its related data stored, and
  no stem or laboratory number of a deleted submission is assigned again.

## Assumptions

- Current application code, views, navigation, version history, role constants, and the automated tests are the
  source of truth. No running system was exercised.
- Accounts are assumed to hold exactly one role, as in specs 011, 014, 017, 018, and 019, so an Administrator account
  is refused by these functions.
- "Restore address" means the restore function of the sender or submission pages requested with the internal record
  number, as the "Wiederherstellen" buttons do; users are assumed to know internal submission numbers only from the
  address bar of earlier visits, since no page shows them for deleted submissions.
- The error pages for the missing Meningococci pages and for missing or unknown submission numbers, the refusal of
  confirmations without anti-forgery token, and the sign-in redirect are derived from the standard behavior of the
  framework features used; not observed in a browser.
- The implicit restore by saving an edit page is derived from the edit forms (which do not carry the deleted mark)
  and from the save, which writes all fields; not exercised.
- That the Meningococci soft delete would work for a crafted confirmation request, and that a Meningococci sender
  restore by direct address would work, are derived from the inherited code; not exercised.
- The intent of the "Gelöschte Einsendungen" selection is inferred from the page title, the menu entry, the
  "Wiederherstellen" button, and the version history; the current behavior is documented as found.
