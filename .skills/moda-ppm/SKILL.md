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

## Domain context

Read `.skills/moda-domain.md` for entity definitions, the Portfolio → Program → Project hierarchy, status lifecycle, and common patterns (idOrKey, integer status filters, UUID references).

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

**Important splits — these are separate endpoints:**

- Change the project's `key`: `Projects_ChangeKey` with `{ key: "NEWKEY" }`
- Change or remove the program: `Projects_ChangeProgram` with `{ programId: "<uuid>" }` or `{ programId: null }` to remove

### Project lifecycle transitions

Projects move through states via dedicated action endpoints. You **cannot** patch the status field directly.

```
Proposed → Approved   →   Active   →   Completed
         Projects_Approve  Projects_Activate  Projects_Complete
```

Each action endpoint takes only the project `id` (UUID). Resolve the UUID first with `Projects_GetProject` or `Projects_GetProjects` if needed.

### Getting work items for a project

`Projects_GetWorkItems` — takes the project `id` (UUID, not idOrKey).
