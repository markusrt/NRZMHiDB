# NRZMHi Database (HaemophilusWeb)

Laboratory database of the German National Reference Center for Meningococci and Haemophilus
influenzae (NRZMHi). It records submissions ("Einsendungen"), patients and isolates, interprets
laboratory results, generates reports, and produces exports for public-health partners.

- ASP.NET MVC 5, Entity Framework 6 (code first, migrations), .NET Framework 4.8, OWIN
  authentication, FluentValidation, AutoMapper, NLog.
- Two near-parallel domains: unprefixed types (`Sending`, `Patient`, `Isolate`) are Haemophilus,
  `Meningo*` types are Meningococci. Senders are shared.
- Project rules live in [`.specify/memory/constitution.md`](.specify/memory/constitution.md); agent
  and contributor rules in [`AGENTS.md`](AGENTS.md) (Claude Code specifics in [`CLAUDE.md`](CLAUDE.md)).

Patient and isolate data are sensitive. Never commit real patient data or secret configuration
values.

## Repository layout

| Path | Purpose |
| --- | --- |
| `HaemophilusWeb/` | Web application (Controllers, Domain, Models, Services, Validators, Views, Migrations, report templates) |
| `HaemophilusWebTests/` | Unit tests (NUnit 4, FluentAssertions, NSubstitute); folders mirror the source |
| `ScreenshotTests/` | Playwright harness: seeded LocalDB catalog, IIS Express, gold-master screenshots, spec verification host |
| `SystemTests/`, `DbTest/` | Legacy Selenium/system tests; `SystemTests/Features/` holds the Gherkin features from spec verification |
| `AccessImporter/`, `CsvImporter/` | One-off import tools |
| `specs/` | Spec Kit specifications (`001`–`025` as-is inventory) and [`specs/BACKLOG.md`](specs/BACKLOG.md) |
| `.specify/`, `.claude/skills/` | Spec Kit constitution, templates, scripts, and agent skills |

## Local development

### Prerequisites

- Windows with Visual Studio 2022 or the VS 2022 Build Tools (MSBuild, web workload), .NET Framework
  4.8 developer pack, IIS Express, and SQL Server LocalDB (`MSSQLLocalDB`).
- `nuget.exe` and `vstest.console.exe` on the `PATH` (both ship with Visual Studio / the Build Tools
  or can be installed separately).
- .NET SDK (for `dotnet test` of the SDK-style `ScreenshotTests` project).
- Node.js (for `npx`: markdownlint and the Playwright MCP server used by spec verification).

### Build

```powershell
nuget restore HaemophilusWeb.sln
msbuild HaemophilusWeb.sln /p:Configuration=Debug /m
```

The build generates `HaemophilusWeb/Web.config` from `Web.template.config` plus the transform of the
build configuration (`Web.Debug.config` or `Web.Release.config`). Edit the template or the
transforms, never the generated `Web.config`.

### Configuration and secrets

`Web.template.config` names the settings; some of them need real values that are not in the
repository: `RegistrationToken`, `GeonamesAccount`, and the `Iris*` keys (IRIS access). Ask the
owner for the values and enter them locally in `Web.Debug.config` (development) or
`Web.Release.config` (publishing). Never commit these values.

### Run the app

**Recommended: the seeded spec verification host.** It creates and seeds its own LocalDB catalog
(`HaemophilusWeb_Screenshots`), starts IIS Express on `http://localhost:9899`, and provides ready-made
accounts: `Labor1` (all roles), `Standard1` (Standardbenutzer), `Admin1` (Administrator), `Rki1`
(RKI), password `123456`.

```powershell
pwsh ScreenshotTests/bin/Debug/net48/playwright.ps1 install chromium   # once, after the first build
msbuild ScreenshotTests\ScreenshotTests.csproj /restore /t:Build /p:Configuration=Debug
dotnet test ScreenshotTests\ScreenshotTests.csproj --no-build --filter "FullyQualifiedName~SpecVerificationHost"
# wait for ScreenshotTests\host.ready, then open http://localhost:9899
New-Item ScreenshotTests\host.stop   # shuts the host down and restores Web.config
```

Avoid building `HaemophilusWeb` while the host runs; see
[`ScreenshotTests/README.md`](ScreenshotTests/README.md) for details and the seeded fixtures.

**Developer database.** Starting the web project from Visual Studio (IIS Express,
`http://localhost:49310/`) uses `DefaultConnection`, a LocalDB file `App_Data/aspnet-HaemophilusWeb.mdf`.
Entity Framework applies pending migrations on application start. A fresh database contains no
roles and no users; they have to be created manually before anyone can sign in (registration at
`/Account/Register?token=<RegistrationToken>` assigns an existing role).

## Testing

| What | How |
| --- | --- |
| Unit tests | `vstest.console.exe HaemophilusWebTests\bin\Debug\HaemophilusWebTests.dll` (no database or network needed) |
| Unit tests while the verification host runs | `msbuild HaemophilusWebTests\HaemophilusWebTests.csproj /p:Configuration=Debug /p:BuildProjectReferences=false`, then run as above |
| Gold-master screenshots | See [`ScreenshotTests/README.md`](ScreenshotTests/README.md) |
| Spec verification | `/spec-verify NNN` against the seeded host, see [Spec verification](#spec-verification) |
| Documentation lint | `npx -y markdownlint-cli "specs/**/*.md"` (config `.markdownlint.json`) |
| Legacy system tests | `SystemTests/`, `DbTest/` need a database and a browser; not part of the default check |

CI (`.github/workflows/main.yml`, Windows) restores, builds the solution, and runs the unit tests on
pushes and pull requests to `master`.

## Deployment

Production runs on an internal IIS server. Publishing uses Visual Studio publish profiles
(`HaemophilusWeb/Properties/PublishProfiles/*.pubxml`, git-ignored; ask the owner for them):

- Method: Web Deploy (MSDeploy) through the IIS Web Management Service (WMSVC, port 8172), build
  configuration `Release`, target an IIS application below `Default Web Site`, server-side backup
  before publishing enabled (`EnableMSDeployBackup`).
- The connection string is replaced by a publish parameter: SQL Server Express with integrated
  security. The profile does not deploy the database; Entity Framework migrates the schema when the
  application starts.
- The release secrets come from your local `Web.Release.config` (see
  [Configuration and secrets](#configuration-and-secrets)).

Publish: in Visual Studio, right-click `HaemophilusWeb` → Publish → choose the profile → Publish.

### Preparing an IIS server (first time)

1. Install the IIS role with ASP.NET 4.8 (Application Development: ASP.NET 4.8, .NET Extensibility
   4.8) and the **Management Service**. If .NET was installed before IIS, register it with
   `%windir%\Microsoft.NET\Framework64\v4.0.30319\aspnet_regiis.exe -iru`.
2. Install the [URL Rewrite module](https://www.iis.net/downloads/microsoft/url-rewrite): `Web.config`
   contains a rule that redirects every non-localhost request to HTTPS, so the site also needs an
   HTTPS binding with a certificate.
3. Install [Web Deploy](https://www.iis.net/downloads/microsoft/web-deploy) after the Management
   Service, enable remote connections for the Management Service, and open TCP port 8172 in the
   firewall. Grant the publishing account **IIS Manager Permissions** on the site and run
   "Configure Web Deploy Publishing" (walkthrough:
   [Installing and Configuring Web Deploy on IIS 8.0 or Later](https://learn.microsoft.com/en-us/iis/install/installing-publishing-technologies/installing-and-configuring-web-deploy-on-iis-80-or-later)).
4. To make the profile's backup setting effective, turn on server-side backups
   ([Web Deploy Automatic Backups](https://learn.microsoft.com/en-us/iis/publish/using-web-deploy/web-deploy-automatic-backups)).
5. Create the IIS application (application pool: .NET CLR v4.0, integrated pipeline). The pool
   identity needs a SQL Server login with rights to change the schema (migrations run at start-up),
   write access to the application's `Logs` folder (NLog), and a usable temp directory (exports and
   uploads are written there first).

## Specifications (Spec Kit)

The project uses [GitHub Spec Kit](https://github.com/github/spec-kit) (Claude Code integration,
PowerShell scripts, sequential numbering). Read in this order:
[constitution](.specify/memory/constitution.md), [`AGENTS.md`](AGENTS.md),
[`specs/BACKLOG.md`](specs/BACKLOG.md), then the spec you work on.

- **As-is (inventory) specs** `specs/001`–`025` describe the current code, one per capability and
  pathogen. Suspected defects and refactoring candidates are tagged and consolidated in the backlog.
- **Spec verification** checks an inventory spec against the running app and records the result in
  `specs/NNN-*/verification.md` and `SystemTests/Features/NNN-*.feature`.
- **Change specs** start from a backlog ID and go through specify → clarify → plan → tasks → analyze
  → implement. `DEC-*` backlog entries need an owner decision first.

Agents reply in German and write all artifacts in English. One coherent commit per spec or change;
push, pull requests, merges, and branch creation need the owner's authorization.

### Prompt templates

Replace `<…>`. The prompts work in German or English.

#### New as-is spec

Run in a fresh session (one specify run per session). In Claude Code, the main session starts one
`general-purpose` subagent per spec with this prompt, then verifies and commits the result.

```text
/speckit-specify Feature title: "<Title>". Document the current, as-implemented behavior of
<capability> as it exists today. Read <controllers, models,
validators, views>. Cover <aspects: create/edit/list, validation, roles, …>. Note <related specs or
shared concepts>. Document observable behavior only — do not propose changes.
```

Afterwards: exactly one new `specs/NNN-*/` directory, commit `Add as-is spec: <Title>`, add every new
`[SUSPECTED-DEFECT]` / `[POTENTIAL-REFACTORING]` tag to `specs/BACKLOG.md`.

#### Spec verification

Start the host first (see [Run the app](#run-the-app)); one spec per session.

```text
Spec verification <NNN>. First read CLAUDE.md, AGENTS.md (section "Spec verification") and
.claude/skills/spec-verify/SKILL.md completely. The host is already running (ScreenshotTests/host.ready);
do not restart it and do not stop it at the end. msbuild and vstest.console.exe are on the PATH.
Run /spec-verify <NNN> completely, both evidence paths. <Optional: extra data to prepare or items to
focus on.> Report in German, artifacts in English, one commit on <branch>. Add a short skill review at
the end (only deviations from the previous verification).
```

#### Change spec (from a backlog entry)

Work on a feature branch created by the owner. Run the steps in order; review each result before the
next.

```text
/speckit-specify <BACKLOG-ID>: <title>. Source: specs/BACKLOG.md <BACKLOG-ID>. Affected inventory
specs: <NNN FR-…, NNN Edge Cases>. Desired change: <what should be different>. State which parts of
the inventory specs this supersedes.
```

```text
/speckit-clarify
```

At most five questions, each with a recommended answer. The agent stops for anything that changes
clinical semantics, data retention, privacy, access rules, or external interfaces (RKI, PubMLST,
Geonames, EpiScanGIS).

```text
/speckit-plan
/speckit-tasks
/speckit-analyze
/speckit-implement
```

The plan must pass the constitution check (principles I–V); tasks include tests (principle II);
fix analyze findings before implementing; implementation stops before push or pull request unless
authorized.

#### Record an owner decision

```text
Decision for <DEC-ID>: <decision and reason>. Record it in specs/BACKLOG.md as
"**decided <YYYY-MM-DD>**" and keep the entry. Name the backlog items that follow from it (e.g. which
DATA/UX entry now becomes a change spec) and update their text. Do not change inventory specs. One
commit.
```

If the decision is a standing rule for future work, it also goes into `AGENTS.md` ("Project working
rules").
