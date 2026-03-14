# Moda Domain Model Reference

This document is a reference for agents using the Moda MCP server. It is not a skill — it is referenced by skills for entity definitions, relationships, and common patterns.

---

## Entity Hierarchy

```object-tree
Portfolio
└── Program (optional grouping)
    └── Project
        └── Work Items

Planning Interval (PI)
├── Iterations
│   └── Sprints (mapped from external systems, e.g. Azure DevOps)
├── Teams
│   └── Objectives
│       └── Work Items (linked)
└── Risks

Roadmap
└── Items
    ├── Activity  (work spanning a time range)
    ├── Timebox   (time-bounded container)
    └── Milestone (point-in-time event)
```

---

## Entity Definitions

### Portfolio

- Top-level container for programs and projects
- Has a status (integer enum — call `Portfolios_GetPortfolioStatuses` to resolve values)
- Can directly contain projects without a program

### Program

- Groups related projects under a portfolio
- Optional — projects can exist without a program
- Has a status (integer enum — call `Programs_GetProgramStatuses` to resolve values)

### Project

- Core unit of delivery work
- Must belong to a portfolio; optionally belongs to a program
- Has a unique string `key` (2–20 uppercase alphanumeric characters, e.g. `MYPROJ`)
- Has a status (integer enum — call `Projects_GetStatuses` to resolve values)
- Team roles: `sponsorIds`, `ownerIds`, `managerIds`, `memberIds` (all UUID arrays, all optional)
- `expenditureCategoryId` — required integer on create; ask the user or inspect existing projects for valid values
- `strategicThemeIds` — optional UUID array

**Lifecycle (action endpoints, not a patchable status field):**

```lifecycle
Proposed → Approved → Active → Completed
           ↑                   ↑         ↑
     Projects_Approve   Projects_Activate  Projects_Complete
```

### Planning Interval (PI)

- A time-boxed planning period (analogous to a Program Increment in SAFe)
- Contains iterations and has teams participating
- Each team has objectives for the PI

### Iteration

- Sub-period within a PI (e.g. Sprint 1, Sprint 2)
- Has a category (call `PlanningIntervals_GetIterationCategories` to resolve)
- Maps to external sprints (from Azure DevOps or similar) for metrics

### Objective

- A team-scoped goal for a PI
- Has a status (call `PlanningIntervals_GetObjectiveStatuses` to resolve values)
- Can have linked work items with daily metrics

### Risk

- Scoped to a PI; optionally scoped to a team
- Open risks are returned by default; closed risks must be explicitly requested

### Roadmap Item

Three concrete types:

| Type | Description |
| --- | --- |
| Activity | Work or effort spanning a date range |
| Timebox | A time-bounded container (e.g. a quarter or PI) |
| Milestone | A single point-in-time event |

`GetItems` returns all three types. `GetActivities` returns activities only.

---

## Common Patterns

### `idOrKey`

Most GET endpoints accept `idOrKey` — either a UUID or a human-readable string key. Use whichever the user provides; both are valid.

### Status filters take integer arrays

`status` query parameters accept an **array of integers** (not strings). Always call the corresponding `GetStatuses` endpoint first to resolve the enum values before filtering.

Example: to filter active projects, call `Projects_GetStatuses`, find the integer for "Active", then pass `status: [3]` (or whatever integer it maps to).

### UUID references

Cross-entity references (`portfolioId`, `programId`, `teamId`, `objectiveId`, etc.) are always UUIDs. If the user gives a name, resolve it first with the appropriate list/options endpoint.

### `Portfolios_GetPortfolioOptions`

Returns a lightweight list of `{ id, name }` pairs — prefer this over `GetPortfolios` when you only need to resolve a name to a UUID.

### Optional filters

- `teamId` on PI endpoints — omit to get all teams; include to scope to one team
- `includeClosed` on `GetRisks` — defaults to `false`; pass `true` to include resolved risks
- `portfolioId` / `programId` on `GetProjects` / `GetPrograms` — optional cross-filters
