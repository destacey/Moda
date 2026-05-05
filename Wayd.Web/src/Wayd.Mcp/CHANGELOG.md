# @wayd/mcp

## 0.1.0

### Minor Changes

- bcba8b3: Add project lifecycle tools, new project read endpoints, and remove the deprecated task tree tool.

  **New tools:**

  - `ProjectLifecycles_GetProjectLifecycles` — list project lifecycles with optional state filter (Proposed/Active/Archived)
  - `ProjectLifecycles_GetProjectLifecycle` — get project lifecycle details including phases
  - `Projects_GetProjectTeam` — get team members for a project with their roles, assigned phases, and active workload
  - `Projects_GetProjectPhases` — get all phases for a project
  - `Projects_GetProjectPhase` — get details for a specific project phase
  - `Projects_GetProjectPlanTree` — get a unified plan tree with phases as top-level nodes and tasks nested within (replaces the removed task tree endpoint)
  - `Projects_GetProjectPlanSummary` — get summary metrics for a project's plan (overdue, due this week, upcoming, total task counts)

  **Updated tools:**

  - `Projects_GetProjects` — added `role` query filter to scope results to projects where the current user holds a specific role (Sponsor/Owner/Manager/Member)

  **Removed tools:**

  - `Tasks_GetProjectTaskTree` — the underlying API endpoint was removed; use `Projects_GetProjectPlanTree` instead for a richer unified view of phases and tasks
