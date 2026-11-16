@spec-022
Feature: Postal-code lookup (Geonames)
  As-is behavior of HaemophilusWeb, verified on 2026-11-08 against build 27f3211.
  Source: specs/022-postal-code-lookup-geonames/spec.md
  Every postal-code entry sends real requests to the Geonames service under the configured account
  (one suggestion request per change, plus one lookup request from five characters on); keep runs few.

  Background:
    Given I am signed in as "Standard1"

  @smoke @US1 @FR-001 @FR-004 @FR-005 @FR-006
  Scenario: A complete postal code fills place, county and state of a Haemophilus patient
    When I open "/PatientSending/Create"
    And I enter "91301" into "Postleitzahl"
    Then "Postleitzahl" contains "91301"
    And "Wohnort" contains "Forchheim"
    And "Landkreis" contains "Landkreis Forchheim"
    And "Bundesland" shows "Bayern"

  @US1 @FR-006
  Scenario: An unknown postal code sets place and county to "Unbekannt"
    When I open "/PatientSending/Create"
    And I enter "00000" into "Postleitzahl"
    Then "Wohnort" contains "Unbekannt"
    And "Landkreis" contains "Unbekannt"
    And "Bundesland" shows "n.n."

  @US2 @FR-002 @FR-004 @FR-005
  Scenario: Choosing a suggestion takes that place of a shared postal code
    When I open "/PatientSending/Create"
    And I enter "06773" into "Postleitzahl"
    Then "Wohnort" contains "Gossa"
    And "Postleitzahl" suggests "06773 Gräfenhainichen"
    When I choose the suggestion "06773 Gräfenhainichen" for "Postleitzahl"
    Then "Postleitzahl" contains "06773"
    And "Wohnort" contains "Gräfenhainichen"
    And "Landkreis" contains "Landkreis Wittenberg"
    And "Bundesland" shows "Sachsen-Anhalt"

  @US1 @FR-001 @FR-006
  Scenario: The sender form fills only "Ort"
    When I open "/Sender/Create"
    And I enter "13353" into "Postleitzahl"
    Then "Ort" contains "Berlin"

  @US3 @FR-007 @FR-010
  Scenario: The Meningococci country list defaults to "Deutschland" and the lookup leaves it unchanged
    When I open "/MeningoPatientSending/Create"
    Then "Land" offers 250 entries
    And "Land" shows "Deutschland"
    When I select "Österreich" in "Land"
    And I enter "97080" into "Postleitzahl"
    Then "Wohnort" contains "Würzburg"
    And "Bundesland" shows "Bayern"
    And "Land" shows "Österreich"
