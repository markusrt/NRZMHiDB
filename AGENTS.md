# Agent operating guide

These instructions apply to any agent or human contributor working on the NRZMHi Database
(HaemophilusWeb) repository. Specifications follow GitHub Spec Kit; the project rules live in
`.specify/memory/constitution.md`, which wins over anything written here.

## Start or resume

Read, in order:

1. `.specify/memory/constitution.md` (principles, legacy debt, specification structure)
2. `AGENTS.md` (this file) and, for Claude Code, `CLAUDE.md`
3. `specs/BACKLOG.md` (consolidated observations and pending owner decisions)
4. The spec you are working on: `specs/NNN-*/spec.md` and `checklists/requirements.md`

Check that Spec Kit is initialized: `.specify/scripts/powershell/` and the agent's `speckit-*`
commands or skills must exist (`.specify/init-options.json` records `integration: claude`,
`script: ps`, `feature_numbering: sequential`). If they are missing, ask the owner to run
`specify init --here --integration <agent> --script ps`; do not hand-write Spec Kit artifacts.
`.specify/feature.json` is gitignored and changes on every Spec Kit run; never commit it.

Repository documents, not chat history, are the handoff. Do not infer completion from a
conversation: compare claims with files and git.

## What this repository is

An ASP.NET MVC 5 / Entity Framework 6 application on .NET Framework 4.8 for the German National
Reference Center for Meningococci and Haemophilus influenzae. Two near-parallel domains exist:
unprefixed types (`Sending`, `Patient`, `Isolate`) are Haemophilus, `Meningo*` types are
Meningococci (constitution, "Known Architectural Constraints").

| Path | Purpose |
| --- | --- |
| `HaemophilusWeb/` | Web application (Controllers, Domain, Models, Services, Validators, Views, Migrations) |
| `HaemophilusWebTests/` | Unit tests (NUnit 4, FluentAssertions, Moq); folders mirror the source |
| `SystemTests/`, `DbTest/` | Integration/system tests that need a database or browser |
| `AccessImporter/`, `CsvImporter/` | One-off import tools |
| `specs/001-…025-*/` | As-is specifications (status-quo inventory), one per capability and pathogen |
| `specs/BACKLOG.md` | Every `[SUSPECTED-DEFECT]` / `[POTENTIAL-REFACTORING]` observation, consolidated |
| `.specify/` | Spec Kit: constitution, templates, PowerShell scripts |

## Two kinds of specifications

**As-is (inventory) specs**, `specs/001` to `specs/025`, describe the code on the branch as it is.
They were produced with `/speckit-specify Feature title: <title> …` and carry:

- Header: `**Feature Branch**: speckit-init` (in backticks), `**Created**`,
  `**Status**: Draft (status-quo inventory)`, `**Input**: User description: "Feature title: …"`.
- A `> **Scope note**:` block naming what is in and out of scope and the related specs.
- User stories with acceptance scenarios, edge cases, functional requirements (`FR-`), tables
  for rules and strings, success criteria (`SC-`), assumptions.
- Observed defects are described neutrally and tagged `[SUSPECTED-DEFECT]`; refactoring
  candidates `[POTENTIAL-REFACTORING]`. No `[NEEDS CLARIFICATION]`: derive from code, and where
  that is impossible make a reasoned assumption and record it under Assumptions and in the
  checklist notes.
- `checklists/requirements.md` with dated review notes (`- Review YYYY-MM-DD: …`).
- Cross-references by spec number (e.g. "see 011 FR-011"), never by file path alone.

Adding another inventory spec: run `/speckit-specify` with the same prompt shape, in a clean
session (one specify run per session; Claude Code uses one general-purpose subagent per spec,
sequentially). Verify that exactly one new `specs/NNN-*/` directory appeared, then commit it on
its own as `Add as-is spec: <Feature title>`. Afterwards add every new tag to `specs/BACKLOG.md`.

**Change specs** describe what should change. Every change starts from a backlog ID
(constitution, "Specification Structure"):

| Step | Command | Notes |
| --- | --- | --- |
| Specify | `/speckit-specify <BACKLOG-ID>: <title> …` | Reference the backlog entry and the inventory specs it affects. `[NEEDS CLARIFICATION]` is allowed here. |
| Clarify | `/speckit-clarify` | At most five questions, each with a recommended answer. Stop for anything that changes clinical semantics, data retention, privacy, access rules, or external interfaces (RKI, PubMLST, Geonames, EpiScanGIS). |
| Plan | `/speckit-plan` | The Constitution Check gate applies; respect Principles I–V. |
| Tasks | `/speckit-tasks`, then `/speckit-analyze` | Tests are mandatory (Principle II); fix inconsistencies before implementing. |
| Implement | `/speckit-implement` | Work on a feature branch; stop before push/PR unless authorized. |

`DEC-*` entries need an owner decision before a spec is written; record the decision in the
backlog entry (`**decided YYYY-MM-DD**`) and keep the entry. Inventory specs are not rewritten
to describe the desired behavior; the change spec says which parts it supersedes.

## Autonomy and hard rules

- Independently perform reversible work: reading code, writing specs, editing docs, local
  commits on the current branch. Push, PR creation, merging, and branch deletion require the
  owner's explicit authorization. Spec Kit's git extension is not installed, so specs land on
  the current branch; branch creation is the owner's call.
- Never copy secret values into specs or docs. Configuration keys such as `RegistrationToken`,
  `Iris*` and `GeonamesAccount` may be named; their values from `Web.config` or its transforms
  may not.
- Never call external services (RKI, PubMLST, Geonames, EpiScanGIS) while specifying or testing.
  Describe integrations from the code only.
- Preserve unrelated and uncommitted work. Do not edit `HaemophilusWeb/Web.*.config` or other
  files outside the task, and do not use destructive git operations or rewrite history.
- Patient and isolate data are sensitive (Principle IV). Specs may quote German UI strings and
  report wording verbatim, but never real patient identifiers or records.
- Ask for decisions that change clinical semantics, data retention, privacy, access rules,
  external interfaces, or architecture; do not substitute plausible guesses. Collect
  non-blocking questions and present them together at the end of the task.
- Standing instructions from the owner go into this file ("Project working rules"), never only
  into agent memory.

## Language

| What | Language |
| --- | --- |
| Chat with the owner, questions, review summaries | German |
| Specs, checklists, backlog, constitution, this file, commit messages, code, tests | English |
| Headings, IDs (`FR-`, `SC-`, `DEC-`, …), tags | English, always |
| UI labels, report wording, enum display names quoted from the application | German, verbatim, in quotes |

## Commits

One coherent commit per spec or per change, on the current branch, in the existing style:
imperative subject (`Add as-is spec: …`, `Use headings instead of bold lines in specs 011-013`),
an optional body explaining why, and the agent's attribution trailer if its tooling requires one.
Never commit `.specify/feature.json`, `feature-prompts.md`, or `Web.*.config` changes you did not
make for the task. Run the lint check before committing documentation.

## Commands

The CI workflow in `.github/workflows/main.yml` is the reference. On Windows with Visual Studio
2022 Build Tools:

```powershell
nuget restore HaemophilusWeb.sln
msbuild HaemophilusWeb.sln /p:Configuration=Debug /m
vstest.console.exe HaemophilusWebTests\bin\Debug\HaemophilusWebTests.dll
npx -y markdownlint-cli "specs/**/*.md"   # config: .markdownlint.json (MD013 disabled)
```

Unit tests under `HaemophilusWebTests/` need no database or network. `SystemTests/` and
`DbTest/` do and are not part of the default check. Command definitions are not execution
evidence; report actual results.

## Working rules

- Edit files with the agent's edit tool; use the shell for commands (build, tests, git, lint)
  and bulk mechanical changes only. Never run commands that wait for stdin.
- Scratch files go to the session's scratchpad or temp directory, never into tracked folders.
- Read specs and code by section or search, not whole files, unless you are about to edit them.
- When a subagent produces a spec, verify its output yourself (directory, checklist, `git status`
  shows only the new directory) before committing.

## Project working rules

Standing rules from the owner. Add new ones here with the date and the reason.

- 2026-10-30: Owner decisions in `specs/BACKLOG.md` (`DEC-*`) may stay open; do not block
  other work on them and do not decide them implicitly in a spec.
- 2026-10-30: The backlog is written in English like the specs.
- 2026-10-30: Spec structure decisions D1–D5 belong in the constitution, not in a planning file.
