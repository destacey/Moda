---
name: moda-ppm
description: Guides agents working with Moda Portfolio, Program, Project, and Task management via the Moda MCP server. Use when looking up portfolios, programs, or projects, or when creating, updating, or managing tasks within a project.
---

# Moda PPM (Portfolio / Program / Project / Task Management)

## When to use

- Finding or listing portfolios, programs, or projects
- Understanding what projects or programs are in a portfolio
- Listing, creating, updating, or deleting tasks within a project
- Managing task hierarchies, dependencies, or the critical path

> **Note:** Portfolios, programs, and projects are **read-only** via MCP — only GET and LIST operations are exposed. Task management (create, update, delete) is fully supported.

---

## Entity context

### Hierarchy

```
Portfolio
└── Program (optional grouping)
    └── Project
        ├── Work Items
        └── Tasks
            └── Subtasks (nested via parentId)
```

### Portfolio

- Top-level container for programs and projects
- Has a status (integer enum — call `Portfolios_GetPortfolioStatuses` to resolve values)

### Program

- Groups related projects under a portfolio; projects can exist without one
- Has a status (integer enum — call `Programs_GetProgramStatuses` to resolve values)

### Project

- Must belong to a portfolio; optionally belongs to a program
- Has a unique string `key` (2–20 uppercase alphanumeric, e.g. `MYPROJ`)
- Has a status (integer enum — call `Projects_GetStatuses` to resolve values)

### Task

- Scoped to a project; accessed via `projectIdOrKey` (UUID or string key)
- Has a **type** (call `Tasks_GetTaskTypes` to resolve), **status** (`Tasks_GetTaskStatuses`), and **priority** (`Tasks_GetTaskPriorities`)
- Three task types exist: regular tasks, subtasks, and milestones — behavior differs slightly per type:
  - Regular tasks and subtasks: use `plannedStart`/`plannedEnd` and `progress` (0.0–100.0)
  - Milestones: use `plannedDate` instead; `progress` is not applicable
- Supports parent/child nesting via `parentId` (UUID of the parent task)
- `assigneeIds` — optional UUID array; resolve user names → UUIDs with `Users_GetUsers`
- `estimatedEffortHours` — optional decimal
- Dependencies are finish-to-start: predecessor must complete before successor starts
- `taskIdOrKey` — GET endpoints accept either a UUID or a string key

### Common patterns

- **`idOrKey`** — most GET endpoints accept either a UUID or a string key
- **Status filters** — take integer arrays; always call the matching `GetStatuses` endpoint first to resolve enum values
- **UUID references** — `portfolioId`, `programId`, etc. are always UUIDs; resolve name → UUID with list/options endpoints
- **`Portfolios_GetPortfolioOptions`** — lightweight `{ id, name }` list; prefer this over `GetPortfolios` when you only need a UUID lookup

---

## Instructions

### Listing and filtering

| Goal | Tool |
|---|---|
| All portfolios (optionally by status) | `Portfolios_GetPortfolios` |
| Portfolio details | `Portfolios_GetPortfolio` |
| Portfolio name → UUID lookup | `Portfolios_GetPortfolioOptions` |
| Programs in a portfolio | `Portfolios_GetPortfolioPrograms` |
| Projects in a portfolio | `Portfolios_GetPortfolioProjects` |
| All programs (cross-portfolio) | `Programs_GetPrograms` |
| Projects in a program | `Programs_GetProgramProjects` |
| All projects (cross-portfolio) | `Projects_GetProjects` |
| Project details | `Projects_GetProject` |

Before filtering by status, call `Projects_GetStatuses` (or `Programs_GetProgramStatuses` / `Portfolios_GetPortfolioStatuses`) to resolve the integer enum values.

### Getting work items for a project

`Projects_GetWorkItems` — takes the project `id` (UUID, not idOrKey).

### Listing and navigating tasks

| Goal | Tool | Notes |
|---|---|---|
| All tasks in a project | `Tasks_GetProjectTasks` | Optional `status` (int) and `parentId` (UUID) filters |
| Task hierarchy with WBS codes | `Tasks_GetProjectTaskTree` | Returns nested structure |
| Single task details | `Tasks_GetProjectTask` | `taskIdOrKey` accepts UUID or string key |
| Critical path | `Tasks_GetCriticalPath` | Returns ordered list of task UUIDs |

Before filtering by status, call `Tasks_GetTaskStatuses` to resolve the integer enum values.

### Creating a task

Required fields: `name`, `typeId`, `statusId`, `priorityId`

1. Resolve reference values first (can be done in parallel):
   - `Tasks_GetTaskTypes` → `typeId`
   - `Tasks_GetTaskStatuses` → `statusId`
   - `Tasks_GetTaskPriorities` → `priorityId`
2. For assignees, resolve user name → UUID with `Users_GetUsers`.
3. For subtasks, provide `parentId` (UUID of the parent task).
4. For milestones: use `plannedDate`; omit `plannedStart`/`plannedEnd` and `progress`.
5. Call `Tasks_CreateProjectTask` with the assembled `requestBody`.

### Updating a task

`Tasks_UpdateProjectTask` — requires `id` (UUID), `name`, `statusId`, `priorityId` in the request body. All other fields are optional patches.

### Deleting a task

`Tasks_DeleteProjectTask` — requires `projectIdOrKey` and `id` (UUID).

### Managing dependencies

- **Add** (finish-to-start): `Tasks_AddTaskDependency` with `{ predecessorId, successorId }` — both UUIDs. Also pass the predecessor task's `id` as the path parameter.
- **Remove**: `Tasks_RemoveTaskDependency` with path params `id` (predecessor UUID) and `successorId`.
