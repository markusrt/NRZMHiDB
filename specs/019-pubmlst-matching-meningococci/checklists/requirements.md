# Specification Quality Checklist: PubMLST matching (Meningococci)

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

- Validation iteration 1: all 16 quality items pass. Page titles, field labels, hints, button labels, menu entries,
  result messages (including the misspelling "abggeglichen" as shown), and role names are quoted as user-visible
  labels, consistent with specs 016–018. The remote PubMLST database names and locus/field names are quoted only in
  the "Copied PubMLST values" and "Remote databases" tables, because the backlog entry asks which fields are copied
  and from where; the IRIS setting names are named without values, as in spec 018. Service and controller wiring is
  mentioned only in the Architecture edge case relating to the constitution's "Known Architectural Constraints
  (Legacy Debt)".
- The individual lookup is covered here only for its remote side (database order, refusal, no write); the PubMLST tab
  of the isolate form and the save of individual results are referenced to spec 005. Exports printing the stored
  values are referenced to spec 011; interpretation (spec 007) does not use PubMLST data.
- Run non-interactively; no clarification questions were asked. Points for review (recorded under Assumptions):
  - "Fetched only once" (backlog wording) does not hold across runs: every batch run re-searches all isolates of the
    period, including already matched ones, and every individual lookup fetches again; "once" holds only within a
    run (per isolate per database, IRIS session reused) and in the sense that other features read only the local copy.
  - Remote search semantics (exact match on the isolate name) and response layout are derived from the requests sent
    and the simulated test responses; PubMLST was not called.
  - Aborted runs on unhandled remote failures, partial saves, request time limit, the concurrent-edit overwrite, and
    the duplicate-locus failure are derived from code paths; not exercised.
  - Handling of empty date fields, anti-forgery refusal, and sign-in redirect are derived from standard framework
    behavior.
  - Whether the two PubMLST databases can share numeric ids was not verified.
- Consistent with specs 011, 014, 017, and 018: accounts are assumed to hold exactly one role, so an Administrator sees
  the "PubMLST" menu but is refused. Both the matching page and the lookup check a role (Standard User only); the
  matching submission verifies the anti-forgery token, the individual lookup does not (read-only).
- As-is observations tagged in the spec: 8 [SUSPECTED-DEFECT], 4 [POTENTIAL-REFACTORING].
