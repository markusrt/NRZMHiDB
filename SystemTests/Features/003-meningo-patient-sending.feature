@spec-003
Feature: Meningococci patient + sending intake
  As-is behavior of HaemophilusWeb, verified on 2026-10-30 against build c1e6d9f.
  Source: specs/003-meningo-patient-sending/spec.md

  Background:
    Given I am signed in as "Standard1"

  @smoke @US1 @FR-001 @FR-002 @FR-003 @FR-015
  Scenario: Record a Meningococci submission together with its patient and find it in the list
    Given a sender "SV-003-Alpha" exists
    When I open "/MeningoPatientSending"
    And I click "Neue Einsendung erfassen"
    Then the page title is "Einsendung erfassen"
    And "Spezies Einsender" contains the text "N. meningitidis"
    When I select "SV-003-Alpha" in "Einsendernummer"
    And I enter "10.03.2020" into "Entnahmedatum"
    And I enter "12.03.2020" into "Eingangsdatum"
    And I enter "SV-003-A1" into "Labnr. Einsender"
    And I choose "Liquor" for "Entnahmeort"
    And I enter "X.A." into "Initialen"
    And I enter "01.02.2001" into "Geburtsdatum"
    And I choose "weiblich" for "Geschlecht"
    And I enter "97080" into "Postleitzahl"
    And I select "Bayern" in "Bundesland"
    And I click "Neu anlegen"
    Then I am on "/MeningoPatientSending"
    And the Meningococci submission list row "SV-003-A1" shows "X.A."
    And the Meningococci submission list row "SV-003-A1" shows "Liquor"
    And the Meningococci submission list row "SV-003-A1" shows "Ja"

  @US2 @FR-010 @FR-011
  Scenario: Attach a new Meningococci submission to an existing patient after the duplicate prompt
    Given a Meningococci submission for patient "X.A." born "01.02.2001" with postal code "97080" exists
    When I open "/MeningoPatientSending/Create"
    And I select "SV-003-Alpha" in "Einsendernummer"
    And I enter "SV-003-D" into "Labnr. Einsender"
    And I enter "X.A." into "Initialen"
    And I enter "01.02.2001" into "Geburtsdatum"
    And I choose "weiblich" for "Geschlecht"
    And I enter "97080" into "Postleitzahl"
    And I select "Bayern" in "Bundesland"
    And I click "Neu anlegen"
    Then I see "Ein Patient mit den selben Initialen, Geburtsdatum und Postleitzahl existiert bereits."
    When I choose "Bestehenden Patienten verwenden" for "Konfliktlösung"
    And I click "Neu anlegen"
    Then I am on "/MeningoPatientSending"
    And the Meningococci submission list row "SV-003-D" shows "X.A."

  @US3 @FR-015
  Scenario: Find a Meningococci submission by sender laboratory number in the extended search
    Given a Meningococci submission with sender laboratory number "SV-003-A1" exists
    When I open "/MeningoPatientSending"
    And I click "Erweiterte Suche"
    Then I see "Einfache Suche"
    And I see the search field "Patienten-Nr."
    When I enter "SV-003-A1" into the search field "Einsender Labornummer"
    Then the Meningococci submission list shows 1 row

  @FR-022
  Scenario: The public-health account opens only the RKI and LGA exports
    Given I am signed in as "Rki1"
    When I open "/MeningoPatientSending/RkiExport"
    Then the page title is "RKI Export (Meningokokken)"
    When I open "/MeningoPatientSending/StateAuthorityExport"
    Then the page title is "Export LGA (Meningokokken)"
    When I open "/MeningoPatientSending/Create"
    Then I am on the sign-in page

  @FR-022
  Scenario Outline: Accounts without the laboratory role are sent to sign-in
    Given I am signed in as "<account>"
    When I open "/MeningoPatientSending"
    Then I am on the sign-in page

    Examples:
      | account |
      | Admin1  |
      | Rki1    |
