@spec-015
Feature: Merge patients (Meningococci)
  As-is behavior of HaemophilusWeb, verified on 2026-11-08 against build 13c8e5e.
  Source: specs/015-merge-patients-meningococci/spec.md

  Background:
    Given I am signed in as "Standard1"

  @smoke @US1 @US3 @FR-003 @FR-005 @FR-006 @FR-010
  Scenario: Merge a duplicate Meningococci patient into the patient to keep
    Given a Meningococci submission with sender laboratory number "SV-015-Alpha" for patient "R.K." born "01.01.1990" with postal code "99971" exists
    And a Meningococci submission with sender laboratory number "SV-015-Beta" for patient "R.L." born "02.02.1991" with postal code "99972" exists
    When I open "/MeningoPatientSending/MergePatient"
    And I enter the patient number of the Meningococci submission "SV-015-Alpha" into "Patienten-Nr. 1"
    And I enter the patient number of the Meningococci submission "SV-015-Beta" into "Patienten-Nr. 2"
    And I click "Zusammenfügen vorbereiten"
    Then the page title is "Patienten Zusammenfügen (Meningokokken)"
    And I see "Bestätigung"
    And I see "Initialen: R.K."
    And I see "Labnr. Einsender: SV-015-Beta"
    And I see "Stammnummer: DE -"
    When I choose "Patient 1" for "Zusammenfügen zu"
    And I click "Zusammenfügen und doppelten Patienten löschen"
    Then I see "Erfolgreich"
    And I see "Alle Einsendungen wurden diesem Patienten zugeordnet."
    And I see "Der Patient wurde gelöscht."
    And the Meningococci submission list row "SV-015-Beta" shows "R.K."
    And the Meningococci submission list filtered by patient number of "SV-015-Alpha" shows 2 rows
    When I click "Zurück zur Übersicht"
    Then I am on "/MeningoPatientSending"

  @US1 @FR-001 @FR-003
  Scenario: Open the Meningococci merge page from the Administration menu
    When I click "Administration"
    And I click "Patienten Zusammenfügen" in the menu section "Meningokokken"
    Then I am on "/MeningoPatientSending/MergePatient"
    And the page title is "Patienten Zusammenfügen (Meningokokken)"
    And I see "Patientennummern"
    And I see the button "Zusammenfügen vorbereiten"

  @US3 @FR-010
  Scenario: Cancelling the confirmation returns to the Meningococci merge page
    Given a Meningococci submission with sender laboratory number "SV-015-Gamma" for patient "R.M." born "03.03.1992" with postal code "99973" exists
    And a Meningococci submission with sender laboratory number "SV-015-Delta" for patient "R.N." born "04.04.1993" with postal code "99974" exists
    When I open "/MeningoPatientSending/MergePatient"
    And I enter the patient number of the Meningococci submission "SV-015-Gamma" into "Patienten-Nr. 1"
    And I enter the patient number of the Meningococci submission "SV-015-Delta" into "Patienten-Nr. 2"
    And I click "Zusammenfügen vorbereiten"
    And I click "Abbrechen"
    Then I am on "/MeningoPatientSending/MergePatient"
    And the page title is "Patienten Zusammenfügen (Meningokokken)"
    And the Meningococci submission list row "SV-015-Delta" shows "R.N."

  @FR-001 @FR-002
  Scenario: The Public Health role neither sees nor reaches the Meningococci merge page
    Given I am signed in as "Rki1"
    Then I do not see the link "Patienten Zusammenfügen"
    When I open "/MeningoPatientSending/MergePatient"
    Then I am on the sign-in page
