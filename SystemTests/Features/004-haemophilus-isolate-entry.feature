@spec-004
Feature: Haemophilus isolate analysis results entry
  As-is behavior of HaemophilusWeb, verified on 2026-11-04 against build ce997d4.
  Source: specs/004-haemophilus-isolate-entry/spec.md

  Background:
    Given I am signed in as "Standard1"

  @smoke @US1 @FR-001 @FR-003 @FR-004 @FR-005 @FR-009 @FR-014
  Scenario: Record laboratory results on the isolate of a submission
    Given a submission with sender laboratory number "SV-004-A" exists
    When I open the isolate of the submission "SV-004-A"
    Then the page title is "Isolat bearbeiten"
    And I see "Patientenalter bei Entnahme"
    And I see "Entnahmeort"
    When I choose "Ja" for "Wachstum"
    And I enter "4001" into "Stammnummer"
    And I choose "positiv" for "Oxidase"
    And I choose "b" for "Agglutination"
    And I choose "durchgeführt" for "MLST"
    And I enter "6" into "MLST Sequenztyp"
    And I enter "SV-004 Bemerkung" into "Bemerkung"
    And I click the button "Änderungen speichern"
    Then I am on "/PatientSending"
    When I open the isolate of the submission "SV-004-A"
    Then "Stammnummer" contains the text "4001"
    And "Beurteilung" contains the text "Hib"
    And "MLST Sequenztyp" contains the text "6"
    And "Bemerkung" contains the text "SV-004 Bemerkung"

  @US1 @FR-004
  Scenario: Growth "Ja" preselects a type of growth
    Given a submission with sender laboratory number "SV-004-A" exists
    When I open the isolate of the submission "SV-004-A"
    And I choose "Ja" for "Wachstum"
    Then "Art des Wachstums" contains the text "Typisches Wachstum auf KB"

  # Automation note: the report page queries the RKI tool on load; block /Report/QueryHealthOffice in the step definition.
  @US1 @FR-014 @FR-015
  Scenario: Save the isolate and continue to the report
    Given a submission with sender laboratory number "SV-004-A" exists
    When I open the isolate of the submission "SV-004-A"
    And I click the button "Änderungen speichern und Befund erstellen"
    Then the page title is "Befund erstellen"

  @US2 @FR-006 @FR-007
  Scenario: The E-Test result follows the measurement and the breakpoint
    Given a Haemophilus breakpoint for "Ampicillin" with "Sensibel <=" "0,5" and "Resistent >" "2" exists
    And a submission with sender laboratory number "SV-004-A" exists
    When I open the isolate of the submission "SV-004-A"
    And I select "4" in the E-Test row "Ampicillin"
    Then the E-Test row "Ampicillin" shows "resistent"
    When I select "1" in the E-Test row "Ampicillin"
    Then the E-Test row "Ampicillin" shows "intermediär"
    When I select "0,5" in the E-Test row "Ampicillin"
    Then the E-Test row "Ampicillin" shows "sensibel"
    When I click the button "Änderungen speichern"
    And I open the isolate of the submission "SV-004-A"
    Then the E-Test row "Ampicillin" shows "sensibel"

  @US3 @FR-008
  Scenario: The evaluation follows the agglutination and a disagreement is flagged without blocking the save
    Given a submission with sender laboratory number "SV-004-A" exists
    When I open the isolate of the submission "SV-004-A"
    And I choose "b" for "Agglutination"
    Then "Beurteilung" contains the text "Hib"
    When I select "Hia" in "Beurteilung"
    Then I see "Achtung: Diskrepanz zu Agglutination mit dem Wert"
    When I click the button "Änderungen speichern"
    Then I am on "/PatientSending"

  @FR-016
  Scenario: Accounts without the standard role are sent to the sign-in page
    Given I am signed in as "Rki1"
    When I open "/Isolate/Edit/1"
    Then I am on the sign-in page
