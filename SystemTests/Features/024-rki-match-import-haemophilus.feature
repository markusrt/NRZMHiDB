@spec-024
Feature: RKI match import (Haemophilus)
  As-is behavior of HaemophilusWeb, verified on 2026-11-08 against build a9aa988.
  Source: specs/024-rki-match-import-haemophilus/spec.md
  The match files are synthetic and reference only SV-024- submissions; no real RKI file is used. Reading, matching,
  the status words and the "best status wins" rule are asserted by unit tests in
  HaemophilusWebTests/Tools/RkiMatchImporterTests and HaemophilusWebTests/Controllers/RkiMatchControllerTests; these
  scenarios cover the page flow, the rollback of a failed upload and the export, which only the running system shows.

  Background:
    Given I am signed in as "Standard1"

  @smoke @US1 @US3 @US4 @US5 @FR-001 @FR-005 @FR-007 @FR-010 @FR-011
  Scenario: Upload an RKI match file and find the match in the laboratory export
    Given a submission with sender laboratory number "SV-024-A", sampling location "Blut" and sampling date "05.03.2012" exists
    When I open the isolate of the submission "SV-024-A"
    And I enter "92401" into "Stammnummer"
    And I click the button "Änderungen speichern"
    And I open "/RkiMatch"
    Then I see "RKI Matching Daten"
    And I see "CSV Datei"
    When I choose a file "SV-024-match.csv" for "CSV Datei" with the lines:
      """
      InterneRef;Aktenzeichen;StatusName;klhi_nr;LaufendeNummer;Jahr
      90240101;SV-024-AZ-A1;definitiv;92401;;
      90240102;SV-024-AZ-A2;wahrscheinlich;92401;;
      """
    And I click the button "Upload"
    Then I am on "/RkiMatch"
    And I see "File Uploaded Successfully!!"
    When I open "/PatientSending/LaboratoryExport"
    And I enter "01.01.2012" into "Start-Datum"
    And I enter "31.12.2012" into "Ende-Datum"
    And I click "Exportieren nach Excel"
    Then a file named like "Labor-Export_20120101-20121231.xlsx" is downloaded
    And the downloaded file has 1 row(s) with "SV-024-AZ-A1" in column "RKI Aktenzeichen"
    And the downloaded file has 1 row(s) with "definitiv" in column "RKI Status"

  @US6 @FR-009 @FR-010
  Scenario: A row with an unknown status word fails the upload and keeps no row of the file
    Given a submission with sender laboratory number "SV-024-G", sampling location "Blut" and sampling date "06.03.2012" exists
    And a submission with sender laboratory number "SV-024-H", sampling location "Blut" and sampling date "07.03.2012" exists
    When I open the isolate of the submission "SV-024-G"
    And I enter "92407" into "Stammnummer"
    And I click the button "Änderungen speichern"
    And I open the isolate of the submission "SV-024-H"
    And I enter "92408" into "Stammnummer"
    And I click the button "Änderungen speichern"
    And I open "/RkiMatch"
    And I choose a file "SV-024-unknown-status.csv" for "CSV Datei" with the lines:
      """
      InterneRef;Aktenzeichen;StatusName;klhi_nr;LaufendeNummer;Jahr
      90240701;SV-024-AZ-G1;definitiv;92407;;
      90240801;SV-024-AZ-H1;xyz;92408;;
      """
    And I click the button "Upload"
    Then I see "File upload failed: 'xyz' is not a valid value for 'RkiStatus'"
    When I open "/PatientSending/LaboratoryExport"
    And I enter "01.01.2012" into "Start-Datum"
    And I enter "31.12.2012" into "Ende-Datum"
    And I click "Exportieren nach Excel"
    Then a file named like "Labor-Export_20120101-20121231.xlsx" is downloaded
    And the downloaded file has 0 row(s) with "SV-024-AZ-G1" in column "RKI Aktenzeichen"

  @US1 @FR-001
  Scenario: The page is not offered in the navigation
    Given I am signed in as "Labor1"
    When I open "/"
    Then I do not see the link "RKI Matching Daten"

  @US1 @FR-001
  Scenario: Without sign-in the page leads to the sign-in page
    Given I am not signed in
    When I open "/RkiMatch"
    Then I am on the sign-in page
