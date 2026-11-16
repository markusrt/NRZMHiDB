@spec-023
Feature: EpiScanGIS export (Meningococci)
  As-is behavior of HaemophilusWeb, verified on 2026-11-08 against build 121be4f.
  Source: specs/023-episcangis-export-meningococci/spec.md
  The export is admitted by the secret RegistrationToken setting, whose value is never used in tests. Selection,
  reduction per patient, line format, coordinates and file handling are asserted by unit tests in
  HaemophilusWebTests/Controllers/EpiscanGisExportControllerTests; these scenarios cover the refusal without a valid
  token and the absence from the navigation, which a browser can observe.

  @smoke @US1 @FR-001
  Scenario: The export address without a token is answered with "not found"
    Given I am not signed in
    When I open "/EpiscanGisExport"
    Then the server answers "not found"
    And no file is downloaded

  @US1 @FR-001 @FR-002
  Scenario Outline: A signed-in account with a wrong token gets "not found" as well
    Given I am signed in as "<account>"
    When I open "/EpiscanGisExport?token=SV-023-wrong"
    Then the server answers "not found"
    And no file is downloaded

    Examples:
      | account   |
      | Standard1 |
      | Admin1    |
      | Rki1      |
      | Labor1    |

  @US1 @FR-002
  Scenario: The export is not offered in the navigation
    Given I am signed in as "Labor1"
    When I open "/"
    Then I do not see the link "EpiScanGIS Export"
