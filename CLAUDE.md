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
├── scripts/init-issues.mjs        # Scaffolds initial SDD issues on GitHub
├── helm/                          # Helm chart for Kubernetes deployment
├── docs/specs/                    # Architecture decisions and how-to guides
└── .agents/                       # Coding conventions and workflow rules
```

## Key Docs

| Doc | What it covers |
|---|---|
| `docs/specs/backend-architecture.md` | 7-project Clean Architecture layer structure |
| `docs/specs/backend-srp.md` | SRP, orchestrator pattern, service naming |
| `docs/specs/openapi-codegen.md` | OpenAPI → RTK Query codegen workflow |
| `docs/specs/testing-strategy.md` | Unit tests, in-process integration tests, Vitest |
| `docs/specs/sdd-workflow.md` | 7-phase Spec Driven Development process |
| `docs/ai-workflow.md` | End-to-end AI-assisted project workflow and phase structure |
| `.agents/rules/project.md` | Coding conventions, branch strategy, GitHub workflow rules |

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

## How to Trigger Claude

Tag `@claude` in any issue or PR comment. Claude reads the full thread before acting.

Examples:
- `@claude the spec questionnaire in [1c] is filled in — please raise the spec PR`
- `@claude implement this issue`
- `@claude address the review comments on this PR`
- `@claude [2a] is ready — please create the design issues`

If you want a plan or have a question, just ask. If you want implementation, say so.

## GitHub Conventions

- **Branch target**: All PRs target `dev`. Never target `main` — `dev` → `main` is a human-only action.
- **Never push directly**: Claude must never `git push` directly to `dev` or `main`. All changes must go through a PR, even if branch protection is not enforced.
- **PR assignment**: Every PR Claude raises must be assigned to the repo owner.
- **Issue assignment**: Every issue Claude acts on must be assigned to the repo owner.
- **Issue linking**: Every PR body must include `Closes #N`. Claude also comments on the issue: *🤖 PR raised: #N — please review when ready.*

## Notification Rule (mandatory)

Always assign the repository owner to every issue and PR:
- `gh issue create ... --assignee $(gh repo view --json owner --jq .owner.login)`
- `gh pr create   ... --assignee $(gh repo view --json owner --jq .owner.login)`
- Finding owner: Use `gh repo view --json owner --jq .owner.login`. Cache it — don't call multiple times.
- Verification: After creating issues/PRs, run `gh issue view <N> --json assignees` to confirm.

## Assumptions & Decisions (mandatory for all AI-raised PRs)

Every PR Claude raises must include a populated "Assumptions & Decisions" section in the PR body:

```markdown
## Assumptions & Decisions

> Any assumption made during implementation that was not explicitly specified in the issue or spec.
> If you disagree with any item, comment and tag `@claude` with the correction — Claude will revise.

| # | Assumption | Rationale | If incorrect... |
|---|---|---|---|
| 1 | | | |
```

Rules:
- Minimum one row. Use "N/A — no ambiguous assumptions" only if genuinely nothing was assumed.
- Always surface: branch target choice, external API/series ID choices, CSS/styling strategy, database decisions.
- Never leave the table blank.

## Implementing an Issue

When tagged to implement a GitHub issue:

### 1. Read the full issue before starting
Read the issue body AND all comments. The comments contain the negotiated spec. Do not start until everything is read.

### 2. Create a branch off dev
```bash
git checkout dev && git pull
git checkout -b {issue-number}-{short-kebab-description}
```

### 3. Implement with TDD
Follow the top-down TDD flow in `docs/specs/testing-strategy.md`. Write tests before implementation. Commit after each meaningful unit of work.

### 4. Format before every commit
```bash
# C# files
cd backend && dotnet format

# TypeScript / CSS files
npx prettier --write "frontend/src/**/*.{ts,tsx,css}"
```

### 5. Push frequently
Push after every meaningful commit. The developer cannot see work until it is on the remote.
```bash
git push -u origin {issue-number}-{description}
```

### 6. Open a PR targeting dev
```bash
gh pr create --base dev --title "..." --body "..."
```
PR body must include:
- `Closes #N` to auto-close the issue on merge
- Summary of what was implemented
- Assumptions & Decisions table
- Anything the reviewer needs to know to assess the work without running it locally

### 7. Do not merge your own PR
Leave it open for the developer to review. Comment on the issue: *🤖 PR raised: #N — please review when ready.*

## Responding to PR Reviews

When tagged to address PR review comments:

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
Do not batch everything and push once at the end. The developer follows progress via the commit history.

### 5. Reply to every comment on GitHub
After pushing the relevant commit, reply explaining what changed and why. Do not leave comments unacknowledged.

### 6. Post a summary comment when done
Once all comments are addressed, post a single PR comment summarising what was changed and flagging anything still needing a decision.

### 7. Do not re-request review or resolve conversations
Leave that to the developer.

## Pre-PR Checklist

Run this checklist before raising any PR against backend changes.

### 1. Naming Conventions

| Check | Rule |
|---|---|
| DataModels use plain nouns | `Ratio`, not `RatioResponse` — use `*Response` only if the route constructs a meaningfully different shape |
| `*Request` suffix | Only for inbound route bodies with non-trivial shape; never for query-string parameters |
| DomainModels prefixed `Domain*` | `DomainRatio`, `DomainIndicator` — not plain nouns |
| Entities suffixed `*Entity` | Reserved for EF Core entity classes in `EntityModels/` |

### 2. Abstractions Project — Interfaces Only

- [ ] No concrete types defined in `HolidayPlanning.Abstractions`
- [ ] Service interface methods return **interfaces**, not concrete types (`IRatio`, not `Ratio`)
- [ ] `HolidayPlanning.Abstractions.csproj` has **zero** `<ProjectReference>` entries to concrete projects

### 3. Inheritance & Extension Pattern

- [ ] Every `Domain*` class extends its `DataModels` counterpart (`DomainRatio : Ratio`)
- [ ] `DataModels` classes implement the corresponding `Abstractions` interface (`Ratio : IRatio`)
- [ ] Domain models in `HolidayPlanning.DomainModels`, data models in `HolidayPlanning.DataModels`

### 4. Route Handlers

- [ ] Handlers are **named static methods**, not inline lambdas
- [ ] Return **typed results** — `Ok<T>`, `Results<Ok<T>, NotFound>` — never bare `IResult`
- [ ] Mapping done via **AutoMapper** (`IMapper`), not manual helpers
- [ ] No business logic in routes

### 5. Service Layer

- [ ] Service methods return interface types
- [ ] No HTTP or serialisation concepts in services
- [ ] Bogus/faker logic lives in `Services/`, never in `WebApi/`

### 6. AutoMapper Profiles

- [ ] `WebApi/Mapper.cs` — maps `Domain*` → DataModel (API boundary)
- [ ] `Services/Mapper.cs` — maps `*Entity` → `Domain*` (DB boundary)
- [ ] Both profiles discovered via `cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies())`

### 7. Build Gate

```bash
cd backend && dotnet build
```

- [ ] **0 errors** before raising the PR

## Code Formatting

Format before every commit. CI enforces these on PRs.

```bash
# C# — run from repo root
cd backend && dotnet format

# TypeScript / CSS
npx prettier --write "frontend/src/**/*.{ts,tsx,css}"
```

## Phase Guard Status Format

When tagged on an orchestrator issue, check phase dependencies and post a concise status block if any checks are pending:

```markdown
## 🔍 Phase Check

| Check | Status |
|---|---|
| Phase N dependencies | ⏳ #32, #34 still open — waiting |
| Key decision from [1c] | ✅ "No DB for MVP" — EF Core items skipped |
| External data sources | ⚠️ CAPE source unverified — flagging in spec |
```

Rules:
- Maximum 3–5 rows — only include relevant checks
- ✅ = confirmed, ⏳ = pending, ⚠️ = flagged
- If all checks pass: proceed directly without posting the table

## Clarification Protocol

**Step 1: Can I make a sensible assumption?**
- Yes → implement with the assumption, document it in the PR's Assumptions & Decisions table. **Do not block.**

**Step 2: Is this a major architectural decision with no sensible default?**
- Yes → post a single comment with the question + recommended default. Wait for a response before implementing.
- No → treat as Step 1.

Default behaviour: **Mixed** — assume minor/stylistic choices, ask only for major architectural decisions with no sensible default.

Anti-patterns:
- ❌ Asking the same question more than once
- ❌ Blocking on minor choices (library patch version, variable names)
- ❌ Posting multiple clarifying questions in separate comments — batch them into one

## Issue Factories ([3b] and [5c])

Issue factories are passes that create multiple downstream issues programmatically.

### [3b] — Creates [4] frontend implementation issues

Each [4] issue must include:
- [ ] A single, clearly scoped frontend feature or component
- [ ] Acceptance criteria referencing the BDD story from [3a]
- [ ] A Testing section (mandatory AC)
- [ ] A Visual Evidence section (mandatory AC)
- [ ] `--assignee $(gh repo view --json owner --jq .owner.login)` (mandatory)

### [5c] — Creates [6] backend implementation issues

Each [6] issue must include:
- [ ] A single, clearly scoped backend service or fetcher
- [ ] Reference to the data source validation from [5a]
- [ ] A Testing section (mandatory AC)
- [ ] `--assignee $(gh repo view --json owner --jq .owner.login)` (mandatory)

### Factory verification

After each factory run:
```bash
gh issue list --repo $(gh repo view --json nameWithOwner --jq .nameWithOwner) --limit 30 --json number,assignees | python3 -c "import sys,json; [print(f'#{i[\"number\"]} — assignees: {[a[\"login\"] for a in i[\"assignees\"]]}') for i in json.load(sys.stdin)]"
```

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

## Helm Scaffold Setup

After creating a new project from this template, update `helm/values.yaml`:
1. Set `fullnameOverride` to your app name (lowercase, hyphens — e.g. `my-app`)
2. Set `apps[*].ingress.path` to `/{app-name}` (frontend) and `/{app-name}/api` (backend)
3. The `balenthiran.co.uk` host, `balenthiran-tls` cert, and `registryPrefix` are shared — do not change them
4. Add app-specific env vars / secrets as needed — none are pre-scaffolded
5. Do NOT add `deploy.sh` — deployment is via `docker-build-push.yml` pipeline
