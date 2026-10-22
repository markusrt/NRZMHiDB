# Specification Quality Checklist: Postal-code lookup (Geonames)

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

- Validation iteration 1: all 16 quality items pass. Field labels, placeholder values ("Unbekannt", "n.n."), and
  state short codes are quoted as user-visible values, consistent with specs 016–021. The external Geonames service
  and the `GeonamesAccount` setting are named because the backlog entry makes the external dependency and its setting
  part of the scope; the setting's value is not reproduced. "Plain HTTP", "anti-forgery token", and the
  page-group/service wording appear only in edge cases tied to Constitution Principle IV and the "Known
  Architectural Constraints (Legacy Debt)", as in spec 021.
- Consistency with spec 021: the suggestion and lookup requests admit only the Standard User role and verify no
  anti-forgery token; the country list is reachable by any signed-in account (spec 021, access matrix rows
  "Postal-code lookups" and "Country list helper"). Spec 001 FR-019 ("populate city/postal code from a geonames-based
  search") agrees and is detailed here. Relation to spec 011: the county text filled by the lookup (or "Unbekannt")
  is the free text that the RKI/LGA exports match against the stored county list.
- Run non-interactively; no clarification questions were asked and the external service was not called. Points for
  review (recorded under Assumptions):
  - The answer structure of the service and the equality of its state codes with the application's codes are taken
    from the code and the recorded test answers, not from live calls.
  - Number and order of suggestions are assumed to be the service's defaults (the application sends no limit).
  - Behavior on network failure, unexpected answers, missing `GeonamesAccount`, out-of-order answers, unescaped place
    names, unencoded parameters, and requesting the country list by address is derived from code; not exercised.
- As-is observations tagged in the spec: 4 [SUSPECTED-DEFECT], 9 [POTENTIAL-REFACTORING].
