---
name: moda-users
description: Guides agents working with Moda Users via the Moda MCP server. Use when looking up users or resolving a user name to a UUID for assignees or project team roles.
---

# Moda Users

## When to use

- Listing all users in the system
- Looking up a specific user's details
- Resolving a user name to a UUID for use in other tools (e.g. task `assigneeIds`, project `ownerIds`/`managerIds`/`memberIds`)

---

## Entity context

### User

- Represents a Moda user account
- User `id` is a **UUID** (string)
- Users can be active or inactive; `Users_GetUsers` returns all users

---

## Instructions

### Listing users

- All users: `Users_GetUsers`

### Getting a specific user

`Users_GetUser` requires `id` (UUID string). If you only have a name, call `Users_GetUsers` first to resolve name → UUID.

### Common usage patterns

- **Task assignees** — `Tasks_CreateProjectTask` and `Tasks_UpdateProjectTask` accept `assigneeIds` (UUID array). Resolve names → UUIDs with `Users_GetUsers` before calling.
- **Project team roles** — `sponsorIds`, `ownerIds`, `managerIds`, `memberIds` on `Projects_Create` / `Projects_Update` all take UUID arrays. Use `Users_GetUsers` to resolve.
- When multiple users need resolving, make a single `Users_GetUsers` call and filter the result — do not make individual `Users_GetUser` calls per person.
