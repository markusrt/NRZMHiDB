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
- Verification 2026-11-08: US1.3 states that the button is also disabled while the run waits (observed with the
  submission held and aborted in the browser).
- Verification 2026-11-08: US2.3 adds that the database is emptied too and that the link to the PubMLST entry keeps
  its previous target after a failed lookup; tagged [SUSPECTED-DEFECT] (UX-017).
- Verification 2026-11-08: Edge case "Periods and bounds" states that the form has no check of its own, that an empty
  or unreadable start date matches every earlier isolate and an empty end date none, and that stored dates carry no
  time of day; tagged [SUSPECTED-DEFECT] (UX-016). The related assumption is now observed, not assumed.
- Verification 2026-11-08: Edge case "Unhandled remote failures abort the run" adds that the individual lookup fails
  before any request without the IRIS settings (reproduced at unit level).
- Verification 2026-11-08: Edge case "Architecture" corrected: four live-service tests (three public, one IRIS), and
  the update of an existing local record is now covered by tests added in this verification.
- Verification 2026-11-08: cross-reference notes added for defects from other specs confirmed here: UX-013 (stem
  number edge case), DATA-036 (client-provided values edge case), SEC-005 (authorization edge case); new assumption
  on what the verification ran. Tags now: 13 [SUSPECTED-DEFECT], 4 [POTENTIAL-REFACTORING].
