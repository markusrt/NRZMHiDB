@spec-002
Feature: Haemophilus patient + sending intake
  As-is behavior of HaemophilusWeb, verified on 2026-10-30 against build f475cea.
  Source: specs/002-patient-sending-intake/spec.md

  Background:
    Given I am signed in as "Standard1"

  @smoke @US1 @FR-001 @FR-002 @FR-003
  Scenario: Record a submission together with its patient and find it in the list
    Given a sender "SV-001-Alpha" exists
    When I open "/PatientSending"
    And I click "Neue Einsendung erfassen"
    Then the page title is "Einsendung erfassen"
    And "Spezies Einsender" contains the text "H. influenzae"
    When I select "SV-001-Alpha" in "Einsendernummer"
    And I enter "10.03.2020" into "Entnahmedatum"
    And I enter "12.03.2020" into "Eingangsdatum"
    And I enter "SV-002-A1" into "Labnr. Einsender"
    And I choose "Blut" for "Entnahmeort"
    And I enter "X.A." into "Initialen"
    And I enter "01.01.1990" into "Geburtsdatum"
    And I choose "weiblich" for "Geschlecht"
    And I enter "97080" into "Postleitzahl"
    And I click "Neu anlegen"
    Then I am on "/PatientSending"
    And the submission list row "SV-002-A1" shows "X.A."
    And the submission list row "SV-002-A1" shows "12.03.2020"

  @US2 @FR-009 @FR-010
  Scenario: Attach a new submission to an existing patient after the duplicate prompt
    Given a submission for patient "Y.D." born "03.03.2003" with postal code "97082" exists
    When I open "/PatientSending/Create"
    And I select "SV-001-Alpha" in "Einsendernummer"
    And I enter "SV-002-B2" into "Labnr. Einsender"
    And I enter "Y.D." into "Initialen"
    And I enter "03.03.2003" into "Geburtsdatum"
    And I choose "weiblich" for "Geschlecht"
    And I enter "97082" into "Postleitzahl"
    And I click "Neu anlegen"
    Then I see "Ein Patient mit den selben Initialen, Geburtsdatum und Postleitzahl existiert bereits."
    When I choose "Bestehenden Patienten verwenden" for "Konfliktlösung"
    And I click "Neu anlegen"
    Then I am on "/PatientSending"
    And the submission list row "SV-002-B2" shows "Y.D."

  @US3 @FR-016 @FR-017
  Scenario: Delete a submission through the confirmation page
    Given a submission with sender laboratory number "SV-002-DEL" exists
    When I open the edit page of the submission "SV-002-DEL"
    Then the page title is "Einsendung bearbeiten"
    When I click "Eintrag löschen"
    Then the page title is "Löschen bestätigen"
    And I see "Sind Sie sicher, dass sie die folgende Einsendung löschen wollen?"
    And I see "Labornummer"
    When I click the button "Eintrag löschen"
    Then I am on "/PatientSending"
    And the submission list does not contain "SV-002-DEL"

  @US3 @FR-013
  Scenario: Find a submission by sender laboratory number in the extended search
    Given a submission with sender laboratory number "SV-002-A1" exists
    When I open "/PatientSending"
    And I click "Erweiterte Suche"
    Then I see "Einfache Suche"
    When I enter "SV-002-A1" into the search field "Einsender Labornummer"
    Then the submission list shows 1 row

  @FR-020
  Scenario Outline: Accounts without the laboratory role are sent to sign-in
    Given I am signed in as "<account>"
    When I open "/PatientSending/Create"
    Then I am on the sign-in page

    Examples:
      | account |
      | Admin1  |
      | Rki1    |
