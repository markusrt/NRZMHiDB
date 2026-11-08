@spec-009
Feature: Meningococci reporting (preliminary + final)
  As-is behavior of HaemophilusWeb, verified on 2026-11-08 against build b86e578.
  Source: specs/009-meningococci-reporting/spec.md
  Report state transitions, the unknown-isolate outcomes, the Azithromycin removal and the template folder and signer
  list are asserted in HaemophilusWebTests/Controllers/MeningoReportControllerTests.

  # Automation note: the report page queries the RKI tool on load; block /Report/QueryHealthOffice in the step definition.

  Background:
    Given I am signed in as "Standard1"

  @smoke @US1 @FR-001 @FR-005 @FR-013 @FR-014 @FR-016 @FR-021
  Scenario: Save the isolate, create a final report and see it as final in the list
    Given a Meningococci submission with sender laboratory number "SV-009-A" exists
    When I open the isolate of the Meningococci submission "SV-009-A"
    And I click the button "Änderungen speichern und Befund erstellen"
    Then the page title is "Befund erstellen"
    And I see "Befund (Auszug)"
    And I see "Zuständiges Gesundheitsamt"
    When I select "Untersuchungsbefund v2.docx" in "Befundsvorlage"
    And I select "Dr. Jane Doe" in "Unterzeichner"
    And I click "Befund erstellen"
    Then a file named like "MZ *_*-????-??-??.docx" is downloaded
    And the Meningococci submission list row "SV-009-A" shows the report status "Endbefund"

  @US2 @FR-010 @FR-018
  Scenario: A template marked "Teilbefund - " creates a preliminary report
    Given a Meningococci submission with sender laboratory number "SV-009-C" exists
    When I open "/MeningoPatientSending"
    And I click "Befund erstellen" in the row "SV-009-C"
    Then the page title is "Befund erstellen"
    When I select "Fax - Teilbefund - v2.docx" in "Befundsvorlage"
    And I select "Dr. John Doe" in "Unterzeichner"
    And I click "Befund erstellen"
    Then a file named like "MZ *.docx" is downloaded
    And the Meningococci submission list row "SV-009-C" shows the report status "Teilbefund"

  @US3 @FR-009
  Scenario: A report needs a template and a signer
    Given a Meningococci submission with sender laboratory number "SV-009-C" exists
    When I open "/MeningoPatientSending"
    And I click "Befund erstellen" in the row "SV-009-C"
    And I select "Untersuchungsbefund v2.docx" in "Befundsvorlage"
    And I click "Befund erstellen"
    Then I see "Bitte wählen Sie eine Befundvorlage und einen Unterzeichner aus."
    And no file is downloaded

  @US1 @FR-007
  Scenario: The report page offers the Meningococci templates and the configured signers
    Given a Meningococci submission with sender laboratory number "SV-009-C" exists
    When I open "/MeningoPatientSending"
    And I click "Befund erstellen" in the row "SV-009-C"
    Then "Befundsvorlage" offers "Untersuchungsbefund v2.docx"
    And "Befundsvorlage" offers "Fax - Teilbefund - v2.docx"
    And "Befundsvorlage" offers "Nativmaterial v2.docx"
    And "Unterzeichner" offers "Dr. Jane Doe"
    And "Unterzeichner" offers "Dr. John Doe"
    And I see the link "Isolat bearbeiten"
