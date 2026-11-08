@spec-014
Feature: Merge patients (Haemophilus)
  As-is behavior of HaemophilusWeb, verified on 2026-11-08 against build db63ae8.
  Source: specs/014-merge-patients-haemophilus/spec.md

  Background:
    Given I am signed in as "Standard1"

  @smoke @US1 @FR-003 @FR-006 @FR-007 @FR-008 @FR-009 @FR-012
  Scenario: Merge a duplicate patient into the patient to keep
    Given a submission with sender laboratory number "SV-014-Alpha" for patient "Q.K." born "01.01.1990" with postal code "99981" exists
    And a submission with sender laboratory number "SV-014-Beta" for patient "Q.L." born "02.02.1991" with postal code "99982" exists
    When I open "/PatientSending/MergePatient"
    And I enter the patient number of the submission "SV-014-Alpha" into "Patienten-Nr. 1"
    And I enter the patient number of the submission "SV-014-Beta" into "Patienten-Nr. 2"
    And I click "Zusammenfügen vorbereiten"
    Then I see "Bestätigung"
    And I see "Initialen: Q.K."
    And I see "Labnr. Einsender: SV-014-Beta"
    And I see the button "Zusammenfügen und doppelten Patienten löschen"
    When I choose "Patient 1" for "Zusammenfügen zu"
    And I click "Zusammenfügen und doppelten Patienten löschen"
    Then I see "Erfolgreich"
    And I see "Alle Einsendungen wurden diesem Patienten zugeordnet."
    And I see "Der Patient wurde gelöscht."
    And the submission list row "SV-014-Beta" shows "Q.K."

  @US1 @FR-001 @FR-003
  Scenario: Open the merge page from the Administration menu
    When I click "Administration"
    And I click "Patienten Zusammenfügen" in the menu section "Haemophilus"
    Then I am on "/PatientSending/MergePatient"
    And the page title is "Patienten Zusammenfügen (Haemophilus)"
    And I see "Patientennummern"
    And I see the button "Zusammenfügen vorbereiten"

  @FR-001 @FR-002
  Scenario: The Public Health role neither sees nor reaches the merge page
    Given I am signed in as "Rki1"
    Then I do not see the link "Patienten Zusammenfügen"
    When I open "/PatientSending/MergePatient"
    Then I am on the sign-in page
