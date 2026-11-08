# System test features

Gherkin features written during spec verification (`/spec-verify NNN`, see `AGENTS.md`, "Spec
verification against the running app"). One file per spec: `NNN-<spec-slug>.feature`, tagged
`@spec-NNN`, scenarios tagged with the `@US…` and `@FR-…` IDs they exercise and `@smoke` for the
primary happy path.

There are no step definitions yet. They follow after the Bootstrap 5 merge (Reqnroll on the
Playwright harness in `ScreenshotTests/`). Until then this file is the step vocabulary: reuse these
phrases so that one step definition serves every feature. When a scenario needs a step that is not
listed, add it here in the same commit.

## Conventions

- English keywords and step text; German UI strings verbatim in double quotes.
- `<…>` is a parameter; `"…"` marks a quoted UI string, address or account name.
- Test data uses the prefix `SV-NNN-` of the spec that created it.
- Steps name what the user sees (labels, button texts, page titles), never CSS selectors or ids.
- No scenarios for defects (`[SUSPECTED-DEFECT]`, backlog entries); no scenarios for rules already
  asserted by unit tests.

## Steps

### Session and navigation

| Step | Meaning |
| --- | --- |
| `Given I am signed in as "<account>"` | Sign in via `/Account/Login` with the seeded account (password `123456`). A later step of this kind signs in again as the new account. |
| `When I open "<address>"` | Navigate to an address relative to the host. |
| `Then I am on "<address>"` | The current address path equals the given one (query ignored). |
| `Then I am on the sign-in page` | The current address is `/Account/Login` (unauthorized accounts are redirected there). |
| `Then the page title is "<title>"` | The page heading (`h1`) equals the text. |

### Actions

| Step | Meaning |
| --- | --- |
| `When I click "<text>"` | Click the link or button with this visible text. |
| `When I click the button "<text>"` | Click the submit button with this text when a link has the same text. |
| `When I click "<text>" in the row "<row text>"` | Click a link or button inside the table row that contains the row text. |
| `When I enter "<value>" into "<label>"` | Fill the field with this label. |
| `When I enter "<value>" into the search field "<placeholder>"` | Fill the search field showing this placeholder (list searches without labels). |
| `When I select "<option text>" in "<label>"` | Choose the entry of the selection list with this label whose text contains the option text. |
| `When I choose "<option>" for "<label>"` | Pick the radio button or toggle with this text in the group with this label. |
| `When I open the edit page of "<name>"` | Open the edit page of the record with this name from its list. |

### Assertions

| Step | Meaning |
| --- | --- |
| `Then I see "<text>"` | The text is visible on the page. |
| `Then I see the link "<text>"` | A link with this text is visible. |
| `Then I see the validation message "<text>"` | A field validation message with this text is shown. |
| `Then I see the search field "<placeholder>"` | A search field showing this placeholder is visible (list searches without labels). |
| `Then "<label>" contains <value description>` | The field with this label holds the described value (e.g. "January 1st of the previous year"). |
| `Then a file named like "<pattern>" is downloaded` | A download starts whose file name matches the wildcard pattern. |
| `Then no file is downloaded` | No download starts within a few seconds after the preceding action. |
| `Then "<label>" offers "<option text>"` | The selection list with this label has an entry whose text contains the option text. |
| `Then "<label>" does not offer "<option text>"` | The selection list with this label has no entry whose text contains the option text. |
| `Then "<label>" offers <n> entries` | The selection list with this label has exactly this many entries, placeholder included. |
| `Then "<label>" shows "<option text>"` | The entry currently selected in the selection list with this label has exactly this text. |
| `Then the downloaded document contains "<text>"` | The text of the downloaded Word document (body, headers and footers, tags stripped) contains the text. |
| `Then the downloaded file has no column "<name>"` | The header row of the downloaded spreadsheet has no column with this name. |
| `Then the downloaded file has <n> row(s) with "<value>" in column "<name>"` | Exactly this many data rows of the downloaded spreadsheet hold the value in that column. |
| `Then the downloaded file has no data rows` | The downloaded spreadsheet contains only the header row. |

### Domain steps

| Step | Spec | Meaning |
| --- | --- | --- |
| `Given a sender "<name>" exists` | 001 | Create the sender through the UI if missing. |
| `Given a sender "<name>" without submissions exists` | 001 | As above; no submission references it. |
| `Given the seeded sender "<name>" has the submissions "<lab no>" and "<lab no>"` | 001 | Precondition on the seed; fails if not true. |
| `Then the sender list contains "<name>"` | 001 | The active sender list (`/Sender`, search box) shows the name. |
| `Then the sender list does not contain "<name>"` | 001 | As above, negated. |
| `Then the sender list row "<name>" shows "<text>"` | 001 | The row of that sender contains the text. |
| `Then the sender selection offers "<name>"` | 001 | The "Einsender" selection of a submission form lists the sender. |
| `Given a submission with sender laboratory number "<lab no>" exists` | 002 | Create a Haemophilus submission with this "Labnr. Einsender" through the UI if missing (sender `SV-001-Alpha`, synthetic patient). |
| `Given a submission for patient "<initials>" born "<date>" with postal code "<code>" exists` | 002 | As above, for a patient with these identifying details. |
| `When I open the edit page of the submission "<lab no>"` | 002 | Find the submission by its "Labnr. Einsender" in the list search and click "Bearbeiten". |
| `Then the submission list row "<search text>" shows "<text>"` | 002 | The Haemophilus submission list (`/PatientSending`), filtered with the search box, has a row containing the text. |
| `Then the submission list does not contain "<search text>"` | 002 | The search box finds no row for the text. |
| `Then the submission list shows <n> row(s)` | 002 | The list currently shows exactly this many rows. |
| `Given a Meningococci submission with sender laboratory number "<lab no>" exists` | 003 | Create a Meningococci submission with this "Labnr. Einsender" through the UI if missing (sender `SV-003-Alpha`, synthetic patient). |
| `Given a Meningococci submission for patient "<initials>" born "<date>" with postal code "<code>" exists` | 003 | As above, for a patient with these identifying details. |
| `Then the Meningococci submission list row "<search text>" shows "<text>"` | 003 | The Meningococci submission list (`/MeningoPatientSending`), filtered with the search box, has a row containing the text. |
| `Then the Meningococci submission list shows <n> row(s)` | 003 | The Meningococci list currently shows exactly this many rows. |
| `Given a Haemophilus breakpoint for "<antibiotic>" with "Sensibel <=" "<value>" and "Resistent >" "<value>" exists` | 004 | Create the breakpoint on `/EucastClinicalBreakpoints/Create` if missing (details with the prefix `SV-004-`). |
| `When I open the isolate of the submission "<lab no>"` | 004 | Find the submission by its "Labnr. Einsender" in the list search and click its "Labor-Nr" link. |
| `When I select "<value>" in the E-Test row "<antibiotic>"` | 004 | Choose the measured value in the row of this antibiotic. |
| `Then the E-Test row "<antibiotic>" shows "<result>"` | 004 | The row of this antibiotic shows this susceptibility result. |
| `Given a Meningococci breakpoint for "<antibiotic>" with "Sensibel <=" "<value>" and "Resistent >" "<value>" exists` | 005 | Create the breakpoint for Meningococci on `/EucastClinicalBreakpoints/Create` if no current one with these limits exists (details with the prefix `SV-005-`). |
| `When I open the isolate of the Meningococci submission "<lab no>"` | 005 | Find the submission by its "Labnr. Einsender" in the Meningococci list search (`/MeningoPatientSending`) and click its "Labor-Nr" link. |
| `Then the report field "<label>" shows "<text>"` | 006 | On the report page ("Befund erstellen"), the value next to this label (e.g. "Teilbefund", "Befund") equals the text. |
| `Then the submission list row "<search text>" shows the report status "<status>"` | 008 | In the Haemophilus submission list (`/PatientSending`), filtered with the search box, the "Befund" icon of the row has this title ("Kein Befund erstellt", "Teilbefund", "Endbefund"). |
| `Then the Meningococci submission list row "<search text>" shows the report status "<status>"` | 009 | As above, in the Meningococci submission list (`/MeningoPatientSending`). |
| `Given a submission with sender laboratory number "<lab no>", sampling location "<location>" and sampling date "<date>" exists` | 011 | Create a Haemophilus submission with this "Labnr. Einsender", "Entnahmeort" (for "Anderer …" also an "Anderer Entnahmeort" text with the prefix `SV-011-`) and "Entnahmedatum" through the UI if missing (sender `SV-001-Alpha`, synthetic patient). |
| `Given two submissions of patient "<initials>" born "<date>" with postal code "<code>", sampled "<date>" and received "<date>", exist` | 012 | Create two Haemophilus submissions ("Blut", isolate defaults) through intake if missing; the second one with "Bestehenden Patienten verwenden", so both belong to one patient and have identical strain characteristics. |
