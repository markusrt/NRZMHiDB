@spec-006
Feature: Haemophilus result interpretation engine
  As-is behavior of HaemophilusWeb, verified on 2026-11-08 against build 04d40da.
  Source: specs/006-haemophilus-result-interpretation/spec.md
  The decision table itself (texts, disclaimers, discrepant combinations) is asserted by
  HaemophilusWebTests/Domain/IsolateInterpretationTests; these scenarios cover the report page only.

  # Automation note: the report page posts /Report/QueryHealthOffice on load, which queries the RKI tool;
  # block that request in the step definitions before the first navigation.

  Background:
    Given I am signed in as "Standard1"
    And a submission with sender laboratory number "SV-006-A" exists

  @smoke @US2 @FR-001 @FR-007 @FR-016
  Scenario: The report page shows the preliminary and the final interpretation
    When I open the isolate of the submission "SV-006-A"
    And I choose "negativ" for "Agglutination"
    And I choose "n.d." for "bexA"
    And I choose "n.d." for "Serotyp-PCR"
    And I click the button "Änderungen speichern und Befund erstellen"
    Then the page title is "Befund erstellen"
    And the report field "Teilbefund" shows "Das Ergebnis spricht für einen unbekapselten Haemophilus influenzae (sog. "nicht-typisierbarer" H. influenzae, NTHi)."
    And the report field "Befund" shows "Das Ergebnis spricht für einen unbekapselten Haemophilus influenzae (sog. "nicht-typisierbarer" H. influenzae, NTHi). Eine molekularbiologische Typisierung wurde aus epidemiologischen und Kostengründen nicht durchgeführt."

  @US4 @FR-017
  Scenario: A discrepant final interpretation asks before a final report is created
    When I open the isolate of the submission "SV-006-A"
    And I choose "a" for "Agglutination"
    And I choose "n.d." for "bexA"
    And I choose "n.d." for "Serotyp-PCR"
    And I click the button "Änderungen speichern und Befund erstellen"
    And I select "BLNAS v10.docx" in "Befundsvorlage"
    And I select "Dr. Jane Doe" in "Unterzeichner"
    And I click "Befund erstellen"
    Then I see "Diskrepante Ergebnisse, bitte Datenbankeinträge kontrollieren. Soll dennoch ein Befundvorschlag ausgegeben werden?"
    When I click "Nein"
    Then no file is downloaded
    When I click "Befund erstellen"
    And I click "Ja"
    Then a file named like "KL *.docx" is downloaded

  @US4 @FR-017
  Scenario: A preliminary template checks the preliminary interpretation
    When I open the isolate of the submission "SV-006-A"
    And I choose "negativ" for "Agglutination"
    And I choose "negativ" for "bexA"
    And I choose "n.d." for "Serotyp-PCR"
    And I click the button "Änderungen speichern und Befund erstellen"
    And I select "Fax - Teilbefund - Serotyp v4.docx" in "Befundsvorlage"
    And I select "Dr. Jane Doe" in "Unterzeichner"
    And I click "Befund erstellen"
    Then I see "Diskrepante Ergebnisse, bitte Datenbankeinträge kontrollieren. Soll dennoch ein Befundvorschlag ausgegeben werden?"
    When I click "Nein"
    And I select "BLNAS v10.docx" in "Befundsvorlage"
    And I click "Befund erstellen"
    Then a file named like "KL *.docx" is downloaded
