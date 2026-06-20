# AI-Assisted Project Workflow

This document describes the end-to-end workflow for projects bootstrapped from this template. It covers the full lifecycle from repository creation through to MVP launch, and defines conventions Claude follows throughout.

---

## Phase 0 — Bootstrap

> No GitHub issues. These are one-off actions performed once when the project is created.

1. Developer creates the GitHub repo
2. Install the Claude GitHub App and add `CLAUDE_CODE_OAUTH_TOKEN` to repository secrets
3. Run `node scripts/init-issues.mjs` to scaffold all initial SDD issues
4. Work through Phase 1 issues to configure the project

---

## Initial Issues

These are created automatically by `init-issues.mjs`. Each is an orchestrator issue — it either requires a direct developer action or triggers Claude to produce a spec/PR that kicks off the next phase.

| ID   | Title                                                             |
|------|-------------------------------------------------------------------|
| [1a] | Set up pipeline secrets and variables                             |
| [1b] | Set up branch policies                                            |
| [1c] | Define high-level project spec, vision and external dependencies  |
| [2a] | Generate UI/UX design issues from approved spec                   |
| [3a] | Frontend tech decisions + user story spec                         |
| [3b] | Create frontend issues + backend skeleton                         |
| [5a] | Create backend design spec                                        |
| [5b] | Generate backend user stories from approved backend design        |
| [5c] | Create backend GitHub issues from user story spec                 |
| [6a] | Hook up Postgres DB (if required)                                 |
| [7a] | Share MVP — publish YouTube walkthrough and gather user feedback  |
| [7b] | Create production refinement tickets *(confirm developer interest before actioning)* |

> **Phase 4 has no initial issues** — it consists entirely of implementation issues created by [3b].
> **Phase 6 has no initial issues** — it consists entirely of implementation issues created by [5c], plus [6a] if Postgres is required.

---

## Issue Numbering Convention

### Orchestrator / initial issues — `[Na]`, `[Nb]`, `[Nc]`
A letter suffix identifies an orchestrator issue. These are the initial issues created at bootstrap that trigger or coordinate a phase.

- `[1a]` — first orchestrator issue in Phase 1
- `[2a]` — first orchestrator issue in Phase 2
- `[5b]` — second orchestrator issue in Phase 5

### Implementation issues — `[N]`
Issues created dynamically *as a result of* an orchestrator issue carry only the phase number (no letter). These are the individual units of work Claude picks up and implements.

- `[2] Auth page design` — a UI/UX design issue created by `[2a]`
- `[4] Auth page` — a frontend implementation issue created by `[3b]`
- `[6] Auth endpoint` — a backend implementation issue created by `[5c]`

---

## Full Workflow

### Phase 1 — Project Setup

| Issue | Action |
|-------|--------|
| [1a] | Developer configures CI/CD pipeline secrets and variables → closed |
| [1b] | Developer configures branch protection rules → closed |
| [1c] | Developer fills in the spec questionnaire in the issue body, then comments `@claude [1c] is ready — please raise the spec PR`. Claude raises a **spec PR** containing the project vision, epics, features, and external dependencies. Developer reviews and merges → closed |

**[1c] Spec questionnaire — Claude expects answers to:**
- What problem does this product solve, and for whom?
- What does a successful MVP look like?
- Is Postgres required? If so, any schema/domain hints?
- Are there any external data sources, APIs, or third-party integrations?
- Any auth providers, payment gateways, or other dependencies?
- What is explicitly out of scope for the MVP?

---

### Phase 2 — UI/UX Design

| Issue | Action |
|-------|--------|
| [2a] | Once [1c] is merged, comment `@claude [2a] is ready — please create the design issues`. Claude scans `docs/features/` and creates one `[2] Feature name design` issue per frontend feature → [2a] closed |
| [2] *per feature* | Comment `@claude implement this design issue` on each. Claude raises a **design PR** with ASCII mockups and Mermaid workflow diagrams. Developer reviews → merges or requests changes → issue closed on merge |

**[2] issue structure — what Claude puts in each design issue body:**

```
Design the <feature name> for <product name>.

**Feature:** `docs/features/<feature-file>.md`

**Open UX questions to resolve:**
- <question from feature file>

**Deliverables (in the design PR):**
- [ ] ASCII mockup for each meaningful page/component state
- [ ] ASCII mockup for each key interaction state (loading, error, empty)
- [ ] Mermaid workflow diagram for each key user action
- [ ] All open UX questions answered
```

---

### Phase 3 — Frontend User Stories & Issues

> **BDD (Behaviour-Driven Development)** — requirements written as human-readable scenarios: *who* wants to do *what* and *why*, with concrete acceptance criteria defining when the story is done.

| Issue | Action |
|-------|--------|
| [3a] | Once all `[2]` issues are closed, comment `@claude [3a] is ready — please raise the frontend tech decisions and user story spec PR`. Claude raises a **spec PR** with: `docs/tech-decisions-frontend.md` (library choices) and `docs/user-stories-frontend.md` (BDD stories + API skeleton contracts). Developer reviews → merges → closed |
| [3b] | Once [3a] is merged, comment `@claude [3b] is ready`. Claude **(1)** creates `[4]` frontend implementation issues and **(2)** raises a **backend skeleton PR** with Bogus-generated endpoints matching the API contracts → closed |

**[3a] spec PR — what Claude produces:**
- `docs/tech-decisions-frontend.md` — library choices (UI component lib, chart lib, date handling, other deps)
- `docs/user-stories-frontend.md` — BDD stories with acceptance criteria + API skeleton contracts

---

### Phase 4 — Frontend Implementation

No initial issues. All work is driven by `[4]` issues created by [3b].

| Issue | Action |
|-------|--------|
| [4] *per story* | Comment `@claude implement this issue` on each. Claude writes Vitest + RTL tests first, then implements the component/feature. Raises a **feature PR** with `Closes #N`. Developer reviews → merges → issue auto-closes |

---

### Phase 5 — Backend Design & Stories

| Issue | Action |
|-------|--------|
| [5a] | Once all `[4]` issues are closed, comment `@claude [5a] is ready — please raise the backend design spec`. Claude raises a **backend design PR** covering data models, API contracts, EF Core entities, service architecture, and ER diagram in Mermaid → closed |
| [5b] | Once [5a] is merged, comment `@claude [5b] is ready`. Claude raises a **backend user story spec PR** → closed |
| [5c] | Once [5b] is merged, comment `@claude [5c] is ready`. Claude creates individual `[6]` backend implementation issues → closed |

---

### Phase 6 — Backend Implementation

| Issue | Action |
|-------|--------|
| [6a] | Once all `[6]` issues are closed, comment `@claude [6a] is ready`. Claude wires up Postgres — schema, EF Core migrations, connection config — and raises a PR. Skipped if Postgres was marked not required in [1c] → closed |
| [6] *per story* | Comment `@claude implement this issue` on each. Claude implements TDD (unit tests first), raises a **feature PR** with `Closes #N`. Developer reviews → merges → issue auto-closes |

---

### Phase 7 — MVP 🎉

| Issue | Action |
|-------|--------|
| [7a] | Comment `@claude [7a] is ready`. Claude drafts a YouTube walkthrough script as `docs/mvp-walkthrough-script.md` via a PR. Developer records and publishes |
| [7b] | Comment `@claude [7b] is ready — please generate refinement tickets`. Claude audits the codebase and raises targeted issues for performance, security, and test coverage gaps. ⚠️ **Only action when explicitly requested** |

---

## Branch Strategy

| Branch | Purpose |
|---|---|
| `main` | Production-ready code only — merged from `dev` by the **developer** when a milestone is complete |
| `dev` | Integration branch — **all feature/spec/docs PRs target `dev`** |
| `feat/*`, `fix/*`, `spec/*`, `docs/*` | Short-lived work branches — always branch from `dev`, always PR back to `dev` |

**Never PR directly to `main`.** Claude always sets `dev` as the base branch when raising PRs.

**`dev` → `main` is a human-only action.** Claude never raises a PR targeting `main` and never merges `dev` into `main`.

---

## PR ↔ Issue Linking

When Claude raises a PR for an issue, it does three things:

1. **`Closes #N` in the PR body** — GitHub automatically closes the issue when the PR is merged.
2. **Comment on the issue** — Claude posts: *🤖 PR raised: #N — please review when ready.*
3. **Assign the PR to the repo owner** — every PR is assigned so it appears in the developer's assigned PRs list.

---

## Milestone

All initial issues are assigned to the **MVP** milestone. This provides a progress view across all phases at a glance.
