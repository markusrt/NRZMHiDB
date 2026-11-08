@spec-018
Feature: Application configuration
  As-is behavior of HaemophilusWeb, verified on 2026-11-08 against build 17bb7a1.
  Source: specs/018-application-configuration/spec.md
  # Automation note: the scenarios change the shared settings of the configuration page; the step definitions must
  # record the five values before and restore them afterwards. Report pages post a health-office lookup on load,
  # which the step definitions must block (see the route block of /spec-verify). Splitting, trimming, the date
  # rules and the report data are asserted by unit tests (ConfigurationControllerTests, ConfigurationServiceTests,
  # ConfigurationViewModelValidatorTests, IsolateViewModelMappingActionTests, MeningoIsolateViewModelMappingActionTests).

  Background:
    Given I am signed in as "Standard1"

  @smoke @US1 @FR-001 @FR-003 @FR-004 @FR-009 @FR-012
  Scenario: Maintain the report signers and find them on the report page
    When I click "Administration"
    And I click "Konfiguration"
    Then the page title is "Konfiguration"
    And I see "Ein Unterzeichner pro Zeile."
    And I see "Mitteilung im Befund"
    And I see "Leitung und Kontakte"
    When I enter the lines into "Unterzeichner":
      """
      SV-018-Signer A

      SV-018-Signer B
      """
    And I click "Speichern"
    Then I am on "/Configuration/Edit"
    And I see "Konfiguration gespeichert."
    And "Unterzeichner" contains the lines:
      """
      SV-018-Signer A
      SV-018-Signer B
      """
    When I open "/Report/Isolate/1"
    Then "Unterzeichner" offers 3 entries
    And "Unterzeichner" offers "SV-018-Signer A"
    And "Unterzeichner" offers "SV-018-Signer B"

  @US4 @FR-003
  Scenario: Leave the configuration page with "Abbrechen" without saving
    When I open "/Configuration/Edit"
    And I enter "SV-018-Abbrechen" into "Gesamtleitung"
    And I click "Abbrechen"
    Then I am on "/"
    When I open "/Configuration/Edit"
    Then "Gesamtleitung" contains the value stored before the change

  @US4 @FR-001 @FR-002
  Scenario: A Public Health account is neither offered nor admitted to the configuration page
    Given I am signed in as "Rki1"
    Then I do not see the link "Konfiguration"
    When I open "/Configuration/Edit"
    Then I am on the sign-in page

  @US4 @FR-002
  Scenario: The configuration page asks for sign-in
    Given I am not signed in
    When I open "/Configuration"
    Then I am on the sign-in page
