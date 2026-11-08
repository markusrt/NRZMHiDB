@spec-005
Feature: Meningococci isolate analysis results entry
  As-is behavior of HaemophilusWeb, verified on 2026-11-08 against build dd1a444.
  Source: specs/005-meningococci-isolate-entry/spec.md

  Background:
    Given I am signed in as "Standard1"

  @smoke @US1 @FR-001 @FR-002 @FR-003 @FR-004 @FR-016
  Scenario: Record laboratory results on the isolate of a Meningococci submission
    Given a Meningococci submission with sender laboratory number "SV-005-A" exists
    When I open the isolate of the Meningococci submission "SV-005-A"
    Then the page title is "Isolat bearbeiten"
    And I see "Patientenalter bei Entnahme"
    And I see "Entnahmeort"
    When I choose "typisches Wachstum" for "Wachstum auf Blutagar"
    And I enter "5001" into "Stammnummer"
    And I choose "positiv" for "Oxidase"
    And I choose "B" for "Agglutination"
    And I choose "B" for "Serogruppen-PCR"
    And I enter "SV-005 Bemerkung" into "Bemerkung"
    And I click "Molekulare Typisierung"
    And I choose "positiv" for "csb-PCR"
    And I click the button "Änderungen speichern"
    Then I am on "/MeningoPatientSending"
    When I open the isolate of the Meningococci submission "SV-005-A"
    Then "Stammnummer" contains the text "5001"
    And "Bemerkung" contains the text "SV-005 Bemerkung"

  @US2 @FR-005 @FR-006
  Scenario: The E-Test result follows the measurement and the preselected newest breakpoint
    Given a Meningococci breakpoint for "Benzylpenicillin" with "Sensibel <=" "0,064" and "Resistent >" "0,25" exists
    And a Meningococci submission with sender laboratory number "SV-005-A" exists
    When I open the isolate of the Meningococci submission "SV-005-A"
    And I select "0,38" in the E-Test row "Penicillin"
    Then the E-Test row "Penicillin" shows "resistent"
    When I select "0,25" in the E-Test row "Penicillin"
    Then the E-Test row "Penicillin" shows "intermediär"
    When I select "0,064" in the E-Test row "Penicillin"
    Then the E-Test row "Penicillin" shows "sensibel"
    When I click the button "Änderungen speichern"
    And I open the isolate of the Meningococci submission "SV-005-A"
    Then the E-Test row "Penicillin" shows "sensibel"

  # Automation note: the report page queries the RKI tool on load; block /Report/QueryHealthOffice in the step definition.
  @US4 @FR-017
  Scenario: Save the isolate and continue to the report
    Given a Meningococci submission with sender laboratory number "SV-005-A" exists
    When I open the isolate of the Meningococci submission "SV-005-A"
    And I click the button "Änderungen speichern und Befund erstellen"
    Then the page title is "Befund erstellen"

  @US1 @FR-015
  Scenario: An error on the molecular typing tab opens that tab
    Given a Meningococci submission with sender laboratory number "SV-005-A" exists
    When I open the isolate of the Meningococci submission "SV-005-A"
    And I choose "typisches Wachstum" for "Wachstum auf Blutagar"
    And I choose "positiv" for "Oxidase"
    And I click "Molekulare Typisierung"
    And I choose "positiv" for "csb-PCR"
    And I choose "positiv" for "csc-PCR"
    And I click the button "Änderungen speichern"
    Then I see the validation message "Es darf nur ein Serogenotyp positiv sein."
    And I see "cswy-PCR"

  @FR-023
  Scenario: Accounts without the standard role are sent to the sign-in page
    Given I am signed in as "Rki1"
    When I open "/MeningoIsolate/Edit/1"
    Then I am on the sign-in page
