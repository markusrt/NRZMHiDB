---
name: "spec-verify"
description: "Verify one as-is specification (specs/NNN-*) against the running HaemophilusWeb app in a browser, record deviations, correct the spec, update the backlog and write a Gherkin smoke feature."
argument-hint: "NNN (spec number, e.g. 001) [optional focus, e.g. 'only US1-US3']"
user-invocable: true
disable-model-invocation: false
---

## User Input

```text
$ARGUMENTS
```

The first token is the spec number `NNN`. Anything after it narrows the scope (e.g. a list of user
stories or FR IDs). Without a number, stop and ask for one.

## Purpose

The specs under `specs/` were written by reading code. This skill checks that transcription against
the real application: the spec says X, the app does Y. It does **not** judge whether the as-is
behavior is right; that is what the `DEC-*` entries in `specs/BACKLOG.md` are for. Never decide a
`DEC-*` item here.

Outcome per spec: `specs/NNN-*/verification.md`, corrections applied to `spec.md`, backlog updates,
screenshots, and `SystemTests/Features/NNN-<slug>.feature` with at least one `@smoke` scenario.

## Preconditions (check, do not assume)

1. **Host**: `ScreenshotTests/host.ready` exists. It lists the base URL (`http://localhost:9899`),
   the LocalDB catalog and the seeded accounts. If it is missing, start the host yourself in the
   background and wait for the file (up to 5 minutes):

   ```powershell
   dotnet test ScreenshotTests\ScreenshotTests.csproj --no-build --filter SpecVerificationHost
   ```

   If the build output is missing, build first (MSBuild, not `dotnet build`; see
   `ScreenshotTests/README.md`). Leave the host running at the end; other specs may follow. Only
   stop it (`New-Item ScreenshotTests\host.stop`) when the owner asks.
2. **Browser tools**: the Playwright MCP tools (`mcp__playwright__browser_*`) must be available.
   They are configured in `.mcp.json`. If they are not loaded, load them with ToolSearch; if the
   server is not connected, stop and tell the owner to approve the project MCP server and restart
   the session. Do not fall back to Claude in Chrome or to hand-written Playwright scripts.
3. **Accounts**: `Labor1` (all roles), `Standard1` (Standardbenutzer), `Admin1` (Administrator),
   `Rki1` (RKI), password `123456`. Use `Standard1` for ordinary laboratory flows so role gaps show
   up; use the others only for role-specific checks.
4. **Data**: the catalog is disposable and reseeded on every host start. Create your own records
   with the prefix `SV-NNN-` (e.g. sender name `SV-001-Alpha`) so they are recognizable in lists and
   exports. Soft deletes, merges and uploads are fine. Seeded ids 1 exist for every entity.

## Hard rules

- External services: Geonames postal-code lookups are allowed (the app uses the owner's account).
  Never trigger actions that call RKI, PubMLST, IRIS or EpiScanGIS endpoints (spec 019 matching,
  023 export run). File exports that are generated locally and downloaded are fine.
- Never copy secret values (`RegistrationToken`, `Iris*`, `GeonamesAccount`) into any file.
- Never paste real patient data. Use obviously synthetic initials, dates and postal codes.
- Do not change application code or `Web.config`. The host swaps the connection string itself.
- Do not edit other specs than `NNN` except for cross-reference fixes that `NNN` requires; record
  those in the verification report.

## Procedure

### 1. Read and plan

Read `specs/NNN-*/spec.md` and `checklists/requirements.md`. Build a verification plan with one
row per checkable item: every acceptance scenario, every FR with observable behavior, every edge
case, every `[SUSPECTED-DEFECT]`. Classify each item:

- **UI**: observable in the browser.
- **DB**: observable only in data; query the catalog with
  `sqlcmd -S "(LocalDB)\MSSQLLocalDB" -d HaemophilusWeb_Screenshots -Q "..."` (read-only).
- **File**: observable in a downloaded export or generated report; download and inspect it.
- **Not verifiable here**: external service, missing fixture (e.g. an RKI match file), timing, or
  behavior that only tests exercise. Say why.

Work data-creating stories before data-reading ones. Keep the plan in your head or the scratchpad,
not in the repo.

### 2. Execute in the browser

Log in via `/Account/Login` with the account the item needs. For each item: navigate, act, observe
the exact strings, validation messages, redirects, list contents and downloads. Take a screenshot of
each state that proves a verdict (`browser_take_screenshot` with a filename like
`FR-012-validation.png`); the MCP server writes them to `.playwright-mcp/`. Move them to
`specs/NNN-*/verification/` at the end. For files, save the download and inspect it with the shell.

Record for each item: spec statement (quote the ID), what the app showed, verdict:

| Verdict | Meaning |
| --- | --- |
| `Match` | App behaves as the spec says. |
| `Spec deviation` | Spec text is wrong or incomplete about current behavior. Fix the spec. |
| `Defect confirmed` | A tagged `[SUSPECTED-DEFECT]` reproduced on the system. |
| `New defect` | Behavior that looks wrong and is not in the spec. Add tag and backlog entry. |
| `Not verifiable` | With the reason and what would make it verifiable. |

### 3. Write `specs/NNN-*/verification.md`

```markdown
# Verification: <spec title>

**Spec**: NNN · **Date**: YYYY-MM-DD · **Build**: `<git short sha>` · **Host**: ScreenshotTests
SpecVerificationHost (`http://localhost:9899`, catalog `HaemophilusWeb_Screenshots`) ·
**Accounts used**: … · **Scope**: full | <narrowed scope>

## Summary

| Verdict | Count |
| --- | --- |
| Match | n |
| Spec deviation | n |
| Defect confirmed | n |
| New defect | n |
| Not verifiable | n |

## Results

| ID | Spec says | App shows | Verdict | Evidence |
| --- | --- | --- | --- | --- |
| US1.1 | … | … | Match | `verification/US1-1.png` |

## Spec corrections applied

- FR-004: … (was: …; now: …)

## Backlog updates

- DATA-012: confirmed on system 2026-…
- NEW UX-010: …

## Not verifiable here

- FR-020: needs an RKI match file …

## Open questions for the owner

- …
```

Screenshots stay local (`specs/*/verification/*.png` is git-ignored); the report references them by
name so the owner can open them from the working tree.

### 4. Correct the spec and the backlog

- `Spec deviation`: edit `spec.md` to describe the observed behavior, in the spec's as-is voice. Do
  not add `[NEEDS CLARIFICATION]`. Append to `checklists/requirements.md`:
  `- Verification YYYY-MM-DD: <what changed and why>`.
- `Defect confirmed`: in `specs/BACKLOG.md`, append `Confirmed on system YYYY-MM-DD (NNN
  verification).` to the entry's text. Do not change its type or "Offen".
- `New defect`: add a `[SUSPECTED-DEFECT]` observation to the spec where it belongs (edge case or
  FR note) and a new backlog entry with the next free ID of the fitting prefix, type, source and
  "Offen" if a decision is needed. Never create or resolve `DEC-*` items; if a finding needs an
  owner decision, write it under "Open questions" and as a `DEC-*` candidate in the report only.

### 5. Write the Gherkin feature

`SystemTests/Features/NNN-<spec-slug>.feature`, English Gherkin keywords, UI strings in German
exactly as observed, in double quotes. No step definitions. Shape:

```gherkin
@spec-001
Feature: Sender management
  As-is behavior of HaemophilusWeb, verified on <date> against build <sha>.
  Source: specs/001-sender-management/spec.md

  Background:
    Given I am signed in as "Standard1"

  @smoke @US1 @FR-001 @FR-003
  Scenario: Create a sender and find it in the list
    When I open "/Sender/Create"
    And I enter "SV-001-Alpha" into "Name"
    ...
    Then the sender list contains "SV-001-Alpha"
```

At least one `@smoke` scenario per spec covering the primary user story's happy path exactly as
observed. Add further scenarios only for items with verdict `Match` or `Spec deviation` (after the
fix), never for defects; defects belong in the backlog, not in a regression suite that would cement
them. Tag scenarios with the US and FR IDs they exercise.

### 6. Commit and report

One commit on the current branch:

```text
Verify spec NNN against running app

<one line per: deviations fixed, defects confirmed/new, feature file>
```

Include `spec.md`, `checklists/requirements.md`, `verification.md`, `specs/BACKLOG.md` and the
`.feature` file. Exclude screenshots, `.playwright-mcp/`, `host.*`, `Web.config`.

Then report to the owner in German: the summary table, the deviations you fixed, confirmed and new
defects with backlog IDs, what was not verifiable and why, open questions, and the suggested next
spec number. Keep the host running.
