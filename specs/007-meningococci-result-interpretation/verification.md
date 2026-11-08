# Verification: Meningococci result interpretation ruleset

**Spec**: 007 · **Date**: 2026-11-08 · **Build**: `8c7c162` · **Host**: ScreenshotTests
SpecVerificationHost (`http://localhost:9899`, catalog `HaemophilusWeb_Screenshots`) ·
**Accounts used**: `Standard1` · **Scope**: full (unit-test path per `AGENTS.md`, browser check of the report page and the
laboratory and RKI exports)

## Summary

| Verdict | Count |
| --- | --- |
| Match | 22 |
| Spec deviation | 2 |
| Defect confirmed | 2 |
| New defect | 1 |
| Not verifiable | 0 |

By primary path: Browser 2 · File 3 · DB 0 · Unit test (existing) 5 · Unit test (added) 13 · Code 4 · Not verifiable 0
(adds up to the row count). Also test-asserted: 3 rows with a browser or file verdict. Test run:
`vstest.console.exe … /TestCaseFilter:` `MeningoIsolateInterpretationTests`, `MeningoIsolateViewModelMappingActionTests`,
`EpiscanGisExportControllerTests`, `MeningoSendingLaboratoryExportTests`, `MeningoStateAuthorityExportTests` → 302
passed, 0 failed (249 of them in `MeningoIsolateInterpretationTests`, 136 cases added there, plus 1 each in the mapping
and EpiScanGIS fixtures); full suite → 1053 passed, 0 failed.

Edge cases are numbered in the order of the spec's Edge Cases list before this verification: EC1 material selects the
rule family, EC2 native exact criteria and cultured wildcards, EC3 rules 20/22/24/26 before 08-11, EC4 removed rule 39,
EC5 multi-organism real-time result, EC6 native 30a/30b shadowed, EC7 row order of native 10/17, EC8 empty report on a
match, EC9 rule 32 flag, EC10 rules 11/14/41 without comment, EC11 interpreter reuse, EC12 MALDI-TOF, EC13 rules without
invasive criterion, EC14 serogroup conflict unreachable.

Own records created through the UI (seeded sender 1, synthetic patients 11–13): `SV-007-A` (MZ007/26, isolate 11,
"Nativmaterial", "Blut", every molecular test "negativ"), `SV-007-B` (MZ008/26, isolate 12, "Nativmaterial", "Blut",
csb/csc/PorA/FetA/Real-Time-PCR "negativ", rest "n.d."), `SV-007-C` (MZ009/26, isolate 13, "Vitaler Stamm",
"Nasenabstrich", saved first with the rule 41 and then with the rule 11 combination). The six unmatched submissions
MZ001/26–MZ006/26 of the 005 verification were read only. No seeded record was changed. Rule identifiers of saved
records were read from the "Regel" column of the laboratory export (`verification/007-lab-export-2026-11.xlsx`).

## Results

| ID | Spec says | App/test shows | Verdict | Path | Evidence |
| --- | --- | --- | --- | --- | --- |
| FR-001 · EC1 | Native material and isolated DNA use native rules; vital strain and "not grown" the cultured rules | One isolate that satisfies cultured rule 02 and native rule 01a selects `StemInterpretation_02` for "Vitaler Stamm"/"Nicht angewachsen" and `NativeMaterialInterpretation_01a` for "Nativmaterial"/"Isolierte DNA"; on the system the native `SV-007-A`/`-B` got native rules 32/06 | Match | Unit test (added) | `Domain/MeningoIsolateInterpretationTests.Interpret_Material_SelectsRuleFamily`; `verification/007-lab-export-2026-11.xlsx` |
| FR-002 · US4.2 | Rules in declaration order, first complete match wins | Representative inputs that also satisfy later rules (native 10 → also 11, 17 → also 18; cultured 20 → also 09) select the earlier rule | Match | Unit test (added) | `Interpret_RepresentativeInput_SelectsDocumentedRule` (78 cases) |
| FR-003 · EC13 | Cultured criteria evaluated; omitted criteria other than blood-agar growth do not restrict | Rule 03 matches every oxidase/ONPG/gamma-GT combination (27), rule 08 every oxidase value, NoNM_01/NoNM_02 both invasive and non-invasive submissions; every cultured rule configures blood-agar growth | Match | Unit test (added) | `Interpret_CulturedRuleOmittingOxidaseOnpgAndGammaGt_AcceptsAnyValue`, `…OmittingOxidase_AcceptsAnyOxidase`, `…OmittingInvasiveStatus_MatchesInvasiveAndNonInvasive`, `Rulesets_DeclareTheDocumentedRulesAndCriteria` |
| FR-004 · EC2 · EC5 | Native criteria exact; omitted best match is a wildcard, omitted allele/organism requires the unset value; combined organisms match no rule | Rule 06 inputs with any cswy allele or any organism result → no rule, discrepancy line; rule 19 accepts any best match; every native rule configures the seven list criteria; combined organisms → discrepancy (existing) | Match | Unit test (added) | `Interpret_NativeRuleOmittingAllele_DoesNotMatchASetAllele`, `…OmittingOrganismResult_DoesNotMatchASetOrganism`, `…OmittingBestMatch_AcceptsAnyBestMatch`; existing `IsolateMatchingNativeMaterialRule7ButWithMultipleRealTimePcrResults_ReturnsInconclusive` |
| FR-005 | Each run clears typings, rule, serogroup, flag and resets the report | A reused instance after a rule 02 match and an unmatched run: report is exactly the discrepancy line, typings empty, rule and serogroup null, flag false | Match | Unit test (added) | `Interpret_ReusedInstanceWithoutMatch_ClearsStateButKeepsPreviousComment`; existing `FirstNonEmptyThenEmptyIsolate_ClearsSerogroup`, `FirstMatchThenNoMatch_ClearsNoMeningococci` |
| FR-006 · US3.3 · EC8 | A match replaces the report, also with an empty collection, and exposes the rule | Cultured 12, 13, 25, 26 return an empty report; every representative input exposes its rule identifier | Match | Unit test (existing) | `IsolateMatchingStemRule12And13_…`, `IsolateMatchingStemRule23to26_…` (`Report.Should().BeEmpty()`); also (added) `Interpret_RepresentativeInput_SelectsDocumentedRule` |
| FR-007 · US3.1 · EC7 | "Identifikation" first, then declared order; native rows in declared order (10 vs 17 differ) | Exact attribute sequence asserted for all 78 reachable rules (e.g. native 10: 16S, molecular typing, best match; 17: 16S, best match, molecular typing); MZ004/20 report data: "Identifikation", "Serogruppe", "Serogenogruppe", "PorA - Sequenztyp", "FetA - Sequenztyp" | Match | Unit test (added) | `Interpret_RepresentativeInput_SelectsDocumentedRule`; `verification/US1-MZ004-20-report.png` |
| FR-008 · US3.2 | Displayed enum values, PorA/FetA VRs, MALDI-TOF and 16S best match, real-time device, molecular-typing statement substituted | Existing tests assert "X, Y"/"Z", MALDI best matches, 16S best match, "NHS Real-Time-PCR (Sacace)", molecular statements, enum texts ("negativ"); on the system "SV1, SV2"/"SV3", "NSH Real-Time-PCR (BD-MAX)", "16S-rDNA-Nachweis" "negativ" | Match | Unit test (existing) | `IsolateMatchingStemRule2_28to32_…`, `…Rule4And5Vitek/Biotyper_…`, `…NativeMaterialRule10_11_17or18_…`, `…NativeMaterialRule29To31_…`; `verification/EC10-SV-007-C-rule41.png`, `verification/report-SV-007-A-rule32.png` |
| FR-009 · US3 | Report lines, typings, comment for the report; rule, serogroup, flag for consumers | Report page of MZ004/20: "Befund" shows both report lines; embedded report data carries `Report`, `Typings`, `Comment`; laboratory export carries "Regel", "Serogruppe", "Meningokokken" | Match | Browser | `verification/US1-MZ004-20-report.png`; also test-asserted (added) `Automapper/MeningoIsolateViewModelMappingActionTests.ProcessModelToViewModel_CopiesReportTypingsAndCommentFromInterpretation` |
| FR-010 (configured rules) | Rules configured as no Meningococci set the flag | Flag asserted for every representative input except rule 32; laboratory export of `SV-007-B` (rule 06): "kein Nachweis" | Match | Unit test (existing) | `AssertNoMeningococciFlagIsValid` in every rule test; also (added) `Interpret_RepresentativeInput_SelectsDocumentedRule`; `verification/007-lab-export-2026-11.xlsx` |
| FR-010 (rule 32) `[SUSPECTED-DEFECT]` · EC9 · DEC-005 | Rule 32 reports no evidence but leaves the flag false | `SV-007-A`: report "Meningokokken- spezifische DNA konnte nicht nachgewiesen werden." / "Kein Hinweis auf Neisseria meningitidis." with the sequencing comment; laboratory export "Regel" `NativeMaterialInterpretation_32`, "Meningokokken" empty; RKI export contains it (patient 11) but not `SV-007-B` (patient 12, rule 06, same report wording). Observation only, no decision | Defect confirmed | File | `verification/007-rki-export-2026-11.xlsx`, `verification/007-lab-export-2026-11.xlsx`, `verification/report-SV-007-A-rule32.png`; also test-asserted (existing flag check of `IsolateMatchingNativeMaterialRule32_ReturnsCorrespondingRule`) |
| FR-011 · US4.1 · SC-002 · EC4 | No match: discrepancy line only, no typings, null rule and serogroup, flag false | Exactly these values (and no comment) for a cultured and a native unmatched input; report page of `SV-007-A` before entering results: "Befund" "Diskrepante Ergebnisse, bitte Datenbankeinträge kontrollieren.", `Typings` empty, `Comment` null (the page shows no dialog) | Match | Unit test (added) | `Interpret_NoRuleMatches_KeepsDiscrepancyDefaults`; existing `IsolateMatchingStemRule39_ReturnsInconclusiveAsRuleWasRemoved`; `verification/US4-1-discrepancy-SV-007-A.png` |
| US1.3 · New EC "No growth with negative molecular results" | A strain that did not grow and has no positive molecular evidence gets the no-cultivation report | Only all-not-determined tests reach rules 01/27; no growth with negative serogroup PCR and non-amplified PorA/FetA matches no rule (discrepancy). Spec corrected | Spec deviation | Unit test (added) | `Interpret_NoRuleMatches_KeepsDiscrepancyDefaults("no growth, negative molecular results")` |
| FR-017 · SC-001 · SC-005 · US1.1 · US1.2 · US1.4 · US2.1–US2.3 | Both decision tables; every rule reproduces its outputs | One input per reachable rule (44 cultured, 34 native; 30a/30b unreachable) selects that rule with the tabled typing order, report line count, comment, flag and serogroup; MZ004/20 (invasive, typical, "auto", PCR "W/Y", PorA/FetA positive) shows the rule 08 output | Match | Unit test (added) | `Interpret_RepresentativeInput_SelectsDocumentedRule`; existing per-rule tests; `verification/US1-MZ004-20-report.png` |
| FR-012 · EC11 · REF-011 (comment retention) | Fresh instance: rule comment or empty; reused unmatched run keeps the previous comment | Unmatched fresh run: comment null; after a rule 02 match an unmatched run keeps "Die technische Durchführung der Sequenzierung erfolgte durch den Unterauftragnehmer Microsynth Seqlab." | Match | Unit test (added) | `Interpret_ReusedInstanceWithoutMatch_ClearsStateButKeepsPreviousComment`, `Interpret_NoRuleMatches_KeepsDiscrepancyDefaults` |
| FR-013 · Assumption (new interpreter per report) | The report view model receives report lines, typings and comment | View model values equal those of a fresh interpretation of the same isolate (rule 02, with comment); the report page shows them | Match | Unit test (added) | `ProcessModelToViewModel_CopiesReportTypingsAndCommentFromInterpretation`; `verification/US1-MZ004-20-report.png` |
| FR-014 (laboratory, state-authority, EpiScanGIS) | Laboratory export: "kein Nachweis", rule; laboratory, state-authority and EpiScanGIS exports use the normalized serogroup; EpiScanGIS skips empty, `NG`, `cnl` | Asserted by existing export tests; EpiScanGIS rows interpreted as `NG` and `cnl` are skipped (added); the laboratory export of November 2026 shows "Regel" and "Meningokokken" as stated | Match | Unit test (existing) | `Tools/MeningoSendingLaboratoryExportTests.DataTable_ContainsNoMeningococci`, `…ContainsInterpretedSerogroup`, `Tools/MeningoStateAuthorityExportTests.DataTable_ContainsInterpretedSerogroup`; (added) `Controllers/EpiscanGisExportControllerTests.DownloadWithEntriesInterpretedAsNgAndCnl_ReturnsTwoLinesLess` |
| FR-014 (RKI/IRIS selection) | RKI and IRIS select invasive submissions with flag false | RKI export 01.11.–30.11.2026: 7 of the 8 invasive submissions; only `SV-007-B` (flag set) is missing | Match | File | `verification/007-rki-export-2026-11.xlsx` |
| FR-014 (one interpretation per submission) `[POTENTIAL-REFACTORING]` | Each export runs the interpretation anew per submission | The laboratory export calls `Interpret` in each of its three derived columns (`MeningoSendingLaboratoryExport`, rule/flag/serogroup getters), i.e. three times per row. Spec extended, REF-011 extended | Spec deviation | Code | `HaemophilusWeb/Tools/MeningoSendingLaboratoryExport.cs` (lines 108, 114, 120) |
| New EC "Unmatched results count as Meningococci found" | (not stated) | Unmatched invasive submissions (empty "Regel") MZ001/26–MZ006/26 appear in the RKI export as cases | New defect | File | `verification/007-rki-export-2026-11.xlsx`, `verification/007-lab-export-2026-11.xlsx`; also test-asserted (added) flag false in `Interpret_NoRuleMatches_KeepsDiscrepancyDefaults` |
| FR-015 · FR-016 · US3.4 · SC-003 | Serogroup derived after typings; `NG`, `cnl`, suffix removal, molecular statement override, no-agglutination `NG` | B, C, E, X, W, Y, W/Y, `NG` (non-groupable, poly, auto, no agglutination) and `cnl` asserted per rule; molecular statements give B/C/W/Y/W/Y | Match | Unit test (existing) | per-rule tests (`Serogroup.Should()`); also (added) `Interpret_RepresentativeInput_SelectsDocumentedRule` |
| EC14 | Agglutination/serogenogroup conflict path unreachable | No cultured rule emits the plain "Agglutination" template together with a serogenogroup template | Match | Unit test (added) | `Rulesets_DeclareTheDocumentedRulesAndCriteria` |
| EC12 | MALDI-TOF "not determined" needs both devices, "determined" either; Biotyper row when Biotyper determined | Rule 06 inputs with either or both devices determined → no rule; rule 04 with VITEK only → VITEK row, Biotyper only or both → Biotyper row | Match | Unit test (added) | `Interpret_MaldiTofNotDeterminedRule_RequiresBothDevicesNotDetermined`, `Interpret_MaldiTofDeterminedRule_AcceptsEitherDeviceAndPrefersBiotyperRow` |
| EC10 `[SUSPECTED-DEFECT]` · DATA-018 (11/14/41) · DEC-005 | Rules 11, 14, 41 emit PorA/FetA sequence rows without the comment | `SV-007-C` with the rule 41 and the rule 11 combination: report data with "PorA - Sequenztyp" "SV1, SV2" and "FetA - Sequenztyp" "SV3", `Comment` null (rule 14 not checked on the system) | Defect confirmed | Browser | `verification/EC10-SV-007-C-rule41.png`, `verification/EC10-SV-007-C-rule11.png` |
| EC3 `[POTENTIAL-REFACTORING]` · DATA-018 (rule 09 cnl) · REF-011 | Rules 20/22/24/26 precede 08-11; rule 09's `cnl` case never selected; source comment names 10 and 12 | `StemRules.json` declares 20, 22, 24, 26 first with the comment "Rule 20, 22, 24, and 26 are more strict then 10 and 12"; rule 09 differs from 20 only by its wider PCR list | Match | Code | `HaemophilusWeb/Domain/Interpretation/StemRules.json`; also test-asserted (added): the rule 20 input selects rule 20 |
| EC6 `[POTENTIAL-REFACTORING]` · DEC-005 · DATA-018 (30a/30b) | Native 30a/30b repeat the criteria of 01b/01a and are never selected | Criteria identical in `NativeMaterialRules.json`; the existing "30a"/"30b" test cases select 01b (TEST-002) | Match | Code | `HaemophilusWeb/Domain/Interpretation/NativeMaterialRules.json` |
| Assumption (rule order) `[POTENTIAL-REFACTORING]` · REF-011 | Evaluation order relies on the keyed collections preserving declaration order | Rules are deserialized into `Dictionary<string, …>` and searched with `FirstOrDefault`; every first-match expectation of the added tests holds | Match | Code | `HaemophilusWeb/Domain/MeningoIsolateInterpretation.cs` (`StemInterpretationRules`, `NativeMaterialInterpretationRules`) |

## Unit tests added

- `HaemophilusWebTests/Domain/MeningoIsolateInterpretationTests.cs` — 136 cases:
  `Interpret_RepresentativeInput_SelectsDocumentedRule` (78; SC-001, FR-002, FR-006, FR-007, FR-009, FR-010, FR-015, FR-016),
  `Interpret_Material_SelectsRuleFamily` (4; FR-001),
  `Interpret_CulturedRuleOmittingOxidaseOnpgAndGammaGt_AcceptsAnyValue` (27; FR-003),
  `Interpret_CulturedRuleOmittingOxidase_AcceptsAnyOxidase` (3; FR-003),
  `Interpret_CulturedRuleOmittingInvasiveStatus_MatchesInvasiveAndNonInvasive` (4; FR-003, EC13),
  `Interpret_NativeRuleOmittingAllele_DoesNotMatchASetAllele` (3; FR-004),
  `Interpret_NativeRuleOmittingOrganismResult_DoesNotMatchASetOrganism` (3; FR-004),
  `Interpret_NativeRuleOmittingBestMatch_AcceptsAnyBestMatch` (3; FR-004),
  `Interpret_MaldiTofNotDeterminedRule_RequiresBothDevicesNotDetermined` (3; EC12),
  `Interpret_MaldiTofDeterminedRule_AcceptsEitherDeviceAndPrefersBiotyperRow` (3; EC12, FR-008),
  `Interpret_NoRuleMatches_KeepsDiscrepancyDefaults` (3; FR-005, FR-011, US1.3),
  `Interpret_ReusedInstanceWithoutMatch_ClearsStateButKeepsPreviousComment` (1; FR-005, FR-012),
  `Rulesets_DeclareTheDocumentedRulesAndCriteria` (1; SC-001, FR-003, FR-004, EC14).
- `HaemophilusWebTests/Automapper/MeningoIsolateViewModelMappingActionTests.cs`:
  `ProcessModelToViewModel_CopiesReportTypingsAndCommentFromInterpretation` (FR-013).
- `HaemophilusWebTests/Controllers/EpiscanGisExportControllerTests.cs`:
  `DownloadWithEntriesInterpretedAsNgAndCnl_ReturnsTwoLinesLess` (FR-014).

Not asserted (DEC-005 / DATA-018): the comment of cultured rules 11, 14 and 41, the flag of native rule 32, and the
shadowed native rules 30a/30b (no case). No test was added for DATA-039.

## Spec corrections applied

- US1.3: the no-cultivation outcome needs all identification and molecular tests not determined (was: "has no positive
  molecular evidence"); new edge case "No growth with negative molecular results".
- FR-014: the laboratory export runs the interpretation three times per row, tagged `[POTENTIAL-REFACTORING]` (was:
  "runs the interpretation anew per submission" only).
- Edge case rule 32: the focused test asserts the flag but not the rule identifier (was: "the focused test asserts this
  value"); observed export inclusion and the sequencing comment without a sequence row added (coverage and observation
  note, no row of its own).
- New edge case "Unmatched results count as Meningococci found" `[SUSPECTED-DEFECT]` (DATA-039).

## Backlog updates

- DEC-005: observed on system 2026-11-08 (rule 32 in the RKI export with empty "Meningokokken", rule 06 with the same
  report wording excluded; rule 32 carries the sequencing comment without a sequence row). No decision taken.
- DATA-018: confirmed on system 2026-11-08 for rules 41 and 11.
- NEW DATA-039: unmatched Meningococci interpretation counts as "Meningococci found" in the RKI/IRIS selection.
- REF-011: extended with the threefold interpretation per row in the laboratory export; source "007 FR-014" added.
- TEST-002: extended with the 007 test weaknesses (rule not asserted, duplicated "30b"/"31b" inputs, random inputs);
  source "007 verification" added.

## Not verifiable here

- None. The interpretation needs no external service; the RKI health-office lookup the report page starts on load was
  blocked.

## Open questions for the owner

- DATA-039 (`DEC-*` candidate): should invasive submissions without a matching rule (discrepant or not yet entered
  results) be left out of the RKI and IRIS exports, flagged, or kept? Today they count as Meningococci found.
- DEC-005 (open, not decided here): rule 32 also carries the sequencing-provider comment without any sequence row.
  Part of the same decision, or separate?
- US1.3 coverage: a strain that did not grow with negative serogroup PCR and non-amplified PorA/FetA gets the
  discrepancy report. Intended (needs manual review) or a missing rule?

Blocked external calls: `POST /Report/QueryHealthOffice/` on every load of the report page (7 loads, all aborted in the
browser). None reached the server.
