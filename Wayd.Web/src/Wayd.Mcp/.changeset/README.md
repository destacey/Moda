# Changesets

This directory manages versioning and changelog generation for the `@wayd/mcp` npm package.

## What is a changeset?

A changeset is a small markdown file you add to a PR when your changes should result in a new npm release. It describes what changed and whether it's a `patch`, `minor`, or `major` bump (following [semver](https://semver.org)).

Not every PR needs a changeset — only ones that should produce a new published version.

## Adding a changeset to your PR

From the `Wayd.Web/src/Wayd.Mcp` directory:

```bash
npx changeset
```

The CLI will ask you to:

1. Choose a bump type (`patch` / `minor` / `major`)
2. Write a short summary of the change (this becomes the changelog entry)

Commit the generated `.changeset/*.md` file with your PR.

## Release flow

1. Your PR merges to `main` with a changeset file
2. A GitHub Action automatically opens or updates a **release PR** that contains the version bump and updated `CHANGELOG.md`
3. A maintainer reviews and merges the release PR
4. The package is automatically published to npm and a git tag is created

## Bump type guide

| Type | When to use | Example: current `0.2.1` → |
| ------ | ------------- | --------------------------- |
| `patch` | Bug fixes, internal changes, non-breaking tweaks | `0.2.2` |
| `minor` | New tools or features, backwards-compatible | `0.3.0` |
| `major` | Breaking changes to the MCP interface or config | `1.0.0` |

## When NOT to add a changeset

- Refactoring or cleanup with no behavior change you want to ship immediately
- Updating dev dependencies only
- Documentation-only changes
- Work-in-progress changes not ready to release

You can always merge to `main` without a changeset — nothing will be released until a changeset is present and the release PR is merged.
