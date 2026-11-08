# Specification Quality Checklist: Data exports (generic)

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

- Validation iteration 1: 15 of 16 items passed. "Requirements are testable and unambiguous" failed because FR-004 and one edge case stated that "unset" enumerated values become empty cells, which is not what happens (a zero value with a label such as "n.d." is exported as that label). Both statements were corrected.
- Validation iteration 2: all 16 items pass.
- Because the feature under inventory produces files for external recipients, the spec names observable output artifacts: file-name prefixes, recipient column names (RKI, PubMLST), German UI labels, and value encodings. Class and method names appear nowhere in requirements or success criteria; internal mechanics are mentioned only in tagged as-is observations, so that reviewers can trace them.
- Written non-interactively; no clarification questions were asked. Items for reviewer attention (recorded under Assumptions): dates assumed date-only, so the end date is inclusive; the outcome of a refused request is assumed to be the login page; the Administrator refusal assumes single-role accounts; the overseas comparison is assumed case-insensitive with blank counties included; handling of the unnamed worksheet was not verified.
- Scope boundaries: duplicate-submission cleaning and patient-column removal are described only at a high level (detail in B2); sender exports are left to B3; the token-protected GIS export is excluded because the Export menu does not offer it.
- As-is observations tagged in the spec: 8 [SUSPECTED-DEFECT], 7 [POTENTIAL-REFACTORING].
- Verification 2026-11-08: Edge case on a missing start date corrected: an unreadable or emptied start date shows the binding message ("Eingabe erforderlich", "Start-Datum ist kein gültiger Wert für abc.") and replaces the submitted end date; only an address without start date shows no message (was: no error message in either case).
- Verification 2026-11-08: Edge cases completed: a missing end date appears as `00010101` in the file name; submissions without stem number come first; the LGA export leaves an unknown birth month empty.
- Verification 2026-11-08: US3.5 completed: PubMLST region "n.a." for the state "n.n.", serotype "Haemophilus Hemolyticus" for an exported H. haemolyticus isolate.
- Verification 2026-11-08: Worksheet name corrected in the edge case and the assumption: the files show "Table1" (was: no name, handling not verified). Assumption on refused requests replaced by the observed redirect to the sign-in page.
- Verification 2026-11-08: [SUSPECTED-DEFECT] notes added for observations sourced elsewhere: FR-011 (unmatched interpretation counts as found, 007, DATA-039), Edge Cases (empty PubMLST record shown as "PubMLST ID" 0, 005, DATA-036; material "Nicht angewachsen" exported, 005, DATA-038). Tagged observations now: 11 [SUSPECTED-DEFECT], 7 [POTENTIAL-REFACTORING].
