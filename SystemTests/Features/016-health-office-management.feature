@spec-016
Feature: Health office management
  As-is behavior of HaemophilusWeb, verified on 2026-11-08 against build 40b979c.
  Source: specs/016-health-office-management/spec.md
  # Automation note: the report pages post /Report/QueryHealthOffice on load and the edit link carries ?postalCode=,
  # both of which ask the RKI tool; block them in the step definitions or answer them in the browser.

  Background:
    Given I am signed in as "Standard1"

  @smoke @US2 @FR-010 @FR-011 @FR-012
  Scenario: Correct the telephone number of a health office from the Administration menu
    Given a health office with address "SV-016-Amt Smoke|Teststraße 1|99031 Teststadt" and postal code "99031" exists
    When I click "Administration"
    And I click "Gesundheitsämter"
    Then the page title is "Gesundheitsämter"
    When I click "Bearbeiten" in the row "99031"
    Then the page title is "Gesundheitsamt bearbeiten"
    And I see the button "Änderungen speichern"
    When I enter "09999 22-4711" into "Telefon"
    And I click "Änderungen speichern"
    Then I am on "/HealthOffices"
    And the health-office list row "99031" shows "09999 22-4711"

  @US3 @FR-012
  Scenario: Create a health office on the page reached only by its address
    When I open "/HealthOffices/Create"
    Then the page title is "Neues Gesundheitsamt anlegen"
    When I enter "SV-016-Amt Neu" into "Adresse"
    And I enter "09999 11-1001" into "Telefon"
    And I enter "sv016-neu@example.org" into "Email"
    And I enter "99032" into "Postleitzahl"
    And I click "Neu anlegen"
    Then I am on "/HealthOffices"
    And the health-office list row "99032" shows "sv016-neu@example.org"

  @US1 @FR-001 @FR-007
  Scenario: The report page shows the health office the lookup returns
    Given the health-office lookup is answered in the browser with address "SV-016-Amt C|Teststraße 3|99011 Teststadt", telephone "09191 86-3504", fax "09191 86-3508" and email "sv016-c@example.org"
    When I open "/Report/Isolate/1"
    Then I see "Zuständiges Gesundheitsamt"
    And I see "Teststraße 3"
    And the link "09191 86-3504" points to "tel:09191863504"
    And the link "09191 86-3508" points to "mailto:009191863508@fax"
    And the link "sv016-c@example.org" points to "mailto:sv016-c@example.org"
    And "Gesundheitsamt bearbeiten" is enabled

  @FR-010
  Scenario: The Public Health account is not offered the health-office list
    Given I am signed in as "Rki1"
    Then I do not see the link "Gesundheitsämter"
