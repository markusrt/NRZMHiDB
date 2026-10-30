<!--
Sync Impact Report
==================
Version change: 1.1.0 → 1.2.0
Bump rationale: MINOR — added a new "Specification Structure" section recording the five
decisions (D1–D5) that shaped the retrospective as-is specifications 001–025, plus the rule that
behavior changes start from `specs/BACKLOG.md`. The Governance "Runtime guidance" pointer now
names `AGENTS.md`/`CLAUDE.md` instead of the retired Copilot instructions. No principle was
removed or redefined.

Amendment history:
  - 1.0.0 (2026-08-26): Initial ratification from the legacy codebase. Established Principles
    I–V and the Technology/Workflow/Governance sections (prior file was an unfilled template).
  - 1.1.0 (2026-08-26): Added "Known Architectural Constraints (Legacy Debt)".
  - 1.2.0 (2026-10-30): Added "Specification Structure" (D1–D5 carried over from the retired
    `feature-prompts.md`; resolves backlog item DEC-012). Updated Governance runtime guidance.

Principles (unchanged since 1.0.0):
  - I. Patient Data Integrity & Domain Validation (NON-NEGOTIABLE)
  - II. Test-First Discipline
  - III. Layered Architecture & Separation of Concerns
  - IV. Data Privacy, Security & Least Privilege
  - V. Explicit, Reversible Schema Evolution

Added sections:
  - Known Architectural Constraints (Legacy Debt)  [v1.1.0]
  - Specification Structure  [v1.2.0]

Removed sections: None

Deferred / follow-up TODOs:
  - RATIFICATION_DATE set to first concrete adoption date (2026-08-26). If an earlier
    informal adoption date is known, amend as a PATCH and update this report.

Template propagation check:
  - .specify/templates/plan-template.md — Constitution Check gates still align (no change required)
  - .specify/templates/spec-template.md — no constitution-specific tokens
  - .specify/templates/tasks-template.md — no constitution-specific tokens
  - AGENTS.md / CLAUDE.md — created alongside v1.2.0; they restate the spec conventions and
    the backlog rule and point back to this document
-->

# NRZMHi Database Constitution
<!-- Project: HaemophilusWeb — the web application for the German National Reference Center
     for Meningococci and Haemophilus influenzae (NRZMHi) laboratory database. -->

## Core Principles

### I. Patient Data Integrity & Domain Validation (NON-NEGOTIABLE)

Laboratory and patient records MUST never be silently corrupted, truncated, or lost. Every
write path that accepts external input MUST enforce domain rules before persistence:

- All user-supplied models MUST be validated through the FluentValidation validators in
  `Validators/` before an entity is saved; controllers MUST reject invalid input rather than
  coerce it.
- Data-losing operations (deletes, merges, bulk edits, importer runs) MUST be explicit,
  auditable, and recoverable from backup or migration history.
- Enumerations, reference values, and clinical breakpoints MUST be treated as authoritative
  domain data; changes to them MUST go through code and review, not ad-hoc database edits.

Rationale: This system holds identifiable patient and isolate data for a national reference
center. Incorrect or lost data has direct clinical and epidemiological consequences, so
validation is a non-negotiable gate rather than a convenience.

### II. Test-First Discipline

New behavior MUST be covered by automated tests, and the test project structure MUST continue
to mirror the source it verifies.

- Bug fixes MUST add a failing test that reproduces the defect before the fix is applied.
- New controllers, services, validators, view models, and mappings MUST have corresponding
  tests under the matching folder in `HaemophilusWebTests/` (Controllers, Services,
  Validators, ViewModels, Automapper, Domain, Models).
- Tests MUST use the established stack (NUnit, FluentAssertions, Moq/Castle, Faker) and MUST
  be deterministic — no reliance on wall-clock time, network, or a live database unless
  explicitly isolated as a system/integration test.

Rationale: The mirrored test suite is the primary safety net for a legacy code base under
active modernization; test-first keeps refactoring safe and regressions visible.

### III. Layered Architecture & Separation of Concerns

The MVC layering MUST be respected so that logic stays testable and replaceable.

- Controllers MUST stay thin: orchestrate, map, and delegate; business logic MUST live in
  `Services/`, `Domain/`, or validators.
- Mapping between entities and view models MUST go through AutoMapper profiles in
  `Automapper/`; controllers and views MUST NOT hand-roll cross-layer mapping.
- Views MUST render `ViewModels/`, not Entity Framework entities directly, to avoid leaking
  persistence concerns into the presentation layer.

Rationale: Clear layer boundaries keep the domain independently testable and make the eventual
platform modernization (e.g. off .NET Framework) tractable rather than a rewrite.

### IV. Data Privacy, Security & Least Privilege

Access to patient and laboratory data MUST follow least-privilege principles at every layer.

- Every non-public route MUST be protected by OWIN authentication and appropriate
  authorization; new endpoints default to protected and are opened only deliberately.
- Analytics and reporting access MUST use dedicated read-only credentials (e.g. the
  `NRZMHiDBReader` database user), never application or administrative accounts.
- Secrets, connection strings, and credentials MUST NOT be committed to the repository; they
  MUST be supplied through configuration transforms or environment-specific config.

Rationale: The application processes sensitive personal health data; a privacy or access
failure is both an ethical and a legal (GDPR) breach, so security is a first-class principle.

### V. Explicit, Reversible Schema Evolution

Database schema changes MUST flow through Entity Framework Code-First migrations and remain
traceable.

- Every schema change MUST be a reviewed migration in `Migrations/`; no out-of-band manual
  schema edits in shared environments.
- Migrations SHOULD be reversible (`Down` implemented) or MUST document why a rollback is not
  possible before merge.
- The entity documentation (`Database/Readme.md` and the entity diagram) MUST be kept in sync
  when core laboratory entities or relationships change.

Rationale: A single, ordered migration history is the source of truth for the schema across
environments and enables safe, auditable evolution of clinical data structures.

## Technology & Platform Constraints

- Runtime baseline: .NET Framework 4.8, ASP.NET MVC 5, and Entity Framework 6 (Code-First).
- Supporting stack: OWIN authentication, AutoMapper, FluentValidation, and NLog for logging.
- Logging MUST go through NLog; failures affecting data integrity or authentication MUST be
  logged with enough context to investigate without exposing patient identifiers in plain logs.
- Dependency upgrades and any move away from .NET Framework MUST be incremental and
  test-guarded; the mirrored test suite MUST stay green across each step.

## Development Workflow & Quality Gates

- All changes MUST land via pull request; a PR MUST NOT merge while the solution fails to build
  or while tests are red.
- PR descriptions MUST stay strictly technical and MUST NOT restate medical interpretation or
  patient-facing phrasing from domain data.
- Reviews MUST verify: input validation is present, tests cover new behavior, layering is
  respected, and any schema change ships as a reviewed migration.
- Complexity that deviates from these principles MUST be justified in the PR; unjustified
  complexity is grounds to request changes.

## Known Architectural Constraints (Legacy Debt)

This constitution was derived from a legacy codebase that carries known architectural debt. The
following constraints are documented so new work is measured against them: changes MUST NOT
deepen these patterns, and SHOULD reduce them when a change already touches the affected area.
Deviations MUST be justified in the PR.

- **Parallel Meningococci and Haemophilus domains.** The domain is duplicated across two
  near-parallel model/controller families — the unprefixed types (e.g. `Sending`, `Patient`,
  `Isolate`) are Haemophilus (which existed first) and the `Meningo*` types (e.g.
  `MeningoSending`, `MeningoPatient`, `MeningoIsolate`) are Meningococci. The two behave
  differently and were kept separate under time pressure rather than unified behind a shared
  abstraction. New work MUST NOT add a third parallel copy; where practical it SHOULD extract or
  reuse shared concepts (e.g. `Sender`, already shared across both databases) rather than
  duplicate them again.
- **God-controllers with weak dependency injection.** Most business logic currently lives in MVC
  controllers, and wiring to the few repositories and services is done manually because the
  application lacks a dependency-injection container. New logic SHOULD move toward `Services/`
  and `Domain/` (per Principle III) and SHOULD be constructor-injected so it can be tested in
  isolation, instead of adding more responsibility to controllers.

Rationale: Naming and documenting this debt keeps it visible and prevents it from silently
growing; it also gives reviewers an explicit basis to push back on changes that entrench it.

## Specification Structure

The feature specifications under `specs/` were written retrospectively as an as-is inventory of
the legacy application (001–025, branch `speckit-init`). The decisions below shaped that
inventory and MUST be applied to new specifications so the set stays navigable.

- **D1 — One specification per capability and pathogen.** Where Meningococci and Haemophilus
  code paths differ (see Legacy Debt), each capability MUST be specified separately per pathogen.
  Each spec MUST name the concepts it shares with its counterpart (e.g. `Sender`, the export
  mechanism, docx templates, merge logic) and reference the owning spec instead of restating them.
- **D2 — As-is specifications describe the code, not the intent.** An inventory spec MUST
  record the behavior of the code on its branch, including observed defects, which are tagged
  `[SUSPECTED-DEFECT]` or `[POTENTIAL-REFACTORING]` and otherwise described neutrally. It MUST
  NOT contain `[NEEDS CLARIFICATION]` markers or silently "correct" observed behavior. Work in
  flight on other branches is documented as-is and re-inventoried on that branch later.
- **D3 — Exports: one generic specification.** The export mechanism is specified once (011)
  with its targets enumerated. A new export target extends that enumeration; it gets its own
  spec only when its mechanism differs (as 013 sender export and 023 EpiScanGIS do).
- **D4 — Reporting per pathogen, interpretation separately.** Preliminary and final reports
  are specified together per pathogen (008, 009). The interpretation engines have their own spec
  per pathogen (006 if/else tree, 007 JSON rule set) because the mechanisms differ.
- **D5 — Report templates have their own specification (010)**, because the docx mechanism
  may be replaced independently of the reporting flow.
- **Changes start from the backlog.** Every tagged observation is consolidated in
  `specs/BACKLOG.md`. A change to current behavior MUST reference a backlog ID and MUST have
  either its own feature specification (`/speckit-specify`) or a recorded owner decision before
  implementation. Inventory specs are not rewritten to describe desired behavior; the change
  spec supersedes the affected parts and says so.

Rationale: The inventory exists to make a legacy system reviewable. Mixing "is" and "should" in
one document, or cutting specs along inconsistent lines, would make both the review and the
later change specs unreliable.

## Governance

This constitution supersedes ad-hoc conventions for the NRZMHi Database project. When guidance
here conflicts with habit or convenience, this document wins.

- **Amendments**: Proposed via pull request that edits this file, including an updated Sync
  Impact Report and a version bump. At least one maintainer review is required to adopt.
- **Versioning policy**: Semantic versioning of the constitution itself —
  - MAJOR: backward-incompatible governance changes or principle removals/redefinitions.
  - MINOR: a new principle or section, or materially expanded guidance.
  - PATCH: clarifications, wording, and non-semantic refinements.
- **Compliance review**: Every PR review MUST confirm the change complies with these
  principles. Deviations MUST be documented and justified, or the change MUST be revised.
- **Runtime guidance**: Use `AGENTS.md` (vendor-neutral agent and contributor guide), `CLAUDE.md`
  (Claude Code specifics) and the Spec Kit templates under `.specify/templates/` for day-to-day
  development and planning guidance consistent with this constitution.

**Version**: 1.2.0 | **Ratified**: 2026-08-26 | **Last Amended**: 2026-10-30
