# Specification Quality Checklist: EUCAST clinical breakpoints management

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

- Validation iteration 1: all 16 quality items pass. Page titles, field labels, button labels, menu entries, validation messages, and role names are quoted as user-visible labels, not implementation details, consistent with specs 014 and 016. Internal identifiers ("Meningococci", "None", "EucastClinicalBreakpoint") are quoted only because the application shows them to the user. Controller/database wiring is mentioned only in one edge case because it relates to the constitution's "Known Architectural Constraints (Legacy Debt)".
- E-Test entry and interpretation, report documents, and exports are summarised at a high level only and referenced to specs 004, 005, 008, 009, 010, and 011.
- Run non-interactively; no clarification questions were asked. Points for review (recorded under Assumptions):
  - Deleting a breakpoint record also deletes all E-Test measurements that refer to it (cascading relation from the original migration); derived, not exercised.
  - A new record saved without choosing "Gültig für" is assumed to be stored as "Haemophilus"; not exercised.
  - "ohne Grenzwerte" records are assumed to be listed after dated records (database null ordering); not verified.
  - After a "Gültig für" change, existing measurements of the previous pathogen are assumed to show no selected breakpoint and to block saving until another is chosen; not exercised.
  - The display of "Gültig ab" with time of day and of internal identifiers on list, details, and delete pages is derived from standard display behavior; not observed in a browser.
  - The stored breakpoint values themselves (which EUCAST/CLSI versions exist) were not inspected.
- Consistent with specs 011 and 014: accounts are assumed to hold exactly one role, so an Administrator sees the menu entry but is refused. The breakpoint pages do check a role (Standard User only), and all three forms verify the anti-forgery token.
- As-is observations tagged in the spec: 6 [SUSPECTED-DEFECT], 2 [POTENTIAL-REFACTORING].
- Verification 2026-11-08: US2.3–US2.5 and FR-005 now say which checks run in the browser (empty details, length
  limits, with the observed messages) and which only on the server (all-or-none rule, its message shown below each of
  the four fields); the spec said only "a validation message".
- Verification 2026-11-08: FR-004 and the "Gültig für" assumption now say that the browser requires a "Gültig für"
  choice ("'Gültig für' darf kein Nullwert sein."); the assumed storage as "Haemophilus" applies only to a request that
  bypasses the browser.
- Verification 2026-11-08: FR-013 and the "Effects of Changes on Existing Data" table now say that exports have fixed
  antibiotic columns, so a measurement whose record names an antibiotic without a column is not exported; the table
  previously said exports list such measurements under the new antibiotic.
- Verification 2026-11-08: Edge cases on edited thresholds and on deletion extended with the observed interplay with
  isolate saves (stale result kept on re-save, DATA-035; deletion also removes orphaned rows, DATA-033).
- Verification 2026-11-08: Assumptions on cascading deletion, "ohne Grenzwerte" ordering, "Gültig für" changes and the
  list/details display updated to observed; coverage notes in Assumptions and Edge Cases updated for the unit tests
  added in the verification (validator, record label, report values).
