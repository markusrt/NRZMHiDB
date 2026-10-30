# Specification Quality Checklist: RKI match import (Haemophilus)

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

- Validation iteration 1: all 16 quality items pass. The six column headers (`InterneRef`, `Aktenzeichen`,
  `StatusName`, `klhi_nr`, `LaufendeNummer`, `Jahr`), the two literal feedback lines, the three export column names,
  the four German state words, and the literal failure message for an unknown state are named because they are the
  observable contract of the file, the page, and the export; the test table of the automated tests is used as the
  reference data set. "Anti-forgery token", "temporary file", "migration", and the page-group wording appear only in
  scenarios tied to Constitution Principles I, IV, and V and the "Known Architectural Constraints (Legacy Debt)", as in
  specs 021, 022, and 023.
- Consistency with existing specs: spec 021 (User Stories 4.6 and 7.5, access matrix row "RKI match import \"RKI
  Matching Daten\"": not linked, no role check, no anti-forgery check, open to every signed-in account including
  Public Health, messages "File Uploaded Successfully!!" / "File upload failed: `<message>`" — tagged there, not
  re-tagged here); spec 011 (User Story 1 scenario 5: Haemophilus laboratory export carries "the RKI match
  reference, file number, and status"); spec 004 (stem and laboratory numbers and their uniqueness); spec 020
  (soft-deleted submissions); spec 002 (every submission owns an isolate). No difference from spec 021 or spec 011
  was found; this spec adds the "kein Match" default for submissions without record and the position of the three
  export columns (after "Bemerkung (Isolat)"), which spec 011 does not state. No edit was made to other specs.
- Run non-interactively; no clarification questions were asked and no running system or real RKI file was available.
  Points for review (recorded under Assumptions):
  - Header-based column recognition, quoted values, failure on non-numeric cells, and header case-sensitivity are
    derived from the reading library, not fixed by tests.
  - The further accepted status forms (English names, numbers 0–3, word prefixes) and the exact message for an
    unknown status word are derived from the shared enumeration helper.
  - No-file upload (technical "object reference" message), request-size limit (platform default), ambiguous lookup,
    and matching of soft-deleted submissions are derived from code and framework behavior; not exercised.
  - The left-over temporary file is derived from the absence of a delete step.
  - The import is assumed to be the only writer and the Haemophilus laboratory export the only reader of RKI match
    records (search over pages and exports).
- As-is observations tagged in the spec: 3 [SUSPECTED-DEFECT] (upload without a chosen file ends in a technical
  "object reference" message; the uploaded file remains in the server's temporary folder; rows without stem number are
  skipped even when they carry a laboratory number), 8 [POTENTIAL-REFACTORING] (success line regardless of matched
  rows; lenient status parsing by the shared helper; soft-deleted submissions are matched and updated; a stored state
  can never be lowered or removed and no page shows or edits it; failures are not logged; no trace of an import at all
  (Constitution Principle I); the shared text-table separator is switched process-wide and left so; importer logic in
  one unit with manually created database access and test gaps incl. no test for the page).
