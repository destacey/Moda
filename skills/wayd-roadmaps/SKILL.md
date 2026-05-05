---
name: wayd-roadmaps
description: Guides agents working with Wayd Roadmaps — exploring activities, timeboxes, milestones, and roadmap item details.
---

# Wayd Roadmaps

## When to use

- Finding or listing roadmaps
- Exploring roadmap items (activities, timeboxes, milestones)
- Getting details for a specific roadmap item
- Understanding what's planned or visualized in a roadmap

---

## Entity context

### Roadmap item types

Roadmaps contain three distinct item types:

| Type | Description |
|---|---|
| **Activity** | Work or effort spanning a date range |
| **Timebox** | A time-bounded container (e.g. a quarter, a PI) |
| **Milestone** | A single point-in-time event |

`GetItems` returns all three types. `GetActivities` returns only activities.

### Common patterns

- **`idOrKey`** — `roadmapIdOrKey` accepts either a UUID or a string key
- **`itemId`** — roadmap items are UUID-only (no string key); resolve via `GetItems` if you only have a name

---

## Instructions

### Navigating a roadmap

1. List all roadmaps: `Roadmaps_GetRoadmaps`
2. Get roadmap details: `Roadmaps_GetRoadmap` with `idOrKey`
3. Get all items (activities + timeboxes + milestones): `Roadmaps_GetItems` with `idOrKey`
4. Get only activities: `Roadmaps_GetActivities` with `idOrKey`
5. Get visibility options (reference list): `Roadmaps_GetVisibilityOptions`

### Getting a specific item

`Roadmaps_GetItem` requires:
- `roadmapIdOrKey` — UUID or string key
- `itemId` — UUID only

If you only have the item's name, call `Roadmaps_GetItems` first to resolve the UUID.
