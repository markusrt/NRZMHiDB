@spec-001
Feature: Sender management
  As-is behavior of HaemophilusWeb, verified on 2026-10-30 against build 001be49.
  Source: specs/001-sender-management/spec.md

  Background:
    Given I am signed in as "Standard1"

  @smoke @US1 @FR-001 @FR-002 @FR-003
  Scenario: Create a sender and find it in the list and in both intake forms
    When I open "/Sender"
    And I click "Neuen Einsender anlegen"
    Then the page title is "Neuen Einsender anlegen"
    When I enter "SV-001-Alpha" into "Name"
    And I enter "0931 12345-0" into "Telefon #1"
    And I enter "sv-001-alpha@example.org" into "E-Mail"
    And I click "Neu anlegen"
    Then I am on "/Sender"
    And the sender list contains "SV-001-Alpha"
    When I open "/PatientSending/Create"
    Then the sender selection offers "SV-001-Alpha"
    When I open "/MeningoPatientSending/Create"
    Then the sender selection offers "SV-001-Alpha"

  @US1 @FR-006
  Scenario: Edit a sender
    Given a sender "SV-001-Alpha" exists
    When I open "/Sender"
    And I click "Bearbeiten" in the row "SV-001-Alpha"
    Then the page title is "Einsender bearbeiten"
    When I enter "SV-001 Abteilung Bakteriologie" into "Abteilung"
    And I click "Änderungen speichern"
    Then I am on "/Sender"
    And the sender list row "SV-001-Alpha" shows "SV-001 Abteilung Bakteriologie"

  @US2 @FR-008 @FR-011
  Scenario: Delete a sender without submissions and restore it
    Given a sender "SV-001-Beta" without submissions exists
    When I open the edit page of "SV-001-Beta"
    And I click "Eintrag löschen"
    Then the page title is "Löschen bestätigen"
    And I see "Sind Sie sicher, dass sie den folgenden Einsender löschen wollen?"
    When I click the button "Eintrag löschen"
    Then I am on "/Sender"
    And the sender list does not contain "SV-001-Beta"
    When I open "/Sender/Deleted"
    Then the page title is "Gelöschte Einsender"
    When I click "Wiederherstellen" in the row "SV-001-Beta"
    Then I am on "/Sender"
    And the sender list contains "SV-001-Beta"

  @US2 @FR-009 @FR-010
  Scenario: Deleting a sender with submissions warns and lists them
    Given the seeded sender "Nationales Referenzzentrum" has the submissions "KL001/20" and "MZ001/20"
    When I open "/Sender/Delete/1"
    Then I see "Dem Einsender \"Nationales Referenzzentrum\" sind noch Einsendungen zugeordnet."
    And I see the link "Einsendung KL001/20"
    And I see the link "Einsendung MZ001/20"

  @US3 @FR-013 @FR-014
  Scenario: Sender export proposes the previous calendar year
    When I open "/Sender/Export"
    Then the page title is "Export der Einsender (Haemophilus)"
    And "Start-Datum" contains January 1st of the previous year
    And "Ende-Datum" contains December 31st of the previous year
    When I click "Exportieren nach Excel"
    Then a file named like "Einsender-Haemophilus-Export_*.xlsx" is downloaded

  @FR-016
  Scenario Outline: Accounts without the laboratory role are sent to sign-in
    Given I am signed in as "<account>"
    When I open "/Sender"
    Then I am on the sign-in page

    Examples:
      | account |
      | Admin1  |
      | Rki1    |
