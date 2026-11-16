@spec-011
Feature: Data exports (generic)
  As-is behavior of HaemophilusWeb, verified on 2026-11-08 against build c180142.
  Source: specs/011-data-exports-generic/spec.md
  Column headers, column order and value encodings of the seven exports are asserted by unit tests in
  HaemophilusWebTests/Tools; these scenarios cover the export pages, the row selection and the role access, which
  no unit test exercises.

  Background:
    Given I am signed in as "Standard1"

  @smoke @US1 @FR-001 @FR-006
  Scenario: The laboratory export pre-fills the previous year and downloads a spreadsheet for the chosen range
    When I open "/PatientSending/LaboratoryExport"
    Then the page title is "Export der Labordaten (Haemophilus)"
    And I see "Entnahmedatum"
    And "Start-Datum" contains January 1st of the previous year
    And "Ende-Datum" contains December 31st of the previous year
    When I enter "01.01.2024" into "Start-Datum"
    And I enter "31.12.2024" into "Ende-Datum"
    And I click "Exportieren nach Excel"
    Then a file named like "Labor-Export_20240101-20241231.xlsx" is downloaded

  @US1 @US2 @US3 @FR-008 @FR-009 @FR-010
  Scenario Outline: A non-invasive submission is only in the laboratory export
    Given a submission with sender laboratory number "SV-011-HC", sampling location "Anderer nicht invasiv" and sampling date "01.06.2024" exists
    When I open "/PatientSending/<export>"
    And I enter "01.01.2024" into "Start-Datum"
    And I enter "31.12.2024" into "Ende-Datum"
    And I click "Exportieren nach Excel"
    Then a file named like "<file>" is downloaded
    And the downloaded file has <rows> row(s) with "<sampled>" in column "<column>"

    Examples:
      | export           | file                   | rows | sampled    | column        |
      | LaboratoryExport | Labor-Export_*.xlsx    | 1    | 01.06.2024 | Entnahmedatum |
      | RkiExport        | RKI-Export_*.xlsx      | 0    | 01.06.2024 | ent           |
      | PubMlstExport    | PubMLST-Export_*.xlsx  | 0    | 2024-06-01 | date_sampled  |

  @FR-007
  Scenario: A start date after the end date is accepted and yields only the header row
    When I open "/MeningoPatientSending/LaboratoryExport"
    And I enter "31.12.2024" into "Start-Datum"
    And I enter "01.01.2024" into "Ende-Datum"
    And I click "Exportieren nach Excel"
    Then a file named like "Labor-Export_20241231-20240101.xlsx" is downloaded
    And the downloaded file has no data rows

  @FR-001
  Scenario: An unreadable start date shows the pre-filled form again with a message
    When I open "/MeningoPatientSending/IrisExport"
    And I enter "abc" into "Start-Datum"
    And I click "Exportieren nach Excel"
    Then no file is downloaded
    And I see the validation message "Start-Datum ist kein gültiger Wert für abc."
    And "Start-Datum" contains January 1st of the previous year

  @US2 @US5 @FR-016 @FR-017
  Scenario: Public Health sees only the two RKI exports and can run them
    Given I am signed in as "Rki1"
    Then I see the link "RKI Export (Haemophilus)"
    And I see the link "RKI Export (Meningokokken)"
    When I click "RKI Export (Meningokokken)"
    Then the page title is "RKI Export (Meningokokken)"
    When I click "Exportieren nach Excel"
    Then a file named like "RKI-Export_*.xlsx" is downloaded

  @US5 @FR-016
  Scenario Outline: Public Health is refused the laboratory, PubMLST and IRIS exports
    Given I am signed in as "Rki1"
    When I open "<address>"
    Then I am on the sign-in page

    Examples:
      | address                                 |
      | /PatientSending/LaboratoryExport        |
      | /PatientSending/PubMlstExport           |
      | /MeningoPatientSending/LaboratoryExport |
      | /MeningoPatientSending/IrisExport       |
