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
  Confirmed on system 2026-11-08 for the exports (011 verification): `Admin1` sees the full Export menu (both
  "Einsender Export" entries and the seven exports of 011), and every one of the seven export addresses, form and
  direct run, redirects it to the sign-in page.
  Confirmed on system 2026-11-08 for the sender exports (013 verification): `Admin1` sees both "Einsender Export"
  entries, and both addresses, form and direct run, redirect it to the sign-in page.
  Confirmed on system 2026-11-08 for the PubMLST matching (019 verification; observation only, no decision): `Admin1`
  sees "PubMLST" → "Matching (Meningokokken)", and `/MeningoIsolate/PubMlstMatch` redirects it to the sign-in page.
- **DEC-002 – Sender export counts soft-deleted submissions** (Decision; source: 013 Edge Cases, FR-008; 001 FR-015):
  Senders are selected via non-deleted submissions, but "Anzahl Einsendungen", "Stammnummern" and "Labornummern"
  include soft-deleted ones. Offen: intended (deleted submissions still "sent") or defect?
  Observed on system 2026-11-08 (013 verification, no decision): the seeded sender "SV-Seed-DEC-002" appears in the
  2020 sender exports with "Anzahl Einsendungen" 2, "Labornummern" "KL002/20,KL003/20" and "MZ002/20,MZ003/20"
  (KL003/20 and MZ003/20 soft-deleted); an own sender with three active and one soft-deleted Haemophilus submission in
  2022 shows 4 and lists the deleted one; a sender whose only submission in the range is soft-deleted is not listed.
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
  Observed on system 2026-11-08 (011 verification, no decision taken): `Rki1` sees only "RKI Export (Haemophilus)"
  and "RKI Export (Meningokokken)"; `/MeningoPatientSending/StateAuthorityExport` opens the form for it and a direct
  request with a date range returns `LGA-Export_20200101-20201231.xlsx`.
- **DEC-007 – Soft deletion of Meningococci submissions** (Decision; source: 003 US3, 015, 020 US4): No Meningococci
  delete-confirmation or deleted-submissions page exists; "Eintrag löschen" ends on the error page. Offen: is soft
  delete wanted for Meningococci (add pages) or not (remove the button and inherited actions)? Confirmed on system
  2026-10-30 (003 verification; observation only, no decision): "Eintrag löschen" and `/MeningoPatientSending/Deleted`
  end in HTTP 500 (view not found); a delete request with anti-forgery token sent outside the pages soft-deletes the
  submission, and `/MeningoPatientSending/Undelete/<id>` restores it.
- **DEC-008 – EpiScanGIS case selection** (Decision; source: 023 Edge Cases): Only sampling locations "Blut" and
  "Liquor" are exported, not the invasive set used by RKI/IRIS/LGA (011 FR-011); the date window starts 2019-10-01 with
  an asymmetric boundary. Offen: agreed with the receiver, or drift? Observed 2026-11-08 (023 verification, unit level,
  no decision taken): of the invasive locations only "Blut" and "Liquor" are exported; a case sampled on 2019-10-01 is
  left out, a case without sampling date received on 2019-10-01 is exported.
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
  report page by direct address and can generate documents and set the report status. Observed on system 2026-11-08
  (009 verification, no decision taken): the same holds for the Meningococci report page.
- **DEC-012 – Where to keep D1–D5** (Decision; **decided 2026-10-30**): Moved to the constitution (v1.2.0, section
  "Specification Structure"). This file keeps only the pointer above.

## SEC — Security and access

- **SEC-001 – Registration submission does not re-check the token** (Bugfix; source: 021 US6.6, FR-007): Only showing
  the form checks `RegistrationToken`; the POST checks just the anti-forgery token, which any visitor obtains from the
  sign-in page. Anyone can register an account with any stored role.
- **SEC-002 – Registration with an unknown role leaves a role-less account** (Bugfix; source: 021 US6.7): The account
  is created before the role assignment fails with the generic error page.
- **SEC-003 – Missing `RegistrationToken` setting opens registration and the EpiScanGIS export** (Bugfix; source: 021
  Edge Cases, 023): A request without token would match an absent setting. Reproduced at unit level 2026-11-08 (023
  verification; the controller run with stubbed data in uncommitted tests, because a run on the system needs the secret
  token): with the setting absent, the EpiScanGIS export answers a request without token with the file; a wrong token
  still gets "not found". On the host the key exists in `Web.config`, its template and both transforms (counted, values
  not read).
- **SEC-004 – Report pages, health offices and RKI match import check no role** (Bugfix; source: 008/009 Edge Cases,
  016 Edge Cases, 021 US4.4–4.6, US7, access matrix, 024): Public Health (and role-less) accounts can open the report
  page of any isolate (patient data, document generation, status marking), create/edit/delete health offices, and
  upload RKI match files. Only the navigation hides these functions (Constitution Principle IV). See DEC-011.
  Confirmed on system 2026-11-08 (008 verification): `Rki1` and `Admin1` open `/Report/Isolate/14` (HTTP 200, patient
  initials and postal code shown) although the isolate form and the submissions list redirect them to the sign-in
  page; `Rki1` generated a "Teilbefund" document there, which set the isolate to "Teilbefund". Confirmed on system
  2026-11-08 (009 verification) for Meningococci: `Rki1` and `Admin1` open `/MeningoReport/Isolate/16` (HTTP 200, patient
  initials and postal code shown) while `/MeningoIsolate/Edit/16` and `/MeningoPatientSending` redirect them to the
  sign-in page; `Rki1` generated a "Fax - Teilbefund - v2.docx" document, which set MZ012/26 to "Teilbefund". Confirmed
  on system 2026-11-08 (024 verification): `Rki1` and `Admin1` open `/RkiMatch` (HTTP 200, page "RKI Matching Daten"),
  and an upload by `Rki1` was processed ("File Uploaded Successfully!!").
- **SEC-005 – Anti-forgery token not verified on state-changing actions** (Bugfix; source: 021 Edge Cases
  "consolidated"; 002 FR-021, 003 FR-023, 008 FR-015, 009 FR-016, 011, 013, 014, 015, 016, 019, 020, 022, 024): Not verified on the combined
  submission create/edit saves, all exports (also any request method), both patient merges (a single request merges
  without the confirmation page), restore links, the health-office lookup (which stores records), report-status
  marking, the PubMLST and postal-code lookups, list data requests, the county upload, the RKI match import, and the
  role page save. Confirmed on system 2026-10-30 (002 verification) for the combined Haemophilus submission create
  save: a POST without token is stored. Confirmed on system 2026-10-30 (003 verification) for the combined
  Meningococci submission create and edit saves. Confirmed on system 2026-11-08 (008 verification) for the
  Haemophilus report-status marking: the report page itself posts only `id` and `preliminary` to
  `/Report/ReportGenerated`, without token, and the status is stored. Confirmed on system 2026-11-08 (009
  verification) for the Meningococci report-status marking: the page posts only `id` and `preliminary` to
  `/MeningoReport/ReportGenerated/`, without token, and the status is stored. Confirmed on system 2026-11-08 (011
  verification) for the exports: a GET with a date range returns the spreadsheet (e.g. the Haemophilus laboratory
  export), and a POST of only the dates without token returns the Meningococci RKI file. Confirmed on system
  2026-11-08 (013 verification) for both sender exports: a GET with a date range returns the Haemophilus sender file,
  and a POST of only the dates without token returns the Meningococci sender file. Confirmed on system 2026-11-08 (022
  verification) for both postal-code lookups: the forms post only `postalCode`/`placeName` and `postalCodePrefix`,
  without token, and get the answer (HTTP 200). Reproduced at unit level 2026-11-08 (019 verification) for the PubMLST
  lookup: the isolate form posts only `isolateReference` (request held in the browser), and the lookup action carries
  no token check. Confirmed on system 2026-11-08 (024 verification) for the RKI match import: the upload form carries no
  token, and every upload of the verification was processed.
- **SEC-006 – No lockout, delay, counting or logging of failed sign-ins; no audit of authentication events** (Bugfix;
  source: 021 US1.6, Edge Cases "No audit of access"): Sign-ins, failures, registrations, role creations and password
  changes are not logged (constitution: failures affecting authentication MUST be logged).
- **SEC-007 – One secret for registration and the EpiScanGIS export, carried in the URL** (Bugfix; source: 021 US7.1–7.2,
  023): The token appears in web-server logs; the export is an anonymous GET. Not verifiable in the 023 verification
  (2026-11-08): the host keeps no web-server request log; the anonymous GET is test-asserted
  (`Index_AdmitsAnonymousGetWithoutRoleOrAntiForgeryCheck`).
- **SEC-008 – Unescaped external data rendered as markup** (Bugfix; source: 016 Edge Cases, 022 Edge Cases; 008
  FR-006; 009 FR-008): Health-office address/phone/fax/email (from the RKI website or local edits) on the report page, and Geonames
  place names in the postal-code suggestion list. Confirmed on system 2026-11-08 (008 verification, with a synthetic
  lookup answer injected in the browser because the RKI call is blocked): an address containing `<b>…</b>` is rendered
  bold on the Haemophilus report page. Confirmed on system 2026-11-08 (009 verification, synthetic answer as above) for
  the Meningococci report page. Confirmed on system 2026-11-08 (022 verification, synthetic suggestion answer injected
  in the browser): "Sankt Peter's Ort" is offered as "12371 Sankt Peter", and text after an apostrophe adds an
  attribute to the suggestion and a `<b>` element to the page.
- **SEC-009 – Geonames called over plain HTTP** (Bugfix; source: 022 Edge Cases): Account name and typed patient/sender
  postal codes travel unencrypted (Principle IV).
- **SEC-010 – Rendering libraries from public CDNs on every page, two without integrity check** (Refactoring; source:
  010 Edge Cases). Confirmed on system 2026-11-08 (010 verification): the sender list loads docxtemplater and
  FileSaver from cdnjs (with integrity) and `pizzip.js` / `pizzip-utils.js` from unpkg (without integrity).
- **SEC-011 – Internal helper actions reachable by URL** (Refactoring; source: 021 Edge Cases, 022): Create patient,
  create sending, assign stem/laboratory numbers (Standard User), country list (any signed-in account) act without
  form, token or duplicate check.
- **SEC-012 – Temporary export/import files are never deleted** (Bugfix; source: 011, 013 Edge Cases, 023, 024, 025):
  Every spreadsheet export, the EpiScanGIS export, the RKI match upload and the county upload leave files in the
  server's temp directory; exports and the RKI match file contain patient data (Principle IV). Confirmed on system
  2026-11-08 (011 verification): every export run of the verification left one `tmp*.tmp` file in the temp directory
  of the account running IIS Express, a spreadsheet of the same size as the downloaded file (e.g. 5318 bytes for the
  Haemophilus laboratory export of 2024), still present after the download. Confirmed on system 2026-11-08 (013
  verification) for both sender exports: each of the nine runs left a `tmp*.tmp` spreadsheet of the downloaded file's
  size (e.g. 2394 bytes for the Haemophilus sender export of 2022). EpiScanGIS (023 verification, 2026-11-08): a
  successful run deletes its temporary file (unit test `Download_SavesNothingAndLeavesNoTemporaryFile`); an aborted run
  (network failure, non-JSON answer, submission without isolate) leaves an empty `tmp*.tmp` file (reproduced at unit
  level). Confirmed on system 2026-11-08 (024 verification) for the RKI match upload: each of the 18 non-empty uploads
  left a `tmp*.tmp` copy of the uploaded file (same size, synthetic content) in the temp directory of the account
  running IIS Express; a zero-length upload and a request above the size limit left none.
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
- **SEC-015 – Refused EpiScanGIS token distinguishable from an unknown address** (Bugfix; source: 023 US1.2):
  A missing or wrong token gets HTTP 404 with the web server's "HTTP Error 404.0 - Not Found" page, an unknown
  address HTTP 404 with the framework's "Die Ressource kann nicht gefunden werden." page, so the refusal shows
  that the export address exists. Observed on system 2026-11-08 (023 verification) for `/EpiscanGisExport`
  without token, with an empty and with a wrong token, anonymous and as `Standard1`, `Admin1`, `Rki1` and
  `Labor1`, against `/SV023DoesNotExist` and `/EpiscanGisExport/SV023Nothing`.

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
  export and missing from the PubMLST file in both modes. Confirmed on system 2026-11-08 (011 verification): the
  "Ja" and "Nein" files of 2024 have the same five rows, without the "Ausland"-county and the non-invasive submission
  the hint promises; a patient with state "Ausland" and county "Paris" is exported with region `blood`; an isolate
  evaluated "H. haemolyticus" is exported with serotype "Haemophilus Hemolyticus".
- **DATA-005 – Unknown birth date exported as age 0** (Bugfix; source: 011 Edge Cases, 023): Indistinguishable from an
  infant under one month in PubMLST, IRIS and EpiScanGIS. Confirmed on system 2026-11-08 (011 verification): a
  Haemophilus patient without birth date has `age_yr` 0 and `age_mth` 0 in the PubMLST file, a Meningococci patient
  without birth date 0 years and 0 months in the IRIS file. Reproduced at unit level 2026-11-08 (023 verification; the
  controller run with stubbed data in uncommitted tests, because a run on the system needs the secret token): a
  Meningococci patient without birth date gets age `0` in the EpiScanGIS line.
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
  FR-006; 009 FR-008): An empty telephone stops the page script; cleared email/fax display as "null". Confirmed on system
  2026-11-08 (008 verification, synthetic lookup answers injected in the browser): without telephone only the address
  appears, fax and email stay empty, "Gesundheitsamt bearbeiten" stays inactive and the browser reports "Cannot read
  properties of null (reading 'replace')"; without fax and email both show "null" (fax linked to `mailto:-@fax`).
  Document generation still works. Confirmed on system 2026-11-08 (009 verification, same synthetic answers) for the
  Meningococci report page, with the same messages.
- **DATA-016 – PubMLST batch matching defects** (Bugfix; source: 019 Edge Cases): Includes isolates of deleted
  sendings; batch asks public then IRIS while the single lookup asks IRIS then public (different records possible);
  local record found by PubMLST id only, ignoring the database (cross-database overwrite, database name not updated);
  unhandled remote errors abort the run and leave partial saves; a duplicate locus would crash the run; each matched
  isolate is written back in full (overwrites concurrent edits); "Failed to match isolate by reference" is logged for
  every isolate including successes. Reproduced at unit level 2026-11-08 (019 verification; the real matcher and
  clients with stubbed remote answers, in uncommitted tests, because a run on the system would call PubMLST): an
  isolate of a deleted sending is matched; the production wiring asks the public database first in the batch and IRIS
  first in the lookup; an IRIS record with the id of a stored public record overwrites its values and keeps
  "pubmlst_neisseria_isolates"; a "not found" status of the search, a connection failure, an IRIS answer to rejected
  credentials and a duplicate locus each end the run with an exception after earlier isolates were saved; the error
  entry is written for a successful match. Without the IRIS settings every individual lookup fails before any request,
  so the public database is never asked. The concurrent-edit overwrite was not reproduced.
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
  values written unescaped without header. Reproduced at unit level 2026-11-08 (023 verification; the controller run
  with stubbed data in uncommitted tests, because a run on the system needs the secret token): "Blut und Liquor",
  "Gelenkspunktat", "Petechien", "Pleurapunktat", "Serum" and "Anderer invasiv" left out; boundary as in DEC-008; a
  network failure (or a non-JSON answer) on the third lookup ends the run with an exception and no file; "Wohnort"
  "Graefenhainichen" (service: "Gräfenhainichen") and "Unbekannt" left out; a PorA VR1 "22;…" adds a column and a line
  break in FetA VR adds a line. See also DATA-041.
- **DATA-021 – RKI match import skips rows without `klhi_nr`** (Bugfix; source: 024 US3.1): The laboratory-number
  fallback is reached only via a stem-number row; "Upload" without a file ends in an object-reference message. Confirmed
  on system 2026-11-08 (024 verification): a row with `LaufendeNummer`/`Jahr` of an existing submission and an empty
  `klhi_nr` created no record; "Upload" without a file shows "File upload failed: Der Objektverweis wurde nicht auf eine
  Objektinstanz festgelegt." (the framework message in the server's language).
- **DATA-022 – Postal-code lookup overwrites a foreign residence** (Bugfix; source: 022 Edge Cases): Typing a postal code
  resets "Bundesland" away from "Ausland" and overwrites "Wohnort"/"Landkreis"; the Meningococci state/country check
  then rejects the save. Confirmed on system 2026-11-08 (022 verification): on the Meningococci submission form with
  "Ausland" and "Österreich", typing "A-1010" set "Wohnort" and "Landkreis" to "Unbekannt" and "Bundesland" to
  "n.n."; "Land" stayed "Österreich" (the save itself was not attempted).
- **DATA-023 – Postal-code suggestion script expects `postalCodes`, server fallback returns `postalcodes`** (Bugfix;
  source: 022 Edge Cases): Suggestions vanish with a browser script error; the empty-result and "error" checks never
  apply. Confirmed on system 2026-11-08 (022 verification, answers injected in the browser; the server side reproduced
  at unit level): the fallback `{"postalcodes":[]}` and a service status message both empty the suggestion list with
  "Cannot read properties of undefined (reading 'length')". Live empty results of the service (`{"postalCodes":[]}`,
  e.g. for "00000") are handled without error.
- **DATA-024 – Report template failures and empty-list label** (Bugfix; source: 010 Edge Cases; 008 Edge Cases; 009 Edge Cases): Template
  retrieval or filling failures show no user message; the "Keine Vorlagen hinterlegt" label is never shown (specs 008/009
  aligned). Confirmed on system 2026-11-08 (008 verification): with the template download aborted in the browser,
  "Befund erstellen" produced no document, no message and no status request; only a browser script error
  ("InvalidStateError: Failed to read the 'responseText' property …") was raised. Confirmed on system 2026-11-08 (009
  verification) for the Meningococci report page: the same outcome with the download of a `ReportTemplates/Meningo/`
  template aborted. Confirmed on system 2026-11-08 (010 verification) for processing failures: a template file that is
  not a zip raised "Can't find end of central directory : is this a zip file ?" without any console output from the
  page code; a template with an unclosed loop tag logged "Unclosed loop" / "Multi error" to the console; an offered
  folder entry answered HTTP 403. In every case no document, no message and no `ReportGenerated`; the isolate's report
  status stayed unchanged. The empty-list label could not be reproduced on the system (it needs an empty template
  folder).
- **DATA-025 – County version choice ignores the submission date** (Bugfix; source: 011 Edge Cases, 025 Edge Cases):
  When several stored county versions match, the earliest validity date wins regardless of sampling/receiving date;
  because uploads only add records, a renumbered county keeps exporting its old number. Confirmed on system 2026-11-08
  (011 verification): with two versions of one county ("09111" valid since 01.01.2000, "08222" valid since
  01.01.2015), submissions sampled in 2024 got "09111" / "09" in the Haemophilus RKI, Meningococci RKI and LGA files.
- **DATA-028 – County normalization collides "X, Stadt" with "Landkreis X"** (Bugfix; source: 025 Edge Cases): Both
  normalize to "X", so a district patient can receive the city's county number (earliest validity or storage order
  wins).
- **DATA-029 – A county record with empty name matches every patient without county text** (Bugfix; source: 025 Edge
  Cases): Absent equals absent in the comparison; all three exports then assign its number and the Meningococci
  exports log no warning.
- **DATA-026 – "Anzahl Einsendungen" written as text** (Bugfix; source: 013 Edge Cases): The generic spreadsheet writer
  treats only certain numeric types as numbers. Confirmed on system 2026-11-08 (013 verification): both sender exports
  write "Anzahl Einsendungen" as a text cell and "Einsendernummer" as a numeric cell.
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
  FR-018, Edge Cases; 008 Edge Cases; 009 Edge Cases): The form posts the report status it loaded and "Befund am". Confirmed on system 2026-11-04 (004 verification, with the
  request the report page sends after generating a report): a form opened before that request set the isolate back to
  "Kein Befund erstellt" and cleared the date on save. Reachable whenever the form stays open while a report is
  generated. Saving a fresh form stored the date with the time 00:00. The Meningococci form carries the same hidden
  status field (from code). Confirmed on system 2026-11-08 (005 verification) for Meningococci isolates: a form opened
  before the report request reset "Endbefund" to "Kein Befund erstellt" and cleared the date; a fresh form kept the
  status and stored the date with the time 00:00. Confirmed on system 2026-11-08 (008 verification): an isolate form
  opened while the isolate was "Teilbefund", saved after a final report had been generated from the report page, set
  the isolate back to "Teilbefund" without report date. Confirmed on system 2026-11-08 (009 verification) with the
  Meningococci report page: the form of MZ012/26 opened at "Teilbefund", a final report generated meanwhile (Endbefund
  with date), then the form saved: back to "Teilbefund" without report date.
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
  (Bugfix; source: 005 US3.5, FR-021, Edge Cases, SC-005; 011 Edge Cases; 019 Edge Cases): The form always posts the PubMLST part, and the save assigns
  it to the isolate; with PubMLST id 0 it becomes a new, empty local PubMLST record and the previously linked one
  stays behind. Confirmed on system 2026-11-08 (005 verification): the first save of an isolate without lookup created
  an empty record, each further save another one (about 20 empty records after the verification). After a lookup
  that found nothing, saving an isolate that was linked to a matched record replaced that link with a new empty
  record, so the match was lost without a message. Saving a matched isolate without a new lookup kept its link. From
  code: the batch matching (019) and the exports read the linked record. Overlaps DATA-016. Confirmed on system
  2026-11-08 (011 verification) in the exports: every isolate saved without lookup shows "PubMLST ID" 0 with empty
  typing columns in the Meningococci laboratory and RKI files, while isolates without any linked record show "-".
  Confirmed on system 2026-11-08 (019 verification): 40 of the 41 local PubMLST records in the catalog carry PubMLST
  id 0, so the "one local record per PubMLST id" of 019 SC-003 does not hold for id 0.
- **DATA-037 – Meningococci isolate form ignores a changed laboratory number** (Bugfix; source: 005 FR-003, FR-014, Edge
  Cases): The form carries the laboratory number as a hidden field before the visible input, the request contains
  both values, and the server binds the first one. Confirmed on system 2026-11-08 (005 verification): "007/26" entered
  for MZ006/26 was posted as "006/26, 007/26" and the isolate kept 006/26 without a message; the laboratory-number
  conflict of 005 FR-014 is therefore unreachable through the form. The Haemophilus form has no such hidden field (004).
- **DATA-038 – Meningococci no-growth material change fires on the default values and is never undone** (Bugfix;
  source: 005 FR-011, Edge Cases; 011 Edge Cases): Both growth fields default to "Nein", so saving a new isolate of a "Vitaler Stamm"
  submission without entering growth sets the material to "Nicht angewachsen"; entering growth later does not set it
  back. Confirmed on system 2026-11-08 (005 verification). From code: the material is exported (laboratory and IRIS
  exports, 011) and shown on the submission form (003). Offen: should the material follow the growth fields in both directions, and
  should "Nein" stay the default? Confirmed on system 2026-11-08 (011 verification): three "Vitaler Stamm"
  submissions whose isolates were first saved with the defaults and later with "typisches Wachstum" are exported
  with material "Nicht angewachsen" in the laboratory and IRIS files.
- **DATA-039 – Unmatched Meningococci interpretation counts as "Meningococci found" in the RKI/IRIS selection**
  (Bugfix; source: 007 Edge Cases, FR-011, FR-014; 011 FR-011): Without a matching rule the no-Meningococci flag stays
  false, and the RKI and IRIS exports include every invasive submission whose flag is false, so submissions with
  discrepant or not yet entered results are exported as Meningococci cases. Observed on system 2026-11-08 (007
  verification): six invasive submissions without a rule (empty "Regel" in the laboratory export) appeared in the RKI
  export for November 2026. Related: DEC-005 (rule 32). Offen: should unmatched results be excluded, flagged, or kept?
  Confirmed on system 2026-11-08 (011 verification): three invasive submissions of 2024 without a rule (one with
  the inputs of rule 08 except a negative FetA PCR, one with only growth and a negative oxidase, one of an "Ausland"
  patient) are in the RKI, IRIS and LGA files; the same six November 2026 submissions as in 007 and the rule 32
  submission are in all three files.
- **DATA-040 – Editing a submission stores duplicate patient identities without a check** (Bugfix; source: 002
  FR-015, 012 Edge Cases): Intake checks initials, birth date and postal code against existing patients and blocks a
  third record (002 FR-011), but the edit save of a submission changes these values without any check. Observed on
  system 2026-11-08 (012 verification): two "X.Y." patients created with other birth dates were edited to the birth
  date of two existing "X.Y." patients, leaving four patient records with identical values; the exports group them
  as one patient (012 FR-003). The intake rejects every further submission with these values ("Es existieren bereits
  zwei Patienten …"; observed with two records, from code for any number above one). Related: DATA-031.
- **DATA-041 – EpiScanGIS export drops cases silently on a Geonames error answer** (Bugfix; source: 023 US6.3,
  FR-011; related 022 Edge Cases "Service unavailable or failing"): The lookup treats only HTTP 404 as an error;
  any other answer with a JSON body but no list of places (for example the service's account or quota message)
  counts as "no place", so the case is left out without log entry and the file is delivered. An exhausted quota
  therefore yields a file with fewer or no cases that looks like a normal run. Reproduced at unit level
  2026-11-08 (023 verification): a status answer for every lookup gave an empty file for 10 qualifying cases.
  Also affects the interactive lookup (022 FR-008, Edge Cases "Service unavailable or failing"): a status answer
  makes the form write "Unbekannt"/"n.n." as for an unknown postal code, without notice. Reproduced at unit level
  2026-11-08 (022 verification: the lookup returns the status with an empty place list) and shown in the browser with
  that answer injected.
- **DATA-042 – Failed postal-code lookup leaves the previous place next to a new postal code** (Bugfix; source: 022
  Edge Cases "Service unavailable or failing"): A change of "Postleitzahl" to exactly five characters empties nothing
  before the lookup; when the lookup then fails (network failure, non-JSON answer, HTTP 500), "Wohnort"/"Ort",
  "Landkreis" and "Bundesland" keep the values of the previous postal code and are saved with the new one without
  notice. Observed on system 2026-11-08 (022 verification, synthetic HTTP 500 injected in the browser): "91301" →
  "97080" left "Forchheim", "Landkreis Forchheim", "Bayern".

## UX — Robustness and user feedback

- **UX-001 – Navigation shows functions the role cannot use** (Bugfix after DEC-001; source: see DEC-001): Also LGA
  export hidden from Public Health (DEC-006).
- **UX-002 – Refusal is shown as the sign-in page** (Refactoring; source: 021 Edge Cases): No "access denied" message;
  signing in again leads to the same refusal.
- **UX-003 – Postal-code lookup: one request per keystroke, no debounce, race on answers, only "not found" handled**
  (Refactoring; source: 022 Edge Cases): Quota use; last answer wins; other failures end in the generic server error;
  log message with stray "$"; parameters not URL-encoded. Observed 2026-11-08 (022 verification): typing "913" sent
  three suggestion requests, each five-character change two requests; network failures and non-JSON answers end in
  an exception (HTTP 500) and "&"/"#" in the typed content are inserted unencoded (reproduced at unit level); JSON
  status answers are not failures but "no place" (DATA-041). See DATA-042.
- **UX-004 – Template discovery accepts lock/temp `.docx` files, applies no sort, matches the preliminary marker on
  the whole path** (Refactoring; source: 010 Edge Cases). Confirmed on system 2026-11-08 (010 verification): a
  temporary `~$SV-010-Lock.docx` placed in `ReportTemplates/` was offered at once; a folder named `SV-010-Ordner.docx`
  was offered as well (discovery lists file-system entries, not only files) and choosing it ended in HTTP 403 without
  a message. The order was the file system's (case-insensitive alphabetical on the NTFS host, `~$…` last).
- **UX-005 – Health-office list and hidden pages** (Refactoring; source: 016 US2.3, US3, Edge Cases): List shows no
  address and only the first lookup's postal code; details/delete pages are untranslated and unlinked; the RKI website
  is queried before local records on every lookup (unreachable website → nothing shown).
- **UX-006 – RKI match import always reports "File Uploaded Successfully!!"** (Refactoring; source: 024 US1.5): Also
  for zero matches or an empty file; failures never reach the application log. Observed on system 2026-11-08 (024
  verification): the success line also for a header-only file, a file with unknown headers and a file whose rows match
  nothing; failures (unknown status word, non-numeric cell, no file, request above the size limit) left no entry in the
  application log; failure lines mix English library messages with German framework messages; the page sets no title
  (browser tab "Haemophilus Web -").
- **UX-007 – PubMLST matching form defaults to the current year, result table unsorted, no progress or cancel**
  (Refactoring; source: 019 US1.2–1.3, Edge Cases): Exports default to the previous year. Confirmed on system
  2026-11-08 (019 verification): "Start-Datum" 01.01.2026 and "Ende-Datum" 31.12.2026; after the click the button
  reads "Abfrage läuft ..." and is disabled, with no progress or cancel. The result order was not observable (the
  result page needs a run against PubMLST).
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
- **UX-013 – PubMLST lookup without stem number asks the remote databases for "DE -"** (Bugfix; source: 005 US3.3; 019 Edge Cases): The
  form checks only that the prefixed reference is not empty, but an isolate without stem number has the reference
  "DE -". Confirmed on system 2026-11-08 (005 verification, request blocked in the browser): "Abfragen" posted
  `isolateReference=DE -` to the lookup, which would ask IRIS and the public PubMLST database (019 US2), and showed
  "Isolat wurde nicht gefunden" only after the request failed. Confirmed on system 2026-11-08 (019 verification,
  request held in the browser): MZ002/26 without stem number posted `isolateReference=DE -`.
- **UX-014 – Meningococci isolate form label and message slips** (Bugfix; source: 005 Edge Cases): Confirmed on system
  2026-11-08 (005 verification): the PubMLST tab labels porB as "'porB"; the 16S rDNA percentage message is repeated
  under PorA-VR2 whenever the PorA fields are shown (the field reuses the 16S message slot).
- **UX-015 – Haemophilus reports print "≤--- µg/ml" for susceptibility results without breakpoints** (Bugfix; source:
  010 Edge Cases; 017 FR-012): The resistance-profile templates wrap the breakpoint values in "≤… µg/ml" and
  ">… µg/ml"; for an "ohne Grenzwerte" result the values are "---", so the report reads "≤--- µg/ml", ">--- µg/ml" and
  "---" as year next to "0,5 µg/ml, n.d.". Confirmed on system 2026-11-08 (010 verification, Azithromycin on isolate
  KL012/26, "BLNAS v10.docx"). Meningococci reports remove Azithromycin (009 FR-006).
- **UX-016 – PubMLST matching runs with empty or unreadable dates without notice** (Bugfix; source: 019 Edge Cases
  "Periods and bounds"): The form has no check and the action ignores the binding errors; an empty or unreadable date
  becomes the earliest possible date. An empty "Start-Datum" therefore matches every isolate up to the end date, back
  to the oldest record, against PubMLST in one request; an empty "Ende-Datum" matches nothing and reports "0".
  Confirmed on system 2026-11-08 (019 verification, submission held and aborted in the browser): "", "abc" and an
  empty end date are posted without a message. Reproduced at unit level 2026-11-08 (019 verification): the dates bind
  to 0001-01-01 with an invalid model state, and the matcher then selects an isolate sampled in 1999.
- **UX-017 – Failed PubMLST lookup leaves the link to the previous PubMLST entry enabled** (Bugfix; source: 019 US2.3;
  005 US3.3): After "Isolat wurde nicht gefunden" the form empties the typing fields and the database and sets the
  PubMLST id to 0, but the link keeps its text, target and enabled state. Confirmed on system 2026-11-08 (019
  verification, answers simulated in the browser): on MZ003/26 with stored record 99001 ("PubMLST Eintrag"), and
  after a successful simulated lookup on MZ001/26 ("PubMLST Eintrag anzeigen"), a failed lookup (HTTP 404 or
  500) left the link pointing to `…db=pubmlst_neisseria_iris&id=…` of the earlier record.

## REF — Refactoring and legacy debt

- **REF-001 – Parallel Meningococci/Haemophilus domains and god-controllers** (Refactoring; source: constitution "Known
  Architectural Constraints", cited by every spec): Business logic in controllers, manual wiring, duplicated
  per-pathogen controllers and views. Umbrella item; the concrete duplications follow.
- **REF-002 – Duplicated logic across pathogens** (Refactoring; source: 010, 014/015, 016, 017, 019): Report
  selection/rendering script (`Views/Report/Isolate.cshtml` vs `Views/MeningoReport/Isolate.cshtml`); merge lookup and
  reassign-and-delete; health-office lookup-and-match (report base vs edit-by-postal-code); antibiotic derivation and
  E-Test interpretation script; the 17-field PubMLST update list (batch vs isolate save, marked TODO in code). The two
  report scripts differ in the discrepancy check (`if (false)//… TODO` on the Meningococci page), the filename prefix,
  the `ReportGenerated` address with its isolate id, and the isolate-edit link (010 verification, 2026-11-08).
- **REF-003 – Services created inside controllers instead of injected** (Refactoring; source: 012, 016, 018, 019, 020,
  022, 023, 024): DuplicatePatientResolver, RKI client, ConfigurationService (plus a static test hook), PubMLST
  services and credentials, the restore path's separate sending controller, Geonames service (a controller
  instantiating another controller for the country list), EpiScanGIS and RKI match logic entirely in controllers.
- **REF-004 – Report templates: includes with absolute developer paths, fax variants embedding base templates,
  deprecated docxtemplater API, unused flattened typing values** (Refactoring; source: 010 Edge Cases). Confirmed
  2026-11-08 (010 verification): all 26 templates carry `INCLUDETEXT` fields pointing to
  `D:\Development\NRZMHiDB\HaemophilusWeb\ReportTemplates\…` (a path that does not exist even in the current
  checkout), and the generated documents keep these fields; the 14 Haemophilus fax variants with a base template
  include it, while the Meningococci fax variants include only the shared fragments; no template references a
  `Typing-<Attribute>` value; the page calls `loadZip`/`setData`/`setOptions`, without a console warning.
- **REF-005 – Export mechanism leftovers** (Refactoring; source: 011 Edge Cases, 013): Unnamed worksheet (shown as
  "Table1", observed 2026-11-08 in the 011 verification); Meningococci
  RKI model carries an ignored "unadjusted" field; PubMLST reuses the IRIS-named query (source TODO); sender export
  matches the range twice with diverging deletion handling and loads unused patient data; no explicit row ordering.
  013 verification 2026-11-08: the sender export files also show "Table1"; rows came in ascending sender number and
  the numbers within a row in ascending submission order, which nothing in the code guarantees.
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
  URL returning a type name. Observed on system 2026-11-08 (022 verification): `Rki1` and `Admin1` get HTTP 200 with
  the type name ``System.Collections.Generic.List`1[HaemophilusWeb.Models.Country]`` from `/Geonames/LoadCountries`.
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
  parameter; no application-log trace; temp-file round trip. Observed 2026-11-08 (023 verification, unit level): 10
  lookups for 10 cases that yield 9 lines (two cases of one patient); a case with sampling date 2030-01-01 is exported.
- **REF-013 – RKI match import internals** (Refactoring; source: 024 Edge Cases, US2.5, US3.7, US4.4): Lenient status
  parsing (English names, numbers, word prefixes); soft-deleted submissions matched; stored state can only be raised;
  process-wide CSV separator switched to ";" and left so; no import trace (Principle I); stale code comment. Observed on
  system 2026-11-08 (024 verification): "Probable", "3" and "mög" are accepted as status; a soft-deleted submission
  received a record; a lower state from a later upload left the stored record unchanged.
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
- **REF-021 – Meningococci report page leftovers** (Refactoring; source: 009 FR-008, FR-011): `Views/MeningoReport/Isolate.cshtml`
  still contains the Haemophilus discrepancy dialog and its "Ja" handler, but the check that opens it is disabled
  (`if (false)//… TODO`), so the dialog never appears (confirmed on system 2026-11-08, 009 verification: a discrepant
  isolate's final report was generated without a dialog); the summary's `Interpretation` branch ("TODO remove soon") is
  never taken because the Meningococci view model fills only `Report`; the page imports an unused namespace
  (`DocumentFormat.OpenXml.EMMA`); the health-office lookup is posted to the Haemophilus report controller
  (`/Report/QueryHealthOffice/`); the filename prefix is hard-coded ("TODO Replace MZ and KL with Database Prefix").
  Overlaps REF-002.

## TEST — Test gaps

- **TEST-001 – Untested functions** (Test; source: Assumptions/Edge Cases of 013, 014, 015, 016, 017, 018, 019, 020,
  021, 022, 023, 024): Sender export; both patient merges; health-office lookup, matching, storing and pages;
  breakpoint validator and pages; configuration page; deleted-records pages and restore; registration, role checks and
  navigation by role; browser behavior and both endpoints of the postal-code lookup; PubMLST update of an existing local
  record, result page and matching page; EpiScanGIS date window, "Blut und Liquor" exclusion, deleted submissions,
  missing coordinates and failures; RKI match controller, unknown status word, non-numeric cells, lab-number-only
  rows, soft-deleted submissions, all-or-nothing; county pages and upload (025; the importer, matching and exports are
  tested). Sender export: covered by tests added in the 013 verification (2026-11-08) for file name, column headers,
  selection by sampling or receiving date and row content of non-deleted submissions; the counting of soft-deleted
  submissions (DEC-002) stays untested on purpose. EpiScanGIS export: covered by tests added in the 023 verification
  (2026-11-08) for the time window away from its boundary day, soft-deleted submissions, missing coordinates, a "not
  found" lookup, file name and type, empty file, line order and temp-file cleanup; the "Blut und Liquor" exclusion, the
  boundary day and the abort cases stay untested on purpose (DEC-008, DATA-020). PubMLST update of an existing local
  record: covered by tests added in the 019 verification (2026-11-08), together with the period bounds, the save per
  found isolate, the kept link of an isolate not found, the repeated search on a later run, the single IRIS session
  and the lookup order of the controller; the result page and the matching page stay untested (their view is rendered
  only after a run against PubMLST). RKI match import: covered by tests added in the 024 verification (2026-11-08) for
  unknown status words in applied and skipped rows, the empty status, non-numeric cells, upper-case headers, a file
  without byte order mark and with CR LF, an ambiguous lookup, the use of one transaction, a lower state against a
  stored record, the export without record, and the page's success line for a zero-length file and its failure line;
  lab-number-only rows (DATA-021), the upload without a file and soft-deleted submissions (REF-013) stay untested on
  purpose; the rollback itself was observed on the system only.
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
  011 (verification 2026-11-08): the column tests of all seven exports asserted only the column count; tests of the
  exact header names and order were added, the PubMLST empty-query test still calls the Laboratory export, and the row
  selection of the export actions (sampling locations, evaluations, "Ausland", interpretation) has no unit test.
  Flaky: `Tools/HaemophilusPubMlstExportTests.DataTable_ContainsAntibioticMeasurements("CTX", Cefotaxime,
  Susceptible, …)` failed once in the full suite (found "I") and passed on rerun; `Setup` adds a random E-test, and when
  it has the same antibiotic it is found first.
- **TEST-003 – Decision-table rows derived from code only** (Test; source: 006 Decision Table note): Haemophilus
  interpretation rows 4 and 7 and the unmatched combinations have no test; the null-submission failure is untested.
  Addressed 2026-11-08 (006 verification): `Domain/IsolateInterpretationTests` now asserts rows 4 and 7, the unmatched
  combinations, the disclaimer for every negative-agglutination combination and sampling location, and the failure
  without a submission.
