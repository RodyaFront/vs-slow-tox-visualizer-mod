---
name: change-freeze-flow
description: Run the project's strict update-freeze workflow for mod changes: verify latest public ModDB baseline, update changelogs, commit on a feature branch, push, and deliver a structured freeze report. Use when user asks to "fix and freeze", "commit and push", "prepare release notes", or "finalize update".
disable-model-invocation: true
---

# Change Freeze Flow

Use this skill to execute the repository's standard "freeze new changes" process end-to-end.

## Preconditions

- Read and follow `.cursor/rules/change-freeze-flow.mdc`.
- Confirm you are on a feature branch (never freeze functional work on `main`).
- If branch name does not reflect the implemented functionality, ask user before commit.

## Execution Checklist

Copy this checklist and keep it updated while working:

```text
Change Freeze Progress
- [ ] 1) Validate branch and scope
- [ ] 2) Verify latest public ModDB release baseline (manual)
- [ ] 3) Update CHANGELOG.md entries for user-visible changes
- [ ] 4) Generate/refresh changelog HTML if release flow requires it
- [ ] 5) Review git status/diff and stage only task-related files
- [ ] 6) Commit with clear why-focused message
- [ ] 7) Push branch to remote
- [ ] 8) Send mandatory freeze report to user
```

## Step-by-Step Rules

### 1) Validate branch and scope

- Ensure current branch is a feature/fix branch.
- Confirm the scope of files being frozen matches the user request.

### 2) Verify latest public ModDB baseline (manual)

Before release-related freeze:

- Identify the latest public version on ModDB.
- Capture and preserve:
  - release URL,
  - release date,
  - short summary of that release changelog.
- If baseline is not verified in the current session, stop and ask user before continuing.

### 3) Update markdown changelog first

- Update relevant `CHANGELOG.md` section(s) with:
  - what changed for users,
  - why this update exists (impact/intent).
- Keep markdown changelog as source of truth.

### 4) HTML changelog handling

- Generate or refresh `changelog-*.html` only when release flow requires it.
- Respect `.gitignore` rules for generated HTML files.

### 5) Freeze in git

- Run git checks (`status`, `diff`, `log`) before commit.
- Stage only related files.
- Commit using repository message style.
- Push branch to remote.

### 6) Mandatory freeze report

After push, report to user:

- branch name and commit hash,
- summary of frozen changes,
- which `CHANGELOG.md` files/sections were updated,
- whether HTML changelog files were committed or intentionally ignored.

## Hard Stop Conditions

- Public ModDB baseline not verified for this session (for release-related freeze).
- Changelog is missing or stale for user-visible changes.
- Branch naming/scope is ambiguous and user has not confirmed intent.
