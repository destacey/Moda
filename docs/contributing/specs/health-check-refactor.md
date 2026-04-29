# Health Check Refactor — Implementation Plan

## Goal

Convert "Health Check" from a polymorphic shared service (`Wayd.Health` + cross-context events + replicated `Planning.PlanningHealthChecks`) into a **shareable architectural pattern** that each consuming context owns directly. The first consumer is Planning Interval objectives; a follow-up PR adds Projects.

## Non-goals

- No frontend work in PR 1 beyond what's required to keep the existing UI working against the new endpoints. The `objectives/[objectiveKey]/health-report` page and `health-checks-api.ts` RTK Query slice get repointed at the new endpoints; deeper UI cleanup is out of scope.
- No regeneration of the Wayd OpenAPI client beyond what NSwag does on Debug build.

## Why

The polymorphic `(ObjectId, Context)` model meant a single `Health.HealthChecks` table held checks for any kind of object, accessed via a single flat controller. As the app grew, this created two real problems:

1. **Permissions don't fit a flat resource.** A health check on a PI objective should require permissions on *that objective*, not a generic "HealthChecks" resource. The flat controller can't express that without re-deriving the parent on every request.
2. **The replication is dead weight.** `Planning.PlanningHealthChecks` (`SimpleHealthCheck`) is a denormalized projection of "the current check" per objective, kept in sync by an event handler. The handler is non-transactional, swallows errors, and exists only because the canonical data lived in another bounded context.

Owning the table within Planning eliminates both problems and lets us delete the projection, the event handler, the cross-context events, and the standalone service.

## Design summary

### Shareable architectural pattern (not a shared service)

A common abstract base class lives in `Wayd.Common.Domain`. Each consuming context inherits with its own concrete entity and table:

```
Wayd.Common.Domain.HealthChecks
├── HealthCheckBase                       (abstract aggregate, owns invariants)
└── HealthReport                          (transient: collection + AddHealthCheck rule)

Wayd.Planning.Domain.Models
└── PlanningIntervalObjectiveHealthCheck : HealthCheckBase

(future PR)
Wayd.Ppm.Domain.Models
└── ProjectHealthCheck : HealthCheckBase
```

`Context` (the `SystemContext` enum) goes away from the model — the table *is* the context. `ObjectId` becomes a real FK with proper cascade semantics. `HealthReport`'s "no overlapping active checks" invariant lives in the base.

### Aggregate ownership

`PlanningIntervalObjective` becomes the aggregate root for its health checks. Today it has `HealthCheck` (singular, a `SimpleHealthCheck`); after the refactor it has `HealthChecks` (collection). All writes go through the objective.

### Read-side: most-recent-check projection in Mapster

Following the `PlanningIntervalIterationDetailsDto.CreateTypeAdapterConfig(asOf)` pattern: each DTO that exposes a "current health check" field defines a static factory that builds a `TypeAdapterConfig` with `now` baked in. The handler injects `IDateTimeProvider`, builds the config, and passes it to `ProjectToType`.

```csharp
config.NewConfig<PlanningIntervalObjective, PlanningIntervalObjectiveListDto>()
    .Map(dest => dest.HealthCheck,
         src => src.HealthChecks
                   .Where(h => h.Expiration > now)
                   .FirstOrDefault());
```

Domain invariant guarantees ≤1 non-expired check, so no `OrderBy` needed in the projection. EF translates this to `OUTER APPLY (SELECT TOP 1 ...)` on SQL Server.

### Endpoints

Health checks become nested under their parent objective:

```
GET    /api/planning/planning-intervals/{piIdOrKey}/objectives/{objIdOrKey}/health-checks
GET    /api/planning/planning-intervals/{piIdOrKey}/objectives/{objIdOrKey}/health-checks/{id}
POST   /api/planning/planning-intervals/{piIdOrKey}/objectives/{objIdOrKey}/health-checks
PUT    /api/planning/planning-intervals/{piIdOrKey}/objectives/{objIdOrKey}/health-checks/{id}
DELETE /api/planning/planning-intervals/{piIdOrKey}/objectives/{objIdOrKey}/health-checks/{id}   (new — closes the dead-handler gap)
GET    /api/planning/health-statuses                                                              (lookup, moves out of /api/healthchecks)
```

The flat `/api/healthchecks/*` controller goes away.

Permission resource shifts from `ApplicationResource.HealthChecks` to `ApplicationResource.PlanningIntervalObjectives` (or whatever the existing PI objective resource is called). Verify the existing actions cover Create/Update/Delete on objectives; if not, add a sub-resource permission.

### Migration strategy

One PR for the whole refactor — splitting it leaves the system in an inconsistent intermediate state where two health-check tables both exist with partial data.

EF migration steps (one migration, sequential SQL):

1. Create new table `Planning.PlanningIntervalObjectiveHealthChecks`.
2. Copy from `Health.HealthChecks` into the new table, filtering `Context = 'PlanningPlanningIntervalObjective'` (the only active context). Source-of-truth is `Health.HealthChecks` because it has `Note` and `ReportedBy`, which `Planning.PlanningHealthChecks` lacks.
3. Verify counts match expected (`Planning.PlanningHealthChecks` row count ≤ migrated row count).
4. Drop FK from `Planning.PlanningHealthChecks` to `PlanningIntervalObjectives`, drop the table.
5. Drop `Health.HealthChecks` table.
6. Drop `Health` schema if empty.

Pre-migration check (run manually before merging):

```sql
SELECT Context, COUNT(*) FROM Health.HealthChecks GROUP BY Context;
```

If anything other than `PlanningPlanningIntervalObjective` shows up, decide row-by-row before proceeding.

---

## File-level changes

### New files

| File | Purpose |
|---|---|
| `Wayd.Common/src/Wayd.Common.Domain/HealthChecks/HealthCheckBase.cs` | Abstract aggregate; owns `Status`, `ReportedBy`, `ReportedOn`, `Expiration`, `Note`, `IsExpired(now)`, `Update(...)`, `ChangeExpiration(...)`. No `ObjectId` or `Context` (parents own those). |
| `Wayd.Common/src/Wayd.Common.Domain/HealthChecks/HealthReport.cs` | Transient collection wrapper; owns the "truncate previous on add" invariant. Generic over `THealthCheck`. |
| `Wayd.Services/Wayd.Planning/src/Wayd.Planning.Domain/Models/PlanningIntervalObjectiveHealthCheck.cs` | Concrete entity. Inherits `HealthCheckBase`. Adds `PlanningIntervalObjectiveId` FK. Soft-deletable. |
| `Wayd.Infrastructure/src/Wayd.Infrastructure/Persistence/Configuration/PlanningIntervalObjectiveHealthCheckConfiguration.cs` | EF config: table `Planning.PlanningIntervalObjectiveHealthChecks`, FK to objective with `OnDelete(Cascade)`, soft-delete columns, indexes on `(PlanningIntervalObjectiveId, Expiration)`. |
| `Wayd.Services/Wayd.Planning/src/Wayd.Planning.Application/PlanningIntervals/Commands/CreateObjectiveHealthCheckCommand.cs` | Replaces `Wayd.Health.Commands.CreateHealthCheckCommand`. Loads objective with `Include(o => o.HealthChecks)`, calls `objective.AddHealthCheck(...)`, saves. |
| `Wayd.Services/Wayd.Planning/src/Wayd.Planning.Application/PlanningIntervals/Commands/UpdateObjectiveHealthCheckCommand.cs` | Update existing check. |
| `Wayd.Services/Wayd.Planning/src/Wayd.Planning.Application/PlanningIntervals/Commands/DeleteObjectiveHealthCheckCommand.cs` | Soft-delete. New — fixes the missing delete path. |
| `Wayd.Services/Wayd.Planning/src/Wayd.Planning.Application/PlanningIntervals/Queries/GetObjectiveHealthChecksQuery.cs` | Returns full history for an objective (replaces the current "health report" endpoint). |
| `Wayd.Services/Wayd.Planning/src/Wayd.Planning.Application/PlanningIntervals/Queries/GetObjectiveHealthCheckQuery.cs` | Get single by id. |
| `Wayd.Services/Wayd.Planning/src/Wayd.Planning.Application/PlanningIntervals/Dtos/PlanningIntervalObjectiveHealthCheckDto.cs` | Full DTO (id, status, reportedBy, reportedOn, expiration, note). Replaces `Wayd.Health.Dtos.HealthCheckDto`. |
| `Wayd.Web/src/Wayd.Web.Api/Controllers/Planning/PlanningIntervalObjectiveHealthChecksController.cs` | Nested controller. Route: `api/planning/planning-intervals/{piIdOrKey}/objectives/{objIdOrKey}/health-checks`. Permissions on PI objectives. |
| `Wayd.Web/src/Wayd.Web.Api/Models/Planning/CreateObjectiveHealthCheckRequest.cs` | API request model + validator. |
| `Wayd.Web/src/Wayd.Web.Api/Models/Planning/UpdateObjectiveHealthCheckRequest.cs` | API request model + validator. |
| `Wayd.Infrastructure/src/Wayd.Infrastructure.Migrators.MSSQL/Migrations/{timestamp}_Migrate-HealthChecks-To-Planning.cs` | The migration described above. |

### Modified files

| File | Change |
|---|---|
| `Wayd.Services/Wayd.Planning/src/Wayd.Planning.Domain/Models/PlanningIntervalObjective.cs` | Replace `SimpleHealthCheck? HealthCheck` (line 54) with `IReadOnlyCollection<PlanningIntervalObjectiveHealthCheck> HealthChecks`. Replace `AddHealthCheck`/`RemoveHealthCheck` with methods that delegate to `HealthReport` (truncate previous, then append). Add `UpdateHealthCheck(id, ...)` and `DeleteHealthCheck(id)`. |
| `Wayd.Services/Wayd.Planning/src/Wayd.Planning.Application/Persistence/IPlanningDbContext.cs` | Replace `DbSet<SimpleHealthCheck> PlanningHealthChecks` with `DbSet<PlanningIntervalObjectiveHealthCheck> PlanningIntervalObjectiveHealthChecks`. |
| `Wayd.Infrastructure/src/Wayd.Infrastructure/Persistence/Context/WaydDbContext.cs` | Same DbSet rename. |
| `Wayd.Services/Wayd.Planning/src/Wayd.Planning.Application/PlanningIntervals/Dtos/PlanningHealthCheckDto.cs` | Remove `Create(SimpleHealthCheck, Instant)` factory. Move to Mapster config (see DTO changes below). |
| `Wayd.Services/Wayd.Planning/src/Wayd.Planning.Application/PlanningIntervals/Dtos/PlanningIntervalObjectiveListDto.cs` | Add `static TypeAdapterConfig CreateTypeAdapterConfig(Instant now)` (mirroring `PlanningIntervalIterationDetailsDto`). Map `HealthCheck` from `src.HealthChecks.Where(h => h.Expiration > now).FirstOrDefault()`. Update `Create(...)` to no longer take or use `now` for the health check (it'll come from the projection path). |
| `Wayd.Services/Wayd.Planning/src/Wayd.Planning.Application/PlanningIntervals/Dtos/PlanningIntervalObjectiveDetailsDto.cs` | Same `CreateTypeAdapterConfig(Instant now)` pattern. |
| `Wayd.Services/Wayd.Planning/src/Wayd.Planning.Application/PlanningIntervals/Queries/GetPlanningIntervalObjectivesQuery.cs` | Replace the manual `Create(...)` loop (lines 73-77) with `ProjectToType<PlanningIntervalObjectiveListDto>(config)` using the new factory. Replace `.ThenInclude(o => o.HealthCheck)` with `.ThenInclude(o => o.HealthChecks)` (lines 47, 55). |
| `Wayd.Services/Wayd.Planning/src/Wayd.Planning.Application/PlanningIntervals/Queries/GetPlanningIntervalObjectiveQuery.cs` | Same: `ProjectToType` with config; `.ThenInclude(o => o.HealthChecks)` (line 46). |
| `Wayd.Services/Wayd.Planning/src/Wayd.Planning.Application/Wayd.Planning.Application.csproj` | (Probably no change — already references `Wayd.Common`. Verify.) |

### Deleted files

| File / dir | Reason |
|---|---|
| `Wayd.Services/Wayd.Health/` (entire slice) | Service goes away. Includes domain `HealthCheck`, `HealthReport`, all commands/queries/DTOs, `IHealthDbContext`, `ConfigureServices`, tests. |
| `Wayd.Services/Wayd.Planning/src/Wayd.Planning.Domain/Models/SimpleHealthCheck.cs` | Replaced by inherited `PlanningIntervalObjectiveHealthCheck`. |
| `Wayd.Services/Wayd.Planning/src/Wayd.Planning.Application/PlanningHealthChecks/EventHandlers/ObjectiveHealthCheckEventHandler.cs` | No more cross-context events to handle. |
| `Wayd.Services/Wayd.Planning/src/Wayd.Planning.Application/PlanningHealthChecks/` (entire dir, if empty after handler removal) | Folder cleanup. |
| `Wayd.Common/src/Wayd.Common.Domain/Events/Health/HealthCheckCreatedEvent.cs` | Cross-context event no longer needed. |
| `Wayd.Common/src/Wayd.Common.Domain/Events/Health/HealthCheckUpdatedEvent.cs` | Same. |
| `Wayd.Common/src/Wayd.Common.Domain/Events/Health/HealthCheckDeletedEvent.cs` | Same (also was never raised). |
| `Wayd.Common/src/Wayd.Common.Domain/Interfaces/IHealthCheck.cs` | Single-implementer interface, no longer useful. |
| `Wayd.Web/src/Wayd.Web.Api/Controllers/Health/HealthChecksController.cs` | Replaced by nested controller. |
| `Wayd.Web/src/Wayd.Web.Api/Models/Health/CreateHealthCheckRequest.cs` | Replaced. |
| `Wayd.Web/src/Wayd.Web.Api/Models/Health/UpdateHealthCheckRequest.cs` | Replaced. |
| `Wayd.Web/src/Wayd.Web.Api/Dtos/Planning/PlanningIntervalObjectiveHealthCheckDto.cs` | Verify what this is — may be stale after the refactor. |
| `Wayd.Common/src/Wayd.Common.Domain/Enums/SystemContext.cs` (member) | Remove `PlanningPlanningIntervalObjective` if no other consumer uses it. Grep first. |
| `ApplicationResource.HealthChecks` | Remove the permission resource if it was specifically for the flat controller. |

### Frontend (light touch)

| File | Change |
|---|---|
| `Wayd.Web/src/wayd.web.reactclient/src/store/features/common/health-checks-api.ts` | Repoint endpoints to nested URLs. |
| `Wayd.Web/src/wayd.web.reactclient/src/components/common/health-check/*.tsx` | Update to use the regenerated NSwag client methods (NSwag regen will rename them). |
| `Wayd.Web/src/wayd.web.reactclient/src/app/planning/planning-intervals/[key]/objectives/health-report/page.tsx` | Same. |
| `Wayd.Web/src/wayd.web.reactclient/src/app/planning/planning-intervals/[key]/objectives/[objectiveKey]/health-report/page.tsx` | Same. |

---

## Domain model — concrete shape

```csharp
// Wayd.Common.Domain/HealthChecks/HealthCheckBase.cs
public abstract class HealthCheckBase : BaseSoftDeletableEntity
{
    protected HealthCheckBase() { }

    protected HealthCheckBase(HealthStatus status, Guid reportedById, Instant reportedOn, Instant expiration, string? note)
    {
        Status = status;
        ReportedById = reportedById;
        ReportedOn = reportedOn;
        Expiration = expiration;
        Note = note;
    }

    public HealthStatus Status { get; private set; }
    public Guid ReportedById { get; private init; }
    public Employee ReportedBy { get; private set; } = default!;
    public Instant ReportedOn { get; private init; }

    public Instant Expiration
    {
        get;
        private set
        {
            if (value < ReportedOn)
                throw new ArgumentException("Expiration must be greater than or equal to ReportedOn.", nameof(Expiration));
            field = value;
        }
    }

    public string? Note { get; private set => field = value.NullIfWhiteSpacePlusTrim(); }

    public bool IsExpired(Instant now) => Expiration <= now;

    internal void ChangeExpiration(Instant expiration) => Expiration = expiration;

    internal Result Update(HealthStatus status, Instant expiration, string? note, Instant now)
    {
        if (IsExpired(now))
            return Result.Failure("Expired health checks cannot be modified.");

        if (expiration <= now)
            return Result.Failure("Expiration must be in the future.");

        Status = status;
        Expiration = expiration;
        Note = note;
        return Result.Success();
    }
}

// Wayd.Common.Domain/HealthChecks/HealthReport.cs
public sealed class HealthReport<THealthCheck> where THealthCheck : HealthCheckBase
{
    private readonly List<THealthCheck> _healthChecks;

    public HealthReport(IEnumerable<THealthCheck> existing)
    {
        _healthChecks = existing.OrderByDescending(h => h.ReportedOn).ToList();
    }

    public IReadOnlyList<THealthCheck> HealthChecks => _healthChecks.AsReadOnly();

    public Result Add(THealthCheck newCheck, Instant now)
    {
        var latest = _healthChecks.FirstOrDefault();
        if (latest is not null && now < latest.Expiration)
            latest.ChangeExpiration(now);

        _healthChecks.Insert(0, newCheck);
        return Result.Success();
    }
}
```

```csharp
// Wayd.Planning.Domain/Models/PlanningIntervalObjectiveHealthCheck.cs
public sealed class PlanningIntervalObjectiveHealthCheck : HealthCheckBase
{
    private PlanningIntervalObjectiveHealthCheck() { }

    internal PlanningIntervalObjectiveHealthCheck(
        Guid planningIntervalObjectiveId,
        HealthStatus status,
        Guid reportedById,
        Instant reportedOn,
        Instant expiration,
        string? note)
        : base(status, reportedById, reportedOn, expiration, note)
    {
        Guard.Against.Default(planningIntervalObjectiveId);
        PlanningIntervalObjectiveId = planningIntervalObjectiveId;
    }

    public Guid PlanningIntervalObjectiveId { get; private init; }
}
```

```csharp
// PlanningIntervalObjective changes (excerpt)
private readonly List<PlanningIntervalObjectiveHealthCheck> _healthChecks = [];
public IReadOnlyCollection<PlanningIntervalObjectiveHealthCheck> HealthChecks => _healthChecks.AsReadOnly();

public Result AddHealthCheck(HealthStatus status, Guid reportedById, Instant expiration, string? note, Instant now)
{
    if (expiration <= now)
        return Result.Failure("Expiration must be in the future.");

    var check = new PlanningIntervalObjectiveHealthCheck(Id, status, reportedById, now, expiration, note);

    var report = new HealthReport<PlanningIntervalObjectiveHealthCheck>(_healthChecks);
    var addResult = report.Add(check, now);
    if (addResult.IsFailure)
        return addResult;

    _healthChecks.Add(check);
    return Result.Success();
}
```

---

## Permission model

| Action | Resource | Notes |
|---|---|---|
| View health checks | `PlanningIntervalObjectives.View` | Already exists. |
| Create health check | `PlanningIntervalObjectives.Update` (or new sub-action) | Reporting health is "updating" the objective. |
| Update health check | `PlanningIntervalObjectives.Update` | Same. |
| Delete health check | `PlanningIntervalObjectives.Update` or `.Delete` | Decide based on existing convention. |

Confirm by reading `ApplicationResource.PlanningIntervalObjectives` permissions before implementing.

---

## Test plan

### Domain tests (move + update)

- Move `Wayd.Health.Tests/Sut/Models/HealthCheckTests.cs` and `HealthReportTests.cs` test cases into `Wayd.Common.Domain.Tests` (or wherever common-domain tests live) targeting `HealthCheckBase` / `HealthReport<T>`.
- New: `PlanningIntervalObjectiveHealthCheckTests` — round-trip create, the FK invariant, soft-delete.
- Updated: `PlanningIntervalObjective` tests around `AddHealthCheck` — verify the previous-check-truncation invariant.

### Application tests

- `CreateObjectiveHealthCheckCommandHandler`: happy path, expired-expiration rejected, current-user-employee-id resolution.
- `UpdateObjectiveHealthCheckCommandHandler`: rejects update on expired check.
- `DeleteObjectiveHealthCheckCommandHandler`: soft-deletes.
- `GetObjectiveHealthChecksQueryHandler`: returns full history ordered desc.
- `GetPlanningIntervalObjectivesQuery`: regression — `HealthCheck` field returns the most-recent non-expired check via projection.

### Architecture tests

- Add a rule that `Wayd.Planning.Domain` is the only assembly allowed to reference `PlanningIntervalObjectiveHealthCheck` (apart from infra config).
- Verify `Wayd.Health` no longer exists (delete its arch-test references).

### Migration smoke test

- Local: snapshot row counts in both source tables, run migration, verify destination row count = `Health.HealthChecks WHERE Context = 'PlanningPlanningIntervalObjective'` count, spot-check a few rows for `Note` and `ReportedById` preservation.

---

## Risks & mitigations

| Risk | Mitigation |
|---|---|
| Mapster nested projection generates ugly SQL | Verify with `.ToQueryString()` on the new list query. If bad, fall back to hand-written `Select` projection in the handler for the list query only. |
| Migration data loss if `Health.HealthChecks` has rows with `Context != PlanningPlanningIntervalObjective` | Run the pre-migration `SELECT DISTINCT Context` check manually before merging. Plan for those rows explicitly. |
| Frontend breaks due to NSwag regeneration | Run `npm run build` locally; fix any compile errors before merging. The TS client method names will change. |
| Permission downgrade for some users | Audit `ApplicationResource.HealthChecks` grants pre-migration; map them to `PlanningIntervalObjectives` grants in a data-fix step if needed. |
| `SystemContext.PlanningPlanningIntervalObjective` is referenced elsewhere | Grep before deleting. If used by another slice, keep the enum member but stop using it in this code path. |

---

## PR sequencing

1. **PR 1 (this plan):** Everything above. Ships PI objective health checks on the new model, deletes the old service.
2. **PR 2 (follow-up):** `ProjectHealthCheck : HealthCheckBase`, nested under project endpoints. Should be small — base class is already in place.
