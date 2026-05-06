---
"@wayd/mcp": minor
---

Add MCP tools for inspecting and creating health checks on planning interval objectives and PPM projects.

**New tools:**

- `PlanningIntervals_GetObjectiveHealthChecks` — get the full health check history for a PI objective (newest first)
- `PlanningIntervals_GetObjectiveHealthCheck` — get a specific PI objective health check by ID
- `PlanningIntervals_CreateObjectiveHealthCheck` — log a new health check on a PI objective (`statusId` 1=Healthy, 2=AtRisk, 3=Unhealthy); auto-expires the previously active check
- `Projects_GetProjectHealthChecks` — get the full health check history for a project (newest first)
- `Projects_GetProjectHealthCheck` — get a specific project health check by ID
- `Projects_CreateProjectHealthCheck` — log a new health check on a project (`status` "Healthy" / "AtRisk" / "Unhealthy"); auto-expires the previously active check
