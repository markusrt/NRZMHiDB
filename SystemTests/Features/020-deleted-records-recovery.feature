@spec-020
Feature: Deleted-records recovery
  As-is behavior of HaemophilusWeb, verified on 2026-11-08 against build b7f58e4.
  Source: specs/020-deleted-records-recovery/spec.md

  Background:
    Given I am signed in as "Standard1"

  @smoke @US1 @FR-001 @FR-003 @FR-004 @FR-006
  Scenario: Delete a sender, find it under "Gelöschte Einsender" and restore it
    Given a sender "SV-020-Alpha" exists
    When I open the edit page of "SV-020-Alpha"
    And I click "Eintrag löschen"
    Then the page title is "Löschen bestätigen"
    When I click the button "Eintrag löschen"
    Then I am on "/Sender"
    And the sender list does not contain "SV-020-Alpha"
    When I open "/PatientSending/Create"
    Then "Einsendernummer" does not offer "SV-020-Alpha"
    When I click "Administration"
    And I click "Gelöschte Einsender"
    Then I am on "/Sender/Deleted"
    And the page title is "Gelöschte Einsender"
    And I see "Postleitzahl"
    When I click "Wiederherstellen" in the row "SV-020-Alpha"
    Then I am on "/Sender"
    And the sender list contains "SV-020-Alpha"
    When I open "/PatientSending/Create"
    Then the sender selection offers "SV-020-Alpha"
    When I open "/MeningoPatientSending/Create"
    Then the sender selection offers "SV-020-Alpha"

  @US1 @FR-006 @SC-003
  Scenario: A restored sender is listed in the sender export again
    Given a submission of sender "SV-020-Bravo" with sampling date "10.03.2016" exists
    When I open the edit page of "SV-020-Bravo"
    And I click "Eintrag löschen"
    And I click the button "Eintrag löschen"
    And I open "/Sender/Export"
    And I enter "01.01.2016" into "Start-Datum"
    And I enter "31.12.2016" into "Ende-Datum"
    And I click "Exportieren nach Excel"
    Then a file named like "Einsender-Haemophilus-Export_20160101-20161231.xlsx" is downloaded
    And the downloaded file has 0 rows with "SV-020-Bravo" in column "Name"
    When I open "/Sender/Deleted"
    And I click "Wiederherstellen" in the row "SV-020-Bravo"
    And I open "/Sender/Export"
    And I enter "01.01.2016" into "Start-Datum"
    And I enter "31.12.2016" into "Ende-Datum"
    And I click "Exportieren nach Excel"
    Then the downloaded file has 1 row with "SV-020-Bravo" in column "Name"

  @FR-003 @FR-011 @SC-006
  Scenario: The stem number of a deleted submission stays assigned
    Given a submission with sender laboratory number "SV-020-H1" exists
    And a submission with sender laboratory number "SV-020-H2" exists
    When I open the isolate of the submission "SV-020-H2"
    And I enter "92021" into "Stammnummer"
    And I click the button "Änderungen speichern"
    And I open the edit page of the submission "SV-020-H2"
    And I click "Eintrag löschen"
    Then the page title is "Löschen bestätigen"
    When I click the button "Eintrag löschen"
    Then I am on "/PatientSending"
    And the submission list does not contain "SV-020-H2"
    When I open the isolate of the submission "SV-020-H1"
    And I enter "92021" into "Stammnummer"
    And I click the button "Änderungen speichern"
    Then I see the validation message "Diese Stammnummer ist bereits vergeben"

  @FR-010
  Scenario: Intake still finds the patient of a deleted submission as a duplicate
    Given a submission with sender laboratory number "SV-020-D1" for patient "Q.Y." born "08.08.2008" with postal code "99021" exists
    When I open the edit page of the submission "SV-020-D1"
    And I click "Eintrag löschen"
    And I click the button "Eintrag löschen"
    And I open "/PatientSending/Create"
    And I select "SV-001-Alpha" in "Einsendernummer"
    And I enter "SV-020-D2" into "Labnr. Einsender"
    And I enter "Q.Y." into "Initialen"
    And I enter "08.08.2008" into "Geburtsdatum"
    And I choose "weiblich" for "Geschlecht"
    And I enter "99021" into "Postleitzahl"
    And I click "Neu anlegen"
    Then I see "Ein Patient mit den selben Initialen, Geburtsdatum und Postleitzahl existiert bereits."

  @US5 @FR-001 @FR-002
  Scenario: A Public Health account is neither offered nor admitted to the deleted-records pages
    Given I am signed in as "Rki1"
    Then I do not see the link "Gelöschte Einsender"
    And I do not see the link "Gelöschte Einsendungen (Haemophilus)"
    When I open "/Sender/Deleted"
    Then I am on the sign-in page
    When I open "/PatientSending/Deleted"
    Then I am on the sign-in page
