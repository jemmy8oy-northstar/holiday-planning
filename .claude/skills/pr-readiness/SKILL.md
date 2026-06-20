# PR Readiness Review Skill

Run this checklist **before raising a PR** against any backend changes. Work through each section, flag any failures, and fix them before opening the PR.

---

## 1. Naming Conventions

| Check | Rule |
|---|---|
| DataModels use plain nouns | `Ratio`, not `RatioResponse` — use `*Response` only if the route constructs a shape meaningfully different from what the service returns |
| `*Request` suffix | Only for inbound route bodies with non-trivial shape; never for simple query-string parameters |
| DomainModels prefixed `Domain*` | `DomainRatio`, `DomainIndicator`, `DomainDataPoint` — not plain nouns |
| Entities suffixed `*Entity` | Reserved for EF Core entity classes in `EntityModels/` |

---

## 2. Abstractions Project — Interfaces Only

- [ ] No concrete types (classes, records, structs) defined in `HolidayPlanning.Abstractions`
- [ ] Service interface methods return **interfaces**, not concrete types (`IRatio`, not `Ratio`)
- [ ] `HolidayPlanning.Abstractions.csproj` has **zero** `<ProjectReference>` entries to projects with concrete types

---

## 3. Inheritance & Extension Pattern

- [ ] Every `Domain*` class **extends** its `DataModels` counterpart (`DomainRatio : Ratio`, `DomainStatus : Status`)
- [ ] `DataModels` classes implement the corresponding `Abstractions` interface (`Ratio : IRatio`)
- [ ] Domain models are in `HolidayPlanning.DomainModels`, data models in `HolidayPlanning.DataModels`

---

## 4. Route Handlers

- [ ] Handlers are **named static methods**, not inline lambdas
- [ ] Return **typed results** — `Ok<T>`, `Results<Ok<T>, NotFound>`, etc. — never bare `IResult`
- [ ] Mapping from domain → data model done via **AutoMapper** (`IMapper`), not manual helper methods
- [ ] No business logic in routes — handlers only: receive input, call service, map, return

---

## 5. Service Layer

- [ ] Service methods return interface types (`IRatio`, `IIndicator?`)
- [ ] No HTTP or serialisation concepts in services (no `IResult`, no `JsonSerializer`)
- [ ] Bogus/faker logic lives in `Services/`, never in `WebApi/`

---

## 6. AutoMapper Profiles

- [ ] `WebApi/Mapper.cs` — contains `Profile` subclass mapping `Domain*` → DataModel (API boundary)
- [ ] `Services/Mapper.cs` — contains `Profile` subclass mapping `*Entity` → `Domain*` (DB boundary)
- [ ] Both profiles are discovered automatically via `cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies())` in `ServiceRegistration.cs`

---

## 7. Build Gate

```bash
cd backend && dotnet build
```

- [ ] **0 errors** before raising the PR
- [ ] Review any new warnings — suppress intentionally or fix
