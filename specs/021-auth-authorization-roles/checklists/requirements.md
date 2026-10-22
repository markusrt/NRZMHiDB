# Specification Quality Checklist: Authentication & authorization roles

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

- Validation iteration 1: all 16 quality items pass. Page titles, menu entries, button labels, messages, and role
  names are quoted as user-visible labels, consistent with specs 016–020. Role identifiers (`Standardbenutzer`,
  `Administrator`, `RKI`) and the setting name `RegistrationToken` are named because they are the observable role and
  setting names; no secret value is reproduced. The "sign-in framework" is mentioned only where behavior is derived
  from its defaults, and page groups appear only in the Architecture edge case relating to the constitution's "Known
  Architectural Constraints (Legacy Debt)".
- Backlog question answered from code: accounts created in the application hold **exactly one role** — registration is
  the only role assignment and assigns the one chosen role; no page adds, changes, or removes roles or lists accounts.
  The storage allows several roles per account, so multi-role accounts could exist only through direct database
  changes. This confirms the assumption of specs 011, 013, 014, 017, 018, 019, and 020.
- Consolidated access matrix built over all page groups (24 rows). Functions without role check (sign-in only):
  health offices (spec 016), report pages incl. report-status marking and health-office lookup (specs 008, 009), the
  RKI match import (newly documented here), own password page, home/"Über", country-list helper. Without sign-in:
  sign-in/registration/external-login pages, the EpiScanGIS export (token = `RegistrationToken`), static files.
- New findings not recorded in earlier specs: the registration submission does not re-check the token; registration
  signs the browser in as the new account; no lockout or logging of sign-ins; refused requests land on the sign-in
  page; role seeding code is never called; RKI match import open to every signed-in account; Public Health can open
  report pages with patient data; the combined submission create/edit save does not verify the anti-forgery token
  (contrary to spec 002 FR-021 and spec 003 FR-023); internal helper functions reachable by address.
- Run non-interactively; no clarification questions were asked. Points for review (recorded under Assumptions):
  - The production database is assumed to contain the three roles (the application never creates them).
  - Refusal → sign-in page redirect, sign-in lifetime (14 days sliding), user-name character rule, and English
    framework messages are derived from framework defaults; not observed in a browser.
  - Static files (including report templates) being delivered without sign-in is derived from the configuration; not
    exercised (open point of spec 010).
  - Direct registration submission without token, the role-less account after a crafted role name, the missing-setting
    cases, and helper functions reachable by address are derived from code; not exercised.
  - Whether multi-role accounts exist in the production database cannot be determined from code.
- Inconsistencies with earlier specs, recorded in the spec and left unchanged there: spec 001 User Story 2 scenario 3
  (an administrator restores a sender), spec 002 FR-021 / spec 003 FR-023 (anti-forgery tokens on the submission form).
- As-is observations tagged in the spec: 12 [SUSPECTED-DEFECT], 8 [POTENTIAL-REFACTORING].
