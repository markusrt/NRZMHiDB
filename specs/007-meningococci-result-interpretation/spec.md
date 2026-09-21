# Feature Specification: Meningococci result interpretation ruleset

**Feature Branch**: `007-meningococci-result-interpretation`

**Created**: 2026-08-30

**Status**: Draft (status-quo inventory)

**Input**: User description: "Feature title: \"Meningococci result interpretation ruleset\" (backlog ref A5-M, Group A - Core laboratory workflow). Document the current, as-implemented behavior of the Meningococci clinical result interpretation that feeds final reports, as it exists today. Read the JSON rulesets under HaemophilusWeb/Domain/Interpretation/*.json and the code that loads and applies them. Capture what isolate inputs drive an interpretation and what report outputs are produced. Document observable behavior only - do not propose changes."

> **Scope note**: This is an as-is specification of Meningococci interpretation behavior
> (backlog ref A5-M). It records the currently observable selection, report, typing, comment,
> Meningococci-presence, and serogroup outcomes. It does not redesign rules, assess clinical
> correctness, or propose new behavior. Isolate data entry (A3-M), susceptibility calculations,
> report layout, Haemophilus interpretation, and export formatting are out of scope.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Interpret a cultured isolate (Priority: P1)

A laboratory user prepares a final report for a vital strain or a strain that did not grow. The
system evaluates the culture, biochemical, agglutination, molecular, and invasive-status results,
selects the first complete cultured-isolate rule, and supplies its report content and typing rows.

**Why this priority**: Cultured isolates are a principal Meningococci reporting path and include
positive identification, no-detection, no-growth, non-invasive, and partial-report outcomes.

**Independent Test**: Supply one isolate matching each cultured-isolate behavior group and verify
the selected rule identifier, ordered report lines, ordered typing rows, comment, presence flag,
and normalized serogroup.

**Acceptance Scenarios**:

1. **Given** a cultured invasive isolate with typical growth, positive oxidase, a specific
   agglutination group, negative ONPG, and positive gamma-GT, **When** interpretation runs,
   **Then** the output identifies *Neisseria meningitidis*, reports the statutory notification and
   matching category, and emits the applicable typing rows.
2. **Given** the corresponding non-invasive cultured isolate, **When** interpretation runs,
   **Then** the output identifies *N. meningitidis* but reports the current note that resistance
   testing was not performed rather than the invasive notification lines.
3. **Given** a submitted strain that did not grow and has no positive molecular evidence,
   **When** interpretation runs, **Then** the report states that cultivation failed, requests a new
   submission, emits no typing rows, and marks the result as no Meningococci.
4. **Given** a strain that did not grow but has qualifying serogroup-PCR and PorA or FetA evidence,
   **When** interpretation runs, **Then** the report identifies *N. meningitidis*, reports its
   category, explains that susceptibility testing was unavailable, and emits molecular typing
   rows for positive and non-amplified targets.

---

### User Story 2 - Interpret native material or isolated DNA (Priority: P2)

A laboratory user prepares a final report for native material or isolated DNA. The system uses the
capsule-gene, PorA, FetA, 16S-rDNA, and multiplex real-time PCR results to select the first complete
native-material rule and produce report and molecular typing output.

**Why this priority**: This path reports direct molecular evidence when no cultured strain is the
interpretation source.

**Independent Test**: Supply representative B, C, W, Y, W/Y, ungrouped positive, alternative-
organism, and negative combinations and compare every output field with the native-material
decision table.

**Acceptance Scenarios**:

1. **Given** positive csb, csc, or cswy evidence with its required allele and supporting test
   combination, **When** interpretation runs, **Then** the report states that Meningococci-specific
   DNA was detected and assigns B, C, W, Y, or W/Y as specified by the matched rule.
2. **Given** a positive Meningococci real-time PCR or a matching *N. meningitidis* 16S sequence
   without a determined capsule group, **When** interpretation runs, **Then** the report states an
   invasive Meningococci infection without adding a serogroup category.
3. **Given** a negative Meningococci combination or positive real-time PCR evidence for another
   organism, **When** interpretation runs, **Then** the report states that Meningococci-specific DNA
   was not detected and emits the test-specific typing rows selected by that rule.

---

### User Story 3 - Populate final-report fields (Priority: P3)

For any matched rule, the reporting flow receives an ordered report-line collection, an ordered
typing collection, and an optional comment. The interpretation also exposes the matched rule,
Meningococci-presence status, and normalized serogroup for downstream reporting and exports.

**Why this priority**: These are the observable products consumed after rule selection.

**Independent Test**: Interpret one result from each output family and verify all six outputs,
including order, substitutions from isolate values, and intentionally empty values.

**Acceptance Scenarios**:

1. **Given** a matched rule with an identification, **When** output is assembled, **Then** the
   identification row appears first, followed by typing-template rows in the rule's declared order.
2. **Given** a typing row for PorA, FetA, MALDI-TOF, molecular typing, 16S-rDNA, or real-time PCR,
   **When** output is assembled, **Then** the recorded isolate values are substituted into the
   corresponding display label and value.
3. **Given** a rule whose report collection is empty, **When** interpretation runs, **Then** the
   final report collection is empty rather than replaced with a discrepancy warning.
4. **Given** a matched result with a derivable group, **When** interpretation completes, **Then**
   the normalized serogroup is exposed as a group value, `NG`, or `cnl` according to the current
   normalization rules.

---

### User Story 4 - Surface unmatched combinations (Priority: P4)

If no rule accepts all relevant isolate values, the interpretation remains a discrepancy result so
the laboratory record can be reviewed instead of being assigned the nearest clinical conclusion.

**Why this priority**: This is the current guard behavior for incomplete, conflicting, and
otherwise uncovered combinations.

**Independent Test**: Supply a combination not accepted by any rule and verify the discrepancy
report, empty typing list, null rule and serogroup, and false no-Meningococci flag.

**Acceptance Scenarios**:

1. **Given** no complete matching rule, **When** interpretation runs, **Then** the sole report line
   is "Diskrepante Ergebnisse, bitte Datenbankeinträge kontrollieren.", no rule identifier or
   serogroup is selected, no typing rows are emitted, and the no-Meningococci flag is false.
2. **Given** several rules whose criteria could match, **When** interpretation runs, **Then** only
   the first rule in the current ordered ruleset supplies output.

### Edge Cases

- `Nativmaterial` and `Isolierte DNA` both use the native-material rules. `Vitaler Stamm` and
  `Nicht angewachsen` use the cultured-isolate rules.
- Every native-material criterion is mandatory. A rule that omits an allele, best-match, or result
  value compares against that value's default/not-determined state rather than treating it as a
  wildcard. By contrast, omitted optional cultured-isolate criteria are wildcards.
- Cultured rules 20, 22, 24, and 26 precede broader overlapping rules 10 and 12; first-match order
  therefore preserves the more specific PorA/FetA-positive outcomes.
- A successful match may intentionally return no report lines (cultured rules 12, 13, 25, and 26).
  This differs from no match, which returns the discrepancy line.
- Native rule 32 reports no Meningococci evidence but does not set the no-Meningococci flag; its
  observable flag value is therefore false.
- Rules that emit a sequence typing row may add the sequencing-provider comment. Rules without
  that configured comment leave it empty, including positive rules with no sequence output.
- If the same interpreter instance is reused, each run clears typings, rule, serogroup, and the
  no-Meningococci flag. The comment is replaced only by a matched rule, so an unmatched run after a
  commented match retains the prior comment while returning the discrepancy report.
- MALDI-TOF matching treats "not determined" as requiring both devices to be not determined and
  "determined" as requiring either device. Output uses the Biotyper row when Biotyper is
  determined; otherwise it uses the VITEK MS row.
- A cultured rule can omit invasive status and therefore match either invasive or non-invasive
  submissions after earlier, more specific rules have been considered.
- Serogroup derivation can remain empty when agglutination and molecular group outputs conflict and
  the agglutination text is not one of the current unknown/non-invasive forms.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST choose the native-material rules for native material and isolated DNA
  and the cultured-isolate rules for vital strains and strains recorded as not grown.
- **FR-002**: The system MUST evaluate rules in their current order and MUST use only the first rule
  for which every configured criterion matches.
- **FR-003**: Cultured-isolate selection MUST evaluate invasive status, blood-agar growth,
  Martin-Lewis-agar growth, oxidase, agglutination, ONPG, gamma-GT, serogroup PCR, MALDI-TOF status,
  PorA PCR, and FetA PCR. A criterion omitted by a cultured rule MUST not restrict that rule.
- **FR-004**: Native-material selection MUST evaluate csb PCR, csc PCR, cswy PCR, cswy allele,
  PorA PCR, FetA PCR, 16S-rDNA result, optional exact 16S best match, real-time PCR result status,
  and real-time PCR organism result as exact configured criteria.
- **FR-005**: Before each run, the system MUST clear typing rows, selected rule, normalized
  serogroup, and no-Meningococci status and MUST initialize the report to the discrepancy line.
- **FR-006**: A match MUST replace the report with that rule's ordered report lines, including an
  intentionally empty collection, and MUST expose the matched rule identifier.
- **FR-007**: A cultured rule with an identification MUST emit `Identifikation` as the first typing
  row; remaining typing rows MUST follow the rule's declared order. Native-material typing rows
  MUST follow their declared order.
- **FR-008**: Output substitution MUST use the isolate's displayed enum values and recorded PorA
  VR1/VR2, FetA VR, MALDI-TOF best match, 16S best match, real-time PCR device, and rule-provided
  molecular-typing statement where selected.
- **FR-009**: The interpretation MUST expose these report-facing outputs: ordered report lines,
  ordered typing attribute/value rows, and an optional comment. It MUST also expose the selected
  rule, normalized serogroup, and no-Meningococci flag to downstream consumers.
- **FR-010**: Rules configured as no Meningococci MUST set the no-Meningococci flag. Native rule 32
  MUST preserve its current exception: negative report wording with the flag left false.
- **FR-011**: If no rule matches, the system MUST retain the discrepancy report, empty typing rows,
  null selected rule, null serogroup, and a false no-Meningococci flag; it MUST NOT infer a nearest
  result.
- **FR-012**: For a fresh interpretation instance, the optional comment MUST be the matched rule's
  configured comment or empty. On a reused instance, an unmatched run MUST preserve the prior
  comment as currently observed.
- **FR-013**: The report view model MUST receive the report lines, typing rows, and comment from the
  interpretation before report configuration is applied.
- **FR-014**: Downstream invasive-case export filtering MUST treat `NoMeningococci = false` as
  Meningococci found, independently of report wording.
- **FR-015**: The system MUST derive normalized serogroup after typing rows are assembled: use the
  sole serogroup or serogenogroup; use equal values when both exist; prefer serogenogroup when the
  agglutination text denotes unknown encapsulation or non-invasive behavior; and allow a recognized
  molecular-typing statement to override the prior value.
- **FR-016**: Serogroup normalization MUST map non-groupable, poly-agglutinating, and auto-
  agglutinating values to `NG`; values containing `cnl` to `cnl`; remove parenthesized explanatory
  suffixes; and map a no-agglutination typing to `NG` when no group was otherwise derived.
- **FR-017**: The feature MUST preserve the current ruleset behavior and wording without adding,
  correcting, or generalizing clinical interpretation rules.

### Cultured-Isolate Decision Table

In the table, `specific group` means A, B, C, E, W, X, Y, Z, or W/Y. `P/F` is PorA/FetA PCR.
All rows use the additional exact criteria declared for the listed rule identifiers.

| Rule identifier(s) | Observable input class | Report and status output | Typing/comment output |
| --- | --- | --- | --- |
| 01, 27 | Non-invasive/invasive; no growth on both media; all identification and molecular tests not determined | Three no-cultivation/resubmission lines; no Meningococci = true | No typings; no comment |
| 02, 28, 29, 35 | Invasive; no growth; serogroup PCR specific or `cnl`; P/F respectively +/+, +/-, -/+, -/- | Notification, matching category, no susceptibility testing; resubmission wording; no Meningococci = false | Identification, serogenogroup, positive or non-amplified P/F rows; sequencing comment when at least one sequence row is present |
| 30, 31, 32 | Invasive; no growth; negative serogroup PCR; P/F respectively +/+, +/-, -/+ | Notification, `NG` category, no susceptibility testing, molecular-method/resubmission wording | Identification, non-groupable serogenogroup, P/F rows, sequencing comment; derived group `NG` |
| 03, 04, 05, 34, NoNM_01, NoNM_02 | Atypical growth combinations with the listed biochemical/MALDI states | No *N. meningitidis* detected; no Meningococci = true | Growth and applicable ONPG, gamma-GT, and MALDI rows; no identification |
| 06, 07 | Typical culture, positive oxidase, specific agglutination, negative ONPG, positive gamma-GT, molecular tests not determined; non-invasive/invasive | Non-invasive resistance-testing note or invasive notification and matching category | Identification and serogroup from agglutination |
| 14, 36 | Invasive/non-invasive specific agglutination with positive P/F and otherwise accepted culture criteria | Invasive category or non-invasive resistance-testing note | Identification, agglutination group, PorA/FetA rows, sequencing comment |
| PartialReport | Invasive typical culture, specific agglutination, ONPG and gamma-GT not determined, other molecular tests not determined | Invasive notification and category | Identification, agglutination group, and susceptibility-test `follows` row |
| 08, 09, 10, 11 | Typical culture; auto/poly agglutination; specific or `cnl` serogroup PCR; positive P/F; invasive/non-invasive | Invasive notification/category or non-invasive resistance-testing note | Identification, contextual agglutination, molecular serogenogroup, P/F rows; sequencing comment except rule 11 |
| 12, 13 | Non-invasive typical culture; auto/poly agglutination; group and P/F not determined | Empty report; no Meningococci = false | Identification plus contextual agglutination; no comment |
| 15-26 | Typical culture with negative/poly/auto agglutination and `cnl`; invasive/non-invasive; P/F not determined or positive | Invasive `cnl` notification/category, non-invasive resistance note, or empty report according to the exact rule | Identification, agglutination, `cnl` serogenogroup, optional P/F; sequencing comment only on configured positive-P/F rules; derived group `cnl` |
| 33, 41 | Non-invasive typical culture; negative agglutination; group not determined; P/F not determined or positive | Non-invasive resistance-testing note | Identification, no-agglutination wording, optional P/F and sequencing comment; derived group `NG` |
| 37, 38 | Non-invasive typical culture; poly with negative PCR or auto with undetermined PCR; positive P/F | Non-invasive resistance-testing note | Identification, contextual agglutination, P/F, sequencing comment; derived group `NG` |
| 40 | Non-invasive typical culture; negative or poly agglutination; specific group PCR; P/F not determined | Non-invasive resistance-testing note | Identification and molecular serogenogroup; derived molecular group |
| 43 | Non-invasive typical culture; agglutination and molecular tests not determined | Note that serogroup and resistance testing are not performed for non-invasive isolates | Identification only; no derived group |

### Native-Material Decision Table

All native rows require the exact csb, csc, cswy, allele, PorA, FetA, 16S, real-time PCR status,
and real-time organism values configured by the selected rule.

| Rule identifier(s) | Observable input class | Report and status output | Typing/comment output |
| --- | --- | --- | --- |
| 01a-05 | B, C, W, Y, or W/Y capsule-gene evidence with positive P/F and real-time PCR not determined | DNA detected, invasive infection with group, notification, matching category; no Meningococci = false | Molecular group, PorA/FetA; sequencing comment; derived B/C/W/Y/W-Y group |
| 01b | B capsule-gene evidence, positive P/F, and positive Meningococci real-time PCR | Same B result and category | Positive real-time PCR, molecular group, PorA/FetA; sequencing comment; derived B |
| 22, 25-28 | B, C, W, Y, or W/Y capsule-gene evidence, negative P/F, and positive Meningococci real-time PCR | DNA detected, invasive infection, notification, matching group category | Positive real-time PCR, molecular group, non-amplified P/F; no comment; derived group |
| 29a/b, 30a/b, 31a/b | B capsule-gene evidence with mixed or positive P/F and positive Meningococci or undetermined real-time PCR | DNA detected, invasive infection, notification, B category | Optional positive real-time PCR, molecular B, positive/non-amplified P/F; sequencing comment; derived B |
| 07, 14 | Capsule-group PCR negative/inhibitory as configured, P/F negative, positive Meningococci real-time PCR | DNA detected, invasive infection, notification, category without group | Positive real-time PCR, negative molecular-group statement, non-amplified P/F; no comment; no derived group |
| 10, 17 | Capsule-group and P/F tests negative/inhibitory; positive 16S with exact best match *N. meningitidis* | DNA detected, invasive infection, notification, category without group | 16S and best match, negative molecular-group statement, non-amplified P/F; sequencing comment; no derived group |
| 21 | Capsule-group tests negative/inhibitory; positive P/F; 16S and real-time PCR not determined | DNA detected, notification, category without group | Negative molecular-group statement and PorA/FetA; sequencing comment; no derived group |
| 06, 08, 09, 11-13, 15, 16, 18-20, 23, 24 | Negative/inhibitory/not-determined Meningococci evidence, alternative-organism real-time result, or non-Meningococci 16S best match as configured | DNA not detected and no indication of *N. meningitidis*; no Meningococci = true | Applicable real-time, 16S, molecular-group, and non-amplified P/F rows; sequencing comment only for configured 16S rules |
| 32 | Every tested capsule-group, P/F, 16S, and real-time PCR result negative | DNA not detected and no indication of *N. meningitidis*; no Meningococci = false | Negative real-time, 16S, molecular-group, and non-amplified P/F rows; sequencing comment; no derived group |

### Output Catalog

- **Report lines**: Ordered final-report statements. Depending on the rule, these communicate
  detection or non-detection, invasive notification/category, inability to culture or test
  susceptibility, resubmission guidance, or non-invasive testing policy.
- **Typing rows**: Ordered attribute/value pairs for identification, culture growth, ONPG,
  gamma-GT, MALDI-TOF, serogroup/agglutination, serogenogroup, PorA, FetA, real-time PCR, 16S-rDNA,
  sequence best match, molecular typing, or a pending susceptibility test.
- **Comment**: Optional sequencing-provider statement supplied only by rules that configure it.
- **Selected rule**: The identifier of the first matched behavior, or null when unmatched.
- **No-Meningococci flag**: A rule-controlled status used by downstream invasive-case exports.
- **Normalized serogroup**: A derived concise group used by downstream exports; it may be a group,
  `NG`, `cnl`, or null.

### Key Entities *(include if feature involves data)*

- **Meningococci isolate**: The laboratory result record whose material type and test values drive
  interpretation.
- **Submission**: The isolate's associated submission. Material selects the rule family and
  sampling location supplies invasive status to cultured-isolate rules.
- **Interpretation rule**: An ordered set of accepted input values and configured output content.
- **Interpretation result**: The transient ordered report lines and optional comment supplied to
  the final-report view model.
- **Typing**: A transient report row with a display attribute and value.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: For all 44 cultured-isolate rules and all 36 native-material rules, a matching input
  selects the documented first rule and reproduces its report lines, typing order, comment, flag,
  and normalized serogroup exactly.
- **SC-002**: For 100% of unmatched input combinations, the report contains exactly the discrepancy
  line and no clinical conclusion is inferred.
- **SC-003**: For B, C, W, Y, W/Y, `NG`, and `cnl` outcomes, 100% of covered examples expose the
  documented normalized serogroup.
- **SC-004**: Every report-facing interpretation can be traced to one selected rule and its recorded
  isolate inputs without relying on undocumented patient data or implementation knowledge.
- **SC-005**: A laboratory-domain reviewer can use the two decision tables to classify every
  current rule into an observable input/output family, with all exceptions explicitly recorded.

## Assumptions

- The current ordered rulesets, interpretation behavior, and focused automated tests are the source
  of truth, including empty reports, missing flags, duplicate or unusual wording, and overlapping
  rules.
- Rule identifiers are included because they are exposed by the interpretation and provide stable
  traceability for this status-quo inventory; they do not imply a proposed design.
- Exact full report prose and long explanatory typing values remain authoritative in the active
  ruleset. This specification describes when each output family is selected and preserves exact
  strings where needed to distinguish fallback behavior.
- Normal report mapping creates a new interpreter for an isolate. Reuse behavior is documented
  because other current consumers retain an interpreter instance across calls.
- Report configuration and templates may control placement or visibility after interpretation;
  they do not change which rule matches or what raw interpretation output is produced.
- Clinical validity, terminology changes, and gaps in rule coverage are outside this documentation
  task.
