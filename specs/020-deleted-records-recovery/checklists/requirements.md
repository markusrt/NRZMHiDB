# Specification Quality Checklist: Deleted-records recovery

**Purpose**: Validate specification completeness and quality before proceeding to planning
**Created**: 2026-10-22
**Feature**: [spec.md](../spec.md)

## Content Quality

- [x] No implementation details (languages, frameworks, APIs)
- [x] Focused on user value and business needs
- [x] Written for non-technical stakeholders
- [x] All mandatory sections completed

## Requirement Completeness

- [x] No [NEEDS CLARIFICATION] markers remain
- [x] Requirements are testable and unambiguous
- [x] Success criteria are measurable
- [x] Success criteria are technology-agnostic (no implementation details)
- [x] All acceptance scenarios are defined
- [x] Edge cases are identified
- [x] Scope is clearly bounded
- [x] Dependencies and assumptions identified

## Feature Readiness

- [x] All functional requirements have clear acceptance criteria
- [x] User scenarios cover primary flows
- [x] Feature meets measurable outcomes defined in Success Criteria
- [x] No implementation details leak into specification

## Notes

- Validation iteration 1: all 16 quality items pass. Menu entries, page titles, column labels, button labels, and role
  names are quoted as user-visible labels, consistent with specs 016–019. Controller wiring (shared base controllers,
  separately created sending controller with its own data connection) is mentioned only in the Architecture edge case
  and in the "field rules skipped" edge case, relating to the constitution's "Known Architectural Constraints (Legacy
  Debt)"; the version-history remark is used only to ground the inverted selection.
- Backlog open question answered from code: there is no Meningococci "deleted sendings" page. The Meningococci
  controller inherits the deleted-list, delete, and restore functions, but no pages exist for the list or the delete
  confirmation, so both end on the error page; restore by direct address works. This confirms the finding of spec 015.
- The spec 002 "naming deviation" was verified: "Gelöschte Einsendungen (Haemophilus)" lists active, not deleted,
  submissions; before the 2018 controller restructuring it listed deleted ones. Consequently, no page lists deleted
  submissions.
- Inconsistencies with earlier specs, recorded in the spec and left unchanged there: specs 002 and 003 (User Story 3 of
  both, spec 002 FR-016, spec 003 FR-018, SC-006 of both) describe restoring a deleted submission back to the active list through the user interface,
  and spec 003 describes a working Meningococci soft delete. Spec 002/003 also state that delete and restore skip
  field validation; for submissions this switch has no effect (see the "field rules skipped" edge case).
- Run non-interactively; no clarification questions were asked. Points for review (recorded under Assumptions):
  - Implicit restore by saving the edit page of a deleted sender or submission is derived from the edit forms and the
    save; not exercised.
  - Crafted Meningococci soft delete and Meningococci sender restore by direct address are derived from inherited code;
    not exercised.
  - Error pages for missing pages and for missing/unknown submission numbers, anti-forgery refusal, and sign-in
    redirect are derived from standard framework behavior.
  - The intended meaning of the "Gelöschte Einsendungen" page is inferred from its title, button, and version history.
- Consistent with specs 011, 014, 017, 018, and 019: accounts are assumed to hold exactly one role, so an
  Administrator sees the "Administration" menu entries but is refused. All functions here check the Standard User role;
  delete confirmations verify the anti-forgery token, restore requests (plain links) do not.
- As-is observations tagged in the spec: 9 [SUSPECTED-DEFECT], 4 [POTENTIAL-REFACTORING].
