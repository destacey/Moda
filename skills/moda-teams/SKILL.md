---
name: moda-teams
description: Guides agents working with Moda Teams via the Moda MCP server. Use when looking up teams or resolving a team name to an ID.
---

# Moda Teams

## When to use

- Listing all teams in the organization
- Looking up a specific team's details
- Resolving a team name to an integer ID for use in other tools (e.g. Planning Interval team filters)

---

## Entity context

### Team

- Represents an organizational team in Moda
- Team `id` is an **integer** (not a UUID) — this is different from most other Moda entities
- Teams can be active or inactive; `Teams_GetTeams` returns active teams by default
- Pass `includeInactive: true` to include inactive teams

---

## Instructions

### Listing teams

- All active teams: `Teams_GetTeams`
- Include inactive teams: `Teams_GetTeams` with `includeInactive: true`

### Getting a specific team

`Teams_GetTeam` requires `id` (integer). If you only have a name, call `Teams_GetTeams` first to resolve name → ID.

### Common usage patterns

- **PI team filters** — `PlanningIntervals_GetTeams`, `PlanningIntervals_GetPredictability`, and related endpoints take a `teamId` (UUID from the PI context, not the organization team integer ID). Use `PlanningIntervals_GetTeams` for those — reserve `Teams_GetTeams` for organization-level team lookups.
- **Project team roles** — `sponsorIds`, `ownerIds`, `managerIds`, `memberIds` on projects take user UUIDs, not team IDs. Use `Users_GetUsers` to resolve those.
