# Verification: Meningococci isolate analysis results entry

**Spec**: 005 · **Date**: 2026-11-08 · **Build**: `dd1a444` · **Host**: ScreenshotTests
SpecVerificationHost (`http://localhost:9899`, catalog `HaemophilusWeb_Screenshots`) ·
**Accounts used**: `Standard1` (all laboratory flows), `Labor1`, `Admin1`, `Rki1` (role checks) ·
**Scope**: full

## Summary

| Verdict | Count |
| --- | --- |
| Match | 16 |
| Spec deviation | 11 |
| Defect confirmed | 8 |
| New defect | 5 |
| Not verifiable | 1 |

By primary path: Browser 31 · File 0 · DB 9 · Unit test (existing) 0 · Unit test (added) 0 · Not verifiable 1 (adds up
to the row count). Also test-asserted: 14 rows with a browser verdict (18 cases added). Test run:
`vstest.console.exe … /Tests:MeningoIsolateViewModelValidatorTests,MeningoIsolateControllerTests,MeningoIsolateViewModelMappingActionTests,EpsilometerTestValidatorTest`
→ 59 passed, 0 failed; full suite → 699 passed, 0 failed.

Edge cases are numbered in the order of the spec's Edge Cases list before this verification: EC1 missing or unknown
id, EC2 laboratory number format, EC3 duplicate identifiers, EC4 growth-dependent identification, EC5 conflicting
serogenogroup PCR results, EC6 conditionally required details, EC7 conditionally displayed molecular fields, EC8 no
PubMLST match, EC9 uploaded but not yet online. Edge cases added by this verification are named in the rows.

Six submissions `SV-005-A` to `SV-005-F` (MZ001/26 to MZ006/26, isolates 5 to 10) and the breakpoint `SV-005-Cip-alt`
(Ciprofloxacin, Meningococci, v10.0, 2020) were created through the UI; the seeded "SV-Seed …" breakpoints supplied
the current ones. "Crafted" marks a request that bypasses the browser check. The status change of DATA-034 was
triggered with the POST the report page sends after generating a report; no report document was generated. The
PubMLST lookup never reached the server: for US3.1/US3.2 the browser answered the lookup request itself with a
synthetic record (PubMLST id 99001, values prefixed `SV`), for US3.3 the request was aborted. UX-012 (agglutination
discrepancy warning) does not apply: the Meningococci form has no evaluation field and no warning.

## Results

| ID | Spec says | App/test shows | Verdict | Path | Evidence |
| --- | --- | --- | --- | --- | --- |
| FR-001 · SC-001 | Edit the one-to-one isolate; no standalone isolate | List link "MZ001/26" opens `/MeningoIsolate/Edit/5` "Isolat bearbeiten"; values reopen as saved; each intake produced one isolate; `/MeningoIsolate`, `/Create`, `/Delete/5`, `/Details/5` → HTTP 404 | Match | Browser | `verification/US1-1-isolate-form.png`; DB query of `MeningoIsolates` |
| FR-002 · US1.1 | Pre-filled, read-only context | "Entnahmeort" "Blut", "Material" "Vitaler Stamm", "Invasiv" "Ja", "Patienten-Nr." 5, "Patientenalter bei Entnahme" 26 | Match | Browser | `verification/US1-1-isolate-form.png`; also test-asserted: `Automapper/MeningoIsolateViewModelMappingActionTests.ProcessModelToViewModel_ConvertsEnumValuesToDescription`, `…SetsPatientId` |
| FR-003 · New EC "Non-numeric stem number" | General results incl. report date and remark; VITEK read-only | All fields saved and reopened (isolate 5). Not in the spec: Biotyper details only for "durchgeführt"; E-Test rows between ONPG and serogroup PCR; "Befund am" date only; non-numeric stem → "The Feld Stammnummer muss eine Zahl enthalten." | Spec deviation | Browser | `verification/US1-2-filled.png`; DB row of isolate 5 |
| FR-003 · REF-020 | VITEK values read-only | Read-only display, posted back as hidden values; crafted `MaldiTofVitek=Determined` without details redisplays the form without a visible message | Defect confirmed | Browser | browser run |
| FR-004 · EC7 | Molecular fields; PorA/FetA/cswy fields shown for positive PCR | All saved. Not in the spec: RIDOM multiple choice shown for positive Real-Time PCR, two species saved; device preselected "NSH Real-Time-PCR (BD-MAX)"; porA, fetA, cswy positive without VR/allele save | Spec deviation | Browser | DB row of isolates 5 and 10; also test-asserted (added): `Validators/MeningoIsolateViewModelValidatorTests.Validate_SeveralRealTimePcrResults_IsValid`, `…PositivePorAFetAAndCswyPcrWithoutDetails_IsValid` |
| FR-005 · US2.1 | Only Meningococci antibiotics and breakpoints, newest first | Free row offers Cefotaxim, Azithromycin, Ciprofloxacin, Penicillin, Rifampicin (no Haemophilus antibiotic); Ciprofloxacin row: "SV-Seed Ciprofloxacin Meningococci - v14.0 vom 01. Jan. 24" before "SV-005-Cip-alt - v10.0 vom 01. Jan. 20"; Azithromycin "ohne Grenzwerte" with its own scale (16 to 0,03) | Match | Browser | `verification/US2-2-etest-derivation.png` |
| FR-006 · US2.2 · New US2.5 | Result derived from measurement and breakpoint | Penicillin S ≤ 0,064 / R > 0,25: 0,064 "sensibel", 0,094 and 0,25 "intermediär", 0,38 "resistent"; Ciprofloxacin 0,016 "sensibel", 0,023 "resistent"; "ohne Grenzwerte" "n.d."; stored and reloaded. Not in the spec: a measurement preselects the newest breakpoint; another breakpoint re-derives (0,032: new "resistent", old "sensibel"); resetting the breakpoint reselects the newest; resetting the measurement clears the result | Spec deviation | Browser | `verification/US2-2-etest-derivation.png`; DB query of `EpsilometerTests`; also test-asserted: `Automapper/IsolateViewModelMappingActionTests.EpsilometerTestsViewModelToModel_RowWithoutMeasurement_IsNotStored` (shared base) |
| FR-006 · US2.4 · REF-019 | Saved rows fixed, one free row, missing primary rows | Rows as stated; order: free row, saved rows sorted (Cefotaxim, Azithromycin, Penicillin), then empty primary rows in configured order (Ciprofloxacin, Rifampicin); free-row selection in a third order | Defect confirmed | Browser | `verification/US1-1-isolate-form.png` |
| FR-006 · SC-003 · New EC "E-Test result taken from the request" · DATA-035 | Every saved row consistent | Crafted: Cefotaxime 0,064 (S ≤ 0,125) stored as "resistent" (`Result` 2); Benzylpenicillin row stored with the Haemophilus Ampicillin breakpoint, then shown as "Ampicillin" without a selectable breakpoint, every save fails with "'Breakpoint' darf nicht leer sein"; no result → HTTP 500 "Das Objekt mit Nullwert muss einen Wert haben." | Defect confirmed | DB | `verification/DATA-035-foreign-breakpoint.png`; DB query of `EpsilometerTests` |
| New EC "Every save rewrites the E-Tests" · DATA-033 | (not in the spec) | Re-saving isolate 5 unchanged: rows 1 to 3 lost their isolate, rows 4 to 6 inserted | Defect confirmed | DB | DB query before and after the save |
| New EC "Free E-Test row without a measurement" · UX-011 | (not in the spec) | Free row "Rifampicin" without measurement: save blocked with "The Feld Messwert muss eine Zahl enthalten." | Defect confirmed | Browser | `verification/UX-011-free-row.png` |
| FR-007 · US2.3 | Off-scale or incomplete measurement rejected | Crafted: 0,07 → "'Messwert' muss ein Wert von der E-Test Skala sein."; no breakpoint → "'Breakpoint' darf nicht leer sein"; free row without antibiotic → the scale message; nothing stored | Match | Browser | browser run; also test-asserted: `Validators/EpsilometerTestValidatorTest.Validate_WithAMaximalInvalidModel_ReturnsAllInvalidFields`, `…GetMhkScale_ValidAntibiotic_ReturnsSuitableList` |
| FR-008 · EC2 (browser) | Empty or malformed number rejected | "abc" → "Die Labornummer muss in der Form '39/14' eingegeben werden."; empty → "'Labornummer' darf nicht leer sein." in the browser, the '39/14' message from the server (crafted) | Spec deviation | Browser | browser run; also test-asserted (added): `Validators/MeningoIsolateViewModelValidatorTests.Validate_MalformedLaboratoryNumber_FailsOnLaboratoryNumber` (4), `…WellFormedLaboratoryNumber_IsValid` (2) |
| FR-008 · EC2 (server) · DATA-032 | Malformed number never stored | Crafted: "abc12/345xyz" → `Year` 2000, number 0; "12/345" → form shows 012/345; "-5/26" → number -5; restored afterwards | Defect confirmed | DB | DB query of `MeningoIsolates` |
| EC2 (leading "-") | Shown read-only | Shown read-only as "NR005/26", no input | Spec deviation | Browser | `verification/EC-negative-labnumber-readonly.png`; also test-asserted: `Utils/ReportFormatterTests.ToLaboratoryNumberWithPrefix_ValidInput_AddsPrefix` ("-01/2019" → "NR01/2019") |
| New EC "Changed laboratory number is ignored" · DATA-037 | (FR-003: laboratory number entered) | "007/26" entered for MZ006/26; request carries `LaboratoryNumber=006/26` (hidden) and `007/26`; saved without message, isolate keeps 006/26 | New defect | DB | posted body; DB query of `MeningoIsolates` |
| FR-009 · EC4 | Oxidase determined unless both growths "Nein" | Growth on blood or Martin-Lewis agar with oxidase "n.d." → 'Oxidase darf nicht den Wert "n.d." haben (angewachsen)' | Match | Browser | `verification/FR-009-oxidase.png`; also test-asserted: `Validators/MeningoIsolateViewModelValidatorTests.Validate_WithOxidaseNotSet_IsInvalid`, `…WithOxidaseAndGrowthSet_IsValid` (6) |
| FR-010 · EC4 | No growth: identification "n.d.", serogroup PCR/siaA/ctrA/cnl allowed | Agglutination, Biotyper or oxidase with both "Nein" → message on both growth fields ('Wenn Wachstum auf Blutagar den Wert "Nein" hat, …'); serogroup PCR, siaA, ctrA, cnl saved | Match | Browser | `verification/FR-010-no-growth.png`; also test-asserted: existing invalid models, `…WithNoGrowthButOnlySerogroup_IsValid`, `…WithNoGrowthButSiaACtrAAndCnl_IsValid`; added oxidase case |
| FR-011 · US1.4 · US1.5 | No growth sets "no growth" unless native | "Vitaler Stamm" → "Nicht angewachsen"; "Nativmaterial" and "Isolierte DNA" unchanged (spec named only native material) | Spec deviation | Browser | DB query of `MeningoSendings`; also test-asserted: `Automapper/MeningoIsolateViewModelMappingActionTests.ProcessViewModelToModel_NoGrowthAtAll_…` (4), added `…AnyGrowth_KeepsMaterial` (2) |
| New EC "No-growth material change on the default values and never undone" · DATA-038 | (not in the spec) | Untouched save of a new isolate (defaults "Nein") set "Nicht angewachsen"; entering growth afterwards left it | New defect | Browser | `verification/FR-011-material-stays.png` |
| FR-012 · EC5 | Only one serogenogroup PCR positive | Two or three positive → "Es darf nur ein Serogenotyp positiv sein." on each positive field | Match | Browser | `verification/FR-012-serogeno.png`; also test-asserted: existing invalid model `twoSeroGenoGroupsPositive` |
| FR-013 · EC6 | Conditional details required | 16S, Biotyper, Real-Time PCR together: five distinct messages, e.g. "16S rDNA beste Übereinstimmung darf nicht leer sein.", "Real-Time-PCR Auswertung (RIDOM) darf nicht leer sein."; values kept | Match | Browser | `verification/FR-013-conditional.png`; also test-asserted: existing RIDOM case, added 16S, Biotyper and VITEK cases |
| New EC "Label and message slips" · UX-014 | (not in the spec) | PubMLST tab label "'porB"; the 16S percentage message appears a second time under PorA-VR2 when porA is positive | New defect | Browser | `verification/16S-message-twice.png`, `verification/US3-1-pubmlst-found.png` |
| FR-014 · EC3 | Duplicates rejected on the field | Stem 5001 on isolate 10: "Diese Stammnummer ist bereits vergeben"; laboratory number only crafted (form ignores changes, DATA-037): "001/26" and "001/20" → "Diese Labornummer ist bereits vergeben"; nothing stored | Spec deviation | Browser | `verification/FR-014-duplicate-stem.png`; also test-asserted: `Controllers/IsolateControllerTests.HandleDbUpdateException_DuplicateIndex_AddsErrorToField` (shared base) |
| FR-015 · US1.3 · New EC "Errors on both tabs" | Nothing persisted, error tab visible | Failed saves stay on `/MeningoIsolate/Edit/n` with messages and entered values; DB unchanged; error on "Molekulare Typisierung" opens that tab; errors on both tabs open "Molekulare Typisierung" only | Spec deviation | Browser | `verification/FR-013-conditional.png` |
| FR-016 · US1.2 | Primary save → Meningococci list | "Änderungen speichern" → `/MeningoPatientSending` | Match | Browser | browser run |
| FR-017 · US4.1 · US4.2 · US4.3 · SC-004 | Secondary save → report creation, no completeness gate | "Änderungen speichern und Befund erstellen" on isolate 10 (many "n.d.") → `/MeningoReport/Isolate/10` "Befund erstellen", remark stored; invalid secondary save stays on the form | Match | Browser | `verification/US4-1-secondary-submit.png` |
| FR-018 | Status none/preliminary/final, kept by entry | New isolates 0; the report POST sets 2 and the date; a fresh form keeps 2 (date stored as 00:00); the form shows no status | Match | DB | DB query of `MeningoIsolates` |
| FR-018 · New EC "Stale form resets the report state" · DATA-034 | Status not changed by this feature | Form opened, report POST sets "Endbefund", form saved: status 0, `ReportDate` NULL | Defect confirmed | DB | DB query of `MeningoIsolates` |
| FR-019 (query) | Individual query by prefixed stem number, result shown | "Abfragen" posts `isolateReference=DE7001` (stem number as loaded) to `/PubMlst/NeisseriaIsolates/` | Match | Browser | intercepted request |
| FR-019 (online) · US3.4 · EC9 | Match depends on PubMLST publishing uploaded data | Needs the external PubMLST and IRIS databases | Not verifiable | — | — |
| FR-020 · US3.1 · SC-005 | Database, id and typing fields read-only; link | Simulated record: "Isolat wurde geladen", 17 typing fields filled read-only; database and PubMLST id only hidden; link "PubMLST Eintrag anzeigen" → `https://pubmlst.org/bigsdb?page=info&db=pubmlst_neisseria_isolates&id=99001` | Spec deviation | Browser | `verification/US3-1-pubmlst-found.png` |
| FR-021 · US3.2 | Add new record or update stored one | Save created record 24 (PubMLST id 99001); a second isolate's lookup with changed values and database `pubmlst_neisseria_iris` updated record 24 for both isolates; reopened link reads "PubMLST Eintrag"; re-saving a matched isolate keeps the link | Spec deviation | DB | DB query of `NeisseriaPubMlstIsolates` |
| US3.3 · UX-013 | No stem number → not found | Isolate without stem number: request `isolateReference=DE -` sent (blocked), then "Isolat wurde nicht gefunden"; after a failed lookup fields emptied, id 0, an earlier link stays enabled | New defect | Browser | `verification/US3-3-no-stem-number.png`, `verification/US3-3-not-found-after-match.png` |
| FR-022 · US3.5 · EC8 · SC-006 | Missing match does not block | Isolates 5, 9 and 10 without match saved and opened report creation | Match | Browser | `verification/US4-1-secondary-submit.png` |
| New EC "Empty PubMLST record on every save without a match" · FR-021 · DATA-036 | (FR-021: only non-zero ids touch PubMLST records) | First save of isolate 5 created an empty record (PubMLST id 0) and linked it; every later save another (about 20 at the end); failed lookup + save moved isolate 8 from record 24 to a new empty record | New defect | DB | DB query of `NeisseriaPubMlstIsolates`, `MeningoIsolates` |
| FR-023 (role, token) | Standard role; anti-forgery token | `Rki1`, `Admin1`, anonymous: GET and POST 302 to `/Account/Login?ReturnUrl=…`; `Labor1` GET 200; POST without token → HTTP 500 "The required anti-forgery form field … is not present." | Match | Browser | browser run; also test-asserted (added): `Controllers/MeningoIsolateControllerTests.Controller_RequiresStandardRole`, `…EditPost_ValidatesAntiForgeryToken` |
| FR-023 (binding) · SEC-014 | (protection implied) | Crafted: `ReportStatus=Final` stored on isolate 9; POST from isolate 9's form with `MeningoIsolateId=8` and laboratory number 004/26 overwrote isolate 8 | Defect confirmed | DB | DB query of `MeningoIsolates` |
| FR-024 · EC1 | No id → bad request; unknown → not found | `/MeningoIsolate/Edit` 400, `/MeningoIsolate/Edit/99999` 404, `/MeningoIsolate/Edit/abc` 400 | Spec deviation | Browser | browser run; also test-asserted (added): `…Edit_WithoutId_ReturnsBadRequest`, `…Edit_UnknownId_ReturnsNotFound` |
| FR-025 · SC-007 | No task planning | Navigation and form offer no assignment or planning | Match | Browser | browser run |
| New EC "Isolate of a deleted submission" (020) | (020, derived) Pages stay reachable | `/MeningoIsolate/Edit/3` (soft-deleted seed submission) → 200 with the full form; not saved | Match | Browser | browser run |

## Unit tests added

- `HaemophilusWebTests/Controllers/MeningoIsolateControllerTests.cs` (new, 4 cases, own empty isolate set):
  `Edit_WithoutId_ReturnsBadRequest`, `Edit_UnknownId_ReturnsNotFound` (FR-024), `Controller_RequiresStandardRole`,
  `EditPost_ValidatesAntiForgeryToken` (FR-023). Added to `HaemophilusWebTests.csproj`.
- `HaemophilusWebTests/Validators/MeningoIsolateViewModelValidatorTests.cs` (extended, 12 cases):
  `Validate_WellFormedLaboratoryNumber_IsValid` (2), `Validate_MalformedLaboratoryNumber_FailsOnLaboratoryNumber` (4)
  (FR-008), `Validate_SeveralRealTimePcrResults_IsValid` (FR-013), `Validate_PositivePorAFetAAndCswyPcrWithoutDetails_IsValid`
  (EC7), invalid models for oxidase without growth (FR-010), 16S, Biotyper and VITEK without details (FR-013).
- `HaemophilusWebTests/Automapper/MeningoIsolateViewModelMappingActionTests.cs` (extended, 2 cases):
  `ProcessViewModelToModel_AnyGrowth_KeepsMaterial` (FR-011).
- No tests for SEC-014, DATA-032 to DATA-038, UX-011, UX-013, UX-014 (defects). The derivation runs in the page script
  and has no unit test (REF-019).

## Spec corrections applied

- US2.4, new US2.5, FR-006: row order, breakpoint preselection and reset, removing a measurement
  (`[POTENTIAL-REFACTORING]` REF-019; was: rows without order, no preselection).
- US3.1, FR-020: database and PubMLST id are hidden values; message and link texts (was: displayed read-only).
- US3.2, FR-021: record found by PubMLST id, database name updated, link text after reopening.
- US3.3: lookup sent without stem number (`[SUSPECTED-DEFECT]` UX-013); earlier link stays enabled.
- US3.5, FR-021, SC-005, new Edge Case "Empty PubMLST record on every save without a match" (`[SUSPECTED-DEFECT]`
  DATA-036).
- FR-003: conditional Biotyper details, position of the E-Test rows, date-only "Befund am", VITEK hidden values
  (`[POTENTIAL-REFACTORING]` REF-020), ignored laboratory-number change.
- FR-004, Edge Cases (conditionally displayed fields): RIDOM multiple choice, preselected device, PorA/FetA/cswy details
  not required.
- FR-008, Edge Cases (laboratory number): browser and server messages, unanchored server check (`[SUSPECTED-DEFECT]`
  DATA-032), "NR" display (was: "displayed read-only").
- New Edge Case "Changed laboratory number is ignored" (`[SUSPECTED-DEFECT]` DATA-037); FR-014 and Edge Cases (duplicate
  identifiers) reference it and quote the messages.
- FR-011: "Isolierte DNA" counts as native; new Edge Case "No-growth material change on the default values and never
  undone" (`[SUSPECTED-DEFECT]` DATA-038).
- FR-015, new Edge Case "Errors on both tabs".
- FR-018, new Edge Case "Stale form resets the report state" (`[SUSPECTED-DEFECT]` DATA-034, see 004).
- FR-023: token and binding described separately (`[SUSPECTED-DEFECT]` SEC-014, see 004); HTTP 500 without token.
- FR-024, Edge Cases (unknown id): bad request also for a non-numeric id.
- SC-003, FR-006, new Edge Case "E-Test result taken from the request" (`[SUSPECTED-DEFECT]` DATA-035, see 004).
- Edge Cases (growth, serogenogroup, conditional details): message texts; VITEK rule concerns stored values only.
- New Edge Cases "Non-numeric stem number", "Label and message slips" (`[SUSPECTED-DEFECT]` UX-014), "Every save
  rewrites the E-Tests" (DATA-033), "Free E-Test row without a measurement" (UX-011), "Isolate of a deleted submission"
  (see 020).

## Backlog updates

- SEC-014, DATA-032, DATA-033, DATA-034, DATA-035, UX-011, REF-019, REF-020: confirmed on system 2026-11-08 for
  Meningococci isolates; 005 added to the sources. The owner decisions on DATA-035 and REF-020 are unchanged.
- NEW DATA-036: every Meningococci isolate save without a current PubMLST match stores an empty PubMLST record.
- NEW DATA-037: Meningococci isolate form ignores a changed laboratory number.
- NEW DATA-038: Meningococci no-growth material change fires on the default values and is never undone (Offen).
- NEW UX-013: PubMLST lookup without stem number asks the remote databases for "DE -".
- NEW UX-014: Meningococci isolate form label and message slips.

## Not verifiable here

- FR-019 (online), US3.4, EC9: whether a match appears only after PubMLST publishes uploaded data needs the external
  PubMLST and IRIS databases, which the verification must not call. The individual lookup was exercised with
  intercepted requests only; 019 covers the remote side from code.
- Blocked or locally answered requests: `POST /PubMlst/NeisseriaIsolates/` with `isolateReference=DE -`, `DE8001`
  (aborted) and `DE7001`, `DE8001` (answered in the browser with a synthetic record); `POST /Report/QueryHealthOffice/`
  with `postalCode=97080` from the Meningococci report page (aborted). None reached the server.

## Open questions for the owner

- DATA-038 (`DEC-*` candidate, clinical semantics): should the submission material follow the growth fields in both
  directions, and should the growth fields keep "Nein" as their default?
- DATA-036: should a failed lookup before a save keep the stored match (the form gives no hint that the save drops it)?
  Part of the fix, not a separate decision, unless the owner wants failed lookups to clear matches on purpose.
- Own records left behind: submissions `SV-005-A` to `SV-005-F` (isolate 8 carries the remark from the SEC-014 check
  and report status "Endbefund", isolate 9 status "Endbefund", isolate 6 status "Endbefund"), breakpoint
  `SV-005-Cip-alt`, about 20 empty PubMLST records and the synthetic PubMLST record 99001. No seeded record was
  changed.
