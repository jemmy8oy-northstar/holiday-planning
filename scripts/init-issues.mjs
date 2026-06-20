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

Configure CI/CD pipeline secrets and repository variables so automated workflows can build, deploy, and run Claude.

## Acceptance Criteria

- [ ] Repository **Variables** added (Settings → Secrets and variables → Variables):
  - \`OCIR_REGISTRY\` — e.g. \`lhr.ocir.io\`
  - \`OCIR_NAMESPACE\` — your OCI tenancy namespace
- [ ] Repository **Secrets** added (Settings → Secrets and variables → Secrets):
  - \`OCIR_USERNAME\` — e.g. \`tenancy/oracleidentitycloudservice/your@email.com\`
  - \`OCIR_AUTH_TOKEN\` — OCI auth token
  - \`CLAUDE_CODE_OAUTH_TOKEN\` — OAuth token for the Claude GitHub Action

## Notes

This issue is actioned by the developer, not Claude. Close it once secrets are configured.
`,
  },
  {
    title: '[1b] Set up branch policies',
    body: `## Summary

Configure branch protection rules so that \`main\` is developer-only and all work flows through \`dev\` via PRs.

## Acceptance Criteria

- [ ] \`main\` branch created and protected (require PR, no direct push)
- [ ] \`dev\` branch created — default branch for Claude PRs
- [ ] **"Automatically delete head branches"** enabled (Settings → General)

## Notes

This issue is actioned by the developer. Close it once branch policies are in place.
`,
  },
  {
    title: '[1c] Define high-level project spec, vision and external dependencies',
    body: `## Summary

Capture the product vision and scope so Claude can produce the first spec PR — covering epics, features, and external dependencies.

## Acceptance Criteria

- [ ] Spec questionnaire completed (see below)
- [ ] Developer comments \`@claude [1c] is ready — please raise the spec PR\` on this issue
- [ ] Claude raises a spec PR with vision statement, \`docs/epics/\`, \`docs/features/\`, and dependency notes
- [ ] All ambiguities resolved via PR comments
- [ ] Spec PR reviewed and merged by developer

## Spec Questionnaire

Please answer the following, then tag \`@claude\` to proceed:

- **What problem does this product solve, and for whom?**

- **What does a successful MVP look like?**

- **Is Postgres required?** If so, any schema or domain hints?

- **Any external data sources, APIs, or third-party integrations?**

- **Any auth providers, payment gateways, or other dependencies?**

- **What is explicitly out of scope for the MVP?**

## When tagged with \`@claude\`

1. Read the questionnaire answers above
2. Raise a spec PR containing: vision statement, \`docs/epics/*.md\`, \`docs/features/*.md\`, out-of-scope list, and external dependency notes
3. Leave a PR comment for anything ambiguous
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

Each \`[2]\` issue must follow this format:

\`\`\`
Design the <feature name> for <product name>.

**Feature:** \`docs/features/<feature-file>.md\`

**Open UX questions to resolve:**
- <question from feature file>

**Deliverables (in the design PR):**
- [ ] ASCII mockup for each meaningful page/component state
- [ ] ASCII mockup for each key interaction state (loading, error, empty)
- [ ] Mermaid workflow diagram for each key user action
- [ ] All open UX questions answered
\`\`\`

## When tagged with \`@claude\`

Read \`docs/features/\`, create one \`[2] <Feature> design\` issue per frontend feature using the structure above, assign each to the repo owner, then close this issue.
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
- Preferred: Faker backend skeleton (confirmed or proposed)
- Alternative: MSW (Mock Service Worker) — if Faker backend not viable

### 4. Frontend TDD approach
- Test framework: Vitest + React Testing Library
- Test-before-component approach (spec-first)
- One test file per component covering key behaviours

### 5. BDD user stories
- Definition: *"As [role], I want [action], so that [benefit]"*
- One story per user workflow
- Acceptance criteria per story (Given/When/Then or checklist)

**Both documents (\`docs/tech-decisions-frontend.md\` and \`docs/user-stories-frontend.md\`) must cover all five areas before the PR is raised.**

## Acceptance Criteria

- [ ] \`docs/tech-decisions-frontend.md\` — library choices with rationale
- [ ] \`docs/user-stories-frontend.md\` — BDD stories for every signed-off design
- [ ] API skeleton contracts included — endpoint shapes, RTK Query hooks, TypeScript types
- [ ] Fake data strategy documented
- [ ] Frontend TDD approach defined
- [ ] Human review gate — developer must approve before merge
- [ ] This issue closed on merge

## Dependencies

Depends on all \`[2]\` design issues being closed.

## When tagged with \`@claude\`

Raise a single PR with \`docs/tech-decisions-frontend.md\` (library choices, fake data strategy, TDD approach) and \`docs/user-stories-frontend.md\` (BDD stories + API skeleton contracts). All five sections must be addressed before the PR is raised.
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

## When tagged with \`@claude\`

(1) Read \`docs/user-stories-frontend.md\`, create \`[4]\` issues, update \`docs/features/*.md\`. (2) Raise a backend skeleton PR implementing the API endpoints with Bogus-generated data in the WebApi project.
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

For each external data source listed in [1c]:

- [ ] Confirm the API endpoint and series ID exist and are accessible (curl the endpoint — do not assume)
- [ ] Document the exact URL pattern used
- [ ] Note rate limits and API key requirements
- [ ] Record the data cadence (daily / monthly / quarterly) and earliest available date
- [ ] Flag any sources that could not be validated with ⚠️ and explain why

**Do not write any [6] fetcher implementation issues until all data sources in scope have been validated.**

## Dependencies

Depends on all \`[4]\` frontend issues being closed.

## When tagged with \`@claude\`

Raise a backend design PR. The OpenAPI spec generated from the skeleton app is the source of truth for API contracts. Follow \`docs/specs/backend-architecture.md\` and \`docs/specs/backend-srp.md\`.
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

## When tagged with \`@claude\`

Raise a PR adding \`docs/user-stories-backend.md\` with BDD-style stories derived from the API contracts and backend design.
`,
  },
  {
    title: '[5c] Create backend GitHub issues from user story spec',
    body: `## Summary

Once the backend user story spec PR from [5b] is merged, create individual \`[6] Feature name\` backend implementation issues — one per story.

## Acceptance Criteria

- [ ] One \`[6] <Story>\` issue created per story in the merged backend user story spec
- [ ] \`docs/features/*.md\` updated with the GH issue numbers
- [ ] This issue closed once all \`[6]\` issues are created

## [6] Issue structure

Each \`[6]\` issue must include:

\`\`\`
## Before implementing

Confirm the series ID and API endpoint from the Phase 5 data source validation notes in \`[5a]\`. If validation notes are absent, run the validation step before writing any code.
\`\`\`

## Dependencies

Depends on [5b] PR being merged.

## When tagged with \`@claude\`

Read \`docs/user-stories-backend.md\`, create one \`[6] <Story>\` issue per story, update \`docs/features/*.md\` with issue numbers, then close this issue.
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

Depends on all \`[6]\` backend implementation issues being closed.

## When tagged with \`@claude\`

Scaffold EF Core migrations, wire connection string from K8s secret, raise a PR. Skip if Postgres was marked not required in [1c].
`,
  },
  {
    title: '[7a] Share MVP — publish YouTube walkthrough and gather user feedback',
    body: `## Summary

Draft a YouTube walkthrough script and outline for the MVP launch.

## Acceptance Criteria

- [ ] Claude drafts a walkthrough script covering key features and user journeys as \`docs/mvp-walkthrough-script.md\`
- [ ] Developer records and publishes the video
- [ ] Feedback gathered from initial users

## Dependencies

Depends on [6a] (or all backend stories) being merged.

## When tagged with \`@claude\`

Draft a YouTube script with an intro, feature walkthrough sections, and a call to action. Raise it as a PR adding \`docs/mvp-walkthrough-script.md\`.
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

## When tagged with \`@claude\`

⚠️ **Only action when the developer explicitly asks.**
Audit the codebase and raise targeted issues for: N+1 queries, missing indexes, auth hardening, rate limiting, load testing, and any coverage gaps identified during implementation.
`,
  },
];

// ── Main ──────────────────────────────────────────────────────────────────────

async function run() {
  log('Scaffolding initial SDD issues...');
  log('');

  const milestone = ensureMilestone();
  log('');

  const owner = gh('repo view --json owner --jq .owner.login');
  log(`Repo owner: ${owner}`);
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

    gh(`issue create --title "${issue.title}" --body-file "${bodyFile}" --milestone "${milestone}" --assignee "${owner}"`);
    log(`  CREATED  "${issue.title}"`);

    try { unlinkSync(bodyFile); } catch { /* ignore */ }
  }

  log('');
  log('✅ Done. All initial SDD issues created.');
  log('   Next step: work through Phase 1 issues ([1a], [1b]), then fill in the [1c] spec questionnaire and comment @claude to proceed.');
}

run().catch(err => {
  console.error(err);
  process.exit(1);
});
