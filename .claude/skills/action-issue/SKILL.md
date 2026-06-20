---
name: action-issue
description: Pick up and implement a GitHub issue. Creates a branch prefixed with the issue number, implements with TDD, pushes commits frequently, opens a PR targeting dev, and uses the PR for all communication.
---

Use this skill when implementing a GitHub issue. The developer interfaces via GitHub only — all progress must be visible via commits and PRs on the remote.

## Steps

### 1. Read the full issue before starting
Read the issue body AND all comments. The comments contain the negotiated spec. Do not start until everything is read.

### 2. Create a branch off dev
```bash
git checkout dev && git pull
git checkout -b {issue-number}-{short-kebab-description}
```

### 3. Implement with TDD
Follow the top-down TDD flow in `docs/specs/testing-strategy.md`. Write tests before implementation. Commit after each meaningful unit of work — do not save everything for one large commit.

### 4. Push frequently
Push after every meaningful commit. The developer cannot see work until it is on the remote.
```bash
git push -u origin {issue-number}-{description}
```

### 5. Open a PR targeting dev
```bash
gh pr create --base dev --title "..." --body "..."
```
PR body must include:
- `Closes #N` to auto-close the issue on merge
- Summary of what was implemented
- Any decisions made that weren't explicitly covered by the spec
- Anything the reviewer needs to know to assess the work without running it locally

### 6. Use the PR for all communication
Post questions, concerns, and trade-off decisions as PR comments — not only in a chat reply. The PR is the record.

### 7. Do not merge your own PR
Leave it open for the developer to review.
