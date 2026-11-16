# Specification Quality Checklist: Meningococci patient + sending intake (MeningoPatientSending)

**Purpose**: Validate specification completeness and quality before proceeding to planning
**Created**: 2026-08-27
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

- Items marked incomplete require spec updates before `/speckit-clarify` or `/speckit-plan`.
- This is an *as-is* status-quo inventory; one suspected defect is recorded factually in Assumptions
  (the "Deleted" view queries non-deleted submissions) and is intentionally not proposed as a change.
- Review 2026-10-30: User Story 3 scenarios 3–4, FR-018, FR-019, SC-006, and the Assumptions corrected — no Meningococci delete-confirmation or deleted-submissions page exists, so "Eintrag löschen" ends on the error page and restore is reachable only by direct address (specs 015, 020; tagged [SUSPECTED-DEFECT]). FR-021 now lists the LGA export among the filtered exports (spec 011). FR-023 corrected — the create/edit saves do not verify the anti-forgery token (spec 021).
- Verification 2026-10-30: Checked against the running application (`verification.md`). FR-003, US1, SC-001, Key Entities and Assumptions corrected — intake assigns only the laboratory number of the year of saving, no stem number ([POTENTIAL-REFACTORING] REF-018). FR-006 and the sending edge case quote the messages; the patient link cannot fail and the sampling date is optional. FR-007 and the edge cases: birth date optional, "?" accepted, browser vs. server initials check ([SUSPECTED-DEFECT] DATA-030). FR-012, SC-003: message quoted, posted duplicate-resolution choice skips the check ([SUSPECTED-DEFECT] DATA-031). FR-015, US3.1: hidden search-only columns incl. patient number, "Erweiterte Suche", default sort and page size. FR-018, US3.3, Assumptions: "Eintrag löschen" and the deleted list end in HTTP 500 (DEC-007). FR-023: no field list on create/edit ([SUSPECTED-DEFECT] SEC-013). New edge cases "Missing or unknown submission number" (UX-010, DATA-008) and "Editing a deleted submission restores it" (DATA-007).
