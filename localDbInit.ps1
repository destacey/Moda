dotnet ef database update `
    --project .\Moda.Infrastructure\src\Moda.Infrastructure.Migrators.MSSQL\Moda.Infrastructure.Migrators.MSSQL.csproj `
    --startup-project .\Moda.Web\src\Moda.Web.Api\Moda.Web.Api.csproj `
    --connection "Server=localhost,1433;Database=moda;User Id=sa;Password=ffadisodija1345@#$;TrustServerCertificate=true"