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
   They are configured (version-pinned) in `.mcp.json`. If they are not loaded, load them with
   ToolSearch; if the server is not connected, stop and tell the owner to approve the project MCP
   server and restart the session. If the first navigation reports a missing browser, install it
   with the version from `.mcp.json` and retry:

   ```powershell
   npx -y @playwright/mcp@<version> install-browser chrome-for-testing
   ```

   Do not fall back to Claude in Chrome or to Playwright scripts outside the MCP server.
3. **Accounts**: `Labor1` (all roles), `Standard1` (Standardbenutzer), `Admin1` (Administrator),
   `Rki1` (RKI), password `123456`. Use `Standard1` for ordinary laboratory flows so role gaps show
   up; use the others only for role-specific checks.
4. **Data**: the catalog is disposable and reseeded on every host start. Create your own records
   with the prefix `SV-NNN-` (e.g. sender name `SV-001-Alpha`) so they are recognizable in lists and
   exports. Soft deletes, merges and uploads are fine. Seeded ids 1 exist for every entity. You may
   change a seeded record through the UI when no own record can show the behavior (e.g. a sender
   with submissions), but restore it afterwards and list the change in the report. Data is changed
   only through the UI; the SQL access below is read-only. Behavior that needs data the UI cannot
   create (e.g. an invalid legacy record) goes to the unit-test path.

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

Read `specs/NNN-*/spec.md` and `checklists/requirements.md`, and search `specs/BACKLOG.md` for
entries whose subject this spec covers, even when another spec is their source. Build a
verification plan with one row per FR. Attach every acceptance scenario and edge case to the FR it
exercises (they become the row's IDs, e.g. `FR-009 · US2.2 · EC4`); give a scenario or edge case
its own row only when no FR covers it. Add a row for every `[SUSPECTED-DEFECT]`, every related
backlog entry, and every success criterion or assumption that claims something no FR states
(e.g. "the count is shown"); skip the others. Classify each row:

- **UI**: observable in the browser.
- **DB**: observable only in data; query the catalog with
  `sqlcmd -S "(LocalDB)\MSSQLLocalDB" -d HaemophilusWeb_Screenshots -Q "..."` (read-only).
- **File**: observable in a downloaded export or generated report; download and inspect it.
- **Test**: domain logic, validation rules, export column layouts, duplicate/anonymization rules.
  Verified by mapping the FR to a unit test in `HaemophilusWebTests/` (step 2b), not in the browser.
  Prefer this path for 006, 007, 012, the validator FRs of 001–005/016/017, and the column
  definitions of 011/013/023. A validator FR may be checked both ways; record both.
- **Not verifiable here**: external service, missing fixture (e.g. an RKI match file), timing.
  Say why and what would make it verifiable.

Work data-creating stories before data-reading ones. Keep the plan in your head or the scratchpad,
not in the repo.

### 2. Execute in the browser

Log in via `/Account/Login` with the account the item needs. For each item: navigate, act, observe
the exact strings, validation messages, redirects, list contents and downloads. Take a screenshot of
each state that proves a verdict (`browser_take_screenshot` with a filename like
`FR-012-validation.png`); the MCP server writes them to `.playwright-mcp/`. Move them to
`specs/NNN-*/verification/` at the end. For files, click the export button; the MCP server saves
the download in `.playwright-mcp/` under the suggested name. Move it to `specs/NNN-*/verification/`
too and inspect it with the shell (an `.xlsx` is a zip: read `xl/sharedStrings.xml` and
`xl/worksheets/sheet1.xml`).

Practical notes for this application:

- Batch a flow (fill, submit, read the result) into one `browser_run_code_unsafe` call that
  returns the observed strings as JSON; use `browser_snapshot` only to find selectors. Stay on
  `http://localhost:9899`. `page.request.get/post` (with the form's `__RequestVerificationToken`)
  is the way to check status codes and server-side validation that the browser would block.
- Forms with `preventSiteNavigationWithPendingChanges` raise a `beforeunload` dialog when you
  leave them with changes. It interrupts the running call; answer it with `browser_handle_dialog`
  and check the page state before continuing.
- The date pickers open over the submit button. Set date fields with `page.evaluate` (value in
  `dd.MM.yyyy`) and click the button via `page.evaluate` as well.
- A signed-in account without the required role is redirected to `/Account/Login?ReturnUrl=…`
  (HTTP 302), not shown a 403 page. Missing views and unhandled errors show as HTTP 500.
- With the `Beta` app setting active, every page shows the banner "Achtung: Änderungen werden
  nicht zentral gespeichert!"; it is not part of the spec under test.

Record for each item: spec statement (quote the ID), what the app showed, verdict:

| Verdict | Meaning |
| --- | --- |
| `Match` | App behaves as the spec says. |
| `Spec deviation` | Spec text is wrong or incomplete about current behavior. Fix the spec. |
| `Defect confirmed` | A `[SUSPECTED-DEFECT]` of this spec, or an existing backlog entry from another spec, reproduced on the system. |
| `New defect` | Behavior that looks wrong and is in neither the spec nor the backlog. Add tag and backlog entry. |
| `Not verifiable` | With the reason and what would make it verifiable. |

### 2b. Map to unit tests and fill gaps

For every item classified **Test** (and for UI items where a test already exists):

1. Search `HaemophilusWebTests/` for a test that asserts the FR (grep for the German string, the
   rule number, the validator or method under test). Record it as evidence:
   `Match, test-asserted: Validators/SenderValidatorTests.PostalCodeMustHaveFiveDigits`. Read the
   assertion; a test that merely calls the method is not evidence.
2. If no test exists and the verdict is `Match` or `Spec deviation` (after the fix), write one:
   a `[Test]` or `[TestCase]` in the mirror folder of the source (`Validators/`, `Domain/`,
   `Services/`, `Tools/`, `Controllers/`), named after the behavior, with the spec ID in a comment
   (`// specs/001 FR-004`). Use the existing stack (NUnit 4, FluentAssertions, NSubstitute,
   TestDataGenerator via `MockData`; there is no Moq) and keep it deterministic. Extend an existing
   fixture before creating a new file; a new file must also be added to
   `HaemophilusWebTests.csproj` (old-style project, explicit `Compile` items).
   `ApplicationDbContextMock` and `InMemoryDbSet<T>(bool)` share one static store per entity type,
   and every new instance clears it, so tests that depend on specific records can break other
   fixtures. Give such tests their own sets (`new InMemoryDbSet<T>(new HashSet<T>())` with a
   `FindFunction`) in a small `IApplicationDbContext` stub or an NSubstitute substitute.
   DataAnnotations rules are tested with `Validator.TryValidateObject`; assert member names, not
   message texts (they depend on the culture).
3. Never write a test for `Defect confirmed` or `New defect`: a test would cement the defect. The
   backlog entry is the record.
4. Run the affected fixtures before committing (PowerShell, from the repository root):

   ```powershell
   msbuild HaemophilusWebTests\HaemophilusWebTests.csproj /p:Configuration=Debug /m /verbosity:minimal
   vstest.console.exe HaemophilusWebTests\bin\Debug\HaemophilusWebTests.dll /Tests:SenderValidatorTests
   ```

   Then run the full suite once with a results file, so a failure can be named:

   ```powershell
   vstest.console.exe HaemophilusWebTests\bin\Debug\HaemophilusWebTests.dll /logger:trx /ResultsDirectory:<scratchpad>\TestResults
   ```

   A red test of yours means your reading of the code or of the app is wrong; investigate, do not
   adjust the assertion to pass. A red existing test that passes on a rerun is flaky: name it in
   the report and in `TEST-002`. Report the results (counts) in `verification.md`. The console
   output follows the Windows display language (e.g. "Bestanden", "Nicht bestanden").

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

By primary path: Browser n · File n · DB n · Unit test (existing) n · Unit test (added) n ·
Not verifiable n (adds up to the row count). Also test-asserted: n rows with a browser, file or DB
verdict. Test run: `vstest.console.exe … /Tests:<fixtures>` → n passed, 0 failed; full suite →
n passed, n failed.

## Results

| ID | Spec says | App/test shows | Verdict | Path | Evidence |
| --- | --- | --- | --- | --- | --- |
| FR-003 · US1.1 | … | … | Match | Browser | `verification/US1-1.png` |
| FR-004 · EC1 | … | … | Match | Browser | `verification/FR-004.png`; also test-asserted: `Models/SenderTests.Validate_WithoutName_FailsOnName` |
| FR-012 | … | … | Match | Unit test (added) | `Controllers/SenderControllerTests.DeleteConfirmed_…` |

## Unit tests added

- `HaemophilusWebTests/Validators/SenderValidatorTests.cs`: `Phone1IsRequired` (FR-009)

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

The ID column names the FR first, then the scenarios (`US2.2`) and edge cases (`EC1` = first
bullet under Edge Cases) the row covers. Path is the one evidence path that decided the verdict;
further evidence goes into the Evidence column. Screenshots and downloads stay local
(`specs/*/verification/` is git-ignored); the report references them by name so the owner can open
them from the working tree.

### 4. Correct the spec and the backlog

- `Spec deviation`: edit `spec.md` to describe the observed behavior, in the spec's as-is voice. Do
  not add `[NEEDS CLARIFICATION]`. Append to `checklists/requirements.md`:
  `- Verification YYYY-MM-DD: <what changed and why>`.
- `Defect confirmed`: in `specs/BACKLOG.md`, append `Confirmed on system YYYY-MM-DD (NNN
  verification).` to the entry's text. Do not change its type or "Offen". If the entry came from
  another spec, also add `NNN <section>` to its source and a `[SUSPECTED-DEFECT]` note to this spec
  that cross-references the source spec and the backlog ID. For a `DEC-*` entry, only record what
  was observed; never take the decision.
- `New defect`: add a `[SUSPECTED-DEFECT]` observation to the spec where it belongs (edge case or
  FR note) and a new backlog entry with the next free ID of the fitting prefix, type, source and
  "Offen" if a decision is needed. Never create or resolve `DEC-*` items; if a finding needs an
  owner decision, write it under "Open questions" and as a `DEC-*` candidate in the report only.
- New refactoring observation (dead or unreachable code, leftovers, scaffolding): verdict
  `Spec deviation`. Describe it in the spec with a `[POTENTIAL-REFACTORING]` tag and extend the
  `REF-*` entry with the same subject, or add a new one with the next free ID.

### 5. Write the Gherkin feature

`SystemTests/Features/NNN-<spec-slug>.feature`, English Gherkin keywords, UI strings in German
exactly as observed, in double quotes. No step definitions. Phrase steps with the shared step
vocabulary in `SystemTests/Features/README.md`; when a scenario needs a step that is not listed,
add it there in the same commit. Shape:

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
observed. Add further scenarios only for UI behavior (navigation, page flow, messages, role
redirects) with verdict `Match` or `Spec deviation` (after the fix) that no unit test covers; rules
already asserted by a unit test (validation, export columns, interpretation) get no scenario.
Never write scenarios for defects; defects belong in the backlog, not in a regression suite that
would cement them. Aim for the smoke scenario plus at most five more. Tag scenarios with the US and
FR IDs they exercise.

### 6. Commit and report

One commit on the current branch:

```text
Verify spec NNN against running app

<one line per: deviations fixed, defects confirmed/new, feature file>
```

Include `spec.md`, `checklists/requirements.md`, `verification.md`, `specs/BACKLOG.md`, the
`.feature` file, `SystemTests/Features/README.md` if you added steps, and any test files (plus
`HaemophilusWebTests.csproj` for new files) under `HaemophilusWebTests/`. Exclude
`specs/*/verification/`, `.playwright-mcp/`, `host.*`, `Web.config`, and build output.

Then report to the owner in German: the summary table, the deviations you fixed, confirmed and new
defects with backlog IDs, what was not verifiable and why, open questions, and the suggested next
spec number. Keep the host running.
