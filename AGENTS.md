# AGENTS.md

This file provides context for AI agents working with the Moda codebase.

## What is Moda?

Moda is an intelligent delivery management platform designed to give engineering leaders and teams end-to-end visibility into software delivery. When delivery spans multiple teams, projects, and systems, visibility breaks down. Moda brings it all together — tracking work items, aligning teams to planning intervals and products, and surfacing dependencies across the organization.

**Core Philosophy:** Moda acts as a unified hub that synchronizes data from multiple business systems and combines it with capabilities those systems lack — connecting the dots so teams can see the full picture in one place.

Built with Clean Architecture, Domain-Driven Design, and a modular monolith approach with a shared database.

## Quick Start for Agents

1. **Read `CLAUDE.md`** for build commands, architecture, and coding conventions
2. **Read `docs/llms-full.txt`** for comprehensive domain context (entities, relationships, business rules)
3. **Read `docs/ai/domain-glossary.mdx`** for domain terminology

## Repository Structure

```
Moda/
  Moda.Common/                    # Shared libraries and base abstractions
  Moda.Services/                  # 9 vertical slice domain services
    Moda.Work/                    # Work items, workspaces, processes, workflows
    Moda.Organization/            # Teams, employees, memberships
    Moda.Planning/                # PIs, sprints, objectives, risks, roadmaps
    Moda.Goals/                   # Objectives and key results (skeleton)
    Moda.ProjectPortfolioManagement/  # Portfolios, programs, projects, tasks
    Moda.StrategicManagement/     # Visions, strategies, themes
    Moda.AppIntegration/          # Integration configuration
    Moda.Links/                   # Cross-entity relationships
    Moda.Health/                  # Health checks
  Moda.Infrastructure/            # EF Core, auth, jobs, logging
  Moda.Integrations/              # Azure DevOps, Microsoft Graph
  Moda.Web/
    Moda.Web.Api/                 # ASP.NET Core Web API
    moda.web.reactclient/         # Next.js 16 / React 19 frontend
  docs/                           # Documentation (MDX, shared by Docusaurus and Next.js)
  docs-site/                      # Docusaurus config for GitHub Pages
```

## Architecture Rules

- **Domain** has zero external dependencies — only entities, value objects, domain events
- **Application** depends only on Domain — commands, queries, handlers, DTOs, validators
- **Infrastructure** depends on Application and Domain — EF Core, auth, background jobs
- **Web API** depends on all layers — thin controllers delegating to MediatR
- Architecture tests in `Moda.ArchitectureTests` enforce these rules

## Key Patterns

| Pattern | Implementation |
|---------|---------------|
| CQRS | MediatR commands and queries |
| Error handling | `Result<T>` from CSharpFunctionalExtensions (no exceptions for business logic) |
| Data access | EF Core DbContext directly (no repository pattern) |
| Validation | FluentValidation (auto-registered with MediatR pipeline) |
| Mapping | Mapster (DTOs) |
| Time | NodaTime (`Instant`, `LocalDate`). Always inject `IDateTimeProvider`. |
| Frontend API calls | NSwag-generated typed client. Never use `authenticatedFetch()` directly. |
| Frontend styling | Ant Design theme tokens via CSS variables. Never hardcode colors. |
| Async naming | Do NOT add `Async` suffix to new async methods |

## Domain Services Overview

### Organization
Teams (Scrum/Kanban), Teams of Teams (hierarchy), Operating Models (methodology + sizing), Team Memberships (date-ranged parent-child with Past/Active/Future states).

### Planning
Planning Intervals (8-12 week PIs with iterations), Sprints (team-owned, mapped to PI iterations), Objectives (team commitments with predictability tracking), Risks (ROAM model), Roadmaps (activities/milestones/timeboxes), Planning Poker (real-time estimation).

### Work Management
Workspaces (containers using work processes), Work Items (hierarchical with dependencies and revision tracking), Work Processes (type-to-workflow mappings), Work Types (Portfolio/Requirement/Task/Other tiers), Workflows (status progressions), Work Statuses (normalized to 4 categories: Proposed/Active/Done/Removed).

### Project Portfolio Management
Portfolios (top-level containers), Programs (project groups), Projects (lifecycle phases, tasks with WBS, dependencies), Strategic Initiatives (KPI tracking with checkpoints/measurements), Expenditure Categories.

### Strategic Management
Visions (one Active at a time), Strategies, Strategic Themes (cross-domain tags on projects/programs/initiatives).

## Cross-Domain Relationships

- Teams → participate in PIs, own Sprints, commit Objectives, track Risks, assigned to Work Items
- Sprints → mapped to PI Iterations, contain Work Items
- Objectives → linked to Work Items
- Work Items → assigned to Teams and Sprints, associated with Projects
- Strategic Themes → tag Projects, Programs, and Strategic Initiatives
- Portfolio tier Work Items → can be associated with PPM Projects (children inherit)

## Documentation

- **User docs**: `docs/user-guide/` — Organizations, Planning, Work Management, PPM, Strategic Management, Administration
- **Developer docs**: `docs/contributing/` — Architecture, coding standards, testing, adding features
- **Reference**: `docs/reference/` — Domain model, API, feature flags, integrations, tech stack
- **AI context**: `docs/ai/domain-glossary.mdx`, `docs/llms.txt`, `docs/llms-full.txt`
