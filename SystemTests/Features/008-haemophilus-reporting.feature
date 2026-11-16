@spec-008
Feature: Haemophilus reporting (preliminary + final)
  As-is behavior of HaemophilusWeb, verified on 2026-11-08 against build 0dabc6d.
  Source: specs/008-haemophilus-reporting/spec.md
  Report state transitions, the unknown-isolate outcomes and the template and signer lists are asserted in
  HaemophilusWebTests/Controllers/ReportControllerTests; the discrepancy dialog is covered by 006.

  # Automation note: the report page queries the RKI tool on load; block /Report/QueryHealthOffice in the step definition.

  Background:
    Given I am signed in as "Standard1"
    And a submission with sender laboratory number "SV-008-B" exists

  @smoke @US1 @FR-001 @FR-005 @FR-012 @FR-013 @FR-015 @FR-016 @FR-020
  Scenario: Save the isolate, create a final report and see it as final in the list
    When I open the isolate of the submission "SV-008-B"
    And I choose "Ja" for "Wachstum"
    And I choose "negativ" for "Agglutination"
    And I choose "negativ" for "bexA"
    And I click the button "Änderungen speichern und Befund erstellen"
    Then the page title is "Befund erstellen"
    And I see "Zuständiges Gesundheitsamt"
    When I select "BLNAS v10.docx" in "Befundsvorlage"
    And I select "Dr. Jane Doe" in "Unterzeichner"
    And I click "Befund erstellen"
    Then a file named like "KL *_*-????-??-??.docx" is downloaded
    And the submission list row "SV-008-B" shows the report status "Endbefund"

  @US2 @FR-008 @FR-017
  Scenario: A template marked "Teilbefund - " creates a preliminary report
    When I open the isolate of the submission "SV-008-B"
    And I choose "Ja" for "Wachstum"
    And I choose "a" for "Agglutination"
    And I choose "n.d." for "bexA"
    And I choose "n.d." for "Serotyp-PCR"
    And I click the button "Änderungen speichern"
    And I click "Befund erstellen" in the row "SV-008-B"
    Then the page title is "Befund erstellen"
    When I select "Fax - Teilbefund - Serotyp v4.docx" in "Befundsvorlage"
    And I select "Dr. John Doe" in "Unterzeichner"
    And I click "Befund erstellen"
    Then a file named like "KL *.docx" is downloaded
    And the submission list row "SV-008-B" shows the report status "Teilbefund"

  @US3 @FR-007
  Scenario: A report needs a template and a signer
    When I click "Befund erstellen" in the row "SV-008-B"
    And I select "BLNAS v10.docx" in "Befundsvorlage"
    And I click "Befund erstellen"
    Then I see "Bitte wählen Sie eine Befundvorlage und einen Unterzeichner aus."
    And no file is downloaded

  @US1 @FR-005
  Scenario: The report page offers the Haemophilus templates and the configured signers
    When I click "Befund erstellen" in the row "SV-008-B"
    Then "Befundsvorlage" offers "BLNAS v10.docx"
    And "Befundsvorlage" offers "Fax - Teilbefund - Serotyp v4.docx"
    And "Unterzeichner" offers "Dr. Jane Doe"
    And "Unterzeichner" offers "Dr. John Doe"
    And I see the link "Isolat bearbeiten"
