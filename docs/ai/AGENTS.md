# AGENTS.md - Moda Agent Context

This file provides context for AI coding agents working with the Moda codebase.

## Quick Start

- **Build**: `dotnet build Moda.slnx`
- **Test**: `dotnet test Moda.slnx`
- **Frontend**: `cd Moda.Web/src/moda.web.reactclient && npm install && npm run dev`
- **Full stack**: `cd Moda.AppHost && dotnet run`

## Architecture at a Glance

Moda is a **modular monolith** using **Clean Architecture** with **DDD**. The dependency flow is:

```
Domain → Application → Infrastructure → Web API
```

Each domain service is a vertical slice:
```
Moda.Services/Moda.{Service}/
  src/Moda.{Service}.Domain/       # Entities, value objects (zero dependencies)
  src/Moda.{Service}.Application/  # Commands, queries, DTOs (depends on Domain)
  tests/                           # Mirror of src structure
```

All services share a single `ModaDbContext` (no repository pattern).

## Domain Services

| Service | Namespace | Purpose |
|---------|-----------|---------|
| Work | `Moda.Work` | Work items, workspaces, work processes, workflows |
| Organization | `Moda.Organization` | Teams, employees, team memberships |
| Planning | `Moda.Planning` | Planning intervals, iterations |
| Goals | `Moda.Goals` | Objectives, key results |
| ProjectPortfolioManagement | `Moda.ProjectPortfolioManagement` | Projects, portfolios |
| StrategicManagement | `Moda.StrategicManagement` | Strategic themes |
| AppIntegration | `Moda.AppIntegration` | External system connections |
| Links | `Moda.Links` | Cross-entity associations |
| Health | `Moda.Health` | Health checks |

## Patterns to Follow

### Adding a Command

```csharp
// 1. Command record (Application layer)
public sealed record CreateThingCommand(string Name) : ICommand<Guid>;

// 2. Validator (FluentValidation, auto-registered)
public sealed class CreateThingCommandValidator : AbstractValidator<CreateThingCommand>
{
    public CreateThingCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
    }
}

// 3. Handler (returns Result<T>, uses DbContext directly)
internal sealed class CreateThingHandler : ICommandHandler<CreateThingCommand, Guid>
{
    private readonly ModaDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;

    public async Task<Result<Guid>> Handle(CreateThingCommand request, CancellationToken ct)
    {
        // Domain logic here
        var thing = Thing.Create(request.Name, _dateTimeProvider.Now);
        _context.Things.Add(thing);
        await _context.SaveChangesAsync(ct);
        return Result.Success(thing.Id);
    }
}
```

### Adding a Query

```csharp
public sealed record GetThingQuery(Guid Id) : IQuery<ThingDto>;

internal sealed class GetThingHandler : IQueryHandler<GetThingQuery, ThingDto>
{
    private readonly ModaDbContext _context;

    public async Task<Result<ThingDto>> Handle(GetThingQuery request, CancellationToken ct)
    {
        var dto = await _context.Things
            .Where(t => t.Id == request.Id)
            .ProjectToType<ThingDto>()
            .FirstOrDefaultAsync(ct);

        return dto is null
            ? Result.Failure<ThingDto>("Thing not found.")
            : Result.Success(dto);
    }
}
```

### Adding an API Endpoint

```csharp
[HttpPost]
public async Task<ActionResult<Guid>> Create(CreateThingRequest request, CancellationToken ct)
{
    var result = await _sender.Send(request.ToCommand(), ct);
    return result.IsSuccess
        ? CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value)
        : BadRequest(result.Error);
}
```

### Adding a Frontend Page

```typescript
// Use NSwag-generated client, never authenticatedFetch()
import { getThingsClient } from '@/src/services/clients'

// Use Ant Design theme tokens, never hardcoded colors
// Use CSS modules with var(--ant-color-primary) etc.
```

## Rules for Agents

1. **Never use `DateTime.UtcNow`** - inject `IDateTimeProvider`
2. **Never hardcode colors** - use Ant Design theme tokens
3. **Never use `authenticatedFetch()`** - use NSwag-generated typed clients
4. **No `Async` suffix** on new async methods
5. **No repository pattern** - use `ModaDbContext` directly
6. **Return `Result<T>`** from handlers, not exceptions
7. **Use NodaTime** (`Instant`, `LocalDate`) instead of `DateTime`
8. **Central Package Management** - versions in `Directory.Packages.props` only
9. **Architecture tests exist** - run `dotnet test Moda.ArchitectureTests` to verify dependency rules
10. **Feature flags** - new features behind flags defined in `FeatureFlags.cs`

## Key File Locations

| What | Where |
|------|-------|
| Solution file | `Moda.slnx` |
| Package versions | `Directory.Packages.props` |
| DB Context | `Moda.Infrastructure/src/Moda.Infrastructure/Persistence/Context/ModaDbContext.cs` |
| Entity configs | `Moda.Infrastructure/src/Moda.Infrastructure/Persistence/Configuration/` |
| Migrations | `Moda.Infrastructure/src/Moda.Infrastructure.Migrators.MSSQL/` |
| Feature flags | `Moda.Common/src/Moda.Common.Domain/FeatureManagement/FeatureFlags.cs` |
| API controllers | `Moda.Web/src/Moda.Web.Api/Controllers/` |
| Generated TS client | `Moda.Web/src/moda.web.reactclient/src/services/moda-api.ts` |
| Client factories | `Moda.Web/src/moda.web.reactclient/src/services/clients.ts` |
| Frontend pages | `Moda.Web/src/moda.web.reactclient/src/app/` |
| Shared test fakers | `Moda.Common/tests/Moda.Tests.Shared/` |
| Architecture tests | `Moda.ArchitectureTests/` |
