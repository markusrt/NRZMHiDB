# Verification: Haemophilus result interpretation engine

**Spec**: 006 · **Date**: 2026-11-08 · **Build**: `04d40da` · **Host**: ScreenshotTests
SpecVerificationHost (`http://localhost:9899`, catalog `HaemophilusWeb_Screenshots`) ·
**Accounts used**: `Standard1` · **Scope**: full (unit-test path per `AGENTS.md`, browser check of the report page)

## Summary

| Verdict | Count |
| --- | --- |
| Match | 25 |
| Spec deviation | 0 |
| Defect confirmed | 2 |
| New defect | 0 |
| Not verifiable | 0 |

By primary path: Browser 7 · File 0 · DB 0 · Unit test (existing) 6 · Unit test (added) 14 · Not verifiable 0 (adds up
to the row count). Also test-asserted: 5 rows with a browser verdict. Test run:
`vstest.console.exe … /TestCaseFilter:` `IsolateInterpretationTests`, `IsolateViewModelMappingActionTests`,
`ReportControllerTests` (Haemophilus classes only) → 266 passed, 0 failed (248 of them in `IsolateInterpretationTests`,
215 cases added there plus 1 in the mapping fixture); full suite → 915 passed, 0 failed.

Edge cases are numbered in the order of the spec's Edge Cases list before this verification: EC1 negative agglutination
always assigns a disclaimer, EC2 specific agglutination with bexA not determined and matching PCR, EC3 specific
agglutination unmatched, EC4 branches mutually exclusive, EC5 auto/poly/non-evaluable agglutination, EC6 growth "not
stated", EC7 evaluation only in the no-growth branch, EC8 invasive status only affects the disclaimer, EC9 isolate
without submission. "Row n" is the n-th data row of the Decision Table.

The submission `SV-006-A` (KL001/26, isolate 4, sampling location "Blut", seeded sender 1) was created through the UI;
its isolate was then saved through "Änderungen speichern und Befund erstellen" with the typing combinations named in the
rows. Report generation from that isolate set its report status (own record). No seeded record was changed.

## Results

| ID | Spec says | App/test shows | Verdict | Path | Evidence |
| --- | --- | --- | --- | --- | --- |
| FR-001 · US1 | Interpretation derived on every isolate-to-view-model conversion; document generation reuses the page values; isolate not altered | Report page `/Report/Isolate/4` shows the texts for the saved typing values after each save; the generated report contains the page values, and the only requests besides the template download are the blocked `QueryHealthOffice` and `ReportGenerated`. The edit-page call has no visible effect (values not displayed) | Match | Browser | `verification/US2-1-report-negative-nd-nd.png`; also test-asserted: `Controllers/ReportControllerTests.Isolate_ValidModel_FillsInterpretationProperties`, (added) `Automapper/IsolateViewModelMappingActionTests.ProcessModelToViewModel_CopiesInterpretationFieldsAndLeavesCommentEmpty`, `Domain/IsolateInterpretationTests.Interpret_ProducesNoReportArrayNorComment_AndLeavesIsolateUnchanged` |
| FR-002 · SC-006 | Only agglutination, bexA, serotype PCR, growth, evaluation and invasive status are read | An isolate with random values in every other field gives the same three texts as a plain isolate with the same six inputs (5 combinations) | Match | Unit test (added) | `Domain/IsolateInterpretationTests.Interpret_FieldsOtherThanTheSixInputs_DoNotChangeTheResult` |
| FR-003 | Value sets; new isolate starts with typing "not determined", growth "not stated" | New isolate defaults asserted; the edit form offers agglutination "n.d.", "a"–"f", "negativ", "auto", "poly", "Nicht beurteilbar", bexA "n.d."/"negativ"/"positiv", serotype PCR "n.d.", "a"–"f", "negativ", "Wachstum" "Nein"/"Ja"/"keine Angabe" | Match | Unit test (added) | `Domain/IsolateInterpretationTests.NewIsolate_StartsWithTypingNotDeterminedAndGrowthNotStated`; edit form of isolate 4 |
| FR-004 · EC6 | Both interpretations start as the discrepancy text, disclaimer empty; all typing "n.d." with growth "not stated" keeps them | All three fields asserted exactly | Match | Unit test (added) | `Domain/IsolateInterpretationTests.Interpret_AllTypingNotDetermined_GrowthNotStated_IsDiscrepantWithEmptyDisclaimer`; also `Interpret_EmptyIsolate_IsUndetermined` (final and disclaimer only) |
| FR-005 · US1.1 · row 1 | Exact unencapsulated text, preliminary discrepant | Exact text asserted (the existing test checked only the fragment "nicht-typisierbar") | Match | Unit test (added) | `Domain/IsolateInterpretationTests.Interpret_NegativeAgglutinationAndBexA_NegativeOrNotDeterminedPcr_IsExactNotTypableText`; browser `verification/FR-017-dialog-Fax_Teilbefund_Serotyp_v4_docx.png` (case negativ/negativ/n.d.) |
| FR-006 · US1.2 · row 2 | Phenotypically non-typable text with `{pcr}` = displayed PCR value | Exact text for PCR a–f; the report page shows "… des Serotyps c wird nicht exprimiert." | Match | Unit test (existing) | `Domain/IsolateInterpretationTests.Interpret_NegativeAgglutinationButDistinctSerotypeWithNegativeBexA_IsSpecificTyping` |
| FR-006 `[SUSPECTED-DEFECT]` · DATA-017 | Produced text spells "phänotpischen" | Report page "Befund": "Die Ergebnisse sprechen für einen phänotpischen nicht-typisierbaren Haemophilus influenzae (NTHi). …" | Defect confirmed | Browser | `verification/FR-006-DATA-017-typo.png`; also test-asserted (existing test asserts the typo) |
| FR-007 · US2.1 · row 3 | Preliminary unencapsulated text; final adds "Eine molekularbiologische Typisierung …" | "Teilbefund" and "Befund" show exactly these texts | Match | Unit test (existing) | `Domain/IsolateInterpretationTests.Interpret_OnlyNegativeAgglutination_TypingNotEfficient`; browser `verification/US2-1-report-negative-nd-nd.png` |
| FR-008 · US3.1 · US3.3 · EC1 | Every negative-agglutination combination gets the unencapsulated reporting disclaimer or the non-invasive note | Exact disclaimer for all bexA × serotype-PCR × sampling-location combinations (96 cases); report data of isolate 4 carries "… Meldekategorie dieses Befundes: Haemophilus influenzae, unbekapselt." | Match | Unit test (added) | `Domain/IsolateInterpretationTests.Interpret_NegativeAgglutination_AnyBexAAndPcr_AssignsDisclaimerBySamplingLocation` |
| Row 4 · EC1 | Negative agglutination with positive bexA, or bexA "n.d." with a determined PCR value: both discrepant, disclaimer assigned | As stated (6 combinations) | Match | Unit test (added) | `Domain/IsolateInterpretationTests.Interpret_NegativeAgglutination_PositiveBexAOrUndeterminedBexAWithPcrValue_IsDiscrepant` |
| FR-009 · US1.3 · row 5 | Specific agglutination, bexA positive, matching or "n.d." PCR: serotype text, preliminary discrepant | Exact text for a–f, both PCR variants (12 cases) | Match | Unit test (existing) | `Domain/IsolateInterpretationTests.Interpret_DistinctAgglutinationWithPositiveBexA_EitherMatchingOrNotDeterminedSerotype_IsSpecificTyping` |
| FR-010 · US2.2 · row 6 | Specific agglutination, bexA and PCR "n.d.": preliminary serotype text, final discrepant | Exact text for a–f; report page for "a": "Teilbefund" "Das Ergebnis spricht für eine Infektion mit Haemophilus influenzae des Serotyp a (Hia).", "Befund" discrepancy text | Match | Unit test (existing) | `Domain/IsolateInterpretationTests.Interpret_DistinctAgglutinationWithNotDeterminedBexA_NotDeterminedSerotype_IsSpecificPreliminaryTyping`; browser `verification/US2-2-report-a-nd-nd.png` |
| FR-011 · US3.2 · SC-002 | `{serotype}` = displayed agglutination value in text and category | Category "Haemophilus influenzae, Serotyp x" for a–f in both specific rows | Match | Unit test (existing) | FR-009/FR-010 tests above; exact disclaimer in (added) `Interpret_SamplingLocation_ChangesOnlyTheDisclaimer` |
| Row 7 · EC2 | Specific agglutination, bexA "n.d.", matching PCR: both discrepant, serotype disclaimer | As stated for a–f | Match | Unit test (added) | `Domain/IsolateInterpretationTests.Interpret_SpecificAgglutinationUndeterminedBexA_MatchingPcr_IsDiscrepantWithSerotypeDisclaimer` |
| FR-012 `[SUSPECTED-DEFECT]` · US1.4 · US3.4 · row 8 · DEC-004 | All typing "n.d." and growth "Ja" yields the could-not-cultivate text and the telephone note | Report page: "Befund" "Der eingesendete Stamm konnte nicht angezüchtet werden. Um Wiedereinsendung wird gebeten.", disclaimer "Eine telefonische Vorabmitteilung ist erfolgt." (observation only, no decision) | Defect confirmed | Browser | `verification/FR-012-DEC-004-growth-ja.png`; also test-asserted (existing) `Interpret_AgglutinationBexAAndSerotypAreNotDeterminedButGrowthIsYes_WithRespectiveSeroType_RequestForResend` |
| FR-013 · US1.5 · row 9 | Growth "Nein": "Kein Nachweis von Haemophilus influenzae.", disclaimer names the evaluation | As stated; on the system the preselected evaluation "NTHi" yields "Beim eingesendeten Isolat handelt es sich am ehesten um NTHi." (recorded under DEC-004) | Match | Unit test (existing) | `Domain/IsolateInterpretationTests.Interpret_AgglutinationBexAAndSerotypAreNotDetermined_ButGrowthIsNo_ReportEvaluation`; browser `verification/FR-013-DEC-004-growth-nein.png` |
| FR-014 · EC3 · EC4 · EC5 · SC-004 | Unmatched combinations keep the defaults; no closest result | Specific agglutination with negative bexA or a conflicting PCR value (7 cases) and auto/poly/non-evaluable with every bexA and PCR value (72 cases): both discrepant, disclaimer empty | Match | Unit test (added) | `Domain/IsolateInterpretationTests.Interpret_SpecificAgglutination_UnmatchedCombination_IsDiscrepantWithEmptyDisclaimer`, `…Interpret_AutoPolyOrNotEvaluableAgglutination_IsDiscrepantWithEmptyDisclaimer` |
| EC7 | Evaluation affects only the all-"n.d."/growth "Nein" outcome | Every evaluation value gives identical output in 7 other combinations, incl. growth "Ja" and "keine Angabe" | Match | Unit test (added) | `Domain/IsolateInterpretationTests.Interpret_OutsideNoGrowthBranch_EvaluationDoesNotChangeTheResult` |
| SC-003 · EC8 · Assumption (invasive derivation) | Sampling location changes only the disclaimer; blood, liquor, other invasive are invasive | Interpretations identical for all four locations; disclaimer invasive for "Blut", "Liquor", "Anderer invasiv", non-invasive note for "Anderer nicht invasiv" (6 combinations) | Match | Unit test (added) | `Domain/IsolateInterpretationTests.Interpret_SamplingLocation_ChangesOnlyTheDisclaimer`; also `Automapper/IsolateViewModelMappingActionTests.ProcessModelToViewModel_PopulatesInvasive` |
| EC9 · Assumption (missing submission) | Negative and specific branches fail without a submission | Both branches throw; the all-"n.d." branch works without one (existing tests) | Match | Unit test (added) | `Domain/IsolateInterpretationTests.Interpret_NegativeOrSpecificBranchWithoutSending_FailsWithAnError` |
| FR-015 | Three fields; no report array, no comment; comment copied as empty, report array not copied | Embedded report data: `Comment` is `null`, no `Report` key; interpreter leaves both unset | Match | Browser | report page of isolate 4 (`documentData`); also test-asserted (added) `Interpret_ProducesNoReportArrayNorComment_AndLeavesIsolateUnchanged`, `ProcessModelToViewModel_CopiesInterpretationFieldsAndLeavesCommentEmpty` |
| FR-016 | Preview shows "Teilbefund" and "Befund"; disclaimer not shown; report data has all three fields | As stated; the disclaimer text is absent from the page but present in `documentData`; the generated preliminary report contains the preliminary text and the disclaimer | Match | Browser | `verification/US2-1-report-negative-nd-nd.png`, `verification/US4-3-Teilbefund-SV-006-A.docx`; also test-asserted (added) mapping test |
| FR-017 · US4.1 · US4.2 · US4.3 · SC-005 | Final templates check "Befund", templates with "Teilbefund - " check "Teilbefund"; dialog "Diskrepante Ergebnisse" with "Nein"/"Ja"; generation only on "Ja" | negativ/negativ/n.d.: "BLNAS v10.docx" downloads at once, "Fax - Teilbefund - Serotyp v4.docx" opens the dialog (title, text and buttons as in the spec), "Nein" closes it without download or `ReportGenerated`. a/n.d./n.d.: "BLNAS v10.docx" opens the dialog, "Ja" downloads "KL 001_26-2026-11-08.docx" and posts `ReportGenerated` (`preliminary=false`); the Teilbefund template downloads at once (`preliminary=true`) | Match | Browser | `verification/FR-017-dialog-Fax_Teilbefund_Serotyp_v4_docx.png`, `verification/FR-017-dialog-final-BLNAS_v10_docx.png` |
| FR-018 · SC-001 | Decision-tree outcomes preserved as documented | Every decision-table row is now asserted by a unit test and matches | Match | Unit test (added) | rows above |
| TEST-003 | Rows 4 and 7, unmatched combinations and the missing-submission failure were untested | Gap confirmed (no such tests existed); closed by the added tests | Match | Unit test (added) | rows 4, 7, FR-014, EC9 above |
| REF-011 (006 part) · Key Entities · Assumptions | Only the obsolete-marked fields are used; always-true guard; TODO in the mapping action | Interpreter sets only `Interpretation`, `InterpretationPreliminary`, `InterpretationDisclaimer` (the first and third marked obsolete; the test build warns CS0618); report array and comment unset | Match | Unit test (added) | `Interpret_ProducesNoReportArrayNorComment_AndLeavesIsolateUnchanged` |
| Assumption (evaluation values) | Displayed evaluation values NTHi, Hia–Hif, H. haemolyticus, H. parainfluenzae, kein Wachstum, keine Haemophilus-Spezies, H. influenzae, Haemophilus sp., nicht H. influenzae, kein H. influenzae | "Beurteilung" offers exactly these 14 values | Match | Browser | edit form of isolate 4 |

## Unit tests added

- `HaemophilusWebTests/Domain/IsolateInterpretationTests.cs`:
  `NewIsolate_StartsWithTypingNotDeterminedAndGrowthNotStated` (FR-003),
  `Interpret_FieldsOtherThanTheSixInputs_DoNotChangeTheResult` (FR-002),
  `Interpret_NegativeAgglutinationAndBexA_NegativeOrNotDeterminedPcr_IsExactNotTypableText` (FR-005),
  `Interpret_NegativeAgglutination_AnyBexAAndPcr_AssignsDisclaimerBySamplingLocation` (FR-008, EC1, EC8),
  `Interpret_NegativeAgglutination_PositiveBexAOrUndeterminedBexAWithPcrValue_IsDiscrepant` (row 4),
  `Interpret_SpecificAgglutinationUndeterminedBexA_MatchingPcr_IsDiscrepantWithSerotypeDisclaimer` (row 7, EC2),
  `Interpret_SpecificAgglutination_UnmatchedCombination_IsDiscrepantWithEmptyDisclaimer` (EC3, FR-014),
  `Interpret_AutoPolyOrNotEvaluableAgglutination_IsDiscrepantWithEmptyDisclaimer` (EC5),
  `Interpret_AllTypingNotDetermined_GrowthNotStated_IsDiscrepantWithEmptyDisclaimer` (EC6, FR-004),
  `Interpret_OutsideNoGrowthBranch_EvaluationDoesNotChangeTheResult` (EC7),
  `Interpret_SamplingLocation_ChangesOnlyTheDisclaimer` (SC-003, FR-011, EC8),
  `Interpret_ProducesNoReportArrayNorComment_AndLeavesIsolateUnchanged` (FR-001, FR-015),
  `Interpret_NegativeOrSpecificBranchWithoutSending_FailsWithAnError` (EC9) — 215 cases.
- `HaemophilusWebTests/Automapper/IsolateViewModelMappingActionTests.cs`:
  `ProcessModelToViewModel_CopiesInterpretationFieldsAndLeavesCommentEmpty` (FR-001, FR-015, FR-016).

No test was added for the growth "Ja"/"Nein" association (DEC-004) or the typo (DATA-017).

## Spec corrections applied

- Decision Table note: updated the test-coverage statement (was: rows 4 and 7 and the unmatched combinations not
  exercised by tests; now: every row asserted, with the tests added by this verification named). Not a deviation of the
  application; the note changed because of the added tests.

## Backlog updates

- DEC-004: observed on system 2026-11-08 (growth "Ja" and "Nein" outputs on the report page; with the preselected
  "Beurteilung" "NTHi" the no-detection disclaimer reads "… am ehesten um NTHi."). No decision taken.
- DATA-017: confirmed on system 2026-11-08.
- TEST-002: extended with the misleading name of the non-invasive preliminary test ("…WithEmptyDisclaimer" asserts the
  non-invasive note) and the fragment-only negative/negative assertion; source "006 verification" added.
- TEST-003: addressed 2026-11-08 by the added tests.

## Not verifiable here

- None. The interpretation needs no external service; the RKI lookup the report page starts on load was blocked.

## Open questions for the owner

- DEC-004 (open, not decided here): a no-growth isolate whose "Beurteilung" keeps the preselected "NTHi" gets "Kein
  Nachweis von Haemophilus influenzae." followed by "Beim eingesendeten Isolat handelt es sich am ehesten um NTHi." Does
  this belong to the same decision, or should the evaluation preselection (004 US3) be looked at separately?
- TEST-003 is fully covered now. Close it, or keep it until a change spec touches the interpretation?

Blocked external calls: `POST /Report/QueryHealthOffice/` on every load of the report page (6 loads, all aborted in the
browser). None reached the server.
