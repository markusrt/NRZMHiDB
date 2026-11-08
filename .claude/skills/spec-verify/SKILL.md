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

1. **Host**: exactly one host runs and serves the app. `ScreenshotTests/host.ready` lists the base
   URL (`http://localhost:9899`), the LocalDB catalog and the seeded accounts, but it is no proof:
   it stays behind when the host process is killed. (A host refuses to start while port 9899 is in
   use and deletes `host.ready` when its IIS Express exits; hosts built before that change do
   neither.) Check all three:

   ```powershell
   $hosts = @(Get-CimInstance Win32_Process -Filter "Name='dotnet.exe'" | Where-Object { $_.CommandLine -match '\stest\s' -and $_.CommandLine -match 'SpecVerificationHost' })
   $iis = @(Get-CimInstance Win32_Process -Filter "Name='iisexpress.exe'" | Where-Object CommandLine -match '/port:9899')
   "Hosts: $($hosts.Count), IIS Express on 9899: $($iis.Count)"
   (Invoke-WebRequest http://localhost:9899/Account/Login -UseBasicParsing -TimeoutSec 30).StatusCode
   ```

   Expected: one host, one IIS Express, status 200. (The port listener belongs to `System` through
   http.sys, so `Get-NetTCPConnection` does not name the owner.) With two hosts (an older build),
   both reseed the same catalog and only one IIS Express serves the port: stop and ask the owner
   which one to stop. With a
   host but no IIS Express or no answer, the app is gone: stop and ask the owner to restart the host.
   If no host runs at all (stale or missing `host.ready`), start it yourself in the background and
   wait until `host.ready` is rewritten and the check above passes (up to 5 minutes):

   ```powershell
   dotnet test ScreenshotTests\ScreenshotTests.csproj --no-build --filter SpecVerificationHost
   ```

   If the build output is missing, build first (MSBuild, not `dotnet build`; see
   `ScreenshotTests/README.md`). Claude Code may end background shells under memory pressure, which
   kills a host started there without restoring `Web.config` (the next host start restores the
   backup); a host the owner started in an own terminal is not affected. Repeat the check when the
   app stops answering during the verification and, if it fails, stop and report instead of
   restarting. Leave the host running at the end; other specs may follow. Only stop it
   (`New-Item ScreenshotTests\host.stop`) when the owner asks.

   Sign in once as `Standard1` to confirm the app uses the seeded catalog. Any build that compiles
   `HaemophilusWeb` (the solution, the web project, or a project that references it without
   `/p:BuildProjectReferences=false`) regenerates `Web.config` from `Web.template.config`
   (`CustomWebConfigTransform`). The host checks every two seconds, re-applies its connection string and
   appends "Connection swap re-applied at …" to `host.ready`, but the app still recycles and runs on
   rebuilt binaries, so avoid such builds while it runs. If sign-in fails although `host.ready` exists
   (e.g. a host started before this check existed), stop and ask the owner whether to restart the host
   (reseeds, own records are lost).
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
   exports. Soft deletes, merges and uploads are fine. Seeded ids 1 exist for every entity. The host
   also seeds fixtures that later specs need and that the UI cannot create reliably in one session;
   `host.ready` lists them (e.g. sender `SV-Seed-DEC-002` with one active and one soft-deleted
   submission per database for 013 FR-015 / DEC-002, sender `SV-Seed-Positive` with MZ004/20, the
   one invasive Meningococci finding that the RKI, IRIS and LGA exports of 2020 include, for 003
   FR-021 and 011; clinical breakpoints "SV-Seed …" for the standard antibiotics of both databases,
   so the E-Test sections, reports and exports have data). Use them read-only. Records you create live only
   until the next host start; when another spec needs data that must survive a restart, propose a seed
   extension in the report instead of relying on your own records. You may
   change a seeded record through the UI when no own record can show the behavior (e.g. a sender
   with submissions), but restore it afterwards and list the change in the report. Data is changed
   only through the UI; the SQL access below is read-only. Behavior that needs data the UI cannot
   create (e.g. an invalid legacy record) goes to the unit-test path.

## Hard rules

- External services: Geonames postal-code lookups are allowed (the app uses the owner's account).
  Never trigger actions that call RKI, PubMLST, IRIS or EpiScanGIS endpoints (spec 019 matching,
  023 export run). File exports that are generated locally and downloaded are fine. Some pages call
  them without a click, so install this block in every browser context **before** the first
  navigation (also in contexts you create for role checks) and report the blocked URLs:

  ```js
  const ctx = page.context();
  await ctx.unrouteAll({ behavior: 'ignoreErrors' });   // the MCP browser context outlives earlier runs
  ctx.svBlocked = [];   // globals do not survive between run_code calls; the context does
  const external = /\/QueryHealthOffice|\/PubMlst\/NeisseriaIsolates|\/HealthOffices\/Edit\?.*postalCode=|\/MeningoIsolate\/PubMlstMatch/i;
  await ctx.route('**/*', r => {
    const req = r.request();
    const hit = external.test(req.url()) && (!/PubMlstMatch/i.test(req.url()) || req.method() === 'POST');
    if (hit) { ctx.svBlocked.push(req.method() + ' ' + req.url()); return r.abort(); }
    return r.continue();
  });
  ```

  To exercise what a page does with a lookup answer (e.g. the PubMLST button), register a more
  specific route afterwards that answers in the browser with synthetic data
  (`r.fulfill({ status: 200, contentType: 'application/json', body: JSON.stringify({...}) })`); the
  request then never reaches the server.

  Known triggers: both report pages (`/Report/Isolate/n`, `/MeningoReport/Isolate/n`) post
  `QueryHealthOffice` on load, which asks the RKI tool; the "PubMLST" button of the Meningococci
  isolate form; the health-office edit page with `?postalCode=`; the PubMLST matching submit. Routing
  does not cover `page.request.get/post`: never send those to the addresses above, not even for role
  checks (check the role on another address of the same controller). A new outbound call you find in
  the code goes into the pattern and into the report.
- Never copy secret values (`RegistrationToken`, `Iris*`, `GeonamesAccount`) into any file.
- Never paste real patient data. Use obviously synthetic initials, dates and postal codes.
- Do not change application code or `Web.config`. The host swaps the connection string itself and
  re-applies it when a build regenerates the file.
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
- **Code**: only readable in the code, with no observable effect and no sensible test (dead code,
  leftovers, a call whose result is discarded). Use it for `[POTENTIAL-REFACTORING]` rows only and
  quote file and member in the evidence; a behavior claim never rests on code reading alone.
- **Not verifiable here**: external service, missing fixture (e.g. an RKI match file), timing.
  Say why and what would make it verifiable.

Work data-creating stories before data-reading ones. Keep the plan in your head or the scratchpad,
not in the repo.

### 2. Execute in the browser

Log in via `/Account/Login` with the account the item needs. For each item: navigate, act, observe
the exact strings, validation messages, redirects, list contents and downloads. Take a screenshot of
each state that proves a verdict with `page.screenshot({ path: '.playwright-mcp/FR-012-validation.png' })`
inside `browser_run_code_unsafe`; `browser_take_screenshot` with a bare filename writes into the
repository root instead. Move the screenshots to `specs/NNN-*/verification/` at the end. For files,
click the export button; the MCP server saves every download in `.playwright-mcp/` on its own. When
you also call `download.saveAs(…)`, delete the automatic copy (same name with a hyphenated suffix)
so each export is kept once. Move the files to `specs/NNN-*/verification/` too and inspect them with
the shell (an `.xlsx` is a zip: read `xl/sharedStrings.xml` and `xl/worksheets/sheet1.xml`). Before
committing, `git status` must show no stray `.png` or export in the repository root.

Practical notes for this application:

- Batch a flow (fill, submit, read the result) into one `browser_run_code_unsafe` call that
  returns the observed strings as JSON; use `browser_snapshot` only to find selectors. Stay on
  `http://localhost:9899`. `page.request.get/post` (with the form's `__RequestVerificationToken`)
  is the way to check status codes and server-side validation that the browser would block.
  Such crafted requests verify what a user can reach by bypassing the browser (client-side
  validation, a hidden link, a direct address, a missing role). A request no form can produce
  (e.g. only the token without the record's fields) is out of scope: no row, no verdict; mention it
  under "Open questions" only when it changes or exposes data.
- Check role behavior in a separate browser context instead of signing out and in:
  `const ctx = await page.context().browser().newContext(); const p2 = await ctx.newPage();`, sign
  in there with the second account, and close the context afterwards.
  The anti-forgery token belongs to the signed-in account: post a form only from the context it was
  loaded in (another account's token ends in HTTP 500, which says nothing about roles). Some forms
  post a field twice (a hidden copy before the visible input, e.g. the Meningococci laboratory
  number); MVC binds the first value, so a crafted request must replace every occurrence
  (`URLSearchParams.delete` then `append`), and the duplicate itself may be a finding.
- Forms with `preventSiteNavigationWithPendingChanges` raise a `beforeunload` dialog when you
  leave them with changes. Before navigating away from a changed form, call
  `await page.evaluate(() => $(window).off('beforeunload'))`. If a dialog still appears, it interrupts
  the running call; answer it with `browser_handle_dialog` and check the page state before continuing.
- The date pickers open over the submit button. Set date fields with `page.evaluate` (value in
  `dd.MM.yyyy`) and click the button via `page.evaluate` as well.
- Radio buttons and flag checkboxes sit inside Bootstrap button-group labels that intercept clicks;
  click the surrounding `label` (`input.closest('label').click()` in `page.evaluate`), not the input.
- The sender selection ("Einsendernummer") is a select2 widget. Set the underlying `select` value in
  `page.evaluate` and trigger `change` via jQuery (`$('#Sending_SenderId').trigger('change')`).
- Postal-code fields call Geonames on every keystroke (allowed, but slow); set them with
  `page.evaluate` unless the lookup itself is under test.
- Export addresses called directly (`page.request.get`) need ISO dates in the query
  (`?From=2020-01-01&To=2020-12-31`); `dd.MM.yyyy` there is not bound and yields a file with only
  the header row. The export forms in the UI are not affected.
- `browser_run_code_unsafe` has no `require('fs')`. Save a file from a directly called export address
  by adding a temporary anchor with that address, clicking it, and calling `saveAs` on the download
  event; `page.request` responses cannot be written to disk.
- The submit buttons of the edit forms are `input type="submit"` elements named `primary-submit` and
  `secondary-submit` ("Änderungen speichern und Befund erstellen"); locate them by name, not as
  `button`.
- The rule ID of an interpretation is not shown on the report page; it appears only in the "Regel"
  column of the laboratory export. The RKI exports carry no laboratory number; match rows by "PatNr
  NRZM" or the stem number.
- Data with identical patient details: intake blocks a third patient with the same initials, birth
  date and postal code (002 FR-012). Create further submissions for an existing patient with
  "Bestehenden Patienten verwenden" while only one matching record exists.
- New Haemophilus and Meningococci submissions get a laboratory number of the year of saving, not of
  the sampling date (002 FR-003); plan date-range checks with that in mind.
- A signed-in account without the required role is redirected to `/Account/Login?ReturnUrl=…`
  (HTTP 302), not shown a 403 page. Missing views and unhandled errors show as HTTP 500. The host
  runs with detailed errors, so it shows the ASP.NET error page with the exception; production may
  show a generic page. Write "HTTP 500" in specs and reports, not the page's appearance.
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

When the spec claims a behavior the app lacks and the actual behavior looks wrong (e.g. the spec
says "binds only the intended fields", the app binds all of them), the verdict is `New defect` (or
`Defect confirmed` if the backlog already has it). Correct the spec text anyway and list the fix
under "Spec corrections applied"; the verdict counts once.

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
   backlog entry is the record. For a `DEC-*` item, a test may assert the parts of the FR the open
   question does not touch (e.g. that the evaluation is inserted), never the disputed wording or
   outcome.
   A `TEST-*` entry is a test gap, not a defect: when your added tests close it, the row's verdict is
   `Match` (path "Unit test (added)"), and the entry gets "Covered by tests added in the NNN
   verification (YYYY-MM-DD)." The owner closes entries; you do not remove them. A coverage note in
   the spec that your tests make outdated is corrected like any spec text (checklist note, listed
   under "Spec corrections applied"), without a row of its own.
   Count test cases (each `[TestCase]`/`[Values]` combination) in the report and name the methods; for
   combinatorial tests add the method count in parentheses.
4. Run the affected fixtures before committing (PowerShell, from the repository root). Build with
   `/p:BuildProjectReferences=false`: the test project references `HaemophilusWeb`, and rebuilding it
   regenerates `Web.config` and recycles the running app (Preconditions, 1).
   Specs are verified without application changes, so the existing `HaemophilusWeb` build output is
   current; if it is missing, stop the host before building the solution. `msbuild` and
   `vstest.console.exe` are often not on the PATH of the agent's shell; resolve them through
   `vswhere` (Visual Studio 2022):

   ```powershell
   $vsRoot = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -version "[17.0,18.0)" -latest -property installationPath
   $msbuild = "$vsRoot\MSBuild\Current\Bin\MSBuild.exe"
   $vstest = "$vsRoot\Common7\IDE\Extensions\TestPlatform\vstest.console.exe"
   & $msbuild HaemophilusWebTests\HaemophilusWebTests.csproj /p:Configuration=Debug /p:BuildProjectReferences=false /m /verbosity:minimal
   & $vstest HaemophilusWebTests\bin\Debug\HaemophilusWebTests.dll /Tests:SenderValidatorTests
   ```

   `/Tests:` matches substrings of the fully qualified name, so `IsolateViewModelValidatorTests` also
   runs `MeningoIsolateViewModelValidatorTests`. For an exact set use
   `/TestCaseFilter:"FullyQualifiedName~HaemophilusWeb.Validators.IsolateViewModelValidatorTests|FullyQualifiedName~…"`.

   Then run the full suite once with a results file, so a failure can be named:

   ```powershell
   & $vstest HaemophilusWebTests\bin\Debug\HaemophilusWebTests.dll /logger:trx /ResultsDirectory:<scratchpad>\TestResults
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

By primary path: Browser n · File n · DB n · Unit test (existing) n · Unit test (added) n · Code n ·
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
bullet under Edge Cases) the row covers. Number the edge cases as the list stood before this
verification and state the numbering under the summary; edge cases that the verification adds to
the spec are referred to by their bold title ("New EC "…""), or by their first words where the list
has no bold titles, so the numbers in the report never shift. Path is the one evidence path that decided the verdict;
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

Use a message file in the scratchpad (`git commit -F <file>`); PowerShell here-strings passed to `git
commit -F -` are not read as the message. After the commit, `git status` must show only changes that
were there before you started.

Then report to the owner in German: the summary table, the deviations you fixed, confirmed and new
defects with backlog IDs, what was not verifiable and why, open questions, and the suggested next
spec number. Keep the host running.

### Running as a subagent

The owner may let a main session run the verifications one after another, one subagent per spec
(never two at once: they share the host and the catalog). In that case the subagent:

- does everything above including the commit, but never starts, stops or restarts the host; if the
  host check fails, it stops and reports the failure;
- does not ask the owner; questions and `DEC-*` candidates go into "Open questions" and the report;
- returns the report as its final message, in German, at most about 400 words: commit hash, summary
  table, new backlog IDs and confirmed ones, spec corrections in one line each, not verifiable items,
  open questions, blocked external URLs, and a "Skill review" list with only what the skill got
  wrong or missed in this run.
