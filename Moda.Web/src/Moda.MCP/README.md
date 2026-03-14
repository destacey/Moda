# @modanpm/moda-mcp

A [Model Context Protocol (MCP)](https://modelcontextprotocol.io) server for the [Moda](https://destacey.github.io/Moda) work management API. Exposes Moda's project portfolio management, planning, and work item data to AI assistants.

## Requirements

- Node.js >= 20
- A running Moda instance with API access
- A Moda Personal Access Token (PAT)

## Configuration

The server requires two values: the base URL of your Moda instance and an API key. These can be supplied as **environment variables** or **CLI arguments** — CLI arguments take priority if both are provided.

| | Environment variable | CLI argument |
|---|---|---|
| Base URL | `MODA_API_BASE_URL` | `--base-url` |
| API key | `MODA_API_KEY` | `--api-key` |

## Installation

### Claude Desktop

CLI arguments are not supported in Claude Desktop — use environment variables. Add to `~/Library/Application Support/Claude/claude_desktop_config.json` (macOS) or `%APPDATA%\Claude\claude_desktop_config.json` (Windows):

```json
{
  "mcpServers": {
    "moda": {
      "command": "npx",
      "args": ["-y", "@modanpm/moda-mcp"],
      "env": {
        "MODA_API_BASE_URL": "https://your-moda-instance.com",
        "MODA_API_KEY": "your-personal-access-token"
      }
    }
  }
}
```

### VS Code / Cursor (with `inputs`)

CLI args enable the `inputs` pattern, which prompts for values at connection time instead of hardcoding them. Add to `.vscode/mcp.json` or `.cursor/mcp.json`:

```json
{
  "servers": {
    "moda": {
      "type": "stdio",
      "command": "npx",
      "args": [
        "-y", "@modanpm/moda-mcp",
        "--base-url", "${input:modaBaseUrl}",
        "--api-key",  "${input:modaApiKey}"
      ],
      "inputs": [
        {
          "id": "modaBaseUrl",
          "description": "Moda base URL",
          "type": "promptString"
        },
        {
          "id": "modaApiKey",
          "description": "Moda API key (Personal Access Token)",
          "type": "promptString",
          "password": true
        }
      ]
    }
  }
}
```

Or with environment variables directly:

```json
{
  "servers": {
    "moda": {
      "type": "stdio",
      "command": "npx",
      "args": ["-y", "@modanpm/moda-mcp"],
      "env": {
        "MODA_API_BASE_URL": "https://your-moda-instance.com",
        "MODA_API_KEY": "your-personal-access-token"
      }
    }
  }
}
```

### Global install

```bash
npm install -g @modanpm/moda-mcp
```

Then use `moda-mcp` as the command instead of `npx -y @modanpm/moda-mcp` in any of the configs above.

## Available Tools

### Project Portfolio Management

| Category | Operations |
|---|---|
| **Portfolios** | List, get details, get programs, get projects |
| **Programs** | List, get details, get projects |
| **Projects** | List, get details, create, update, approve, activate, complete, change program/key |

### Planning

| Category | Operations |
|---|---|
| **Planning Intervals** | List, get details, calendar, predictability, teams, iterations, objectives, risks |
| **Roadmaps** | List, get details, get items and activities |

## Links

- [Moda documentation](https://destacey.github.io/Moda)
- [GitHub repository](https://github.com/destacey/Moda)
- [Report an issue](https://github.com/destacey/Moda/issues)
