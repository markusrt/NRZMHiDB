# Specification Quality Checklist: Merge patients (Meningococci)

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

- Validation iteration 1: all 16 quality items pass. Page titles, button labels, messages, and role names are quoted as user-visible labels, consistent with spec 014. The shared controller base (`PatientSendingControllerBase`) is named deliberately because the backlog entry asks for the shared C1-H/C1-M concept and because it relates to the constitution's "Known Architectural Constraints (Legacy Debt)".
- Per the backlog entry, the shared flow is not repeated; FR-003 delegates to spec 014's requirements, and each spec 014 finding is classified in Edge Cases as "applies the same", "differs", or "does not apply". A "Differences from the Haemophilus merge" table summarizes them.
- Run non-interactively; no clarification questions were asked. Points for review (recorded under Assumptions):
  - Meningococci soft-delete through the normal pages appears broken (no Meningococci deletion page, no "Gelöschte Einsendungen" entry); this contradicts spec 003 User Story 3, which was not changed. Not exercised at runtime.
  - The merge failure on soft-deleted submissions is derived from the schema and the assumption that every submission has an isolate; not exercised. FR-008 pins this current behavior.
  - Derivations inherited from spec 014 (first-open message, masked Patient 2 number, request method / anti-forgery handling) are not verified at runtime.
  - EpiScanGIS one-line-per-patient effect is derived from code; that export is not yet specified.
  - Administrator refusal assumes single-role accounts, as in specs 011 and 014.
  - No automated test covers the merge function.
- As-is observations tagged in the spec: 6 [SUSPECTED-DEFECT], 1 [POTENTIAL-REFACTORING].
- Review 2026-10-30: the Assumptions no longer describe spec 003 as contradicting this spec; spec 003 was aligned with the finding that no Meningococci delete-confirmation page exists.
