# Backlog: Potential bugfixes and refactorings

**Branch**: `speckit-init`

**Created**: 2026-10-30

**Status**: Draft — not yet triaged by the owner (2026-10-30: the owner will take the DEC decisions later; nothing in
this list is blocked on them)

**Source**: Every `[SUSPECTED-DEFECT]` and `[POTENTIAL-REFACTORING]` observation recorded in the as-is
specifications `specs/001` to `specs/025` (status-quo inventory of the NRZMHi Database), consolidated into one
entry per underlying issue. Items observed in several specs are listed once and cite all sources.

> **Rule** (constitution v1.2.0, "Specification Structure"): nothing in this list is approved. Before an item is
> implemented it needs either its own feature specification (`/speckit-specify`, referencing the backlog ID) or an
> explicit owner decision recorded here. Items are observations derived from reading the code; unless marked
> "test-asserted" they were **not** exercised in a running system. The specs describe current behavior; this list is
> the only place where "should this change?" is asked.

## How to read

- **ID prefixes**: `DEC` owner decision needed before anything else · `SEC` security and access · `DATA` data
  correctness and privacy · `UX` robustness and user feedback · `REF` refactoring and legacy debt · `TEST` test gaps.
- **Type**: Decision · Bugfix · Refactoring · Test.
- **Source**: spec number plus the section (FR, User Story/scenario, Edge Cases, Assumptions) that records the
  observation.
- **Offen** names what the owner must decide; absent when the fix is unambiguous.

## Spec structure decisions

The decisions D1–D5 that shaped how the as-is specs were cut (one spec per capability and pathogen; as-is only;
one generic export spec; reporting per pathogen with separate interpretation specs; own spec for report
templates) live in `.specify/memory/constitution.md` v1.2.0, section "Specification Structure", together with the
backlog rule above. D6 (legacy debt) is in the same file since v1.1.0. Moved there on 2026-10-30 (DEC-012).

## DEC — Owner decisions

- **DEC-001 – Administrator role: defect or intended?** (Decision; source: 011, 013, 014, 015, 017, 018, 019, 020,
  021 US5): An Administrator-only account sees the full laboratory navigation and all Administration entries, but
  every function except the unlinked role pages and the functions open to any signed-in account refuses it. Nine specs
  tag this `[SUSPECTED-DEFECT]`. Offen: is Administrator meant to be a pure role-management role (then the navigation
  is wrong and the tags become one UX item) or a superset of Standard User (then the authorization rules are wrong)?
  Confirmed on system 2026-10-30 for the sender pages (001 verification): `Admin1` sees "Einsender", both "Einsender
  Export" entries and "Gelöschte Einsender", and every sender address redirects it to the sign-in page.
  Confirmed on system 2026-10-30 for the Haemophilus intake (002 verification): `Admin1` sees "Haemophilus",
  "Gelöschte Einsendungen (Haemophilus)" and "Patienten Zusammenfügen", and every `/PatientSending` address (list,
  create, edit, delete, restore, merge, all three exports, list data) redirects it to the sign-in page.
  Confirmed on system 2026-10-30 for the Meningococci intake (003 verification): `Admin1` sees "Meningokokken",
  "Labor Export", "IRIS Export", "LGA Export", "RKI Export" and "Patienten Zusammenfügen" for Meningococci, and every
  `/MeningoPatientSending` address (list, create, edit, delete, deleted list, restore, merge, all four exports, list
  data) redirects it to the sign-in page.
- **DEC-002 – Sender export counts soft-deleted submissions** (Decision; source: 013 Edge Cases, FR-008; 001 FR-015):
  Senders are selected via non-deleted submissions, but "Anzahl Einsendungen", "Stammnummern" and "Labornummern"
  include soft-deleted ones. Offen: intended (deleted submissions still "sent") or defect?
- **DEC-003 – "Gültig für = Allgemein" breakpoints** (Decision; source: 017 Edge Cases): Storable, never offered for
  either pathogen. Offen: dead value to remove, or should such records apply to both pathogens?
- **DEC-004 – Haemophilus interpretation: growth "Ja"/"Nein" wording** (Decision; source: 006 FR-012/FR-013,
  test-asserted): With all typing not determined, growth "Ja" yields "Der eingesendete Stamm konnte nicht angezüchtet
  werden …", growth "Nein" yields "Kein Nachweis von Haemophilus influenzae." — inverse to the "Wachstum" label but
  asserted by tests. Offen: intended laboratory semantics or long-standing defect? Confirmed on system 2026-11-08 (006
  verification): on the report page of an isolate with all typing "n.d.", "Wachstum" "Ja" shows "Der eingesendete Stamm
  konnte nicht angezüchtet werden. Um Wiedereinsendung wird gebeten." with the disclaimer "Eine telefonische
  Vorabmitteilung ist erfolgt."; "Nein" shows "Kein Nachweis von Haemophilus influenzae." with "Beim eingesendeten Isolat
  handelt es sich am ehesten um NTHi.", because "Beurteilung" keeps its preselected "NTHi" unless the user changes it.
- **DEC-005 – Meningococci native rule 32 and the NoMeningococci flag** (Decision; source: 007 FR-010, Edge Cases,
  test-asserted): An all-negative native-material result does not set the flag, so it counts as "Meningococci found"
  in the RKI/IRIS selection (011 FR-011). Related: rules 11, 14, 41 emit sequence rows without the sequencing-provider
  comment; native rules 30a/30b are fully shadowed by 01b/01a. Offen: clinical intent for each. Observed on system
  2026-11-08 (007 verification): an invasive native submission with every test negative (rule 32, report "Kein Hinweis
  auf Neisseria meningitidis.") had an empty "Meningokokken" column in the laboratory export and appeared in the RKI
  export, while native rule 06 with the same report wording showed "kein Nachweis" and was left out; rule 32 also
  carries the sequencing-provider comment although it emits no sequence row. No decision taken.
- **DEC-006 – LGA export open to Public Health** (Decision; source: 011 Edge Cases, 021 US4.2): Authorized for the
  `RKI` role but not in its navigation. Offen: should Public Health have it (add to menu) or not (restrict)?
- **DEC-007 – Soft deletion of Meningococci submissions** (Decision; source: 003 US3, 015, 020 US4): No Meningococci
  delete-confirmation or deleted-submissions page exists; "Eintrag löschen" ends on the error page. Offen: is soft
  delete wanted for Meningococci (add pages) or not (remove the button and inherited actions)? Confirmed on system
  2026-10-30 (003 verification; observation only, no decision): "Eintrag löschen" and `/MeningoPatientSending/Deleted`
  end in HTTP 500 (view not found); a delete request with anti-forgery token sent outside the pages soft-deletes the
  submission, and `/MeningoPatientSending/Undelete/<id>` restores it.
- **DEC-008 – EpiScanGIS case selection** (Decision; source: 023 Edge Cases): Only sampling locations "Blut" and
  "Liquor" are exported, not the invasive set used by RKI/IRIS/LGA (011 FR-011); the date window starts 2019-10-01 with
  an asymmetric boundary. Offen: agreed with the receiver, or drift?
- **DEC-009 – Duplicate-submission window in adjusted exports** (Decision; source: 012 Edge Cases, FR-006): Code
  comments describe "less than six months", the implementation compares only the month component of the date
  difference (drops < 1 month and whole-year distances, keeps 1–11 months). Offen: which rule is intended, so that
  DATA-001 can be fixed and tested? Observed on system 2026-11-08 (012 verification, no decision taken): an identical
  strain received 3 months and 5 days after the kept submission stayed in the adjusted RKI export.
- **DEC-010 – Export duplicate rules for Meningococci** (Decision; source: 012 Edge Cases, 011): No Meningococci export
  applies duplicate cleaning or identifier removal; the Meningococci RKI page accepts but ignores the "unadjusted"
  setting. Offen: should Meningococci RKI/IRIS get the same pseudonymization as Haemophilus RKI/PubMLST? Observed on
  system 2026-11-08 (012 verification, no decision taken): the Meningococci RKI file for 2020 has the same rows and
  columns with and without `Unadjusted=Yes`; no Meningococci file has a `duplicate_group` column.
- **DEC-011 – Report pages open to every signed-in account** (Decision; source: 008, 009 Edge Cases, 021 US4.4): See
  SEC-004; the decision is whether report generation stays available to roles other than Standard User at all.
  Observed on system 2026-11-08 (008 verification, no decision taken): `Rki1` and `Admin1` reach the Haemophilus
  report page by direct address and can generate documents and set the report status.
- **DEC-012 – Where to keep D1–D5** (Decision; **decided 2026-10-30**): Moved to the constitution (v1.2.0, section
  "Specification Structure"). This file keeps only the pointer above.

## SEC — Security and access

- **SEC-001 – Registration submission does not re-check the token** (Bugfix; source: 021 US6.6, FR-007): Only showing
  the form checks `RegistrationToken`; the POST checks just the anti-forgery token, which any visitor obtains from the
  sign-in page. Anyone can register an account with any stored role.
- **SEC-002 – Registration with an unknown role leaves a role-less account** (Bugfix; source: 021 US6.7): The account
  is created before the role assignment fails with the generic error page.
- **SEC-003 – Missing `RegistrationToken` setting opens registration and the EpiScanGIS export** (Bugfix; source: 021
  Edge Cases, 023): A request without token would match an absent setting.
- **SEC-004 – Report pages, health offices and RKI match import check no role** (Bugfix; source: 008/009 Edge Cases,
  016 Edge Cases, 021 US4.4–4.6, US7, access matrix, 024): Public Health (and role-less) accounts can open the report
  page of any isolate (patient data, document generation, status marking), create/edit/delete health offices, and
  upload RKI match files. Only the navigation hides these functions (Constitution Principle IV). See DEC-011.
  Confirmed on system 2026-11-08 (008 verification): `Rki1` and `Admin1` open `/Report/Isolate/14` (HTTP 200, patient
  initials and postal code shown) although the isolate form and the submissions list redirect them to the sign-in
  page; `Rki1` generated a "Teilbefund" document there, which set the isolate to "Teilbefund".
- **SEC-005 – Anti-forgery token not verified on state-changing actions** (Bugfix; source: 021 Edge Cases
  "consolidated"; 002 FR-021, 003 FR-023, 008 FR-015, 011, 013, 014, 015, 016, 019, 020, 022, 024): Not verified on the combined
  submission create/edit saves, all exports (also any request method), both patient merges (a single request merges
  without the confirmation page), restore links, the health-office lookup (which stores records), report-status
  marking, the PubMLST and postal-code lookups, list data requests, the county upload, the RKI match import, and the
  role page save. Confirmed on system 2026-10-30 (002 verification) for the combined Haemophilus submission create
  save: a POST without token is stored. Confirmed on system 2026-10-30 (003 verification) for the combined
  Meningococci submission create and edit saves. Confirmed on system 2026-11-08 (008 verification) for the
  Haemophilus report-status marking: the report page itself posts only `id` and `preliminary` to
  `/Report/ReportGenerated`, without token, and the status is stored.
- **SEC-006 – No lockout, delay, counting or logging of failed sign-ins; no audit of authentication events** (Bugfix;
  source: 021 US1.6, Edge Cases "No audit of access"): Sign-ins, failures, registrations, role creations and password
  changes are not logged (constitution: failures affecting authentication MUST be logged).
- **SEC-007 – One secret for registration and the EpiScanGIS export, carried in the URL** (Bugfix; source: 021 US7.1–7.2,
  023): The token appears in web-server logs; the export is an anonymous GET.
- **SEC-008 – Unescaped external data rendered as markup** (Bugfix; source: 016 Edge Cases, 022 Edge Cases; 008
  FR-006): Health-office address/phone/fax/email (from the RKI website or local edits) on the report page, and Geonames
  place names in the postal-code suggestion list. Confirmed on system 2026-11-08 (008 verification, with a synthetic
  lookup answer injected in the browser because the RKI call is blocked): an address containing `<b>…</b>` is rendered
  bold on the Haemophilus report page.
- **SEC-009 – Geonames called over plain HTTP** (Bugfix; source: 022 Edge Cases): Account name and typed patient/sender
  postal codes travel unencrypted (Principle IV).
- **SEC-010 – Rendering libraries from public CDNs on every page, two without integrity check** (Refactoring; source:
  010 Edge Cases).
- **SEC-011 – Internal helper actions reachable by URL** (Refactoring; source: 021 Edge Cases, 022): Create patient,
  create sending, assign stem/laboratory numbers (Standard User), country list (any signed-in account) act without
  form, token or duplicate check.
- **SEC-012 – Temporary export/import files are never deleted** (Bugfix; source: 011, 013 Edge Cases, 023, 024, 025):
  Every spreadsheet export, the EpiScanGIS export, the RKI match upload and the county upload leave files in the
  server's temp directory; exports and the RKI match file contain patient data (Principle IV).
- **SEC-013 – Combined submission form binds every patient and sending property** (Bugfix; source: 002 FR-021; 003
  FR-023): Create and edit have no list of intended fields. Confirmed on system 2026-10-30 (002 verification): a create
  request with `Sending.Deleted=true` stores the new submission as deleted. Confirmed on system 2026-10-30 (003
  verification) for Meningococci submissions. From code: on edit, the posted patient and submission
  numbers decide which records are overwritten, so one request can change another patient. The sender pages already
  restrict binding (001 FR-017).
- **SEC-014 – Isolate edit binds every isolate property** (Bugfix; source: 004 FR-018, 005 FR-023): The save has no
  list of intended fields; the anti-forgery token, which 004 FR-018 used to describe as protection against
  over-posting, does not limit them. Confirmed on system 2026-11-04 (004 verification): a request with
  `ReportStatus=Final` stores the status, and a request with another `IsolateId` overwrites that isolate. Same pattern
  as SEC-013. Confirmed on system 2026-11-08 (005 verification) for Meningococci isolates: `ReportStatus=Final` stored,
  and a request from one isolate's form with another `MeningoIsolateId` (and that isolate's laboratory number)
  overwrote the other isolate.

## DATA — Data correctness and privacy

- **DATA-001 – Duplicate-submission window compares only the month component** (Bugfix after DEC-009; source: 012
  Edge Cases, FR-006). Confirmed on system 2026-11-08 (012 verification): of four identical-strain submissions of
  one patient, the one received 10 days after the first was dropped, the one received 3 months and 5 days later was
  kept (the documented six-month rule would drop it); the whole-year case was not exercised.
- **DATA-002 – Duplicate matching over-merges patients without birth date and postal code** (Bugfix; source: 012 Edge
  Cases): Same initials and gender with both identifiers empty are one patient; the four values are concatenated
  without separator (collision risk). Confirmed on system 2026-11-08 (012 verification): two separate patient
  records "Q.Q." (same gender, no birth date, no postal code) with identical strains received 7 days apart; the
  second submission was missing from the adjusted RKI and PubMLST files.
- **DATA-003 – PubMLST export compares stem numbers as text** (Bugfix; source: 012 Edge Cases): "H1000" sorts before
  "H999", so the "earlier" submission is chosen by text order; RKI compares numerically. Confirmed on system
  2026-11-08 (012 verification): for four identical-strain submissions H9998, H9999, H10000, H10001 of one patient
  the adjusted RKI file kept 9998 and 10000, the adjusted PubMLST file kept only H10000 (H10000 was taken as the
  earliest and H9998/H9999, received three months before it, were dropped).
- **DATA-004 – PubMLST export selection and encoding defects** (Bugfix; source: 011 Edge Cases, 012): Hint promises
  overseas data and all locations in unadjusted mode but both are always excluded; overseas check uses the free-text
  county ("Ausland") instead of the state; state "Ausland" yields region `blood`; H. haemolyticus is exported
  although marked "not to be exported"; the adjusted-mode removal of non-invasive rows is a no-op. Confirmed on
  system 2026-11-08 (012 verification) for the no-op: an "Anderer nicht invasiv" submission is in the laboratory
  export and missing from the PubMLST file in both modes.
- **DATA-005 – Unknown birth date exported as age 0** (Bugfix; source: 011 Edge Cases, 023): Indistinguishable from an
  infant under one month in PubMLST, IRIS and EpiScanGIS.
- **DATA-006 – "Gelöschte Einsendungen (Haemophilus)" lists active submissions** (Bugfix; source: 020 US2, Edge Cases
  "Inverted selection"; 002/003 Assumptions): Inverted by the 2018 generics restructuring (commit `e8aac1d`); no page
  lists deleted submissions, restore works only by direct address. Confirmed on system 2026-10-30 (002
  verification).
- **DATA-007 – Saving the edit page of a deleted submission or sender silently restores it** (Bugfix; source: 020
  US3.3–3.4; 001 Edge Cases; 002 Edge Cases; 003 Edge Cases). Confirmed on system 2026-10-30 (001 verification) for
  senders. Confirmed on system 2026-10-30 (002 verification) for Haemophilus submissions. Confirmed on system
  2026-10-30 (003 verification) for Meningococci submissions.
- **DATA-008 – Restore is a plain link without confirmation; unknown id ends on the error page** (Bugfix; source: 020
  US3.5, US5.4; 002 Edge Cases; 003 Edge Cases). Confirmed on system 2026-10-30 (002 verification):
  `/PatientSending/Undelete/<id>` restores on a plain GET; an unknown or missing id ends in HTTP 500 (null reference).
  Confirmed on system 2026-10-30 (003 verification) for `/MeningoPatientSending/Undelete/<id>`.
- **DATA-009 – Patient merge: soft-deleted submissions, latent data mix-up, no audit** (Bugfix; source: 014/015 Edge
  Cases, FR-011/FR-014): Soft-deleted submissions of the patient to delete make the save fail (or are dropped silently
  without isolate); Patient 1's number is placed in Patient 2's slot of the confirmation data (masked today); no log,
  history or undo for a permanent patient deletion (Principle I); the request page shows the validation message on
  first open; the Meningococci sampling-before-birth rule is not re-checked.
- **DATA-010 – Haemophilus merge pages link to the Meningococci pages** (Bugfix; source: 014 US3, FR-013): "Abbrechen"
  and "Zurück zur Übersicht" are copy-paste leftovers.
- **DATA-011 – Breakpoint deletion cascades to all E-Test measurements** (Bugfix; source: 017 US4.3, FR-014): No
  warning, audit or undo; the page is reachable by direct address only (Principle I).
- **DATA-012 – Editing a breakpoint in use changes historical report output** (Bugfix; source: 017 US3.4, Edge Cases):
  Reports show new thresholds and year next to the unchanged stored result.
- **DATA-013 – Breakpoint validation gaps** (Bugfix; source: 017 Edge Cases): No check that "Sensibel <=" is at most
  "Resistent >" (reversed thresholds classify everything above resistant and nothing intermediate); duplicate
  combinations are rejected only by the database (generic error page, input lost).
- **DATA-014 – Health-office records created for postal codes unknown to the RKI website are never shown** (Bugfix;
  source: 016 US3.2, Edge Cases): Matching uses only the address the website returns; the create page is unlinked.
- **DATA-015 – Locally cleared health-office fields break the report page** (Bugfix; source: 016 Edge Cases; 008
  FR-006): An empty telephone stops the page script; cleared email/fax display as "null". Confirmed on system
  2026-11-08 (008 verification, synthetic lookup answers injected in the browser): without telephone only the address
  appears, fax and email stay empty, "Gesundheitsamt bearbeiten" stays inactive and the browser reports "Cannot read
  properties of null (reading 'replace')"; without fax and email both show "null" (fax linked to `mailto:-@fax`).
  Document generation still works.
- **DATA-016 – PubMLST batch matching defects** (Bugfix; source: 019 Edge Cases): Includes isolates of deleted
  sendings; batch asks public then IRIS while the single lookup asks IRIS then public (different records possible);
  local record found by PubMLST id only, ignoring the database (cross-database overwrite, database name not updated);
  unhandled remote errors abort the run and leave partial saves; a duplicate locus would crash the run; each matched
  isolate is written back in full (overwrites concurrent edits); "Failed to match isolate by reference" is logged for
  every isolate including successes.
- **DATA-017 – Haemophilus interpretation text contains the typo "phänotpischen"** (Bugfix; source: 006 FR-006,
  test-asserted — the test must change too). Confirmed on system 2026-11-08 (006 verification): the report page shows
  "Die Ergebnisse sprechen für einen phänotpischen nicht-typisierbaren Haemophilus influenzae (NTHi). …".
- **DATA-018 – Meningococci interpretation rules without sequencing comment / shadowed rules** (Bugfix after DEC-005;
  source: 007 Edge Cases): Rules 11, 14, 41; native 30a/30b unreachable; rule 09's cnl case shadowed by rule 20.
  Confirmed on system 2026-11-08 (007 verification): a non-invasive cultured isolate matching rule 41 and then rule 11
  showed PorA and FetA sequence rows on the report page without a comment.
- **DATA-019 – Export spreadsheet write failure delivers a partial file silently** (Bugfix; source: 011, 013 Edge
  Cases): Failure is only traced; the user downloads the empty/partial temp file.
- **DATA-020 – EpiScanGIS export defects** (Bugfix after DEC-008; source: 023 Edge Cases): Blut/Liquor-only selection;
  asymmetric date boundary at 2019-10-01; any lookup failure other than "not found" aborts the whole run; cases whose
  "Wohnort" differs from the Geonames place name (or is "Unbekannt") are dropped without notice (also 022 US4.2);
  values written unescaped without header.
- **DATA-021 – RKI match import skips rows without `klhi_nr`** (Bugfix; source: 024 US3.1): The laboratory-number
  fallback is reached only via a stem-number row; "Upload" without a file ends in an object-reference message.
- **DATA-022 – Postal-code lookup overwrites a foreign residence** (Bugfix; source: 022 Edge Cases): Typing a postal code
  resets "Bundesland" away from "Ausland" and overwrites "Wohnort"/"Landkreis"; the Meningococci state/country check
  then rejects the save.
- **DATA-023 – Postal-code suggestion script expects `postalCodes`, server fallback returns `postalcodes`** (Bugfix;
  source: 022 Edge Cases): Suggestions vanish with a browser script error; the empty-result and "error" checks never
  apply.
- **DATA-024 – Report template failures and empty-list label** (Bugfix; source: 010 Edge Cases; 008 Edge Cases): Template
  retrieval or filling failures show no user message; the "Keine Vorlagen hinterlegt" label is never shown (specs 008/009
  aligned). Confirmed on system 2026-11-08 (008 verification): with the template download aborted in the browser,
  "Befund erstellen" produced no document, no message and no status request; only a browser script error
  ("InvalidStateError: Failed to read the 'responseText' property …") was raised.
- **DATA-025 – County version choice ignores the submission date** (Bugfix; source: 011 Edge Cases, 025 Edge Cases):
  When several stored county versions match, the earliest validity date wins regardless of sampling/receiving date;
  because uploads only add records, a renumbered county keeps exporting its old number.
- **DATA-028 – County normalization collides "X, Stadt" with "Landkreis X"** (Bugfix; source: 025 Edge Cases): Both
  normalize to "X", so a district patient can receive the city's county number (earliest validity or storage order
  wins).
- **DATA-029 – A county record with empty name matches every patient without county text** (Bugfix; source: 025 Edge
  Cases): Absent equals absent in the comparison; all three exports then assign its number and the Meningococci
  exports log no warning.
- **DATA-026 – "Anzahl Einsendungen" written as text** (Bugfix; source: 013 Edge Cases): The generic spreadsheet writer
  treats only certain numeric types as numbers.
- **DATA-027 – Empty signer list is accepted and blocks all reporting** (Bugfix; source: 018 US1.5, Edge Cases, untagged
  there): No warning on save; afterwards every report attempt fails the required-selection check.
- **DATA-030 – Server-side initials check accepts free text** (Bugfix; source: 002 FR-007, Edge Cases; 003 FR-007,
  Edge Cases): The Haemophilus patient rule `([a-zA-ZäöüÄÖÜ]\.)+` and the Meningococci rule `([a-zA-ZäöüÄÖÜ\?]\.)+`
  are not anchored, so any value containing one letter (or "?") followed by a dot passes; only the browser check
  enforces the whole "E.M." form. Confirmed on system 2026-10-30 (002 verification): "Max M." sent past the browser
  is stored as "MAX M.". Confirmed on system 2026-10-30 (003 verification) for Meningococci patients.
- **DATA-031 – Duplicate-resolution choice skips the third-patient block** (Bugfix; source: 002 FR-011, Edge Cases;
  003 FR-012, Edge Cases):
  The "two patients already exist" check runs only when the request carries no duplicate-resolution choice.
  Confirmed on system 2026-10-30 (002 verification): with "Neuen Patienten anlegen" posted, a third patient with the
  same initials, birth date and postal code was stored; with "Bestehenden Patienten verwenden" the save ends in HTTP
  500 ("Die Sequenz enthält mehr als ein Element."). Reachable through the UI when two pages sit at the duplicate
  prompt for the same patient. Confirmed on system 2026-10-30 (003 verification) for Meningococci patients, with the
  same two outcomes.
- **DATA-032 – Server check of the isolate laboratory number is not anchored** (Bugfix; source: 004 FR-010, Edge
  Cases; 005 FR-008, Edge Cases): The rule `\d+/\d\d` accepts any value that contains the pattern, and unparsable parts
  become 0. The browser checks the whole value, so only a request that bypasses it gets through. Confirmed on system
  2026-11-04 (004 verification): "abc12/345xyz" stored as 000/00 (year 2000), "12/345" as 012/345 (year 2345), "1/2/34"
  as 001/02. The Meningococci validator has the same rule (from code). Confirmed on system 2026-11-08 (005
  verification) for Meningococci isolates: "abc12/345xyz" stored as year 2000, number 0; "12/345" shown as 012/345;
  "-5/26" stored with the sequential number -5 and then shown read-only as "NR005/26".
- **DATA-033 – Every isolate save orphans the previous E-Test rows** (Bugfix; source: 004 Edge Cases, 005 Edge Cases):
  Each save inserts all measurements as new rows and only clears the foreign key of the old ones. Confirmed on system
  2026-11-04 (004 verification): re-saving an isolate with four measurements left four rows without isolate; at the
  end of the verification 10 of 15 rows were orphaned. From code: no page or export reads them, and the mapping is
  shared with the Meningococci isolate. Overlaps DATA-011. Confirmed on system 2026-11-08 (005 verification) for
  Meningococci isolates: re-saving an isolate with three measurements unchanged left three rows without isolate.
- **DATA-034 – Isolate form writes back a stale report status and date** (Bugfix; source: 004 FR-015, Edge Cases; 005
  FR-018, Edge Cases; 008 Edge Cases): The form posts the report status it loaded and "Befund am". Confirmed on system 2026-11-04 (004 verification, with the
  request the report page sends after generating a report): a form opened before that request set the isolate back to
  "Kein Befund erstellt" and cleared the date on save. Reachable whenever the form stays open while a report is
  generated. Saving a fresh form stored the date with the time 00:00. The Meningococci form carries the same hidden
  status field (from code). Confirmed on system 2026-11-08 (005 verification) for Meningococci isolates: a form opened
  before the report request reset "Endbefund" to "Kein Befund erstellt" and cleared the date; a fresh form kept the
  status and stored the date with the time 00:00. Confirmed on system 2026-11-08 (008 verification): an isolate form
  opened while the isolate was "Teilbefund", saved after a final report had been generated from the report page, set
  the isolate back to "Teilbefund" without report date.
- **DATA-035 – E-Test result is taken from the request unchecked** (Bugfix; source: 004 Edge Cases, Assumptions, SC-003;
  005 FR-006, Edge Cases, SC-003): The derivation exists only in the page script. Confirmed on system 2026-11-04 (004 verification): 0,5 against a
  breakpoint "Sensibel <= 0,5" stored as "resistent" when the request says so; an Ampicillin row accepted a Meningococci
  breakpoint of another antibiotic (the row then shows as Rifampicin without a selectable breakpoint, and every save
  fails with "'Breakpoint' darf nicht leer sein"); a measurement without a result ends in HTTP 500 ("Das Objekt mit
  Nullwert muss einen Wert haben."). From code: the stored result reaches reports and exports (Principle I).
  Confirmed on system 2026-11-08 (005 verification) for Meningococci isolates: 0,064 against Cefotaxime "Sensibel <=
  0,125" stored as "resistent"; a Benzylpenicillin row accepted the Haemophilus Ampicillin breakpoint (the row then
  shows as "Ampicillin" without a selectable breakpoint, and every save fails with "'Breakpoint' darf nicht leer
  sein"); a measurement without a result ends in HTTP 500.
  **decided 2026-11-07** (owner): the server repeats the derivation on save and validates the E-Test input, for
  Haemophilus and Meningococci isolates alike; the details belong in the change spec.
- **DATA-036 – Every Meningococci isolate save without a current PubMLST match stores an empty PubMLST record**
  (Bugfix; source: 005 US3.5, FR-021, Edge Cases, SC-005): The form always posts the PubMLST part, and the save assigns
  it to the isolate; with PubMLST id 0 it becomes a new, empty local PubMLST record and the previously linked one
  stays behind. Confirmed on system 2026-11-08 (005 verification): the first save of an isolate without lookup created
  an empty record, each further save another one (about 20 empty records after the verification). After a lookup
  that found nothing, saving an isolate that was linked to a matched record replaced that link with a new empty
  record, so the match was lost without a message. Saving a matched isolate without a new lookup kept its link. From
  code: the batch matching (019) and the exports read the linked record. Overlaps DATA-016.
- **DATA-037 – Meningococci isolate form ignores a changed laboratory number** (Bugfix; source: 005 FR-003, FR-014, Edge
  Cases): The form carries the laboratory number as a hidden field before the visible input, the request contains
  both values, and the server binds the first one. Confirmed on system 2026-11-08 (005 verification): "007/26" entered
  for MZ006/26 was posted as "006/26, 007/26" and the isolate kept 006/26 without a message; the laboratory-number
  conflict of 005 FR-014 is therefore unreachable through the form. The Haemophilus form has no such hidden field (004).
- **DATA-038 – Meningococci no-growth material change fires on the default values and is never undone** (Bugfix;
  source: 005 FR-011, Edge Cases): Both growth fields default to "Nein", so saving a new isolate of a "Vitaler Stamm"
  submission without entering growth sets the material to "Nicht angewachsen"; entering growth later does not set it
  back. Confirmed on system 2026-11-08 (005 verification). From code: the material is exported (laboratory and IRIS
  exports, 011) and shown on the submission form (003). Offen: should the material follow the growth fields in both directions, and
  should "Nein" stay the default?
- **DATA-039 – Unmatched Meningococci interpretation counts as "Meningococci found" in the RKI/IRIS selection**
  (Bugfix; source: 007 Edge Cases, FR-011, FR-014; 011 FR-011): Without a matching rule the no-Meningococci flag stays
  false, and the RKI and IRIS exports include every invasive submission whose flag is false, so submissions with
  discrepant or not yet entered results are exported as Meningococci cases. Observed on system 2026-11-08 (007
  verification): six invasive submissions without a rule (empty "Regel" in the laboratory export) appeared in the RKI
  export for November 2026. Related: DEC-005 (rule 32). Offen: should unmatched results be excluded, flagged, or kept?
- **DATA-040 – Editing a submission stores duplicate patient identities without a check** (Bugfix; source: 002
  FR-015, 012 Edge Cases): Intake checks initials, birth date and postal code against existing patients and blocks a
  third record (002 FR-011), but the edit save of a submission changes these values without any check. Observed on
  system 2026-11-08 (012 verification): two "X.Y." patients created with other birth dates were edited to the birth
  date of two existing "X.Y." patients, leaving four patient records with identical values; the exports group them
  as one patient (012 FR-003). The intake rejects every further submission with these values ("Es existieren bereits
  zwei Patienten …"; observed with two records, from code for any number above one). Related: DATA-031.

## UX — Robustness and user feedback

- **UX-001 – Navigation shows functions the role cannot use** (Bugfix after DEC-001; source: see DEC-001): Also LGA
  export hidden from Public Health (DEC-006).
- **UX-002 – Refusal is shown as the sign-in page** (Refactoring; source: 021 Edge Cases): No "access denied" message;
  signing in again leads to the same refusal.
- **UX-003 – Postal-code lookup: one request per keystroke, no debounce, race on answers, only "not found" handled**
  (Refactoring; source: 022 Edge Cases): Quota use; last answer wins; other failures end in the generic server error;
  log message with stray "$"; parameters not URL-encoded.
- **UX-004 – Template discovery accepts lock/temp `.docx` files, applies no sort, matches the preliminary marker on
  the whole path** (Refactoring; source: 010 Edge Cases).
- **UX-005 – Health-office list and hidden pages** (Refactoring; source: 016 US2.3, US3, Edge Cases): List shows no
  address and only the first lookup's postal code; details/delete pages are untranslated and unlinked; the RKI website
  is queried before local records on every lookup (unreachable website → nothing shown).
- **UX-006 – RKI match import always reports "File Uploaded Successfully!!"** (Refactoring; source: 024 US1.5): Also
  for zero matches or an empty file; failures never reach the application log.
- **UX-007 – PubMLST matching form defaults to the current year, result table unsorted, no progress or cancel**
  (Refactoring; source: 019 US1.2–1.3, Edge Cases): Exports default to the previous year.
- **UX-008 – Haemophilus submission list has no patient-number column or search** (Refactoring; source: 014 Edge Cases):
  The Meningococci list offers both.
- **UX-009 – Breakpoint and health-office details/delete pages are untranslated generated pages** (Refactoring; source:
  016, 017 Edge Cases): Internal identifiers shown in "Gültig für" and on details pages.
- **UX-010 – Submission pages with missing or unknown id end in HTTP 500** (Bugfix; source: 002 Edge Cases; 003 Edge
  Cases): `/PatientSending/Edit`, `/Delete` and `/Undelete` without id or with an unknown id fail with a null reference
  instead of "bad request"/"not found" (the sender pages answer 400/404, 001 FR-018). Confirmed on system 2026-10-30
  (002 verification). Confirmed on system 2026-10-30 (003 verification) for the same `/MeningoPatientSending`
  addresses. Restore overlaps DATA-008.
- **UX-011 – Free E-Test row is rebuilt without an empty value and in the browser language** (Bugfix; source: 004 Edge
  Cases, 005 Edge Cases): Confirmed on system 2026-11-04 (004 verification): after an antibiotic is selected, the
  placeholder "MHK auswählen..." is posted as value, so saving without a measurement is blocked with "The Feld Messwert
  muss eine Zahl enthalten." until the antibiotic is reset. In an English browser the scale reads "0.75": no result is
  derived, no breakpoint is preselected and the value cannot be saved. The field messages are half translated ("The
  Feld …"). Confirmed on system 2026-11-08 (005 verification) for Meningococci isolates (German browser; the English
  case was not repeated).
- **UX-012 – Agglutination discrepancy warning reacts only to changes of the evaluation** (Bugfix; source: 004 FR-008,
  SC-005, Edge Cases): Confirmed on system 2026-11-04 (004 verification): after the agglutination changes, the old
  warning stays although the evaluation now matches; a saved disagreement shows no warning on reopening. The warning
  quotes the internal letter "B" where the form shows "b".
- **UX-013 – PubMLST lookup without stem number asks the remote databases for "DE -"** (Bugfix; source: 005 US3.3): The
  form checks only that the prefixed reference is not empty, but an isolate without stem number has the reference
  "DE -". Confirmed on system 2026-11-08 (005 verification, request blocked in the browser): "Abfragen" posted
  `isolateReference=DE -` to the lookup, which would ask IRIS and the public PubMLST database (019 US2), and showed
  "Isolat wurde nicht gefunden" only after the request failed.
- **UX-014 – Meningococci isolate form label and message slips** (Bugfix; source: 005 Edge Cases): Confirmed on system
  2026-11-08 (005 verification): the PubMLST tab labels porB as "'porB"; the 16S rDNA percentage message is repeated
  under PorA-VR2 whenever the PorA fields are shown (the field reuses the 16S message slot).

## REF — Refactoring and legacy debt

- **REF-001 – Parallel Meningococci/Haemophilus domains and god-controllers** (Refactoring; source: constitution "Known
  Architectural Constraints", cited by every spec): Business logic in controllers, manual wiring, duplicated
  per-pathogen controllers and views. Umbrella item; the concrete duplications follow.
- **REF-002 – Duplicated logic across pathogens** (Refactoring; source: 010, 014/015, 016, 017, 019): Report
  selection/rendering script (`Views/Report/Isolate.cshtml` vs `Views/MeningoReport/Isolate.cshtml`); merge lookup and
  reassign-and-delete; health-office lookup-and-match (report base vs edit-by-postal-code); antibiotic derivation and
  E-Test interpretation script; the 17-field PubMLST update list (batch vs isolate save, marked TODO in code).
- **REF-003 – Services created inside controllers instead of injected** (Refactoring; source: 012, 016, 018, 019, 020,
  022, 023, 024): DuplicatePatientResolver, RKI client, ConfigurationService (plus a static test hook), PubMLST
  services and credentials, the restore path's separate sending controller, Geonames service (a controller
  instantiating another controller for the country list), EpiScanGIS and RKI match logic entirely in controllers.
- **REF-004 – Report templates: includes with absolute developer paths, fax variants embedding base templates,
  deprecated docxtemplater API, unused flattened typing values** (Refactoring; source: 010 Edge Cases).
- **REF-005 – Export mechanism leftovers** (Refactoring; source: 011 Edge Cases, 013): Unnamed worksheet; Meningococci
  RKI model carries an ignored "unadjusted" field; PubMLST reuses the IRIS-named query (source TODO); sender export
  matches the range twice with diverging deletion handling and loads unused patient data; no explicit row ordering.
- **REF-006 – Breakpoints maintained as unversioned runtime data** (Refactoring; source: 017 Edge Cases): Authoritative
  reference data without review step, history or audit (Principle I).
- **REF-007 – Health-office records saved without validation and then unsavable** (Refactoring; source: 016 Edge Cases):
  Deliberate exception to Principle I that blocks the edit page until values are corrected.
- **REF-008 – Sender model validated by DataAnnotations only; Meningococci sender controller exposes full CRUD without
  navigation** (Refactoring; source: 001 Assumptions, Edge Cases, FR-007): The Meningococci CRUD addresses have no
  views and end in HTTP 500 (confirmed on system 2026-10-30, 001 verification); its restore address would clear the
  deleted mark before failing (from code, not exercised). The sender details page is unlinked, untranslated
  scaffolding.
- **REF-009 – Postal-code lookup leftovers** (Refactoring; source: 022 Edge Cases, US3.3): `countries.json` snapshot from
  2020 (code TODO to refresh); "Unbekannt" assigned twice; fields matched by id suffix; `LoadCountries` reachable by
  URL returning a type name.
- **REF-010 – Deleted-records page loads all active submissions and looks up each isolate; skip-validation switch set
  on the wrong data context** (Refactoring; source: 020 Edge Cases).
- **REF-011 – Interpretation code smells** (Refactoring; source: 006 Key Entities/Assumptions, 007 Edge Cases, 007
  FR-014): `[Obsolete]`-marked fields are the only ones used by the Haemophilus path; always-true guard in the
  negative/negative branch; TODO to move interpretation into the shared base class; rule-order comment in
  `StemRules.json` names the wrong rules; rule evaluation order relies on dictionary enumeration order; comment retention
  on reused interpreter instances; the Meningococci laboratory export interprets each isolate three times per row (once
  each for rule, flag and serogroup; 007 verification 2026-11-08).
- **REF-012 – EpiScanGIS export internals** (Refactoring; source: 023 Edge Cases): "Most data" per patient decided by
  line length; Geonames queried once per case before de-duplication; unbounded ever-growing window with no range
  parameter; no application-log trace; temp-file round trip.
- **REF-013 – RKI match import internals** (Refactoring; source: 024 Edge Cases, US2.5, US3.7, US4.4): Lenient status
  parsing (English names, numbers, word prefixes); soft-deleted submissions matched; stored state can only be raised;
  process-wide CSV separator switched to ";" and left so; no import trace (Principle I); stale code comment.
- **REF-014 – Configuration saved as five separate writes without history or concurrency check** (Refactoring; source:
  018 Edge Cases).
- **REF-015 – Role seeding code never called; role page save without anti-forgery or rules** (Refactoring; source: 021
  Edge Cases).
- **REF-017 – County list maintained as unvalidated runtime data** (Refactoring; source: 025 Edge Cases): Any Standard
  User creates, overwrites, hard-deletes or bulk-uploads counties without review, audit or recovery; the county number
  is not validated (any text, first two characters become the state number); upload failures show untranslated
  exception text and are not logged; the lookup plus first-two-characters rule is copied into three exports
  (Principles I–III).
- **REF-016 – No audit trail for data-losing operations** (Refactoring; source: 014/015, 017, 020 Edge Cases, 024):
  Patient merges, breakpoint deletion, sender/submission delete and restore, RKI match import. Cross-cutting consequence
  of Principle I; overlaps DATA-009, DATA-011 and SEC-006.
- **REF-018 – Automatic stem numbers implemented but never enabled** (Refactoring; source: 002 FR-003; 003 FR-003): The
  sending controllers compute the next stem number, but the switch that enables it is `false` for every submission
  type, so intake leaves the stem number empty and it is entered on the isolate page. Confirmed on system 2026-10-30
  (002 and 003 verifications). Specs 002 and 003 described automatic stem numbers; both are corrected.
- **REF-019 – E-Test row construction** (Refactoring; source: 004 FR-006, 005 US2.4): The final sort of the rows
  discards its result, so empty standard rows keep the order of `PrimaryAntibiotics_Haemophilus` after the saved rows;
  the row template emits its script once per row and hard-codes the free row; it repeats the scale selection in
  JavaScript (marked TODO in code); the warning script sets an unused global `invalid`. The susceptibility derivation
  lives in the page script, so no unit test reaches it (see also REF-002). Confirmed on system 2026-11-08 (005
  verification) for Meningococci isolates: saved rows come sorted (Cefotaxim, Azithromycin, Penicillin), the empty
  standard rows follow in the order of `PrimaryAntibiotics_Meningococci`, and the free row offers yet another order.
- **REF-020 – api NH and MALDI-TOF (VITEK MS) have no entry path** (Refactoring; source: 004 FR-005, 005 FR-003): The
  api NH entry was removed from the form, but its three fields stay on the model and in the laboratory export. VITEK is
  read-only, yet its validation rules and hidden fields remain. Confirmed on system 2026-11-08 (005 verification) for
  Meningococci isolates (no api NH there): VITEK shown read-only, posted back as hidden values; a request with
  "Determined" and no details redisplays the form without a visible message. **decided 2026-11-07** (owner): the stored values are still
  needed for old records and stay read-only. The Biotyper replaced the VITEK MS for MALDI-TOF from a cut-over date on. A
  cleaner architecture for such legacy device data may follow later; until then this entry is a refactoring
  candidate only.

## TEST — Test gaps

- **TEST-001 – Untested functions** (Test; source: Assumptions/Edge Cases of 013, 014, 015, 016, 017, 018, 019, 020,
  021, 022, 023, 024): Sender export; both patient merges; health-office lookup, matching, storing and pages;
  breakpoint validator and pages; configuration page; deleted-records pages and restore; registration, role checks and
  navigation by role; browser behavior and both endpoints of the postal-code lookup; PubMLST update of an existing local
  record, result page and matching page; EpiScanGIS date window, "Blut und Liquor" exclusion, deleted submissions,
  missing coordinates and failures; RKI match controller, unknown status word, non-numeric cells, lab-number-only
  rows, soft-deleted submissions, all-or-nothing; county pages and upload (025; the importer, matching and exports are
  tested).
- **TEST-002 – Misleading or weak existing tests** (Test; source: 011, 012, 022 Edge Cases, 006 and 007 verification): The PubMLST empty-query test
  calls the Laboratory export; one duplicate-resolver test depends on the wall clock and the RKI-layout test checks only
  column removal; the duplicate-window tests (0 days, exactly 6 months) pass under both readings of the rule;
  "…MultipleMatches_ReturnsAllEntries" verifies that one entry is returned. 006 (verification 2026-11-08):
  "Interpret_DistinctAgglutinationWithNotDeterminedBexA_NotDeterminedSerotype_NonInvasive_IsSpecificPreliminaryTypingWithEmptyDisclaimer"
  asserts the non-invasive note, not an empty disclaimer; the negative/negative test checks only the fragment
  "nicht-typisierbar" (the exact text is now asserted by an added test). 007 (verification 2026-11-08): most
  rule-named tests in `Domain/MeningoIsolateInterpretationTests` do not assert the selected rule (e.g. the rule 32 test
  checks the flag only through its list of no-Meningococci rules); the "30b" and "31b" test cases repeat the inputs of
  "30a"/"31a" (the "30a"/"30b" cases select rule 01b); several tests draw inputs at random
  (`GetRandomNegativeOrInhibitory` and similar), so each run checks a different combination. The added test
  `Interpret_RepresentativeInput_SelectsDocumentedRule` asserts the rule identifier and typing order for every
  reachable rule. 012 (verification 2026-11-08): tests with fixed dates added for the undisputed parts of the
  duplicate rules (10 days dropped, 7 months kept, reference replacement, all four strain characteristics, case-
  sensitive matching, group cells, unadjusted mode, empty selection) and one with the RKI layout; the disputed window
  distances (DEC-009) and the export actions' mode choice stay untested, and the wall-clock test is unchanged.
  Flaky: `Tools/HaemophilusPubMlstExportTests.DataTable_ContainsAntibioticMeasurements("CTX", Cefotaxime,
  Susceptible, …)` failed once in the full suite (found "I") and passed on rerun; `Setup` adds a random E-test, and when
  it has the same antibiotic it is found first.
- **TEST-003 – Decision-table rows derived from code only** (Test; source: 006 Decision Table note): Haemophilus
  interpretation rows 4 and 7 and the unmatched combinations have no test; the null-submission failure is untested.
  Addressed 2026-11-08 (006 verification): `Domain/IsolateInterpretationTests` now asserts rows 4 and 7, the unmatched
  combinations, the disclaimer for every negative-agglutination combination and sampling location, and the failure
  without a submission.
