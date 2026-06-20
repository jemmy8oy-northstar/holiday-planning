# Backend Architecture: SRP & the Orchestrator Pattern

This document covers the Single Responsibility Principle as applied to service design. These rules keep code readable, navigable, and testable — and make the codebase predictable for AI-assisted development.

For the 7-project layer structure, see `docs/specs/backend-architecture.md`.
For the testing strategy that this design enables, see `docs/specs/testing-strategy.md`.

---

## Single Responsibility Principle (SRP)

Every method should do exactly one thing. If you find yourself writing a method that fetches data *and* processes it *and* sends a notification, that is three methods.

The practical test: you can describe the method with a single verb phrase that contains no "and".

| Too broad | Single responsibility |
|---|---|
| `FetchAndProcessOrders` | `FetchOrders`, `ProcessOrder` |
| `ValidateAndSaveUser` | `ValidateUser`, `SaveUser` |
| `GenerateAndSendReport` | `GenerateReport`, `SendReport` |

---

## The Orchestrator Pattern

Complex business workflows are expressed as **orchestrator methods** — high-level methods that read like a plain-English description of the business process, delegating each step to a single-responsibility service.

```csharp
public async Task ProcessMonthlyReportAsync(Guid organisationId)
{
    var data       = await _reportDataService.FetchAsync(organisationId);
    var recipients = await _contactsService.GetRecipientsAsync(organisationId);
    var report     = _reportGenerator.Generate(data);
    var email      = _emailBuilder.BuildReportEmail(report, recipients);
    await _emailService.SendAsync(email);
}
```

Notice:
- The method name describes the business intent
- Each line is a single, named responsibility
- The method is self-documenting — no comments needed
- Every dependency is injected and named by what it does
- The orchestrator contains **no business logic** — it only coordinates

This is the target for any method that coordinates more than one concern. Business logic lives inside the called services, not in the orchestrator.

---

## Service Naming & Folder Structure

Services are named by their single responsibility and grouped by domain area:

```
HolidayPlanning.Services/
├── Reporting/
│   ├── ReportDataService.cs      # Fetches raw data for reports
│   ├── ReportGenerator.cs        # Transforms data into a report model
│   └── ReportEmailBuilder.cs     # Builds the email payload from a report
├── Notifications/
│   ├── EmailService.cs           # Sends emails via the gateway
│   └── SmsService.cs
└── Users/
    ├── UserFetchService.cs
    └── UserValidationService.cs
```

Good service names make navigation trivial — the folder structure is a map of the domain, and you always know where to look.

Each service has a corresponding interface in `HolidayPlanning.Abstractions/Services/` following the same folder grouping.

---

## Why This Matters for AI Development

When services are small and named by responsibility:
- The AI can locate the correct service immediately from a description of the task
- Adding a new responsibility means creating a new, clearly named service — not expanding an existing one
- The call graph reads like a requirements document
- Bugs are isolated: if `ReportGenerator` has a bug, you know exactly where to look

When methods do too much, the AI (and the developer) must read the entire method body to understand what it does. When methods do one thing, the name is sufficient.

---

## Dependency Injection

Every service dependency is constructor-injected. No service locator, no static access, no `new` inside a service body.

```csharp
public class MonthlyReportOrchestrator(
    IReportDataService reportDataService,
    IContactsService contactsService,
    IReportGenerator reportGenerator,
    IEmailBuilder emailBuilder,
    IEmailService emailService)
{
    // ...
}
```

All services are registered in `ServiceRegistration.cs`. The registration mirrors the folder structure — if a service is in `Services/Reporting/`, its registration is grouped with other Reporting services.
