---
name: moda-ppm
description: Guides agents working with Moda Portfolio, Program, and Project management via the Moda MCP server. Use when looking up, creating, updating, or transitioning projects, programs, or portfolios.
---

# Moda PPM (Portfolio / Program / Project Management)

## When to use

- Finding or listing portfolios, programs, or projects
- Creating or updating a project
- Transitioning a project through its lifecycle (approve, activate, complete)
- Understanding what projects or programs are in a portfolio
- Changing a project's program assignment or key

---

## Entity context

### Hierarchy

```
Portfolio
└── Program (optional grouping)
    └── Project
        └── Work Items
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
- Team roles: `sponsorIds`, `ownerIds`, `managerIds`, `memberIds` (UUID arrays, all optional)
- `expenditureCategoryId` — required integer on create; ask the user if unknown
- `strategicThemeIds` — optional UUID array

**Lifecycle — action endpoints, not a patchable status field:**

```
Proposed → Approved → Active → Completed
         ↑           ↑         ↑
  Projects_Approve  Projects_Activate  Projects_Complete
```

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
| Portfolio name → UUID lookup | `Portfolios_GetPortfolioOptions` |
| Programs in a portfolio | `Portfolios_GetPortfolioPrograms` |
| Projects in a portfolio | `Portfolios_GetPortfolioProjects` |
| Projects in a program | `Programs_GetProgramProjects` |
| All projects (cross-portfolio) | `Projects_GetProjects` |

Before filtering by status, call `Projects_GetStatuses` (or `Programs_GetProgramStatuses` / `Portfolios_GetPortfolioStatuses`) to resolve the integer enum values.

### Creating a project

Required fields: `name`, `description`, `key`, `expenditureCategoryId`, `portfolioId`

1. If `portfolioId` is not known, call `Portfolios_GetPortfolioOptions` to resolve portfolio name → UUID.
2. `key` must be 2–20 uppercase alphanumeric characters (e.g. `ALPHA`, `PROJ01`). Ask the user if not provided.
3. `expenditureCategoryId` is a required integer. If unknown, ask the user — there is no dedicated lookup endpoint exposed via MCP.
4. Optional: `programId`, `start`, `end` (ISO date strings `YYYY-MM-DD`), `sponsorIds`, `ownerIds`, `managerIds`, `memberIds`, `strategicThemeIds` (all UUIDs).
5. Call `Projects_Create` with the assembled `requestBody`.

### Updating a project

`Projects_Update` updates core fields: `name`, `description`, `expenditureCategoryId`, `start`, `end`, and team role arrays.

**These are separate endpoints — not part of `Projects_Update`:**

- Change the project's `key`: `Projects_ChangeKey` with `{ key: "NEWKEY" }`
- Change or remove the program: `Projects_ChangeProgram` with `{ programId: "<uuid>" }` or `{ programId: null }` to remove

### Project lifecycle transitions

Projects move through states via dedicated action endpoints. You cannot patch the status field directly.

```
Proposed → Approved   →   Active   →   Completed
         Projects_Approve  Projects_Activate  Projects_Complete
```

Each action endpoint takes only the project `id` (UUID). Resolve it first with `Projects_GetProject` or `Projects_GetProjects` if needed.

### Getting work items for a project

`Projects_GetWorkItems` — takes the project `id` (UUID, not idOrKey).
