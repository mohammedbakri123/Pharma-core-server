# How to Build an Endpoint: Infrastructure Model to API Controller

This guide walks through creating a complete endpoint in the PharmaCore project, following the Clean Architecture layered pattern.

**Layers (dependency direction):**

```
API  →  Application  →  Domain
           ↑
     Infrastructure
```

---

## Step 1 — Domain Layer (`PharmaCore.Domain`)

> **What:** The domain layer holds the core business logic and rules. It has **zero dependencies** on other layers — no EF Core, no ASP.NET, no external libraries beyond base .NET. This is the heart of your application.

### 1.1 Create the Domain Entity

**File:** `PharmaCore.Domain/Entities/{EntityName}.cs`

Use a **rich domain model**: private constructor, factory methods (`Create`, `Rehydrate`), and behavior methods. Properties have private setters.

```csharp
namespace PharmaCore.Domain.Entities;

public sealed class Example
{
    public int Id { get; private set; }
    public string Name { get; private set; }

    private Example() { }

    private Example(int id, string name)
    {
        Id = id;
        Name = !string.IsNullOrWhiteSpace(name)
            ? name
            : throw new ArgumentException("Name is required.", nameof(name));
    }

    public static Example Create(string name)
    {
        return new Example(0, name);
    }

    public static Example Rehydrate(int id, string name)
    {
        return new Example(id, name);
    }

    public void Update(string? name)
    {
        if (!string.IsNullOrWhiteSpace(name))
            Name = name;
    }
}
```

**Why this matters:**
- **Encapsulation:** Private setters prevent external code from putting the entity in an invalid state.
- **Factory methods:** `Create()` enforces invariants at construction time; `Rehydrate()` rebuilds from persistence without re-running creation validation.
- **Testability:** Domain entities can be unit-tested in isolation — no database, no framework needed.
- **Self-validating:** The entity itself guarantees it's always valid.

### 1.2 Add Enums / Shared Types (if needed)

- **Enums:** `PharmaCore.Domain/Enums/`
- **Shared types:** `PharmaCore.Domain/Shared/` (`ServiceResult`, `ServiceError`, `ServiceErrorType`)

**Why this matters:** These types are shared across all layers. Putting them in Domain prevents circular dependencies and gives every layer a common vocabulary.

---

## Step 2 — Application Layer (`PharmaCore.Application`)

> **What:** The application layer orchestrates use cases. It defines **what** the system can do (create, update, list, delete) without knowing **how** it's done. It depends on Domain but not on Infrastructure or API. This layer is the contract between your business rules and the outside world.

### 2.1 Define the Repository Interface

**File:** `PharmaCore.Application/Abstractions/Persistence/I{EntityName}Repository.cs`

```csharp
using PharmaCore.Domain.Entities;

namespace PharmaCore.Application.Abstractions.Persistence;

public interface IExampleRepository
{
    Task<Example?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Example>> ListAsync(CancellationToken cancellationToken = default);
    Task<Example> AddAsync(Example entity, CancellationToken cancellationToken = default);
    Task<Example> UpdateAsync(Example entity, CancellationToken cancellationToken = default);
    Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default);
}
```

> **Key:** Interfaces live in Application so services depend on abstractions, not implementations.

**Why this matters:**
- **Dependency Inversion:** The Application layer defines the interface; Infrastructure implements it. This inverts the dependency direction — Application doesn't depend on Infrastructure.
- **Swappable persistence:** You could swap EF Core for Dapper, a web API, or an in-memory store without touching a single line of Application code.
- **Testability:** Unit tests can provide mock/fake implementations of the interface.

### 2.2 Create the DTO

**File:** `PharmaCore.Application/{Feature}/Dtos/{EntityName}Dto.cs`

```csharp
namespace PharmaCore.Application.Examples.Dtos;

public sealed record ExampleDto(int Id, string Name);
```

**Why this matters:**
- **Shape control:** DTOs define exactly what data leaves the application layer. Domain entities may have internal fields that should never be exposed.
- **Decoupling:** If the domain entity changes, the DTO can stay stable — API consumers aren't affected.
- **Records:** Immutable, value-based comparison, concise syntax.

### 2.3 Create Request/Command/Query Records

**File:** `PharmaCore.Application/{Feature}/Requests/CreateExampleCommand.cs`

```csharp
namespace PharmaCore.Application.Examples.Requests;

public sealed record CreateExampleCommand(string Name);
```

**File:** `PharmaCore.Application/{Feature}/Requests/GetExampleByIdQuery.cs`

```csharp
namespace PharmaCore.Application.Examples.Requests;

public sealed record GetExampleByIdQuery(int Id);
```

Create additional commands/queries as needed (`Update{Entity}Command`, `List{Entity}sQuery`, `Delete{Entity}Command`).

**Why this matters:**
- **Explicit contracts:** Each use case has its own typed input. No ambiguity about what data is required.
- **Immutable:** Records are immutable by default, preventing accidental mutation during processing.
- **Separation from API contracts:** The controller receives an API Request DTO, maps it to a Command/Query. If the API contract changes but the use case doesn't, the command stays the same.

### 2.4 Create the Service Interface (one per use case)

**File:** `PharmaCore.Application/{Feature}/Interfaces/ICreateExampleService.cs`

```csharp
using PharmaCore.Application.Examples.Dtos;
using PharmaCore.Application.Examples.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Examples.Interfaces;

public interface ICreateExampleService
{
    Task<ServiceResult<ExampleDto>> ExecuteAsync(CreateExampleCommand command, CancellationToken cancellationToken = default);
}
```

> **Key:** All services return `ServiceResult<T>` instead of throwing exceptions. This makes error handling explicit and consistent.

**Why this matters:**
- **Single Responsibility:** One interface per use case keeps each service focused and easy to test.
- **`ServiceResult<T>`:** Errors are data, not control flow. Callers must handle both success and failure — no hidden exceptions.
- **Cancellation:** Every method accepts `CancellationToken` for proper async cancellation support.

### 2.5 Implement the Service

**File:** `PharmaCore.Application/{Feature}/Services/CreateExampleService.cs`

```csharp
using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Examples.Dtos;
using PharmaCore.Application.Examples.Interfaces;
using PharmaCore.Application.Examples.Requests;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Examples.Services;

public class CreateExampleService : ICreateExampleService
{
    private readonly IExampleRepository _repository;
    private readonly ILogger<CreateExampleService> _logger;

    public CreateExampleService(IExampleRepository repository, ILogger<CreateExampleService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<ServiceResult<ExampleDto>> ExecuteAsync(CreateExampleCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
            return ServiceResult<ExampleDto>.Fail(ServiceErrorType.Validation, "Name is required.");

        var entity = Example.Create(command.Name);
        var created = await _repository.AddAsync(entity, cancellationToken);

        _logger.LogInformation("Example '{Name}' created with ID {Id}", created.Name, created.Id);
        return ServiceResult<ExampleDto>.Ok(new ExampleDto(created.Id, created.Name));
    }
}
```

> **Key pattern:** Use `ServiceResult<T>.Ok(data)` for success and `ServiceResult<T>.Fail(errorType, message)` for errors. Do **not** throw exceptions for expected business errors.

**Why this matters:**
- **Orchestration:** The service is the "traffic cop" — it validates input, calls domain factory, persists via repository, maps to DTO.
- **Business errors are normal flow:** Validation failures, duplicates, not-found — these are expected outcomes, not exceptional circumstances.
- **Logging:** Every service logs what it does. This is critical for production debugging.

### 2.6 Register Application Services in DI

**File:** `PharmaCore.Application/DependencyInjection.cs`

```csharp
services.AddScoped<ICreateExampleService, CreateExampleService>();
services.AddScoped<IUpdateExampleService, UpdateExampleService>();
services.AddScoped<IDeleteExampleService, DeleteExampleService>();
services.AddScoped<IListExamplesService, ListExamplesService>();
services.AddScoped<IGetExampleByIdService, GetExampleByIdService>();
```

> Add all service interface → implementation pairs here with **Scoped** lifetime.

**Why this matters:**
- **Wiring:** Without registration, the DI container can't inject services into controllers.
- **Scoped:** One instance per HTTP request. This is the right balance between performance and correctness.
- **Interface → Implementation:** Controllers depend on interfaces only, making them trivially mockable.

---

## Step 3 — Infrastructure Layer (`PharmaCore.Infrastructure`)

> **What:** Infrastructure is where the rubber meets the road — database access, external APIs, file systems, email services. It **implements** the interfaces defined in Application. This layer depends on EF Core (or whatever ORM/SDK you use) and nothing else from your solution.

### 3.1 Create the EF Core Model

**File:** `PharmaCore.Infrastructure/Models/{EntityName}.cs`

```csharp
namespace PharmaCore.Infrastructure.Models;

public partial class Example
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public bool? IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
```

> **Note:** Infrastructure models are separate from domain entities. They are pure EF Core entities with public getters/setters.

**Why this matters:**
- **EF Core requires mutability:** EF Core needs public setters to materialize entities from the database. Domain entities have private setters for encapsulation. These two needs are incompatible in a single class.
- **Database-specific concerns:** Models carry attributes, navigation properties, and EF-specific configuration that the domain layer should never know about.
- **Two models, two purposes:** The domain entity enforces business rules; the infrastructure model maps to a database table.

### 3.2 Register the Model in ApplicationDbContext

**File:** `PharmaCore.Infrastructure/Persistence/ApplicationDbContext.cs`

Add a `DbSet`:

```csharp
public virtual DbSet<Example> Examples { get; set; }
```

Add fluent configuration in `OnModelCreating`:

```csharp
modelBuilder.Entity<Example>(entity =>
{
    entity.HasKey(e => e.Id);
    entity.ToTable("examples");
    entity.Property(e => e.Id).HasColumnName("id");
    entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(200);
    // ... additional column mappings
});
```

**Why this matters:**
- **DbSet:** Without it, EF Core doesn't track the entity or generate SQL for it.
- **Fluent API:** Keeps configuration out of the model class, keeping it clean. Also enables complex mappings that data annotations can't express.
- **Column naming:** `HasColumnName` enforces a consistent database naming convention (e.g. snake_case) independent of C# naming (PascalCase).

### 3.3 Create the Repository Implementation

**File:** `PharmaCore.Infrastructure/Persistence/Repositories/{EntityName}Repository.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Domain.Entities;
using ExampleModel = PharmaCore.Infrastructure.Models.Example;

namespace PharmaCore.Infrastructure.Persistence.Repositories;

public class ExampleRepository : IExampleRepository
{
    private readonly ApplicationDbContext _dbContext;

    public ExampleRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Example?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var model = await _dbContext.Examples.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id && e.IsDeleted != true, cancellationToken);
        return model is null ? null : Map(model);
    }

    public async Task<IEnumerable<Example>> ListAsync(CancellationToken cancellationToken = default)
    {
        var models = await _dbContext.Examples.AsNoTracking()
            .Where(e => e.IsDeleted != true)
            .ToListAsync(cancellationToken);
        return models.Select(Map);
    }

    public async Task<Example> AddAsync(Example entity, CancellationToken cancellationToken = default)
    {
        var model = new ExampleModel { Name = entity.Name };
        _dbContext.Examples.Add(model);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Map(model);
    }

    public async Task<Example> UpdateAsync(Example entity, CancellationToken cancellationToken = default)
    {
        var model = await _dbContext.Examples.FindAsync([entity.Id], cancellationToken: cancellationToken);
        if (model is null)
            throw new KeyNotFoundException($"Example with ID {entity.Id} not found.");

        model.Name = entity.Name;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Map(model);
    }

    public async Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var model = await _dbContext.Examples.FindAsync([id], cancellationToken: cancellationToken);
        if (model is null) return false;

        model.IsDeleted = true;
        model.DeletedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static Example Map(ExampleModel model)
    {
        return Example.Rehydrate(model.Id, model.Name);
    }
}
```

> **Key pattern:** The repository maps between Infrastructure models (EF entities) and Domain entities using `Map()` and `Rehydrate()`. Soft-delete filtering (`IsDeleted != true`) is applied at the repository level.

**Why this matters:**
- **Translation layer:** Repositories translate between the domain's rich objects and the database's relational tables.
- **`AsNoTracking()`:** For read queries, this skips EF Core's change tracking, improving performance and reducing memory.
- **Soft delete:** `IsDeleted` filtering is centralized here so no caller ever accidentally sees deleted records.
- **`Rehydrate()`:** Uses the domain entity's factory method to reconstruct from database state, bypassing creation validation.

### 3.4 Register the Repository in DI

**File:** `PharmaCore.Infrastructure/DependencyInjection.cs`

```csharp
services.AddScoped<IExampleRepository, ExampleRepository>();
```

**Why this matters:**
- **Completes the DI chain:** Application defines `IExampleRepository`, Infrastructure provides the implementation. At runtime, DI resolves the concrete type when a service asks for the interface.
- **Scoped:** Shares the same `ApplicationDbContext` instance for the lifetime of one HTTP request.

---

## Step 4 — API Layer (`PharmaCore.API`)

> **What:** The API layer is the entry point for external clients (web browsers, mobile apps, other services). It receives HTTP requests, delegates to Application services, and returns HTTP responses. It knows about everything below it but contains no business logic.

### 4.1 Create the Request Contract (API DTO)

**File:** `PharmaCore.API/Contracts/{Feature}/CreateExampleRequest.cs`

```csharp
namespace PharmaCore.API.Contracts.Examples;

public sealed record CreateExampleRequest(string Name);
```

**Why this matters:**
- **API versioning stability:** If you need to change the internal command but keep the public API the same, you can. The API contract is the "public face" of your endpoint.
- **Validation attributes:** This is where you'd add `[Required]`, `[MaxLength]`, etc. — concerns specific to HTTP, not to the use case itself.
- **Swagger/OpenAPI:** These records drive the generated API documentation.

### 4.2 Create the Controller

**File:** `PharmaCore.API/Controllers/{FeatureName}Controller.cs`

Controllers inherit from `ApiControllerBase` (which provides `MapServiceResult` for centralized error handling). Services are injected per-action via `[FromServices]`.

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaCore.Application.Examples.Interfaces;
using PharmaCore.Application.Examples.Requests;
using PharmaCore.API.Contracts.Examples;

namespace PharmaCore.API.Controllers;

[Route("examples")]
[Authorize]
public class ExamplesController : ApiControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateExampleRequest request,
        [FromServices] ICreateExampleService createExampleService,
        CancellationToken cancellationToken)
    {
        var result = await createExampleService.ExecuteAsync(
            new CreateExampleCommand(request.Name), cancellationToken);

        if (!result.Success)
            return MapServiceResult(result);

        return StatusCode(201, result.Data);
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromServices] IListExamplesService listExamplesService,
        CancellationToken cancellationToken)
    {
        var result = await listExamplesService.ExecuteAsync(
            new ListExamplesQuery(page, limit), cancellationToken);

        if (!result.Success)
            return MapServiceResult(result);

        return Ok(new
        {
            items = result.Data!.Items,
            pagination = new { total = result.Data.Total, page = result.Data.Page, limit = result.Data.Limit }
        });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(
        int id,
        [FromServices] IGetExampleByIdService getExampleByIdService,
        CancellationToken cancellationToken)
    {
        var result = await getExampleByIdService.ExecuteAsync(
            new GetExampleByIdQuery(id), cancellationToken);

        return MapServiceResult(result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateExampleRequest request,
        [FromServices] IUpdateExampleService updateExampleService,
        CancellationToken cancellationToken)
    {
        var result = await updateExampleService.ExecuteAsync(
            new UpdateExampleCommand(id, request.Name), cancellationToken);

        return MapServiceResult(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(
        int id,
        [FromServices] IDeleteExampleService deleteExampleService,
        CancellationToken cancellationToken)
    {
        var result = await deleteExampleService.ExecuteAsync(id, cancellationToken);

        if (!result.Success)
            return MapServiceResult(result);

        return Ok(new { message = "Example deleted successfully" });
    }
}
```

> **Key pattern:** Call `MapServiceResult(result)` to map the `ServiceResult<T>` to the appropriate HTTP status code. For endpoints that return a wrapper object, check `result.Success` first before accessing `result.Data`.

**Why this matters:**
- **Thin controllers:** Controllers do three things: receive HTTP data, map to command/query, call service, return result. No business logic.
- **`[FromServices]`:** Per-action injection avoids constructor bloat. Each endpoint only gets what it needs.
- **`MapServiceResult`:** Centralized error mapping — `ServiceErrorType.Validation` → 400, `NotFound` → 404, etc. One call replaces a dozen `try/catch` blocks.
- **Route + Authorize attributes:** `[Route("examples")]` defines the base URL; `[Authorize]` gates access. Per-endpoint overrides like `[AllowAnonymous]` handle public endpoints.

---

## Complete Data Flow: "Create Example"

```
HTTP POST /examples  { "name": "Test" }
        │
        ▼
[API]       ExamplesController.Create()
            Receives CreateExampleRequest → maps to CreateExampleCommand
            → calls service.ExecuteAsync()
        │
        ▼
[App]       CreateExampleService.ExecuteAsync(command)
            Validates → returns ServiceResult.Fail() on error
            → calls Example.Create() → calls IExampleRepository.AddAsync()
        │
        ▼
[Domain]    Example.Create() — private constructor with validation
        │
        ▼
[Infra]     ExampleRepository.AddAsync()
            Maps Domain → EF Model → EF Core Add + SaveChanges → Maps back
        │
        ▼
[App]       Returns ServiceResult<ExampleDto>.Ok(dto)
        │
        ▼
[API]       MapServiceResult(result) → HTTP 201  { "id": 1, "name": "Test" }
            or HTTP 400/404/409/500 on error
```

**Why this flow matters:**
- **Unidirectional dependencies:** Each arrow goes in one direction. API depends on Application, Application depends on Domain, Infrastructure implements Application. Nothing points backward.
- **No layer skipping:** The controller never talks to the repository directly. The service never talks to the database directly. Every layer has a single responsibility.
- **Failure propagates upward:** If validation fails at any layer, it returns as `ServiceResult.Fail()` — no exceptions to catch, no hidden control flow.
- **Two mapping points:** API Request → Command (in controller), Domain Entity → DTO (in service). These are the only places data shape changes.

---

## ServiceResult Error Mapping

| `ServiceErrorType` | HTTP Status | Use Case |
|---|---|---|
| `None` | 200 / 201 | Success |
| `Validation` | 400 | Invalid input, missing required fields |
| `Duplicate` | 409 | Unique constraint violation (e.g. username exists) |
| `NotFound` | 404 | Resource doesn't exist |
| `Unauthorized` | 401 | Invalid credentials, expired token, deleted user |
| `ServerError` | 500 | Unexpected internal error |

> The `MapServiceResult` method in `ApiControllerBase` handles this mapping automatically.

**How to choose the right error type:**
- Ask: *"Is this the client's fault or ours?"* If the client sent bad data → `Validation`. If the resource is gone → `NotFound`. If the client is lying about identity → `Unauthorized`.
- **Duplicate vs Validation:** Use `Duplicate` when the input is valid in isolation but conflicts with existing data (e.g., unique username). Use `Validation` when the input is invalid on its own (e.g., empty string, too-short password).
- **ServerError is a last resort:** Only use it for truly unexpected failures — database connection drops, third-party API outages. Never use it for something the client could have prevented.

---

## Checklist

| # | Step | File(s) |
|---|------|---------|
| 1 | Domain entity | `PharmaCore.Domain/Entities/{EntityName}.cs` |
| 2 | Repository interface | `PharmaCore.Application/Abstractions/Persistence/I{EntityName}Repository.cs` |
| 3 | DTO | `PharmaCore.Application/{Feature}/Dtos/{EntityName}Dto.cs` |
| 4 | Command/Query records | `PharmaCore.Application/{Feature}/Requests/` |
| 5 | Service interfaces (one per use case) | `PharmaCore.Application/{Feature}/Interfaces/` |
| 6 | Service implementations (return `ServiceResult<T>`) | `PharmaCore.Application/{Feature}/Services/` |
| 7 | Register app services in DI | `PharmaCore.Application/DependencyInjection.cs` |
| 8 | EF Core model | `PharmaCore.Infrastructure/Models/{EntityName}.cs` |
| 9 | Register model in DbContext | `PharmaCore.Infrastructure/Persistence/ApplicationDbContext.cs` |
| 10 | Repository implementation | `PharmaCore.Infrastructure/Persistence/Repositories/{EntityName}Repository.cs` |
| 11 | Register repository in DI | `PharmaCore.Infrastructure/DependencyInjection.cs` |
| 12 | API request contract | `PharmaCore.API/Contracts/{Feature}/` |
| 13 | Controller (use `MapServiceResult`) | `PharmaCore.API/Controllers/{FeatureName}Controller.cs` |

**Order matters:** Steps 1–7 can be completed before 8–10 exist, because Application depends on interfaces not implementations. Step 12–13 come last — you can't test an endpoint until everything below it is wired up.

**Quick verification:** After completing all steps, run `dotnet build`. If it compiles, your DI registrations are likely correct. If the service returns `ServiceResult<T>` and the controller calls `MapServiceResult`, error handling is consistent.
