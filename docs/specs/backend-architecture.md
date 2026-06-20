# Backend Architecture

This document explains the structure and rationale behind the 7-project .NET backend pattern used across all projects scaffolded from this template.

## Project Structure

```
HolidayPlanning/
├── HolidayPlanning.WebApi         # Entry point: routes, DI, OpenAPI, middleware
│   ├── Routes/                 # Minimal API route groups (one file per domain area)
│   ├── Mapper.cs               # DataModel ↔ DomainModel mappings
│   └── ServiceRegistration.cs  # All DI registrations in one place
├── HolidayPlanning.Services       # Business logic implementations
│   └── Mapper.cs               # EntityModel ↔ DomainModel mappings
├── HolidayPlanning.Abstractions   # Interfaces only — no implementations
│   ├── DataModels/             # IStatus, ISubscriber, etc.
│   ├── DomainModels/           # IDomainStatus, IDomainSubscriber, etc.
│   └── Services/               # IStatusService, IInterestService, etc.
├── HolidayPlanning.Database       # EF Core DbContext + migrations
├── HolidayPlanning.EntityModels   # Database entity classes (anemic POCOs)
├── HolidayPlanning.DomainModels   # Rich business-layer objects
└── HolidayPlanning.DataModels     # Request/Response DTOs (public API contract)
```

## Dependency Direction

```
WebApi → Services → Database
WebApi → Abstractions ← Services
WebApi → DataModels
Services → EntityModels
Services → DomainModels
```

`WebApi` never imports `Services` directly — only interfaces from `Abstractions`. This keeps the API layer decoupled from business logic implementations.

## Core Principles

### 1. Single Responsibility per Layer

Each project does exactly one thing:
- `EntityModels`: what the database looks like
- `DomainModels`: what the business logic works with
- `DataModels`: what the API exposes to clients
- `Services`: how to transform between them and apply logic
- `Database`: how to persist and query
- `WebApi`: how to route, authenticate, and document

### 2. Layered Mapping Isolation

Two separate AutoMapper profiles keep mappings small and focused:
- `WebApi/Mapper.cs` — maps `DataModel ↔ DomainModel` (API boundary)
- `Services/Mapper.cs` — maps `EntityModel ↔ DomainModel` (DB boundary)

AutoMapper is configured in `ServiceRegistration.cs` to scan all assemblies for `Profile` subclasses automatically.

### 3. Interface-Driven Development

`WebApi` only talks to service interfaces (`IStatusService`, etc.) defined in `Abstractions`. This means:
- The API layer never knows about EF Core, Npgsql, or concrete implementations
- Services can be swapped or mocked without touching routes
- The compile-time contract is clear

### 4. Model Sovereignty: Four Model Types

| Model | Naming | Location | Purpose |
|---|---|---|---|
| Request | `*Request` | `DataModels/` | What the client sends (minimal fields, no server-managed properties) |
| Data Model | Simple noun | `DataModels/` | What the API returns to clients |
| Domain Model | `Domain*` | `DomainModels/` | Internal rich model used by service layer; extends its DataModels counterpart |
| Entity | `*Entity` | `EntityModels/` | Direct EF Core database mapping |

**Never include server-managed fields** (`Id`, `CreatedAt`, `IsVerified`) in `*Request` models. This prevents over-posting attacks.

**Naming rules for `*Request` / `*Response`:**
- Use `*Request` only for inbound route arguments that have non-trivial shape (e.g. a POST body with validation). Simple query-string parameters do not need a request wrapper.
- Use `*Response` only when the route constructs a return shape that is **meaningfully different** from what the service returns — i.e. it aggregates, renames, or adds fields. If the route simply maps a domain model 1-to-1 to a data model, the data model should use a plain noun (`Ratio`, not `RatioResponse`).
- `Domain*` models extend their `DataModels/` counterpart (e.g. `DomainRatio : Ratio`) and add business-layer behaviour. They are never returned directly by routes.

### 5. Route Grouping (Minimal APIs)

Routes are organized into `Routes/` and registered via extension methods:

```csharp
// Program.cs
app.MapGroup("/api")
    .MapStatusRoutes()
    .MapInterestRoutes()
    .WithOpenApi();

// Routes/StatusRoutes.cs
public static class StatusRoutes
{
    public static RouteGroupBuilder MapStatusRoutes(this RouteGroupBuilder group)
    {
        group.MapGet("/status", async (IStatusService svc) => await svc.GetStatusAsync());
        return group;
    }
}
```

This keeps `Program.cs` lean regardless of how many endpoints are added.

### 6. OpenAPI as the Source of Truth

The backend generates an OpenAPI schema at `/openapi/v1.json`. The frontend RTK Query client is generated directly from this schema — no manual HTTP calls, no drifting types. See `docs/specs/openapi-codegen.md`.

### 7. Auto-Run Migrations

`Program.cs` runs pending EF migrations on startup:

```csharp
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}
```

This means a fresh deployment always reaches the correct schema without manual intervention. Acceptable for solo/small-team projects; revisit for high-availability deployments.

## Adding a New Feature (Checklist)

1. Add `*Entity` to `EntityModels/`, add `DbSet<>` to `DbContext`, create migration
2. Add `Domain*` to `DomainModels/`
3. Add `I*` interface to `Abstractions/DataModels/` and `Abstractions/DomainModels/`
4. Add `*` data model and `*Request` to `DataModels/`
5. Add `I*Service` interface to `Abstractions/Services/`
6. Implement `*Service` in `Services/`, add `EntityModel ↔ DomainModel` mappings in `Services/Mapper.cs`
7. Register service in `ServiceRegistration.cs`
8. Add `DataModel ↔ DomainModel` mappings in `WebApi/Mapper.cs`
9. Add route group in `Routes/*Routes.cs`, register in `Program.cs`
10. Run `npm run codegen` in `frontend/` to regenerate typed hooks
