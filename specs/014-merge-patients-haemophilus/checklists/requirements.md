# Specification Quality Checklist: Merge patients (Haemophilus)

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

- Validation iteration 1: all 16 quality items pass. Page titles, button labels, messages, and role names are quoted as user-visible labels, not implementation details, consistent with specs 009, 011, and 013. The shared controller base (`PatientSendingControllerBase`) is named deliberately because the backlog entry asks for the shared C1-H/C1-M concept to be called out and because it relates to the constitution's "Known Architectural Constraints (Legacy Debt)".
- Run non-interactively; no clarification questions were asked. Points for review (recorded under Assumptions):
  - The "two different patient numbers" message on first opening of the request page is derived from request binding/validation, not observed.
  - The failure of a merge when the patient to be deleted has soft-deleted submissions is derived from the schema (cascading submission delete vs. non-cascading isolate link) and from the assumption that every submission has an isolate; not exercised. FR-011 pins this current behavior.
  - The latent mix-up of Patient 2's number in the confirmation data is assumed to be masked by re-display of submitted form values; not verified at runtime.
  - Administrator refusal assumes single-role accounts, as in spec 011.
  - No automated test covers the merge function, so no statement is confirmed by a test.
- As-is observations tagged in the spec: 6 [SUSPECTED-DEFECT], 2 [POTENTIAL-REFACTORING].
