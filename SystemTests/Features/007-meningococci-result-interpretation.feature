@spec-007
Feature: Meningococci result interpretation ruleset
  As-is behavior of HaemophilusWeb, verified on 2026-11-08 against build 8c7c162.
  Source: specs/007-meningococci-result-interpretation/spec.md
  The rulesets themselves (rule selection, report lines, typing order, comment, flag, serogroup, discrepancy
  defaults) are asserted by HaemophilusWebTests/Domain/MeningoIsolateInterpretationTests; this scenario covers
  the report page only.

  # Automation note: the report page posts /Report/QueryHealthOffice on load, which queries the RKI tool;
  # block that request in the step definitions before the first navigation.
  # Data: MZ004/20 is the seeded invasive cultured isolate of sender "SV-Seed-Positive" (isolate 4;
  # typical growth, oxidase positive, agglutination "auto", serogroup PCR "W/Y", PorA and FetA positive).

  Background:
    Given I am signed in as "Standard1"

  @smoke @US1 @US3 @FR-009 @FR-013
  Scenario: The report page shows the interpretation of an invasive cultured isolate
    When I open "/MeningoReport/Isolate/4"
    Then the page title is "Befund erstellen"
    And I see "MZ004/20"
    And I see "Der Nachweis von Neisseria meningitidis aus primär sterilem Material ist nach §7 IfSG meldepflichtig."
    And I see "Meldekategorie dieses Befundes: Neisseria meningitidis, Serogruppe W/Y."
