# Backlog: Potential bugfixes and refactorings

**Branch**: `speckit-init`

**Created**: 2026-10-30

**Status**: Draft — not yet triaged by the owner

**Source**: Every `[SUSPECTED-DEFECT]` and `[POTENTIAL-REFACTORING]` observation recorded in the as-is
specifications `specs/001` to `specs/025` (status-quo inventory of the NRZMHi Database), consolidated into one
entry per underlying issue. Items observed in several specs are listed once and cite all sources.

> **Rule** (as in the owner's earlier Spec Kit project): nothing in this list is approved. Before an item is
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

## Spec structure decisions (carried over from the retired `feature-prompts.md`)

These decisions shaped how the as-is specs were cut. They are recorded here so the reasoning survives the removal of
the planning file; D6 lives in `.specify/memory/constitution.md` (v1.1.0).

- **D1 — Meningococci vs Haemophilus: separate specs per capability.** The two domains behave differently and were kept
  parallel only for lack of time for a shared architecture; shared concepts (Sender, export mechanism, docx
  templates, merge logic) are called out explicitly in each spec.
- **D2 — As-is only.** Specs capture the code on this branch; in-flight refactorings (e.g. the Haemophilus
  interpretation tree in PR #186) are documented as-is and re-run on the refactor branch later.
- **D3 — Exports: one generic spec (011)** documenting the mechanism and enumerating the targets, not one spec per
  target.
- **D4 — Reporting: preliminary + final together, per pathogen (008, 009)**; interpretation engines separately per
  pathogen because the mechanisms differ (006 if/else tree vs 007 JSON ruleset).
- **D5 — Report templates: own spec (010)**, because the docx mechanism may move to a different system.

## DEC — Owner decisions

- **DEC-001 – Administrator role: defect or intended?** (Decision; source: 011, 013, 014, 015, 017, 018, 019, 020,
  021 US5): An Administrator-only account sees the full laboratory navigation and all Administration entries, but
  every function except the unlinked role pages and the functions open to any signed-in account refuses it. Nine specs
  tag this `[SUSPECTED-DEFECT]`. Offen: is Administrator meant to be a pure role-management role (then the navigation
  is wrong and the tags become one UX item) or a superset of Standard User (then the authorization rules are wrong)?
- **DEC-002 – Sender export counts soft-deleted submissions** (Decision; source: 013 Edge Cases, FR-008; 001 FR-015):
  Senders are selected via non-deleted submissions, but "Anzahl Einsendungen", "Stammnummern" and "Labornummern"
  include soft-deleted ones. Offen: intended (deleted submissions still "sent") or defect?
- **DEC-003 – "Gültig für = Allgemein" breakpoints** (Decision; source: 017 Edge Cases): Storable, never offered for
  either pathogen. Offen: dead value to remove, or should such records apply to both pathogens?
- **DEC-004 – Haemophilus interpretation: growth "Ja"/"Nein" wording** (Decision; source: 006 FR-012/FR-013,
  test-asserted): With all typing not determined, growth "Ja" yields "Der eingesendete Stamm konnte nicht angezüchtet
  werden …", growth "Nein" yields "Kein Nachweis von Haemophilus influenzae." — inverse to the "Wachstum" label but
  asserted by tests. Offen: intended laboratory semantics or long-standing defect?
- **DEC-005 – Meningococci native rule 32 and the NoMeningococci flag** (Decision; source: 007 FR-010, Edge Cases,
  test-asserted): An all-negative native-material result does not set the flag, so it counts as "Meningococci found"
  in the RKI/IRIS selection (011 FR-011). Related: rules 11, 14, 41 emit sequence rows without the sequencing-provider
  comment; native rules 30a/30b are fully shadowed by 01b/01a. Offen: clinical intent for each.
- **DEC-006 – LGA export open to Public Health** (Decision; source: 011 Edge Cases, 021 US4.2): Authorized for the
  `RKI` role but not in its navigation. Offen: should Public Health have it (add to menu) or not (restrict)?
- **DEC-007 – Soft deletion of Meningococci submissions** (Decision; source: 003 US3, 015, 020 US4): No Meningococci
  delete-confirmation or deleted-submissions page exists; "Eintrag löschen" ends on the error page. Offen: is soft
  delete wanted for Meningococci (add pages) or not (remove the button and inherited actions)?
- **DEC-008 – EpiScanGIS case selection** (Decision; source: 023 Edge Cases): Only sampling locations "Blut" and
  "Liquor" are exported, not the invasive set used by RKI/IRIS/LGA (011 FR-011); the date window starts 2019-10-01 with
  an asymmetric boundary. Offen: agreed with the receiver, or drift?
- **DEC-009 – Duplicate-submission window in adjusted exports** (Decision; source: 012 Edge Cases, FR-006): Code
  comments describe "less than six months", the implementation compares only the month component of the date
  difference (drops < 1 month and whole-year distances, keeps 1–11 months). Offen: which rule is intended, so that
  DATA-001 can be fixed and tested?
- **DEC-010 – Export duplicate rules for Meningococci** (Decision; source: 012 Edge Cases, 011): No Meningococci export
  applies duplicate cleaning or identifier removal; the Meningococci RKI page accepts but ignores the "unadjusted"
  setting. Offen: should Meningococci RKI/IRIS get the same pseudonymization as Haemophilus RKI/PubMLST?
- **DEC-011 – Report pages open to every signed-in account** (Decision; source: 008, 009 Edge Cases, 021 US4.4): See
  SEC-004; the decision is whether report generation stays available to roles other than Standard User at all.
- **DEC-012 – Where to keep D1–D5** (Decision): Keep the spec structure decisions above in this file, or amend the
  constitution (v1.2.0) with them.

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
- **SEC-005 – Anti-forgery token not verified on state-changing actions** (Bugfix; source: 021 Edge Cases
  "consolidated"; 002 FR-021, 003 FR-023, 011, 013, 014, 015, 016, 019, 020, 022, 024): Not verified on the combined
  submission create/edit saves, all exports (also any request method), both patient merges (a single request merges
  without the confirmation page), restore links, the health-office lookup (which stores records), report-status
  marking, the PubMLST and postal-code lookups, list data requests, the county upload, the RKI match import, and the
  role page save.
- **SEC-006 – No lockout, delay, counting or logging of failed sign-ins; no audit of authentication events** (Bugfix;
  source: 021 US1.6, Edge Cases "No audit of access"): Sign-ins, failures, registrations, role creations and password
  changes are not logged (constitution: failures affecting authentication MUST be logged).
- **SEC-007 – One secret for registration and the EpiScanGIS export, carried in the URL** (Bugfix; source: 021 US7.1–7.2,
  023): The token appears in web-server logs; the export is an anonymous GET.
- **SEC-008 – Unescaped external data rendered as markup** (Bugfix; source: 016 Edge Cases, 022 Edge Cases):
  Health-office address/phone/fax/email (from the RKI website or local edits) on the report page, and Geonames place
  names in the postal-code suggestion list.
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

## DATA — Data correctness and privacy

- **DATA-001 – Duplicate-submission window compares only the month component** (Bugfix after DEC-009; source: 012
  Edge Cases, FR-006).
- **DATA-002 – Duplicate matching over-merges patients without birth date and postal code** (Bugfix; source: 012 Edge
  Cases): Same initials and gender with both identifiers empty are one patient; the four values are concatenated
  without separator (collision risk).
- **DATA-003 – PubMLST export compares stem numbers as text** (Bugfix; source: 012 Edge Cases): "H1000" sorts before
  "H999", so the "earlier" submission is chosen by text order; RKI compares numerically.
- **DATA-004 – PubMLST export selection and encoding defects** (Bugfix; source: 011 Edge Cases, 012): Hint promises
  overseas data and all locations in unadjusted mode but both are always excluded; overseas check uses the free-text
  county ("Ausland") instead of the state; state "Ausland" yields region `blood`; H. haemolyticus is exported
  although marked "not to be exported"; the adjusted-mode removal of non-invasive rows is a no-op.
- **DATA-005 – Unknown birth date exported as age 0** (Bugfix; source: 011 Edge Cases, 023): Indistinguishable from an
  infant under one month in PubMLST, IRIS and EpiScanGIS.
- **DATA-006 – "Gelöschte Einsendungen (Haemophilus)" lists active submissions** (Bugfix; source: 020 US2, Edge Cases
  "Inverted selection"; 002/003 Assumptions): Inverted by the 2018 generics restructuring (commit `e8aac1d`); no page
  lists deleted submissions, restore works only by direct address.
- **DATA-007 – Saving the edit page of a deleted submission or sender silently restores it** (Bugfix; source: 020
  US3.3–3.4).
- **DATA-008 – Restore is a plain link without confirmation; unknown id ends on the error page** (Bugfix; source: 020
  US3.5, US5.4).
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
- **DATA-015 – Locally cleared health-office fields break the report page** (Bugfix; source: 016 Edge Cases): An empty
  telephone stops the page script; cleared email/fax display as "null".
- **DATA-016 – PubMLST batch matching defects** (Bugfix; source: 019 Edge Cases): Includes isolates of deleted
  sendings; batch asks public then IRIS while the single lookup asks IRIS then public (different records possible);
  local record found by PubMLST id only, ignoring the database (cross-database overwrite, database name not updated);
  unhandled remote errors abort the run and leave partial saves; a duplicate locus would crash the run; each matched
  isolate is written back in full (overwrites concurrent edits); "Failed to match isolate by reference" is logged for
  every isolate including successes.
- **DATA-017 – Haemophilus interpretation text contains the typo "phänotpischen"** (Bugfix; source: 006 FR-006,
  test-asserted — the test must change too).
- **DATA-018 – Meningococci interpretation rules without sequencing comment / shadowed rules** (Bugfix after DEC-005;
  source: 007 Edge Cases): Rules 11, 14, 41; native 30a/30b unreachable; rule 09's cnl case shadowed by rule 20.
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
- **DATA-024 – Report template failures and empty-list label** (Bugfix; source: 010 Edge Cases): Template retrieval or
  filling failures show no user message; the "Keine Vorlagen hinterlegt" label is never shown (specs 008/009 aligned).
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
  navigation** (Refactoring; source: 001 Assumptions).
- **REF-009 – Postal-code lookup leftovers** (Refactoring; source: 022 Edge Cases, US3.3): `countries.json` snapshot from
  2020 (code TODO to refresh); "Unbekannt" assigned twice; fields matched by id suffix; `LoadCountries` reachable by
  URL returning a type name.
- **REF-010 – Deleted-records page loads all active submissions and looks up each isolate; skip-validation switch set
  on the wrong data context** (Refactoring; source: 020 Edge Cases).
- **REF-011 – Interpretation code smells** (Refactoring; source: 006 Key Entities/Assumptions, 007 Edge Cases):
  `[Obsolete]`-marked fields are the only ones used by the Haemophilus path; always-true guard in the negative/negative
  branch; TODO to move interpretation into the shared base class; rule-order comment in `StemRules.json` names the wrong
  rules; rule evaluation order relies on dictionary enumeration order; comment retention on reused interpreter
  instances.
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

## TEST — Test gaps

- **TEST-001 – Untested functions** (Test; source: Assumptions/Edge Cases of 013, 014, 015, 016, 017, 018, 019, 020,
  021, 022, 023, 024): Sender export; both patient merges; health-office lookup, matching, storing and pages;
  breakpoint validator and pages; configuration page; deleted-records pages and restore; registration, role checks and
  navigation by role; browser behavior and both endpoints of the postal-code lookup; PubMLST update of an existing local
  record, result page and matching page; EpiScanGIS date window, "Blut und Liquor" exclusion, deleted submissions,
  missing coordinates and failures; RKI match controller, unknown status word, non-numeric cells, lab-number-only
  rows, soft-deleted submissions, all-or-nothing; county pages and upload (025; the importer, matching and exports are
  tested).
- **TEST-002 – Misleading or weak existing tests** (Test; source: 011, 012, 022 Edge Cases): The PubMLST empty-query test
  calls the Laboratory export; one duplicate-resolver test depends on the wall clock and the RKI-layout test checks only
  column removal; the duplicate-window tests (0 days, exactly 6 months) pass under both readings of the rule;
  "…MultipleMatches_ReturnsAllEntries" verifies that one entry is returned.
- **TEST-003 – Decision-table rows derived from code only** (Test; source: 006 Decision Table note): Haemophilus
  interpretation rows 4 and 7 and the unmatched combinations have no test; the null-submission failure is untested.
