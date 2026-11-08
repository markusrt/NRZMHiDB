@spec-021
Feature: Authentication and authorization roles
  As-is behavior of HaemophilusWeb, verified on 2026-11-08 against build ef341c1.
  Source: specs/021-auth-authorization-roles/spec.md
  # Automation note: never use the value of the RegistrationToken setting and never change the password of a seeded
  # account. Registration, the password change, the account rules and the role checks of the access matrix are asserted
  # by unit tests (AccountControllerTests, AccountViewModelsTests, AuthorizationMatrixTests). The Administrator
  # refusals (DEC-001), the LGA export for Public Health (DEC-006) and the functions without role check (SEC-004) get
  # no scenario.

  @smoke @US1 @FR-001 @FR-002 @FR-003
  Scenario: Sign in from a deep page, return to it and sign out again
    Given I am not signed in
    When I open "/Sender"
    Then I am on the sign-in page
    And I see "Bitte melden Sie sich an:"
    And I see the link "Anmelden"
    And I do not see the link "Einsender"
    When I enter "Standard1" into "Benutzername"
    And I enter "123456" into "Passwort"
    And I click the button "Anmelden"
    Then I am on "/Sender"
    And I see the link "Über"
    And I see the link "Abmelden (Standard1)"
    When I click "Abmelden (Standard1)"
    Then I am on the sign-in page
    And I see the link "Anmelden"

  @US1 @FR-002
  Scenario: A wrong password does not say which part was wrong
    Given I am not signed in
    When I open "/Account/Login"
    And I enter "Standard1" into "Benutzername"
    And I enter "SV-021-wrong" into "Passwort"
    And I click the button "Anmelden"
    Then I am on the sign-in page
    And I see "Ungültiger Benutzername oder Passwort."

  @US2 @US4 @FR-005
  Scenario: Public Health sees only the RKI exports
    Given I am signed in as "Rki1"
    When I open "/"
    Then I see the link "RKI Export (Haemophilus)"
    And I see the link "RKI Export (Meningokokken)"
    And I see the link "RKI Export"
    And I do not see the link "Einsender"
    And I do not see the link "Administration"

  @US4 @FR-013 @EC1
  Scenario: A function the role is not admitted to leads to the sign-in page
    Given I am signed in as "Rki1"
    When I open "/Sender"
    Then I am on the sign-in page
    And I see the link "Abmelden (Rki1)"

  @US6 @FR-007
  Scenario: The registration address without token shows the sign-in page
    Given I am not signed in
    When I open "/Account/Register?token=SV-021-wrong"
    Then I am on "/Account/Register"
    And I see "Bitte melden Sie sich an:"

  @US5 @FR-010
  Scenario: The Administrator role manages roles on unlinked pages
    Given I am signed in as "Admin1"
    When I open "/Role"
    Then the page title is "Rollen"
    And I see "Standardbenutzer"
    When I click "Neue Rolle anlegen"
    Then I see "Bitte legen Sie eine neue Rolle an:"
    And I see the button "Neu anlegen"
