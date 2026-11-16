@spec-017
Feature: EUCAST clinical breakpoints management
  As-is behavior of HaemophilusWeb, verified on 2026-11-08 against build 6605431.
  Source: specs/017-eucast-clinical-breakpoints/spec.md
  # Automation note: the validation rules, the record label and the report values are asserted by unit tests
  # (EucastClinicalBreakpointValidatorTests, Models/EucastClinicalBreakpointTests, EpsilometerTestReportModelTests).

  Background:
    Given I am signed in as "Standard1"

  @smoke @US1 @US2 @FR-001 @FR-003 @FR-004
  Scenario: Create a Meningococci breakpoint from the Administration menu and find it in the list
    When I click "Administration"
    And I click "Antibiotika Grenzwerte"
    Then the page title is "Grenzwerte"
    When I click "Neuen Grenzwert anlegen"
    Then the page title is "Grenzwert anlegen"
    When I select "Ceftriaxon" in "Antibiotikum"
    And I enter "SV-017-Smoke Ceftriaxon" into "Antibiotikum Details"
    And I enter "1.0" into "Version"
    And I choose "Meningokokken" for "Gültig für"
    And I enter "01.03.2025" into "Gültig ab"
    And I enter "0,125" into "Sensibel <="
    And I enter "0,25" into "Resistent >"
    And I click "Neu anlegen"
    Then I am on "/EucastClinicalBreakpoints"
    And the breakpoint list row "SV-017-Smoke Ceftriaxon" shows "Meningococci"
    And the breakpoint list row "SV-017-Smoke Ceftriaxon" shows "01.03.2025 00:00:00"

  @US3 @FR-004 @FR-010
  Scenario: Correct the details of a breakpoint record
    Given a Meningococci breakpoint "SV-017-Edit Ceftriaxon" for "Ceftriaxon" with version "1.0", "Gültig ab" "01.03.2025", "Sensibel <=" "0,125" and "Resistent >" "0,25" exists
    When I open "/EucastClinicalBreakpoints"
    And I click "Bearbeiten" in the row "SV-017-Edit Ceftriaxon"
    Then the page title is "Grenzwert bearbeiten"
    And I see the button "Änderungen speichern"
    And I see the link "Zurück zur Liste"
    When I enter "SV-017-Edit Ceftriaxon korrigiert" into "Antibiotikum Details"
    And I click "Änderungen speichern"
    Then I am on "/EucastClinicalBreakpoints"
    And the breakpoint list row "SV-017-Edit Ceftriaxon korrigiert" shows "Ceftriaxon"

  @US2 @FR-011 @FR-008 @SC-002 @SC-003
  Scenario: A breakpoint for a new antibiotic is offered only on the isolate form of its pathogen, newest first
    Given a Meningococci breakpoint "SV-017-Offer Ceftriaxon" for "Ceftriaxon" with version "1.0", "Gültig ab" "01.03.2025", "Sensibel <=" "0,125" and "Resistent >" "0,125" exists
    And a Meningococci breakpoint "SV-017-Offer Ceftriaxon" for "Ceftriaxon" with version "2.0", "Gültig ab" "01.06.2025", "Sensibel <=" "0,25" and "Resistent >" "0,5" exists
    And a Meningococci submission with sender laboratory number "SV-017-L1" exists
    And a submission with sender laboratory number "SV-017-H1" exists
    When I open the isolate of the Meningococci submission "SV-017-L1"
    Then the free E-Test row offers the antibiotic "Ceftriaxon"
    When I select "Ceftriaxon" in the free E-Test row
    And I select "0,25" in the free E-Test row
    Then the free E-Test row shows the breakpoint "SV-017-Offer Ceftriaxon - v2.0 vom 01. Jun. 25"
    And the free E-Test row shows "sensibel"
    When I open the isolate of the submission "SV-017-H1"
    Then the free E-Test row does not offer the antibiotic "Ceftriaxon"

  @US1 @FR-001
  Scenario: The Public Health account is not offered the breakpoint list
    Given I am signed in as "Rki1"
    Then I do not see the link "Antibiotika Grenzwerte"
