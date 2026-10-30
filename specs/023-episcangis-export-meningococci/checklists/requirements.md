# Specification Quality Checklist: EpiScanGIS export (Meningococci)

**Purpose**: Validate specification completeness and quality before proceeding to planning
**Created**: 2026-10-30
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

- Validation iteration 1: all 16 quality items pass. The file name, the `token` parameter name, the `text/csv`
  announcement, the date pattern `dd.MM.yyyy`, and the literal example line are named because they are the observable
  contract with the receiving system and are fixed by the automated test; the `RegistrationToken` and
  `GeonamesAccount` settings are named without values, as in specs 018, 021, and 022. "Anti-forgery token", "plain
  HTTP", "temporary file", and the page-group/service wording appear only in scenarios tied to Constitution
  Principle IV and the "Known Architectural Constraints (Legacy Debt)", as in specs 021 and 022.
- Consistency with existing specs: spec 021 (User Story 7, FR-012, access matrix row, SC-006: anonymous retrieval
  with token, "not found" otherwise, one secret shared with registration, token in the address — tagged there, not
  re-tagged here); spec 022 (User Story 4, FR-005, FR-011, SC-006: one lookup per case with "Postleitzahl" and
  "Wohnort", exact place-name match, case left out without notice — tagged there); spec 015 (one line per patient
  number after a merge); spec 020 (soft-deleted submissions excluded); spec 011 (no Export-menu entry). One
  **difference** from spec 011 is recorded explicitly: this export selects only the sampling locations "Blut" and
  "Liquor", not the full set of invasive locations used by the RKI, IRIS, and LGA exports (User Story 2.3,
  Selection summary). Specs 015 and 020 call this export "not yet specified"; this spec now covers it (no edit made
  to those specs).
- Run non-interactively; no clarification questions were asked and the external service was not called. Points for
  review (recorded under Assumptions):
  - The receiver's import format and schedule are known only through the produced file and a version-history note.
  - The ascending patient-number order of the lines is derived from reading order and practical container behavior,
    not a guarantee.
  - Behavior for a missing isolate, an empty or foreign postal code, a network failure of the external service
    (whole-run abort, left-over empty temporary file), a negative age, and unescaped semicolons is derived from code;
    not exercised or tested.
  - Coordinate precision is assumed to be the service's own; the application neither rounds nor pads.
- As-is observations tagged in the spec: 4 [SUSPECTED-DEFECT] (sampling-location subset "Blut"/"Liquor" only, incl.
  exclusion of "Blut und Liquor"; asymmetric 2019-10-01 boundary; whole-run abort on a lookup failure with left-over
  temporary file; age `0` for unknown birth date), 7 [POTENTIAL-REFACTORING] (unescaped values / no header;
  longest-line heuristic with reading-order tie-break; one lookup per case before per-patient reduction; unbounded
  growing window of sequential lookups; no application-log trace of requests or token outcomes; temporary-file round
  trip; logic in the page group with manual wiring and test gaps).
