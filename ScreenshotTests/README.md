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
for tagged routes) into `screenshots/<mode>/`. `GalleryReportTest` (namespace
`ScreenshotTests.Reporting`, so it does **not** boot the app) diffs `baseline/` vs `current/` and
writes `report/index.html`.

## Prerequisites

- Visual Studio 2022 Build Tools / MSBuild, IIS Express, and SQL Server LocalDB (`MSSQLLocalDB`).
- Playwright's browser (one-time): build the project, then
  `pwsh ScreenshotTests/bin/Debug/net48/playwright.ps1 install chromium`.

## Build

The project references the classic `HaemophilusWeb` web project, so build with **MSBuild**, not
`dotnet build`:

```powershell
& "<VS>\MSBuild\Current\Bin\MSBuild.exe" ScreenshotTests\ScreenshotTests.csproj /t:Restore,Build
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

During the Bootstrap 3 → 5 upgrade the gallery is a **review aid** (differences are expected). Once
the migration is complete the baseline is re-recorded on Bootstrap 5 and the suite becomes a
regression gate.
