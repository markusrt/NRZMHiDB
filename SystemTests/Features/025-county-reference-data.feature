@spec-025
Feature: County reference data management
  As-is behavior of HaemophilusWeb, verified on 2026-11-08 against build bfb97df.
  Source: specs/025-county-reference-data/spec.md

  Background:
    Given I am signed in as "Standard1"

  @smoke @US2 @FR-009 @FR-010 @FR-003
  Scenario: Upload a GV100AD file and find its county records in the list
    When I open "/County/Upload"
    Then I see "GV100AD acs-Datei hochladen"
    When I choose a file "SV-025-gv100ad.asc" for "ACS-Datei:" with the lines:
      """
      102014123199          SV-025-Land
      402014103199001       SV-025-Flensburg, Stadt                           SV-025-Ort
      402014123199002       SV-025-Kiel, Landeshauptstadt                     SV-025-Ort
      """
    And I click the button "Hochladen"
    Then I am on "/County"
    And I see "SV-025-Flensburg, Stadt"
    And I see "99001"
    And I see "31.10.2014 00:00:00"
    And I see "SV-025-Kiel, Landeshauptstadt"
    And I do not see "SV-025-Land"

  @US2 @FR-009
  Scenario: The upload page asks for a file when opened and when sent without one
    When I open "/County/Upload"
    Then I see "Bitte geben sie eine Datei an."
    When I click the button "Hochladen"
    Then I am on "/County/Upload"
    And I see "Bitte geben sie eine Datei an."

  @US4 @FR-005 @FR-003
  Scenario: Create a county record by hand with a free-text number
    When I open "/County"
    And I click "Create New"
    And I enter "abc" into "CountyNumber"
    And I enter "SV-025-Frei" into "Name"
    And I enter "01.01.2016" into "ValidSince"
    And I click the button "Create"
    Then I am on "/County"
    And I see "SV-025-Frei"
    And I see "01.01.2016 00:00:00"

  @US4 @FR-005
  Scenario: The validity date is checked by the server
    When I open "/County/Create"
    And I enter "SV-025-Leer" into "Name"
    And I click the button "Create"
    Then I am on "/County/Create"
    And I see the validation message "Eingabe erforderlich"
    When I enter "kein Datum" into "ValidSince"
    And I click the button "Create"
    Then I see the validation message "ValidSince ist kein gültiger Wert für kein Datum."

  @US4 @FR-007
  Scenario: Delete a county record after confirmation
    Given I open "/County/Create"
    And I enter "SV-025-Weg" into "Name"
    And I enter "01.01.2016" into "ValidSince"
    And I click the button "Create"
    When I click "Delete" in the row "SV-025-Weg"
    Then I see "Are you sure you want to delete this?"
    When I click the button "Delete"
    Then I am on "/County"
    And I do not see "SV-025-Weg"

  @US3 @FR-001 @FR-002
  Scenario Outline: Accounts without the Standard User role are sent to the sign-in page
    Given I am signed in as "<account>"
    Then I do not see the link "County"
    When I open "<address>"
    Then I am on the sign-in page

    Examples:
      | account | address        |
      | Admin1  | /County        |
      | Admin1  | /County/Upload |
      | Rki1    | /County        |
      | Rki1    | /County/Upload |
