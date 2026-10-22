# Feature Specification: Postal-code lookup (Geonames)

**Feature Branch**: `speckit-init`

**Created**: 2026-10-22

**Status**: Draft (status-quo inventory)

**Input**: User description: "Feature title: \"Postal-code lookup (Geonames)\" (backlog ref E2, Group E — Cross-cutting). Document the current, as-implemented behavior of the postal-code lookup and autocompletion used in sender and patient forms as it exists today, which resolves a German postal code to place name, county, and state and offers prefix-based suggestions. Read HaemophilusWeb/Controllers/GeonamesController.cs, HaemophilusWeb/Services/GeonamesService.cs and IGeonamesService.cs, the shared partial Views/Shared/_QueryPostalCodeViaGeonames.cshtml, and where it is included (Views/Sender/_CreateOrEdit.cshtml and the Patient and MeningoPatient editor templates). Capture the lookup endpoints (PostalCode, PostalCodeStartsWith), how the City/County/State fields are populated, the external Geonames API dependency and its configuration, and the countries-list source. Document observable behavior only — do not propose changes."

> **Scope note**: This is an *as-is* specification (backlog ref E2). It records the current, observable behavior of
> the postal-code assistance shared by the sender form and the patient part of the Haemophilus and Meningococci
> submission forms: the suggestions offered while a postal code is typed ("PostalCodeStartsWith"), the resolution of a
> complete postal code to place, county, and federal state ("PostalCode"), how the fields "Ort"/"Wohnort",
> "Landkreis", and "Bundesland" are filled, the dependency on the external Geonames service and its `GeonamesAccount`
> setting, and the locally stored country list that feeds the "Land" choice of Meningococci patients. The coordinate
> lookup used by the EpiScanGIS export is summarised as a second consumer only. It does **not** propose changes. Out of
> scope and referenced instead: the rest of the sender form (spec 001), the rest of the submission forms and the
> state/country consistency rule (specs 002, 003), the settings table (spec 018), access and anti-forgery rules (spec
> 021), the use of the county text by exports (spec 011), and the EpiScanGIS export itself (spec 021, User Story 7).
> The value of the `GeonamesAccount` setting is named but never reproduced. No call to the external service was made;
> everything is derived from code, configuration structure, version history, and automated tests.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Resolve a complete postal code to place, county, and state (Priority: P1)

While capturing a patient of a Haemophilus or Meningococci submission, a laboratory employee types the patient's
five-digit German postal code; the form fills "Wohnort", "Landkreis", and "Bundesland" automatically.

**Why this priority**: County and federal state of the patient are the basis of the county and state numbers of the
RKI and LGA exports (spec 011); the lookup is how these fields are normally filled.

**Independent Test**: On the submission create page of either pathogen, type a postal code that belongs to one place
only, one that belongs to several places, and one that does not exist; inspect "Postleitzahl", "Wohnort",
"Landkreis", and "Bundesland" after each. Repeat on the sender create page and inspect "Ort".

**Acceptance Scenarios**:

1. **Given** the patient part of a submission form (create or edit, either pathogen), **When** the content of
   "Postleitzahl" reaches five or more characters, **Then** the system asks the external Geonames service for the
   places with that postal code in Germany; this happens on every change of the field while it has five or more
   characters.
2. **Given** the field contains a value with a space (for example a chosen suggestion "91301 Forchheim"), **Then** the
   part before the first space is used as postal code and written back into "Postleitzahl", and the part after it is
   used as place name; without a space the whole value is the postal code and no place name is used.
3. **Given** no place name, **When** the service knows the postal code, **Then** the **first** place it returns is
   taken, also when the postal code belongs to several places; no choice is offered.
4. **Given** a place name, **When** the service returns a place whose name equals it exactly (same spelling and case),
   **Then** that place is taken; **given** no exactly equal place name, **Then** no place is taken (scenario 6).
5. **Given** a place is taken, **Then** "Postleitzahl" is set to the postal code returned for it, "Wohnort" to its
   place name, "Landkreis" to its county name as delivered by the service (for example "Landkreis Forchheim" or
   "Anhalt-Bitterfeld"), and "Bundesland" to the federal state whose short code equals the state code returned for it
   (`SH`, `HH`, `NI`, `HB`, `NW`, `HE`, `RP`, `BW`, `BY`, `SL`, `BE`, `BB`, `MV`, `SN`, `ST`, `TH`).
6. **Given** no place is taken (unknown postal code, no matching place name, or the service could not be reached and
   reported "not found"), **Then** "Wohnort" and "Landkreis" are set to "Unbekannt" and "Bundesland" to "n.n.".
7. **Given** the result, **Then** no message is shown to the user in any case; the outcome is written only to the
   browser's developer console ("Postal code query result: …" or "Error querying postal code data: …").
8. **Given** the fields were filled automatically, **Then** they remain editable and are saved like typed values;
   nothing marks them as looked up.
9. **Given** the sender form (create or edit), **Then** the same lookup runs and fills "Postleitzahl" and "Ort" only,
   because that form has no county or state field.

---

### User Story 2 - Choose from postal-code suggestions while typing (Priority: P1)

While typing a postal code, the employee is offered matching postal codes with their place names and picks one.

**Why this priority**: Choosing a suggestion is the only way the user can steer which of several places sharing a
postal code is taken (User Story 1, scenarios 2–4).

**Independent Test**: On a sender or submission form, type the first digits of a postal code and verify that
suggestions "<postal code> <place>" appear under the field; pick one belonging to a postal code shared by several
places and verify that "Wohnort"/"Ort", "Landkreis", and "Bundesland" show that place.

**Acceptance Scenarios**:

1. **Given** the field "Postleitzahl" of a sender or patient form, **When** its content changes and is not empty,
   **Then** the system asks the external service for postal codes in Germany that start with the content and offers
   them under the field as suggestions of the form "<postal code> <place name>"; the previous suggestions are replaced.
2. **Given** suggestions, **Then** they are offered by the browser's own suggestion list for the field (the browser's
   autofill for the field is switched off); the number and order of suggestions are those returned by the service —
   the application sets no limit and no minimum number of typed characters.
3. **Given** a suggestion is chosen, **Then** the field briefly holds "<postal code> <place name>" and the lookup of
   User Story 1 runs with that place name, so that the chosen place is taken and "Postleitzahl" ends up holding the
   postal code only.
4. **Given** the content changes to anything other than exactly five characters (including the moment a suggestion
   with place name is chosen), **Then** "Wohnort"/"Ort" and "Landkreis" are emptied and "Bundesland" is set to "n.n."
   before any result arrives; a following lookup (User Story 1) fills them again.
5. **Given** the field is emptied in one step (for example by selecting all and deleting), **Then** neither
   suggestions nor lookup run and "Wohnort"/"Ort", "Landkreis", and "Bundesland" keep their values.

---

### User Story 3 - Choose the country of a Meningococci patient (Priority: P2)

For a Meningococci patient the employee chooses the country of residence from a list of countries.

**Why this priority**: The country is required to be consistent with the federal state "Ausland" (spec 003); the list
is the second part of this feature's geographic reference data.

**Independent Test**: Open the Meningococci submission create page; verify that "Land" offers the countries by German
name in alphabetical order with "Deutschland" preselected; open the Haemophilus submission page and verify that no
country choice exists.

**Acceptance Scenarios**:

1. **Given** the patient part of the Meningococci submission form or the Meningococci patient page, **Then** "Land"
   offers 250 countries with German names (for example "Afghanistan", "Albanien", "Algerien", …, "Deutschland"),
   ordered by name, and stores the three-letter country code of the chosen country.
2. **Given** a new patient or a patient without stored country, **Then** "Deutschland" (code `DEU`) is preselected and
   stored.
3. **Given** the list, **Then** it is read from a country file stored with the application each time a form is
   prepared; it is not fetched from the external service and is not updated automatically. The file entered the
   version history on 2020-01-09 and has not changed since; a code comment notes that it should be refreshed from
   the external service "from time to time". [POTENTIAL-REFACTORING]
4. **Given** the postal-code lookup of User Story 1, **Then** it never changes "Land"; it always searches German
   postal codes only, whatever country is chosen.
5. **Given** the Haemophilus patient form, **Then** it offers no country field (a placeholder note in the form says the
   country is not implemented); the patient's country defaults to `DEU`.

---

### User Story 4 - Supply coordinates for the EpiScanGIS export (Priority: P3)

The automated EpiScanGIS export (spec 021, User Story 7) asks the same lookup for the coordinates of each case's
patient postal code.

**Why this priority**: It is a second, non-interactive consumer of the same external service and the same place-name
rule; its details belong to the export.

**Independent Test**: Run the export for cases whose patient city equals, differs from, or is missing against the
place names known for the postal code; verify which cases carry coordinates.

**Acceptance Scenarios**:

1. **Given** a Meningococci case selected for the export, **When** the export is produced, **Then** the service is
   queried once per case with the patient's postal code and the patient's "Wohnort" as place name, under the same
   rules as User Story 1 (exact place-name match, otherwise none; without place name the first place).
2. **Given** a place is taken, **Then** its latitude and longitude are written for the case; **given** none, **Then**
   the case is left out of the export file without notice (spec 021). A "Wohnort" spelled differently from the
   service, or "Unbekannt", therefore excludes the case. [SUSPECTED-DEFECT]

### Edge Cases

- **Foreign residence**: The lookup searches only German postal codes. For a patient living abroad, typing the postal
  code empties and then overwrites "Wohnort" and "Landkreis" with "Unbekannt" (or with a German place sharing the
  code) and sets "Bundesland" to "n.n." or a German state, also when "Ausland" was chosen before; the user must choose
  "Ausland" again, otherwise the Meningococci state/country consistency check (spec 003) rejects the save for a
  non-German country. [SUSPECTED-DEFECT]
- **Values written as data**: "Unbekannt" is written into "Wohnort"/"Ort" and "Landkreis" as ordinary text and is
  saved unless corrected. Exports that derive county and state numbers from the county text (spec 011) find no county
  for it and leave both numbers empty; the county names delivered by the service (for example "Landkreis …") are the
  free text those exports match against the stored county list after removing fill words such as "Landkreis".
- **Existing values on edit**: Opening an edit page does not run the lookup; stored values stay as they are until
  "Postleitzahl" is changed.
- **Longer input**: Any content of five or more characters without a space (for example six digits or letters) is
  sent as postal code; it typically matches nothing and yields "Unbekannt"/"n.n.". The application does not check
  the format of a postal code anywhere; postal codes are free text in the stored data (version history 2016-02-04:
  "Removed 5-digit restriction on postalcode").
- **One request per keystroke**: Each change of the field sends a suggestion request and, from five characters on,
  also a lookup request; nothing delays or bundles them. Answers can arrive out of order, so the last answer to arrive
  decides the field contents (derived from code; not exercised). Every request uses the quota of the configured
  Geonames account. [POTENTIAL-REFACTORING]
- **Service unavailable or failing**: Only a "not found" answer from the service is handled; it yields an empty result
  ("Unbekannt"/"n.n." for the lookup, no suggestions). A network failure, a time-out, or an answer that is not in the
  expected format ends the request with the generic server error; the form then shows nothing, the fields keep what
  User Story 2, scenario 4, left in them, and nothing is reported to the user. The failure is logged by the general error logging;
  the "not found" case is logged with a message starting with a stray "$" ("$Failed to call Geonames …").
  [POTENTIAL-REFACTORING]
- **Service error answers**: The suggestion list expects a list named differently from the one in the empty result
  the server returns after "not found"; in that case, and whenever the service answers without a list (for example an
  account or quota message), the previous suggestions are removed and a script error occurs in the browser; no
  suggestions are shown. A check meant to stop on an empty result never applies. The page itself checks only for an
  "error" entry in the answer, which the application itself never adds (derived from code; not exercised). [SUSPECTED-DEFECT]
- **Place names in suggestions**: Place names from the service are inserted into the suggestion list without
  escaping; a name containing an apostrophe would be cut off in the suggestion, and markup in a name would become part
  of the page (derived from code; not exercised). [SUSPECTED-DEFECT]
- **Unencoded parameters**: The typed content is passed to the service address without encoding, so characters such
  as "&" or "#" alter the request to the service (derived from code; not exercised). [POTENTIAL-REFACTORING]
- **Unencrypted connection**: The service is called over plain HTTP, so the account name and the typed patient and
  sender postal codes travel unencrypted to the external provider (Constitution Principle IV). The place name is used
  only inside the application and is not sent. [POTENTIAL-REFACTORING]
- **Missing setting**: Without the `GeonamesAccount` setting the service is called with an empty account name; the
  outcome depends on the service's answer to such a request and falls under the error cases above (derived; not
  exercised).
- **Duplicated statement**: In the "no place taken" branch the city is set to "Unbekannt" twice; this has no visible
  effect. [POTENTIAL-REFACTORING]
- **Fields matched by name ending**: The page fills every field whose identifier ends with "PostalCode", "City",
  "County", or "State" and lists suggestions in one fixed list; on the current forms each name occurs once, so only
  the intended fields are affected. A form with two such fields would fill both. [POTENTIAL-REFACTORING]
- **Access** (consistent with spec 021): The suggestion and lookup requests admit only the Standard User role and do
  not verify an anti-forgery token; all forms that use them admit only the Standard User role as well. A refused
  background request has no visible effect. The country list is also reachable as an address of its own for any
  signed-in account; requested that way it answers with a technical type name instead of a list, because it is a
  helper and not a page (derived from framework behavior; not exercised). [POTENTIAL-REFACTORING]
- **Architecture** (Constitution "Known Architectural Constraints (Legacy Debt)", Principles II and III): The service
  is wired manually; the lookup page group creates its own service instance by its concrete type, and the
  Meningococci patient page group creates an instance of the lookup page group to read the country list instead of
  using a service. The fill logic lives in a script shared by the sender, Haemophilus patient, and Meningococci
  patient forms (one shared copy, not duplicated per pathogen). Automated tests cover the server-side place selection
  (single and several matches, place name, "not found", unexpected answer), the coordinate lookup, and reading the
  country file; the browser behavior (suggestions, field filling, clearing) and the two request endpoints are not
  covered. One test named "…MultipleMatches_ReturnsAllEntries" actually verifies that only one entry is returned.
  [POTENTIAL-REFACTORING]

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The sender form (create, edit), the patient part of the Haemophilus and Meningococci submission forms
  (create, edit), and the Haemophilus and Meningococci patient pages MUST offer postal-code suggestions and
  postal-code resolution on the field "Postleitzahl".
- **FR-002**: On every non-empty change of "Postleitzahl", the system MUST request postal codes in Germany starting
  with the content from the external Geonames service and MUST offer them as "<postal code> <place name>"
  suggestions, replacing earlier ones, without own limit or minimum length.
- **FR-003**: On every change to a content other than exactly five characters, the system MUST empty the place and
  county fields and set "Bundesland" to "n.n." (where present); an emptied field MUST leave them unchanged.
- **FR-004**: On every change to a content of five or more characters, the system MUST split it at the first space
  into postal code and optional place name, write the postal code back into the field, and request the places of that
  postal code in Germany.
- **FR-005**: Of the returned places, the system MUST take the one whose name equals the place name exactly, or,
  without place name, the first one; otherwise none.
- **FR-006**: For a taken place, the system MUST set "Postleitzahl" to its postal code, the place field ("Ort" or
  "Wohnort") to its name, "Landkreis" to its county name, and "Bundesland" to the state with the matching short code;
  without a taken place it MUST set place and county to "Unbekannt" and "Bundesland" to "n.n.". Fields absent from a
  form MUST be skipped.
- **FR-007**: The lookup MUST NOT show messages to the user and MUST NOT change "Land".
- **FR-008**: A "not found" answer of the service MUST yield an empty result; other failures end the request with the
  generic server error (current behavior).
- **FR-009**: The service MUST be addressed with the account name from the `GeonamesAccount` setting (spec 018); its
  value MUST NOT appear in this specification.
- **FR-010**: The Meningococci patient form MUST offer "Land" from the stored country file (250 countries, German
  names, ordered by name, stored as three-letter code) with "Deutschland" (`DEU`) as default; the Haemophilus patient
  form MUST NOT offer a country, whose value then defaults to `DEU`.
- **FR-011**: The EpiScanGIS export MUST obtain coordinates through the same place selection (FR-005) using the
  patient's postal code and "Wohnort", and MUST leave out cases without a taken place.
- **FR-012**: The suggestion and lookup requests MUST admit only the Standard User role and MUST NOT verify an
  anti-forgery token (current behavior, spec 021).
- **FR-013**: This feature MUST document the current behavior only and MUST NOT introduce new lookups, fields, or
  rules.

### Field mapping

| Form field (label) | Sender form | Patient forms (both pathogens) | Filled with (place taken) | Filled with (no place) | Emptied on change ≠ 5 characters |
| --- | --- | --- | --- | --- | --- |
| "Postleitzahl" | ✓ | ✓ | Postal code of the place | Part before the first space | No |
| "Ort" / "Wohnort" | ✓ ("Ort") | ✓ ("Wohnort") | Place name | "Unbekannt" | Yes |
| "Landkreis" | – | ✓ | County name as delivered | "Unbekannt" | Yes |
| "Bundesland" | – | ✓ | State with matching short code | "n.n." | Yes (set to "n.n.") |
| "Land" | – | Meningococci only | Not touched | Not touched | No |

### Roles and Boundaries

| Actor or role | Current observable involvement |
| --- | --- |
| Standard User (`Standardbenutzer`) | The only role admitted to the forms and to the suggestion and lookup requests (spec 021). |
| Other roles | Refused by the forms and the requests; the country list address is reachable by any signed-in account but has no use. |
| Geonames service (external) | Supplies postal-code suggestions, places with county, state code, and coordinates for German postal codes; reached over plain HTTP under the configured account; its unavailability leaves the fields unfilled or "Unbekannt". |
| EpiScanGIS export (automated) | Second consumer; asks for coordinates per case (User Story 4). |
| Operator | Maintains the `GeonamesAccount` setting (spec 018) and, if ever, the stored country file. |

### Key Entities *(include if feature involves data)*

- **Postal-code suggestion**: Postal code and place name returned for a typed prefix; shown only, never stored.
- **Place (lookup result)**: Postal code, place name, county name, federal-state short code, and coordinates of one
  place in Germany; supplies the values written into the form and the export coordinates.
- **Patient / sender address fields**: Free-text postal code, place ("Ort"/"Wohnort"), free-text county
  ("Landkreis"), federal state ("Bundesland": 16 states, "Ausland", or "n.n."), and for Meningococci patients the
  country code ("Land"). Specs 001, 002, 003.
- **Country list**: A stored file of 250 countries with German name, three-letter code, two-letter code, continent,
  and capital; only name and three-letter code are used.
- **`GeonamesAccount` setting**: The account name under which the external service is used. The main configuration
  file and its template carry a default value, and both build transforms (Debug, Release) replace it; no value is
  reproduced here.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: For postal codes known to the service and belonging to one place, 100% of lookups fill place, county,
  and state with that place's values.
- **SC-002**: For postal codes shared by several places, 100% of lookups without chosen suggestion take the first
  returned place, and 100% of lookups after choosing a suggestion take the chosen place.
- **SC-003**: For unknown postal codes, 100% of lookups set place and county to "Unbekannt" and state to "n.n.".
- **SC-004**: In 100% of tests, the lookup never changes "Land" and never shows a message.
- **SC-005**: The Meningococci "Land" choice offers the same 250 countries in 100% of form openings, with
  "Deutschland" preselected for new patients.
- **SC-006**: 100% of EpiScanGIS cases whose "Wohnort" does not equal a place name of their postal code are absent
  from the export file.

## Assumptions

- Current application code, views, configuration structure (values not read into this spec), version history, and
  automated tests are the source of truth. The external service was not called and no running system was exercised.
- The structure of the service's answers (lists of postal codes with place name, county name, state short code, and
  coordinates) is taken from the code and the answers recorded in the automated tests. That the state short codes
  equal the application's state codes (`SH` … `TH`) is assumed from those recorded answers.
- The number and order of suggestions are assumed to follow the service's defaults, since the application sends no
  limit; not observed.
- The outcome of network failures, unexpected answers, a missing setting, out-of-order answers, unescaped place names,
  unencoded parameters, and the country list requested by address is derived from code and framework behavior; not
  exercised.
- That a refused background request has no visible effect is derived from the page script, which reacts only to
  successful answers.
- Labels are quoted as shown to users; the console messages are English developer texts.
