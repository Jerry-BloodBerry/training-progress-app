# Backend Architecture Rules — .NET 8 / FastEndpoints / PostgreSQL

## Summary

This document governs all architectural, structural, and process decisions for the backend
of the training-progress-app. It defines how the three-project .NET 8 solution must be
organised, how endpoints are implemented, how data access is handled, how security is enforced,
and what quality gates must be met after every iteration. All agents performing tasks that
touch backend code — including the API, Core, or Test projects — must read and follow these
rules before making any changes.

---

## Description

The backend is a **.NET 8 solution** built on a **clean hexagonal architecture** (also known
as Ports & Adapters). The solution has exactly three projects with strictly enforced dependency
boundaries. The **Core** project contains everything that is not HTTP-specific: domain
entities, ports, application use cases, adapter implementations (e.g. EF Core repositories),
entity configurations, the `DbContext`, and DI registration helpers. The **API** project is
a thin consumer layer — it defines endpoints, DTOs, and wires up the services registered by
Core. The **Tests** project validates both layers.

The API layer uses **FastEndpoints** for defining HTTP endpoints and exposes an **OpenAPI
specification** (via Swagger) that the Angular frontend consumes to generate TypeScript types.
Data persistence uses **PostgreSQL** accessed via **Entity Framework Core**. Authentication
and authorisation are handled exclusively by **Clerk** — no other auth mechanism may be
introduced.

After every iteration (feature, fix, or refactor) the agent must perform a built-in **code
review** and **security check** before marking the task done. Vulnerabilities that can be
fixed must be fixed immediately; those that cannot must be listed with their CVSS score.

---

## Technologies

| Technology | Version / Package | Role |
|---|---|---|
| .NET | 8 | Runtime and language platform |
| C# | 12 | Language |
| FastEndpoints | latest stable | HTTP endpoint definition (replaces controllers) |
| Entity Framework Core | latest stable (.NET 8 compat) | ORM for PostgreSQL |
| Npgsql EF Core Provider | latest stable | PostgreSQL EF Core driver |
| PostgreSQL | latest stable | Primary database |
| Clerk C# SDK | [`clerk/clerk-sdk-csharp`](https://github.com/clerk/clerk-sdk-csharp) | Sole auth/identity provider |
| Swashbuckle / NSwag | latest stable | OpenAPI spec generation |
| xUnit | latest stable | Test framework |
| Testcontainers for .NET | latest stable | Integration test infrastructure |
| coverlet | latest stable | Code coverage instrumentation |
| Docker / docker-compose | current | Containerised local environment |

---

## Rules

### 1. Project Structure and Dependency Boundaries

The solution contains exactly **three projects**:

| Project | Responsibility |
|---|---|
| `Core` | Domain entities, value objects, ports (interfaces), domain services, application use cases, adapter implementations (repositories, etc.), EF Core `DbContext`, entity type configurations, DI registration extension methods |
| `API` | FastEndpoints endpoints, request/response DTOs, Clerk middleware integration, OpenAPI configuration, application entry point |
| `Tests` | xUnit test suites, Testcontainers fixtures, coverage setup |

Dependency rules are **absolute** and must never be violated:

- `Core` **must not** reference `API` or `Tests`.
- `API` **must not** reference `Tests`.
- `Tests` **may** reference both `API` and `Core`.

Enforce these rules with `<ProjectReference>` entries only in the correct direction. If a
circular dependency appears, stop and redesign before continuing.

### 2. Hexagonal Architecture (Ports & Adapters)

- **Domain logic** belongs in `Core`. No HTTP-specific types or ASP.NET Core attributes
  should appear in `Core`.
- **Ports** (interfaces) are defined in `Core` and describe what the domain needs from the
  outside world (e.g. `ITrainingRepository`, `IUserContext`).
- **Adapters** (implementations) also live in `Core` and fulfil the ports (e.g. EF Core
  repositories). The `API` project must not implement any adapter logic.
- The `API` project's only responsibilities are: defining endpoints (FastEndpoints), defining
  request/response DTOs, integrating Clerk middleware, and calling DI registration methods
  exposed by `Core`.
- Use constructor injection throughout. Never resolve services via the service locator pattern
  (`IServiceProvider` direct calls inside domain code).
- Application use cases / command handlers are plain C# classes in `Core` — do **not** use
  MediatR or any paid/licensed pipeline library.

### 3. FastEndpoints

- Every HTTP endpoint is a class that extends `Endpoint<TRequest, TResponse>` (or the
  appropriate FastEndpoints base class for the use case).
- One endpoint per file; file name mirrors the endpoint class name in PascalCase.
- Group endpoints by feature folder under `API/Features/<FeatureName>/`.
- Validators are co-located with their endpoint using FastEndpoints' built-in
  `Validator<TRequest>`.
- Do not use ASP.NET Core `[ApiController]` or MVC controllers.
- Map HTTP status codes explicitly using FastEndpoints' response helpers (`ThrowError`,
  `SendAsync`, `SendNotFoundAsync`, etc.) — do not return raw `IActionResult`.

### 4. OpenAPI and TypeScript Type Generation

- Swagger/OpenAPI must be configured to produce a spec compatible with the frontend's
  TypeScript generator.
- All request and response DTOs must have explicit, descriptive names (no anonymous types).
- Decorate DTOs with XML doc comments or Swagger attributes so the generated spec is
  self-documenting.
- The OpenAPI spec file is the **contract** between backend and frontend — breaking changes
  to existing endpoints require explicit communication and a versioning strategy.

### 5. Entity Framework Core and PostgreSQL

- `DbContext` lives in `Core` alongside the rest of the persistence infrastructure.
- Define entity configurations using `IEntityTypeConfiguration<T>` in separate classes under
  `Core/Persistence/Configurations/` — do not configure entities in `OnModelCreating`
  directly. Apply them by calling `modelBuilder.ApplyConfigurationsFromAssembly()` inside the
  `DbContext`, or via the DI registration class (see below).
- `Core` must expose a static DI extension method (e.g.
  `IServiceCollection.AddCoreServices(IConfiguration)`) that registers the `DbContext`,
  repositories, and all other infrastructure services. `API` calls this method in its
  composition root and must not duplicate or override those registrations.
- Use EF Core **migrations** for all schema changes. Never modify the database manually.
  Migrations are generated from the `Core` project (or a dedicated migrations project) and
  applied at startup by `API`.
- Repository adapters in `Core` implement the port interfaces declared in `Core`.
- Queries must be written to avoid N+1 problems — use `Include`/`ThenInclude` or
  projection (`Select`) deliberately.
- Connection strings must come from environment variables or a secrets manager — never
  hard-coded.

### 6. Authentication and Security (Clerk Only)

- **Clerk** is the sole permitted authentication and authorisation solution. Use the
  official [`clerk/clerk-sdk-csharp`](https://github.com/clerk/clerk-sdk-csharp) SDK.
- Do not introduce JWT middleware, ASP.NET Core Identity, OAuth libraries, or any custom
  token validation logic.
- Protect endpoints with Clerk's middleware and policy-based authorisation as documented in
  the SDK.
- Never log, store, or expose session tokens, API keys, or Clerk secret keys in code,
  comments, or version control.
- All Clerk configuration (secret key, publishable key) must be read from environment
  variables at runtime.

### 7. Dependency Management

The following packages are **pre-approved** and may be added without asking the user:

| Package | Purpose |
|---|---|
| `FastEndpoints` | Endpoint framework |
| `FastEndpoints.Swagger` | OpenAPI integration for FastEndpoints |
| `Microsoft.EntityFrameworkCore` | ORM |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | PostgreSQL driver |
| `Clerk.BackendAPI` (clerk-sdk-csharp) | Authentication |
| `xUnit` | Test framework |
| `xUnit.runner.visualstudio` | Test runner integration |
| `Testcontainers.PostgreSql` | Integration test containers |
| `coverlet.collector` | Coverage collection |
| `Microsoft.NET.Test.Sdk` | Test SDK |

**For any other package, the agent must ask the user for approval before adding it.**
State the package name, version, reason, and any known licence implications.

### 8. SOLID, KISS, and DRY Principles

- **Single Responsibility**: each class has one reason to change. Split classes that grow
  beyond a single cohesive purpose.
- **Open/Closed**: extend behaviour through new classes or configuration, not by modifying
  existing stable code.
- **Liskov Substitution**: any implementation of a port interface must be substitutable
  without changing the correctness of callers.
- **Interface Segregation**: define narrow, focused interfaces. Avoid fat interfaces that
  force implementors to provide no-op methods.
- **Dependency Inversion**: high-level modules (use cases in `Core`) depend on abstractions,
  not on concrete infrastructure types.
- **KISS**: prefer the simplest correct solution. Avoid speculative abstractions.
- **DRY**: extract shared logic into a single place; do not copy-paste business rules across
  endpoints or use cases.

### 9. Code Review and Security Check After Every Iteration

After completing every feature, fix, or refactor the agent **must** perform the following
before marking the task done:

1. **Code review**: Check for SOLID violations, dead code, N+1 queries, missing validation,
   and incorrect status codes.
2. **Security check**: Scan changed code for OWASP Top 10 vulnerabilities, insecure
   deserialization, over-posting, mass assignment, missing authorisation checks, and exposed
   secrets.
3. **Fix immediately**: Any vulnerability that can be fixed within the scope of the current
   change must be fixed before the task is marked done.
4. **Report remaining issues**: Any vulnerability that cannot be fixed immediately must be
   listed in the task output with:
   - A short description of the issue.
   - The CVSS v3.1 base score and vector string.
   - A recommended remediation path.

### 10. Docker Compose

- The `docker-compose.yml` at the solution root must allow the full backend stack (API +
  PostgreSQL) to start with a single `docker compose up` command.
- The API service must read all configuration from environment variables defined in
  `docker-compose.yml` or an `.env` file (never hard-coded).
- The `.env` file must be listed in `.gitignore` — a `.env.example` with placeholder values
  must be committed instead.
- Health checks must be configured for the PostgreSQL service so the API waits for the
  database to be ready before starting.

### 11. Testing

- Every endpoint must have at least one **integration test** that exercises the full HTTP
  stack using Testcontainers (a real PostgreSQL container).
- **Unit tests** are required for non-trivial domain logic and use case classes in `Core`
  (i.e. anything with branching, calculation, or validation logic).
- Tests are organised to mirror the production code structure:
  - `Tests/Integration/Features/<FeatureName>/` for endpoint tests.
  - `Tests/Unit/<FeatureName>/` for unit tests.
- Use `coverlet` to collect coverage. The coverage report must be generated as part of CI.
  Aim for ≥ 80 % line coverage on the `Core` project.
- Do not write tests for trivial getters/setters, EF Core migrations, or auto-generated code.

### 12. Module Structure Within Core (Screaming Architecture)

Every feature area in `Core` is a self-contained **module** named after its domain concept
(e.g. `TrainingLog`, `UserProfile`). Each module has exactly four sub-folders:

| Folder | Contents |
|---|---|
| `Domain/` | Entities, value objects, aggregate roots, domain events, domain service interfaces |
| `Application/` | Use case classes, application events, command/query objects |
| `Infrastructure/` | Adapter implementations: repositories, EF Core entity configurations, external service adapters |
| `Api/` | The module's **public surface**: DTOs, port interfaces consumed by the `API` project, mapping helpers |

Shared cross-cutting infrastructure (event bus, base types, shared value objects) lives in
`Core/Shared/`.

**Access modifiers are enforced by convention:**

- All types in `Domain/`, `Application/`, and `Infrastructure/` **must be `internal`**.
- All types in `Api/` **must be `public`** — these are the only types the `API` project may
  depend on from a given module.
- Domain and application **event types** are `public` because they cross module boundaries;
  everything else in those folders remains `internal`.

**Event communication:**

- A shared `IEventBus` abstraction and `IEventHandler<TEvent>` contract are defined in
  `Core/Shared/Events/` and are `public`.
- The current implementation (`InMemoryEventBus`) dispatches handlers **synchronously** and
  is registered as `internal`.
- All event publishing goes through `IEventBus.PublishAsync<TEvent>()` — handlers are never
  called directly. Use `async` signatures throughout even in the sync implementation so the
  contract does not need to change when a message broker is introduced.
- `IEventBus` is registered in `DependencyInjection.AddCoreServices()`. Swapping the
  implementation for an async broker requires only a single registration change.

**Canonical folder layout:**

```
Core/
  Shared/
    Events/
      IEventBus.cs                 ← public abstraction
      IEventHandler.cs             ← public handler contract
      InMemoryEventBus.cs          ← internal sync implementation
  TrainingLog/                     ← module (screaming name)
    Domain/
      TrainingLog.cs               ← internal entity
      TrainingLogCreatedEvent.cs   ← public domain event
    Application/
      CreateTrainingLogUseCase.cs  ← internal
      TrainingLogSummaryQuery.cs   ← internal
    Infrastructure/
      TrainingLogRepository.cs     ← internal (implements ITrainingLogRepository)
      TrainingLogConfiguration.cs  ← internal (IEntityTypeConfiguration<TrainingLog>)
    Api/
      ITrainingLogService.cs       ← public port consumed by API endpoints
      TrainingLogDto.cs            ← public DTO
```

---

## Examples

### Correct: Hexagonal boundary — port in Core, adapter in Core, DI registration in Core

```csharp
// Core/Ports/ITrainingLogRepository.cs
public interface ITrainingLogRepository
{
    Task<TrainingLog?> GetByIdAsync(Guid id, CancellationToken ct);
    Task AddAsync(TrainingLog log, CancellationToken ct);
}

// Core/Adapters/Persistence/TrainingLogRepository.cs
public sealed class TrainingLogRepository : ITrainingLogRepository
{
    private readonly AppDbContext _db;
    public TrainingLogRepository(AppDbContext db) => _db = db;

    public Task<TrainingLog?> GetByIdAsync(Guid id, CancellationToken ct) =>
        _db.TrainingLogs.FirstOrDefaultAsync(t => t.Id == id, ct);

    public Task AddAsync(TrainingLog log, CancellationToken ct)
    {
        _db.TrainingLogs.Add(log);
        return _db.SaveChangesAsync(ct);
    }
}

// Core/Persistence/Configurations/TrainingLogConfiguration.cs
public sealed class TrainingLogConfiguration : IEntityTypeConfiguration<TrainingLog>
{
    public void Configure(EntityTypeBuilder<TrainingLog> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.DurationMinutes).IsRequired();
    }
}

// Core/DependencyInjection.cs — the ONLY place Core services are registered
public static class DependencyInjection
{
    public static IServiceCollection AddCoreServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Default")));

        services.AddScoped<ITrainingLogRepository, TrainingLogRepository>();
        // register additional adapters here

        return services;
    }
}

// API/Program.cs — API is a thin consumer; it only calls Core's registration method
builder.Services.AddCoreServices(builder.Configuration);
```

### Correct: FastEndpoints endpoint

```csharp
// API/Features/TrainingLog/CreateTrainingLogEndpoint.cs
public sealed class CreateTrainingLogEndpoint : Endpoint<CreateTrainingLogRequest, CreateTrainingLogResponse>
{
    private readonly CreateTrainingLogUseCase _useCase;

    public CreateTrainingLogEndpoint(CreateTrainingLogUseCase useCase) => _useCase = useCase;

    public override void Configure()
    {
        Post("/training-logs");
        Claims("userId"); // Clerk claim required
    }

    public override async Task HandleAsync(CreateTrainingLogRequest req, CancellationToken ct)
    {
        var result = await _useCase.ExecuteAsync(req.ToCommand(), ct);
        await SendAsync(result.ToResponse(), StatusCodes.Status201Created, ct);
    }
}
```

### Incorrect: Business logic inside an endpoint (violates hexagonal architecture)

```csharp
// BAD — domain logic does not belong in the endpoint
public override async Task HandleAsync(CreateTrainingLogRequest req, CancellationToken ct)
{
    if (req.DurationMinutes <= 0) ThrowError("Duration must be positive");

    var log = new TrainingLog { Id = Guid.NewGuid(), Duration = req.DurationMinutes };
    _db.TrainingLogs.Add(log);         // direct DB access from endpoint layer
    await _db.SaveChangesAsync(ct);
    await SendOkAsync(ct);
}
```

### Correct: IEventBus abstraction and in-memory implementation

```csharp
// Core/Shared/Events/IEventBus.cs
public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default)
        where TEvent : class;
}

// Core/Shared/Events/IEventHandler.cs
public interface IEventHandler<TEvent> where TEvent : class
{
    Task HandleAsync(TEvent @event, CancellationToken ct);
}

// Core/Shared/Events/InMemoryEventBus.cs  (internal)
internal sealed class InMemoryEventBus : IEventBus
{
    private readonly IServiceProvider _sp;
    public InMemoryEventBus(IServiceProvider sp) => _sp = sp;

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default)
        where TEvent : class
    {
        var handlers = _sp.GetServices<IEventHandler<TEvent>>();
        foreach (var handler in handlers)
            await handler.HandleAsync(@event, ct);
    }
}

// Core/DependencyInjection.cs — register IEventBus with the in-memory implementation
services.AddScoped<IEventBus, InMemoryEventBus>();
// When moving to a broker: replace the line above only — all producers/consumers unchanged.
```

### Correct: Module access modifier discipline

```csharp
// Core/TrainingLog/Domain/TrainingLog.cs — internal; not visible outside the module
internal sealed class TrainingLog
{
    public Guid Id { get; private set; }
    public int DurationMinutes { get; private set; }
    // ...
}

// Core/TrainingLog/Domain/TrainingLogCreatedEvent.cs — public; crosses module boundary
public sealed record TrainingLogCreatedEvent(Guid TrainingLogId, DateTimeOffset OccurredAt);

// Core/TrainingLog/Api/TrainingLogDto.cs — public; consumed by API endpoints
public sealed record TrainingLogDto(Guid Id, int DurationMinutes);

// Core/TrainingLog/Api/ITrainingLogService.cs — public port consumed by API
public interface ITrainingLogService
{
    Task<TrainingLogDto> CreateAsync(CreateTrainingLogCommand cmd, CancellationToken ct);
}
```

### Incorrect: Leaking internal types across module boundaries

```csharp
// BAD — internal entity exposed through public port; internal adapter used directly in API
public interface ITrainingLogService
{
    Task<TrainingLog> CreateAsync(...); // TrainingLog is internal — must not appear here
}

// BAD — API endpoint directly newing up an internal use case
var useCase = new CreateTrainingLogUseCase(...); // bypasses DI and violates encapsulation
```

### Correct: Integration test with Testcontainers

```csharp
public class CreateTrainingLogTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public CreateTrainingLogTests(ApiFactory factory)
        => _client = factory.CreateClient();

    [Fact]
    public async Task Post_ValidRequest_Returns201()
    {
        var response = await _client.PostAsJsonAsync("/training-logs", new
        {
            DurationMinutes = 45,
            ActivityType = "Running"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
```

---

## DO's and DON'Ts

### DO

- Keep all domain logic, ports, entities, adapters, entity configurations, and DI registration in the `Core` project.
- Structure every `Core` module with `Domain/`, `Application/`, `Infrastructure/`, and `Api/` sub-folders.
- Mark all types in `Domain/`, `Application/`, and `Infrastructure/` as `internal`; mark `Api/` types and event types as `public`.
- Publish all events through `IEventBus.PublishAsync()` — never call handlers directly.
- Use FastEndpoints for every HTTP endpoint — one class per endpoint.
- Implement all port interfaces as adapter classes inside `Core`.
- Expose a single `AddCoreServices()` extension method from `Core`; call it from `API`.
- Use EF Core migrations for every schema change.
- Read all secrets and connection strings from environment variables.
- Use Clerk exclusively for authentication and authorisation.
- Ask the user before adding any package not on the pre-approved list.
- Write an integration test (Testcontainers) for every endpoint.
- Perform a code review and OWASP security check after every iteration.
- Report unfixed vulnerabilities with their CVSS v3.1 score.
- Keep `docker-compose.yml` working with a single `docker compose up`.

### DON'T

- Don't reference `API` or `Tests` from `Core`.
- Don't reference `Tests` from `API`.
- Don't use MediatR, Brighter, or any other paid/licensed mediator pipeline.
- Don't write business logic inside FastEndpoints endpoint classes.
- Don't implement adapters, repositories, or entity configurations in the `API` project.
- Don't register Core services in `API` directly — always call `AddCoreServices()` instead.
- Don't expose `internal` domain/application types through public `Api/` interfaces.
- Don't call event handlers directly — always publish through `IEventBus`.
- Don't use `async void` in event handlers — always return `Task`.
- Don't hard-code connection strings, Clerk keys, or any secrets.
- Don't bypass Clerk and introduce custom JWT validation, ASP.NET Core Identity, or any
  other auth mechanism.
- Don't use the service locator pattern inside domain code.
- Don't create N+1 queries — always review EF Core query plans for new data access code.
- Don't modify the database schema outside of EF Core migrations.
- Don't skip the post-iteration security check, even for small changes.
- Don't commit `.env` files — commit only `.env.example` with placeholder values.
