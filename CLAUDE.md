# Claude Code notes

@AGENTS.md

Claude-specific details on top of the shared guide:

- Spec Kit skills are installed under `.claude/skills/speckit-*/SKILL.md`; invoke them as
  `/speckit-specify`, `/speckit-clarify`, `/speckit-plan`, `/speckit-tasks`, `/speckit-analyze`,
  `/speckit-implement`. They call the PowerShell scripts in `.specify/scripts/powershell/`.
- `/speckit-specify` must run in a clean context. To create several specs in one session, start
  one `general-purpose` subagent per spec, sequentially, with the full `Feature title: …` prompt
  and the conventions from `AGENTS.md`; the main session verifies and commits each result.
- `/speckit-constitution` amends `.specify/memory/constitution.md` only with explicit owner
  approval; bump the version and update the Sync Impact Report.
- Reply to the owner in German; write every artifact in English (see "Language" in `AGENTS.md`).
- Lint documentation before committing: `npx -y markdownlint-cli "specs/**/*.md"`. The IDE's
  markdownlint extension and the CLI share `.markdownlint.json`.
- Use the scratchpad directory for temporary scripts and notes, never the repository.
