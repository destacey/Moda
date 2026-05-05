---
name: wayd-users
description: Guides agents working with Wayd Users via the Wayd MCP server. Use when looking up users or resolving a user name to a UUID for assignees or project team roles.
---

# Wayd Users

## When to use

- Listing all users in the system
- Looking up a specific user's details
- Resolving a user name to a UUID for use in other tools (e.g. task `assigneeIds`, project `ownerIds`/`managerIds`/`memberIds`)

---

## Entity context

### User

- Represents a Wayd user account
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
- **Project team roles** — project team role fields (`sponsorIds`, `ownerIds`, `managerIds`, `memberIds`) take UUID arrays. Use `Users_GetUsers` to resolve user names → UUIDs before passing them to any tool that accepts these fields.
- When multiple users need resolving, make a single `Users_GetUsers` call and filter the result — do not make individual `Users_GetUser` calls per person.
