#!/usr/bin/env node
/**
 * init-issues.mjs
 *
 * Scaffolds the initial SDD orchestrator issues for a new project.
 * Run this once after the repo is pushed to GitHub.
 *
 * Prerequisites:
 *   - gh CLI installed and authenticated (gh auth login)
 *   - Run from the project root
 *
 * Usage:
 *   node scripts/init-issues.mjs
 */

import { execSync } from 'child_process';
import { writeFileSync, unlinkSync } from 'fs';
import { tmpdir } from 'os';
import { join } from 'path';

function log(msg) {
  console.log(`[init-issues] ${msg}`);
}

function gh(args) {
  return execSync(`gh ${args}`, { encoding: 'utf8' }).trim();
}

function ghSilent(args) {
  try {
    return execSync(`gh ${args}`, { encoding: 'utf8', stdio: ['pipe', 'pipe', 'pipe'] }).trim();
  } catch {
    return null;
  }
}

// ── Labels ────────────────────────────────────────────────────────────────────

const LABELS = [
  { name: 'waiting-for-ai',    color: '2ea44f', description: "AI's turn — discussion, spec iteration, or PR review" },
  { name: 'waiting-for-human', color: '0075ca', description: "Human's turn — reviewing, providing input, or signing off" },
  { name: 'action-ready',      color: '7057ff', description: 'Issue approved for implementation — AI should start coding' },
];

function ensureLabels() {
  log('Creating labels...');
  for (const label of LABELS) {
    const existing = ghSilent(`label list --search "${label.name}" --json name -q ".[0].name"`);
    if (existing === label.name) {
      log(`  Label "${label.name}" already exists — skipping`);
    } else {
      gh(`label create "${label.name}" --color "${label.color}" --description "${label.description}"`);
      log(`  Created label "${label.name}"`);
    }
  }
}

// ── Milestone ─────────────────────────────────────────────────────────────────

const MILESTONE_TITLE = 'MVP';

function ensureMilestone() {
  log('Creating MVP milestone...');
  const existing = ghSilent(`api repos/{owner}/{repo}/milestones --jq '.[] | select(.title=="MVP") | .number'`);
  if (existing) {
    log(`  Milestone "${MILESTONE_TITLE}" already exists (#${existing}) — using it`);
    return MILESTONE_TITLE;
  }
  const number = gh(`api repos/{owner}/{repo}/milestones --method POST --field title="${MILESTONE_TITLE}" --field description="All initial SDD phases through to first launch" --jq '.number'`);
  log(`  Created milestone "${MILESTONE_TITLE}" (#${number})`);
  return MILESTONE_TITLE;
}

// ── Issues ────────────────────────────────────────────────────────────────────

const ISSUES = [
  {
    title: '[1a] Set up pipeline secrets and variables',
    body: `## Summary

Configure CI/CD pipeline secrets and repository variables so automated workflows can build and deploy the app.

## Acceptance Criteria

- [ ] Repository **Variables** added (Settings → Secrets and variables → Variables):
  - \`OCIR_REGISTRY\` — e.g. \`lhr.ocir.io\`
  - \`OCIR_NAMESPACE\` — your OCI tenancy namespace
- [ ] Repository **Secrets** added (Settings → Secrets and variables → Secrets):
  - \`OCIR_USERNAME\` — e.g. \`tenancy/oracleidentitycloudservice/your@email.com\`
  - \`OCIR_AUTH_TOKEN\` — OCI auth token

## AI Notes

This issue is actioned by the developer, not the AI. Close it once secrets are configured.
`,
  },
  {
    title: '[1b] Set up branch policies',
    body: `## Summary

Configure branch protection rules so that \`main\` is developer-only and all agent work flows through \`dev\` via PRs.

## Acceptance Criteria

- [ ] \`main\` branch created and protected (require PR, no direct push)
- [ ] \`dev\` branch created — default branch for agent PRs
- [ ] **"Automatically delete head branches"** enabled (Settings → General)

## AI Notes

This issue is actioned by the developer. Close it once branch policies are in place.
`,
  },
  {
    title: '[1c] Define high-level project spec, vision and external dependencies',
    body: `## Summary

Capture the product vision and scope so the AI can produce the first spec PR — covering epics, features, and external dependencies.

## Acceptance Criteria

- [ ] Spec questionnaire completed (see below)
- [ ] \`ai-ready\` label applied to this issue
- [ ] AI raises a spec PR with vision statement, \`docs/epics/\`, \`docs/features/\`, and dependency notes
- [ ] All ambiguities resolved via PR comments or \`needs-input\` issues
- [ ] Spec PR reviewed and merged by developer

## Spec Questionnaire

Please answer the following, then add the \`ai-ready\` label:

- **What problem does this product solve, and for whom?**

- **What does a successful MVP look like?**

- **Is Postgres required?** If so, any schema or domain hints?

- **Any external data sources, APIs, or third-party integrations?**

- **Any auth providers, payment gateways, or other dependencies?**

- **What is explicitly out of scope for the MVP?**

## AI Notes

When this issue is labelled \`ai-ready\`:
1. Read the questionnaire answers above
2. Raise a spec PR containing: vision statement, \`docs/epics/*.md\`, \`docs/features/*.md\`, out-of-scope list, and external dependency notes
3. Leave a PR comment for anything ambiguous — if unresolved after one round, open a \`needs-input\` issue
`,
  },
  {
    title: '[2a] Generate UI/UX design issues from approved spec',
    body: `## Summary

Once the spec PR from [1c] is merged, scan \`docs/features/\` for frontend features and create one \`[2] <Feature name> design\` issue per feature.

## Acceptance Criteria

- [ ] One \`[2] Feature name design\` issue created per frontend feature in the spec
- [ ] Each issue assigned to the repo owner
- [ ] This issue closed once all \`[2]\` issues are created

## Dependencies

Depends on [1c] spec PR being merged.

## [2] Issue structure

Each \`[2]\` issue the AI creates must follow this format:

\`\`\`
Design the <feature name> for <product name>.

**Feature:** \`docs/features/<feature-file>.md\`

**Open UX questions to resolve:**
- <question from feature file>
- <question from feature file>

**Deliverables (in the design PR):**
- [ ] ASCII mockup for each meaningful page/component state
- [ ] ASCII mockup for each key interaction state (loading, error, empty)
- [ ] Mermaid workflow diagram for each key user action
- [ ] All open UX questions answered
\`\`\`

## AI Notes

Trigger: [1c] PR merged.
Action: Read \`docs/features/\`, create one \`[2] <Feature> design\` issue per frontend feature using the structure above, assign each to the repo owner, then close this issue.
`,
  },
  {
    title: '[3a] Frontend tech decisions + user story spec',
    body: `## [3a] — Frontend Tech Decisions & Stories

The AI raises a single PR containing two documents. Before raising the PR, the five sections below must all be addressed:

### 1. Library choices
- UI component library (e.g. shadcn/ui, Radix, Mantine)
- Chart library (e.g. Recharts, Nivo, Chart.js)
- Date handling library (e.g. date-fns, dayjs)
- State management approach (RTK Query + Redux Toolkit, or other)

### 2. API skeleton contracts
- Endpoint shapes (URL, method, response type)
- RTK Query hook definitions
- TypeScript response types

### 3. Fake data strategy
- How does the frontend get data before the real backend exists?
- Preferred: [2] Faker backend skeleton (confirmed or proposed)
- Alternative: MSW (Mock Service Worker) — if Faker backend not viable

### 4. Frontend TDD approach
- Test framework: Vitest + React Testing Library
- Test-before-component approach (spec-first)
- One test file per component covering key behaviours

### 5. BDD user stories
- Definition: *"As [role], I want [action], so that [benefit]"*
- One story per user workflow from [1e]
- Acceptance criteria per story (Given/When/Then or checklist)

**Both documents (\`docs/tech-decisions-frontend.md\` and \`docs/user-stories-frontend.md\`) must cover all five areas before the PR is raised.**

## Acceptance Criteria

- [ ] \`docs/tech-decisions-frontend.md\` — library choices with rationale (sections 1, 3, 4)
- [ ] \`docs/user-stories-frontend.md\` — BDD stories for every signed-off design (section 5)
- [ ] API skeleton contracts included — endpoint shapes, RTK Query hooks, TypeScript types (section 2)
- [ ] Fake data strategy documented — Faker backend skeleton or MSW alternative (section 3)
- [ ] Frontend TDD approach defined — Vitest + RTL, spec-first, one test file per component (section 4)
- [ ] Human review gate — developer must approve before merge
- [ ] This issue closed on merge

## Dependencies

Depends on all \`[2]\` design issues being closed.

## AI Notes

Trigger: all \`[2]\` issues closed.
Action: Raise a single PR with \`docs/tech-decisions-frontend.md\` (library choices, fake data strategy, TDD approach) and \`docs/user-stories-frontend.md\` (BDD stories + API skeleton contracts). All five sections must be addressed before the PR is raised — do not leave API skeleton, fake data strategy, TDD approach, or BDD definition to AI discretion.
`,
  },
  {
    title: '[3b] Create frontend issues + backend skeleton',
    body: `## Summary

Once the [3a] spec PR is merged, do two things:

**1 — Frontend implementation issues**
Create individual \`[4] Feature name\` issues from the merged user story spec. Closely related stories (e.g. same component) may be grouped into one issue.

**2 — Backend skeleton PR**
Implement the API contracts from the [3a] spec as real ASP.NET Core Minimal API endpoints using \`Bogus\` (Faker for .NET). Seeded, deterministic data. No service layer, no database — just route handlers returning shaped fake data. OpenAPI spec auto-generated so \`npm run codegen\` gives the frontend its RTK Query hooks.

## Acceptance Criteria

- [ ] \`[4]\` issues created (one per story or grouped where appropriate)
- [ ] \`docs/features/*.md\` updated with the GH issue numbers
- [ ] Backend skeleton PR raised — Faker endpoints matching the API contracts in \`docs/user-stories-frontend.md\`
- [ ] This issue closed once all \`[4]\` issues are created and skeleton PR is raised

## Dependencies

Depends on [3a] PR being merged.

## AI Notes

Trigger: [3a] PR merged.
Action: (1) Read \`docs/user-stories-frontend.md\`, create \`[4]\` issues, update \`docs/features/*.md\`. (2) Raise a backend skeleton PR implementing the API endpoints with Bogus-generated data in the WebApi project.
`,
  },
  {
    title: '[5a] Create backend design spec',
    body: `## Summary

Once all frontend implementation issues (\`[4]\`) are closed, raise a backend design PR covering data models, API contracts, EF Core entities, service architecture, and an ER diagram.

## Acceptance Criteria

- [ ] Backend design PR raised containing:
  - [ ] Mermaid ER diagram
  - [ ] Entity and relationship definitions
  - [ ] Service layer outline (SRP — see \`docs/specs/backend-srp.md\`)
  - [ ] In-process integration test scenarios (what, not how)
  - [ ] Any ADR notes
- [ ] Developer reviews and merges
- [ ] This issue closed on merge

## Data Source Validation (required before writing implementation issues)

For each external data source listed in [1c] / [1e]:

- [ ] Confirm the API endpoint and series ID exist and are accessible (curl/httpie the endpoint — do not assume)
- [ ] Document the exact URL pattern used
- [ ] Note rate limits and API key requirements
- [ ] Record the data cadence (daily / monthly / quarterly) and earliest available date
- [ ] Flag any sources that could not be validated with ⚠️ and explain why

**Do not write any [6] fetcher implementation issues until all data sources in scope have been validated.**

## Dependencies

Depends on all \`[4]\` frontend issues being closed.

## AI Notes

Trigger: all \`[4]\` issues closed.
Action: Raise a backend design PR. The OpenAPI spec generated from the skeleton app is the source of truth for API contracts. Follow \`docs/specs/backend-architecture.md\` and \`docs/specs/backend-srp.md\`.
`,
  },
  {
    title: '[5b] Generate backend user stories from approved backend design',
    body: `## Summary

Once the backend design PR from [5a] is merged, raise a backend user story spec PR.

## Acceptance Criteria

- [ ] Backend user story spec PR raised in \`docs/\`
- [ ] Human review gate — developer must approve before merge
- [ ] This issue closed on merge

## Dependencies

Depends on [5a] PR being merged.

## AI Notes

Trigger: [5a] PR merged.
Action: Raise a PR adding \`docs/user-stories-backend.md\` with BDD-style stories derived from the API contracts and backend design.
`,
  },
  {
    title: '[5c] Create backend GitHub issues from user story spec',
    body: `## Summary

Once the backend user story spec PR from [5b] is merged, create individual \`[6] Feature name\` backend implementation issues — one per story.

## Acceptance Criteria

- [ ] One \`[6] <Story>\` issue created per story in the merged backend user story spec
- [ ] Each issue uses the **User Story** issue template
- [ ] \`docs/features/*.md\` updated with the GH issue numbers
- [ ] This issue closed once all \`[6]\` issues are created

## [6] Issue structure

Each \`[6]\` issue the AI creates must include the following section:

\`\`\`
## Before implementing

Confirm the series ID and API endpoint from the Phase 5 data source validation notes in \`[5a]\`. If validation notes are absent, run the validation step before writing any code.
\`\`\`

## Dependencies

Depends on [5b] PR being merged.

## AI Notes

Trigger: [5b] PR merged.
Action: Read \`docs/user-stories-backend.md\`, create one \`[6] <Story>\` issue per story, update \`docs/features/*.md\` with issue numbers, then close this issue.
`,
  },
  {
    title: '[6a] Hook up Postgres DB (if required)',
    body: `## Summary

Wire up the real Postgres database — EF Core migrations, connection config, and schema — replacing the skeleton faker data.

## Acceptance Criteria

- [ ] EF Core migrations created and applied
- [ ] Connection string config documented
- [ ] All skeleton faker routes replaced or wired to real data
- [ ] PR raised and merged

## Dependencies

Depends on all \`[5]\` backend implementation issues being closed.

## AI Notes

Trigger: all \`[6]\` issues closed (or skip if Postgres was marked not required in [1c]).
Action: Scaffold EF Core migrations, wire connection string from K8s secret, raise a PR.
`,
  },
  {
    title: '[7a] Share MVP — publish YouTube walkthrough and gather user feedback',
    body: `## Summary

Draft a YouTube walkthrough script and outline for the MVP launch.

## Acceptance Criteria

- [ ] AI drafts a walkthrough script covering key features and user journeys
- [ ] Developer records and publishes the video
- [ ] Feedback gathered from initial users

## Dependencies

Depends on [6a] (or all backend stories) being merged.

## AI Notes

Trigger: MVP is deployed and stable.
Action: Draft a YouTube script with an intro, feature walkthrough sections, and a call to action. Structure it as a \`docs/mvp-walkthrough-script.md\` PR.
`,
  },
  {
    title: '[7b] Create production refinement tickets',
    body: `## Summary

Generate production refinement tickets covering performance, security, scaling, and testing gaps.

## Acceptance Criteria

- [ ] Refinement tickets created covering: performance, security hardening, scaling considerations, and test coverage gaps

## Dependencies

Depends on [7a].

## AI Notes

⚠️ **Confirm developer interest before actioning this issue.**
Trigger: Developer explicitly asks the AI to action this.
Action: Audit the codebase and raise targeted issues for: N+1 queries, missing indexes, auth hardening, rate limiting, load testing, and any coverage gaps identified during implementation.
`,
  },
];

// ── Main ──────────────────────────────────────────────────────────────────────

async function run() {
  log('Scaffolding initial SDD issues...');
  log('');

  ensureLabels();
  log('');

  const milestone = ensureMilestone();
  log('');

  log(`Creating ${ISSUES.length} orchestrator issues...`);
  for (const issue of ISSUES) {
    const existing = ghSilent(`issue list --search "${issue.title}" --state all --json title -q ".[0].title"`);
    if (existing === issue.title) {
      log(`  SKIP  "${issue.title}" — already exists`);
      continue;
    }

    const bodyFile = join(tmpdir(), `issue-body-${Date.now()}.md`);
    writeFileSync(bodyFile, issue.body);

    gh(`issue create --title "${issue.title}" --body-file "${bodyFile}" --milestone "${milestone}"`);
    log(`  CREATED  "${issue.title}"`);

    try { unlinkSync(bodyFile); } catch { /* ignore */ }
  }

  log('');
  log('✅ Done. All initial SDD issues created.');
  log('   Next step: work through Phase 1 issues, then add the waiting-for-ai label to [1c] once the spec questionnaire is complete.');
}

run().catch(err => {
  console.error(err);
  process.exit(1);
});
