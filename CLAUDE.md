# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

Moda is a work management system used to plan, manage, and create associations across work items, projects, teams, planning and products. It uses Clean Architecture with Domain-Driven Design principles and a modular monolith approach with a shared database.

Documentation: <https://destacey.github.io/Moda>

## Build and Test Commands

### .NET Backend

**Build the entire solution:**

```bash
dotnet build Moda.slnx
```

**Build a specific project:**

```bash
dotnet build "Moda.Web/src/Moda.Web.Api/Moda.Web.Api.csproj"
```

**Run all tests:**

```bash
dotnet test Moda.slnx
```

**Run tests for a specific project:**

```bash
dotnet test "Moda.Services/Moda.Work/tests/Moda.Work.Application.Tests/Moda.Work.Application.Tests.csproj"
```

**Run tests with filters:**

```bash
# Run specific test class
dotnet test --filter "FullyQualifiedName~ProjectServiceTests"

# Run specific test method
dotnet test --filter "FullyQualifiedName~ProjectServiceTests.ShouldReturnValidResult"

# Run tests with verbosity
dotnet test --verbosity minimal

# Run architecture tests (enforce Clean Architecture rules)
dotnet test Moda.ArchitectureTests/Moda.ArchitectureTests.csproj
```

**Run the API locally:**

```bash
cd Moda.Web/src/Moda.Web.Api
dotnet run
```

**Database migrations (from repository root):**

```bash
# Add new migration
dotnet ef migrations add <MigrationName> --project "Moda.Infrastructure/src/Moda.Infrastructure.Migrators.MSSQL" --startup-project "Moda.Web/src/Moda.Web.Api"

# Apply migrations
dotnet ef database update --project "Moda.Infrastructure/src/Moda.Infrastructure.Migrators.MSSQL" --startup-project "Moda.Web/src/Moda.Web.Api"
```

### Frontend (Next.js)

**From the `Moda.Web/src/moda.web.reactclient` directory:**

```bash
# Install dependencies
npm install

# Run development server (with Turbopack)
npm run dev

# Build for production
npm run build

# Start production server
npm start

# Run linter
npm run lint

# Run tests
npm test
```

### .NET Aspire (Recommended for Local Development)

**Run the entire stack with Aspire:**

```bash
cd Moda.AppHost
dotnet run
```

**Access points when running via Aspire:**

- Aspire Dashboard: <http://localhost:15888> (telemetry, logs, metrics, traces)
- API: Dynamic HTTPS port (shown in Aspire dashboard)
- Client: <http://localhost:3000>

**Aspire Configuration:**

- Located in `Moda.AppHost/AppHost.cs`
- Automatically configures OpenTelemetry for all resources
- Database connection string loaded from `Moda.Web.Api/Configurations/database.json`
- Environment variables auto-injected (OTLP endpoint, service names, etc.)

**Aspire Benefits:**

- Unified dashboard for monitoring all services
- Automatic OpenTelemetry instrumentation
- Simplified local development orchestration
- Real-time telemetry (traces, metrics, logs)

### Docker

**Run entire stack with Docker Compose:**

```bash
docker compose up
```

**Tear down containers:**

```bash
docker compose down
```

**Access points when running via Docker:**

- API: <https://localhost:5001> (Swagger: <https://localhost:5001/swagger>)
- Client: <http://localhost:5002>
- Seq (logs): <http://localhost:8081>

### VSCode Launch Configurations

- **Compound: Launch Moda on Host** - Runs API and client natively without Docker
- **Compound: Launch Moda with Compose** - Runs full stack via Docker Compose (requires `.env` file with Azure AD credentials)

## Architecture

### Clean Architecture + Domain-Driven Design

The codebase follows Clean Architecture with clear dependency rules:

```
Domain (innermost, zero dependencies)
  ↑
Application (depends on Domain only)
  ↑
Infrastructure (depends on Application & Domain)
  ↑
Web API (depends on all layers)
```

**Architecture Tests**: The solution includes automated architecture tests in `Moda.ArchitectureTests` that enforce these dependency rules using NetArchTest.Rules. These tests ensure:

- **Domain Layer** (`Moda.*.Domain`):
  - Cannot depend on Application, Infrastructure, Web, or Integrations
  - Can only depend on `Moda.Common` and `Moda.Common.Domain`

- **Application Layer** (`Moda.*.Application`):
  - Cannot depend on Infrastructure, Web, or Integrations
  - Can depend on its own Domain and cross-cutting domains (e.g., `Organization.Domain`)
  - Can depend on `Moda.Common`, `Moda.Common.Domain`, and `Moda.Common.Application`

- **Common Layers**:
  - `Moda.Common.Domain` cannot depend on `Moda.Common.Application`
  - Common projects cannot depend on Infrastructure

- **Service Isolation**:
  - Application projects should primarily reference their own domain
  - Cross-service domain references must be intentional (e.g., `Organization.Domain` is a known cross-cutting concern)

Run architecture tests: `dotnet test Moda.ArchitectureTests/Moda.ArchitectureTests.csproj`

### Solution Structure

The solution is organized into 5 main areas:

1. **Moda.Common** - Shared libraries and base abstractions

   - `Moda.Common` - Utilities, extensions, helpers
   - `Moda.Common.Domain` - Base entities, value objects, interfaces
   - `Moda.Common.Application` - Base application behaviors, validators, interfaces
   - `Moda.Tests.Shared` - Shared test utilities, fakers, fake DbContext implementations

2. **Moda.Services** - 10 vertical slice domain services:

   - `Moda.Work` - Work item management (tasks, bugs, user stories)
   - `Moda.Organization` - Teams and employees
   - `Moda.Planning` - Iterations and planning intervals
   - `Moda.Goals` - Objectives and key results
   - `Moda.AppIntegration` - Integration configuration and management
   - `Moda.Links` - Cross-entity relationships
   - `Moda.Health` - Health checks
   - `Moda.StrategicManagement` - Strategic planning
   - `Moda.ProjectPortfolioManagement` - Project portfolio management

3. **Moda.Infrastructure** - Cross-cutting concerns

   - Database (ModaDbContext - single shared context for all services)
   - Authentication (Azure AD and local JWT)
   - Identity (ASP.NET Core Identity for user management)
   - Background jobs (Hangfire)
   - Logging (Serilog with multiple sinks)
   - OpenAPI/Swagger configuration

4. **Moda.Integrations** - External system integrations

   - `Moda.Integrations.AzureDevOps` - Bidirectional work item sync with Azure DevOps
   - `Moda.Integrations.MicrosoftGraph` - Employee/user sync from Azure AD

5. **Moda.Web** - Presentation layer

   - `Moda.Web.Api` - ASP.NET Core Web API
   - `moda.web.reactclient` - Next.js/React frontend

### Key Architectural Patterns

**CQRS with MediatR**: All business operations are commands or queries handled via MediatR. Controllers are thin and delegate to handlers.

**No Repository Pattern**: Application handlers use EF Core DbContext directly for data access.

**Result Pattern**: Uses `CSharpFunctionalExtensions` for functional error handling instead of exceptions.

**Vertical Slices**: Each service follows Domain/Application layering independently with consistent folder structure:

```
Moda.Services/
  Moda.{ServiceName}/
    src/
      Moda.{ServiceName}.Domain/        # Entities, value objects, interfaces
      Moda.{ServiceName}.Application/   # Commands, queries, DTOs, validators
    tests/
      Moda.{ServiceName}.Domain.Tests/
      Moda.{ServiceName}.Application.Tests/
```

**Feature Folders**: Application layer organized by aggregate root/feature:

```
Moda.Work.Application/
  WorkItems/
    Commands/
      CreateWorkItemCommand.cs
      CreateWorkItemCommandHandler.cs
      CreateWorkItemCommandValidator.cs
    Queries/
      GetWorkItemQuery.cs
      GetWorkItemQueryHandler.cs
    Dtos/
      WorkItemDto.cs
```

### Technology Stack

**Backend:**

- .NET 10.0
- ASP.NET Core Web API
- Entity Framework Core with SQL Server
- MediatR (CQRS)
- FluentValidation
- Mapster (object mapping)
- NodaTime (date/time)
- Hangfire (background jobs)
- Serilog (structured logging)
- NSwag (OpenAPI/Swagger)
- Azure AD and local JWT authentication
- ASP.NET Core Identity (user management)

**Frontend:**

- Next.js 16 with App Router
- React 19
- TypeScript
- Ant Design (UI components)
- Redux Toolkit (state management)
- Azure MSAL or local JWT authentication
- AG Grid (data tables)

**Testing:**

- xUnit
- FluentAssertions
- Moq + Moq.AutoMock
- Bogus (fake data generation)
- NetArchTest.Rules (architecture tests)
- Jest (frontend)

### Database

**Single Database Approach**: All services share `ModaDbContext`. Entity configurations are organized by domain area in `Moda.Infrastructure/Persistence/Configuration/`.

**Migrations**: Managed in `Moda.Infrastructure.Migrators.MSSQL` project.

**Database Initialization**: Runs on application startup via `await app.Services.InitializeDatabases()` in `Program.cs`.

### Where Code Lives

**Domain logic** (entities, value objects, business rules):

- `Moda.Services/{ServiceName}/src/{ServiceName}.Domain/Models/`

**Use cases** (commands, queries, handlers):

- `Moda.Services/{ServiceName}/src/{ServiceName}.Application/{Feature}/Commands/`
- `Moda.Services/{ServiceName}/src/{ServiceName}.Application/{Feature}/Queries/`

**API endpoints**:

- `Moda.Web/src/Moda.Web.Api/Controllers/{DomainArea}/`

**Cross-cutting infrastructure**:

- `Moda.Infrastructure/src/Moda.Infrastructure/{Concern}/`

**External integrations**:

- `Moda.Integrations/src/Moda.Integrations.{SystemName}/`

**Shared utilities**:

- `Moda.Common/src/Moda.Common/`

**Tests**: Mirror the source structure in corresponding `tests/` folders

## Development Notes

### Authentication

Moda supports two authentication methods, configured per user via `LoginProvider`:

1. **Microsoft Entra ID (Azure AD)** - Directory-based SSO. Requires Azure AD app registration and `.env` configuration (see README).
2. **Moda (Local)** - Self-contained username/password auth with JWT tokens. Requires `SecuritySettings:LocalJwt:Secret` in API configuration.

**Local Auth Configuration** (`Moda.Web/src/Moda.Web.Api/Configurations/security.json` or user secrets):

```json
{
  "SecuritySettings": {
    "LocalJwt": {
      "Secret": "<strong-random-secret-at-least-32-chars>",
      "Issuer": "Moda",
      "Audience": "ModaApi",
      "TokenExpirationInMinutes": 60,
      "RefreshTokenExpirationInDays": 7
    }
  }
}
```

**Azure AD Configuration** - Create a `.env` file in the repository root:

```env
AAD_CLIENT_ID='{your AAD client ID}'
AAD_TENANT_ID='{your AAD tenant ID}'
AAD_LOGON_AUTHORITY='https://login.microsoftonline.com/{your AAD tenant ID}'
API_SCOPE='{scope to attach to API requests; for AAD this is usually api://{client ID}/access_as_user}'
API_BASE_URL='https://localhost:5001'
```

Use User Secrets for API configuration (right-click `Moda.Web.Api` project → Manage User Secrets)

### User Management

User management lives in `Moda.Common.Application/Identity/Users/` (commands/interfaces) and `Moda.Infrastructure/Identity/` (implementation).

**Key concepts:**

- `LoginProvider` - Each user is either `MicrosoftEntraId` or `Moda` (set at creation, immutable)
- `MustChangePassword` - Local users created with this flag; cleared after first password change
- Account lifecycle: activate, deactivate, unlock (locked after failed login attempts)
- Password management: users change their own password; admins can reset passwords (forces change on next login)

**Key files:**

- `Moda.Common.Application/Identity/Users/IUserService.cs` - User management interface
- `Moda.Infrastructure/Identity/UserService.CreateUpdate.cs` - User CRUD and account operations
- `Moda.Infrastructure/Auth/Local/TokenService.cs` - JWT token generation and refresh
- `Moda.Web.Api/Controllers/UserManagement/` - Auth, Profile, and Users API controllers
- `moda.web.reactclient/src/app/settings/user-management/users/` - User management UI
- `moda.web.reactclient/src/app/account/profile/` - User profile and password change
- `moda.web.reactclient/src/components/contexts/auth/auth-context.tsx` - Auth state (supports both MSAL and local JWT)

### Feature Management

Moda uses Microsoft.FeatureManagement to control feature visibility. Feature flags are stored in the database, seeded from code, and managed via the Settings UI. See [docs/feature-management.md](docs/feature-management.md) for full documentation.

**Adding a new feature flag:**

1. Define in `Moda.Common.Domain/FeatureManagement/FeatureFlags.cs` (add both a `FeatureFlagDefinition` field and a `Names` constant)
2. Gate backend: `[FeatureGate(FeatureFlags.Names.MyFlag)]` on controllers/actions, or `IFeatureManager.IsEnabledAsync()` in handlers
3. Gate frontend: `requireFeatureFlag` HOC for pages, `useFeatureFlag` hook for conditional rendering
4. Gate menus: pass flag state into menu builder functions
5. Deploy — seeder creates the flag as disabled; admin enables it from the UI

**Key files:**

- `Moda.Common.Domain/FeatureManagement/FeatureFlags.cs` - Flag definitions and compile-time name constants
- `Moda.Infrastructure/Persistence/Initialization/FeatureFlagSeeder.cs` - Auto-seeds flags from code
- `Moda.Infrastructure/FeatureManagement/DatabaseFeatureDefinitionProvider.cs` - Database-backed provider with caching
- `moda.web.reactclient/src/hooks/use-feature-flag.ts` - Frontend hook for checking flags
- `moda.web.reactclient/src/components/hoc/require-feature-flag.tsx` - Page-level feature gate HOC

### Background Jobs

Hangfire runs recurring jobs for external system synchronization. Dashboard available at `/hangfire` with basic auth.

### Logging

Serilog configured with:

- Console sink (default, always enabled)
- Optional: Seq, DataDog, Application Insights (configured via environment variables)
- Structured logging with enrichers (thread, process, span, environment)

### API Documentation

NSwag generates OpenAPI specification and TypeScript client on build (Debug configuration). Access Swagger UI at `/swagger` when API is running.

### Testing Conventions

**Test naming**: `{ProjectName}.Tests` (e.g., `Moda.Work.Application.Tests`)

**Test organization**: Tests mirror source structure and use feature folders

**Fakers and test data**:

- Domain fakers in `Moda.Tests.Shared`
- Recent improvements added fake DbContext implementations for each application area
- Generic test helpers and extensions available

**Moq.AutoMock**: Used to automatically mock dependencies in tests

### Common Patterns

**Command/Query handlers** return `Result<T>` or `Result` from CSharpFunctionalExtensions

**Validators** use FluentValidation and are automatically registered with MediatR pipeline

**DTOs** use Mapster for mapping from domain entities

**Time handling** uses NodaTime (`Instant`, `LocalDate`) instead of `DateTime`

**Entity configuration** uses fluent API in separate configuration classes implementing `IEntityTypeConfiguration<T>`

**Frontend theming** uses Ant Design theme tokens — never hardcode colors (e.g., `#1677ff`, `#52c41a`). Prefer antd's CSS variables (e.g., `var(--ant-color-primary)`, `var(--ant-color-success)`, `var(--ant-color-error)`, `var(--ant-color-border)`) in CSS modules over `theme.useToken()` in JS. Only fall back to `theme.useToken()` when you need token values in JS logic (e.g., conditional color computation). See `tree-grid.module.css` and `phase-timeline.module.css` for examples.

## Important Considerations

### Package Management

Uses **Central Package Management** via `Directory.Packages.props` - all package versions defined centrally

### Main Branch

The main branch for pull requests is `main` (not master)

### Docker Compose Limitations

When using Docker Compose for local development:

- Environment variable changes require full teardown and rebuild (not just restart)
- This is due to Next.js environment variable handling at container startup
- Use `docker compose down` then `docker compose up` to apply changes

### OpenAPI Client Generation

The TypeScript client for the frontend is auto-generated from the API's OpenAPI spec. Configuration in `nswag.json`.

**IMPORTANT: Always use the NSwag-generated typed client** (e.g., `getProfileClient()`, `getProjectsClient()`) for API calls in RTK Query endpoints. **Do NOT use `authenticatedFetch()` directly** unless there is no generated client method available (e.g., the endpoint doesn't exist in the generated client yet). If the generated client is missing a method for a new endpoint, define the TypeScript type manually and write the `queryFn` using the NSwag client pattern — then ask the developer for help regenerating the client. The generated clients are located in `moda.web.reactclient/src/services/moda-api.ts` and factory functions in `moda.web.reactclient/src/services/clients.ts`.

### OpenTelemetry and Observability

**Backend (.NET API):**

- Configured in `Moda.Infrastructure/src/Moda.Infrastructure/OpenTelemetry/ConfigureServices.cs`
- Automatic instrumentation: ASP.NET Core, HttpClient, SQL Client
- Exports to OTLP endpoint (Aspire dashboard) when `OTEL_EXPORTER_OTLP_ENDPOINT` is set
- Optional Application Insights export via `APPLICATIONINSIGHTS_CONNECTION_STRING`
- Excludes health check endpoints from traces
- Captures SQL statements and parameters in traces

**Frontend (Next.js):**

- Server-side instrumentation only (Node.js runtime)
- Configured in `instrumentation.ts` and `instrumentation.node.ts`
- Uses OpenTelemetry NodeSDK with auto-instrumentations
- Only initializes when `OTEL_EXPORTER_OTLP_ENDPOINT` is present (i.e., when running with Aspire)
- Automatically disabled in production unless explicitly configured
- Client-side browser requests are NOT instrumented (limitation of browser-based telemetry with gRPC)

**Key OpenTelemetry Packages (Next.js):**

- `@opentelemetry/sdk-node` - Core Node.js SDK
- `@opentelemetry/exporter-trace-otlp-grpc` - gRPC exporter for Aspire
- `@opentelemetry/auto-instrumentations-node` - Auto-instrumentation for HTTP, etc.

**Telemetry in Aspire:**

- Traces, metrics, and logs visible in Aspire dashboard
- API traces include SQL queries with full details
- Next.js server-side operations are traced (SSR, API routes, middleware)
- Browser-to-API calls appear as separate traces (not connected due to browser limitations)
