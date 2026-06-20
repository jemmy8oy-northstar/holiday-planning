---
name: respond-to-pr
description: Respond to pull request review comments. Reads all comments first, makes focused commits per change, pushes immediately, and replies to every comment on GitHub.
---

Use this skill when the developer has left review comments on a PR. The developer interfaces via GitHub only — every change must be pushed so they can see it.

## Steps

### 1. Read all comments before making any changes
Read every comment on the PR before touching code. Understand the full picture first.

### 2. Assess each comment
- **Implement** — clear request, agreed upon
- **Ask for clarification** — ambiguous; reply on the comment thread before acting, don't guess
- **Push back with reasoning** — if you disagree, explain why in a comment before implementing

### 3. Make focused commits per change
Address comments in separate, focused commits where possible.
```bash
git commit -m "Address PR feedback: extract validation into separate method (#42)"
```

### 4. Push after every commit
```bash
git push
```
Do not batch everything and push once at the end. The developer follows progress via the commit history.

### 5. Reply to every comment on GitHub
After pushing the relevant commit, reply explaining what changed and why. Do not leave comments unacknowledged.

### 6. Post a summary comment when done
Once all comments are addressed, post a single PR comment summarising what was changed and flagging anything still needing a decision or that you are unsure about.

### 7. Do not re-request review or resolve conversations
Leave that to the developer.
