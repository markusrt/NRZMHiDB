# Specification Quality Checklist: Haemophilus patient + sending intake (PatientSending)

**Purpose**: Validate specification completeness and quality before proceeding to planning
**Created**: 2026-08-26
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

- This is an *as-is* (status-quo) inventory of existing behavior; it documents observable behavior
  only and proposes no changes.
- One observed naming deviation (the "Deleted" action queries non-deleted submissions) is recorded
  factually in the Assumptions section rather than corrected.
- Items marked incomplete require spec updates before `/speckit-clarify` or `/speckit-plan`.
- Review 2026-10-30: FR-021 corrected — the combined form carries anti-forgery tokens but the create/edit saves do not verify them (spec 021; tagged [SUSPECTED-DEFECT]). User Story 3 scenario 4, SC-006, and the Assumptions now state that restore is reachable only by direct address because the deleted-submissions page lists active submissions (spec 020).
- Verification 2026-10-30: Checked against the running app (see `verification.md`). FR-003, US1, SC-001, Key Entities and Assumptions: intake assigns only the laboratory number (year of saving), no stem number (REF-018). FR-006 and Edge Cases: patient link and invasive value can never fail; messages quoted; both "Anderer …" locations need the free text. FR-007: the server accepts initials the browser rejects (DATA-030). FR-011, SC-003, Edge Cases: a posted duplicate-resolution choice skips the third-patient block (DATA-031). FR-013, US3.1: postal codes and sender laboratory number are hidden, search-only columns; "Erweiterte Suche" described. FR-021: no field list on create/edit (SEC-013); missing token confirmed (SEC-005). New Edge Cases: missing/unknown submission number ends in HTTP 500 (UX-010, DATA-008); editing a deleted submission restores it (DATA-007).
