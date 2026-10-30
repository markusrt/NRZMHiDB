# Specification Quality Checklist: County reference data management

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

- Validation iteration 1: all 16 quality items pass. The GV100AD character positions and the record type "40", the
  five fill words, the English labels of the generated pages ("Index", "Create", "Edit", "Details", "Delete", "County",
  "CountyNumber", "Name", "ValidSince", "Create New", "Back to List", "Are you sure you want to delete this?"), the
  upload texts ("GV100AD acs-Datei hochladen", "ACS-Datei:", "Hochladen", "Bitte geben sie eine Datei an.",
  "Fehler:"), and the export column names (`kreis_nr`, `bundesland`, `landkreis`, `bundeslandName`, "Landkreis",
  "Bundesland") are named because they are the observable contract of the file, the pages, and the exports; the
  three-county test file of the importer tests and the name pairs of the matching tests are used as reference data.
  "Anti-forgery token", "temporary folder", "application log", "generic error page", "storage order", and "soft
  delete" appear only where tied to Constitution Principles I, II, III, and IV and the "Known Architectural Constraints
  (Legacy Debt)", or where they are the established cross-spec vocabulary (specs 011, 017, 020, 021, 024).
- Consistency with existing specs: spec 011 (Edge Cases: case, comma suffix, five fill words; earliest validity wins,
  tagged [POTENTIAL-REFACTORING] there and referenced, not re-tagged; FR-014 state number = first two digits; warning
  in the Meningococci exports only) — no difference found; this spec adds the exact order and the literal (substring,
  case-sensitive) application of the fill words, the comma-in-first-position exception, the absent/absent equality,
  and the tie behavior. Spec 021 (access matrix row "Counties (generated pages, file upload)": Standard User only, not
  linked, forms verify the anti-forgery token, the upload does not and accepts any method; User Story 4.5 "no entry
  … for counties"; generated-pages observation) — confirmed from the controller, the views, and the navigation; no
  difference found. Spec 022 (county text originates from the postal-code lookup, e.g. "Landkreis Forchheim";
  "Unbekannt" finds no county) — consistent. Spec 020 — counties are hard-deleted, not soft-deleted; stated as such.
  No edit was made to other specs.
- Run non-interactively; no clarification questions were asked and no running system or real GV100AD file was
  available. Points for review (recorded under Assumptions):
  - The change log has no county entry; "RKI Export Modul" (2015-02-13) is assumed to be the introducing release.
  - Storage order (list page, ties between equal validity dates) is assumed to be creation order; not verified.
  - Date display with time of day, German input format, and the validation message texts for "ValidSince" follow the
    culture setting and standard date handling; not observed in a browser.
  - Empty "Name"/"CountyNumber" stored as "no value" follows standard form binding; the resulting match for patients
    without county text is derived from the matching rule's own test (two absent names) and was not exercised.
  - "X, Stadt" next to "Landkreis X" normalizing to the same text is derived from the rules; concrete pairs in a given
    GV100AD edition were not checked.
  - All-or-nothing upload and the left-over temporary file are derived from the code structure; not exercised.
  - GV100AD layout facts are taken from the importer and its test data only.
- As-is observations tagged in the spec: 2 [SUSPECTED-DEFECT] (a city "X, Stadt" and a district "Landkreis X"
  normalize to the same text, so a district patient can receive the city's number; a record saved with an empty name
  matches every patient without county text and gives them its number without warning), 6 [POTENTIAL-REFACTORING]
  (uploads only add while the earliest validity wins, so a renumbered county keeps its old number and old editions can
  only be removed record by record; no validation of the county number — Constitution Principle I; the list is
  runtime data without review, audit, log, or recovery — Principle I; upload failures show untranslated technical
  text, are not logged, and leave the uploaded copy in the temporary folder; controller/view layering, matching rule in
  the record, derivation copied into three exports, no tests for the pages or the upload — Principles II and III and
  the legacy-debt constraints). The earliest-validity choice itself remains tagged in spec 011 and is referenced.
