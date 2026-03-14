---
name: moda-pi
description: Guides agents working with Moda Planning Intervals — iterations, sprint metrics, team objectives, health reports, predictability, and risks.
---

# Moda Planning Intervals (PI)

## When to use

- Finding or exploring planning intervals and their iterations
- Getting sprint mappings, iteration metrics, or iteration backlogs
- Listing or analyzing team objectives and their linked work items
- Generating PI health reports or predictability summaries
- Reviewing PI risks

## Domain context

Read `.skills/moda-domain.md` for entity definitions, the PI → Iteration → Sprint hierarchy, objective lifecycle, and common patterns (idOrKey, teamId scoping, includeClosed behavior).

---

## Instructions

### Finding PIs and iterations

1. List all PIs: `PlanningIntervals_GetList`
2. Get PI details: `PlanningIntervals_GetPlanningInterval` with `idOrKey`
3. Get PI calendar: `PlanningIntervals_GetCalendar` with `idOrKey`
4. List iterations in a PI: `PlanningIntervals_GetIterations`
5. Get a specific iteration: `PlanningIntervals_GetIteration` with `idOrKey` + `iterationIdOrKey`
6. Resolve iteration category values: `PlanningIntervals_GetIterationCategories`

### Sprint mappings and iteration metrics

These require the PI resolved first:

| Goal | Tool | Required params |
|---|---|---|
| Sprint mappings for the whole PI | `PlanningIntervals_GetIterationSprints` | `idOrKey` |
| Sprint mappings for one iteration | `PlanningIntervals_GetIterationSprints` | `idOrKey`, `iterationId` (UUID) |
| Aggregated metrics for an iteration | `PlanningIntervals_GetIterationMetrics` | `idOrKey`, `iterationIdOrKey` |
| Combined backlog for an iteration | `PlanningIntervals_GetIterationBacklog` | `idOrKey`, `iterationIdOrKey` |

### Objectives

- List objectives (all teams): `PlanningIntervals_GetObjectives` with `idOrKey`
- List objectives for one team: add `teamId` (UUID) filter
- Get a specific objective: `PlanningIntervals_GetObjective` with `idOrKey` + `objectiveIdOrKey`
- Resolve objective status values: `PlanningIntervals_GetObjectiveStatuses`
- Work items linked to an objective: `PlanningIntervals_GetObjectiveWorkItems`
- Daily work item metrics for an objective: `PlanningIntervals_GetObjectiveWorkItemMetrics`

### Health report

The health report is a dedicated endpoint — do not attempt to derive it from objectives manually.

- All teams: `PlanningIntervals_GetObjectivesHealthReport` with `idOrKey`
- Scoped to one team: add `teamId` filter

### Predictability

- All teams: `PlanningIntervals_GetPredictability` with `idOrKey`
- One team: `PlanningIntervals_GetTeamPredictability` with `idOrKey` + `teamId` (UUID)
- If `teamId` is unknown, call `PlanningIntervals_GetTeams` first to list participating teams.

### Risks

- `PlanningIntervals_GetRisks` — defaults to open risks only (`includeClosed` defaults to `false`)
- Pass `includeClosed: true` to include resolved/closed risks
- Pass `teamId` to scope to one team

**Reminder**: if the user asks "why aren't there more risks" or seems to be missing risks, suggest adding `includeClosed: true`.

### PI health report recipe (compound task)

When the user asks for a comprehensive PI health summary, run these in parallel then synthesize:

1. `PlanningIntervals_GetPlanningInterval` — PI dates, name, metadata
2. `PlanningIntervals_GetObjectivesHealthReport` — objective status across teams
3. `PlanningIntervals_GetPredictability` — predictability scores per team
4. `PlanningIntervals_GetRisks` — active risks (add `includeClosed: true` if full picture needed)
