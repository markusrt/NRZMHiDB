# Feature Specification: Sender management (shared across both databases)

**Feature Branch**: `001-sender-management`

**Created**: 2026-08-26

**Status**: Draft (status-quo inventory)

**Input**: User description: "Feature title: \"Sender management\" (backlog ref A1, Group A — Core laboratory workflow). Document the current, as-implemented behavior of Sender management as it exists today: creating, editing, viewing, listing, deleting, and recovering senders, and the fact that senders are shared across both the Haemophilus and Meningococci databases. Read HaemophilusWeb/Controllers/SenderController.cs (and any MeningoSender equivalent), the Sender model and its FluentValidation validator, and the related Views. Capture observable behavior, validation rules, roles/permissions, and outcomes only — do not propose changes."

> **Scope note**: This is an *as-is* specification. It documents the behavior of Sender
> management exactly as implemented on the current branch (backlog ref A1). It does **not**
> propose changes. Where the implementation deviates from what the backlog prompt assumed,
> the deviation is recorded factually (see Assumptions).

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Maintain the shared sender directory (Priority: P1)

A laboratory user maintains the directory of senders (submitting institutions — "Einsender")
that laboratory submissions are attributed to. The same directory is shared by both the
Haemophilus and the Meningococci workflows, so a sender created once is usable from either
pathogen's intake workflow.

**Why this priority**: Every laboratory submission must be attributed to a sender. Without a
maintained sender directory, submissions cannot be recorded or reported, so this is the
foundational capability of the feature.

**Independent Test**: Sign in as a standard user, open the "Einsender" list, create a new
sender with the required fields, confirm it appears in the list, and confirm it is selectable
when recording a submission in either pathogen workflow.

**Acceptance Scenarios**:

1. **Given** an authenticated standard user on the sender list, **When** they choose "Neuen
   Einsender anlegen", fill in at least Name and Telefon #1, and save, **Then** the new sender
   is persisted and the user is returned to the sender list where the sender appears.
2. **Given** an authenticated standard user, **When** they open an existing sender for editing,
   change one or more fields, and save, **Then** the changes are persisted and the user is
   returned to the sender list.
3. **Given** a sender exists in the shared directory, **When** a submission is recorded in either
   the Haemophilus or the Meningococci workflow, **Then** the same sender record is available
   for selection in both.

---

### User Story 2 - Remove a sender from active use and recover it later (Priority: P2)

A user removes a sender that should no longer be offered for new submissions, and can later
restore it. Removal is a soft delete (recoverable) rather than a permanent deletion: existing
submissions keep their attribution to that sender.

**Why this priority**: The directory must stay clean of obsolete senders without breaking the
attribution of historical submissions, and mistaken removals must be reversible. This is
important but secondary to being able to create and edit senders at all.

**Independent Test**: Delete a sender, confirm it disappears from the active list, confirm it
appears under "Gelöschte Einsender", restore it, and confirm it returns to the active list.

**Acceptance Scenarios**:

1. **Given** a sender with no active submissions, **When** the user confirms deletion, **Then**
   the sender is flagged as deleted, is removed from the active sender list, and no longer
   appears for selection on new submissions.
2. **Given** a sender that still has active submissions in either database, **When** the user
   opens the delete confirmation page, **Then** a warning is shown listing the associated
   submissions (from both Haemophilus and Meningococci) with links to reassign them, and the
   user may still confirm deletion.
3. **Given** a deleted sender, **When** an administrator opens "Gelöschte Einsender" and chooses
   "Wiederherstellen", **Then** the sender is un-flagged and returns to the active list.
4. **Given** a sender is deleted, **When** existing submissions already attributed to it are
   viewed, **Then** those submissions remain attributed to the (now-deleted) sender.

---

### User Story 3 - Export the sender directory for a period (Priority: P3)

A user exports the list of senders that submitted material within a chosen date range, together
with per-sender submission counts and identifiers, as a spreadsheet — separately for the
Haemophilus and the Meningococci workflows.

**Why this priority**: Export supports reporting and analysis but is not required for the core
day-to-day maintenance of the directory, so it is the lowest priority of the three journeys.

**Independent Test**: Open the sender export, accept or change the default date range, submit,
and confirm a spreadsheet download is produced containing the senders active in that range.

**Acceptance Scenarios**:

1. **Given** a user opens the sender export without a date range, **When** the page loads,
   **Then** a date range defaulting to the whole of the previous calendar year is presented.
2. **Given** a user submits a date range, **When** the export runs, **Then** a spreadsheet is
   produced listing every sender that had at least one matching submission in that range, with
   sender details plus submission count, stem numbers, and laboratory numbers.
3. **Given** the two workflows, **When** the export is run from the Haemophilus vs. the
   Meningococci menu entry, **Then** each produces a file scoped to its own submissions, named
   for the corresponding database.

---

### Edge Cases

- **Missing required fields**: Creating or editing a sender without a Name or without Telefon #1
  fails validation; the form is redisplayed with validation messages and nothing is persisted.
- **Malformed contact fields**: A malformed e-mail address, or a phone/fax value that does not
  satisfy phone formatting, fails validation on save; the form is redisplayed.
- **Missing id**: Requesting Details, Edit, or Delete with no id returns a "bad request"
  outcome; requesting a non-existent id returns a "not found" outcome.
- **Deleting a sender with active submissions**: Deletion is still permitted; the sender simply
  becomes unavailable for new submissions while remaining attached to the existing ones.
- **Delete/restore bypasses field validation**: Toggling the deleted flag saves the record even
  if it would otherwise fail normal field validation, so an incomplete legacy record can still
  be deleted or restored.
- **Meningococci sender maintenance**: Only the sender *export* is surfaced for the Meningococci
  workflow in navigation; list/create/edit/delete/restore of senders are reached through the
  Haemophilus "Einsender" area and operate on the same shared directory.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST maintain a single, shared directory of senders used by both the
  Haemophilus and the Meningococci workflows; a sender created once is usable from either.
- **FR-002**: The system MUST let an authorized user list active (non-deleted) senders, showing
  Name, Abteilung (department), Ort (city), Telefon #1, and E-Mail, with a paged, sortable,
  searchable table and an action to create a new sender.
- **FR-003**: The system MUST let an authorized user create a sender by entering Name, Abteilung,
  Straße (street), Postleitzahl (postal code), Ort, Telefon #1, Telefon #2, Fax, E-Mail, and
  Bemerkung (remark); on success the sender is persisted and the user is returned to the list.
- **FR-004**: The system MUST require a Name and a Telefon #1 for a sender; it MUST reject a
  save that omits either, redisplaying the entry form without persisting.
- **FR-005**: The system MUST validate that E-Mail, when provided, is a well-formed e-mail
  address, and that Telefon #1, Telefon #2, and Fax, when provided, satisfy phone formatting;
  invalid input MUST cause the save to be rejected and the form redisplayed.
- **FR-006**: The system MUST let an authorized user edit an existing sender's fields and
  persist the changes, returning the user to the list on success.
- **FR-007**: The system MUST provide a read-only details view of a single sender showing Name,
  Abteilung, Telefon #1, Telefon #2, Fax, E-Mail, and Bemerkung.
- **FR-008**: The system MUST support removal of a sender as a soft delete (a recoverable
  "deleted" flag) rather than a permanent deletion; deleted senders MUST NOT appear in the
  active list and MUST NOT be offered for new submissions.
- **FR-009**: On the delete confirmation, the system MUST show the sender's key details and, if
  the sender still has active submissions in either the Haemophilus or the Meningococci database,
  MUST warn the user and list those submissions with links to reassign them, while still allowing
  the deletion to proceed.
- **FR-010**: When a sender is deleted, the system MUST keep any existing submissions attributed
  to that sender; deletion only removes the sender from future selection.
- **FR-011**: The system MUST provide a list of deleted senders (Name, Postleitzahl, Ort) with an
  action to restore ("Wiederherstellen") each one back into the active directory.
- **FR-012**: The system MUST allow the deleted flag to be toggled (delete and restore) without
  enforcing the normal field-level validation on the record.
- **FR-013**: The system MUST provide a per-database sender export that, for a user-supplied date
  range, produces a spreadsheet of senders having matching submissions in that range, including
  sender fields plus submission count, stem numbers, and laboratory numbers, and named for the
  database (Haemophilus or Meningococci).
- **FR-014**: When the export is opened without a date range, the system MUST default the range
  to the entire previous calendar year.
- **FR-015**: The export MUST match a submission to the range by its sampling date when present,
  otherwise by its receiving date, and MUST exclude deleted submissions.
- **FR-016**: All sender maintenance actions (list, create, edit, details, delete, restore,
  export) MUST require the user to hold the standard laboratory-user role; unauthenticated or
  unauthorized requests MUST be denied.
- **FR-017**: The system MUST protect create and edit against over-posting by binding only the
  editable sender fields from the form, so the deleted flag cannot be set through those forms.
- **FR-018**: Requests for a single sender (details, edit, delete) with a missing id MUST return
  a bad-request outcome, and with a non-existent id MUST return a not-found outcome.
- **FR-019**: The create/edit form MUST offer a postal-code lookup that can populate city/postal
  code from a geonames-based search, and MUST warn the user before navigating away with unsaved
  changes.

### Key Entities *(include if feature involves data)*

- **Sender ("Einsender")**: A submitting institution that laboratory material is attributed to.
  Shared across both databases. Key attributes: identifier (Einsendernummer), Name (required),
  Abteilung, Straße, Postleitzahl, Ort, Telefon #1 (required), Telefon #2, Fax, E-Mail,
  Bemerkung, and a "deleted" flag marking soft deletion. Referenced by submissions in both the
  Haemophilus and the Meningococci workflows.
- **Submission ("Einsendung" / Sending)**: A laboratory submission attributed to exactly one
  sender. Two parallel kinds exist — Haemophilus submissions and Meningococci submissions — and
  both reference the shared sender directory. Only used here to determine whether a sender has
  active submissions (for the delete warning) and to scope the export.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: An authorized user can create a new valid sender and see it in the active list
  without any manual data cleanup or a second attempt.
- **SC-002**: A sender created once is selectable from both the Haemophilus and the Meningococci
  submission workflows, with no duplicate entry required.
- **SC-003**: 100% of deletions are recoverable: any deleted sender can be restored to the active
  directory, and no submission loses its sender attribution as a result of a deletion.
- **SC-004**: When deleting a sender that still has active submissions, the user is shown the
  count and links of all affected submissions from both databases before confirming.
- **SC-005**: Attempting to save a sender without a Name or Telefon #1, or with a malformed
  e-mail/phone/fax, never persists the record and always returns an actionable validation message.
- **SC-006**: The sender export for a chosen period returns a spreadsheet whose rows are exactly
  the senders with at least one matching, non-deleted submission in that period, each with its
  submission count and identifiers.

## Assumptions

- **No FluentValidation validator exists for `Sender`.** The backlog prompt referred to "its
  FluentValidation validator", but there is no `SenderValidator` in `Validators/`. Sender
  validation is enforced entirely by DataAnnotations on the `Sender` model: `[Required]` on Name
  and Telefon #1, `[Phone]` on Telefon #1/Telefon #2/Fax, and `[EmailAddress]` on E-Mail. This is
  recorded as an observed as-is fact, not a proposed change.
- **A separate `MeningoSenderController` does exist** (resolving the open question in the backlog):
  both `SenderController` (Haemophilus) and `MeningoSenderController` (Meningococci) derive from a
  shared `SenderControllerBase` and operate on the *same* shared sender table. They differ only in
  which submission set they query (for the delete warning and the export) and in the export's
  database label/filename.
- **Sender CRUD is surfaced only through the Haemophilus "Einsender" area in navigation.** Only
  the Meningococci sender *export* is linked from navigation; list/create/edit/delete/restore are
  reached via the shared Haemophilus sender pages. Both controllers technically expose the same
  actions against the shared directory.
- **Authorization**: All sender actions require the standard laboratory-user role
  ("Standardbenutzer"). Navigation exposes the "Einsender" and "Gelöschte Einsender" entries to
  standard-user or administrator roles; the deleted-senders list is placed under the
  Administration menu. Authorization is enforced at the controller level by role.
- **Soft delete is the only deletion**: there is no hard/permanent delete path for senders in
  this feature; "delete" sets a flag and "restore" clears it.
- **Country is not implemented**: the model notes a placeholder for a sender country field that is
  not present; the feature does not capture a sender's country.
- The export mechanism (Excel generation from a field definition) and the postal-code lookup are
  shared infrastructure used elsewhere in the application; this spec treats them as existing
  dependencies and documents only the sender-facing behavior.
