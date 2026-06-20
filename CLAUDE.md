# CLAUDE.md — web-template

A `dotnet new` monorepo template for .NET 10 + React 19 projects. Every new project you start should be scaffolded from this template.

## Tech Stack

| Layer | Tech |
|---|---|
| Backend | .NET 10, ASP.NET Core Minimal APIs, EF Core + PostgreSQL, AutoMapper, Scalar/OpenAPI |
| Frontend | React 19, TypeScript, Vite (Rolldown), Redux Toolkit + RTK Query |
| Infrastructure | Docker (multi-stage), Helm, Kubernetes (OCI) |

## Repository Structure

```
/
├── backend/                       # .NET solution — 7 projects, Clean Architecture
│   ├── HolidayPlanning.WebApi/       # Entry point: routes, DI, OpenAPI
│   ├── HolidayPlanning.Services/     # Business logic, AutoMapper profiles
│   ├── HolidayPlanning.Abstractions/ # Service interfaces + model interfaces
│   ├── HolidayPlanning.Database/     # EF Core DbContext + migrations
│   ├── HolidayPlanning.EntityModels/ # Database entities (anemic POCOs)
│   ├── HolidayPlanning.DomainModels/ # Rich business-layer objects
│   └── HolidayPlanning.DataModels/   # Request/Response DTOs
├── frontend/                      # React + TypeScript Vite app
│   └── src/
│       ├── api/                   # RTK Query: emptyApi, generatedApi (codegen), custom endpoints
│       ├── components/            # Reusable UI components (each with co-located .scss)
│       ├── context/               # React context for UI state
│       ├── data/                  # config.json and static JSON
│       ├── pages/                 # Route-level page components
│       ├── store/                 # Redux store
│       └── styles/                # Global SCSS: design tokens, resets, utilities
├── scripts/init.mjs               # First-run onboarding: generates appsettings + .env
├── helm/                          # Helm chart for Kubernetes deployment
├── deploy.sh                      # Local CD: build, push images, restart K8s deployments
├── docs/specs/                    # Architecture decisions and how-to guides
└── .agents/                       # AI rules and workflows
```

## Key Docs

| Doc | What it covers |
|---|---|
| `docs/specs/backend-architecture.md` | 7-project Clean Architecture layer structure |
| `docs/specs/backend-srp.md` | SRP, orchestrator pattern, service naming |
| `docs/specs/openapi-codegen.md` | OpenAPI → RTK Query codegen workflow |
| `docs/specs/testing-strategy.md` | Unit tests, in-process integration tests, Vitest |
| `docs/specs/sdd-workflow.md` | 7-phase Spec Driven Development process |
| `.agents/rules/project.md` | Coding conventions, branch strategy, GitHub workflow rules |
| `.claude/skills/action-issue/SKILL.md` | How to pick up and implement a GH issue |
| `.claude/skills/respond-to-pr/SKILL.md` | How to respond to PR review comments |
| `.claude/skills/pr-readiness/SKILL.md` | Pre-PR checklist — naming, AutoMapper, typed results, architecture |

## Backend Architecture Quick Reference

> Full detail in `docs/specs/backend-architecture.md`. These are the rules needed most often.

### Model Naming

| Type | Naming | Project | Rule |
|---|---|---|---|
| Data model | Plain noun | `DataModels/` | What the API returns; implements `I*` from Abstractions |
| Domain model | `Domain*` | `DomainModels/` | Extends its DataModels counterpart (`DomainRatio : Ratio`) |
| Entity | `*Entity` | `EntityModels/` | EF Core mapping only |
| Inbound DTO | `*Request` | `DataModels/` | Only for non-trivial POST/PUT bodies — never for query params |
| Custom response | `*Response` | `DataModels/` | Only when the route assembles a shape the service does NOT directly return |

### Key Rules
- **Abstractions** — interfaces only; zero concrete types; no project references to concrete projects
- **Service interfaces** — always return interfaces (`IRatio`), never concrete types
- **Route handlers** — named static methods; typed results (`Ok<T>` / `Results<T1,T2>`); map via `IMapper`; no business logic
- **AutoMapper** — `WebApi/Mapper.cs` for Domain→Data (API boundary); `Services/Mapper.cs` for Entity→Domain (DB boundary)
- **Always run `.claude/skills/pr-readiness/SKILL.md` before opening a PR**

## Running Locally

```bash
# Backend (Terminal 1)
cd backend && dotnet run --project HolidayPlanning.WebApi
# API: http://localhost:5000  |  Docs: http://localhost:5000/scalar/v1

# Frontend (Terminal 2)
cd frontend && npm run dev
# App: http://localhost:5173

# Regenerate API client after backend changes
cd frontend && npm run codegen
```

## First-Time Setup

```bash
node scripts/init.mjs   # generates appsettings.Development.json + frontend/.env
cd backend && dotnet ef database update \
  --project HolidayPlanning.Database \
  --startup-project HolidayPlanning.WebApi
```

## Deployment

```bash
./deploy.sh   # builds ARM64 images, pushes to OCIR, rolling restart in K8s
```

CI runs on PRs only (`ci.yml`). Image builds are manual (`docker-build-push.yml`, `workflow_dispatch`).

## GitHub Conventions

- **Branch target**: All PRs target `dev`. Never target `main` — `dev` → `main` is a human-only action.
- **PR assignment**: Every PR the AI raises must be assigned to `the repo owner`.
- **Issue assignment**: Every issue the AI acts on must be assigned to `the repo owner`.
- **Issue linking**: Every PR body must include `Closes #N`. The AI also comments on the issue: *🤖 PR raised: #N — please review when ready.*
- **Labels**: After completing work, remove `waiting-for-ai` or `action-ready` — do NOT apply `waiting-for-human`. Anything without a trigger label is implicitly the human's turn. If blocked, remove the trigger label and post a comment.

## Notification Rule (mandatory)
Always assign the repository owner to every issue and PR:
- `gh issue create ... --assignee $(gh repo view --json owner --jq .owner.login)`
- `gh pr create   ... --assignee $(gh repo view --json owner --jq .owner.login)`
- Finding owner: Use `gh repo view --json owner --jq .owner.login`. Cache it — don't call multiple times.
- Verification: After creating issues/PRs, run `gh issue view <N> --json assignees` to confirm.

## Assumptions & Decisions (mandatory for all AI-raised PRs)

Every PR the AI raises must include a populated "Assumptions & Decisions" section in the PR body:

```markdown
## Assumptions & Decisions

> Any assumption made during implementation that was not explicitly specified in the issue or spec.
> If you disagree with any item, comment and re-apply `action-ready` — the AI will revise.

| # | Assumption | Rationale | If incorrect... |
|---|---|---|---|
| 1 | | | |
```

Rules:
- Minimum one row. Use "N/A — no ambiguous assumptions" only if genuinely nothing was assumed.
- Always surface: branch target choice, external API/series ID choices, CSS/styling strategy, database decisions.
- Never leave the table blank.

## Agent Conventions

### Phase Guard Status Format

When triggered on an issue, check phase dependencies and post a concise status block:

````markdown
## 🔍 Phase Check

| Check | Status |
|---|---|
| Phase N dependencies | ⏳ #32, #34 still open — waiting |
| Key decision from [1c] | ✅ "No DB for MVP" — EF Core items skipped |
| External data sources | ⚠️ CAPE source unverified — flagging in spec |

> Proceeding once #32 and #34 close. Will post one update when they do.
````

Rules:
- Post this table **instead of** a verbose paragraph analysis
- Maximum 3–5 rows — only include the checks that are relevant to this issue
- ✅ = confirmed / met, ⏳ = pending / waiting, ⚠️ = flagged / needs attention
- If all checks pass: proceed directly to implementation without posting the table

### Dependency Check Responses

When an issue is triggered before its dependencies are met, respond with a **single short comment only**:

> ⏳ Dependencies not yet met: [list open blocking issues as `#N`]. Will proceed once they close.

Rules:
- Do NOT repeat the full issue analysis
- Do NOT re-list acceptance criteria
- Do NOT re-summarise the issue body
- Post **one comment per trigger maximum** — do not post multiple comments in a single run
- If you have already posted a dependency-waiting comment and nothing has changed, do not post again

### Multi-pass Issue Behaviour

Before posting any comment on an issue, read ALL existing comments in full.

- If you have already asked clarifying questions and the owner has answered them: proceed with the implementation using those answers. Do not re-ask.
- If you have already proposed a plan or analysis: do not repeat it — continue from where you left off.
- If a previous pass left a structured summary comment (see "Structured Pass Summary Format" below): read that summary first and skip re-reading files already listed there.

When a task is **partially complete** (ran out of time, blocking question posted):
1. Post a clearly labelled partial pass comment: `## Pass N — Partial`
2. List what was completed and what remains
3. If you asked a blocking question: remove `action-ready`, wait for developer's answer
4. If partial due to scope/time (no blocking question): re-apply `action-ready` yourself (see Self-relabelling below)

At the end of every partial pass, state explicitly: **"Re-apply `action-ready` (not `waiting-for-ai`) to continue implementation."**

When a task is **complete** (PR raised):
1. Post: `🤖 PR raised: #N — please review when ready.`
2. Remove `action-ready` — the ball is in the developer's court

### Self-relabelling on Partial Completion

When a task is **partially complete** due to scope/time (not a blocking question):

```bash
# Re-apply action-ready so the next pass fires without developer intervention
gh issue edit $ISSUE_NUMBER --add-label "action-ready"
gh issue comment $ISSUE_NUMBER --body "⚡ Pass N complete (partial). Re-labelled for next pass. Remaining: [X, Y]"
```

Rules:
- **Do** re-apply `action-ready` when: implementation is in progress but not finished (no PR raised yet)
- **Do NOT** re-apply `action-ready` when: you have asked a blocking question and are waiting for the developer's answer
- When re-applying, always post a comment explaining what was completed and what remains

Iteration limit hints (for operator — not enforced here):
- Orchestrator issues (`[1c]`, `[3a]`, `[5a]`): expect ~2–4 passes. Suggest `max_turns=80` for these.
- Implementation issues (`[4]`, `[6]`): should complete in one pass. Suggest `max_turns=40`.

### Structured Pass Summary Format

At the end of every AI pass (complete or partial), post a structured summary comment using this exact format:

````markdown
## AI Pass N Summary — YYYY-MM-DD

**Status:** [completed / partial / blocked]

**Completed this pass:**
- [description] ✅

**Not completed:**
- [description — be specific about what remains]

**Key decisions made:**
- [decision]: [rationale]

**Files to skip re-reading next pass (already processed):**
- `path/to/file.md` — [one-line summary of what it contains]

**Next trigger:** [what the developer needs to do — e.g. "Re-apply `action-ready` to continue", "Answer question about X"]
````

On the **next trigger**, read this summary comment **first** before reading any files listed in "Files to skip re-reading next pass". This eliminates redundant file reads and maintains continuity across passes.

## Label Modes

| Label | Effect | When to apply |
|---|---|---|
| `waiting-for-ai` | Bot enters **discussion mode** — answers questions, proposes plans. **Will NOT write code or raise a PR.** | New issues, Q&A rounds, requesting analysis or a plan |
| `action-ready` | Bot enters **implementation mode** — writes code, runs tests, raises a PR. | After reviewing a plan and wanting implementation to begin or continue |

When finishing a partial pass, always state explicitly:
> "Re-apply `action-ready` (not `waiting-for-ai`) to continue implementation."

This is mandatory — the developer has no other way to know which label triggers implementation.

## Clarification Protocol

When implementation requires information not provided in the issue:

**Step 1: Can I make a sensible assumption?**
- Yes → implement with the assumption, document it in the PR's "Assumptions & Decisions" table. The developer reviews and overrides if needed. **Do not block.**

**Step 2: Is this a major architectural decision with no sensible default?**
- Yes → post a single comment with the question + a recommended default. Remove `action-ready`. Do not start implementation until answered.
- No → treat as Step 1 (assume + document).

**Anti-patterns to avoid:**
- ❌ Asking the same question more than once
- ❌ Re-listing full dependency analysis when the open-item count hasn't changed
- ❌ Blocking on minor/stylistic choices (library patch version, variable names, etc.)
- ❌ Posting multiple clarifying questions across separate comments — batch them into one

---

## AI Behaviour Mode

At the start of each project, check `docs/specs/proposal.md` (from [1e]) for the AI Behaviour Preference set in [1d]:

- **Assume & Document:** Implement directly. Surface all non-obvious choices in the PR's Assumptions & Decisions table.
- **Ask First:** Post clarifying questions before starting implementation. Wait for answers.
- **Mixed (default if not specified):** Assume minor/stylistic choices. Ask only for major architectural decisions with no sensible default.

---

## [1e] Formal Proposal Template

When the AI opens a `[1e]` issue from a [1d] discussion, use this structure:

```
## User Workflows

### [Workflow name]
- User can [action]
- User can [action]

## External Dependencies

| Dependency | Purpose | Notes |
|------------|---------|-------|

## Required API Endpoints

| Method | Path | Purpose |
|--------|------|---------|

## Open decisions
- [decision] — Proposal: [recommendation]. Rationale: [reason]. ✅ Unless you disagree?
```

---

## Issue Factories ([3b] and [5c])

Issue factories are AI passes that create multiple downstream issues programmatically. Follow these rules for all factory runs:

### [3b] — Creates [4] frontend implementation issues

Each [4] issue must include:
- [ ] A single, clearly scoped frontend feature or component
- [ ] Acceptance criteria referencing the BDD story from [3a] user stories
- [ ] A Testing section (mandatory AC) — see Testing Standards
- [ ] A Visual Evidence section (mandatory AC) — see Frontend PR Screenshots
- [ ] `--assignee $(gh repo view --json owner --jq .owner.login)` (mandatory)

### [5c] — Creates [6] backend fetcher/service issues

Each [6] issue must include:
- [ ] A single, clearly scoped backend service or fetcher
- [ ] Reference to the data source validation from [5a]
- [ ] The "real HTTP enforcement" note (no stubs)
- [ ] A Testing section (mandatory AC) — see Testing Standards
- [ ] `--assignee $(gh repo view --json owner --jq .owner.login)` (mandatory)

### Factory verification

After each factory run:
```bash
# Verify all created issues have an assignee
gh issue list --repo $(gh repo view --json nameWithOwner --jq .nameWithOwner) --limit 30 --json number,assignees | python3 -c "import sys,json; [print(f'#{i[\"number\"]} — assignees: {[a[\"login\"] for a in i[\"assignees\"]]}') for i in json.load(sys.stdin)]"
```

---

## Testing Standards (mandatory)

Before raising any PR, all tests must pass. Testing is never optional.

- **Backend:** Write tests first (TDD). Run `dotnet test` — zero failures.
- **Frontend:** Write tests spec-first. Run `npm test` — zero failures.
- **Strategy + examples:** `docs/specs/testing-strategy.md`

Never raise a PR with failing tests. If tests cannot be made to pass, flag this explicitly in the PR body and ask for guidance.

## CSS Coding Standards

- Always use CSS custom properties (`var(--colour-name)`) for colours, spacing, and typography. **Never hardcode colour values** (hex, rgb, hsl) directly in component CSS.
- Define custom properties in `:root` or a `:global` scope block. Reference them everywhere else.
- Acceptable: `color: var(--colour-primary);`
- Not acceptable: `color: #1a1a2e;` or `color: rgb(26, 26, 46);`

## Frontend PR Screenshots (mandatory)

For any PR that modifies React components, pages, or CSS:

1. Start the dev server: `npm run dev` or `npx serve dist`
2. Capture: `npx playwright screenshot --full-page http://localhost:5173 pr-screenshot.png`
3. Attach to the PR body: `![screenshot](./pr-screenshot.png)` or as a GitHub comment

If Playwright is not available in the environment:
- Note this explicitly in the PR body
- Add a TODO: "Install Playwright in the bot container to enable automated screenshots"

## AI Guards (Claude Code Hooks)

The `.claude/settings.json` in this repo defines shell hooks that run automatically before/after Claude's tool calls. These are structural guards — they enforce rules that cannot be overridden by AI instructions.

Active guards:
- **Linter (PostToolUse):** `prettier` runs after every `.ts`/`.tsx`/`.css` write. `dotnet format` runs after every `.cs` write.
- **Branch guard (PreToolUse):** `git push` to `main` is blocked. Target `dev` and raise a PR.

Do not modify or remove these guards. If a guard is triggering incorrectly, raise an issue.

## Helm Scaffold Setup

After creating a new project from this template, update `helm/values.yaml`:
1. Set `fullnameOverride` to your app name (lowercase, hyphens — e.g. `my-app`)
2. Set `apps[*].ingress.path` to `/{app-name}` (frontend) and `/{app-name}/api` (backend)
3. The `balenthiran.co.uk` host, `balenthiran-tls` cert, and `registryPrefix` are shared — do not change them
4. Add app-specific env vars / secrets as needed — none are pre-scaffolded
5. Do NOT add `deploy.sh` — deployment is via `docker-build-push.yml` pipeline
