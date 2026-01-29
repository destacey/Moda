# Team Operating Model

## Overview

Define how a team works so the system can adapt UI, metrics, and calculations accordingly.

## Problem Statement

Not all teams work the same way:
- Some teams use sprints (Scrum), others use continuous flow (Kanban)
- Some teams estimate with story points, others count items, others don't estimate
- The system needs to know how a team works to show relevant UI and calculate meaningful metrics

Without knowing the operating model:
- UI shows irrelevant features (sprint tabs for Kanban teams)
- Metrics are calculated incorrectly or become meaningless
- Aggregations across teams mix incompatible units

## Domain Model

### Entity: TeamOperatingModel

| Property | Type | Required | Notes |
|----------|------|----------|-------|
| Id | Guid | Yes | Primary key |
| TeamId | Guid | Yes | FK to Team |
| DateRange | OperatingModelDateRange | Yes | Start required, End nullable |
| Methodology | Methodology | Yes | Scrum, Kanban |
| SizingMethod | SizingMethod | Yes | StoryPoints, Count |

### Enums

```csharp
public enum Methodology
{
    Scrum,   // Sprint backlog, time-boxed planning
    Kanban   // Pull from product backlog, continuous flow
}

public enum SizingMethod
{
    StoryPoints,  // Relative sizing
    Count         // Item count (each item = 1)
}
// Future: Hours
```

### Value Object: OperatingModelDateRange

| Property | Type | Required | Notes |
|----------|------|----------|-------|
| Start | LocalDate | Yes | When this model became effective |
| End | LocalDate | No | When this model ended (null = current) |

## Business Rules

1. A team can have zero or more operating models
2. Only one operating model can be "current" per team (End = null)
3. Date ranges cannot overlap for the same team
4. When creating a new model, the previous model's End date is set automatically
5. Operating model is **optional** on team creation
6. Teams without an operating model have limited functionality until one is defined

## Location

Organization domain (`Moda.Organization`)

## Cross-Domain Access

MediatR queries for other domains to access operating model information:

```csharp
// Single team query
public record GetTeamOperatingModelQuery(Guid TeamId, LocalDate? AsOfDate = null)
    : IRequest<TeamOperatingModelDto?>;

// Batch query for multiple teams
public record GetTeamOperatingModelsQuery(IEnumerable<Guid> TeamIds, LocalDate? AsOfDate = null)
    : IRequest<Dictionary<Guid, TeamOperatingModelDto>>;
```

When `AsOfDate` is null, returns the current model. When specified, returns the model active on that date.

**Note:** Returns `null` when no operating model is defined (not defaults).

## UI Requirements

### Team Details Page

#### Sprints Tab
- **Show if:** Team has ever had an operating model with `Methodology = Scrum`
- **Hide if:** Team has never been Scrum (no operating model, or only Kanban history)
- **Rationale:** Allows historical access to sprint data even if team is now Kanban

#### Active Sprint Component
- **Show if:** Team's current operating model has `Methodology = Scrum`
- **Hide if:** No current operating model OR current model is Kanban

#### Action Menu
- Include menu item to "Set Operating Model" / "Update Operating Model" / "Manage Operating Model"
- Allows admin to define or change how the team works

### Sprint Details Page

- Uses operating model **at the time of the sprint** for defaults
- **If team uses StoryPoints:**
  - Show both calculation options (Story Points, Count)
  - Default to Story Points
- **If team uses Count:**
  - Only show Count option (Story Points not relevant)

### Active Sprint Component

- Uses **current** operating model for defaults
- Default metric calculation based on team's SizingMethod

### PI Iteration Page

- Aggregates across multiple teams with potentially different sizing methods
- Always show both options (Story Points, Count)
- **TBD:** How to handle aggregation when teams have mixed sizing methods

## Migration Strategy

For existing teams:
- Create migration that adds `TeamOperatingModel` records for all existing teams
- Default values: `Methodology = Scrum`, `SizingMethod = StoryPoints`
- `DateRange.Start` = team's `ActiveDate`, `End` = null
- Admins review and correct after release

## Future Considerations

- **Cadence property:** For Scrum teams, capture sprint length (1-4 weeks)
- **Hours sizing method:** Add as third option for time-based estimation
- **Hybrid methodology:** Teams that blend Scrum and Kanban practices
- **Capacity tracking:** Whether team tracks capacity
- **Velocity settings:** Calculation method, rolling average window

## Open Questions

1. PI Iteration aggregation: When teams have mixed sizing methods, how should we aggregate?
   - Option A: Treat Count items as 1 SP each
   - Option B: Show separate totals ("120 SP + 45 items")
   - Option C: Normalize to Count when mixing
   - Option D: Show breakdown by team/method
