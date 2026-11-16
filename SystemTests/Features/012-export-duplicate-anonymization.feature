@spec-012
Feature: Duplicate-patient anonymization in exports
  As-is behavior of HaemophilusWeb, verified on 2026-11-08 against build 82f4f63.
  Source: specs/012-export-duplicate-anonymization/spec.md
  The duplicate rules themselves (matching, dropping window, group labels, identifier columns) are asserted by unit
  tests in HaemophilusWebTests/Tools; these scenarios cover only the export pages and the choice between the two
  modes, which no unit test exercises.

  Background:
    Given I am signed in as "Standard1"

  @smoke @US1 @FR-001 @FR-009
  Scenario: The adjusted RKI export is preselected and leaves out the direct patient identifiers
    When I open "/PatientSending/RkiExport"
    Then the page title is "RKI Export (Haemophilus)"
    And "Unbereinigte Daten" contains the selected option "Nein"
    And I see "Unbereinigt: Inklusive doppelter Einsendungen."
    When I click "Exportieren nach Excel"
    Then a file named like "RKI-Export_*.xlsx" is downloaded
    And the downloaded file has no column "initials"
    And the downloaded file has no column "date_of_birth"
    And the downloaded file has no column "postal_code"

  @US1 @US2 @FR-002 @FR-006
  Scenario Outline: The adjustment option decides whether a same-day repeat submission is exported
    Given two submissions of patient "Z.S." born "04.04.2004" with postal code "97002", sampled "12.01.2026" and received "14.01.2026", exist
    When I open "/PatientSending/<export>"
    And I enter "01.01.2026" into "Start-Datum"
    And I enter "31.01.2026" into "Ende-Datum"
    And I choose "<choice>" for "Unbereinigte Daten"
    And I click "Exportieren nach Excel"
    Then a file named like "<file>" is downloaded
    And the downloaded file has <rows> row(s) with "<received>" in column "<column>"
    And the downloaded file has no column "duplicate_group"

    Examples:
      | export        | choice | file                   | rows | received   | column        |
      | RkiExport     | Nein   | RKI-Export_*.xlsx      | 1    | 14.01.2026 | eing          |
      | RkiExport     | Ja     | RKI-Export_*.xlsx      | 2    | 14.01.2026 | eing          |
      | PubMlstExport | Nein   | PubMLST-Export_*.xlsx  | 1    | 2026-01-14 | date_received |
      | PubMlstExport | Ja     | PubMLST-Export_*.xlsx  | 2    | 2026-01-14 | date_received |
