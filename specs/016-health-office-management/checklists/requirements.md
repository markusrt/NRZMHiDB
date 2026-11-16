# Specification Quality Checklist: Health office management

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

- Validation iteration 1: all 16 quality items pass. Page titles, button labels, menu entries, and role names are quoted as user-visible labels, not implementation details, consistent with specs 008, 009, and 014. The RKI postal-code website is named as an external dependency because the feature cannot be described without it; it was not called. The fax email-link format (`0<digits>@fax`) is quoted because it is the user-visible link target. The controller/client wiring is mentioned only in one edge case because it relates to the constitution's "Known Architectural Constraints (Legacy Debt)".
- Run non-interactively; no clarification questions were asked. Points for review (recorded under Assumptions):
  - Letter-case sensitivity of the address match is assumed to follow the database's default comparison rules; not verified.
  - The report-page failure on a locally cleared telephone number and the "null" display of cleared email/fax are derived from the page script and standard form handling; not exercised.
  - The leading `0` in the fax email link is assumed to be an outside-line prefix for the institute's fax-to-email gateway; the code gives no reason.
  - The report page's behavior for an empty lookup answer (nothing shown, no visible error) was not observed in a browser.
  - The rationale for local edits and for matching by address is taken from the application's change history (2017-04-20, 2017-05-31), not from code comments.
  - Only the reading of RKI answers is covered by automated tests; lookup, matching, storing, and the health-office pages are not.
- Unlike specs 011 and 014, an Administrator is not refused here: the health-office functions check no role at all, so the single-role assumption does not change the outcome; Public Health accounts can reach them by direct address.
- As-is observations tagged in the spec: 5 [SUSPECTED-DEFECT], 4 [POTENTIAL-REFACTORING].
- Verification 2026-11-08: Edge Cases (second bullet) now describe the empty (`null`) lookup answer as observed: no message, the box stays empty only because the page script stops ("Cannot read properties of null (reading 'error')"), and the script's error branch never applies; Assumptions updated accordingly.
- Verification 2026-11-08: letter case of the address match observed instead of assumed: the verification catalog's collation `SQL_Latin1_General_CP1_CI_AS` ignores letter case and trailing spaces (Edge Cases, Assumptions).
- Verification 2026-11-08: cleared telephone, fax, and email are saved without a value (was: "as empty"); the report-page behavior for them was observed with the values injected in the browser, so "not exercised" was removed (Edge Cases, Assumptions).
- Verification 2026-11-08: markup in the report box can come only from the RKI website; local edits or creates containing markup are refused by request validation with HTTP 500 (Edge Cases, unescaped markup bullet).
- Verification 2026-11-08: the delete confirmation for a non-existent record ends in HTTP 500 ("Der Wert darf nicht NULL sein.", logged as an unhandled error) instead of "the generic error page".
- Verification 2026-11-08: coverage notes updated (Edge Cases last bullet, Assumptions): tests added in the 016 verification cover the report-page lookup, the "not found"/"bad request" answers of the health-office pages, and the format rules; the edit page's matching branch and the views stay untested.
