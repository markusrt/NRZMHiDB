@spec-019
Feature: PubMLST matching (Meningococci)
  As-is behavior of HaemophilusWeb, verified on 2026-11-08 against build 7bb4b55.
  Source: specs/019-pubmlst-matching-meningococci/spec.md
  The matching submission and the individual lookup would contact PubMLST and IRIS. No scenario submits the matching
  form; the lookup is always answered in the browser with synthetic data, so PubMLST is never contacted.

  Background:
    Given I am signed in as "Standard1"

  @smoke @US1 @FR-001 @FR-003
  Scenario: Open the matching form from the menu
    When I click "PubMLST"
    And I click "Matching (Meningokokken)"
    Then I am on "/MeningoIsolate/PubMlstMatch"
    And the page title is "PubMLST Matching"
    And I see "Entnahmedatum"
    And "Start-Datum" contains January 1st of the current year
    And "Ende-Datum" contains December 31st of the current year
    And I see "Hinweis: Die Abfrage nimmt einige Zeit in Anspruch. Die Seite lädt automatisch neu sobald alle Daten von PubMLST geladen sind."
    And I see the button "Abfrage der PubMLST Daten"

  @US4 @FR-001 @FR-002
  Scenario: A Public Health account is neither offered nor admitted to the matching
    Given I am signed in as "Rki1"
    Then I do not see the link "Matching (Meningokokken)"
    When I open "/MeningoIsolate/PubMlstMatch"
    Then I am on the sign-in page

  @US4 @FR-002
  Scenario: An Administrator-only account is refused by the matching page
    Given I am signed in as "Admin1"
    When I open "/MeningoIsolate/PubMlstMatch"
    Then I am on the sign-in page

  @US4 @FR-002
  Scenario: Without sign-in the matching page leads to the sign-in page
    Given I am not signed in
    When I open "/MeningoIsolate/PubMlstMatch"
    Then I am on the sign-in page

  @US2 @FR-011
  Scenario: A record found by the individual lookup is shown on the isolate form
    Given a Meningococci submission with sender laboratory number "SV-019-Lookup" exists
    And the PubMLST lookup is answered in the browser with PubMLST id "99019", database "pubmlst_neisseria_iris" and ST "SV-ST"
    When I open the isolate of the Meningococci submission "SV-019-Lookup"
    And I enter "19001" into "Stammnummer"
    And I click the button "Änderungen speichern"
    And I open the isolate of the Meningococci submission "SV-019-Lookup"
    And I open the tab "PubMLST"
    And I click "Abfragen"
    Then I see "Isolat wurde geladen"
    And "ST" contains "SV-ST"

  @US2 @FR-011
  Scenario: A lookup without record reports "Isolat wurde nicht gefunden"
    Given a Meningococci submission with sender laboratory number "SV-019-NotFound" exists
    And the PubMLST lookup is answered in the browser with "not found"
    When I open the isolate of the Meningococci submission "SV-019-NotFound"
    And I enter "19002" into "Stammnummer"
    And I click the button "Änderungen speichern"
    And I open the isolate of the Meningococci submission "SV-019-NotFound"
    And I open the tab "PubMLST"
    And I click "Abfragen"
    Then I see "Isolat wurde nicht gefunden"
    And "ST" contains an empty value
