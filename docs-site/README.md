# Moda Documentation Site

This site is built using [Docusaurus](https://docusaurus.io/) and deployed to GitHub Pages. It reads MDX content from the shared `docs/` folder at the repository root.

## Installation

```bash
npm install
```

## Local Development

```bash
npm start
```

This starts a local development server. Most changes are reflected live without restarting.

## Build

```bash
npm run build
```

Generates static content into the `build` directory.

## Deployment

The site is automatically deployed to GitHub Pages via the `deploy-docs.yml` GitHub Actions workflow when changes are pushed to `main` in the `docs/` or `docs-site/` directories.
