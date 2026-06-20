# Personal Web Template

A high-performance monorepo template for .NET backends and React frontends, built on Clean Architecture principles.

## Stack

- **Backend**: .NET 10, Entity Framework Core, PostgreSQL, Scalar/OpenAPI
- **Frontend**: React, Vite, TypeScript, Redux Toolkit + RTK Query
- **Infrastructure**: Docker, Helm, Kubernetes

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/)
- [PostgreSQL](https://www.postgresql.org/) (local or Docker)
- `dotnet new` CLI

## Creating a New Project

### 1. Install the Template

```bash
dotnet new install /path/to/web-template
```

Or once published to NuGet:

```bash
dotnet new install Balenthiran.WebTemplate
```

### 2. Scaffold Your Project

Pass your solution name as `Company.ProjectName`:

```bash
dotnet new web-template -n Balenthiran.Apeify
```

This will generate:

```
Balenthiran.Apeify/
├── backend/
│   ├── Balenthiran.Apeify.Abstractions/
│   ├── Balenthiran.Apeify.DataModels/
│   ├── Balenthiran.Apeify.DomainModels/
│   ├── Balenthiran.Apeify.EntityModels/
│   ├── Balenthiran.Apeify.Services/
│   ├── Balenthiran.Apeify.Database/
│   ├── Balenthiran.Apeify.WebApi/
│   └── Balenthiran.Apeify.slnx
├── frontend/
├── scripts/
├── helm/
└── deploy.sh
```

### 3. Run the Onboarding Script

```bash
cd Balenthiran.Apeify
node scripts/init.mjs
```

This will:
- Prompt for your PostgreSQL connection details
- Generate `appsettings.Development.json` with a fresh JWT secret
- Generate `frontend/.env` with a unique project ID
- Run `dotnet restore` and `npm install`

### 4. Apply the Initial Migration

```bash
cd backend
dotnet ef database update --project Balenthiran.Apeify.Database --startup-project Balenthiran.Apeify.WebApi
```

### 5. Run the App

**Backend:**
```bash
cd backend
dotnet run --project Balenthiran.Apeify.WebApi
```

**Frontend:**
```bash
cd frontend
npm run dev
```

API docs available at: `http://localhost:5000/scalar/v1`

## Running Locally

> This section is worth keeping when you replace this README with your project spec.

### Prerequisites

`appsettings.Development.json` is gitignored (it contains secrets). You need it before the backend will start. Either run the onboarding script which generates it for you:

```bash
node scripts/init.mjs
```

Or create it manually at `backend/HolidayPlanning.WebApi/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=your_db;Username=your_user;Password=your_password"
  },
  "Jwt": {
    "Secret": "your-secret-key-min-32-chars-long"
  }
}
```

Similarly, `frontend/.env` is gitignored. Create it manually at `frontend/.env` if needed:

```
VITE_API_URL=http://localhost:5000
```

### Start the backend

```bash
cd backend
dotnet run --project HolidayPlanning.WebApi
```

API runs at `http://localhost:5000` — interactive docs at `http://localhost:5000/scalar/v1`.

### Start the frontend

```bash
cd frontend
npm run dev
```

App runs at `http://localhost:5173`. The Vite dev server proxies `/api` and `/openapi` to the backend automatically.

### Regenerate the API client

Run this after any backend endpoint change to keep the frontend types in sync:

```bash
cd frontend
npm run codegen
```

See `docs/specs/openapi-codegen.md` for the full workflow.

---

## Initialise as a New Git Repo

```bash
cd Balenthiran.Apeify
git init
git add .
git commit -m "Initial commit from web-template"
# Add your remote and push
```

## Kubernetes Deployment

### 1. Create the K8s Namespace

```bash
kubectl create namespace your-app
```

> This must match `KUBERNETES_NAMESPACE` in `deploy.sh` and `-n` in the commands below.

### 2. Provision a PostgreSQL Database

Create a PostgreSQL database on your cloud provider (e.g. OCI, AWS RDS, Supabase). Note down the connection string in the format:

```
Host=your-host;Database=your_db;Username=your_user;Password=your_password
```

### 3. Create the K8s Secret

```bash
kubectl create secret generic your-app-secrets \
  --from-literal=DATABASE_URL="Host=your-host;Database=your_db;Username=your_user;Password=your_password" \
  -n your-app
```

> **Important**: The secret name (`your-app-secrets`) must match `secretKeyRef.name` in `helm/values.yaml`.

### 4. Update Configuration

Update these two files before deploying:

**`helm/values.yaml`** — set `fullnameOverride`, domain, registry, and secret name.

**`deploy.sh`** — set `APP_NAME` to match `fullnameOverride` in `values.yaml`, and update `REGISTRY_NAMESPACE` and `COMPARTMENT_ID`.

### 5. Deploy

```bash
./deploy.sh
```

This will build and push Docker images, then trigger a rolling restart of both deployments.

## New Project Checklist

After scaffolding with `dotnet new web-template -n Company.ProjectName`, work through this list:

### Local setup
- [ ] Run `node scripts/init.mjs` — generates `appsettings.Development.json` and `frontend/.env`
- [ ] Run the initial EF migration: `dotnet ef database update --project *.Database --startup-project *.WebApi`
- [ ] Verify the app starts: backend on `http://localhost:5000`, frontend on `http://localhost:5173`

### Branding / placeholders
- [ ] `frontend/src/components/Hero.tsx` — replace "Your App Name" and the subtitle
- [ ] `frontend/src/App.tsx` — replace "Your Name Here" in the footer
- [ ] `frontend/src/components/Navbar.tsx` — replace "App Name" in the mobile logo
- [ ] `frontend/src/data/config.json` — add any additional nav routes
- [ ] `frontend/index.html` — update `<title>`

### Helm + deploy script
- [ ] `helm/values.yaml` — set `fullnameOverride`, `ingress.hosts[0].host`, and image repository paths
- [ ] `deploy.sh` — set `APP_NAME`, `REGION`, `REGISTRY_NAMESPACE`, `COMPARTMENT_ID`, `KUBERNETES_NAMESPACE`

### GitHub Actions
- [ ] **Private repo?** Consider removing `ci.yml` — it runs on every PR and counts against the 2,000 free minutes/month. Delete the file or disable it in the repository settings (Settings → Actions → General). The deploy workflow (`docker-build-push.yml`) is manual-only so it only runs when you trigger it, making it less of a concern.
- [ ] **Automated deploys on merge to main?** By default `docker-build-push.yml` is manual-only. To trigger it automatically on merge to main, add `push: branches: [main]` to the `on:` block:
  ```yaml
  on:
    push:
      branches:
        - main
    workflow_dispatch:
  ```
- [ ] `.github/workflows/docker-build-push.yml` — set `FRONTEND_IMAGE` and `BACKEND_IMAGE` env vars to match `helm/values.yaml`
- [ ] Repository **Variables** (Settings → Secrets and variables → Variables):
  - `OCIR_REGISTRY` — e.g. `lhr.ocir.io`
  - `OCIR_NAMESPACE` — your OCI tenancy namespace
- [ ] Repository **Secrets** (Settings → Secrets and variables → Secrets):
  - `OCIR_USERNAME` — e.g. `your-tenancy/oracleidentitycloudservice/your@email.com`
  - `OCIR_AUTH_TOKEN` — OCI auth token (OCI Console → User Settings → Auth Tokens)

### Kubernetes
- [ ] `kubectl create namespace your-app`
- [ ] Provision a PostgreSQL database and note the connection string
- [ ] `kubectl create secret generic your-app-secrets --from-literal=DATABASE_URL="..." -n your-app`

### Git & GitHub repository settings
- [ ] `git init && git add . && git commit -m "Initial commit from web-template"`
- [ ] Create `main` and `dev` branches — all agent work branches off `dev`, PRs target `dev`, `main` is developer-only
- [ ] Add remote and push
- [ ] Enable **"Automatically delete head branches"** (Settings → General) — GitHub will delete feature branches after a PR merges, safely, without any agent involvement

### SDD bootstrap — create initial issues
- [ ] Run `node scripts/init-issues.mjs` — creates all Phase 1–7 orchestrator issues with the `MVP` milestone and ensures the three workflow labels exist. This is the first action the AI takes after the repo is pushed. See [`docs/ai-workflow.md`](docs/ai-workflow.md) for the full issue structure and label-driven workflow.

### Documentation
- [ ] Overwrite this `README.md` with your project's business spec — what the product is, who it's for, and what problem it solves. Keep the **Running Locally** section (or adapt it) so contributors know how to get started
- [ ] Flesh out `docs/specs/` with feature specs before writing code — define the data model, API contracts, and UI behaviour up front
- [ ] Update `CLAUDE.md` to reflect your project's specific conventions, data files, and any decisions made during setup
- [ ] Delete any `docs/specs/` files from the template that don't apply to your project

---

## GitHub Actions

Two workflows are included in `.github/workflows/`:

| Workflow | Trigger | Purpose |
|---|---|---|
| `ci.yml` | Pull requests only | Builds backend and frontend to catch compile errors — uses `ubuntu-latest` (unlimited on public repos, counts against the 2,000 min/month free allowance on private repos) |
| `docker-build-push.yml` | Manual (`workflow_dispatch`) | Builds ARM64 Docker images and pushes to OCI Container Registry |

### ARM64 Runner Note

The deploy workflow uses `ubuntu-24.04-arm` (native ARM64, required for OKE free tier). This runner is **free for public repositories**. For private repositories it requires a paid GitHub plan — see the comment at the top of `docker-build-push.yml` for the `ubuntu-latest` + QEMU alternative.

## Project Structure

| Layer | Project | Responsibility |
|---|---|---|
| API | `*.WebApi` | Routes, DI, OpenAPI, Middleware |
| Services | `*.Services` | Business logic, AutoMapper |
| Abstractions | `*.Abstractions` | Interfaces, DTOs, Contracts |
| Database | `*.Database` | EF Core DbContext, Migrations |
| Entity Models | `*.EntityModels` | Database entities |
| Domain Models | `*.DomainModels` | Business-layer objects |
| Data Models | `*.DataModels` | Request/Response DTOs |
