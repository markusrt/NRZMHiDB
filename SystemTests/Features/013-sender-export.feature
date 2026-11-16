@spec-013
Feature: Sender export
  As-is behavior of HaemophilusWeb, verified on 2026-11-08 against build ec29065.
  Source: specs/013-sender-export/spec.md
  File name, column headers and order, sender selection and row content are asserted by unit tests in
  HaemophilusWebTests/Controllers/SenderControllerTests; these scenarios cover the export pages, the form behavior
  and the role access, which no unit test exercises.

  Background:
    Given I am signed in as "Standard1"

  @smoke @US1 @FR-001 @FR-002 @FR-005 @FR-010
  Scenario: The sender export pre-fills the previous year and downloads the senders active in the chosen range
    Given a sender "SV-013-Alpha" exists
    And a submission of sender "SV-013-Alpha" with sampling date "01.01.2022" exists
    When I open "/Sender/Export"
    Then the page title is "Export der Einsender (Haemophilus)"
    And I see "Entnahmedatum der Einsendung(en)"
    And "Start-Datum" contains January 1st of the previous year
    And "Ende-Datum" contains December 31st of the previous year
    When I enter "01.01.2022" into "Start-Datum"
    And I enter "31.12.2022" into "Ende-Datum"
    And I click "Exportieren nach Excel"
    Then a file named like "Einsender-Haemophilus-Export_20220101-20221231.xlsx" is downloaded
    And the downloaded file has 1 row(s) with "SV-013-Alpha" in column "Name"

  @US1 @FR-001 @FR-002
  Scenario: The Meningococci sender export is offered in the Export menu with its own page
    When I click "Export"
    And I click "Einsender Export" in the menu section "Meningokokken"
    Then I am on "/MeningoSender/Export"
    And the page title is "Export der Einsender (Meningokokken)"
    And I see "Entnahmedatum der Einsendung(en)"

  @FR-004
  Scenario: A start date after the end date is accepted and yields only the header row
    When I open "/Sender/Export"
    And I enter "31.12.2022" into "Start-Datum"
    And I enter "01.01.2022" into "Ende-Datum"
    And I click "Exportieren nach Excel"
    Then a file named like "Einsender-Haemophilus-Export_20221231-20220101.xlsx" is downloaded
    And the downloaded file has no data rows

  @FR-002
  Scenario: An unreadable start date shows the pre-filled form again with a message
    When I open "/MeningoSender/Export"
    And I enter "abc" into "Start-Datum"
    And I click "Exportieren nach Excel"
    Then no file is downloaded
    And I see the validation message "Start-Datum ist kein gültiger Wert für abc."
    And "Start-Datum" contains January 1st of the previous year

  @US3 @FR-011 @FR-012
  Scenario Outline: Public Health is offered no sender export and is refused both
    Given I am signed in as "Rki1"
    Then I do not see the link "Einsender Export"
    When I open "<address>"
    Then I am on the sign-in page

    Examples:
      | address               |
      | /Sender/Export        |
      | /MeningoSender/Export |

  @US3 @FR-011
  Scenario Outline: Without sign-in the sender exports ask for a sign-in
    Given I am not signed in
    When I open "<address>"
    Then I am on the sign-in page

    Examples:
      | address               |
      | /Sender/Export        |
      | /MeningoSender/Export |
