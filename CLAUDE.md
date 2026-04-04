# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

Moda is an intelligent delivery management platform designed to give engineering leaders and teams end-to-end visibility into software delivery. It acts as a unified hub that synchronizes data from multiple business systems and combines it with capabilities those systems lack — connecting the dots so teams can see the full picture in one place. Built with Clean Architecture, Domain-Driven Design, and a modular monolith approach with a shared database.

**For domain context** (entities, relationships, business rules): See [AGENTS.md](AGENTS.md) and [docs/llms-full.txt](docs/llms-full.txt)
**For domain terminology**: See [docs/ai/domain-glossary.mdx](docs/ai/domain-glossary.mdx)
**For user-facing documentation**: See [docs/](docs/) (shared by Docusaurus and Next.js in-app docs)

Documentation site: <https://destacey.github.io/Moda>

## Build and Test Commands

### .NET Backend

```bash
# Build the entire solution
dotnet build Moda.slnx

# Build a specific project
dotnet build "Moda.Web/src/Moda.Web.Api/Moda.Web.Api.csproj"

# Run all tests
dotnet test Moda.slnx

# Run tests for a specific project
dotnet test "Moda.Services/Moda.Work/tests/Moda.Work.Application.Tests/Moda.Work.Application.Tests.csproj"

# Run specific test class or method
dotnet test --filter "FullyQualifiedName~ProjectServiceTests"

# Run architecture tests (enforce Clean Architecture rules)
dotnet test Moda.ArchitectureTests/Moda.ArchitectureTests.csproj

# Run the API locally
cd Moda.Web/src/Moda.Web.Api && dotnet run

# Database migrations (from repository root)
dotnet ef migrations add <MigrationName> --project "Moda.Infrastructure/src/Moda.Infrastructure.Migrators.MSSQL" --startup-project "Moda.Web/src/Moda.Web.Api"
dotnet ef database update --project "Moda.Infrastructure/src/Moda.Infrastructure.Migrators.MSSQL" --startup-project "Moda.Web/src/Moda.Web.Api"
```

### Frontend (Next.js)

From the `Moda.Web/src/moda.web.reactclient` directory:

```bash
npm install     # Install dependencies
npm run dev     # Run development server (with Turbopack)
npm run build   # Build for production
npm run lint    # Run linter
npm test        # Run tests
```

### .NET Aspire (Recommended for Local Development)

```bash
cd Moda.AppHost && dotnet run
```

- Aspire Dashboard: <http://localhost:15888>
- Client: <http://localhost:3000>
- API: Dynamic HTTPS port (shown in Aspire dashboard)

### Docker

```bash
docker compose up       # Run entire stack
docker compose down     # Tear down
```

- API: <https://localhost:5001> (Swagger: <https://localhost:5001/swagger>)
- Client: <http://localhost:5002>

## Architecture

### Clean Architecture Layers

```
Domain (innermost, zero dependencies)
  ↑
Application (depends on Domain only)
  ↑
Infrastructure (depends on Application & Domain)
  ↑
Web API (depends on all layers)
```

Architecture tests in `Moda.ArchitectureTests` enforce these dependency rules.

### Where Code Lives

- **Domain logic**: `Moda.Services/{ServiceName}/src/{ServiceName}.Domain/Models/`
- **Commands/Queries**: `Moda.Services/{ServiceName}/src/{ServiceName}.Application/{Feature}/Commands/` and `Queries/`
- **API endpoints**: `Moda.Web/src/Moda.Web.Api/Controllers/{DomainArea}/`
- **Infrastructure**: `Moda.Infrastructure/src/Moda.Infrastructure/{Concern}/`
- **Integrations**: `Moda.Integrations/src/Moda.Integrations.{SystemName}/`
- **Frontend pages**: `Moda.Web/src/moda.web.reactclient/src/app/`
- **Tests**: Mirror the source structure in `tests/` folders

### Key Patterns

- **CQRS with MediatR** — All operations are commands/queries. Controllers are thin.
- **Result Pattern** — Handlers return `Result<T>` from CSharpFunctionalExtensions. No exceptions for business logic.
- **No Repository Pattern** — EF Core DbContext used directly in handlers.
- **Vertical Slices** — Each service: `{ServiceName}.Domain/` + `{ServiceName}.Application/`
- **Feature Folders** — Application layer organized by aggregate root: `{Feature}/Commands/`, `{Feature}/Queries/`, `{Feature}/Dtos/`

## Coding Conventions

### .NET Backend

- **Time handling**: NodaTime (`Instant`, `LocalDate`). Never use `DateTime.UtcNow` — always inject `IDateTimeProvider`.
- **Async naming**: Do NOT use `Async` suffix for new async methods.
- **Validation**: FluentValidation, auto-registered with MediatR pipeline.
- **Mapping**: Mapster for DTOs.
- **Entity configuration**: Fluent API in `IEntityTypeConfiguration<T>` classes. No data annotations.
- **Package management**: Central Package Management via `Directory.Packages.props`.

### Frontend (React/Next.js)

- **API calls**: Always use NSwag-generated typed client (e.g., `getProjectsClient()`). Never use `authenticatedFetch()` directly. Clients in `moda.web.reactclient/src/services/clients.ts`.
- **Theming**: Ant Design theme tokens only — never hardcode colors. Prefer CSS variables (`var(--ant-color-primary)`) in CSS modules over `theme.useToken()` in JS. Only use `theme.useToken()` when values are needed in JS logic.
- **State**: Redux Toolkit + RTK Query for API data. React Context for auth/theme. `useState` for local UI state.
- **PWA**: Installable via Serwist (`@serwist/turbopack`). See [Frontend Development docs](docs/contributing/frontend.mdx#pwa-progressive-web-app) for details.

## Development Notes

### Authentication

Two methods, configured per user via `LoginProvider`:

1. **Microsoft Entra ID** — SSO via Azure AD. Requires `.env` with `AAD_CLIENT_ID`, `AAD_TENANT_ID`, etc.
2. **Moda (Local)** — JWT auth. Requires `SecuritySettings:LocalJwt:Secret` in API config.

Key files: `Moda.Infrastructure/Auth/Local/TokenService.cs`, `moda.web.reactclient/src/components/contexts/auth/auth-context.tsx`

### Feature Flags

Microsoft.FeatureManagement — defined in code, stored in database, managed via Settings UI.

1. Define in `Moda.Common.Domain/FeatureManagement/FeatureFlags.cs`
2. Gate backend: `[FeatureGate(FeatureFlags.Names.MyFlag)]` or `IFeatureManager.IsEnabledAsync()`
3. Gate frontend: `requireFeatureFlag` HOC or `useFeatureFlag` hook
4. Gate menus: pass flag state into menu builder functions
5. Deploy — seeder creates flag as disabled; admin enables via UI

### OpenAPI Client Generation

NSwag generates TypeScript client from API's OpenAPI spec on Debug build. Config in `nswag.json`. Generated client in `moda.web.reactclient/src/services/moda-api.ts`.

### Database

Single shared `ModaDbContext`. Entity configs in `Moda.Infrastructure/Persistence/Configuration/`. Migrations in `Moda.Infrastructure.Migrators.MSSQL`. Auto-applied on startup via `app.Services.InitializeDatabases()`.

### Testing

- Test naming: `{ProjectName}.Tests`
- Domain fakers in `Moda.Tests.Shared`
- Fake DbContext implementations for each application area
- Moq.AutoMock for automatic dependency mocking

## Important Considerations

- **Main branch**: `main` (not master)
- **Docker Compose**: Environment variable changes require full teardown and rebuild (`docker compose down` then `up`)
- **OpenTelemetry**: Configured in `Moda.Infrastructure/src/Moda.Infrastructure/OpenTelemetry/ConfigureServices.cs`. Frontend server-side only via `instrumentation.ts`.
