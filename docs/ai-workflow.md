# AI-Assisted Project Workflow

This document describes the end-to-end workflow for projects bootstrapped from this template. It covers the full lifecycle from repository creation through to MVP launch, and defines conventions the AI agent follows throughout.

---

## Phase 0 — Bootstrap

> No GitHub issues. These are one-off actions performed once when the project is created.

1. Developer creates the GitHub repo and grants the bot access
2. AI scaffolds the project from this template and pushes the initial commit
3. AI creates all **initial issues** (see table below) with the `MVP` milestone
4. Developer works through Phase 1 issues to configure the project

---

## Initial Issues

These are created automatically at bootstrap. Each is an orchestrator issue — it either requires a direct action (e.g. configure secrets) or it triggers the AI to produce a spec/PR that kicks off the next phase.

| ID   | Title                                                             |
|------|-------------------------------------------------------------------|
| [1a] | Set up pipeline secrets and variables                             |
| [1b] | Set up branch policies                                            |
| [1c] | Define high-level project spec, vision and external dependencies  |
| [2a] | Generate UI/UX design issues from approved spec                   |
| [3a] | Generate frontend user stories from approved UI/UX designs        |
| [3b] | Create frontend GitHub issues from user story spec                |
| [5a] | Create backend design spec                                        |
| [5b] | Generate backend user stories from approved backend design        |
| [5c] | Create backend GitHub issues from user story spec                 |
| [6a] | Hook up Postgres DB (if required)                                 |
| [7a] | Share MVP — publish YouTube walkthrough and gather user feedback  |
| [7b] | Create production refinement tickets *(confirm user interest before actioning)* |

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
Issues created dynamically *as a result of* an orchestrator issue carry only the phase number (no letter). These are the individual units of work the AI picks up and implements.

- `[2] Auth page design` — a UI/UX design issue created by `[2a]`
- `[4] Auth page` — a frontend implementation issue created by `[3b]`
- `[6] Auth endpoint` — a backend implementation issue created by `[5c]`

This distinction makes it immediately clear whether an issue is orchestrating a phase or implementing a piece of work within it.

---

## Full Workflow

### Phase 1 — Project Setup

| Issue | Action |
|-------|--------|
| [1a] | Developer (or AI where possible) configures CI/CD pipeline secrets and variables in repository settings → closed |
| [1b] | Developer configures branch protection rules for `main` and `dev` → closed |
| [1c] | Developer fills in the spec questionnaire (see below), then adds the `waiting-for-ai` label. AI raises a **spec PR** containing the project vision, epics, features, and external dependencies. If anything is ambiguous, the AI removes `waiting-for-ai` and leaves a PR comment. Developer answers, re-applies `waiting-for-ai` to iterate, then reviews and merges → closed |

**[1c] Spec questionnaire — the AI expects answers to:**
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
| [2a] | Triggered once [1c] is merged. AI scans the spec for features, creates one `[2] Feature name design` issue per feature. → [2a] closed |
| [2] *per feature* | AI raises a **design PR** with ASCII mockups and Mermaid workflow diagrams. Developer reviews → merges or requests changes → issue closed on merge |

**[2] issue structure — what the AI puts in each design issue body:**

```
Design the <feature name> for <product name>.

**Feature:** `docs/features/<feature-file>.md`

**Open UX questions to resolve:**
- <question from feature file>
- <question from feature file>

**Deliverables (in the design PR):**
- [ ] ASCII mockup for each meaningful page/component state
- [ ] ASCII mockup for each key interaction state (loading, error, empty)
- [ ] Mermaid workflow diagram for each key user action
- [ ] All open UX questions answered
```

**[2] design PR — what the AI produces:**
- One ASCII mockup per page state and interaction state
- One Mermaid sequence/flowchart diagram per key user action (shows what the system does, what data flows, what side effects occur)
- Answers to all open UX questions listed in the issue
- Sign-off checklist in the PR body for the developer to review before merge

---

### Phase 3 — Frontend User Stories & Issues

> **BDD (Behaviour-Driven Development)** — a format for writing requirements as human-readable scenarios. Each story follows the pattern: *who* wants to do *what* and *why*, with concrete acceptance criteria that define when the story is done.

| Issue | Action |
|-------|--------|
| [3a] | Triggered once all `[2]` issues are closed. AI raises a **spec PR** with three parts: **(A)** `docs/tech-decisions-frontend.md` — proposed library choices with rationale; **(B)** `docs/user-stories-frontend.md` — BDD stories derived from the signed-off designs; **(C)** API skeleton — endpoint contracts, response shapes, and RTK Query hooks table. Human review gate — developer must approve before merge → closed |
| [3b] | Triggered once [3a] is merged. AI does two things: **(1)** creates individual `[4] Feature name` frontend implementation issues (closely related stories may be grouped into one issue); **(2)** raises a **backend skeleton PR** implementing the API contracts from [3a] using `Bogus` (Faker for .NET) — real HTTP endpoints, no service layer, deterministic seeded data. → closed |

**[3a] spec PR — what the AI produces:**
- `docs/tech-decisions-frontend.md` — library choices (UI component lib, chart lib, date handling, other deps)
- `docs/user-stories-frontend.md` — BDD stories with acceptance criteria referencing the chosen libraries
- API skeleton section in the stories doc: endpoint contracts (path, params, response shapes), RTK Query hooks table

**[3a] tech decisions — the AI proposes choices for:**
- UI component library (e.g. shadcn/ui, MUI, Mantine, Radix UI, or none)
- Chart / visualisation library (e.g. Recharts, Chart.js, Nivo, Visx, D3)
- Date handling (e.g. date-fns, dayjs, native `Intl`/Temporal)
- Any other notable runtime dependencies specific to the project

---

### Phase 4 — Frontend Implementation

No initial issues. All work is driven by `[4]` issues created by [3b]. The backend skeleton (Faker endpoints) is already running — frontend makes real HTTP calls throughout.

| Issue | Action |
|-------|--------|
| [4] *per story* | AI writes Vitest + RTL tests first (AC items → test cases), then implements the component/feature to make them pass. Raises a **feature PR** with `Closes #N` in the body and a comment on the issue. Developer reviews → merges → issue auto-closes |

---

### Phase 5 — Backend Design & Stories

| Issue | Action |
|-------|--------|
| [5a] | Triggered once all `[4]` issues are closed. AI raises a **backend design PR** covering data models, API contracts, EF Core entities, service architecture, and ER diagram in Mermaid. Developer reviews → merges → closed |
| [5b] | Triggered once [5a] is merged. AI raises a **backend user story spec PR**. Human review gate → merge → closed |
| [5c] | Triggered once [5b] is merged. AI creates individual `[5] Feature name` backend implementation issues → closed |

---

### Phase 6 — Backend Implementation

| Issue | Action |
|-------|--------|
| [6a] | Triggered once all `[5]` issues are closed. AI wires up Postgres — schema, EF Core migrations, connection config — and raises a PR. Skipped if Postgres was marked not required in [1c] → closed |
| [6] *per story* | AI implements the backend story (TDD: unit tests first), raises a **feature PR** with `Closes #N` and a comment on the issue. Developer reviews → merges → issue auto-closes |

---

### Phase 7 — MVP 🎉

Formal phases end here. Post-MVP work is informal and developer-led.

| Issue | Action |
|-------|--------|
| [7a] | AI drafts a YouTube script and walthrough outline for the MVP. Developer records and publishes |
| [7b] | AI generates production refinement tickets (performance, security, scaling, testing gaps). ⚠️ **Confirm user interest before actioning these** |

---

## Branch Strategy

| Branch | Purpose |
|---|---|
| `main` | Production-ready code only — merged from `dev` by the **human developer** when a milestone is complete |
| `dev` | Integration branch — **all feature/spec/docs PRs target `dev`** |
| `feat/*`, `fix/*`, `spec/*`, `docs/*` | Short-lived work branches — always branch from `dev`, always PR back to `dev` |

**Never PR directly to `main`.** The AI always sets `dev` as the base branch when raising PRs.

**`dev` → `main` is a human-only action.** The AI never raises a PR targeting `main` and never merges `dev` into `main`. This is a deliberate gate — the developer decides when a milestone is production-ready.

---

## PR ↔ Issue Linking

When the AI raises a PR for an issue, it does three things:

1. **`Closes #N` in the PR body** — GitHub automatically closes the issue when the PR is merged and shows the link in both the PR and issue sidebars.
2. **Comment on the issue** — The AI posts a comment on the issue itself:
   > 🤖 PR raised: #42 — please review when ready.
3. **Assign the PR to the repo owner** — Every PR is assigned to `the repo owner` so it appears in the developer's assigned PRs list and is easy to find.

This means anyone watching the issue gets notified and can navigate to the PR without searching for it.

---

## Agent Conventions

### Label-driven turns

Labels signal whose turn it is to act. The AI monitors for `waiting-for-ai` on both issues and PRs.

**On issues:**
- Developer adds `waiting-for-ai` → AI picks up, responds or raises a PR, then removes `waiting-for-ai` (no replacement label needed — no label = human's turn)
- Developer adds `action-ready` → AI implements without discussion, raises a PR, removes `action-ready`
- If the AI has a blocking question when it sees `action-ready`, it removes `action-ready` and posts the question as a comment. Developer answers and re-applies `action-ready` to proceed.

**On PRs:**
- Developer leaves a review and adds `waiting-for-ai` → AI addresses comments, pushes, removes `waiting-for-ai`
- No `action-ready` on PRs — a PR is already an implementation artefact

**No labels = idle/backlog.** An issue without labels is simply waiting — neither the AI nor the developer is blocked.

### "Iterate" shorthand

When the developer sends `iterate`, `iterate issue`, or `iterate pr`, the AI will:

1. Fetch the most recently active open issue or PR
2. Read all new comments/activity since the last response
3. Reply or act accordingly

This is a lightweight text-based trigger for use alongside the label system.

---

## Labels

| Label | Usage |
|-------|-------|
| `waiting-for-ai` | AI's turn — discussion, spec iteration, or PR review iteration |
| `action-ready` | Issue approved for implementation — AI should start coding |
| `ai-error` | Claude hit an error — human needs to review and re-trigger |

Labels apply to both issues and PRs. **No label = human's turn / idle/backlog** — no explicit label is needed to signal this.

---

## Milestone

All initial issues are assigned to the **MVP** milestone. This provides a progress view across all phases at a glance.
