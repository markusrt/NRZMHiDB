# Verification: Haemophilus isolate analysis results entry

**Spec**: 004 · **Date**: 2026-11-04 · **Build**: `ce997d4` · **Host**: ScreenshotTests
SpecVerificationHost (`http://localhost:9899`, catalog `HaemophilusWeb_Screenshots`) ·
**Accounts used**: `Standard1` (all laboratory flows), `Labor1`, `Admin1`, `Rki1` (role checks) ·
**Scope**: full

## Summary

| Verdict | Count |
| --- | --- |
| Match | 16 |
| Spec deviation | 7 |
| Defect confirmed | 0 |
| New defect | 7 |
| Not verifiable | 0 |

By primary path: Browser 23 · File 0 · DB 6 · Unit test (existing) 1 · Unit test (added) 0 · Not verifiable 0 (adds up
to the row count). Also test-asserted: 10 rows with a browser verdict (21 cases added). Test run:
`vstest.console.exe … /Tests:IsolateViewModelValidatorTests,EpsilometerTestValidatorTest,IsolateControllerTests,IsolateViewModelMappingActionTests`
→ 74 passed, 0 failed (the filter also matches the Meningococci validator fixture); full suite → 681 passed, 0 failed
(685 total, 4 ignored).

Edge cases are numbered in the order of the spec's Edge Cases list before this verification: EC1 laboratory number
format, EC2 growth type, EC3 conditional detail fields, EC4 duplicate stem number, EC5 duplicate laboratory number, EC6
evaluation discrepancy, EC7 unknown or missing id. Edge cases added by this verification are named in the rows.

The seeded catalog has no breakpoints, so the form shows only the free E-Test row. Nine breakpoints with the prefix
`SV-004-` and five submissions `SV-004-A` to `SV-004-E` (KL001/26 to KL005/26, isolates 4 to 8) were created through the
UI. "Crafted" marks a request that bypasses the browser check. The status change of DATA-034 was triggered with the POST
the report page sends after generating a report; no report document was generated.

## Results

| ID | Spec says | App/test shows | Verdict | Path | Evidence |
| --- | --- | --- | --- | --- | --- |
| FR-001 · SC-001 | Edit form, pre-filled with current values | List link "KL001/26" opens `/Isolate/Edit/4` "Isolat bearbeiten"; after saving, reopening shows all entered values | Match | Browser | `verification/US1-1-isolate-form.png` |
| FR-002 | 1:1 with the submission; no isolate is created here | Each of the five intakes produced exactly one isolate; `/Isolate`, `/Isolate/Create`, `/Isolate/Delete/4`, `/Isolate/Details/4` → HTTP 404 | Match | Browser | DB query of `Sendings` and `Isolates` |
| FR-003 · US1.1 | Read-only context | "Entnahmeort" "Blut", "Material" "Isolat", "Invasiv" "Ja", "Patienten-Nr." 4, "Patientenalter bei Entnahme" 19; kept on a failed save | Match | Browser | `verification/US1-1-isolate-form.png`; also test-asserted: `Automapper/IsolateViewModelMappingActionTests.ProcessModelToViewModel_PopulatesInvasive` |
| FR-004 · EC2 | Type of growth required when growth is "yes" | Rule holds ("Die Art des Wachstums muss angegeben werden."). Not in the spec: "Ja" ticks "Typisches Wachstum auf KB", several types can be ticked (stored as 3), "Nein" hides and clears the types | Spec deviation | Browser | `verification/FR-004-growth-type.png`; also test-asserted: existing invalid-model case, added `Validators/IsolateViewModelValidatorTests.Validate_GrowthOtherThanYes_RequiresNoTypeOfGrowth` |
| FR-005 | Entry of all listed results incl. VITEK and api NH | All listed fields save and reload, except: api NH is not on the form, MALDI-TOF (VITEK MS) is read-only (hidden fields). Details appear only for "durchgeführt"/"positiv". REF-020 | Spec deviation | Browser | `verification/US1-2-filled.png`; DB row of isolate 4 |
| FR-006 · US2.2 · US2.3 | User adds rows, selects antibiotic and breakpoint; result derived and stored | Derivation correct at all limits: S ≤ 0,5 / R > 2: 0,5 "sensibel", 1 and 2 "intermediär", 4 "resistent"; S ≤ 1 / R > 1: 1 "sensibel", 1,5 "resistent"; "ohne Grenzwerte" "n.d.". Stored and reloaded. Not as specified: fixed rows (free, saved, standard antibiotics) instead of added rows; a measurement preselects the newest breakpoint; the breakpoint cannot be reset while a measurement exists; resetting the measurement removes the test | Spec deviation | Browser | `verification/US2-2-etest-derivation.png`, `verification/US2-3-etest-reopened.png`; also test-asserted: `Validators/EpsilometerTestValidatorTest.GetMhkScale_ValidAntibiotic_ReturnsSuitableList`, added `Automapper/IsolateViewModelMappingActionTests.EpsilometerTestsModelToViewModel_BuildsFreeSavedAndStandardRows`, `…ViewModelToModel_RowWithoutMeasurement_IsNotStored`, `Validators/EpsilometerTestValidatorTest.Validate_RowWithoutMeasurement_IsValid` |
| New EC "Free E-Test row without a measurement" · UX-011 | (not in the spec) | Antibiotic selected, no measurement: option value "MHK auswählen...", save blocked with "The Feld Messwert muss eine Zahl enthalten."; resetting the antibiotic saves. `en-US` browser: scale "0.75", no breakpoint preselected, no result, save blocked | New defect | Browser | `verification/row0-no-measurement.png` |
| FR-006 · SC-003 · New EC "E-Test result taken from the request" · DATA-035 | A measurement always yields a consistent result | Crafted: 0,5 with S ≤ 0,5 stored as "resistent" (`Result` 2); Ampicillin row stored with Meningococci breakpoint 8, then shown as Rifampicin without a selectable breakpoint, every save fails with "'Breakpoint' darf nicht leer sein"; no `Result` → HTTP 500 "Das Objekt mit Nullwert muss einen Wert haben." | New defect | DB | `verification/DATA-035-foreign-breakpoint.png`; DB query of `EpsilometerTests` |
| New EC "Every save rewrites the E-Tests" · DATA-033 | (not in the spec) | Re-saving isolate 4 unchanged: rows 1 to 4 lost their isolate (`Isolate_SendingId` NULL), rows 7 to 10 inserted; end of the run: 10 of 15 rows orphaned | New defect | DB | DB query before and after the save |
| FR-006 · REF-019 | (not in the spec) | Empty standard rows keep the configured order (Ampicillin, Cefotaxim, Amoxicillin / Clavulansäure, Meropenem) because the final sort discards its result; the antibiotic selection uses another order. Script emitted once per row, scale repeated in JavaScript | Spec deviation | Browser | `verification/US1-1-isolate-form.png` |
| FR-007 · US2.1 | Only Haemophilus antibiotics and breakpoints, current ones first | `SV-004-Rif-Meningo` and `SV-004-Mem-Meningo` not offered; Ampicillin row: "SV-004-Amp-neu - v14.0 vom 01. Jan. 24" before "SV-004-Amp-alt - v10.0 vom 01. Jan. 20"; "ohne Grenzwerte" last in the page data | Match | Browser | `verification/US1-1-isolate-form.png`; also test-asserted (added): `…BuildsFreeSavedAndStandardRows` |
| FR-008 · US3.1 | Serogroups A–F select the serotype, other results NTHi | a–f → "Hia" … "Hif"; "negativ", "auto", "poly", "Nicht beurteilbar", "n.d." → "NTHi" | Match | Browser | browser run |
| FR-008 · US3.2 · EC6 | Warning on disagreement, save not blocked | Agglutination "b", evaluation "Hia": 'Achtung: Diskrepanz zu Agglutination mit dem Wert "B"'; back to "Hib": gone; save succeeds | Match | Browser | `verification/US3-2-mismatch-warning.png` |
| FR-008 · SC-005 · UX-012 | A disagreement is always flagged | Warning stays after the agglutination changes ("a" → evaluation "Hia", warning still "B"; "negativ" → "NTHi", warning still "B"); saved disagreement shows no warning on reopening | New defect | Browser | `verification/US3-2-mismatch-reopened.png` |
| FR-009 | Laboratory number, stem number, report date, remark | All stored. Not in the spec: non-numeric stem number → "The Feld Stammnummer muss eine Zahl enthalten."; "Befund am" takes a date only | Spec deviation | Browser | DB row of isolate 4 |
| FR-010 · EC1 · SC-002 (browser) | Empty or malformed number fails with the '39/14' message | "abc", "12/3", "12/345", "12-34", "KL12/34", " 12/34" → the '39/14' message. Empty: browser "'Labornummer' darf nicht leer sein.", server the '39/14' message | Spec deviation | Browser | `verification/FR-010-labnumber-format.png`; also test-asserted (added): `Validators/IsolateViewModelValidatorTests.Validate_MalformedLaboratoryNumber_FailsOnLaboratoryNumber`, `…WellFormedLaboratoryNumber_IsValid`; existing `Controllers/IsolateControllerTests.ParseAndMapLaboratoryNumber_…` |
| FR-010 · SC-002 (server) · DATA-032 | Malformed number never stored | Crafted: "abc12/345xyz" → stored `Year` 2000, `YearlySequentialIsolateNumber` 0; "12/345" → form shows 012/345; "1/2/34" → form shows 001/02; afterwards restored to 005/26 | New defect | DB | DB query of `Isolates`; form value after each save |
| FR-011 · EC3 | Conditional details | 16S, Biotyper, MLST, Real-Time PCR together: six messages, e.g. "16S rDNA beste Übereinstimmung darf nicht leer sein.", "MLST Sequenztyp darf nicht leer sein."; values kept on redisplay; ftsI needs no evaluation | Match | Browser | `verification/FR-011-conditional.png`; also test-asserted: existing invalid-model cases, added Biotyper case, `Validate_OtherTestResults_RequireNoDetails`, `Validate_SeveralRealTimePcrResults_IsValid` |
| FR-011 (VITEK) | VITEK details required when determined | Rule asserted by test; crafted `MaldiTofVitek=Determined` redisplays the form without a visible message (no field for it) | Match | Unit test (existing) | `Validators/IsolateViewModelValidatorTests.Validate_WithAMaximalInvalidModel_ReturnsAllInvalidFields` (`noMaldiTofDetails`) |
| FR-012 · EC4 · EC5 · SC-004 | Duplicates rejected on the field | Stem 4001: "Diese Stammnummer ist bereits vergeben"; 001/26: "Diese Labornummer ist bereits vergeben"; both: only the laboratory-number message; 1/20 (seeded in 2020) also conflicts, nothing stored | Spec deviation | Browser | `verification/FR-012-duplicate-stem.png`, `verification/FR-012-duplicate-labnumber.png`; also test-asserted (added): `Controllers/IsolateControllerTests.HandleDbUpdateException_DuplicateIndex_AddsErrorToField` (2 cases), `…OtherFailure_Rethrows` |
| FR-013 · US1.4 | Invalid save persists nothing, form redisplayed | All failing saves stay on `/Isolate/Edit/n` with messages; entered results and E-Test rows remain; DB unchanged | Match | Browser | `verification/FR-011-conditional.png` |
| FR-014 · US1.2 · US1.3 · US3.3 | Primary → list; secondary → report creation | "Änderungen speichern" → `/PatientSending`; "Änderungen speichern und Befund erstellen" → `/Report/Isolate/7` "Befund erstellen"; invalid secondary save stays on the form | Match | Browser | `verification/US1-2-saved-list.png`, `verification/US1-3-secondary-submit.png` |
| FR-015 · SC-006 | Three states, set by the reporting flow | New isolates hold 0 ("Kein Befund erstellt"); the report POST sets 2 ("Endbefund") and the date; the form shows no status | Match | DB | DB query of `Isolates` |
| FR-015 · New EC "Stale form resets the report state" · DATA-034 | Status is not set by this feature | Form opened with "None", report POST sets Final, form saved: status 0, `ReportDate` NULL. Fresh form saved: time cut to 00:00 | New defect | DB | DB query of `Isolates` |
| FR-016 | Standard role required | `Rki1`, `Admin1`: GET and POST 302 to `/Account/Login?ReturnUrl=…`; anonymous likewise; `Labor1` GET 200 | Match | Browser | browser run; also test-asserted (added): `Controllers/IsolateControllerTests.Controller_RequiresStandardRole` |
| FR-017 · EC7 | No id → bad request; unknown → not found | `/Isolate/Edit` 400, `/Isolate/Edit/99999` 404; `/Isolate/Edit/abc` 400 (spec text extended) | Match | Browser | browser run; also test-asserted (added): `…Edit_WithoutId_ReturnsBadRequest`, `…Edit_UnknownId_ReturnsNotFound` |
| FR-018 (token) | Anti-forgery token on the form | Token present; POST without it → HTTP 500 "The required anti-forgery form field … is not present." | Match | Browser | browser run; also test-asserted (added): `…EditPost_ValidatesAntiForgeryToken` |
| FR-018 (binding) · SEC-014 | Protected against over-posting | Crafted: `ReportStatus=Final` stored on isolate 8; POST with `IsolateId=7` changed isolate 7 instead of 8 | New defect | DB | DB query of `Isolates` |
| FR-019 | No task planning | Navigation and form offer no assignment or planning | Match | Browser | browser run |
| New EC "Isolate of a deleted submission" (020 Assumptions) | (020, derived) Pages of a deleted submission stay reachable | `/Isolate/Edit/3` (soft-deleted `SEED-H-2`) → 200 with the full form; opened read-only, not saved | Match | Browser | browser run |

## Unit tests added

- `HaemophilusWebTests/Validators/IsolateViewModelValidatorTests.cs` (extended, 11 cases): invalid-model case without
  Biotyper details (FR-011), `Validate_WellFormedLaboratoryNumber_IsValid` (2 cases),
  `Validate_MalformedLaboratoryNumber_FailsOnLaboratoryNumber` (4 cases) (FR-010),
  `Validate_GrowthOtherThanYes_RequiresNoTypeOfGrowth` (2 cases) (FR-004), `Validate_OtherTestResults_RequireNoDetails`,
  `Validate_SeveralRealTimePcrResults_IsValid` (FR-011).
- `HaemophilusWebTests/Validators/EpsilometerTestValidatorTest.cs` (extended, 1 case): `Validate_RowWithoutMeasurement_IsValid`
  (FR-006).
- `HaemophilusWebTests/Controllers/IsolateControllerTests.cs` (extended, 7 cases): `Edit_WithoutId_ReturnsBadRequest`,
  `Edit_UnknownId_ReturnsNotFound` (FR-017), `Controller_RequiresStandardRole` (FR-016),
  `EditPost_ValidatesAntiForgeryToken` (FR-018), `HandleDbUpdateException_DuplicateIndex_AddsErrorToField` (2 cases),
  `HandleDbUpdateException_OtherFailure_Rethrows` (FR-012).
- `HaemophilusWebTests/Automapper/IsolateViewModelMappingActionTests.cs` (extended, 2 cases):
  `EpsilometerTestsModelToViewModel_BuildsFreeSavedAndStandardRows` (FR-006, FR-007),
  `EpsilometerTestsViewModelToModel_RowWithoutMeasurement_IsNotStored` (FR-006). Own breakpoint set, not the shared
  static store.
- The susceptibility derivation runs in the page script and has no unit test (REF-019). No tests for SEC-014, DATA-032
  to DATA-035, UX-011, UX-012 (defects).

## Spec corrections applied

- US2 intro, Independent Test, FR-006, new US2.4 and US2.5: fixed rows, breakpoint preselection, removing a measurement
  (was: "add an E-Test row"; the user selects antibiotic and breakpoint for each measurement). `[POTENTIAL-REFACTORING]`
  REF-019.
- FR-005, Key Entities (Isolate), Assumptions: api NH is not on the form, VITEK is read-only, details are conditional
  (was: both listed as entered results). `[POTENTIAL-REFACTORING]` REF-020.
- FR-011, Edge Cases (conditional fields): the VITEK rule concerns stored values only; messages carry the field label.
- FR-004, Edge Cases (growth type): preselected type, several types, hiding and clearing (was: only "must be selected").
- FR-009, Edge Cases: whole stem numbers, date-only "Befund am".
- FR-010, Edge Cases (laboratory number): different messages for an empty value in browser and server (was: the '39/14'
  message for both).
- FR-012, Edge Cases (duplicate laboratory number): one conflict per save; the same number of another year is no
  conflict.
- FR-017, Edge Cases (unknown id): bad request also for a non-numeric id.
- FR-008, Edge Cases (discrepancy), SC-005: warning text, evaluated only when the evaluation changes
  (`[SUSPECTED-DEFECT]` UX-012; was: "always flagged").
- FR-010, SC-002, Edge Cases: server check (`[SUSPECTED-DEFECT]` DATA-032; was: "never persists").
- SC-003, Assumptions, new Edge Case "E-Test result taken from the request" (`[SUSPECTED-DEFECT]` DATA-035; was:
  "always yields … consistent").
- FR-015, new Edge Case "Stale form resets the report state" (`[SUSPECTED-DEFECT]` DATA-034; was: "not by this feature").
- FR-018: token and binding described separately (`[SUSPECTED-DEFECT]` SEC-014; was: "protect … against over-posting via
  an anti-forgery token").
- New Edge Cases "Non-numeric stem number", "Free E-Test row without a measurement" (`[SUSPECTED-DEFECT]` UX-011), "Every
  save rewrites the E-Tests" (`[SUSPECTED-DEFECT]` DATA-033) and "Isolate of a deleted submission" (see 020).

## Backlog updates

- NEW SEC-014: isolate edit binds every isolate property.
- NEW DATA-032: server check of the isolate laboratory number is not anchored.
- NEW DATA-033: every isolate save orphans the previous E-Test rows.
- NEW DATA-034: isolate form writes back a stale report status and date.
- NEW DATA-035: E-Test result is taken from the request unchecked.
- NEW UX-011: free E-Test row is rebuilt without an empty value and in the browser language.
- NEW UX-012: agglutination discrepancy warning reacts only to changes of the evaluation.
- NEW REF-019: E-Test row construction.
- NEW REF-020: api NH and MALDI-TOF (VITEK MS) have no entry path.

## Not verifiable here

- None. The report document itself belongs to 008. The report page was opened with `/Report/QueryHealthOffice` blocked
  in the browser, because that request reaches the RKI tool on page load; every call was blocked, none reached the server.

## Open questions for the owner

- REF-020: are the stored api NH and VITEK values still needed, or can the fields leave the model and the laboratory
  export? `DEC-*` candidate; it touches export columns. Answered 2026-11-07: still needed for old records, read-only;
  recorded in the backlog entry.
- DATA-035: should the server repeat the E-Test derivation, or is the browser result authoritative? `DEC-*` candidate
  because it concerns clinical semantics. Answered 2026-11-07: the server repeats and validates; recorded in the
  backlog entry.
- Own records left behind: nine `SV-004-*` breakpoints; submissions `SV-004-A` to `SV-004-E`. Isolate 5 keeps the
  Meningococci breakpoint row, isolates 5 to 8 have report status "Endbefund" from the checks above. No seeded record was
  changed. A seeded Haemophilus breakpoint set would spare 008 and 011 the same setup.
