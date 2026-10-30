# Gold-master screenshot tests

Visual-regression harness for the Bootstrap upgrade (and future UI changes). It boots the real
`HaemophilusWeb` app against an isolated database, logs in, and captures a full-page screenshot of
every navigable view. A diff report highlights what changed between two runs.

## How it works

`Harness` (an NUnit `[SetUpFixture]`) runs once per test session and:

1. Drops and recreates a dedicated LocalDB catalog `HaemophilusWeb_Screenshots` (`LocalDb`).
2. Seeds a login user (`Labor1` / `123456`, all roles) plus one record per entity at **id = 1**
   so id-based routes render (`Seeder`, in-process against the app's own `ApplicationDbContext`).
3. Points the app's `DefaultConnection` at that catalog (`WebConfigConnectionSwap`, auto-restored).
4. Starts IIS Express hosting the built app on `http://localhost:9899` (`IisExpress`).
5. Launches headless Chromium and logs in once, reusing the session for every screenshot.

`GoldMasterTests` then captures each route in `Routes.All` (desktop 1366×900, plus mobile 390×844
for tagged routes) into `screenshots/<mode>/`. `GalleryReportTest` (namespace `GalleryReport`, kept
outside the `ScreenshotTests` hierarchy so it does **not** boot the app) diffs `baseline/` vs
`current/` and writes `report/index.html`.

## Run locally (quick start)

No manual database setup is needed — the harness creates and seeds its own isolated LocalDB catalog.
From the repository root, in PowerShell:

```powershell
$msbuild = "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"

# 1. Build (compiles the web app + this project, copies runtime deps).
& $msbuild ScreenshotTests\ScreenshotTests.csproj /restore /t:Build

# 2. One-time: download the headless browser.
pwsh ScreenshotTests\bin\Debug\net48\playwright.ps1 install chromium

# 3. Capture the current screenshots (boots the app, seeds data, logs in, shoots every view).
$env:SCREENSHOT_MODE = "current"
dotnet test ScreenshotTests\ScreenshotTests.csproj --no-build --filter GoldMasterTests

# 4. Build the baseline-vs-current gallery, then open it.
dotnet test ScreenshotTests\ScreenshotTests.csproj --no-build --filter GalleryReportTest
start ScreenshotTests\report\index.html
```

Re-run steps 1, 3 and 4 after a UI change to see what moved. To reset the gold master to the current
rendering, capture with `SCREENSHOT_MODE = "baseline"` instead and commit `screenshots/baseline/`.
See **Build** and **Run** below for details.

## Prerequisites

- Visual Studio 2022 Build Tools / MSBuild, IIS Express, and SQL Server LocalDB (`MSSQLLocalDB`).
- Playwright's browser (one-time): build the project, then
  `pwsh ScreenshotTests/bin/Debug/net48/playwright.ps1 install chromium`.

## Build

The project references the classic `HaemophilusWeb` web project, so build with **MSBuild**, not
`dotnet build`:

```powershell
& "<VS>\MSBuild\Current\Bin\MSBuild.exe" ScreenshotTests\ScreenshotTests.csproj /restore /t:Build
```

The build compiles the web app and copies its `bin` (runtime dependencies) into the test output.

## Run

```powershell
# 1. Record the baseline (the gold master) – commit these PNGs.
$env:SCREENSHOT_MODE = "baseline"
dotnet test ScreenshotTests\ScreenshotTests.csproj --no-build --filter GoldMasterTests

# 2. After a change, record the current set.
$env:SCREENSHOT_MODE = "current"
dotnet test ScreenshotTests\ScreenshotTests.csproj --no-build --filter GoldMasterTests

# 3. Build the side-by-side diff gallery -> ScreenshotTests/report/index.html
dotnet test ScreenshotTests\ScreenshotTests.csproj --no-build --filter GalleryReportTest
```

`screenshots/baseline/` is committed; `screenshots/current/` and `report/` are git-ignored.

During the Bootstrap 3 → 5 upgrade the gallery was a **review aid** (differences were expected). The
committed baseline now reflects **Bootstrap 5**, so the suite is a regression reference going forward.

## Bootstrap 3 → 5 one-time comparison gallery

A second gallery (`report/bs3-vs-bs5/index.html`) shows the full before/after for the upgrade.
It compares `screenshots/bs3/` (Bootstrap 3 snapshots) against `screenshots/baseline/` (Bootstrap 5).

`screenshots/bs3/` is **git-ignored** (49 large PNGs). To regenerate it after checking out this
branch, extract the snapshots that were committed on the original BS3 gold-master (commit `bb9616d`):

```powershell
$bs3 = "D:\Development\NRZMHiDB\ScreenshotTests\screenshots\bs3"
New-Item -ItemType Directory -Force $bs3 | Out-Null
git ls-tree --name-only bb9616d ScreenshotTests/screenshots/baseline/ | ForEach-Object {
    $fname = Split-Path $_ -Leaf
    git show "bb9616d:$_" | Set-Content "$bs3\$fname" -Encoding Byte
}
Write-Host "Extracted $(Get-ChildItem $bs3 | Measure-Object | Select-Object -ExpandProperty Count) PNGs"
```

Then regenerate the gallery report:

```powershell
$vstest = "C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe"
& $vstest ScreenshotTests\bin\Debug\net48\ScreenshotTests.dll /Tests:BuildBs3VsBs5Gallery
start ScreenshotTests\report\bs3-vs-bs5\index.html
```

**If new routes are added to the branch** and you want them included in the BS3 comparison, you would
need to manually capture those views from a BS3 checkout and add the PNGs to `screenshots/bs3/`.
For most purposes (reviewing the upgrade) the existing 49 views are sufficient.

## CI

The `screenshot-tests` job in `.github/workflows/main.yml` builds the app (which generates `Web.config`
from `Web.template.config`), installs headless Chromium, starts SQL LocalDB, captures the current
screenshots, verifies every view renders and the date picker opens, and uploads the screenshots and
diff gallery as a build artifact. Pixel diffing is a human-review aid (font rendering differs across
machines), not a hard pass/fail gate.

## Spec verification host

The same harness can keep the seeded app running for an interactive session, for example an agent
that verifies the as-is specifications under `specs/` in a browser (`/spec-verify NNN`, see
`AGENTS.md`). `SpecVerificationHost.BootAndWait` is an `[Explicit]` test: the `Harness` resets and
seeds `HaemophilusWeb_Screenshots`, starts IIS Express on `http://localhost:9899`, and the test then
waits until `ScreenshotTests\host.stop` exists (or `SPEC_VERIFY_TIMEOUT_MINUTES`, default 240).

```powershell
& $msbuild ScreenshotTests\ScreenshotTests.csproj /restore /t:Build
dotnet test ScreenshotTests\ScreenshotTests.csproj --no-build --filter "FullyQualifiedName~SpecVerificationHost"
# ... in another shell, wait for ScreenshotTests\host.ready, then drive http://localhost:9899
New-Item ScreenshotTests\host.stop   # shuts the host down and restores Web.config
```

`host.ready` lists the base URL and the seeded accounts: `Labor1` (all roles) plus one account per
role, `Standard1` (Standardbenutzer), `Admin1` (Administrator) and `Rki1` (RKI), all with password
`123456`. Both marker files are git-ignored.

Run one host at a time. Every harness run (host and gold-master) fails before touching the catalog or
`Web.config` when port 9899 is already in use. While the host waits, it watches its IIS Express; when
that exits, the host deletes `host.ready`, restores `Web.config` and ends with a failed test. A killed
host process leaves `host.ready` behind, so the file alone does not prove that the app runs; the
`/spec-verify` skill checks the host process, the IIS Express process and the login page.

On top of the gold-master seed, the host adds fixtures that later specs need
(`Seeder.SeedSpecVerificationFixtures`, listed in `host.ready`): sender `SV-Seed-DEC-002` with the
Haemophilus submissions KL002/20 (active) and KL003/20 (soft-deleted) and the Meningococci submissions
MZ002/20 (active) and MZ003/20 (soft-deleted), sampled 10.03.2020 and 15.06.2020 (spec 013 FR-015,
backlog DEC-002); sender `SV-Seed-Positive` with the Meningococci submission MZ004/20 (blood, sampled
20.05.2020, isolate matching stem interpretation rule 8: Neisseria meningitidis, serogroup W/Y), the only
record the RKI, IRIS and LGA exports include (spec 003 FR-021, spec 011). The gold-master run does not
seed them, so its lists keep one record each.

**Avoid building the web app while the host runs.** Every build of `HaemophilusWeb` (the solution, the
web project, `ScreenshotTests` without the switch below, or `HaemophilusWebTests` without
`/p:BuildProjectReferences=false`) regenerates `HaemophilusWeb\Web.config` from `Web.template.config`,
which would point the running app back at the developer database. The host checks `Web.config` every
two seconds, re-applies its connection string (the regenerated file becomes the backup restored on
shutdown) and appends "Connection swap re-applied at …" to `host.ready`; the app still recycles and runs
on the rebuilt binaries. Use:

```powershell
msbuild HaemophilusWebTests\HaemophilusWebTests.csproj /p:Configuration=Debug /p:BuildProjectReferences=false
& $msbuild ScreenshotTests\ScreenshotTests.csproj /restore /t:Build /p:SkipWebAppBuild=true   # only while the host is stopped: its DLL is locked
```
