# Specification Quality Checklist: Report templates (.docx)

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

- Validation iteration 1: all 16 quality items pass. Because the feature under inventory is itself a file-based template mechanism, the spec names observable artifacts (template folder names, `.docx` file names, the `PreliminaryReportMarker` setting and its default, placeholder names). Library names and versions (docxtemplater, PizZip, FileSaver) are confined to the Assumptions section for traceability and are not used in requirements or success criteria.
- Written non-interactively; no clarification questions were asked. Items for reviewer attention (recorded under Assumptions): rendering of missing/null values as `undefined` (documented library default, not reproduced); whether direct access to template files is covered by authentication (not established from code); file-system enumeration order assumed alphabetical on the server; deprecated-interface statement based on library documentation; reliance on authoring-time include content.
- The spec records that an empty template list shows only the `Auswählen...` placeholder, never the "Keine Vorlagen hinterlegt" label (tagged [SUSPECTED-DEFECT]). Review 2026-10-30: specs 008/009, which previously stated that the list "communicates that none are stored", were aligned with this finding.
- Verification 2026-11-08: FR-011, US2.5 and the image edge case now say that the filling step does not center the QR image and that it keeps the alignment of the template paragraph, which is centered in all 26 templates (the generated reports show it centered). The lock-file edge case now also records that folders named `*.docx` are offered. The failure edge case distinguishes unreadable files (no console output) from placeholder errors (console log). The missing-value edge case and its assumption record the reproduced `undefined` rendering and that no stored template showed it. New edge case for "≤--- µg/ml" on results without breakpoints (UX-015). The fax edge case covers `Fax - Teilbefund - Serotyp v4` and the Meningococci fax variants; the duplicate-script edge case adds the notification address and isolate id; the "Template Data Consumed" rows for susceptibility, typing, and comment/announcement name the templates and the nesting; the direct-access assumption records that the template files are served without sign-in.
