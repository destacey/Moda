# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

Wayd is an intelligent delivery management platform designed to give engineering leaders and teams end-to-end visibility into software delivery. It acts as a unified hub that synchronizes data from multiple business systems and combines it with capabilities those systems lack — connecting the dots so teams can see the full picture in one place. Built with Clean Architecture, Domain-Driven Design, and a modular monolith approach with a shared database.

**For domain context** (entities, relationships, business rules): See [AGENTS.md](AGENTS.md) and [docs/llms-full.txt](docs/llms-full.txt)
**For domain terminology**: See [docs/ai/domain-glossary.mdx](docs/ai/domain-glossary.mdx)
**For user-facing documentation**: See [docs/](docs/) (shared by Docusaurus and Next.js in-app docs)

Documentation site: <https://wayd.dev>

## Build and Test Commands

### .NET Backend

```bash
# Build the entire solution
dotnet build Wayd.slnx

# Build a specific project
dotnet build "Wayd.Web/src/Wayd.Web.Api/Wayd.Web.Api.csproj"

# Run all tests
dotnet test Wayd.slnx

# Run tests for a specific project
dotnet test "Wayd.Services/Wayd.Work/tests/Wayd.Work.Application.Tests/Wayd.Work.Application.Tests.csproj"

# Run specific test class or method
dotnet test --filter "FullyQualifiedName~ProjectServiceTests"

# Run architecture tests (enforce Clean Architecture rules)
dotnet test Wayd.ArchitectureTests/Wayd.ArchitectureTests.csproj

# Run the API locally
cd Wayd.Web/src/Wayd.Web.Api && dotnet run

# Database migrations (from repository root)
dotnet ef migrations add <MigrationName> --project "Wayd.Infrastructure/src/Wayd.Infrastructure.Migrators.MSSQL" --startup-project "Wayd.Web/src/Wayd.Web.Api"
dotnet ef database update --project "Wayd.Infrastructure/src/Wayd.Infrastructure.Migrators.MSSQL" --startup-project "Wayd.Web/src/Wayd.Web.Api"
```

### Frontend (Next.js)

From the `Wayd.Web/src/wayd.web.reactclient` directory:

```bash
npm install     # Install dependencies
npm run dev     # Run development server (with Turbopack)
npm run build   # Build for production
npm run lint    # Run linter
npm test        # Run tests
```

### .NET Aspire (Recommended for Local Development)

```bash
cd Wayd.AppHost && dotnet run
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

Architecture tests in `Wayd.ArchitectureTests` enforce these dependency rules.

### Where Code Lives

- **Domain logic**: `Wayd.Services/{ServiceName}/src/{ServiceName}.Domain/Models/`
- **Commands/Queries**: `Wayd.Services/{ServiceName}/src/{ServiceName}.Application/{Feature}/Commands/` and `Queries/`
- **API endpoints**: `Wayd.Web/src/Wayd.Web.Api/Controllers/{DomainArea}/`
- **Infrastructure**: `Wayd.Infrastructure/src/Wayd.Infrastructure/{Concern}/`
- **Integrations**: `Wayd.Integrations/src/Wayd.Integrations.{SystemName}/`
- **Frontend pages**: `Wayd.Web/src/wayd.web.reactclient/src/app/`
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

- **API calls**: Always use NSwag-generated typed client (e.g., `getProjectsClient()`). Never use `authenticatedFetch()` directly. Clients in `wayd.web.reactclient/src/services/clients.ts`.
- **Theming**: Ant Design theme tokens only — never hardcode colors. Prefer CSS variables (`var(--ant-color-primary)`) in CSS modules over `theme.useToken()` in JS. Only use `theme.useToken()` when values are needed in JS logic.
- **State**: Redux Toolkit + RTK Query for API data. React Context for auth/theme. `useState` for local UI state.
- **PWA**: Installable via Serwist (`@serwist/turbopack`). See [Frontend Development docs](docs/contributing/frontend.mdx#pwa-progressive-web-app) for details.
- **Ant Design reference**: For component APIs, usage examples, and design tokens, see <https://ant.design/llms-full.txt>

## Development Notes

### Authentication

Two methods, configured per user via `LoginProvider`:

1. **Microsoft Entra ID** — SSO via Azure AD. Requires `.env` with `AAD_CLIENT_ID`, `AAD_TENANT_ID`, etc.
2. **Wayd (Local)** — JWT auth. Requires `SecuritySettings:LocalJwt:Secret` in API config.

Key files: `Wayd.Infrastructure/Auth/Local/TokenService.cs`, `wayd.web.reactclient/src/components/contexts/auth/auth-context.tsx`

**Ongoing refactor:** the identity model (how users link to login providers) is being generalized. See [docs/contributing/specs/identity-model-refactor.mdx](docs/contributing/specs/identity-model-refactor.mdx) — multi-PR spec covering a `UserIdentity` table, multi-provider support (Entra + Auth0), tenant-migration history, and a future token-exchange flow.

### Feature Flags

Microsoft.FeatureManagement — defined in code, stored in database, managed via Settings UI.

1. Define in `Wayd.Common.Domain/FeatureManagement/FeatureFlags.cs`
2. Gate backend: `[FeatureGate(FeatureFlags.Names.MyFlag)]` or `IFeatureManager.IsEnabledAsync()`
3. Gate frontend: `requireFeatureFlag` HOC or `useFeatureFlag` hook
4. Gate menus: pass flag state into menu builder functions
5. Deploy — seeder creates flag as disabled; admin enables via UI

### OpenAPI Client Generation

NSwag generates TypeScript client from API's OpenAPI spec on Debug build. Config in `nswag.json`. Generated client in `wayd.web.reactclient/src/services/wayd-api.ts`.

### Database

Single shared `WaydDbContext`. Entity configs in `Wayd.Infrastructure/Persistence/Configuration/`. Migrations in `Wayd.Infrastructure.Migrators.MSSQL`. Auto-applied on startup via `app.Services.InitializeDatabases()`.

### Testing

- Test naming: `{ProjectName}.Tests`
- Domain fakers in `Wayd.Tests.Shared`
- Fake DbContext implementations for each application area
- Moq.AutoMock for automatic dependency mocking

## Important Considerations

- **Main branch**: `main` (not master)
- **Docker Compose**: Environment variable changes require full teardown and rebuild (`docker compose down` then `up`)
- **OpenTelemetry**: Configured in `Wayd.Infrastructure/src/Wayd.Infrastructure/OpenTelemetry/ConfigureServices.cs`. Frontend server-side only via `instrumentation.ts`.
