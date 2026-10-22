# Specification Quality Checklist: Duplicate-patient anonymization in exports

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

- Validation iteration 1: 15 of 16 items passed. "Requirements are testable and unambiguous" failed because one edge case claimed that PubMLST maps unlisted sampling locations to "other", which cannot occur under the PubMLST selection (blood, liquor, other-invasive only); the statement was reduced to the observable free-text comparison. The mode overview table was also reworded so that the identifier column names exactly the three removed values.
- Validation iteration 2: all 16 items pass.
- Because the feature under inventory shapes files for external recipients, the spec names observable output artifacts: recipient column names (`initials`, `date_of_birth`, `postal_code`, `duplicate_group`), the label format `Group <n>`, and German UI labels and hints. Class and method names appear nowhere in requirements or success criteria; internal mechanics (date-difference month component, text ordering of stem numbers, key concatenation, controller wiring) are mentioned only in tagged as-is observations so reviewers can trace them.
- Generic export behavior (selection filters, column sets, file names, roles) is referenced from spec 011 (B1) and not repeated. Consistent with B1: the PubMLST "unbereinigt" hint promises more than the code does (only duplicate handling differs), and the Meningococci RKI export accepts but ignores the option.
- Written non-interactively; no clarification questions were asked. Items for reviewer attention (recorded under Assumptions): the privacy intent is inferred from code, not from data-protection documentation; the effective "less than one month" dropping window (versus the documented six-month intent) is derived from the date-difference calculation and was not executed against real data; stem numbers are assumed to follow receiving order; recipients of unadjusted files are assumed not to need same-patient linkage.
- As-is observations tagged in the spec: 4 [SUSPECTED-DEFECT], 6 [POTENTIAL-REFACTORING] (two of them restate observations from spec 011).
