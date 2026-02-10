# Moda

**An intelligent delivery management platform designed to give engineering leaders and teams end-to-end visibility into software delivery**

When delivery spans multiple teams, projects, and systems, visibility breaks down. Moda brings it all together — tracking work items, aligning teams to planning intervals and products, and surfacing dependencies across the organization.

**Core Philosophy:** Moda acts as a unified hub that synchronizes data from multiple business systems and combines it with capabilities those systems lack — connecting the dots so teams can see the full picture in one place.

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE.md)

## 📚 Documentation

[Full Documentation](https://destacey.github.io/Moda) | [Contributing Guide](#-contributing)

## ✨ Key Features

- Work Item Management - Track tasks, bugs, user stories, and dependencies across multiple systems
- Team Organization - Manage teams, team-of-teams, and organizational structure
- Planning - Planning intervals, sprint planning, and roadmaps
- Dependency Tracking - Visualize and manage cross-team and cross-project dependencies
- Portfolio Management - Project and portfolio tracking with organizational alignment
- External Integrations - Sync teams, work items, iterations, and sprint data from external systems like Azure DevOps on automated cycles
- Observability - Built-in OpenTelemetry support for tracing, metrics, and logging

## 🏗️ Technology Stack

**Backend:**

- .NET 10.0 with Clean Architecture
- ASP.NET Core Web API
- Entity Framework Core + SQL Server
- MediatR (CQRS), FluentValidation
- Hangfire (background jobs)
- OpenTelemetry observability

**Frontend:**

- Next.js 16 (React 19, TypeScript)
- Ant Design UI components
- Redux Toolkit + RTK Query
- Azure AD (MSAL) authentication

## 🚀 Quick Start

### Prerequisites

Before you begin, ensure you have installed:

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js 20+](https://nodejs.org/) and npm
- [SQL Server](https://www.microsoft.com/sql-server) (local or remote)
- [Git](https://git-scm.com/)

### 1. Clone the Repository

```bash
git clone https://github.com/destacey/Moda.git
cd Moda
```

### 2. Configure Database

Create or update `Moda.Web/src/Moda.Web.Api/Configurations/database.json`:

```json
{
  "DatabaseSettings": {
    "DBProvider": "mssql",
    "ConnectionString": "Server=localhost;Database=Moda;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### 3. Configure Authentication

You'll need an Azure AD app registration. Create a `.env` file in the repository root:

```env
AAD_CLIENT_ID='{your AAD client ID}'
AAD_TENANT_ID='{your AAD tenant ID}'
AAD_LOGON_AUTHORITY='https://login.microsoftonline.com/{your AAD tenant ID}'
API_SCOPE='api://{your AAD client ID}/access_as_user'
API_BASE_URL='http://localhost:5000'
```

### 4. Run with .NET Aspire (Recommended)

The easiest way to run Moda locally with full observability:

```bash
cd Moda.AppHost
dotnet run
```

**Access the application:**

- **Aspire Dashboard**: <http://localhost:15888> (telemetry, logs, traces, metrics)
- **Client (Next.js)**: <http://localhost:3000>
- **API**: Check Aspire dashboard for dynamically assigned port
- **API Swagger**: `{API URL}/swagger`

The database will be automatically created and migrations applied on first run.

### Alternative: Run with Docker Compose

```bash
docker compose up
```

**Access points:**

- Client: <http://localhost:5002>
- API: <https://localhost:5001>
- API Swagger: <https://localhost:5001/swagger>
- Seq (logs): <http://localhost:8081>

**Note:** Environment variable changes in Docker require full teardown: `docker compose down && docker compose up`

### Alternative: Run Components Individually

**Terminal 1 - API:**

```bash
cd Moda.Web/src/Moda.Web.Api
dotnet run
```

**Terminal 2 - Client:**

```bash
cd Moda.Web/src/moda.web.reactclient
npm install
npm run dev
```

Access at <http://localhost:3000>

## 🧪 Testing

**Run all tests:**

```bash
dotnet test Moda.slnx
```

**Run architecture tests (enforce Clean Architecture rules):**

```bash
dotnet test Moda.ArchitectureTests/Moda.ArchitectureTests.csproj
```

**Run specific test project:**

```bash
dotnet test "Moda.Services/Moda.Work/tests/Moda.Work.Application.Tests/Moda.Work.Application.Tests.csproj"
```

## 📊 Database Migrations

**Add migration:**

```bash
dotnet ef migrations add <MigrationName> \
  --project "Moda.Infrastructure/src/Moda.Infrastructure.Migrators.MSSQL" \
  --startup-project "Moda.Web/src/Moda.Web.Api"
```

**Apply migrations:**

```bash
dotnet ef database update \
  --project "Moda.Infrastructure/src/Moda.Infrastructure.Migrators.MSSQL" \
  --startup-project "Moda.Web/src/Moda.Web.Api"
```

## 🏛️ Architecture

Moda follows **Clean Architecture** with **Domain-Driven Design** principles:

```text
┌─────────────────────────────────────────┐
│  Presentation (Web API, Next.js Client) │
├─────────────────────────────────────────┤
│  Infrastructure (EF Core, Integrations) │
├─────────────────────────────────────────┤
│  Application (CQRS, Commands, Queries)  │
├─────────────────────────────────────────┤
│  Domain (Entities, Value Objects, Rules)│
└─────────────────────────────────────────┘
```

**Key Patterns:**

- CQRS with MediatR
- Repository-less (direct DbContext usage)
- Vertical slice architecture per domain
- Functional error handling with Result pattern

See [CLAUDE.md](CLAUDE.md) for detailed architecture documentation.

## 🔧 Troubleshooting

### Database Connection Issues

- Verify SQL Server is running
- Check connection string in `database.json`
- Ensure database user has proper permissions

### Authentication Issues

- Verify Azure AD app registration configuration
- Check `.env` file values match your Azure AD setup
- Ensure redirect URIs are configured in Azure AD

### Aspire Dashboard Not Loading

- Check if port 15888 is available
- Verify .NET Aspire workload is installed: `dotnet workload list`
- Install if needed: `dotnet workload install aspire`

### Next.js Build Errors

- Clear Next.js cache: `cd Moda.Web/src/moda.web.reactclient && rm -rf .next`
- Reinstall dependencies: `npm clean-install`

## 🤝 Contributing

Contributions are welcome! Please:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request to the `main` branch

**Before submitting:**

- Run tests: `dotnet test Moda.slnx`
- Run architecture tests: `dotnet test Moda.ArchitectureTests/Moda.ArchitectureTests.csproj`
- Ensure code follows existing patterns (see [CLAUDE.md](CLAUDE.md))

## 📖 Additional Resources

- **Full Documentation**: <https://destacey.github.io/Moda>
- **Architecture Guide**: [CLAUDE.md](CLAUDE.md)
- **API Documentation**: Available at `/swagger` when API is running

## Deployment

If you plan to use the client container, the following environment variables must be set:

```env
NEXT_PUBLIC_AZURE_AD_CLIENT_ID='{your client ID}'
NEXT_PUBLIC_AZURE_AD_TENANT_ID='{your tenant ID}'
NEXT_PUBLIC_MICROSOFT_LOGON_AUTHORITY='{your login authority}'
NEXT_PUBLIC_API_SCOPE='{your API scope}'
NEXT_PUBLIC_API_BASE_URL='{Your API URL}'
```

The API container needs the following:

```env
CorsSettings__WebClient={your client URL}
DatabaseSettings__ConnectionString={connection string to your database}
HangfireSettings__Storage__ConnectionString={connection string to your database}
SecuritySettings__AzureAd__ClientSecret={client secret to your API app reg}
SecuritySettings__AzureAd__ClientId={client ID to your API app reg}
SecuritySettings__AzureAd__Domain={your domain}
SecuritySettings__AzureAd__RootIssuer={your root issuer/sts url for AAD}
SecuritySettings__AzureAd__TenantId={your tenant ID}
```

Additionally, by default Moda logs to the console via Serilog. If you wish to configure any of the other supported sinks (currently Seq, Datadog and Application Insights), provide the appropriate Serilog__Using__x and Serilog__WriteTo__x settings as env vars for your moda-api container. An example with DataDog (taken from some TF for the `azurerm_container_app` resource type):

```terraform
      env {
        name  = "Serilog__Using__1"
        value = "Serilog.Sinks.Datadog.Logs"
      }

      env {
        name  = "Serilog__WriteTo__1__Name"
        value = "DatadogLogs"
      }

      env {
        name  = "Serilog__WriteTo__1__Args__apiKey"
        value = var.dd_api_key
      }

      env {
        name  = "Serilog__WriteTo__1__Args__source"
        value = "csharp"
      }

      env {
        name  = "Serilog__WriteTo__1__Args__host"
        value = "moda-api"
      }

      env {
        name  = "Serilog__WriteTo__1__Args__tags__0"
        value = "product:moda"
      }

      env {
        name  = "Serilog__WriteTo__1__Args__tags__1"
        value = "service:moda-api"
      }
```

> Note, if you use `__0` for the WriteTo and Using values, you will override the default console logging

## License

See [LICENSE](LICENSE.md)
