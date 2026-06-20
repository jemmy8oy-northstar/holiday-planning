# Spec Driven Development (SDD) Workflow

A structured, iterative development process designed to maximise in-chat planning before any code is written — ensuring the full scope of the application is understood before committing to implementation.

## Core Philosophy

- **Iterate in chat first** — decisions made in chat are cheap; decisions made in code are expensive.
- **Defer implementation details** — don't think about databases or frameworks until the product shape is fully understood.
- **Let the spec drive the code** — user stories, mockups, and API contracts are the source of truth.
- **Tests are a spec artefact** — a unit test is a more precise, executable version of a spec step. Writing tests before implementation is not extra work; it is the spec becoming code.
- **Strict phase linearity** — each phase has a gate. Do not produce artefacts that belong to a later phase. If detail is discovered early, note it as an open question and defer it to the correct phase.

## Phase gates — do not cross

> **Terminology note:** "GitHub Phase N" refers to the phase numbering used in issue titles (`[1c]`, `[2]`, `[3]`, etc.) and in `docs/ai-workflow.md`. These align with but do not map 1:1 to the SDD phase names below — see the [GitHub Phase Mapping](#github-phase-mapping) table.

Each phase ends with a hard gate. The next phase must not begin until all gate conditions are met.

| GitHub Phase complete | Gate conditions |
|---|---|
| **GitHub Phase 1** (spec) | `[1c]` spec PR merged **and** `[2a]` has created all `[2]` design issues |
| **GitHub Phase 2** (UI/UX design) | All `[2]` design issues closed |
| **GitHub Phase 3** (user stories) | `[3a]` story spec PR merged **and** `[3b]` has created all `[3]` story issues **and** all `[4]` frontend implementation issues |
| **GitHub Phase 4** (frontend impl) | All `[4]` frontend implementation issues closed |
| **GitHub Phase 5** (backend design) | `[5a]` backend design PR merged **and** `[5b]`/`[5c]` have created all `[5]` story issues **and** all `[6]` backend implementation issues |
| **GitHub Phase 6** (backend impl) | All `[5]`/`[6]` backend stories closed |
| **GitHub Phase 7** (MVP) | `[7a]` walkthrough published |

**Current phase discipline:** Only produce artefacts that belong to the current GitHub phase. Do not include:
- `[2]` design issue content in a GitHub Phase 1 spec PR (create issues via `[2a]`, don't write mockups in the spec)
- API endpoint contracts in a GitHub Phase 1/2 artefact (belongs in GitHub Phase 5 via `[5a]`)
- Database schema before UI/UX is signed off (belongs in GitHub Phase 5/6)
- Component props or implementation code in feature files (belongs in GitHub Phase 4 skeleton)
- User stories before mockups are approved (belongs in GitHub Phase 3 via `[3a]`/`[3b]`)

---

## Ticketing Structure

Tickets live in two places depending on their role:

| Artifact | Where | Purpose |
|---|---|---|
| Epics | `docs/epics/*.md` | Broad capability groupings — stable, high-level |
| Features | `docs/features/*.md` | A scoped piece of an epic — references epic, lists story issue numbers |
| User Stories | GitHub Issues | Unit of work — precise acceptance criteria tied to a specific screen or interaction |

Epics and features are written early and are stable. User stories are written after the UI/UX is signed off, so acceptance criteria can reference specific UI states.

Each feature MD is updated with GH issue numbers as stories are created:

```markdown
## Stories
- #12 — User can submit the registration form with a valid email
- #13 — User sees an inline error when the email is already registered
- #14 — User receives a confirmation email after successful registration
```

---

## The Workflow

### Issue-driven development

```
AI creates GH issue with initial spec
  ↓
Developer reviews, leaves comments
  ↓
AI responds, spec is iterated
  ↓
Consensus reached — issue is the agreed spec
  ↓
AI implements, opens PR referencing "Closes #N"
  ↓
Developer reviews PR, leaves comments
  ↓
AI addresses feedback
  ↓
Merge → issue auto-closes
```

The issue body is the first-pass spec. Comments are the negotiation. The PR is the proof. This creates a full audit trail from spec to discussion to implementation to review.

---

## GitHub Phase Mapping

Each SDD phase maps to a numbered project phase used in GitHub issue titles. See [`docs/ai-workflow.md`](../ai-workflow.md) for the full bootstrap workflow, initial issues, and the `[Na]` / `[N]` issue numbering convention.

| SDD Phase | GitHub Phase | Issues |
|-----------|-------------|--------|
| Phase 1 — Vision & Planning | Phase 1 — Project Setup | `[1a]`, `[1b]`, `[1c]` |
| Phase 2 — Epic & Feature Breakdown | *(part of Phase 1 spec PR)* | — |
| Phase 3 — UI/UX Design | Phase 2 — UI/UX Design | `[2a]`, `[2]` per feature |
| Phase 4 — User Story Definition | Phase 3 — Frontend User Stories | `[3a]`, `[3b]`, `[3]` per story |
| Phase 5 — UI + Skeleton Backend | Phase 4 — Frontend Implementation | `[4]` per story |
| Phase 6 — Backend Architecture & DB | Phase 5 — Backend Design & Stories | `[5a]`, `[5b]`, `[5c]` |
| Phase 7 — Backend Implementation | Phase 6 — Backend Implementation | `[6a]` (DB), `[6]` per story |
| *(post-implementation)* | Phase 7 — MVP | `[7a]`, `[7b]` |

---

## The Seven Phases

### Phase 1 — Vision & High-Level Planning
**In chat**

Align on what we are building and why before any decomposition.

**Activities:**
- Define the product vision in 2–3 sentences
- List the core problems being solved
- Identify the primary user personas
- Agree on MVP scope boundaries — what is explicitly out of scope for POC

**Outputs:**
- Vision statement
- Initial out-of-scope list

---

### Phase 2 — Epic & Feature Breakdown
**In chat → `docs/epics/` and `docs/features/`**

Decompose the product into epics and features at a coarse-grained level — establishing *what* the app needs to do, not *how*. User stories are not written here; they come after the UI is designed.

**Activities:**
- Identify epics (broad capability groupings)
- Break each epic into features (scoped, deliverable pieces)
- Flag features that are unclear or carry obvious technical risk

**Outputs:**
- `docs/epics/*.md` — one file per epic
- `docs/features/*.md` — one file per feature, referencing its epic

**Spec PR guidance:**
The Phase 1/2 spec PR should contain the vision doc, epics, and feature files. Keep the following in mind:

- **Frontend features** (UI epics): describe *what the user sees and does* — layout, interactions, open UX questions. Do not specify React component props or implementation detail here.
- **Backend features** (data/API epics): describe the *capability* only at this stage (e.g. "fetch time-series from FRED"). Mark them as `[draft]` — implementation detail belongs in Phase 6. Do not include endpoint contracts, code snippets, or service design in the spec PR.
- **Open questions**: surface UI/UX decisions that need resolving before Phase 3 (e.g. date range selector style, card layout, picker interaction). Collect these in the vision doc's "Next: Phase 2" section.
- **No implementation detail jumps**: resist the urge to spec the API contract or database schema before the UI is designed. The full shape of the API only becomes clear after Phase 3 (UI/UX) and Phase 4 (user stories).

---

### Phase 3 — UI/UX Design
**In chat — ASCII mockups + Mermaid diagrams**

Fully design the user-facing product before touching a component. Every meaningful page state and user journey should be covered.

**Activities:**
- **User journey mapping** — narrative descriptions of key workflows
- **ASCII mockups** — for each page / modal / drawer, iterate until signed off
- **Mermaid workflow diagrams** — sequence or flowchart diagrams showing what happens when the user performs an action (what the system does in response, what data flows, what side effects occur)
- **Data flow consideration** — what data is displayed, where it comes from, what mutations occur

**Outputs:**
- Signed-off ASCII mockup per page/state
- Mermaid workflow diagram per key user action
- Data flow notes

**Why ASCII mockups?**
ASCII mockups can be reviewed, critiqued, and iterated entirely within the chat window — no screenshots, no build steps, no rendering pipeline. Once the design is locked, translating it to React is straightforward because the component structure is already implicit in the layout.

**Why Mermaid workflow diagrams?**
They make backend complexity visible before any skeleton code is written. A sequence diagram for "user submits registration form" might reveal that you need an email service, a duplicate-check query, and a redirect — all things that affect the API design.

---

### Phase 4 — User Story Definition
**Spec PR (`[3a]`) → GitHub Issues (`[3b]`)**

Now that the UI is signed off, raise a spec PR (`[3a]`) containing two documents that together cover five required areas. Stories are tied to specific screen states and interactions, not vague feature descriptions.

**`[3a]` — Five required sections (all must be addressed before the PR is raised):**

1. **Library choices** — UI component library, chart library, date handling, state management approach
2. **API skeleton contracts** — endpoint shapes (URL, method, response type), RTK Query hook definitions, TypeScript response types
3. **Fake data strategy** — how the frontend gets data before the real backend exists (Faker backend skeleton preferred; MSW as fallback)
4. **Frontend TDD approach** — Vitest + React Testing Library, spec-first (test-before-component), one test file per component
5. **BDD user stories** — *As [role], I want [action], so that [benefit]*; one story per user workflow; Given/When/Then acceptance criteria

**Activities:**
- Write user stories per feature: *As a [persona], I want to [action] so that [outcome]*
- Define acceptance criteria referencing specific UI states from the signed-off mockups
- Document API skeleton contracts — endpoint shapes as TypeScript interfaces, RTK Query hooks
- Define fake data strategy and TDD approach before `[3b]` creates implementation issues
- AI creates GH issues with the initial spec — developer reviews, comments, and iterates
- Update `docs/features/*.md` with the GH issue numbers once created

**Outputs:**
- `docs/tech-decisions-frontend.md` — library choices, fake data strategy, TDD approach
- `docs/user-stories-frontend.md` — BDD stories + API skeleton contracts
- GH issues per user story (iterated to consensus before implementation begins)
- Feature MD files updated with issue numbers

**Story quality bar:**
A good story has acceptance criteria specific enough that there is no ambiguity about whether it is done. "User can register" is a feature. "Given the registration modal is open and the user submits a valid email, the submit button shows a loading state and the modal closes on success" is a story.

---

### Issue naming conventions

#### GitHub Phase 2 — UI/UX design issues (created by `[2a]`)

One issue per **frontend feature**. Created automatically when `[2a]` is actioned after `[1c]` merges. Title format:

```
[2] <Feature name> design
```

Examples:
- `[2] Homepage layout & navigation design`
- `[2] Preset ratio card grid design`
- `[2] Ratio chart component design`
- `[2] Metric picker UI design`
- `[2] Custom comparison chart design`
- `[2] Indicator card component design`
- `[2] Indicators section design`

Backend features do **not** get a `[2]` design issue — their design is handled in GitHub Phase 5 via `[5a]`.

---

#### GitHub Phase 3 — Frontend user story issues (created by `[3b]`)

One issue per **user story** within a feature. Created automatically when `[3b]` is actioned after `[3a]` merges. Title format:

```
[3] <Short description of the user action or screen state>
```

Examples:
- `[3] User can view preset ratio cards on homepage`
- `[3] Ratio card shows current value and % deviation from average`
- `[3] User can select a custom numerator and denominator`
- `[3] Custom comparison chart updates on metric selection`
- `[3] User sees a loading state while ratio data fetches`

Rules:
- One story = one testable behaviour
- Name from the user's perspective, not the implementation
- Acceptance criteria must reference a specific signed-off ASCII mockup state

---

#### GitHub Phase 4 — Frontend implementation issues (created by `[3b]`)

One issue per **frontend story** to be implemented. Created at the same time as `[3]` story issues when `[3b]` is actioned. Title format:

```
[4] <Feature or story name>
```

Examples:
- `[4] Preset ratio card grid`
- `[4] Ratio chart component`
- `[4] Metric picker UI`
- `[4] Custom comparison chart`
- `[4] Indicator card component`

Each `[4]` issue maps to a `[3]` story — the `[3]` issue holds the acceptance criteria, the `[4]` issue is what the AI implements.

---

#### GitHub Phase 5/6 — Backend user story issues (created by `[5c]`)

One issue per **backend capability**. Created automatically when `[5c]` is actioned after `[5b]` merges. Title format:

```
[5] <Short description of the capability or endpoint>
```

Examples:
- `[5] GET /api/metrics returns full metric catalogue`
- `[5] GET /api/metrics/{id} returns time series for a metric`
- `[5] GET /api/metrics/ratio computes and returns ratio series`
- `[5] FRED fetcher retrieves US CPI series`

---

#### GitHub Phase 6 — Backend implementation issues (created by `[5c]`)

One issue per **backend capability** to be implemented. Created at the same time as `[5]` story issues when `[5c]` is actioned. Title format:

```
[6] <Backend capability or endpoint>
```

Examples:
- `[6] GET /api/metrics endpoint`
- `[6] GET /api/metrics/{id} endpoint`
- `[6] GET /api/metrics/ratio endpoint`
- `[6] FRED fetcher service`
- `[6] ONS fetcher service`
- `[6] yfinance fetcher service`

Each `[6]` issue maps to a `[5]` story — the `[5]` issue holds the acceptance criteria, the `[6]` issue is what the AI implements (TDD: tests first).

---

#### Feature file — Stories section

Once issues are created by `[3b]` or `[5c]`, update the relevant `docs/features/*.md` `## Stories` section:

```markdown
## Stories
- #42 — [3] User can view preset ratio cards on homepage
- #43 — [3] Ratio card shows current value and % deviation from average
- #44 — [3] User sees a loading state while ratio data fetches
```

---

### Phase 5 — UI + Skeleton Backend
**Implementation**

Build a fully navigable, fully wired-up frontend backed by faked endpoints. The app should look and behave like the real thing — just no real data.

**Activities:**
- Scaffold routes in .NET — one route per endpoint, returning Faker-generated DTOs
- Generate OpenAPI spec from the running skeleton app
- Run codegen to produce RTK Query hooks from the OpenAPI spec
- Implement the frontend using generated hooks — no hardcoded data
- Deploy to scratch environment for interactive QA and sign-off

**Outputs:**
- Fully functional skeleton app (deployed to scratch)
- Complete OpenAPI spec

---

### Phase 6 — Backend Architecture & DB Design
**In chat**

Now that the full scope of business contracts is known, design the backend before writing any implementation code.

**Activities:**
- Entity design — identify domain entities from API contracts, draw ER diagram in Mermaid
- EF Core model design — define entities, relationships, indexes, and constraints
- Service architecture — agree on orchestrators, single-responsibility services, and DI wiring (see `docs/specs/backend-srp.md`)
- Define in-process integration test scenarios — capture what end-to-end workflows must be verified (implemented at the end of Phase 7)
- Architectural Decision Records (ADRs) — capture significant decisions as short notes in `docs/`

**Outputs:**
- Mermaid ER diagram
- Service layer outline
- Integration test scenarios (what, not how)
- ADR notes

---

### Phase 7 — Backend Implementation
**Story by story, test first**

Replace faked skeleton endpoints with real business logic. Work story by story from the GH issue list, closing each issue via a PR. Follow top-down TDD — see `docs/specs/testing-strategy.md`.

**Activities:**
- Pick up a GH issue, implement, open PR with `Closes #N`
- Developer reviews PR, AI addresses feedback, merges
- Each story: write unit tests → implement service → wire to repository → migration
- The OpenAPI spec should not change during this phase — flag any contract changes explicitly
- Implement in-process integration tests once the full dependency tree is built

**Outputs:**
- Production-ready backend
- EF Core migrations
- Unit tests per service
- In-process integration tests covering Phase 6 scenarios
- All user story issues closed via merged PRs

---

## Tooling Conventions

| Concern | Tool |
|---|---|
| UI Mockups | ASCII (in-chat) |
| Workflow diagrams | Mermaid (sequence / flowchart) |
| Architecture / ER diagrams | Mermaid |
| Epic & feature specs | MD files in `docs/epics/`, `docs/features/` |
| User stories | GitHub Issues |
| API contract generation | OpenAPI (.NET routes) |
| Frontend API client | RTK Query (codegen) |
| Fake data | Faker (.NET skeleton routes) |
| PR / QA review | Scratch environment deployment |
| Backend unit tests | xUnit + Moq |
| In-process integration tests | xUnit + Moq (real services, external I/O mocked) |
| Frontend unit tests | Vitest + React Testing Library |

---

## Summary Flow

```
1. Vision & Planning              (chat)
2. Epic & Feature Breakdown       (chat → docs/epics/, docs/features/)
3. UI/UX Design                   (chat — ASCII mockups + Mermaid workflows)
4. User Story Definition          (chat → GH Issues, iterated via comments)
5. UI + Skeleton Backend          (implementation)
6. Backend Architecture & DB      (chat — Mermaid ER + service design)
7. Backend Implementation         (TDD, story by story, PRs close issues)
```
