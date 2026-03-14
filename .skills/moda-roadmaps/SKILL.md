---
name: moda-roadmaps
description: Guides agents working with Moda Roadmaps — exploring activities, timeboxes, milestones, and roadmap item details.
---

# Moda Roadmaps

## When to use

- Finding or listing roadmaps
- Exploring roadmap items (activities, timeboxes, milestones)
- Getting details for a specific roadmap item
- Understanding what's planned or visualized in a roadmap

## Domain context

Read `.skills/moda-domain.md` for roadmap item type definitions (Activity, Timebox, Milestone) and common idOrKey patterns.

---

## Instructions

### Roadmap item types

Roadmaps contain three distinct item types:

| Type | Description |
|---|---|
| **Activity** | Work or effort that spans a date range |
| **Timebox** | A time-bounded container (e.g. a quarter, a PI) |
| **Milestone** | A single point-in-time event |

`GetItems` returns all three types in one call. `GetActivities` returns only activities.

### Navigating a roadmap

1. List all roadmaps: `Roadmaps_GetRoadmaps`
2. Get roadmap details: `Roadmaps_GetRoadmap` with `idOrKey`
3. Get all items: `Roadmaps_GetItems` with `idOrKey`
4. Get only activities: `Roadmaps_GetActivities` with `idOrKey`
5. Get visibility options: `Roadmaps_GetVisibilityOptions` (reference list)

### Getting a specific item

`Roadmaps_GetItem` requires:
- `roadmapIdOrKey` — supports both UUID and string key
- `itemId` — **UUID only** (items do not have string keys)

If you only have the item's name, call `Roadmaps_GetItems` first to resolve the UUID.
