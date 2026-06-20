# Testing Strategy

This document covers the testing approach across the full stack. The strategy is designed to give fast feedback at every level without the overhead of real infrastructure.

For the SRP/orchestrator design that makes this testable, see `docs/specs/backend-srp.md`.
For when tests are written relative to the development workflow, see `docs/specs/sdd-workflow.md`.

---

## Test Pyramid

```
         [ Frontend: Vitest + RTL ]     ← component behaviour
        [  Unit: xUnit + Moq      ]     ← single service in isolation
     [ In-Process Integration     ]     ← real services wired, I/O mocked
```

No test database. No containers. No infrastructure dependencies.

---

## Backend Unit Tests (xUnit + Moq)

Unit tests cover a single service class with all dependencies mocked. They verify that the class behaves correctly in isolation.

```csharp
public class MonthlyReportOrchestratorTests
{
    private readonly Mock<IReportDataService> _reportDataService = new();
    private readonly Mock<IContactsService> _contactsService = new();
    private readonly Mock<IReportGenerator> _reportGenerator = new();
    private readonly Mock<IEmailBuilder> _emailBuilder = new();
    private readonly Mock<IEmailService> _emailService = new();

    private MonthlyReportOrchestrator CreateSut() => new(
        _reportDataService.Object,
        _contactsService.Object,
        _reportGenerator.Object,
        _emailBuilder.Object,
        _emailService.Object
    );

    [Fact]
    public async Task ProcessMonthlyReport_SendsEmailToAllRecipients()
    {
        // Arrange
        var orgId      = Guid.NewGuid();
        var data       = new ReportData();
        var recipients = new List<Recipient> { new("a@b.com") };
        var report     = new Report();
        var email      = new Email();

        _reportDataService.Setup(x => x.FetchAsync(orgId)).ReturnsAsync(data);
        _contactsService.Setup(x => x.GetRecipientsAsync(orgId)).ReturnsAsync(recipients);
        _reportGenerator.Setup(x => x.Generate(data)).Returns(report);
        _emailBuilder.Setup(x => x.BuildReportEmail(report, recipients)).Returns(email);

        // Act
        await CreateSut().ProcessMonthlyReportAsync(orgId);

        // Assert
        _emailService.Verify(x => x.SendAsync(email), Times.Once);
    }
}
```

**Conventions:**
- One test class per service
- `CreateSut()` factory keeps construction in one place
- Each test verifies one behaviour
- Mock field names match the injected interface names

---

## In-Process Integration Tests

These tests wire **real service implementations** together via DI, mocking only at the external I/O boundary (database, email gateway, HTTP clients). No test database is needed.

This is distinct from unit tests (where all dependencies are mocked) and from E2E tests (where the full stack including infrastructure is real). The goal is to verify that the orchestration is correct when real implementations are plugged in — catching bugs that mocks hide, such as mapping errors or interface contract mismatches between services.

```csharp
public class MonthlyReportIntegrationTests
{
    [Fact]
    public async Task ProcessMonthlyReport_CallsEmailGatewayWithCorrectPayload()
    {
        // Real implementations, DI-wired
        var emailGateway = new Mock<IEmailGateway>(); // only the external boundary is mocked

        var services = new ServiceCollection()
            .AddScoped<IReportDataService, ReportDataService>()
            .AddScoped<IContactsService, ContactsService>()
            .AddScoped<IReportGenerator, ReportGenerator>()
            .AddScoped<IEmailBuilder, ReportEmailBuilder>()
            .AddSingleton(emailGateway.Object)
            .BuildServiceProvider();

        var orchestrator = services.GetRequiredService<MonthlyReportOrchestrator>();

        await orchestrator.ProcessMonthlyReportAsync(TestData.OrgId);

        emailGateway.Verify(x => x.SendAsync(It.Is<EmailPayload>(e =>
            e.Recipients.Count == 1 &&
            e.Subject.Contains("Monthly Report")
        )), Times.Once);
    }
}
```

**What these tests catch:**
- Bugs in the orchestration that mocks hide (e.g. arguments passed in wrong order)
- Mapping errors between services (e.g. a field used by one service not populated by another)
- Regression: once passing, these protect against breaking changes to the call chain

**What these tests do not cover:**
- Individual service logic (that's unit tests)
- Database persistence (out of scope — no test DB)
- UI behaviour (that's frontend tests)
- External APIs (stub at the gateway boundary)

### Scenario-First, Implement Last

In-process integration test **scenarios** are defined during architecture design (SDD Phase 5) — before any implementation. The scenarios capture what end-to-end workflows must succeed. The actual test implementation is deferred to the end of Phase 6, once the full service dependency tree is built and you know exactly what needs to be wired.

**Scenario (defined in Phase 5):**
```
Given an organisation with active subscribers
When ProcessMonthlyReportAsync is called
Then the email gateway receives a correctly formatted report email
And the email is addressed to all active subscribers
```

**Test implementation (end of Phase 6):** — as shown above.

---

## Top-Down TDD Flow

```
Phase 5:  Define in-process integration test scenarios (what, not how)
Phase 6:  ┌─ Write unit test for the orchestrator (all deps mocked)
          │  Implement orchestrator → tests pass
          │
          ├─ Write unit tests for each called service
          │  Implement each service → tests pass
          │
          ├─ Continue down to leaves (data services, builders, etc.)
          │
          └─ Implement in-process integration tests
             Wire real services, mock only external I/O → tests pass
```

**Why top-down?**
- You define the contract at each level before worrying about implementation
- Orchestrator-level unit tests pass immediately — mocks return expected data, so you get green feedback before any dependent service exists
- Each level is testable in isolation regardless of what is below it
- The final in-process integration tests verify the full orchestration and serve as a regression safety net

---

## Frontend Tests (Vitest + React Testing Library)

Frontend tests follow the same spec-first philosophy: write the test before the component, describe behaviour from the user's perspective, not implementation details.

```tsx
// Navbar.test.tsx
describe('Navbar', () => {
  it('highlights the active route link', () => {
    render(<Navbar />, { wrapper: routerWrapper('/boards') });
    expect(screen.getByRole('link', { name: 'Boards' })).toHaveAttribute('data-active', 'true');
  });
});
```

**Use Vitest** — it is native to Vite and requires no additional configuration.

A frontend unit test is an executable spec step. Writing it first forces you to define what "done" looks like for the component before writing any JSX.
