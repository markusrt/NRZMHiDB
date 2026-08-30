# Feature Specification: Haemophilus result interpretation engine

**Feature Branch**: `006-haemophilus-result-interpretation`

**Created**: 2026-08-30

**Status**: Draft (status-quo inventory)

**Input**: User description: "Feature title: \"Haemophilus result interpretation engine\" (backlog ref A5-H, Group A — Core laboratory workflow). Document the current, as-implemented behavior of the Haemophilus clinical result interpretation that feeds final reports, as it exists today. Read the interpretation code under HaemophilusWeb/Domain. Capture what isolate inputs drive an interpretation and what report outputs are produced. Document the current implementation as-is (a large if/else decision tree). Document observable behavior only — do not propose changes."

> **Scope note**: This is an *as-is* specification of the Haemophilus interpretation behavior
> (backlog ref A5-H). The current behavior is expressed as a single, ordered if/else decision tree.
> This specification records its observable input combinations and outputs without redesigning,
> correcting, or generalizing them. Meningococci interpretation, isolate result entry (A3-H),
> report-template management, susceptibility interpretation, and report document formatting are
> out of scope except where they consume the three interpretation text fields described here.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Produce the final interpretation from isolate results (Priority: P1)

A laboratory user opens report creation for a Haemophilus isolate. The system evaluates the
recorded agglutination, capsule-marker, serotype-PCR, growth, evaluation, and sampling-location
information and supplies the final interpretation text used by the report flow.

**Why this priority**: The final interpretation is the primary clinical output of this feature and
is the text selected for a final report.

**Independent Test**: Supply one isolate for each row in the decision table and verify that the
final interpretation and accompanying disclaimer exactly match the documented outputs.

**Acceptance Scenarios**:

1. **Given** negative agglutination, negative bexA, and negative or not-determined serotype PCR,
   **When** the isolate is interpreted, **Then** the final interpretation identifies an
   unencapsulated, non-typable *H. influenzae* result.
2. **Given** negative agglutination, negative bexA, and a serotype-PCR result from a through f,
   **When** the isolate is interpreted, **Then** the final interpretation identifies a
   phenotypically non-typable result and states that the genetic capsule locus for that PCR
   serotype is not expressed.
3. **Given** a specific agglutination result from a through f, positive bexA, and either the same
   serotype-PCR result or no serotype-PCR determination, **When** the isolate is interpreted,
   **Then** the final interpretation identifies the matching serotype.
4. **Given** agglutination, bexA, and serotype PCR are all not determined, **When** growth is "yes",
   **Then** the final interpretation states that the submitted strain could not be cultivated and
   requests resubmission, exactly as the current behavior does.
5. **Given** agglutination, bexA, and serotype PCR are all not determined, **When** growth is "no",
   **Then** the final interpretation states that *H. influenzae* was not detected and the disclaimer
   names the isolate evaluation's displayed value as the most likely identity.

---

### User Story 2 - Produce a preliminary interpretation (Priority: P2)

When a preliminary report template is selected, the report flow uses a separate preliminary
interpretation. Only two incomplete-typing combinations produce a non-discrepant preliminary
conclusion; other recognized final-result combinations leave the preliminary output at the
discrepancy warning.

**Why this priority**: Preliminary reporting is a distinct observable output path, but the final
interpretation remains the principal result.

**Independent Test**: Interpret isolates with negative agglutination plus undetermined bexA/PCR,
and with specific agglutination plus undetermined bexA/PCR; verify the preliminary output and
compare it with the final output for each case.

**Acceptance Scenarios**:

1. **Given** negative agglutination with bexA and serotype PCR both not determined, **When** the
   isolate is interpreted, **Then** the preliminary interpretation identifies an unencapsulated,
   non-typable result, while the final interpretation adds that molecular typing was not performed.
2. **Given** a specific agglutination result from a through f with bexA and serotype PCR both not
   determined, **When** the isolate is interpreted, **Then** the preliminary interpretation names
   the agglutination serotype and the final interpretation remains the discrepancy warning.
3. **Given** a combination that produces a recognized final conclusion but no preliminary-specific
   conclusion, **When** the isolate is interpreted, **Then** the preliminary interpretation remains
   the discrepancy warning.

---

### User Story 3 - Supply reporting category or non-invasive note (Priority: P3)

Alongside the interpretation, the report flow receives one shared disclaimer text. For recognized
negative-agglutination and specific-serotype branches, the text depends on whether the submission's
sampling location is invasive. The no-growth/no-detection branch supplies its own branch-specific
note instead.

**Why this priority**: The disclaimer provides reporting context but depends on an interpretation
branch having already been selected.

**Independent Test**: Run the same recognized negative-agglutination and specific-serotype cases
once with an invasive sampling location and once with a non-invasive location, then compare the
disclaimer while confirming the interpretation itself is unchanged.

**Acceptance Scenarios**:

1. **Given** a recognized negative-agglutination branch and an invasive sampling location,
   **When** the isolate is interpreted, **Then** the disclaimer contains the statutory reporting
   statement and categorizes the result as unencapsulated *H. influenzae*.
2. **Given** a recognized specific-serotype branch and an invasive sampling location, **When** the
   isolate is interpreted, **Then** the disclaimer contains the statutory reporting statement and
   categorizes the result using the agglutination serotype.
3. **Given** either branch with a non-invasive sampling location, **When** the isolate is
   interpreted, **Then** the disclaimer states that molecular typing and resistance tests are not
   performed for non-invasive isolates for epidemiological and cost reasons.
4. **Given** the no-cultivation outcome, **When** the isolate is interpreted, **Then** the disclaimer
   states that advance notification by telephone occurred.

---

### User Story 4 - Guard discrepant result combinations during report creation (Priority: P4)

When no complete decision-tree outcome replaces the default, the selected final or preliminary
interpretation remains a discrepancy warning. Report creation detects that warning and asks the
laboratory user whether a report proposal should still be produced.

**Why this priority**: This is the fallback and review behavior for incomplete, inconsistent, or
otherwise unmatched combinations.

**Independent Test**: Use an unmatched isolate combination, select a final report template and a
preliminary report template in turn, and verify that each selection checks its corresponding
interpretation and requires explicit confirmation before report generation continues.

**Acceptance Scenarios**:

1. **Given** an isolate combination not matched by a complete outcome, **When** it is interpreted,
   **Then** final and preliminary interpretation fields not replaced by a matched branch contain
   "Diskrepante Ergebnisse, bitte Datenbankeinträge kontrollieren."
2. **Given** the interpretation selected for the chosen report type contains the discrepancy
   marker, **When** the user requests report creation, **Then** the system asks whether a report
   proposal should nevertheless be generated.
3. **Given** the discrepancy prompt, **When** the user declines, **Then** no report document is
   generated; **When** the user explicitly continues, **Then** report generation proceeds with the
   current report data.

### Edge Cases

- Negative agglutination always assigns an invasive or non-invasive disclaimer even when its bexA
  and serotype-PCR combination leaves final and preliminary interpretations discrepant.
- Specific agglutination a through f with not-determined bexA and a matching determined PCR
  serotype enters the specific branch and assigns a serotype disclaimer, but neither interpretation
  is replaced; both remain discrepant.
- Specific agglutination with negative bexA, or with positive bexA and a conflicting determined PCR
  serotype, is unmatched and retains both discrepancy texts with an empty disclaimer.
- Auto-agglutination, poly-agglutination, and non-evaluable agglutination have no dedicated outcome
  and retain the discrepancy defaults unless another documented branch can match (none currently
  can).
- When all three typing inputs are not determined, growth "not stated" does not trigger the
  no-cultivation/no-detection outcomes; both interpretations remain discrepant and the disclaimer is
  empty.
- The evaluation value affects output only for the all-not-determined typing combination with
  growth "no"; elsewhere it does not influence interpretation.
- Invasive status affects only the disclaimer, not the final or preliminary interpretation. Blood,
  cerebrospinal fluid, and "other invasive" are invasive; "other non-invasive" is non-invasive.
- Recognized negative-agglutination and specific-serotype branches require an associated submission
  to determine invasive status. The normal report flow supplies that association.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST derive the Haemophilus interpretation when an isolate is prepared for
  report preview or report generation; interpretation MUST NOT alter the isolate or submission.
- **FR-002**: The decision MUST use only these recorded inputs: agglutination, bexA result,
  serotype-PCR result, growth result, isolate evaluation, and invasive status derived from the
  submission's sampling location.
- **FR-003**: Agglutination MUST distinguish not determined, specific serotypes a through f,
  negative, auto, poly, and non-evaluable values; serotype PCR MUST distinguish not determined,
  serotypes a through f, and negative; bexA MUST distinguish not determined, negative, and positive;
  growth MUST distinguish no, yes, and not stated.
- **FR-004**: Before evaluating a combination, the system MUST initialize both final and preliminary
  interpretations to "Diskrepante Ergebnisse, bitte Datenbankeinträge kontrollieren." and the
  disclaimer to an empty value.
- **FR-005**: For negative agglutination with negative bexA and negative or not-determined serotype
  PCR, the final interpretation MUST be "Die Ergebnisse sprechen für einen unbekapselten
  Haemophilus influenzae (sog. \"nicht-typisierbarer\" H. influenzae, NTHi)."; the preliminary
  interpretation MUST retain the discrepancy warning.
- **FR-006**: For negative agglutination with negative bexA and serotype PCR a through f, the final
  interpretation MUST identify a phenotypically non-typable *H. influenzae* and state that the
  genetic capsule locus for the PCR serotype is not expressed; the preliminary interpretation MUST
  retain the discrepancy warning.
- **FR-007**: For negative agglutination with both bexA and serotype PCR not determined, the
  preliminary interpretation MUST be "Das Ergebnis spricht für einen unbekapselten Haemophilus
  influenzae (sog. \"nicht-typisierbarer\" H. influenzae, NTHi)." and the final interpretation MUST
  append "Eine molekularbiologische Typisierung wurde aus epidemiologischen und Kostengründen nicht
  durchgeführt."
- **FR-008**: Every negative-agglutination combination, including combinations whose interpretation
  remains discrepant, MUST receive the invasive unencapsulated reporting disclaimer or the
  non-invasive testing disclaimer according to sampling location.
- **FR-009**: For specific agglutination a through f with positive bexA and either a matching
  serotype-PCR result or not-determined serotype PCR, the final interpretation MUST state "Die
  Ergebnisse sprechen für eine Infektion mit Haemophilus influenzae des Serotyp {serotype}
  (Hi{serotype})." and the preliminary interpretation MUST retain the discrepancy warning.
- **FR-010**: For specific agglutination a through f with bexA and serotype PCR both not determined,
  the preliminary interpretation MUST state "Das Ergebnis spricht für eine Infektion mit
  Haemophilus influenzae des Serotyp {serotype} (Hi{serotype})." and the final interpretation MUST
  retain the discrepancy warning.
- **FR-011**: A specific-serotype branch MUST use the displayed agglutination value for
  `{serotype}` and MUST assign either the invasive reporting disclaimer categorized to that
  serotype or the non-invasive testing disclaimer.
- **FR-012**: With agglutination, bexA, and serotype PCR all not determined and growth "yes", the
  final interpretation MUST be "Der eingesendete Stamm konnte nicht angezüchtet werden. Um
  Wiedereinsendung wird gebeten.", the preliminary interpretation MUST remain discrepant, and the
  disclaimer MUST be "Eine telefonische Vorabmitteilung ist erfolgt."
- **FR-013**: With agglutination, bexA, and serotype PCR all not determined and growth "no", the
  final interpretation MUST be "Kein Nachweis von Haemophilus influenzae.", the preliminary
  interpretation MUST remain discrepant, and the disclaimer MUST be "Beim eingesendeten Isolat
  handelt es sich am ehesten um {evaluation}.", where `{evaluation}` is the evaluation's displayed
  value.
- **FR-014**: Any output field not replaced by a matched outcome MUST retain its initialized
  discrepancy or empty value; the interpreter MUST NOT infer a closest result for unmatched
  combinations.
- **FR-015**: The interpreter MUST output three report fields: final interpretation, preliminary
  interpretation, and one disclaimer shared by both report types. It MUST NOT produce report-array
  entries or a comment for Haemophilus isolates.
- **FR-016**: The report preview MUST display final and preliminary interpretation values, and the
  report document data MUST include all three interpretation fields.
- **FR-017**: Report creation MUST inspect the final interpretation for final templates and the
  preliminary interpretation for preliminary templates; if the selected value contains the
  discrepancy marker, explicit user confirmation MUST be required before document generation.
- **FR-018**: This feature MUST preserve the current decision-tree outcomes as documented and MUST
  NOT introduce new clinical rules, resolve discrepant combinations, or change report wording.

### Decision Table

| Agglutination | bexA | Serotype PCR | Growth | Final interpretation | Preliminary interpretation | Disclaimer |
| --- | --- | --- | --- | --- | --- | --- |
| Negative | Negative | Negative or not determined | Any | Unencapsulated/non-typable result | Discrepancy | Invasive unencapsulated category or non-invasive testing note |
| Negative | Negative | a-f | Any | Phenotypically non-typable; matching PCR capsule locus is not expressed | Discrepancy | Invasive unencapsulated category or non-invasive testing note |
| Negative | Not determined | Not determined | Any | Unencapsulated/non-typable result plus molecular typing not performed | Unencapsulated/non-typable result | Invasive unencapsulated category or non-invasive testing note |
| Negative | Positive, or not determined with a determined PCR value | Any applicable value | Any | Discrepancy | Discrepancy | Invasive unencapsulated category or non-invasive testing note |
| Specific a-f | Positive | Matching value or not determined | Any | Matching serotype result | Discrepancy | Invasive matching-serotype category or non-invasive testing note |
| Specific a-f | Not determined | Not determined | Any | Discrepancy | Matching serotype result | Invasive matching-serotype category or non-invasive testing note |
| Specific a-f | Not determined | Matching determined value | Any | Discrepancy | Discrepancy | Invasive matching-serotype category or non-invasive testing note |
| Not determined | Not determined | Not determined | Yes | Could not cultivate; request resubmission | Discrepancy | Telephone advance-notification note |
| Not determined | Not determined | Not determined | No | No *H. influenzae* detected | Discrepancy | Most likely identity from evaluation |
| Any unmatched combination | Any | Any | Any | Discrepancy | Discrepancy | Empty unless the negative or specific branch assigned one as above |

### Key Entities *(include if feature involves data)*

- **Haemophilus isolate**: The laboratory result record being interpreted. Of its many analysis
  fields, this feature reads only agglutination, bexA, serotype PCR, growth, and evaluation.
- **Submission (Sending)**: The isolate's associated submission. Its sampling location determines
  whether the disclaimer follows the invasive or non-invasive path.
- **Interpretation result**: A transient reporting result containing final interpretation,
  preliminary interpretation, and disclaimer. The result type also has report-array and comment
  fields, but this Haemophilus decision tree leaves both unset.
- **Report data**: The report-preview data populated from the isolate and interpretation result.
  The selected report template determines whether final or preliminary interpretation is checked
  for discrepancy before document generation.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: For 100% of input classes in the decision table, the final interpretation,
  preliminary interpretation, and disclaimer match the documented current outcome exactly.
- **SC-002**: For all six specific serotypes a through f, matching inputs substitute the same
  displayed serotype consistently in interpretation and invasive reporting-category text.
- **SC-003**: Switching only between an invasive and non-invasive sampling location changes the
  disclaimer in 100% of recognized negative-agglutination and specific-serotype cases and never
  changes either interpretation field.
- **SC-004**: Every unmatched output remains visibly discrepant; no unmatched combination is
  silently presented as a recognized final or preliminary conclusion.
- **SC-005**: A report whose selected final or preliminary interpretation is discrepant never
  proceeds without an explicit user decision to continue.
- **SC-006**: A laboratory-domain reviewer can trace every populated report interpretation field
  to one decision-table row using only the six documented inputs, with no undocumented clinical
  input required.

## Assumptions

- This inventory treats the current domain decision tree and its automated tests as the source of
  truth, including wording or value associations that may appear counterintuitive.
- The report flow supplies a persisted isolate with its associated submission. Behavior for a
  missing submission association is outside the supported reporting flow.
- "Invasive" is derived from sampling location: blood, cerebrospinal fluid, and other invasive are
  invasive; other non-invasive is non-invasive.
- The displayed evaluation values available to the no-detection disclaimer are NTHi, Hia-Hif,
  *H. haemolyticus*, *H. parainfluenzae*, no growth, no Haemophilus species, *H. influenzae*,
  Haemophilus species other than *H. influenzae*, and no *H. influenzae*.
- Report templates decide where the final interpretation, preliminary interpretation, and shared
  disclaimer appear in the generated document. Template content and layout are outside this feature.
- Meningococci interpretation uses a separate behavior path and is outside this specification.
