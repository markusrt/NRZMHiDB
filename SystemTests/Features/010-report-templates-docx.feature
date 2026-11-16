@spec-010
Feature: Report templates (.docx)
  As-is behavior of HaemophilusWeb, verified on 2026-11-08 against build 214b7fd.
  Source: specs/010-report-templates-docx/spec.md
  Top-level-only discovery, the per-request listing, discovery without an isolate and the Meningococci template folder
  are asserted in HaemophilusWebTests/Utils/FileListerTests, Controllers/ReportControllerTests and
  Controllers/MeningoReportControllerTests. Report state transitions are covered by 008 and 009.

  # Automation note: both report pages query the RKI tool on load; block /Report/QueryHealthOffice in the step definition.

  Background:
    Given I am signed in as "Standard1"

  @smoke @US1 @US2 @FR-002 @FR-005 @FR-006 @FR-009 @FR-010 @FR-012
  Scenario: Choose a Haemophilus template and download the filled report
    Given a submission with sender laboratory number "SV-010-A" exists
    When I click "Befund erstellen" in the row "SV-010-A"
    Then the page title is "Befund erstellen"
    And "Befundsvorlage" shows "Auswählen..."
    And "Befundsvorlage" offers 22 entries
    And "Befundsvorlage" offers "BLNAS v10.docx"
    And "Befundsvorlage" offers "Fax - Teilbefund - Serotyp v4.docx"
    And "Befundsvorlage" does not offer "Untersuchungsbefund v2.docx"
    And "Befundsvorlage" does not offer "Fax - Deckblatt.docx"
    When I select "BLNAS v10.docx" in "Befundsvorlage"
    And I select "Dr. Jane Doe" in "Unterzeichner"
    And I click "Befund erstellen"
    Then a file named like "KL *_*-????-??-??.docx" is downloaded
    And the downloaded document contains "SV-010-A"
    And the downloaded document contains "Dr. Jane Doe"

  @US1 @FR-002 @FR-005
  Scenario: The Meningococci report page offers only the Meningococci templates
    Given a Meningococci submission with sender laboratory number "SV-010-M" exists
    When I click "Befund erstellen" in the row "SV-010-M"
    Then the page title is "Befund erstellen"
    And "Befundsvorlage" shows "Auswählen..."
    And "Befundsvorlage" offers 6 entries
    And "Befundsvorlage" offers "Untersuchungsbefund v2.docx"
    And "Befundsvorlage" offers "Fax - Teilbefund - v2.docx"
    And "Befundsvorlage" does not offer "BLNAS v10.docx"
    And "Befundsvorlage" does not offer "Signature.docx"

  @US3 @FR-007
  Scenario: The Meningococci template marked "Teilbefund - " creates a preliminary report
    Given a Meningococci submission with sender laboratory number "SV-010-M" exists
    When I click "Befund erstellen" in the row "SV-010-M"
    And I select "Fax - Teilbefund - v2.docx" in "Befundsvorlage"
    And I select "Dr. John Doe" in "Unterzeichner"
    And I click "Befund erstellen"
    Then a file named like "MZ *_*-????-??-??.docx" is downloaded
    And the downloaded document contains "Fax- Empfangsbestätigung"
    And the Meningococci submission list row "SV-010-M" shows the report status "Teilbefund"
