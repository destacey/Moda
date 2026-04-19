using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wayd.Infrastructure.Migrators.MSSQL.Migrations;

/// <inheritdoc />
public partial class RenameModaLoginProviderToWayd : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Data migration: rename the local-auth provider value on existing user rows
        // from the legacy 'Moda' to 'Wayd' to match the renamed LoginProviders.Wayd constant.
        migrationBuilder.Sql("UPDATE [Identity].[Users] SET [LoginProvider] = 'Wayd' WHERE [LoginProvider] = 'Moda';");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("UPDATE [Identity].[Users] SET [LoginProvider] = 'Moda' WHERE [LoginProvider] = 'Wayd';");
    }
}
