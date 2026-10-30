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
| `When I open the edit page of "<name>"` | Open the edit page of the record with this name from its list. |

### Assertions

| Step | Meaning |
| --- | --- |
| `Then I see "<text>"` | The text is visible on the page. |
| `Then I see the link "<text>"` | A link with this text is visible. |
| `Then I see the validation message "<text>"` | A field validation message with this text is shown. |
| `Then "<label>" contains <value description>` | The field with this label holds the described value (e.g. "January 1st of the previous year"). |
| `Then a file named like "<pattern>" is downloaded` | A download starts whose file name matches the wildcard pattern. |

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
