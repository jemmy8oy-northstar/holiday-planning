# Project Rules

## Branch Strategy

```
main  ← developer only, production-ready, never touched by the agent
 └── dev  ← integration branch, PRs are merged here by the developer
      └── {issue-number}-{description}  ← all agent work happens here
```

- **`main`**: managed by the developer only. Never commit or push to `main`.
- **`dev`**: the integration branch. Never commit or push directly to `dev`. All work reaches `dev` via a PR.
- **Feature branches**: always branch off `dev`, named `{issue-number}-{short-kebab-description}` (e.g. `42-user-registration-form`). All PRs target `dev`.
- **Branch deletion**: do not delete branches manually. The repository is configured to delete head branches automatically after a PR is merged.

## GitHub Workflow

The developer interfaces via GitHub only — they do not have access to the local machine. All progress must be visible via commits and PRs on the remote.

- When picking up a GH issue, follow the "Implementing an Issue" steps in `CLAUDE.md`
- When responding to PR review comments, follow the "Responding to PR Reviews" steps in `CLAUDE.md`
- Never leave work uncommitted and unpushed at the end of a session
- Never merge your own PR

## Communication

All discussion, questions, and remarks happen in the PR — not only in chat responses.

- If you have a question about the work, post it as a comment on the relevant PR before waiting for a response
- If you want to flag a concern, decision, or trade-off, comment on the relevant line or post a general PR comment
- If you are making a design decision that is not explicitly covered by the spec, note it in a PR comment so the developer can review it
- A chat reply alone is not enough — the PR is the record of what was agreed and why

## Documentation

- Update `CLAUDE.md` any time the project structure, stack, or running instructions change.
- Update `docs/specs/` when an architectural decision or workflow changes significantly.
- Update `.agents/rules/project.md` (this file) when a new coding convention or behavioural rule is agreed.
- Do not create new doc files for one-off notes — commit messages are the right place for those.
- If any doc describes something that no longer matches reality, fix it in the same session it was found.

## Backend

- Follow the 7-layer architecture — see `docs/specs/backend-architecture.md` for the full structure and dependency rules.
- Follow SRP and the orchestrator pattern — see `docs/specs/backend-srp.md`.
- All new API endpoints must be registered within the `.WithOpenApi()` chain in `Program.cs`.
- Request models (`*Request`) must never include server-managed fields (`Id`, `CreatedAt`, `IsVerified`).
- Routes live in `Routes/` as extension methods — do not add routes directly in `Program.cs`.
- Service methods accept and return interfaces, not concrete types.
- After adding or changing a backend endpoint, run `npm run codegen` in `frontend/` to regenerate `generatedApi.ts`.

## Frontend

- Never hand-edit `src/api/generatedApi.ts` — it is always overwritten by `npm run codegen`.
- Custom endpoints not covered by the OpenAPI schema go in a separate file (e.g. `src/api/customApi.ts`).
- Components should be small and focused.
- **Styling**: use SCSS. Each component gets a co-located `.scss` file (e.g. `Navbar.tsx` + `Navbar.scss`). Shared design tokens, resets, and utilities live in `src/styles/`. Do not write component-specific styles in global files, and do not use inline styles where a class exists.

## Testing

- Follow the testing strategy in `docs/specs/testing-strategy.md`.
- Write tests before implementation (TDD).
- Unit tests: one class at a time, all dependencies mocked.
- In-process integration tests: real service implementations wired via DI, only external I/O mocked — no test database.
- Frontend: Vitest + React Testing Library.

## Infrastructure

- `deploy.sh` and `helm/values.yaml` contain app-specific values — flag these to the developer when onboarding a new project.
- `docker-build-push.yml` is triggered manually only (`workflow_dispatch`).
- `ci.yml` runs on pull requests only and validates that the code builds — it does not push images.
